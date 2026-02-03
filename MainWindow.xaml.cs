using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Data.SqlClient;

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
                ContentFrame.Navigate(typeof(MigrationPage));
            }
        }
    }



}
