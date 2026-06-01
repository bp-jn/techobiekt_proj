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
        partial void OnSelectedTableChanged(Table? oldValue, Table? newValue)
        {
            if (oldValue != null) oldValue.IsSelected = false;
            if (newValue != null) newValue.IsSelected = true;

            if (SelectedSourceTable != null && newValue != null && SelectedSourceTable != newValue)
            {
                SelectedTargetTable = newValue;

                var rel = new Relationship
                {
                    SourceTable = SelectedSourceTable,
                    TargetTable = SelectedTargetTable,
                    Type = SelectedRelType
                };
                Project.Relationships.Add(rel);
                UpdateAllLines();

                SelectedSourceTable.IsConnectingWaiting = false;
                SelectedSourceTable = null;
                SelectedTargetTable = null;
            }
        }
        [RelayCommand]
        private void ChangeRelType(object parameter)
        {

            if (parameter is string typeStr && Enum.TryParse(typeStr, out RelationshipType newType))
            {
            }
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
        private void AddColumnToSpecificTable(Table? table)
        {
            if (table != null)
            {
                table.Columns.Add(new Column { Name = $"Kolumna_{table.Columns.Count + 1}" });
            }
        }

        [RelayCommand]
        private void RemoveColumn(Column? column)
        {
            if (column != null)
            {
                foreach (var table in Project.Tables)
                {
                    if (table.Columns.Contains(column))
                    {
                        table.Columns.Remove(column);
                        break;
                    }
                }
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

        [RelayCommand]
        private void RemoveRelationship(Relationship? rel)
        {
            if (rel != null)
            {
                Project.Relationships.Remove(rel);
            }
        }

        [RelayCommand]
        private void SetAsSource(Table? table)
        {
            if (table != null)
            {

                if (SelectedSourceTable != null) SelectedSourceTable.IsConnectingWaiting = false;

                SelectedSourceTable = table;
                table.IsConnectingWaiting = true;
            }
        }

        [RelayCommand]
        private void ConnectAsTarget(Table? table)
        {
            if (SelectedSourceTable != null && table != null && SelectedSourceTable != table)
            {
                SelectedTargetTable = table;

                var rel = new Relationship
                {
                    SourceTable = SelectedSourceTable,
                    TargetTable = SelectedTargetTable,
                    Type = SelectedRelType
                };

                Project.Relationships.Add(rel);
                UpdateAllLines();

                SelectedSourceTable = null;
            }
        }

        public void UpdateAllLines()
        {
            foreach (var rel in Project.Relationships)
            {
                if (rel.SourceTable != null && rel.TargetTable != null)
                {
                    rel.StartX = rel.SourceTable.CanvasX + 100;
                    rel.StartY = rel.SourceTable.CanvasY + 20;
                    rel.EndX = rel.TargetTable.CanvasX + 100;
                    rel.EndY = rel.TargetTable.CanvasY + 20;

                    rel.MidX = (rel.StartX + rel.EndX) / 2;
                    rel.MidY = (rel.StartY + rel.EndY) / 2;
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

            var stiRels = Project.Relationships.Where(r => r.Type == RelationshipType.InheritanceSingleTable).ToList();
            var ctiRels = Project.Relationships.Where(r => r.Type == RelationshipType.InheritanceClassTable).ToList();
            var tpcRels = Project.Relationships.Where(r => r.Type == RelationshipType.InheritanceConcreteTable).ToList(); 

            var stiChildren = stiRels.Select(r => r.TargetTable).Distinct().ToList();

            var stiAndCtiRels = stiRels.Concat(ctiRels).ToList();
            var scChildren = stiAndCtiRels.Select(r => r.TargetTable).Distinct().ToList();
            var scParents = stiAndCtiRels.Select(r => r.SourceTable).Distinct().ToList();
            var scRootTables = scParents.Where(p => !scChildren.Contains(p)).ToList();

            List<Column> GetTpcInheritedColumns(Table currentTable)
            {
                var inherited = new List<Column>();
                var parentRel = tpcRels.FirstOrDefault(r => r.TargetTable == currentTable);
                if (parentRel != null && parentRel.SourceTable != null)
                {

                    inherited.AddRange(parentRel.SourceTable.Columns.Where(c => !c.IsPrimaryKey));
                    inherited.AddRange(GetTpcInheritedColumns(parentRel.SourceTable));
                }
                return inherited;
            }


            Table GetStiRootTable(Table t)
            {
                var parentRel = stiRels.FirstOrDefault(r => r.TargetTable == t);
                return parentRel != null && parentRel.SourceTable != null ? GetStiRootTable(parentRel.SourceTable) : t;
            }


            var tablesToCreate = Project.Tables.Where(t => !stiChildren.Contains(t)).ToList();

            foreach (var table in tablesToCreate)
            {
                sb.AppendLine($"CREATE TABLE {table.Name} (");

                var allColumns = new List<Column>(table.Columns);

 
                if (scRootTables.Contains(table))
                {
                    allColumns.Insert(1, new Column { Name = "type", DataType = SqlDataType.Varchar, Length = "31", IsNotNull = true });
                }

                var currentStiChildren = stiRels.Where(r => r.SourceTable == table).Select(r => r.TargetTable).ToList();
                if (currentStiChildren.Any())
                {
                    var queue = new Queue<Table>(currentStiChildren);
                    while (queue.Count > 0)
                    {
                        var child = queue.Dequeue();
                        foreach (var col in child.Columns.Where(c => !c.IsPrimaryKey))
                        {
                            allColumns.Add(new Column { Name = col.Name, DataType = col.DataType, Length = col.Length, IsNotNull = false });
                        }
                        var nextChildren = stiRels.Where(r => r.SourceTable == child).Select(r => r.TargetTable).ToList();
                        foreach (var nc in nextChildren) queue.Enqueue(nc);
                    }
                }


                var inheritedCols = GetTpcInheritedColumns(table);
                foreach (var col in inheritedCols)
                {

                    allColumns.Add(new Column { Name = col.Name, DataType = col.DataType, Length = col.Length, IsNotNull = col.IsNotNull });
                }

                var columnsDefs = allColumns.Select(c =>
                {
                    string typeStr = c.DataType.ToString().ToUpper();
                    if (!string.IsNullOrWhiteSpace(c.Length)) typeStr += $"({c.Length})";

                    string def = $"    {c.Name} {typeStr}";
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


            var standardRels = Project.Relationships.Where(r => r.Type == RelationshipType.OneToMany || r.Type == RelationshipType.OneToOne);
            foreach (var rel in standardRels)
            {
                if (rel.SourceTable != null && rel.TargetTable != null)
                {
                    var actualSource = GetStiRootTable(rel.SourceTable);
                    var actualTarget = GetStiRootTable(rel.TargetTable);

                    var pkCol = actualSource.Columns.FirstOrDefault(c => c.IsPrimaryKey);
                    string pkName = pkCol != null ? pkCol.Name : "ID";

                    sb.AppendLine($"ALTER TABLE {actualTarget.Name}");
                    sb.AppendLine($"ADD CONSTRAINT FK_{actualTarget.Name}_{actualSource.Name}");
                    sb.AppendLine($"FOREIGN KEY ({actualSource.Name}Id) REFERENCES {actualSource.Name}({pkName});\n");
                }
            }

            foreach (var rel in ctiRels)
            {
                if (rel.SourceTable != null && rel.TargetTable != null)
                {
                    var parentPk = rel.SourceTable.Columns.FirstOrDefault(c => c.IsPrimaryKey);
                    var childPk = rel.TargetTable.Columns.FirstOrDefault(c => c.IsPrimaryKey);

                    string parentPkName = parentPk != null ? parentPk.Name : "ID";
                    string childPkName = childPk != null ? childPk.Name : "ID";

                    sb.AppendLine($"ALTER TABLE {rel.TargetTable.Name}");
                    sb.AppendLine($"ADD CONSTRAINT FK_CTI_{rel.TargetTable.Name}_{rel.SourceTable.Name}");
                    sb.AppendLine($"FOREIGN KEY ({childPkName}) REFERENCES {rel.SourceTable.Name}({parentPkName});\n");
                }
            }

            SqlDocument.Text = sb.ToString();
        }
        [RelayCommand]
        private void AutoArrangeTables()
        {
            if (Project?.Tables == null || Project.Tables.Count == 0) return;

            double startX = 50;
            double startY = 50;
            double spacingX = 40;
            double spacingY = 40;
            int maxColumns = 4;

            double currentX = startX;
            double currentY = startY;
            double maxRowHeight = 0;

            for (int i = 0; i < Project.Tables.Count; i++)
            {
                var table = Project.Tables[i];


                double actualWidth = table.Width > 0 ? table.Width : 220;
                double actualHeight = table.Height > 0 ? table.Height : 300;

                if (i > 0 && i % maxColumns == 0)
                {
                    currentX = startX;
                    currentY += maxRowHeight + spacingY;
                    maxRowHeight = 0;
                }

                table.CanvasX = currentX;
                table.CanvasY = currentY;


                currentX += actualWidth + spacingX;

                if (actualHeight > maxRowHeight)
                {
                    maxRowHeight = actualHeight;
                }
            }

            UpdateAllLines();
        }
    }
}