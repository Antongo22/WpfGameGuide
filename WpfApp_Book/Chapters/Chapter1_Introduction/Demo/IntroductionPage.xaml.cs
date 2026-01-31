using System.Windows;
using System.Windows.Controls;

namespace WpfApp_Book.Chapters.Chapter1_Introduction.Demo
{
    public partial class IntroductionPage : Page
    {
        private int clickCount = 0;

        public IntroductionPage()
        {
            InitializeComponent();
        }

        private void ClickerButton_Click(object sender, RoutedEventArgs e)
        {
            clickCount++;
            ClickCountText.Text = $"Кликов: {clickCount}";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            clickCount = 0;
            ClickCountText.Text = "Кликов: 0";
        }

        private void DemoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxPreview != null)
            {
                TextBoxPreview.Text = $"Вы ввели: {DemoTextBox.Text}";
            }
        }
    }
}
