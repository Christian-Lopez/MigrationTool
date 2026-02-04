using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationTool
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // This makes the title bar look integrated with the app
            ExtendsContentIntoTitleBar = true;
            // Set the default page to Connections
            ContentFrame.Navigate(typeof(ConnectionsPage));
            SetVersionDisplay();
        }

        private void MainNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItemContainer as NavigationViewItem;
            if (selectedItem?.Tag?.ToString() == "ConnectionsPage")
            {
                ContentFrame.Navigate(typeof(ConnectionsPage));
            }
            else if (selectedItem?.Tag?.ToString() == "MigrationPage")
            {
                ContentFrame.Navigate(typeof(DataMigrationPage));
            }
            else if (selectedItem?.Tag?.ToString() == "MigrationsPage")
            {
                ContentFrame.Navigate(typeof(MigrationsPage));
            }
        }
        //private void ClearAllSettings()
        //{
        //    try
        //    {
        //        // 1. Clear LocalSettings (Server, DB, etc.)
        //        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        //        localSettings.Values.Clear();

        //        // 2. Clear the Password Vault
        //        var vault = new Windows.Security.Credentials.PasswordVault();
        //        try
        //        {
        //            // The vault doesn't have a "ClearAll" for specific apps, 
        //            // so we retrieve the list and delete our specific entries.
        //            var credentials = vault.RetrieveAll();
        //            foreach (var cred in credentials)
        //            {
        //                if (cred.Resource.StartsWith("SQLDataBridge_"))
        //                {
        //                    vault.Remove(cred);
        //                }
        //            }
        //        }
        //        catch { /* Vault is already empty */ }

        //        // 3. Clear UI Fields
        //        SourceServer.Text = string.Empty;
        //        SourceDatabase.Text = string.Empty;
        //        SourceUser.Text = string.Empty;
        //        SourcePassword.Password = string.Empty;
        //        SourceUseWindowsAuth.IsChecked = false;
        //        DestinationServer.Text = string.Empty;
        //        DestinationDatabase.Text = string.Empty;
        //        DestinationUser.Text = string.Empty;
        //        DestinationPassword.Password = string.Empty;
        //        DestinationUseWindowsAuth.IsChecked = false;

        //        ShowStatus("All local data and credentials have been wiped.", InfoBarSeverity.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowStatus($"Error clearing data: {ex.Message}", InfoBarSeverity.Error);
        //    }
        //}
        private async void ClearAllSettings_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Clear All Data?",
                Content = "This will permanently delete saved server addresses and passwords. Continue?",
                PrimaryButtonText = "Clear Everything",
                CloseButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot, // Required in WinUI 3
                DefaultButton = ContentDialogButton.Close
            };
            if (await confirmDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // 1. Wipe LocalSettings
                Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();

                // 2. Wipe Vault
                var vault = new Windows.Security.Credentials.PasswordVault();
                try
                {
                    foreach (var c in vault.RetrieveAll())
                        if (c.Resource.StartsWith("SQLDataBridge_")) vault.Remove(c);
                }
                catch { }

                // 3. Force go back to Connections page to show it's empty
                ContentFrame.Navigate(typeof(ConnectionsPage));
            }
        }
        private void SetVersionDisplay()
        {
            // Get the package version (Major.Minor.Build)
            var version = Package.Current.Id.Version;
            VersionTextBlock.Text = $"v{version.Major}.{version.Minor}.{version.Build}";
        }
    }



}
