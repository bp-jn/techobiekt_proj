using MySqlConnector;
using System;
using System.Windows;

namespace DbModeler
{
    public partial class MySqlExportDialog : Window
    {
        private readonly string _sqlScript;
        public MySqlExportDialog(string sqlScript)
        {
            InitializeComponent();
            _sqlScript = sqlScript;
        }
        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;
            TxtStatus.Text = "Łączenie...";
            try
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = TxtHost.Text,
                    Port = uint.Parse(TxtPort.Text),
                    UserID = TxtUser.Text,
                    Password = PwdPassword.Password,
                    AllowUserVariables = true
                };
                await using var conn = new MySqlConnection(builder.ConnectionString);
                await conn.OpenAsync();

                if (ChkCreateDb.IsChecked == true)
                {
                    string db = TxtDatabase.Text;
                    await using var cmdDb = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{db}`; USE `{db}`;", conn);
                    await cmdDb.ExecuteNonQueryAsync();
                }
                var statements = _sqlScript.Split(new[] { ";\n", ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                int count = 0;

                foreach (var stmt in statements)
                {
                    if (string.IsNullOrWhiteSpace(stmt)) continue;

                    await using var cmd = new MySqlCommand(stmt, conn);
                    await cmd.ExecuteNonQueryAsync();
                    count++;
                }

                TxtStatus.Text = $"Sukces! Wykonano {count} zapytań.";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Błąd: {ex.Message}";
            }
            finally
            {
                BtnConnect.IsEnabled = true;
            }
        }
    }
}