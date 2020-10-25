using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CppSourceManager.Utils;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using Task = System.Threading.Tasks.Task;

namespace CppSourceManager.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RemoveCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f4d09845-4149-48f3-aad8-a0c88fd54007");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RemoveCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RemoveCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in RemoveCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new RemoveCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            object item = ProjectUtils.GetSelectedItem();
            ProjectItem selectedItem = item as ProjectItem;
            VCFilter selectedFilter = item as VCFilter;

            try
            {
                // It's a source file
                if (selectedItem.ProjectItems.Item(1) == null)
                {
                    // Just remove the source file
                    string targetPath = selectedItem.Properties.Item("FullPath").Value as string;
                    string dir = Path.GetDirectoryName(targetPath);

                    if (Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length <= 1)
                    {
                        // If this is the last source file in the dir, remove everything
                        Directory.Delete(dir, true);

                        ProjectItem parent = selectedItem.Properties.Parent as ProjectItem;
                        parent = parent.Properties.Parent as ProjectItem;

                        if (parent == null)
                        {
                            parent = selectedItem.Properties.Parent as ProjectItem;
                        }

                        //selectedItem.Remove();
                        string parentName = parent.Name;
                        string parParentName = (parent.Properties.Parent as ProjectItem).Name;
                        //string collName = selectedItem.Collection.;

                        parent.Remove();
                        parent.ContainingProject.Save();
                    }
                    else
                    {
                        // Remove physical file from the disk
                        File.Delete(targetPath);

                        // Remove file from the project
                        selectedItem.Remove();
                        selectedItem.ContainingProject.Save();
                    }
                }
                else
                {
                    RemoveDir(selectedItem);
                }
            }
            catch (Exception exc)
            {
                // It's a filter (folder)
            }
        }

        private void RemoveDir(ProjectItem item)
        {
            string childItem = item.ProjectItems.Item(1).FileNames[1];
            string dir = Path.GetDirectoryName(childItem);

            Directory.Delete(dir, true);

            item.Remove();
            item.ContainingProject.Save();
        }
    }
}
