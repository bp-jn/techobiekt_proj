using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbModeler.Models;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

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

        [ObservableProperty]
        private TextDocument _sqlDocument = new TextDocument();

        public Array AvailableDataTypes => Enum.GetValues(typeof(SqlDataType));
        public Array AvailableRelationTypes => Enum.GetValues(typeof(RelationshipType));

        public MainViewModel()
        {
            AddTable();
        }

        [RelayCommand]
        private void AddTable()
        {
            int counter = Project.Tables.Count + 1;
            string newName = $"NowaTabela_{counter}";

            while (Project.Tables.Any(t => t.Name == newName))
            {
                counter++;
                newName = $"NowaTabela_{counter}";
            }

            var newTable = new Table { Name = newName, CanvasX = 50, CanvasY = 50 };

            var primaryKeyColumn = new Column
            {
                Name = "Id",
                DataType = SqlDataType.Int, 
                IsPrimaryKey = true,
                IsNotNull = true
            };
            newTable.Columns.Add(primaryKeyColumn);

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

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        [RelayCommand]
        private void ExportToJson()
        {
            var dialog = new SaveFileDialog { Filter = "Pliki projektu JSON (*.json)|*.json", FileName = "MojProjekt.json" };
            if (dialog.ShowDialog() == true)
            {
                string json = JsonSerializer.Serialize(Project, _jsonOptions);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        [RelayCommand]
        private void ImportFromJson()
        {
            var dialog = new OpenFileDialog { Filter = "Pliki projektu JSON (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var loadedProject = JsonSerializer.Deserialize<DatabaseProject>(json, _jsonOptions);
                    if (loadedProject != null)
                    {
                        Project = loadedProject;
                        UpdateAllLines(); 
                    }
                }
                catch (Exception ex)
                {
                    
                    System.Windows.MessageBox.Show($"Błąd podczas odczytu: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }


        [RelayCommand]
        private void GenerateSql()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- ==========================================");
            sb.AppendLine($"-- Skrypt bazy danych: {Project.ProjectName}");
            sb.AppendLine($"-- Wygenerowano: {DateTime.Now}");
            sb.AppendLine("-- ==========================================\n");

            foreach (var table in Project.Tables)
            {
                sb.AppendLine($"CREATE TABLE {table.Name} (");
                var columnsDefs = table.Columns.Select(c =>
                {
                    string def = $"    {c.Name} {c.DataType.ToString().ToUpper()}";
                    if (c.IsPrimaryKey) def += " PRIMARY KEY";
                    else if (c.IsNotNull) def += " NOT NULL";
                    return def;
                });

                sb.AppendLine(string.Join(",\n", columnsDefs));
                sb.AppendLine(");\n");
            }

            sb.AppendLine("-- ==========================================");
            sb.AppendLine("-- RELACJE (Klucze Obce)");
            sb.AppendLine("-- ==========================================\n");

            foreach (var rel in Project.Relationships)
            {
                if (rel.SourceTable != null && rel.TargetTable != null)
                {
                    var pkCol = rel.SourceTable.Columns.FirstOrDefault(c => c.IsPrimaryKey);
                    string pkName = pkCol != null ? pkCol.Name : "ID";

                    if (rel.Type == RelationshipType.OneToMany)
                    {
                        sb.AppendLine($"ALTER TABLE {rel.TargetTable.Name}");
                        sb.AppendLine($"ADD CONSTRAINT FK_{rel.TargetTable.Name}_{rel.SourceTable.Name}");
                        sb.AppendLine($"FOREIGN KEY ({rel.SourceTable.Name}Id) REFERENCES {rel.SourceTable.Name}({pkName});\n");
                    }
                }
            }
            SqlDocument.Text = sb.ToString();
        }
    }
}