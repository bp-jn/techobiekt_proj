using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DbModeler.Models
{
    public partial class Relationship : ObservableObject
    {
        [ObservableProperty]
        private Table _sourceTable;

        [ObservableProperty]
        private Table _targetTable;

        [ObservableProperty]
        private RelationshipType _type = RelationshipType.OneToMany;
    }
}
