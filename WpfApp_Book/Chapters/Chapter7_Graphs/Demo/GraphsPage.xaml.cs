using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp_Book.Chapters.Chapter7_Graphs.Demo
{
    public partial class GraphsPage : Page
    {
        private const int CellSize = 30;
        private int mazeWidth = 15, mazeHeight = 10;
        private int[,] maze = null!;
        private Point playerPos, exitPos;
        private Rectangle playerRect = null!, exitRect = null!;
        private List<Rectangle> pathRects = new List<Rectangle>();
        private Random random = new Random();

        public GraphsPage() { InitializeComponent(); }

        private void Page_Loaded(object sender, RoutedEventArgs e) { this.Focus(); GenerateMaze(); DrawMaze(); }

        private void MazeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MazeCanvas.ActualWidth > 0) mazeWidth = Math.Max(5, (int)(MazeCanvas.ActualWidth / CellSize));
            if (MazeCanvas.ActualHeight > 0) mazeHeight = Math.Max(5, (int)(MazeCanvas.ActualHeight / CellSize));
        }

        private void GenerateMaze()
        {
            maze = new int[mazeHeight, mazeWidth];
            for (int y = 0; y < mazeHeight; y++)
                for (int x = 0; x < mazeWidth; x++)
                    maze[y, x] = random.NextDouble() < 0.3 ? 1 : 0;
            
            playerPos = new Point(0, 0); maze[0, 0] = 0;
            exitPos = new Point(mazeWidth - 1, mazeHeight - 1); maze[mazeHeight - 1, mazeWidth - 1] = 0;
            
            int px = 0, py = 0;
            while (px < mazeWidth - 1 || py < mazeHeight - 1)
            {
                maze[py, px] = 0;
                if (px >= mazeWidth - 1) py++;
                else if (py >= mazeHeight - 1) px++;
                else if (random.NextDouble() < 0.5) px++; else py++;
            }
            StatusText.Text = "Найди выход! 🏁";
        }

        private void DrawMaze()
        {
            MazeCanvas.Children.Clear(); pathRects.Clear();
            for (int y = 0; y < mazeHeight; y++)
                for (int x = 0; x < mazeWidth; x++)
                {
                    var cell = new Rectangle { Width = CellSize - 1, Height = CellSize - 1,
                        Fill = new SolidColorBrush(maze[y, x] == 1 ? Color.FromRgb(52, 73, 94) : Color.FromRgb(44, 62, 80)) };
                    Canvas.SetLeft(cell, x * CellSize); Canvas.SetTop(cell, y * CellSize);
                    MazeCanvas.Children.Add(cell);
                }
            
            exitRect = new Rectangle { Width = CellSize - 4, Height = CellSize - 4, Fill = Brushes.LimeGreen };
            Canvas.SetLeft(exitRect, exitPos.X * CellSize + 2); Canvas.SetTop(exitRect, exitPos.Y * CellSize + 2);
            MazeCanvas.Children.Add(exitRect);
            
            playerRect = new Rectangle { Width = CellSize - 6, Height = CellSize - 6, Fill = Brushes.Yellow, RadiusX = 10, RadiusY = 10 };
            Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3); Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);
            MazeCanvas.Children.Add(playerRect);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            Point newPos = playerPos;
            if (e.Key == Key.Up) newPos.Y--;
            else if (e.Key == Key.Down) newPos.Y++;
            else if (e.Key == Key.Left) newPos.X--;
            else if (e.Key == Key.Right) newPos.X++;
            else if (e.Key == Key.Space) { ShowPath(); e.Handled = true; return; }
            
            if (CanMove((int)newPos.X, (int)newPos.Y))
            {
                ClearPath(); playerPos = newPos;
                Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3);
                Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);
                if (playerPos == exitPos) StatusText.Text = "🎉 Победа!";
            }
            e.Handled = true;
        }

        private bool CanMove(int x, int y) => x >= 0 && y >= 0 && x < mazeWidth && y < mazeHeight && maze[y, x] == 0;

        private void ShowPath()
        {
            ClearPath();
            var path = FindPathBFS(playerPos, exitPos);
            if (path == null) { StatusText.Text = "❌ Путь не найден!"; return; }
            StatusText.Text = $"📍 Путь: {path.Count} шагов";
            foreach (var p in path)
            {
                if (p == playerPos || p == exitPos) continue;
                var r = new Rectangle { Width = CellSize - 8, Height = CellSize - 8, Fill = new SolidColorBrush(Color.FromArgb(150, 52, 152, 219)) };
                Canvas.SetLeft(r, p.X * CellSize + 4); Canvas.SetTop(r, p.Y * CellSize + 4);
                MazeCanvas.Children.Add(r); pathRects.Add(r);
            }
        }

        private List<Point>? FindPathBFS(Point start, Point end)
        {
            var queue = new Queue<Point>(); var cameFrom = new Dictionary<Point, Point>();
            queue.Enqueue(start); cameFrom[start] = start;
            int[] dx = { 0, 0, -1, 1 }, dy = { -1, 1, 0, 0 };
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == end)
                {
                    var path = new List<Point>(); var c = end;
                    while (cameFrom[c] != c) { path.Add(c); c = cameFrom[c]; }
                    path.Add(c); path.Reverse(); return path;
                }
                for (int i = 0; i < 4; i++)
                {
                    var next = new Point(current.X + dx[i], current.Y + dy[i]);
                    if (CanMove((int)next.X, (int)next.Y) && !cameFrom.ContainsKey(next))
                    { queue.Enqueue(next); cameFrom[next] = current; }
                }
            }
            return null;
        }

        private void ClearPath() { foreach (var r in pathRects) MazeCanvas.Children.Remove(r); pathRects.Clear(); }
        private void NewMaze_Click(object sender, RoutedEventArgs e) { GenerateMaze(); DrawMaze(); this.Focus(); }
        private void ShowPath_Click(object sender, RoutedEventArgs e) { ShowPath(); this.Focus(); }
    }
}
