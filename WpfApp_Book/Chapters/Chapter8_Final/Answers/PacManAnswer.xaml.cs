using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfGameGuide.Chapters.Chapter8_Final.Answers
{
    public partial class PacManAnswer : Page
    {
        #region Constants
        private const int CellSize = 19;
        private const int MapWidth = 28;
        private const int MapHeight = 31;
        #endregion

        #region Game State
        private enum Direction { None, Up, Down, Left, Right }
        private enum GameState { Ready, Playing, Paused, Dying, GameOver, Win }
        
        private GameState gameState = GameState.Ready;
        private int score = 0;
        private int highScore = 0;
        private int lives = 3;
        private int level = 1;
        private int dotsCollected = 0;
        private int totalDots = 0;
        #endregion

        #region Pac-Man
        private Point pacmanPos;
        private Point pacmanStartPos;
        private Direction pacmanDir = Direction.None;
        private Direction nextDir = Direction.None;
        private Path? pacmanElement;
        private bool mouthOpen = true;
        private int animationTick = 0;
        #endregion

        #region Ghosts
        private class Ghost
        {
            public Point Position;
            public Point StartPosition;
            public Direction Direction;
            public Ellipse? Element;
            public Ellipse? Eyes;
            public Color NormalColor;
            public bool IsScared;
            public bool IsEaten;
            public int ScatterTicks;
        }
        
        private List<Ghost> ghosts = new List<Ghost>();
        private bool powerMode = false;
        private int powerModeTicks = 0;
        private const int PowerModeDuration = 80;
        #endregion

        #region Map
        // 0=empty, 1=wall, 2=dot, 3=power, 4=ghost house, 5=tunnel
        private int[,] map = null!;
        private int[,] originalMap = null!;
        private Rectangle[,] mapCells = null!;
        private List<Ellipse> dotElements = new List<Ellipse>();
        #endregion

        #region Timers
        private DispatcherTimer gameTimer = null!;
        private DispatcherTimer animationTimer = null!;
        private Random random = new Random();
        #endregion

        // Классическая карта Pac-Man
        private static readonly string[] MapTemplate = {
            "1111111111111111111111111111",
            "1222222222222112222222222221",
            "1211112111112112111112111121",
            "1311112111112112111112111131",
            "1211112111112112111112111121",
            "1222222222222222222222222221",
            "1211112112111111112112111121",
            "1211112112111111112112111121",
            "1222222112222112222112222221",
            "1111112111110110111112111111",
            "0000012111110110111112100000",
            "0000012110000000000112100000",
            "0000012110111441110112100000",
            "1111112110100000010112111111",
            "5000002000100000010002000005",
            "1111112110100000010112111111",
            "0000012110111111110112100000",
            "0000012110000000000112100000",
            "0000012110111111110112100000",
            "1111112110111111110112111111",
            "1222222222222112222222222221",
            "1211112111112112111112111121",
            "1211112111112112111112111121",
            "1322112222222002222222112231",
            "1112112112111111112112112111",
            "1112112112111111112112112111",
            "1222222112222112222112222221",
            "1211111111112112111111111121",
            "1211111111112112111111111121",
            "1222222222222222222222222221",
            "1111111111111111111111111111"
        };

        public PacManAnswer()
        {
            InitializeComponent();
            
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            gameTimer.Tick += GameLoop;
            
            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animationTimer.Tick += AnimationLoop;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            InitGame();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            animationTimer.Stop();
        }

        #region Game Initialization
        private void InitGame()
        {
            gameTimer.Stop();
            animationTimer.Stop();
            
            GameCanvas.Children.Clear();
            dotElements.Clear();
            ghosts.Clear();
            
            gameState = GameState.Ready;
            score = 0;
            lives = 3;
            level = 1;
            dotsCollected = 0;
            powerMode = false;
            pacmanDir = Direction.None;
            nextDir = Direction.None;
            
            UpdateUI();
            CreateMap();
            DrawMap();
            SpawnPacman();
            SpawnGhosts();
            UpdateLivesDisplay();
            
            StatusText.Text = "PRESS SPACE TO START";
        }

        private void CreateMap()
        {
            map = new int[MapHeight, MapWidth];
            originalMap = new int[MapHeight, MapWidth];
            totalDots = 0;
            
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int cell = MapTemplate[y][x] - '0';
                    map[y, x] = cell;
                    originalMap[y, x] = cell;
                    if (cell == 2 || cell == 3) totalDots++;
                }
            }
        }

        private void DrawMap()
        {
            mapCells = new Rectangle[MapHeight, MapWidth];
            
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int cell = map[y, x];
                    
                    if (cell == 1) // Стена
                    {
                        var wall = new Rectangle
                        {
                            Width = CellSize,
                            Height = CellSize,
                            Fill = new SolidColorBrush(Color.FromRgb(33, 33, 255)),
                            Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 139)),
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(wall, x * CellSize);
                        Canvas.SetTop(wall, y * CellSize);
                        GameCanvas.Children.Add(wall);
                        mapCells[y, x] = wall;
                    }
                    else if (cell == 2) // Точка
                    {
                        var dot = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
                            Fill = new SolidColorBrush(Color.FromRgb(255, 184, 151))
                        };
                        Canvas.SetLeft(dot, x * CellSize + CellSize / 2 - 2);
                        Canvas.SetTop(dot, y * CellSize + CellSize / 2 - 2);
                        GameCanvas.Children.Add(dot);
                        dotElements.Add(dot);
                    }
                    else if (cell == 3) // Power pellet
                    {
                        var power = new Ellipse
                        {
                            Width = 12,
                            Height = 12,
                            Fill = new SolidColorBrush(Color.FromRgb(255, 184, 151))
                        };
                        Canvas.SetLeft(power, x * CellSize + CellSize / 2 - 6);
                        Canvas.SetTop(power, y * CellSize + CellSize / 2 - 6);
                        GameCanvas.Children.Add(power);
                        dotElements.Add(power);
                    }
                }
            }
        }

        private void SpawnPacman()
        {
            pacmanStartPos = new Point(14, 23);
            pacmanPos = pacmanStartPos;
            pacmanDir = Direction.None;
            nextDir = Direction.None;
            
            // Создаём Pac-Man как Path (для анимации рта)
            pacmanElement = CreatePacmanShape();
            Canvas.SetLeft(pacmanElement, pacmanPos.X * CellSize);
            Canvas.SetTop(pacmanElement, pacmanPos.Y * CellSize);
            GameCanvas.Children.Add(pacmanElement);
        }

        private Path CreatePacmanShape()
        {
            var path = new Path
            {
                Fill = Brushes.Yellow,
                Width = CellSize,
                Height = CellSize
            };
            UpdatePacmanMouth(path, mouthOpen);
            return path;
        }

        private void UpdatePacmanMouth(Path path, bool open)
        {
            double size = CellSize;
            double center = size / 2;
            double radius = size / 2 - 1;
            
            // Угол рта в зависимости от направления
            double startAngle = pacmanDir switch
            {
                Direction.Right => open ? 35 : 5,
                Direction.Left => open ? 215 : 185,
                Direction.Up => open ? 305 : 275,
                Direction.Down => open ? 125 : 95,
                _ => open ? 35 : 5
            };
            
            double sweepAngle = open ? 290 : 350;
            
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                double startRad = startAngle * Math.PI / 180;
                double endRad = (startAngle + sweepAngle) * Math.PI / 180;
                
                Point startPoint = new Point(
                    center + radius * Math.Cos(startRad),
                    center + radius * Math.Sin(startRad)
                );
                Point endPoint = new Point(
                    center + radius * Math.Cos(endRad),
                    center + radius * Math.Sin(endRad)
                );
                
                ctx.BeginFigure(new Point(center, center), true, true);
                ctx.LineTo(startPoint, true, false);
                ctx.ArcTo(endPoint, new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise, true, false);
            }
            geometry.Freeze();
            path.Data = geometry;
        }

        private void SpawnGhosts()
        {
            var ghostData = new[]
            {
                (new Point(13, 11), Color.FromRgb(255, 0, 0)),     // Blinky (Red)
                (new Point(14, 14), Color.FromRgb(255, 184, 255)), // Pinky (Pink)
                (new Point(13, 14), Color.FromRgb(0, 255, 255)),   // Inky (Cyan)
                (new Point(14, 11), Color.FromRgb(255, 184, 82))   // Clyde (Orange)
            };

            foreach (var (pos, color) in ghostData)
            {
                var ghost = new Ghost
                {
                    Position = pos,
                    StartPosition = pos,
                    Direction = Direction.Up,
                    NormalColor = color,
                    IsScared = false,
                    IsEaten = false
                };

                // Тело призрака
                ghost.Element = new Ellipse
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    Fill = new SolidColorBrush(color)
                };
                
                // Глаза
                ghost.Eyes = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.White
                };

                Canvas.SetLeft(ghost.Element, pos.X * CellSize + 1);
                Canvas.SetTop(ghost.Element, pos.Y * CellSize + 1);
                Canvas.SetLeft(ghost.Eyes, pos.X * CellSize + CellSize / 2 - 3);
                Canvas.SetTop(ghost.Eyes, pos.Y * CellSize + 4);
                
                GameCanvas.Children.Add(ghost.Element);
                GameCanvas.Children.Add(ghost.Eyes);
                ghosts.Add(ghost);
            }
        }
        #endregion

        #region Game Loop
        private void GameLoop(object? sender, EventArgs e)
        {
            if (gameState != GameState.Playing) return;

            // Power mode countdown
            if (powerMode)
            {
                powerModeTicks--;
                if (powerModeTicks <= 0)
                {
                    EndPowerMode();
                }
                else if (powerModeTicks <= 20)
                {
                    // Мигание перед окончанием
                    foreach (var ghost in ghosts)
                    {
                        if (ghost.IsScared && !ghost.IsEaten)
                        {
                            ghost.Element!.Fill = (powerModeTicks % 4 < 2) 
                                ? Brushes.Blue 
                                : Brushes.White;
                        }
                    }
                }
            }

            MovePacman();
            MoveGhosts();
            CheckCollisions();
        }

        private void AnimationLoop(object? sender, EventArgs e)
        {
            if (gameState != GameState.Playing) return;
            
            animationTick++;
            if (animationTick % 2 == 0 && pacmanDir != Direction.None)
            {
                mouthOpen = !mouthOpen;
                if (pacmanElement != null)
                {
                    UpdatePacmanMouth(pacmanElement, mouthOpen);
                }
            }
        }

        private void MovePacman()
        {
            // Пробуем применить следующее направление
            if (nextDir != Direction.None)
            {
                Point nextPos = GetNextPosition(pacmanPos, nextDir);
                if (CanMove(nextPos))
                {
                    pacmanDir = nextDir;
                }
            }

            if (pacmanDir == Direction.None) return;

            Point newPos = GetNextPosition(pacmanPos, pacmanDir);
            
            // Туннель
            if (newPos.X < 0) newPos.X = MapWidth - 1;
            else if (newPos.X >= MapWidth) newPos.X = 0;
            
            if (CanMove(newPos))
            {
                pacmanPos = newPos;
                Canvas.SetLeft(pacmanElement!, pacmanPos.X * CellSize);
                Canvas.SetTop(pacmanElement!, pacmanPos.Y * CellSize);
                
                CollectDot();
            }
        }

        private void MoveGhosts()
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.IsEaten)
                {
                    // Возвращаемся в дом
                    MoveTowardsTarget(ghost, ghost.StartPosition);
                    if (ghost.Position == ghost.StartPosition)
                    {
                        ghost.IsEaten = false;
                        ghost.IsScared = false;
                        ghost.Element!.Fill = new SolidColorBrush(ghost.NormalColor);
                        ghost.Eyes!.Fill = Brushes.White;
                    }
                }
                else if (ghost.IsScared)
                {
                    // Случайное движение когда напуган
                    MoveRandomly(ghost);
                }
                else
                {
                    // Преследование
                    ghost.ScatterTicks++;
                    if (ghost.ScatterTicks % 200 < 50)
                    {
                        MoveRandomly(ghost); // Периодически разбредаются
                    }
                    else
                    {
                        MoveTowardsTarget(ghost, pacmanPos);
                    }
                }

                Canvas.SetLeft(ghost.Element!, ghost.Position.X * CellSize + 1);
                Canvas.SetTop(ghost.Element!, ghost.Position.Y * CellSize + 1);
                Canvas.SetLeft(ghost.Eyes!, ghost.Position.X * CellSize + CellSize / 2 - 3);
                Canvas.SetTop(ghost.Eyes!, ghost.Position.Y * CellSize + 4);
            }
        }

        private void MoveTowardsTarget(Ghost ghost, Point target)
        {
            var possibleDirs = GetPossibleDirections(ghost);
            if (possibleDirs.Count == 0) return;

            Direction bestDir = possibleDirs[0];
            double bestDist = double.MaxValue;

            foreach (var dir in possibleDirs)
            {
                Point nextPos = GetNextPosition(ghost.Position, dir);
                double dist = Distance(nextPos, target);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestDir = dir;
                }
            }

            ghost.Direction = bestDir;
            ghost.Position = GetNextPosition(ghost.Position, bestDir);
            
            // Туннель для призраков
            if (ghost.Position.X < 0) ghost.Position = new Point(MapWidth - 1, ghost.Position.Y);
            else if (ghost.Position.X >= MapWidth) ghost.Position = new Point(0, ghost.Position.Y);
        }

        private void MoveRandomly(Ghost ghost)
        {
            var possibleDirs = GetPossibleDirections(ghost);
            if (possibleDirs.Count == 0) return;

            // Предпочитаем текущее направление, если возможно
            if (possibleDirs.Contains(ghost.Direction) && random.NextDouble() > 0.3)
            {
                ghost.Position = GetNextPosition(ghost.Position, ghost.Direction);
            }
            else
            {
                ghost.Direction = possibleDirs[random.Next(possibleDirs.Count)];
                ghost.Position = GetNextPosition(ghost.Position, ghost.Direction);
            }
            
            // Туннель
            if (ghost.Position.X < 0) ghost.Position = new Point(MapWidth - 1, ghost.Position.Y);
            else if (ghost.Position.X >= MapWidth) ghost.Position = new Point(0, ghost.Position.Y);
        }

        private List<Direction> GetPossibleDirections(Ghost ghost)
        {
            var dirs = new List<Direction>();
            var opposites = new Dictionary<Direction, Direction>
            {
                { Direction.Up, Direction.Down },
                { Direction.Down, Direction.Up },
                { Direction.Left, Direction.Right },
                { Direction.Right, Direction.Left }
            };

            foreach (Direction dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
            {
                // Не разворачиваемся на 180°
                if (opposites.TryGetValue(ghost.Direction, out var opposite) && dir == opposite)
                    continue;

                Point next = GetNextPosition(ghost.Position, dir);
                if (CanMoveGhost(next))
                {
                    dirs.Add(dir);
                }
            }

            // Если нет вариантов — разворот
            if (dirs.Count == 0 && opposites.TryGetValue(ghost.Direction, out var opp))
            {
                Point back = GetNextPosition(ghost.Position, opp);
                if (CanMoveGhost(back))
                    dirs.Add(opp);
            }

            return dirs;
        }
        #endregion

        #region Collision Detection
        private void CheckCollisions()
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.Position == pacmanPos || 
                    (Math.Abs(ghost.Position.X - pacmanPos.X) < 0.5 && 
                     Math.Abs(ghost.Position.Y - pacmanPos.Y) < 0.5))
                {
                    if (ghost.IsScared && !ghost.IsEaten)
                    {
                        // Съедаем призрака
                        EatGhost(ghost);
                    }
                    else if (!ghost.IsEaten)
                    {
                        // Pac-Man умирает
                        Die();
                        return;
                    }
                }
            }
        }

        private void EatGhost(Ghost ghost)
        {
            ghost.IsEaten = true;
            ghost.Element!.Fill = Brushes.Transparent;
            ghost.Eyes!.Fill = Brushes.Blue;
            
            score += 200 * (int)Math.Pow(2, ghosts.FindAll(g => g.IsEaten).Count - 1);
            UpdateUI();
        }

        private void CollectDot()
        {
            int x = (int)pacmanPos.X;
            int y = (int)pacmanPos.Y;
            
            if (map[y, x] == 2) // Обычная точка
            {
                map[y, x] = 0;
                score += 10;
                dotsCollected++;
                RemoveDotAt(x, y);
                UpdateUI();
                CheckWin();
            }
            else if (map[y, x] == 3) // Power pellet
            {
                map[y, x] = 0;
                score += 50;
                dotsCollected++;
                RemoveDotAt(x, y);
                UpdateUI();
                StartPowerMode();
                CheckWin();
            }
        }

        private void RemoveDotAt(int x, int y)
        {
            double centerX = x * CellSize + CellSize / 2;
            double centerY = y * CellSize + CellSize / 2;

            for (int i = dotElements.Count - 1; i >= 0; i--)
            {
                var dot = dotElements[i];
                double dotCenterX = Canvas.GetLeft(dot) + dot.Width / 2;
                double dotCenterY = Canvas.GetTop(dot) + dot.Height / 2;

                if (Math.Abs(dotCenterX - centerX) < CellSize / 2 && 
                    Math.Abs(dotCenterY - centerY) < CellSize / 2)
                {
                    GameCanvas.Children.Remove(dot);
                    dotElements.RemoveAt(i);
                    break;
                }
            }
        }
        #endregion

        #region Power Mode
        private void StartPowerMode()
        {
            powerMode = true;
            powerModeTicks = PowerModeDuration;

            foreach (var ghost in ghosts)
            {
                if (!ghost.IsEaten)
                {
                    ghost.IsScared = true;
                    ghost.Element!.Fill = Brushes.Blue;
                }
            }
        }

        private void EndPowerMode()
        {
            powerMode = false;
            foreach (var ghost in ghosts)
            {
                ghost.IsScared = false;
                if (!ghost.IsEaten)
                {
                    ghost.Element!.Fill = new SolidColorBrush(ghost.NormalColor);
                }
            }
        }
        #endregion

        #region Life & Death
        private void Die()
        {
            gameTimer.Stop();
            animationTimer.Stop();
            gameState = GameState.Dying;
            
            lives--;
            
            if (lives <= 0)
            {
                lives = 0;
                GameOver();
            }
            else
            {
                UpdateLivesDisplay();
                StatusText.Text = "READY!";
                
                // Небольшая задержка перед респавном
                var respawnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                respawnTimer.Tick += (s, e) =>
                {
                    respawnTimer.Stop();
                    Respawn();
                };
                respawnTimer.Start();
            }
        }

        private void Respawn()
        {
            pacmanPos = pacmanStartPos;
            pacmanDir = Direction.None;
            nextDir = Direction.None;
            
            Canvas.SetLeft(pacmanElement!, pacmanPos.X * CellSize);
            Canvas.SetTop(pacmanElement!, pacmanPos.Y * CellSize);
            
            // Сброс призраков
            foreach (var ghost in ghosts)
            {
                ghost.Position = ghost.StartPosition;
                ghost.Direction = Direction.Up;
                ghost.IsScared = false;
                ghost.IsEaten = false;
                ghost.Element!.Fill = new SolidColorBrush(ghost.NormalColor);
                ghost.Eyes!.Fill = Brushes.White;
                
                Canvas.SetLeft(ghost.Element, ghost.Position.X * CellSize + 1);
                Canvas.SetTop(ghost.Element, ghost.Position.Y * CellSize + 1);
            }
            
            EndPowerMode();
            
            gameState = GameState.Playing;
            StatusText.Text = "";
            gameTimer.Start();
            animationTimer.Start();
            this.Focus();
        }

        private void GameOver()
        {
            gameState = GameState.GameOver;
            UpdateLivesDisplay();
            StatusText.Text = "GAME OVER";
            
            if (score > highScore)
            {
                highScore = score;
                HighScoreText.Text = highScore.ToString();
            }
            
            MessageBox.Show($"GAME OVER\n\nScore: {score}\nHigh Score: {highScore}", 
                "Pac-Man", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckWin()
        {
            if (dotsCollected >= totalDots)
            {
                gameTimer.Stop();
                animationTimer.Stop();
                gameState = GameState.Win;
                
                level++;
                LevelText.Text = level.ToString();
                
                MessageBox.Show($"LEVEL {level - 1} COMPLETE!\n\nScore: {score}", 
                    "Pac-Man", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Следующий уровень
                NextLevel();
            }
        }

        private void NextLevel()
        {
            // Восстанавливаем карту
            GameCanvas.Children.Clear();
            dotElements.Clear();
            ghosts.Clear();
            dotsCollected = 0;
            powerMode = false;
            
            CreateMap();
            DrawMap();
            SpawnPacman();
            SpawnGhosts();
            
            // Увеличиваем сложность
            int newInterval = Math.Max(60, 120 - level * 10);
            gameTimer.Interval = TimeSpan.FromMilliseconds(newInterval);
            
            gameState = GameState.Playing;
            StatusText.Text = "";
            gameTimer.Start();
            animationTimer.Start();
            this.Focus();
        }
        #endregion

        #region Input
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    nextDir = Direction.Up;
                    break;
                case Key.Down:
                case Key.S:
                    nextDir = Direction.Down;
                    break;
                case Key.Left:
                case Key.A:
                    nextDir = Direction.Left;
                    break;
                case Key.Right:
                case Key.D:
                    nextDir = Direction.Right;
                    break;
                case Key.Space:
                    HandleSpace();
                    break;
                case Key.Escape:
                    if (gameState == GameState.Playing)
                        PauseGame();
                    break;
                case Key.R:
                    InitGame();
                    break;
            }
            e.Handled = true;
        }

        private void HandleSpace()
        {
            switch (gameState)
            {
                case GameState.Ready:
                    StartGame();
                    break;
                case GameState.Playing:
                    PauseGame();
                    break;
                case GameState.Paused:
                    ResumeGame();
                    break;
                case GameState.GameOver:
                    InitGame();
                    break;
            }
        }

        private void StartGame()
        {
            gameState = GameState.Playing;
            StatusText.Text = "";
            gameTimer.Start();
            animationTimer.Start();
        }

        private void PauseGame()
        {
            gameState = GameState.Paused;
            gameTimer.Stop();
            animationTimer.Stop();
            StatusText.Text = "PAUSED";
        }

        private void ResumeGame()
        {
            gameState = GameState.Playing;
            StatusText.Text = "";
            gameTimer.Start();
            animationTimer.Start();
        }
        #endregion

        #region Helpers
        private Point GetNextPosition(Point pos, Direction dir)
        {
            return dir switch
            {
                Direction.Up => new Point(pos.X, pos.Y - 1),
                Direction.Down => new Point(pos.X, pos.Y + 1),
                Direction.Left => new Point(pos.X - 1, pos.Y),
                Direction.Right => new Point(pos.X + 1, pos.Y),
                _ => pos
            };
        }

        private bool CanMove(Point pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            // Туннель
            if (x < 0 || x >= MapWidth) return true;
            if (y < 0 || y >= MapHeight) return false;
            
            int cell = map[y, x];
            return cell != 1 && cell != 4; // Не стена и не дом призраков
        }

        private bool CanMoveGhost(Point pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            
            if (x < 0 || x >= MapWidth) return true;
            if (y < 0 || y >= MapHeight) return false;
            
            return map[y, x] != 1; // Только не стена
        }

        private double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private void UpdateUI()
        {
            ScoreText.Text = score.ToString();
            if (score > highScore)
            {
                highScore = score;
                HighScoreText.Text = highScore.ToString();
            }
        }

        private void UpdateLivesDisplay()
        {
            LivesPanel.Children.Clear();
            // Math.Max(0, lives) предотвращает ошибку при отрицательных значениях
            int displayLives = Math.Max(0, lives);
            for (int i = 0; i < displayLives; i++)
            {
                var lifeIcon = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.Yellow,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                LivesPanel.Children.Add(lifeIcon);
            }
        }
        #endregion
    }
}

