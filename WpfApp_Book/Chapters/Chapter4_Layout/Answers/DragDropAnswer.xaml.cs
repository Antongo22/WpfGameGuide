using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp_Book.Chapters.Chapter4_Layout.Answers
{
    /// <summary>
    /// Drag-Drop редактор - решение задания Главы 4
    /// Демонстрирует: Canvas, координаты, CaptureMouse, расчет расстояния, AABB collision
    /// </summary>
    public partial class DragDropAnswer : Page
    {
        // Списки для хранения всех фигур и их свойств
        private List<Rectangle> shapes = new List<Rectangle>();
        private List<Point> initialPositions = new List<Point>();       // Начальные позиции для сброса
        private List<SolidColorBrush> originalColors = new List<SolidColorBrush>();  // Оригинальные цвета
        
        // Состояние перетаскивания
        private Rectangle? draggedShape = null;  // Текущая перетаскиваемая фигура
        private Point clickOffset;               // Смещение клика от угла фигуры
        private bool isDragging = false;         // Флаг: идет ли перетаскивание

        // Цвета фигур
        private static readonly Color[] shapeColors = 
        {
            Color.FromRgb(231, 76, 60),   // Красный
            Color.FromRgb(46, 204, 113),  // Зелёный
            Color.FromRgb(52, 152, 219),  // Синий
            Color.FromRgb(155, 89, 182),  // Фиолетовый
            Color.FromRgb(241, 196, 15),  // Жёлтый
        };

        public DragDropAnswer()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateShapes();
        }

        /// <summary>
        /// Создание фигур на Canvas
        /// </summary>
        private void CreateShapes()
        {
            MainCanvas.Children.Clear();
            shapes.Clear();
            initialPositions.Clear();
            originalColors.Clear();

            double canvasWidth = MainCanvas.ActualWidth > 0 ? MainCanvas.ActualWidth : 800;
            double canvasHeight = MainCanvas.ActualHeight > 0 ? MainCanvas.ActualHeight : 400;

            for (int i = 0; i < 5; i++)
            {
                var color = new SolidColorBrush(shapeColors[i]);
                var rect = new Rectangle
                {
                    Width = 80,
                    Height = 80,
                    Fill = color,
                    RadiusX = 10,
                    RadiusY = 10,
                    Cursor = Cursors.Hand,
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };

                // Начальные позиции
                double x = 50 + i * 120;
                double y = canvasHeight / 2 - 40;
                
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                // События мыши
                rect.MouseDown += Shape_MouseDown;
                rect.MouseMove += Shape_MouseMove;
                rect.MouseUp += Shape_MouseUp;

                MainCanvas.Children.Add(rect);
                shapes.Add(rect);
                initialPositions.Add(new Point(x, y));
                originalColors.Add(color);
            }
        }

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            draggedShape = sender as Rectangle;
            if (draggedShape == null) return;

            isDragging = true;
            clickOffset = e.GetPosition(draggedShape);
            draggedShape.CaptureMouse();
            
            // Поднимаем наверх
            Panel.SetZIndex(draggedShape, 100);
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || draggedShape == null) return;

            Point mousePos = e.GetPosition(MainCanvas);
            
            // Новые координаты
            double newX = mousePos.X - clickOffset.X;
            double newY = mousePos.Y - clickOffset.Y;

            // Ограничиваем границами Canvas
            newX = Math.Clamp(newX, 0, MainCanvas.ActualWidth - draggedShape.Width);
            newY = Math.Clamp(newY, 0, MainCanvas.ActualHeight - draggedShape.Height);

            Canvas.SetLeft(draggedShape, newX);
            Canvas.SetTop(draggedShape, newY);

            // Обновляем информацию
            UpdateInfo();
            CheckCollisions();
        }

        private void Shape_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedShape != null)
            {
                draggedShape.ReleaseMouseCapture();
                Panel.SetZIndex(draggedShape, 0);
            }
            
            isDragging = false;
            draggedShape = null;
        }

        /// <summary>
        /// Обновление информации о позиции
        /// </summary>
        private void UpdateInfo()
        {
            if (draggedShape == null) return;

            double x = Canvas.GetLeft(draggedShape);
            double y = Canvas.GetTop(draggedShape);
            
            // Центр фигуры
            double centerX = x + draggedShape.Width / 2;
            double centerY = y + draggedShape.Height / 2;

            CoordsText.Text = $"X: {x:F0}  Y: {y:F0}";

            // Расстояние до центра Canvas
            double canvasCenterX = MainCanvas.ActualWidth / 2;
            double canvasCenterY = MainCanvas.ActualHeight / 2;
            double distance = Math.Sqrt(Math.Pow(centerX - canvasCenterX, 2) + Math.Pow(centerY - canvasCenterY, 2));
            
            DistanceToCenterText.Text = $"{distance:F1} px";
        }

        /// <summary>
        /// Проверка столкновений между фигурами
        /// </summary>
        private void CheckCollisions()
        {
            // Сначала восстанавливаем оригинальные цвета
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Fill = originalColors[i];
            }

            bool hasCollision = false;

            // Проверяем каждую пару
            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = i + 1; j < shapes.Count; j++)
                {
                    if (IsColliding(shapes[i], shapes[j]))
                    {
                        hasCollision = true;
                        // Меняем цвет на белый при столкновении
                        shapes[i].Fill = Brushes.White;
                        shapes[j].Fill = Brushes.White;
                    }
                }
            }

            CollisionText.Text = hasCollision ? "Есть!" : "Нет";
            CollisionText.Foreground = hasCollision ? Brushes.Red : Brushes.LimeGreen;
        }

        /// <summary>
        /// Проверка пересечения двух прямоугольников (AABB)
        /// </summary>
        private bool IsColliding(Rectangle a, Rectangle b)
        {
            double ax = Canvas.GetLeft(a);
            double ay = Canvas.GetTop(a);
            double bx = Canvas.GetLeft(b);
            double by = Canvas.GetTop(b);

            return ax < bx + b.Width &&
                   ax + a.Width > bx &&
                   ay < by + b.Height &&
                   ay + a.Height > by;
        }

        /// <summary>
        /// Сброс позиций
        /// </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < shapes.Count; i++)
            {
                Canvas.SetLeft(shapes[i], initialPositions[i].X);
                Canvas.SetTop(shapes[i], initialPositions[i].Y);
                shapes[i].Fill = originalColors[i];
            }

            CoordsText.Text = "X: —  Y: —";
            DistanceToCenterText.Text = "— px";
            CollisionText.Text = "Нет";
            CollisionText.Foreground = Brushes.LimeGreen;
        }
    }
}

