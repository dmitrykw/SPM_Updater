using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
                                 

            Task.Run(() =>
            {               
                Updater updater = new Updater(incrementProgressBar, incrementStatus);
                updater.updateCompletedEvent += Updater_updateCompletedEvent;
                updater.UpdateAllFiles();
            });
        }

        private void Updater_updateCompletedEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                 this.Close();
            }));
        }

        private void incrementProgressBar(long bytesReceived, long totalBytes)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                 MyProgressBar.Value = bytesReceived; MyProgressBar.Maximum = totalBytes;
            }));
        }

        private void incrementStatus(string status_text)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                 StatusLabel.Content = status_text;
            }));
        }
    }
}
