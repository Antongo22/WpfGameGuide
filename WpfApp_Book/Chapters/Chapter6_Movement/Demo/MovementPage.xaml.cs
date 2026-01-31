using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter6_Movement.Demo
{
    public partial class MovementPage : Page
    {
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Right;
        private List<Point> snakeBody = new List<Point>();
        private List<Rectangle> snakeSegments = new List<Rectangle>();
        private const int CellSize = 20;
        private Point foodPosition;
        private Ellipse? foodElement;
        private DispatcherTimer gameTimer;
        private bool isRunning = false;
        private int score = 0;
        private Random random = new Random();

        public MovementPage()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            gameTimer.Tick += GameLoop;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { this.Focus(); InitGame(); }
        private void Page_Unloaded(object sender, RoutedEventArgs e) { gameTimer.Stop(); }

        private void InitGame()
        {
            GameCanvas.Children.Clear();
            snakeBody.Clear(); snakeSegments.Clear();
            currentDirection = Direction.Right; score = 0;
            ScoreText.Text = "Счёт: 0";
            
            for (int i = 0; i < 3; i++)
            {
                Point pos = new Point(100 - i * CellSize, 100);
                snakeBody.Add(pos);
                var seg = new Rectangle { Width = CellSize - 2, Height = CellSize - 2, Fill = Brushes.LimeGreen };
                Canvas.SetLeft(seg, pos.X); Canvas.SetTop(seg, pos.Y);
                snakeSegments.Add(seg); GameCanvas.Children.Add(seg);
            }
            SpawnFood();
        }

        private void SpawnFood()
        {
            if (foodElement != null) GameCanvas.Children.Remove(foodElement);
            int maxX = Math.Max(5, (int)(GameCanvas.ActualWidth / CellSize) - 1);
            int maxY = Math.Max(5, (int)(GameCanvas.ActualHeight / CellSize) - 1);
            foodPosition = new Point(random.Next(0, maxX) * CellSize, random.Next(0, maxY) * CellSize);
            foodElement = new Ellipse { Width = CellSize - 2, Height = CellSize - 2, Fill = Brushes.Red };
            Canvas.SetLeft(foodElement, foodPosition.X); Canvas.SetTop(foodElement, foodPosition.Y);
            GameCanvas.Children.Add(foodElement);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && currentDirection != Direction.Down) currentDirection = Direction.Up;
            else if (e.Key == Key.Down && currentDirection != Direction.Up) currentDirection = Direction.Down;
            else if (e.Key == Key.Left && currentDirection != Direction.Right) currentDirection = Direction.Left;
            else if (e.Key == Key.Right && currentDirection != Direction.Left) currentDirection = Direction.Right;
            e.Handled = true;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            Point head = snakeBody[0];
            Point newHead = currentDirection switch
            {
                Direction.Up => new Point(head.X, head.Y - CellSize),
                Direction.Down => new Point(head.X, head.Y + CellSize),
                Direction.Left => new Point(head.X - CellSize, head.Y),
                _ => new Point(head.X + CellSize, head.Y)
            };

            if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= GameCanvas.ActualWidth || newHead.Y >= GameCanvas.ActualHeight)
            { gameTimer.Stop(); isRunning = false; StartButton.Content = "▶"; return; }

            snakeBody.Insert(0, newHead);
            var seg = new Rectangle { Width = CellSize - 2, Height = CellSize - 2, Fill = Brushes.LimeGreen };
            Canvas.SetLeft(seg, newHead.X); Canvas.SetTop(seg, newHead.Y);
            snakeSegments.Insert(0, seg); GameCanvas.Children.Add(seg);

            if (Math.Abs(newHead.X - foodPosition.X) < CellSize && Math.Abs(newHead.Y - foodPosition.Y) < CellSize)
            { score++; ScoreText.Text = $"Счёт: {score}"; SpawnFood(); }
            else
            {
                snakeBody.RemoveAt(snakeBody.Count - 1);
                var tail = snakeSegments[snakeSegments.Count - 1];
                GameCanvas.Children.Remove(tail); snakeSegments.RemoveAt(snakeSegments.Count - 1);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) { gameTimer.Stop(); StartButton.Content = "▶"; }
            else { gameTimer.Start(); StartButton.Content = "⏸"; }
            isRunning = !isRunning; this.Focus();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        { gameTimer.Stop(); isRunning = false; StartButton.Content = "▶"; InitGame(); this.Focus(); }
    }
}
