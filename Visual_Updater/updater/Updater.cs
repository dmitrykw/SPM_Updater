using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace updater
{
    class Updater
    {        
        private const string _tempdirname = "spm_update_tmp";
        private const string _archivename = "update.zip";

        private readonly string _update_file_uri;

        private readonly string _appDir = Directory.GetCurrentDirectory();
        private readonly string _tempDir = Directory.GetCurrentDirectory() + "\\" + _tempdirname + "\\";


        public event Action UpdateCompletedEvent;
        public event Action UpdateFailedEvent;

        private readonly Action<long,long> _incrementProgressAction;
        private readonly Action<string> _incrementStatusAction;

        public Updater(Action<long, long> incrementProgress, Action<string> incrementStatus, string updateFileUri)
        {
            _incrementProgressAction = incrementProgress;
            _incrementStatusAction = incrementStatus;
            _update_file_uri = updateFileUri;
        }


        public void UpdateAllFiles()
        {
            _incrementStatusAction.Invoke("Starting Update...Wait(7)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(6)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(5)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(4)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(3)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(2)");
            Thread.Sleep(1000);
            _incrementStatusAction.Invoke("Starting Update...Wait(1)");
            Thread.Sleep(1000);



            _incrementStatusAction.Invoke("Create temp folder");            

            try
            {
                if (File.Exists(_archivename)) //Если файлы существуют удалим их
                {
                    File.Delete(_archivename);
                }


                if (Directory.Exists(_tempDir)) //Если директория существует удалим её
                {
                    Directory.Delete(_tempDir, true);
                }
            }
            catch
            {
                _incrementStatusAction.Invoke("Cannot create temp folder. Check app folder ntfs permissions.");
                UpdateFailedEvent?.Invoke();
                return;
            }

            _incrementStatusAction.Invoke("Downloading files");

            try
            {

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileTaskAsync(new Uri(_update_file_uri), _archivename);
                }
            }
            catch
            {

                _incrementStatusAction.Invoke("Error: Cannot connect to update server!");
                UpdateFailedEvent?.Invoke();
                return;
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _incrementProgressAction.Invoke(e.BytesReceived, e.TotalBytesToReceive);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _incrementStatusAction.Invoke("Updating Files");

            if (File.Exists(_archivename) && new FileInfo(_archivename).Length > 0)
            {
                try
                {
                    
                    Directory.CreateDirectory(_tempDir);

                    ZipFile.ExtractToDirectory(_archivename, _tempDir);

                    File.Delete(_archivename);

                    

                    // Получаем полные имена (с путем) файлов                     
                    List<string> files = Directory.GetFiles(_tempDir, "*.*", SearchOption.AllDirectories).ToList();


                    int handledFilesCounter = 1;
                    foreach (string file in files) //В цикле будем копировать каждый файл из временной папке в основную с заменой исходных
                    {
                        //Filename относительно _tempDir
                        string filename = file.Substring(file.LastIndexOf(_tempdirname) + _tempdirname.Count() + 1).Trim('\\'); // Отрезаем путь
                        try
                        {
                            if (filename.Contains('\\'))
                            {
                                string dirname = filename.Substring(0,  filename.LastIndexOf('\\'));
                                if (!Directory.Exists(dirname)) { Directory.CreateDirectory(dirname); }
                            }

                            File.Copy(file, _appDir + "\\" + filename, true);

                            _incrementProgressAction.Invoke(handledFilesCounter++, files.Count());
                            _incrementStatusAction.Invoke("Updating File: " + filename);
                            Thread.Sleep(100);
                        }
                        catch
                        {
                            try{Directory.Delete(_tempDir, true); }catch{}
                            _incrementStatusAction.Invoke("Error: " + filename + " is busy. Restart program or reboot your computer.");
                            UpdateFailedEvent?.Invoke();
                            return;
                        }
                    }

                    Directory.Delete(_tempDir, true); //Удаляем временную папку

                    _incrementStatusAction.Invoke("Update Complete");
                    Thread.Sleep(1000);

                    System.Diagnostics.Process.Start("Spm.exe"); //Запускаем SPM Monitoring.

                    UpdateCompletedEvent?.Invoke();
                }
                catch (Exception ex)
                {
                    _incrementStatusAction.Invoke("Exception: " + ex.Message);
                    UpdateFailedEvent?.Invoke();
                    return;
                }
            }
            else
            {
                _incrementStatusAction.Invoke("Update source file not found or corrupted.");
                UpdateFailedEvent?.Invoke();
                return;
            }
        }
    }
}
