using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MigrationTool.Shared;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataMigrationPage : Page
    {
        public ObservableCollection<TableMetadata> TableList { get; set; } = new();
        public DataMigrationPage()
        {
            
            this.InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            // Example of adding data that the grid will display
            TableList.Add(new TableMetadata { Name = "Users", Schema = "dbo" });
        }

        //private async Task LoadTablesIntoGridAsync()
        //{
        //    if (ConnectionsService.SourceBuilder == null) return;

        //    TableList.Clear();
        //    try
        //    {
        //        using var conn = new SqlConnection(ConnectionsService.SourceBuilder.ConnectionString);
        //        await conn.OpenAsync();

        //        string query = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
        //        using var cmd = new SqlCommand(query, conn);
        //        using var reader = await cmd.ExecuteReaderAsync();

        //        while (await reader.ReadAsync())
        //        {
        //            TableList.Add(new TableMetadata
        //            {
        //                Schema = reader.GetString(0),
        //                Name = reader.GetString(1)
        //            });
        //        }

        //        TablesGrid.ItemsSource = TableList;
        //    }
        //    catch (Exception ex)
        //    {
        //        MigrationStatusInfoBar.Message = $"Error loading tables: {ex.Message}";
        //        MigrationStatusInfoBar.IsOpen = true;
        //    }
        //}

        //private void RefreshTables_Click(object sender, RoutedEventArgs e) => _ = LoadTablesIntoGridAsync();

        //private void TableSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        //{
        //    var searchText = sender.Text.ToLower();
        //    if (string.IsNullOrWhiteSpace(searchText))
        //    {
        //        TablesGrid.ItemsSource = TableList;
        //    }
        //    else
        //    {
        //        TablesGrid.ItemsSource = new ObservableCollection<TableMetadata>(
        //            TableList.Where(t => t.Name.ToLower().Contains(searchText) ||
        //                                 t.Schema.ToLower().Contains(searchText))
        //        );
        //    }
        //}
        //private async Task RunBulkCopyAsync(string fullTableName)
        //{
        //    using var sourceConn = new SqlConnection(ConnectionsService.SourceBuilder.ConnectionString);
        //    using var destConn = new SqlConnection(ConnectionsService.DestBuilder.ConnectionString);

        //    await sourceConn.OpenAsync();
        //    await destConn.OpenAsync();

        //    //set row limit
        //    int limit = (int)RowLimitBox.Value;
        //    string topClause = limit > 0 ? $"TOP ({limit})" : "";

        //    //Where condition
        //    string filter = WhereConditionBox.Text.Trim();
        //    string whereClause = !string.IsNullOrEmpty(filter) ? $"WHERE {filter}" : "";

        //    // Prepareing query
        //    string countQuery = limit > 0
        //        ? $"SELECT TOP ({limit}) * FROM {fullTableName} {whereClause}"
        //        : $"SELECT COUNT(*) FROM {fullTableName} {whereClause}";

        //    // Get total count to set the Progress Bar maximum
        //    var countCmd = new SqlCommand(countQuery, sourceConn);
        //    DataTable table = new DataTable();
        //    if (limit > 0)
        //    {
        //        var r = await countCmd.ExecuteReaderAsync();
        //        table.Load(r);
        //    }



        //    int totalRows = limit>0 ? table.Rows.Count :(int)await countCmd.ExecuteScalarAsync();

        //    // Update UI Thread: Show bar and set Max
        //    DispatcherQueue.TryEnqueue(() => {
        //        MigrationProgress.Maximum = totalRows;
        //        MigrationProgress.Value = 0;
        //        MigrationProgress.Visibility = Visibility.Visible;
        //        ProgressStatusText.Text = $"Migrating {fullTableName}...";
        //    });

        //    //Setup Reader
        //    string selectQuery = $"SELECT {topClause} * FROM {fullTableName} {whereClause}";
        //    var cmd = new SqlCommand(selectQuery, sourceConn);
        //    using var reader = await cmd.ExecuteReaderAsync();

        //    // Setup Bulk Copy with Events
        //    using var bulkCopy = new SqlBulkCopy(destConn,SqlBulkCopyOptions.KeepIdentity,null);
        //    bulkCopy.DestinationTableName = fullTableName;

        //    // Notify us every 100 rows
        //    bulkCopy.NotifyAfter = 1;

        //    // The Event Handler
        //    bulkCopy.SqlRowsCopied += (s, e) => {
        //        // We must jump back to the UI thread to update the bar
        //        DispatcherQueue.TryEnqueue(() => {
        //            MigrationProgress.Value = e.RowsCopied;
        //            ProgressStatusText.Text = $"Copied {e.RowsCopied} of {totalRows} rows...";
        //        });
        //    };

        //    //Start the move
        //    await bulkCopy.WriteToServerAsync(reader);

        //    DispatcherQueue.TryEnqueue(() => {
        //        ProgressStatusText.Text = "Done!";
        //    });
        //}
        //private async void StartMigration_Click(object sender, RoutedEventArgs e)
        //{
        //    // 1. Get selected tables from the DataGrid
        //    var selectedTables = TablesGrid.SelectedItems.Cast<TableMetadata>().ToList();

        //    if (!selectedTables.Any())
        //    {
        //        MigrationStatusInfoBar.Message = "Please select at least one table from the list.";
        //        MigrationStatusInfoBar.Severity = InfoBarSeverity.Warning;
        //        MigrationStatusInfoBar.IsOpen = true;
        //        return;
        //    }

        //    // 2. UI Feedback
        //    MigrationStatusInfoBar.IsOpen = false;
        //    // Assuming you added a ProgressBar named 'MigrationProgress' in XAML
        //     MigrationProgress.Visibility = Visibility.Visible; 

        //    try
        //    {
        //        foreach (var table in selectedTables)
        //        {
        //            string tableName = $"[{table.Schema}].[{table.Name}]";
        //            await RunBulkCopyAsync(tableName);
        //        }

        //        MigrationStatusInfoBar.Message = $"Successfully migrated {selectedTables.Count} table(s)!";
        //        MigrationStatusInfoBar.Severity = InfoBarSeverity.Success;
        //        MigrationStatusInfoBar.IsOpen = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MigrationStatusInfoBar.Message = $"Migration Error: {ex.Message}";
        //        MigrationStatusInfoBar.Severity = InfoBarSeverity.Error;
        //        MigrationStatusInfoBar.IsOpen = true;
        //    }
        //}
    }
}
