using DbModeler.Models;
using System.Windows;
using System.Windows.Input;
using DbModeler.ViewModels;

namespace DbModeler
{
    public partial class MainWindow : Window
    {
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private Table? _draggedTable;
        private FrameworkElement? _draggedElement;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TableBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Table table)
            {
                _isDragging = true;
                _draggedTable = table;
                _draggedElement = element;
                _lastMousePosition = e.GetPosition(this);
                element.CaptureMouse();
            }
        }

        private void TableBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedTable != null && _draggedElement != null)
            {
                Point currentMousePosition = e.GetPosition(this);
                double deltaX = currentMousePosition.X - _lastMousePosition.X;
                double deltaY = currentMousePosition.Y - _lastMousePosition.Y;

                _draggedTable.CanvasX += deltaX;
                _draggedTable.CanvasY += deltaY;
                _lastMousePosition = currentMousePosition;

                if (DataContext is MainViewModel vm)
                {
                    vm.UpdateAllLines();
                }
            }
        }

        private void TableBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _draggedElement != null)
            {
                _isDragging = false;
                _draggedElement.ReleaseMouseCapture();
                _draggedTable = null;
                _draggedElement = null;
            }
        }
    }
}