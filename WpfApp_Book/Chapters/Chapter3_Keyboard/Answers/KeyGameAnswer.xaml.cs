using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter3_Keyboard.Answers
{
    /// <summary>
    /// Игра "Нажми клавишу" - решение задания Главы 3
    /// Демонстрирует: KeyDown, Key enum, DispatcherTimer, Focus()
    /// </summary>
    public partial class KeyGameAnswer : Page
    {
        private Random random = new Random();
        private DispatcherTimer gameTimer;      // Таймер игры (каждую секунду)
        private DispatcherTimer feedbackTimer;  // Таймер для скрытия feedback
        
        private char currentLetter = '?';  // Текущая буква для нажатия
        private int score = 0;
        private int highScore = 0;
        private int timeLeft = 30;
        private bool isPlaying = false;

        public KeyGameAnswer()
        {
            InitializeComponent();
            
            // Таймер игры - каждую секунду уменьшает время
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            gameTimer.Tick += GameTimer_Tick;
            
            // Таймер для скрытия feedback через 500мс
            feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            feedbackTimer.Tick += (s, e) =>
            {
                FeedbackText.Text = "";
                LetterBorder.Background = new SolidColorBrush(Color.FromRgb(44, 62, 80));
                feedbackTimer.Stop();
            };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // ВАЖНО: Чтобы Page получал события клавиатуры
            // 1. Focusable="True" в XAML
            // 2. Focus() после загрузки
            this.Focus();
        }

        /// <summary>
        /// Главный обработчик нажатия клавиш
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isPlaying) return;

            // e.Key содержит нажатую клавишу (Key.A, Key.B, и т.д.)
            Key pressedKey = e.Key;
            
            // Преобразуем букву в Key
            Key expectedKey = (Key)Enum.Parse(typeof(Key), currentLetter.ToString());

            if (pressedKey == expectedKey)
            {
                // Правильно!
                score++;
                ShowFeedback("OK", Colors.LimeGreen);
                GenerateNewLetter();
            }
            else if (pressedKey >= Key.A && pressedKey <= Key.Z)
            {
                // Неправильно (только буквы считаются ошибкой)
                score = Math.Max(0, score - 1);
                ShowFeedback("X", Colors.Red);
            }

            ScoreText.Text = $"Счёт: {score}";
            e.Handled = true;  // Помечаем событие как обработанное
        }

        private void ShowFeedback(string text, Color color)
        {
            FeedbackText.Text = text;
            FeedbackText.Foreground = new SolidColorBrush(color);
            LetterBorder.Background = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
            feedbackTimer.Stop();
            feedbackTimer.Start();
        }

        private void GenerateNewLetter()
        {
            // random.Next(26) -> 0..25
            // 'A' + 0 = 'A', 'A' + 25 = 'Z'
            currentLetter = (char)('A' + random.Next(26));
            LetterText.Text = currentLetter.ToString();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            timeLeft--;
            TimerText.Text = $"Время: {timeLeft}";

            if (timeLeft <= 0)
            {
                EndGame();
            }
            else if (timeLeft <= 5)
            {
                TimerText.Foreground = Brushes.Red;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                // Пауза
                gameTimer.Stop();
                isPlaying = false;
                StartButton.Content = "Старт";
                InstructionText.Text = "Пауза. Нажмите Старт.";
            }
            else
            {
                // Запуск
                if (timeLeft <= 0)
                {
                    timeLeft = 30;
                    score = 0;
                    ScoreText.Text = "Счёт: 0";
                    TimerText.Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18));
                }
                
                isPlaying = true;
                StartButton.Content = "Пауза";
                InstructionText.Text = "Нажимайте показанную букву!";
                GenerateNewLetter();
                gameTimer.Start();
                this.Focus();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            isPlaying = false;
            score = 0;
            timeLeft = 30;
            currentLetter = '?';
            
            ScoreText.Text = "Счёт: 0";
            TimerText.Text = "Время: 30";
            TimerText.Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18));
            LetterText.Text = "?";
            FeedbackText.Text = "";
            InstructionText.Text = "Нажмите Старт!";
            StartButton.Content = "Старт";
            LetterBorder.Background = new SolidColorBrush(Color.FromRgb(44, 62, 80));
            this.Focus();
        }

        private void EndGame()
        {
            gameTimer.Stop();
            isPlaying = false;
            
            if (score > highScore)
            {
                highScore = score;
                HighScoreText.Text = $"Рекорд: {highScore}";
            }
            
            StartButton.Content = "Старт";
            InstructionText.Text = $"Игра окончена! Счёт: {score}";
            LetterText.Text = "!";
            
            MessageBox.Show($"Время вышло!\n\nВаш счёт: {score}\nРекорд: {highScore}", 
                "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

/*
================================================================================
                           КАК ЭТО РАБОТАЕТ
================================================================================

КЛЮЧЕВЫЕ КОНЦЕПЦИИ ГЛАВЫ 3:
---------------------------

1. СОБЫТИЯ КЛАВИАТУРЫ
   - KeyDown - клавиша нажата
   - KeyUp - клавиша отпущена
   - PreviewKeyDown - срабатывает раньше (tunneling)

2. KEY ENUM
   Все клавиши в перечислении Key:
   - Key.A ... Key.Z - буквы
   - Key.D0 ... Key.D9 - цифры
   - Key.Space, Key.Enter - специальные
   - Key.Up, Down, Left, Right - стрелки

3. FOCUS()
   Элемент получает события клавиатуры только с фокусом:
   - В XAML: Focusable="True"
   - В коде: this.Focus();

4. DISPATCHERTIMER
   Таймер в UI-потоке (можно обновлять UI):
   - timer.Interval = TimeSpan.FromSeconds(1);
   - timer.Tick += Handler;
   - timer.Start() / timer.Stop();

5. e.Handled = true
   Помечает событие как обработанное
   Предотвращает дальнейшую обработку

================================================================================
*/
