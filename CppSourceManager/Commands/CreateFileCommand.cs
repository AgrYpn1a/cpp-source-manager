using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CppSourceManager.Utils;
using CppSourceManager.View;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CppSourceManager.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateFileCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f4d09845-4149-48f3-aad8-a0c88fd54007");

        private string m_CppFilePath = "";
        private string m_HppFilePath = "";

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateFileCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CreateFileCommand Instance
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
            // Switch to the main thread - the call to AddCommand in CreateFileCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CreateFileCommand(package, commandService);
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
            object item = ProjectUtils.GetSelectedItem();
            string folder = ProjectUtils.FindFolder(item);

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            ProjectItem selectedItem = item as ProjectItem;
            Project selectedProject = item as Project;
            Project project = selectedItem?.ContainingProject ?? selectedProject ?? ProjectUtils.GetActiveProject();

            if (project == null)
            {
                throw new Exception("Could not find project!"); // TODO remove exception after testing
                return;
            }

            // Get files
            if (PromptForFileName(folder))
            {
                var cppInfo = new FileInfo(m_CppFilePath);
                var hppInfo = new FileInfo(m_HppFilePath);

                ProjectItem projectItemCpp = project.AddFileToProject(cppInfo);
                ProjectItem projectItemHpp = project.AddFileToProject(hppInfo);

                if (projectItemCpp == null || projectItemHpp == null)
                {
                    // We have a problem here!
                    MessageBox.Show("Could not create project files!");
                    return;
                }

                project.Save();

                VsShellUtilities.OpenDocument(this.package, cppInfo.FullName);
                VsShellUtilities.OpenDocument(this.package, hppInfo.FullName);

                CppSourceManagerPackage.ms_DTE.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
                CppSourceManagerPackage.ms_DTE.ActiveDocument.Activate();
            }
        }

        private bool PromptForFileName(string rootFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(rootFolder);
            FileDialog dialog = new OpenFileDialog();

            CreateFileWindow createFileWin = new CreateFileWindow();
            createFileWin.Model.ProjectRootDirPath = rootFolder;

            createFileWin.ShowDialog();

            m_CppFilePath = createFileWin.Model.CppSourcePath;
            m_HppFilePath = createFileWin.Model.HppSourcePath;

            return !createFileWin.Model.IsCancelled;
        }
    }
}
