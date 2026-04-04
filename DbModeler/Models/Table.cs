using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModeler.Models
{
    public partial class Table : ObservableObject
    {
        [ObservableProperty]
        private string _name = "NowaTabela";

        public ObservableCollection<Column> Columns { get; set; } = new ObservableCollection<Column>();
        [ObservableProperty]
        private double _canvasX = 0;

        [ObservableProperty]
        private double _canvasY = 0;
    }
}