/*
================================================================================
                           КАК ЭТО РАБОТАЕТ
================================================================================

КЛЮЧЕВЫЕ КОНЦЕПЦИИ ГЛАВЫ 4:
---------------------------

1. CANVAS - АБСОЛЮТНОЕ ПОЗИЦИОНИРОВАНИЕ
   - Canvas.SetLeft(element, x) - установить X-координату
   - Canvas.SetTop(element, y) - установить Y-координату
   - Canvas.GetLeft(element) - получить X-координату
   - Canvas.GetTop(element) - получить Y-координату

2. DRAG-AND-DROP АЛГОРИТМ
   MouseDown:
   - Запоминаем перетаскиваемый объект
   - Запоминаем смещение клика от угла объекта
   - CaptureMouse() - захватываем мышь
   
   MouseMove:
   - Получаем позицию мыши относительно Canvas
   - Вычисляем новую позицию = мышь - смещение
   - Ограничиваем границами Canvas (Math.Clamp)
   - Устанавливаем новую позицию
   
   MouseUp:
   - ReleaseMouseCapture() - отпускаем мышь
   - Сбрасываем состояние

3. CAPTUREMOUSE / RELEASEMOUSECAPTURE
   - CaptureMouse() - все события мыши идут к этому элементу
   - Даже если мышь выходит за пределы элемента
   - ReleaseMouseCapture() - возвращает нормальное поведение

4. РАСЧЕТ РАССТОЯНИЯ (ТЕОРЕМА ПИФАГОРА)
   distance = sqrt((x2-x1)^2 + (y2-y1)^2)
   
   В C#:
   double dist = Math.Sqrt(Math.Pow(x2-x1, 2) + Math.Pow(y2-y1, 2));

5. AABB COLLISION DETECTION (столкновение прямоугольников)
   Два прямоугольника пересекаются если:
   - A.Left < B.Right AND A.Right > B.Left
   - A.Top < B.Bottom AND A.Bottom > B.Top
   
   В коде:
   ax < bx + b.Width && ax + a.Width > bx &&
   ay < by + b.Height && ay + a.Height > by

6. PANEL.SETZINDEX
   - Устанавливает порядок отрисовки
   - Больше значение = выше (поверх других)
   - Panel.SetZIndex(element, 100) - поднять наверх

================================================================================
*/
