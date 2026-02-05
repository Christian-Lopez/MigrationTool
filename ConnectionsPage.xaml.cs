using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using MigrationTool.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionsPage : Page
    {
        public ConnectionsPage()
        {
            InitializeComponent();
            // Wire up the loaded event
            this.Loaded += (s, e) =>
            {
                LoadSavedSettings();
            };
        }
        private async void TestSourceConnection_Click(object sender, RoutedEventArgs e)
        {

            var dataSource = SourceServer.Text;

            // For LocalDB, resolve to actual pipe name for reliability
            if (dataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
            {
                var pipeName = await GetLocalDbPipeNameAsync();
                if (!string.IsNullOrEmpty(pipeName))
                {
                    dataSource = pipeName;
                    System.Diagnostics.Debug.WriteLine($"Resolved LocalDB to pipe: {pipeName}");
                }
            }

            // Build the string dynamically
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = SourceDatabase.Text,
                IntegratedSecurity = SourceUseWindowsAuth.IsChecked ?? false,
                Encrypt = true,
                TrustServerCertificate = true,
                ConnectTimeout = 30,           // ADD THIS
                Pooling = true,                // ADD THIS
                MinPoolSize = 0,               // ADD THIS
                MaxPoolSize = 100              // ADD THIS
            };
            // Only add credentials if NOT using Windows Auth
            if (!(SourceUseWindowsAuth.IsChecked ?? false))
            {
                builder.UserID = SourceUser.Text;
                builder.Password = SourcePassword.Password;
            }
            SaveBasicSettings(true, SourceServer.Text, SourceDatabase.Text, SourceUser.Text, SourcePassword.Password, SourceUseWindowsAuth.IsChecked ?? false);
            if (await OpenConnectionAsync(builder.ConnectionString))
            {
                // SAVE TO DESTINATION SLOT
                ConnectionsService.SourceBuilder = builder;
                ShowStatus("Source Saved!", InfoBarSeverity.Success);
            }
        }
        private async void TestDestinationConnection_Click(object sender, RoutedEventArgs e)
        {

            var dataSource = SourceServer.Text;

            // For LocalDB, resolve to actual pipe name for reliability
            if (dataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
            {
                var pipeName = await GetLocalDbPipeNameAsync();
                if (!string.IsNullOrEmpty(pipeName))
                {
                    dataSource = pipeName;
                    System.Diagnostics.Debug.WriteLine($"Resolved LocalDB to pipe: {pipeName}");
                }
            }

            // Build the string dynamically
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = DestinationServer.Text,
                InitialCatalog = DestinationDatabase.Text,
                IntegratedSecurity = DestinationUseWindowsAuth.IsChecked ?? false,
                Encrypt = true,
                TrustServerCertificate = true,
                 ConnectTimeout = 30,           // ADD THIS
                Pooling = true,                // ADD THIS
                MinPoolSize = 0,               // ADD THIS
                MaxPoolSize = 100              // ADD THIS
            };
            // Only add credentials if NOT using Windows Auth
            if (!(DestinationUseWindowsAuth.IsChecked ?? false))
            {
                builder.UserID = DestinationUser.Text;
                builder.Password = DestinationPassword.Password;
            }
            SaveBasicSettings(false, DestinationServer.Text, DestinationDatabase.Text, DestinationUser.Text, DestinationPassword.Password, DestinationUseWindowsAuth.IsChecked ?? false);
            if (await OpenConnectionAsync(builder.ConnectionString))
            {
                // SAVE TO DESTINATION SLOT
                ConnectionsService.DestBuilder = builder;
                ShowStatus("Destination Saved!", InfoBarSeverity.Success);
            }
        }
        private async Task<string> GetLocalDbPipeNameAsync()
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "info MSSQLLocalDB",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    // Use regex to extract the pipe name
                    var match = System.Text.RegularExpressions.Regex.Match(
                        output,
                        @"Instance pipe name:\s*(np:[^\r\n]+)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    if (match.Success)
                    {
                        var pipeName = match.Groups[1].Value.Trim();
                        System.Diagnostics.Debug.WriteLine($"Found pipe name: {pipeName}");
                        return pipeName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting LocalDB pipe name: {ex.Message}");
            }

            return null;
        }
        private async Task<bool> OpenConnectionAsync(string connectionString)
        {
            try
            {
                // Clear the connection pool before attempting to connect
                // This is CRITICAL for LocalDB instances
                SqlConnection.ClearAllPools();


                // For LocalDB: Ensure instance is started
                if (connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
                {
                    await EnsureLocalDbStartedAsync();
                }

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Optionally verify the connection with a simple query
                //using var command = connection.CreateCommand();
                //command.CommandText = "SELECT 1";
                //await command.ExecuteScalarAsync();

                // Verify with a simple query
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT DB_NAME()";
                var result = await command.ExecuteScalarAsync();

                System.Diagnostics.Debug.WriteLine($"Successfully connected to: {result}");

                return true;
            }
            catch (SqlException ex)
            {
                ShowStatus($"SQL Error: {ex.Number} - {ex.Message}", InfoBarSeverity.Error);
                System.Diagnostics.Debug.WriteLine($"SqlException Details: {ex}");
                return false;
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", InfoBarSeverity.Error);
                System.Diagnostics.Debug.WriteLine($"Exception Details: {ex}");
                return false;
            }
        }
        private async Task EnsureLocalDbStartedAsync()
        {
            try
            {
                // Start LocalDB instance
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "start MSSQLLocalDB",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    // Give LocalDB a moment to fully initialize
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LocalDB start warning: {ex.Message}");
                // Don't fail if we can't start it - it might already be running
            }
        }
        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.Title = severity == InfoBarSeverity.Success ? "Success" : "Error";
            StatusInfoBar.IsOpen = true;
        }
        private void Auth_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (SourceGrid == null) return;
            if (DestinationGrid == null) return;

            // Disable inputs if Windows Auth is checked
            bool isWindowsAuthS = SourceUseWindowsAuth.IsChecked ?? false;
            SourceGrid.Opacity = isWindowsAuthS ? 0.5 : 1.0;
            SourceUser.IsEnabled = !isWindowsAuthS;
            SourcePassword.IsEnabled = !isWindowsAuthS;

            bool isWindowsAuthD = DestinationUseWindowsAuth.IsChecked ?? false;
            DestinationGrid.Opacity = isWindowsAuthD ? 0.5 : 1.0;
            DestinationUser.IsEnabled = !isWindowsAuthD;
            DestinationPassword.IsEnabled = !isWindowsAuthD;
        }
        private void SaveBasicSettings(bool isSource, string server, string database, string user, string password, bool isWindowsAuth)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            
            string prefix = isSource ? "Source" : "Dest";
            
            localSettings.Values[$"{prefix}Server"] = server;
            localSettings.Values[$"{prefix}Database"] = database;
            localSettings.Values[$"{prefix}User"] = user;
            localSettings.Values[$"{prefix}UseWindowsAuth"] = isWindowsAuth;

            // 2. Save Password securely to Credential Locker
            if (!isWindowsAuth && !string.IsNullOrEmpty(password))
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                // Unique resource name including the database
                string resourceKey = $"SQLDataBridge_{prefix}_{server}_{database}";

                var credential = new Windows.Security.Credentials.PasswordCredential(
                    resourceKey, user, password);

                vault.Add(credential);
            }
                       

        }
        private void LoadSavedSettings()
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                var vault = new PasswordVault();

                // 1. Load basic text fields
                if (localSettings.Values.ContainsKey("SourceServer"))
                    SourceServer.Text = localSettings.Values["SourceServer"].ToString();

                if (localSettings.Values.ContainsKey("DestServer"))
                    DestinationServer.Text = localSettings.Values["DestServer"].ToString();

                if (localSettings.Values.ContainsKey("SourceDatabase"))
                    SourceDatabase.Text = localSettings.Values["SourceDatabase"].ToString();

                if (localSettings.Values.ContainsKey("DestDatabase"))
                    DestinationDatabase.Text = localSettings.Values["DestDatabase"].ToString();

                if (localSettings.Values.ContainsKey("SourceUser"))
                    SourceUser.Text = localSettings.Values["SourceUser"].ToString();

                if (localSettings.Values.ContainsKey("DestUser"))
                    DestinationUser.Text = localSettings.Values["DestUser"].ToString();

                // 2. Load the Windows Auth toggle state
                if (localSettings.Values.ContainsKey("SourceUseWindowsAuth"))
                {
                    bool isWindowsAuth = (bool)localSettings.Values["SourceUseWindowsAuth"];
                    SourceUseWindowsAuth.IsChecked = isWindowsAuth;
                    // Manually trigger the UI state change for the textboxes
                    Auth_CheckChanged(null, null);
                }
                if (localSettings.Values.ContainsKey("DestUseWindowsAuth"))
                {
                    bool isWindowsAuth = (bool)localSettings.Values["DestUseWindowsAuth"];
                    DestinationUseWindowsAuth.IsChecked = isWindowsAuth;
                    // Manually trigger the UI state change for the textboxes
                    Auth_CheckChanged(null, null);
                }

                // 3. Retrieve Password securely if not using Windows Auth
                if (!(SourceUseWindowsAuth.IsChecked ?? false))
                {
                    string server = SourceServer.Text;
                    string db = SourceDatabase.Text;
                    string user = SourceUser.Text;
                    string resourceKey = $"SQLDataBridge_Source_{server}_{db}";

                    try
                    {
                        var cred = vault.Retrieve(resourceKey, user);
                        SourcePassword.Password = cred.Password;
                    }
                    catch
                    {
                        // No password found for this specific server/db combo; ignore
                    }
                }
                if (!(DestinationUseWindowsAuth.IsChecked ?? false))
                {
                    string server = DestinationServer.Text;
                    string db = DestinationDatabase.Text;
                    string user = DestinationUser.Text;
                    string resourceKey = $"SQLDataBridge_Dest_{server}_{db}";

                    try
                    {
                        var cred = vault.Retrieve(resourceKey, user);
                        DestinationPassword.Password = cred.Password;
                    }
                    catch
                    {
                        // No password found for this specific server/db combo; ignore
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Could not load saved settings.", InfoBarSeverity.Informational);
            }
        }
        
    }
}
