using CppSourceManager.Attributes;
using CppSourceManager.View;

namespace CppSourceManager.MVVM.CreateFile
{
    public enum FileType
    {
        [ExtensionName(".cpp")]
        CPP,
        [ExtensionName(".hpp")]
        HPP,
        [ExtensionName(".h")]
        H
    }

    sealed class CreateFileViewModel
    {
        public CreateFileModel Model;
        public CreateFileWindow m_Window { get; private set; }

        public CreateFileViewModel(CreateFileWindow window)
        {
            m_Window = window;
            Model = new CreateFileModel(this);
        }

        public string GetTransformedFileName(string folder, string fileName, FileType type)
        {
            string transformedName = string.Empty;

            // Save class name
            Model.ClassName = fileName;

            if (Model.OptionsLowercaseChecked)
            {
                transformedName = $@"{folder}\{fileName.ToLower()}{GetExtension(type)}";
            }
            else
            {
                transformedName = $@"{folder}\{fileName}{GetExtension(type)}";
            }

            return transformedName;
        }

        public bool OptionUseHppInstead()
        {
            return m_Window.OptionHpp.IsChecked.Value;
        }

        private string GetExtension(FileType type)
        {
            return EnumAttributesUtil.Get<ExtensionNameAttribute>(type).StringifedValue;
        }
    }
}
