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
            // Build the string dynamically
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = SourceServer.Text,
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
        private async Task<bool> OpenConnectionAsync(string connectionString)
        {
            try
            {
                // Clear the connection pool before attempting to connect
                // This is CRITICAL for LocalDB instances
                SqlConnection.ClearAllPools();
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Optionally verify the connection with a simple query
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync();

                
                return true;
            }
            catch (SqlException ex)
            {
                ShowStatus($"Connection failed: {ex.Message}", InfoBarSeverity.Error);
                return false;
            }
            catch (Exception ex)
            {
                ShowStatus($"Unexpected error: {ex.Message}", InfoBarSeverity.Error);
                return false;
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
