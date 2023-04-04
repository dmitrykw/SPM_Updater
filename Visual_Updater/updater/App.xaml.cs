using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static string UpdateFileURI = "http://soft.renkti.net/Software/Pinger/update/update.deploy";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length < 1 || e.Args[0].Trim().ToLower() != "update")
            {
                MessageBox.Show("This is a part of SPM Monitoring Application. Do not launch this module directly. It will take no effect.", "Updater Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            if (e.Args.Length > 1)
            {
                UpdateFileURI = e.Args[1].Trim();
            }            
        }
    }
}
