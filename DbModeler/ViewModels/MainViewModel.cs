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

        [ObservableProperty] private Table? _selectedSourceTable;
        [ObservableProperty] private Table? _selectedTargetTable;
        [ObservableProperty] private RelationshipType _selectedRelType = RelationshipType.OneToMany;

        public Array AvailableDataTypes => Enum.GetValues(typeof(SqlDataType));
        public Array AvailableRelationTypes => Enum.GetValues(typeof(RelationshipType));

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
        private void RemoveTable(Table? table)
        {
            if (table != null)
            {
                var relationsToRemove = Project.Relationships.Where(r => r.SourceTable == table || r.TargetTable == table).ToList();
                foreach (var rel in relationsToRemove)
                {
                    Project.Relationships.Remove(rel);
                }

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
        [RelayCommand]
        private void AddRelationship()
        {
            if (SelectedSourceTable != null && SelectedTargetTable != null && SelectedSourceTable != SelectedTargetTable)
            {
                var rel = new Relationship
                {
                    SourceTable = SelectedSourceTable,
                    TargetTable = SelectedTargetTable,
                    Type = SelectedRelType
                };
                Project.Relationships.Add(rel);
                UpdateAllLines();
            }
        }

        public void UpdateAllLines()
        {
            foreach (var rel in Project.Relationships)
            {
                if (rel.SourceTable != null && rel.TargetTable != null)
                {
                    rel.StartX = rel.SourceTable.CanvasX + 75;
                    rel.StartY = rel.SourceTable.CanvasY + 15;

                    rel.EndX = rel.TargetTable.CanvasX + 75;
                    rel.EndY = rel.TargetTable.CanvasY + 15;
                }
            }

        }
    }
}