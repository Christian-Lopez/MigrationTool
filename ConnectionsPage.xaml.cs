using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MigrationTool.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
                TrustServerCertificate = true
            };
            // Only add credentials if NOT using Windows Auth
            if (!(SourceUseWindowsAuth.IsChecked ?? false))
            {
                builder.UserID = SourceUser.Text;
                builder.Password = SourcePassword.Password;
            }
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
                DataSource = SourceServer.Text,
                InitialCatalog = SourceDatabase.Text,
                IntegratedSecurity = SourceUseWindowsAuth.IsChecked ?? false,
                Encrypt = true,
                TrustServerCertificate = true
            };
            // Only add credentials if NOT using Windows Auth
            if (!(SourceUseWindowsAuth.IsChecked ?? false))
            {
                builder.UserID = SourceUser.Text;
                builder.Password = SourcePassword.Password;
            }
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
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                ShowStatus($"Connection failed: {ex.Message}", InfoBarSeverity.Error);
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
    }
}
