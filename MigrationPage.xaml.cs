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
using Windows.Foundation;
using Windows.Foundation.Collections;
using MigrationTool.Shared;
// Inside MigrationPage logic
//using var source = new SqlConnection(ConnectionsService.SourceBuilder.ConnectionString);
//using var dest = new SqlConnection(ConnectionsService.DestBuilder.ConnectionString);

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MigrationPage : Page
    {
        public MigrationPage()
        {

            InitializeComponent();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            ShowStatus("Destination Saved!", InfoBarSeverity.Success);
        }
        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            //StatusInfoBar.Message = message;
            //StatusInfoBar.Severity = severity;
            //StatusInfoBar.Title = severity == InfoBarSeverity.Success ? "Success" : "Error";
            //StatusInfoBar.IsOpen = true;
        }
    }
}
