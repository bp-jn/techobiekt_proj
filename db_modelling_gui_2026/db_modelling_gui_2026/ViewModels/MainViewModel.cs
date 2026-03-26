using db_modelling_gui_2026.Helpers;
using db_modelling_gui_2026.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using TableModel = db_modelling_gui_2026.Models.Table;

namespace db_modelling_gui_2026.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TableModel> Tables { get; set; }

        private TableModel _selectedTable;
        public TableModel SelectedTable
        {
            get
            {
                return _selectedTable;
            }
            set
            {
                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }
        public RelayCommand AddTableCommand { get; set; }
        public RelayCommand RemoveTableCommand { get; set; }

        public MainViewModel()
        {
            Tables = new ObservableCollection<TableModel>();
            AddTableCommand = new RelayCommand(AddTable);
            RemoveTableCommand = new RelayCommand(RemoveTable);
        }
        private void AddTable()
        {
            Tables.Add(new TableModel { Name = "NowaTabela" });
        }
        private void RemoveTable()
        {
            if (SelectedTable != null)
            {
                Tables.Remove(SelectedTable);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
    }
}
