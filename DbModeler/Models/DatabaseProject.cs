using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DbModeler.Models
{
    public partial class DatabaseProject : ObservableObject
    {
        [ObservableProperty]
        private string _projectName = "Baza danych";

        public ObservableCollection<Table> Tables { get; set; } = new ObservableCollection<Table>();

        public ObservableCollection<Relationship> Relationships { get; set; } = new ObservableCollection<Relationship>();
    }
}