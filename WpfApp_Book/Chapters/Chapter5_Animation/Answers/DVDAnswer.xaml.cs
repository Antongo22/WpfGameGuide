using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter5_Animation.Answers
{
    /// <summary>
    /// DVD-заставка - решение задания Главы 5
    /// Демонстрирует: DispatcherTimer, игровой цикл, физика отскока, смена цвета
    /// </summary>
    public partial class DVDAnswer : Page
    {
        // Таймер для игрового цикла (~60 FPS)
        private DispatcherTimer gameTimer;
        private Random random = new Random();
        
        // Скорость движения (пиксели за кадр)
        private double velocityX = 3;  // Скорость по X (+ вправо, - влево)
        private double velocityY = 2;  // Скорость по Y (+ вниз, - вверх)
        private double speed = 5;      // Базовая скорость
        
        // Статистика
        private int bounceCount = 0;   // Количество отскоков
        private int cornerHits = 0;    // Попадания в угол (редкость!)
        private bool isRunning = false;

        public DVDAnswer()
        {
            InitializeComponent();
            
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            gameTimer.Tick += GameLoop;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Начальная позиция
            Canvas.SetLeft(DVDLogo, 100);
            Canvas.SetTop(DVDLogo, 100);
            
            // Случайное направление
            velocityX = (random.NextDouble() > 0.5 ? 1 : -1) * speed;
            velocityY = (random.NextDouble() > 0.5 ? 1 : -1) * speed * 0.7;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
        }

        /// <summary>
        /// Игровой цикл
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            double x = Canvas.GetLeft(DVDLogo);
            double y = Canvas.GetTop(DVDLogo);
            
            // Получаем размеры
            double logoWidth = DVDLogo.ActualWidth > 0 ? DVDLogo.ActualWidth : 100;
            double logoHeight = DVDLogo.ActualHeight > 0 ? DVDLogo.ActualHeight : 50;
            double canvasWidth = AnimationCanvas.ActualWidth;
            double canvasHeight = AnimationCanvas.ActualHeight;

            // Обновляем позицию
            x += velocityX;
            y += velocityY;

            bool bouncedX = false;
            bool bouncedY = false;

            // Проверка столкновения с левой/правой границей
            if (x <= 0)
            {
                x = 0;
                velocityX = Math.Abs(velocityX);
                bouncedX = true;
            }
            else if (x + logoWidth >= canvasWidth)
            {
                x = canvasWidth - logoWidth;
                velocityX = -Math.Abs(velocityX);
                bouncedX = true;
            }

            // Проверка столкновения с верхней/нижней границей
            if (y <= 0)
            {
                y = 0;
                velocityY = Math.Abs(velocityY);
                bouncedY = true;
            }
            else if (y + logoHeight >= canvasHeight)
            {
                y = canvasHeight - logoHeight;
                velocityY = -Math.Abs(velocityY);
                bouncedY = true;
            }

            // Если был отскок — меняем цвет
            if (bouncedX || bouncedY)
            {
                bounceCount++;
                BounceCountText.Text = bounceCount.ToString();
                ChangeColor();

                // Проверяем попадание в угол!
                if (bouncedX && bouncedY)
                {
                    cornerHits++;
                    CornerHitText.Text = $"🎯 Угол x{cornerHits}!";
                    // Особый эффект для угла
                    DVDLogo.Background = Brushes.Gold;
                    DVDText.Foreground = Brushes.Black;
                }
            }

            Canvas.SetLeft(DVDLogo, x);
            Canvas.SetTop(DVDLogo, y);
        }

        /// <summary>
        /// Смена цвета на случайный
        /// </summary>
        private void ChangeColor()
        {
            Color newColor = Color.FromRgb(
                (byte)random.Next(100, 256),
                (byte)random.Next(100, 256),
                (byte)random.Next(100, 256)
            );
            DVDLogo.Background = new SolidColorBrush(newColor);
            DVDText.Foreground = Brushes.White;
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                gameTimer.Stop();
                StartStopButton.Content = "▶ Старт";
                StartStopButton.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            }
            else
            {
                gameTimer.Start();
                StartStopButton.Content = "⏸ Стоп";
                StartStopButton.Background = new SolidColorBrush(Color.FromRgb(243, 156, 18));
            }
            isRunning = !isRunning;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            isRunning = false;
            
            bounceCount = 0;
            cornerHits = 0;
            BounceCountText.Text = "0";
            CornerHitText.Text = "";
            
            Canvas.SetLeft(DVDLogo, 100);
            Canvas.SetTop(DVDLogo, 100);
            
            velocityX = (random.NextDouble() > 0.5 ? 1 : -1) * speed;
            velocityY = (random.NextDouble() > 0.5 ? 1 : -1) * speed * 0.7;
            
            DVDLogo.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            DVDText.Foreground = Brushes.White;
            
            StartStopButton.Content = "▶ Старт";
            StartStopButton.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SpeedText == null) return;
            
            speed = e.NewValue;
            SpeedText.Text = speed.ToString("F0");
            
            // Обновляем скорости с сохранением направления
            velocityX = Math.Sign(velocityX) * speed;
            velocityY = Math.Sign(velocityY) * speed * 0.7;
        }
    }
}

/*
================================================================================
                           КАК ЭТО РАБОТАЕТ
================================================================================

КЛЮЧЕВЫЕ КОНЦЕПЦИИ ГЛАВЫ 5:
---------------------------

1. ИГРОВОЙ ЦИКЛ (GAME LOOP)
   - DispatcherTimer с интервалом 16мс = ~60 FPS
   - Каждый тик: обновляем позицию, проверяем столкновения, перерисовываем
   
   gameTimer.Interval = TimeSpan.FromMilliseconds(16);
   gameTimer.Tick += GameLoop;

2. ФИЗИКА ДВИЖЕНИЯ
   - Позиция = Позиция + Скорость
   - x += velocityX;
   - y += velocityY;
   
   velocityX > 0 = движение вправо
   velocityX < 0 = движение влево
   velocityY > 0 = движение вниз
   velocityY < 0 = движение вверх

3. ФИЗИКА ОТСКОКА
   При столкновении с границей - инвертируем скорость:
   
   if (x <= 0 || x + width >= canvasWidth)
       velocityX = -velocityX;  // Отражение по X
   
   if (y <= 0 || y + height >= canvasHeight)
       velocityY = -velocityY;  // Отражение по Y

4. MATH.SIGN - СОХРАНЕНИЕ НАПРАВЛЕНИЯ
   Math.Sign(value) возвращает:
   -1 если value < 0
    0 если value = 0
   +1 если value > 0
   
   Используется для изменения скорости с сохранением направления:
   velocityX = Math.Sign(velocityX) * newSpeed;

5. СМЕНА ЦВЕТА
   Color.FromRgb(red, green, blue) - создание цвета
   Random для случайных значений:
   
   Color newColor = Color.FromRgb(
       (byte)random.Next(100, 256),
       (byte)random.Next(100, 256),
       (byte)random.Next(100, 256)
   );
   element.Fill = new SolidColorBrush(newColor);

================================================================================
*/
