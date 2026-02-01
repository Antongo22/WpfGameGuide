using System.Windows;

namespace WpfGameGuide.Chapters.Chapter2_PagesAndWindows.Demo
{
    public partial class DemoDialogWindow : Window
    {
        public DemoDialogWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
