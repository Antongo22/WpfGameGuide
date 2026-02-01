using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WpfGameGuide.Chapters.Chapter2_PagesAndWindows.Answers
{
    /// <summary>
    /// Приложение для заметок - решение задания Главы 2
    /// Демонстрирует: Window, Page, MessageBox, ShowDialog, модальные окна
    /// </summary>
    public partial class NotesAnswer : Page
    {
        // ObservableCollection автоматически уведомляет UI об изменениях
        private ObservableCollection<string> notes = new ObservableCollection<string>();

        public NotesAnswer()
        {
            InitializeComponent();
            
            // Привязываем коллекцию к ListBox
            NotesListBox.ItemsSource = notes;
            
            // Добавляем примеры
            notes.Add("Первая заметка - пример");
            notes.Add("Не забыть купить молоко");
            UpdateCount();
        }

        /// <summary>
        /// Добавление новой заметки
        /// </summary>
        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            string text = NewNoteTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(text))
            {
                // MessageBox.Show - простой диалог с сообщением
                MessageBox.Show("Введите текст заметки!", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            notes.Add(text);
            NewNoteTextBox.Clear();
            UpdateCount();
        }

        /// <summary>
        /// Удаление заметки с подтверждением
        /// </summary>
        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите заметку для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // КЛЮЧЕВОЙ МОМЕНТ: Диалог подтверждения с YesNo
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить эту заметку?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                notes.Remove(NotesListBox.SelectedItem.ToString()!);
                SelectedNoteTextBox.Clear();
                UpdateCount();
            }
        }

        /// <summary>
        /// Редактирование заметки через модальное окно
        /// </summary>
        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите заметку для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentText = notes[NotesListBox.SelectedIndex];
            
            // КЛЮЧЕВОЙ МОМЕНТ: Создание окна программно
            var editWindow = new Window
            {
                Title = "Редактирование заметки",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = System.Windows.Media.Brushes.White
            };

            var stack = new StackPanel { Margin = new Thickness(20) };
            var textBox = new TextBox 
            { 
                Text = currentText, 
                Height = 80, 
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };
            var saveBtn = new Button 
            { 
                Content = "Сохранить", 
                Margin = new Thickness(0, 10, 0, 0),
                Height = 30
            };
            
            saveBtn.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    notes[NotesListBox.SelectedIndex] = textBox.Text.Trim();
                    SelectedNoteTextBox.Text = textBox.Text.Trim();
                }
                editWindow.Close();
            };

            stack.Children.Add(new TextBlock { Text = "Текст заметки:", Margin = new Thickness(0, 0, 0, 5) });
            stack.Children.Add(textBox);
            stack.Children.Add(saveBtn);
            editWindow.Content = stack;
            
            // ShowDialog() - МОДАЛЬНОЕ окно (код ждет закрытия)
            editWindow.ShowDialog();
        }

        /// <summary>
        /// Очистка всех заметок
        /// </summary>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (notes.Count == 0)
            {
                MessageBox.Show("Список уже пуст!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Удалить все {notes.Count} заметок? Это действие нельзя отменить!",
                "Подтверждение очистки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                notes.Clear();
                SelectedNoteTextBox.Clear();
                UpdateCount();
            }
        }

        private void NotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotesListBox.SelectedItem != null)
            {
                SelectedNoteTextBox.Text = NotesListBox.SelectedItem.ToString();
            }
        }

        private void UpdateCount()
        {
            CountText.Text = $"Заметок: {notes.Count}";
        }
    }
}

/*
================================================================================
                           КАК ЭТО РАБОТАЕТ
================================================================================

КЛЮЧЕВЫЕ КОНЦЕПЦИИ ГЛАВЫ 2:
---------------------------

1. OBSERVABLECOLLECTION<T>
   - Автоматически уведомляет UI об изменениях
   - При Add(), Remove(), Clear() - ListBox обновится сам
   - Обычный List<T> НЕ обновляет UI автоматически

2. MESSAGEBOX - ПРОСТЫЕ ДИАЛОГИ
   Типы кнопок (MessageBoxButton):
   - OK, OKCancel, YesNo, YesNoCancel
   
   Иконки (MessageBoxImage):
   - Information, Warning, Error, Question
   
   Результат (MessageBoxResult):
   - OK, Cancel, Yes, No, None

3. SHOW() vs SHOWDIALOG()
   
   Show() - НЕМОДАЛЬНОЕ окно:
   - Код продолжает выполняться сразу
   - Можно переключаться между окнами
   
   ShowDialog() - МОДАЛЬНОЕ окно:
   - Код ЖДЕТ пока окно не закроется
   - Нельзя переключиться на родительское окно

4. DIALOGRESULT
   В модальном окне:
   - this.DialogResult = true;  // Закрыть с "ОК"
   - this.DialogResult = false; // Закрыть с "Отмена"

================================================================================
*/
