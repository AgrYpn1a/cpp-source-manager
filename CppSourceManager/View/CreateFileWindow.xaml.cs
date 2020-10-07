using System.Windows;
using CppSourceManager.MVVM.CreateFile;

namespace CppSourceManager.View
{
    /// <summary>
    /// Interaction logic for CreateFileWindow.xaml
    /// </summary>
    public partial class CreateFileWindow : Window
    {
        private CreateFileViewModel m_ViewModel;
        internal CreateFileModel Model { get; private set; }

        public CreateFileWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;

            m_ViewModel = new CreateFileViewModel(this);
            Model = m_ViewModel.Model;
            DataContext = Model;
        }
    }
}