/*
================================================================================
                     PAC-MAN: ФИНАЛЬНЫЙ ПРОЕКТ
================================================================================

Этот проект объединяет ВСЕ концепции из предыдущих глав:

ГЛАВА 1 - КОМПОНЕНТЫ:
- TextBlock для счета, жизней, статуса
- Панели для организации UI

ГЛАВА 2 - ОКНА И СТРАНИЦЫ:
- Page для игрового экрана
- MessageBox для Game Over / Victory

ГЛАВА 3 - КЛАВИАТУРА:
- KeyDown для управления (WASD / стрелки)
- Focus() для получения событий
- Буферизация направления (nextDir)

ГЛАВА 4 - ПОЗИЦИОНИРОВАНИЕ:
- Canvas для игрового поля
- Canvas.SetLeft/SetTop для позиционирования
- Координаты на основе сетки (x * CellSize)

ГЛАВА 5 - АНИМАЦИЯ:
- DispatcherTimer для игрового цикла
- Отдельный таймер для анимации рта
- Таймер для движения призраков

ГЛАВА 6 - УПРАВЛЕНИЕ И СВЯЗАННЫЕ ОБЪЕКТЫ:
- Управление стрелками/WASD
- Ghost как класс с несколькими свойствами
- List<Ghost> для управления группой объектов

ГЛАВА 7 - ГРАФЫ И ПОИСК ПУТИ:
- 2D-массив map[,] как лабиринт
- BFS для ИИ призраков (преследование)
- Проверка проходимости CanMove()

ДОПОЛНИТЕЛЬНЫЕ КОНЦЕПЦИИ:
-------------------------

1. GAMESTATE ENUM - состояние игры
   Ready, Playing, Paused, Dying, GameOver, Win
   
   Позволяет правильно обрабатывать паузу, смерть, победу

2. POWER MODE
   - Таймер powerModeTicks
   - Призраки меняют поведение (убегают)
   - Можно съесть призрака

3. GHOST AI
   - Scatter mode (разбредаются периодически)
   - Chase mode (преследуют Pac-Man через BFS)
   - Frightened mode (убегают при power-up)

4. КЛАССИЧЕСКАЯ КАРТА
   28x31 клеток как в оригинале
   Хранится в массиве строк MapTemplate[]

5. АНИМАЦИЯ РОТА PAC-MAN
   Path + StreamGeometry для отрисовки "пирога"
   Угол рта зависит от направления движения

================================================================================
*/
