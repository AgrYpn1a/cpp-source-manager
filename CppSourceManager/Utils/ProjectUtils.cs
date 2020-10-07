using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;

namespace CppSourceManager.Utils
{
    static class ProjectUtils
    {
        public static object GetSelectedItem()
        {
            object selectedObject = null;
            IVsMonitorSelection monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));

            try
            {
                monitorSelection.GetCurrentSelection(out IntPtr hierarchyPointer,
                                                 out uint itemId,
                                                 out IVsMultiItemSelect multiItemSelect,
                                                 out IntPtr selectionContainerPointer);

                if (Marshal.GetTypedObjectForIUnknown(
                                                     hierarchyPointer,
                                                     typeof(IVsHierarchy)) is IVsHierarchy selectedHierarchy)
                {
                    ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject));
                }

                Marshal.Release(hierarchyPointer);
                Marshal.Release(selectionContainerPointer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }

            return selectedObject;
        }

        public static Project GetActiveProject()
        {
            try
            {
                if (CppSourceManagerPackage.ms_DTE.ActiveSolutionProjects is Array activeSolutionProjects && activeSolutionProjects.Length > 0)
                {
                    return activeSolutionProjects.GetValue(0) as Project;
                }

                Document doc = CppSourceManagerPackage.ms_DTE.ActiveDocument;

                if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                {

                    ProjectItem item = doc.ProjectItem.ContainingProject.Object as ProjectItem;

                    if (item != null)
                    {
                        return item.ContainingProject;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting the active project" + ex);
            }

            return null;
        }

        public static ProjectItem AddFileToProject(this Project project, FileInfo file, string itemType = null)
        {
            string root = project.FullName;

            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            // Find folder
            string filePath = file.DirectoryName;
            int indexOfProjectNameSubstr = filePath.IndexOf(project.Name.ToLower());
            string filterString = filePath.Substring(indexOfProjectNameSubstr + project.Name.Length + 1); // +1 for \\

            var filters = filterString.Split('\\');

            // Add multilevel filters
            ProjectItem item = project.ProjectItems.Item(filters[0]);
            ProjectItem lastItem = item;

            EnvDTE.DTE dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE));
            VCProject prj = (VCProject)dte.Solution.Projects.Item(1).Object;


            if (item == null)
            {
                // Top level dir
                prj.AddFilter(filters[0]);
                item = project.ProjectItems.Item(filters[0]);
            }

            for (int i = 1; i < filters.Length; i++)
            {
                item = item.ProjectItems.Item(filters[i]);

                if (item == null)
                {
                    //string canonicalFilterName = Path.Combine(filters.Take(i + 1).ToArray());
                    string canonicalFilterName = string.Join(Path.DirectorySeparatorChar.ToString(), filters.Take(i).ToArray());
                    //prj.AddFilter(canonicalFilterName);

                    foreach (VCFilter fltr in prj.Filters)
                    {
                        if (fltr.CanonicalName == canonicalFilterName)
                        {
                            // to i + 1 to add the next filter
                            for (int j = i; j < filters.Length; j++)
                            {
                                if (fltr.CanAddFilter(filters[i]))
                                {
                                    fltr.AddFilter(filters[j]);
                                }
                            }
                        }
                    }

                    prj.Save();

                    item = lastItem.ProjectItems.Item(filters[i]);
                }

                lastItem = item;
            }

            item.ProjectItems.AddFromFile(file.FullName);
            item.SetItemType(itemType);

            return item;
        }

        public static void AddFolderToProject(this Project project, DirectoryInfo dirInfo, string itemType = null)
        {
            string root = project.FullName;

            if (string.IsNullOrEmpty(root))
            {
                return;
            }

            EnvDTE.DTE dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE));
            VCProject prj = (VCProject)dte.Solution.Projects.Item(1).Object;

            foreach (var filter in prj.Filters)
            {
                var vcFilter = (VCFilter)filter;
                if (vcFilter.Name == "core")
                {
                    vcFilter.AddFilter("newdir");
                }
            }

            //VCFilter[] filters = prj.Filters;
            //var filters = (VCFilter[])prj.Filters;
            //((VCFilter)filters[0]).AddFilter(dirInfo.Name);

            //filter.AddFile("D:\\path\\File.h");
            prj.Save();

            return;

            //string dirPath = dirInfo.FullName;
            //int indexOfProjectNameSubstr = dirPath.IndexOf(project.Name.ToLower());
            //string filterString = dirPath.Substring(indexOfProjectNameSubstr + project.Name.Length + 1); // +1 for \\

            //var filters = filterString.Split('\\');
            //filters = filters.Take(filters.Length - 1).ToArray(); // last one is the dir we're creating

            //ProjectItem item = project.ProjectItems.Item(filters[0]);
            //filters = filters.Skip(1).ToArray();

            //foreach (var filter in filters)
            //{
            //    item = item.ProjectItems.Item(filter);
            //}

            //item.ProjectItems.AddFolder(dirInfo.FullName);
            //item.SetItemType("directory");

            //var prjItem = project.ProjectItems.AddFolder(dirInfo.FullName);
            //var prjItem = project.ProjectItems.AddFromDirectory(dirInfo.FullName);
        }

        public static bool IsKind(this Project project, params string[] kindGuids)
        {
            foreach (string guid in kindGuids)
            {
                if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetItemType(this ProjectItem item, string itemType)
        {
            try
            {
                if (item == null || item.ContainingProject == null)
                {
                    return;
                }

                //if (string.IsNullOrEmpty(itemType)
                //    || item.ContainingProject.IsKind(ProjectTypes.WEBSITE_PROJECT)
                //    || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                //{
                //    return;
                //}

                item.Properties.Item("ItemType").Value = itemType;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static string FindRootFolder(object item)
        {
            if (item == null)
            {
                return null;
            }

            ProjectItem projectItem = item as ProjectItem;
            string[] projectPath = projectItem.ContainingProject.FullName.Split('\\');
            return string.Join("\\", projectPath.Take(projectPath.Length - 1));
        }

        public static string FindFolder(object item)
        {
            if (item == null)
            {
                return null;
            }

            if (CppSourceManagerPackage.ms_DTE.ActiveWindow is Window2 window && window.Type == vsWindowType.vsWindowTypeDocument)
            {
                // if a document is active, use the document's containing directory
                Document doc = CppSourceManagerPackage.ms_DTE.ActiveDocument;
                if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                {
                    ProjectItem docItem = CppSourceManagerPackage.ms_DTE.Solution.FindProjectItem(doc.FullName);

                    if (docItem != null && docItem.Properties != null)
                    {
                        string fileName = docItem.Properties.Item("FullPath").Value.ToString();
                        if (File.Exists(fileName))
                        {
                            return Path.GetDirectoryName(fileName);
                        }
                    }
                }
            }

            string folder = null;

            ProjectItem projectItem = item as ProjectItem;
            if (projectItem != null && "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}" == projectItem.Kind) //Constants.vsProjectItemKindVirtualFolder
            {
                ProjectItems items = projectItem.ProjectItems;
                foreach (ProjectItem it in items)
                {
                    if (File.Exists(it.FileNames[1]))
                    {
                        folder = Path.GetDirectoryName(it.FileNames[1]);
                        break;
                    }
                }
            }
            else
            {
                Project project = item as Project;
                if (projectItem != null)
                {
                    string fileName = projectItem.FileNames[1];

                    if (File.Exists(fileName))
                    {
                        folder = Path.GetDirectoryName(fileName);
                    }
                    else
                    {
                        folder = fileName;
                    }


                }
                else if (project != null)
                {
                    folder = project.FullName;
                }
            }

            // At this point, if folder is still null, we're targeting an empty filter
            // so we need to calculate folder path in a different way
            if (folder == null)
            {
                string projName = projectItem.ContainingProject.FullName;
                string prjItem = projectItem.Name;
                ProjectItem parent = projectItem.Properties.Parent as ProjectItem;
                string prjItemFullName = projectItem.DTE.FullName;
                string prjItemParent = parent.Name;
                string prnt = ((ProjectItem)projectItem.ProjectItems.Parent).Name;
            }

            string projectRoot = null;
            foreach (ProjectItem projItem in projectItem.ContainingProject.ProjectItems)
            {
                if (File.Exists(projItem.FileNames[1]))
                {
                    projectRoot = Path.GetDirectoryName(projItem.FileNames[1]);
                }
            }

            return folder;
        }

    }
}
