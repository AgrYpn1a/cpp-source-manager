using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CppSourceManager.Commands;
using CppSourceManager.MVVM.CreateFile;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;

namespace CppSourceManager.MVVM.Commands
{

    static class Templates
    {
        public const string CPP_SOURCE_TEMPLATE =
@"#include ""$ClassName.h""\n
$ClassName::$ClassName()
{\n
}\n";

        public const string HPP_SOURCE_TEMPLATE =
@"#pragma once\n
class $ClassName
{
public:
    $ClassName();\n
};\n";
    }

    class CreateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private CreateFileModel m_CreateFileModel;

        public CreateCommand(CreateFileModel createFileModel)
        {
            m_CreateFileModel = createFileModel;
        }


        public bool CanExecute(object parameter)
        {
            return m_CreateFileModel.CanCreateFile;
        }

        public void Execute(object parameter)
        {
            string hppTemplate = Templates.HPP_SOURCE_TEMPLATE;
            hppTemplate = hppTemplate.Replace("\\n", Environment.NewLine);
            hppTemplate = hppTemplate.Replace("$ClassName", m_CreateFileModel.ClassName);

            string cppTemplate = Templates.CPP_SOURCE_TEMPLATE;
            cppTemplate = cppTemplate.Replace("\\n", Environment.NewLine);
            cppTemplate = cppTemplate.Replace("$ClassName", m_CreateFileModel.ClassName);

            if(File.Exists(m_CreateFileModel.CppSourcePath) || File.Exists(m_CreateFileModel.HppSourcePath))
            {
                MessageBox.Show("Files with the same name already exist!");
                return;
            }

            // Make sure directories exist
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(m_CreateFileModel.CppSourcePath));
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(m_CreateFileModel.HppSourcePath));

            File.WriteAllText(m_CreateFileModel.CppSourcePath, cppTemplate);
            File.WriteAllText(m_CreateFileModel.HppSourcePath, hppTemplate);

            m_CreateFileModel.m_ViewModel.m_Window.Close();
        }

        public void RasieCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
