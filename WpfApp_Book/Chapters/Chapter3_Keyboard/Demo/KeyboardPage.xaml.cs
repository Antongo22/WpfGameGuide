using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfGameGuide.Chapters.Chapter3_Keyboard.Demo
{
    public partial class KeyboardPage : Page
    {
        private DispatcherTimer? timer;
        private int secondsElapsed = 0;

        public KeyboardPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// При загрузке страницы получаем фокус
        /// Без этого события клавиатуры не будут приходить!
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        /// <summary>
        /// Обработчик нажатия клавиш
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            // e.Key может быть Key.System для Alt-комбинаций
            // В этом случае реальная клавиша в e.SystemKey
            Key pressedKey = e.Key == Key.System ? e.SystemKey : e.Key;
            
            // Отображаем название клавиши
            KeyPressedText.Text = $"Нажатая клавиша: {pressedKey}";
            
            // Отображаем код клавиши
            KeyCodeText.Text = $"Код: Key.{pressedKey} ({(int)pressedKey})";
            
            // Проверяем модификаторы
            ModifierKeys mods = Keyboard.Modifiers;
            if (mods == ModifierKeys.None)
            {
                ModifiersText.Text = "Модификаторы: нет";
            }
            else
            {
                var list = new System.Collections.Generic.List<string>();
                if (mods.HasFlag(ModifierKeys.Control)) list.Add("Ctrl");
                if (mods.HasFlag(ModifierKeys.Shift)) list.Add("Shift");
                if (mods.HasFlag(ModifierKeys.Alt)) list.Add("Alt");
                if (mods.HasFlag(ModifierKeys.Windows)) list.Add("Win");
                ModifiersText.Text = $"Модификаторы: {string.Join(" + ", list)}";
            }
            
            // Предотвращаем дальнейшую обработку
            e.Handled = true;
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer == null || !timer.IsEnabled)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1); // Каждую секунду
                timer.Tick += Timer_Tick;
                timer.Start();
                TimerStatus.Text = "Таймер запущен";
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
                TimerStatus.Text = "Таймер остановлен";
            }
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
            }
            secondsElapsed = 0;
            UpdateTimerDisplay();
            TimerStatus.Text = "Таймер сброшен";
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            secondsElapsed++;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            int minutes = secondsElapsed / 60;
            int seconds = secondsElapsed % 60;
            TimerDisplay.Text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
