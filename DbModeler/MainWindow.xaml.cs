using DbModeler.Models;
using System.Windows;
using System.Windows.Input;
using DbModeler.ViewModels;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Reflection;
using System.Xml;

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
            LoadCustomSyntax();
        }
        private void LoadCustomSyntax()
        {
            string resourceName = "DbModeler.SyntaxSql.xshd";
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (XmlTextReader reader = new XmlTextReader(stream))
                    {
                        SqlEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
                else
                {
                    MessageBox.Show("Nie znaleziono pliku kolorowania składni.");
                }
            }
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
                if (DataContext is MainViewModel vm)
                {
                    vm.SelectedTable = table;
                }
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
        private void TableBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Table table)
            {
                table.Width = e.NewSize.Width;
                table.Height = e.NewSize.Height;

                if (DataContext is MainViewModel vm)
                {
                    vm.UpdateAllLines();
                }
            }
        }
        private void ExportToMySQL_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.GenerateSqlCommand.Execute(null);

                string sql = vm.SqlDocument.Text;

                if (string.IsNullOrWhiteSpace(sql))
                {
                    MessageBox.Show("Najpierw dodaj tabele do projektu.", "Brak danych",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new MySqlExportDialog(sql) { Owner = this };
                dialog.ShowDialog();
            }
        }

    }
}