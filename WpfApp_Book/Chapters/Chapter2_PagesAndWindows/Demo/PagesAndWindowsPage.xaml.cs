using System.Windows;
using System.Windows.Controls;

namespace WpfGameGuide.Chapters.Chapter2_PagesAndWindows.Demo
{
    public partial class PagesAndWindowsPage : Page
    {
        public PagesAndWindowsPage()
        {
            InitializeComponent();
        }

        private void OpenWindow_Click(object sender, RoutedEventArgs e)
        {
            var demoWindow = new DemoWindow();
            demoWindow.Show();
        }

        private void OpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialogWindow = new DemoDialogWindow();
            bool? result = dialogWindow.ShowDialog();
            MessageBox.Show(result == true ? "OK нажат" : "Отмена/закрыто");
        }

        private void ShowMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Это сообщение!");
            MessageBoxResult.Text = "Сообщение показано";
        }

        private void ShowYesNo_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            MessageBoxResult.Text = result == System.Windows.MessageBoxResult.Yes ? "Да" : "Нет";
        }

        private void AddToListBox_Click(object sender, RoutedEventArgs e)
        {
            string text = ListBoxInput.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                DemoListBox.Items.Add(text);
                ListBoxInput.Text = "";
                ListBoxStatus.Text = $"Добавлено: '{text}'";
            }
            else
            {
                ListBoxStatus.Text = "Введите текст для добавления";
            }
        }

        private void RemoveFromListBox_Click(object sender, RoutedEventArgs e)
        {
            if (DemoListBox.SelectedItem != null)
            {
                var selected = DemoListBox.SelectedItem;
                DemoListBox.Items.Remove(selected);
                ListBoxStatus.Text = "Элемент удалён";
            }
            else
            {
                ListBoxStatus.Text = "Выберите элемент для удаления";
            }
        }

        private void DemoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoListBox.SelectedItem != null)
            {
                ListBoxStatus.Text = $"Выбран: '{DemoListBox.SelectedItem}' (индекс: {DemoListBox.SelectedIndex})";
            }
            else
            {
                ListBoxStatus.Text = "Ничего не выбрано";
            }
        }
    }
}
