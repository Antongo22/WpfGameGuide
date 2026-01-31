using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp_Book.Chapters.Chapter4_Layout.Demo
{
    public partial class LayoutPage : Page
    {
        private bool isDragging = false;
        private Point clickOffset;
        private UIElement? draggedElement = null;

        public LayoutPage()
        {
            InitializeComponent();
            UpdateDistance();
        }

        private void Box_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            if (element == null) return;
            
            draggedElement = element;
            isDragging = true;
            clickOffset = e.GetPosition(element);
            element.CaptureMouse();
        }

        private void Box_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || draggedElement == null) return;
            
            Point mousePos = e.GetPosition(DemoCanvas);
            Canvas.SetLeft(draggedElement, mousePos.X - clickOffset.X);
            Canvas.SetTop(draggedElement, mousePos.Y - clickOffset.Y);
            UpdateDistance();
        }

        private void Box_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedElement != null)
                draggedElement.ReleaseMouseCapture();
            isDragging = false;
            draggedElement = null;
        }

        private void UpdateDistance()
        {
            double x1 = Canvas.GetLeft(BlueBox) + 30;
            double y1 = Canvas.GetTop(BlueBox) + 30;
            double x2 = Canvas.GetLeft(RedBox) + 30;
            double y2 = Canvas.GetTop(RedBox) + 30;
            double dist = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            DistanceText.Text = $"Расстояние: {dist:F1} px";
        }
    }
}
