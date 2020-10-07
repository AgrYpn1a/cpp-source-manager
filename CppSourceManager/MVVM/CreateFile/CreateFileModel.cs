using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CppSourceManager.MVVM.Commands;

namespace CppSourceManager.MVVM.CreateFile
{
    internal sealed class CreateFileModel : INotifyPropertyChanged
    {
        #region Options Properties
        private bool m_OptionsHppChecked;
        public bool OptionsHppChecked
        {
            get => m_OptionsHppChecked;
            set
            {
                m_OptionsHppChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OptionsHppChecked)));

                // Trigger change
                FileName = FileName;
            }
        }

        private bool m_OptionsLowercaseChecked;
        public bool OptionsLowercaseChecked
        {
            get => m_OptionsLowercaseChecked;
            set
            {
                m_OptionsLowercaseChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OptionsLowercaseChecked)));

                // Trigger change
                FileName = FileName;
            }
        }
        #endregion

        private string m_ProjectRootDirPath = @"C:\";
        public string ProjectRootDirPath
        {
            get => m_ProjectRootDirPath;
            set
            {
                m_ProjectRootDirPath = value;

                string dir = value.Split('\\').Last().ToString();
                FilePath = dir;

                FileName = FileName; // Trigger update
            }
        }

        private string m_FilePath = @"C:\";
        public string FilePath
        {
            get => m_FilePath;
            set
            {
                m_FilePath = value;
                FileName = FileName; // Trigger update
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
            }
        }

        private string m_CppSourcePath;
        public string CppSourcePath
        {
            get => m_CppSourcePath;
            set
            {
                m_CppSourcePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CppSourcePath)));
            }
        }

        private string m_HSourcePath;
        public string HppSourcePath
        {
            get => m_HSourcePath;
            set
            {
                m_HSourcePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HppSourcePath)));
            }
        }

        private string m_FileName = "ExampleClass";
        public string FileName
        {
            get => m_FileName;
            set
            {
                m_FileName = value;

                // TODO make this more optimal, maybe calculate only once when RootDir is initialized
                string[] dirs = ProjectRootDirPath.Split('\\');
                dirs = dirs.Take(dirs.Length - 1).ToArray();
                dirs[0] = dirs[0] + "\\";
                string parentDir = Path.Combine(dirs);

                CppSourcePath = m_ViewModel.GetTransformedFileName(Path.Combine(parentDir, FilePath), m_FileName, FileType.CPP);
                HppSourcePath = m_ViewModel.GetTransformedFileName(Path.Combine(parentDir, FilePath), m_FileName, FileType.H);

                if (OptionsHppChecked)
                {
                    HppSourcePath = m_ViewModel.GetTransformedFileName(Path.Combine(parentDir, FilePath), m_FileName, FileType.HPP);
                }

                //CppSourcePath = $@"{ProjectRootDirPath}\{m_FileName}.cpp";
                //HppSourcePath = $@"{ProjectRootDirPath}\{m_FileName}.h";

                // TODO not working correctly
                CanCreateFile = !string.IsNullOrEmpty(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }

        // TODO this is not updating button state properly
        private bool m_CanCreateFile = true;
        public bool CanCreateFile
        {
            get => m_CanCreateFile;
            set
            {
                m_CanCreateFile = value;
                ((CreateCommand)CreateFileCmd).RasieCanExecuteChanged();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreateFile)));
            }
        }

        public string ClassName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand CreateFileCmd { get; private set; }

        public readonly CreateFileViewModel m_ViewModel;

        public CreateFileModel(CreateFileViewModel viewModel)
        {
            m_ViewModel = viewModel;
            CreateFileCmd = new CreateCommand(this);
        }

    }
}
