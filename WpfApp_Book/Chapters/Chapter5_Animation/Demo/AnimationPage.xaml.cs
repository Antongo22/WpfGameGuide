using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter5_Animation.Demo
{
    public partial class AnimationPage : Page
    {
        private DispatcherTimer gameTimer;
        private double velocityX = 4, velocityY = 3;
        private int bounceCount = 0;
        private Random random = new Random();
        private bool isRunning = false;

        public AnimationPage()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(Ball, 50);
            Canvas.SetTop(Ball, 50);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) { gameTimer.Stop(); StartStopButton.Content = "▶ Старт"; }
            else { gameTimer.Start(); StartStopButton.Content = "⏸ Стоп"; }
            isRunning = !isRunning;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            double x = Canvas.GetLeft(Ball), y = Canvas.GetTop(Ball);
            x += velocityX; y += velocityY;
            
            bool bounced = false;
            if (x <= 0 || x + 50 >= AnimationCanvas.ActualWidth) { velocityX = -velocityX; bounced = true; }
            if (y <= 0 || y + 50 >= AnimationCanvas.ActualHeight) { velocityY = -velocityY; bounced = true; }
            
            if (bounced)
            {
                BallBrush.Color = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                bounceCount++;
                BounceCountText.Text = $"Отскоков: {bounceCount}";
            }
            
            Canvas.SetLeft(Ball, Math.Clamp(x, 0, AnimationCanvas.ActualWidth - 50));
            Canvas.SetTop(Ball, Math.Clamp(y, 0, AnimationCanvas.ActualHeight - 50));
        }
    }
}
