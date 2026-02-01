using System.Windows;

namespace WpfGameGuide.Chapters.Chapter2_PagesAndWindows.Demo
{
    public partial class DemoWindow : Window
    {
        public DemoWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
