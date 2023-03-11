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
            StartUpdate();
        }


        private void StartUpdate()
        {
            Task.Run(() =>
            {               
                Updater updater = new Updater(incrementProgressBar, incrementStatus);
                updater.UpdateCompletedEvent += Updater_UpdateCompletedEvent;
                updater.UpdateFailedEvent += Updater_UpdateFailedEvent;
                updater.UpdateAllFiles();
            });
        }

        private void Updater_UpdateFailedEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                TryAgainButton.Visibility = Visibility.Visible;
            }));
        }

        private void Updater_UpdateCompletedEvent()
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
                StatusLabel.ToolTip = status_text;
            }));
        }

        private void TryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            TryAgainButton.Visibility = Visibility.Collapsed;
            StartUpdate();
        }
    }
}
