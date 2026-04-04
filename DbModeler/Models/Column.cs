using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DbModeler.Models
{
    public partial class Column : ObservableObject
    {
        [ObservableProperty]
        private string _name = "NowaKolumna";

        [ObservableProperty]
        private SqlDataType _dataType = SqlDataType.Int;
        
        [ObservableProperty]
        private bool _isPrimaryKey = false;
        
        [ObservableProperty]
        private bool _isNotNull = false;
    }
}
