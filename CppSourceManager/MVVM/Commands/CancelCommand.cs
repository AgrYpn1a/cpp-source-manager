using System;
using System.Windows.Input;
using CppSourceManager.MVVM.CreateFile;

namespace CppSourceManager.MVVM.Commands
{
    class CancelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private CreateFileModel m_CreateFileModel;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            m_CreateFileModel.IsCancelled = true;
            m_CreateFileModel.m_ViewModel.m_Window.Close();
        }

        public CancelCommand(CreateFileModel createFileModel)
        {
            m_CreateFileModel = createFileModel;
        }
    }
}
