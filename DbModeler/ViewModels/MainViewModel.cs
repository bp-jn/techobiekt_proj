using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbModeler.Models;

namespace DbModeler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
       
        [ObservableProperty]
        private DatabaseProject _project = new DatabaseProject();

     
        [ObservableProperty]
        private Table? _selectedTable;

        public MainViewModel()
        {
            
            AddTable();
        }

        [RelayCommand]
        private void AddTable()
        {
            var newTable = new Table { Name = $"NowaTabela_{Project.Tables.Count + 1}" };
            Project.Tables.Add(newTable);

            
            SelectedTable = newTable;
        }

        [RelayCommand]
        private void RemoveTable(Table table)
        {
            if (table != null)
            {
                Project.Tables.Remove(table);
            }

        }

        [RelayCommand]
        private void AddColumn()
        {
            if (SelectedTable != null)
            {
                var newColumn = new Column { Name = $"Kolumna_{SelectedTable.Columns.Count + 1}" };
                SelectedTable.Columns.Add(newColumn);
            }
        }

    }
}