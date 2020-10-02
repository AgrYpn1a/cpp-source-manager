using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
            filters = filters.Skip(1).ToArray();

            foreach (var filter in filters)
            {
                item = item.ProjectItems.Item(filter);
            }

            item.ProjectItems.AddFromFile(file.FullName);
            item.SetItemType(itemType);

            return item;
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
    }
}
