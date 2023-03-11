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
        private const string updateuri = "http://soft.piter-furry.ru/Software/Pinger/update/update.deploy";
        private const string tempdirname = "spm_update_tmp";
        private const string archivename = "update.zip";

        private string _appDir = Directory.GetCurrentDirectory();
        private string _tempDir = Directory.GetCurrentDirectory() + "\\" + tempdirname + "\\";


        public event Action UpdateCompletedEvent;
        public event Action UpdateFailedEvent;

        Action<long,long> incrementProgress;
        Action<string> incrementStatus;

        public Updater(Action<long, long> incrementProgress, Action<string> incrementStatus)
        {
            this.incrementProgress = incrementProgress;
            this.incrementStatus = incrementStatus;
        }


        public void UpdateAllFiles()
        {
            incrementStatus.Invoke("Starting Update...Wait(7)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(6)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(5)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(4)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(3)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(2)");
            Thread.Sleep(1000);
            incrementStatus.Invoke("Starting Update...Wait(1)");
            Thread.Sleep(1000);

            

            incrementStatus.Invoke("Create temp folder");            

            try
            {
                if (File.Exists(archivename)) //Если файлы существуют удалим их
                {
                    File.Delete(archivename);
                }


                if (Directory.Exists(_tempDir)) //Если директория существует удалим её
                {
                    Directory.Delete(_tempDir, true);
                }
            }
            catch
            {
                incrementStatus.Invoke("Cannot create temp folder. Check app folder ntfs permissions.");
                UpdateFailedEvent?.Invoke();
                return;
            }

            incrementStatus.Invoke("Downloading files");

            try
            {

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted; ;
                    client.DownloadFileTaskAsync(new Uri(updateuri), archivename);
                }
            }
            catch
            {
                
                incrementStatus.Invoke("Error: Cannot connect to update server!");
                UpdateFailedEvent?.Invoke();
                return;
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            incrementProgress.Invoke(e.BytesReceived, e.TotalBytesToReceive);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            incrementStatus.Invoke("Updating Files");

            if (File.Exists(archivename) && new FileInfo(archivename).Length > 0)
            {
                try
                {
                    
                    Directory.CreateDirectory(_tempDir);

                    ZipFile.ExtractToDirectory(archivename, _tempDir);

                    File.Delete(archivename);

                    

                    // Получаем полные имена (с путем) файлов                     
                    List<string> files = Directory.GetFiles(_tempDir, "*.*", SearchOption.AllDirectories).ToList();


                    int handledFilesCounter = 1;
                    foreach (string file in files) //В цикле будем копировать каждый файл из временной папке в основную с заменой исходных
                    {
                        //Filename относительно _tempDir
                        string filename = file.Substring(file.LastIndexOf(tempdirname) + tempdirname.Count() + 1).Trim('\\'); // Отрезаем путь
                        try
                        {
                            if (filename.Contains('\\'))
                            {
                                string dirname = filename.Substring(0,  filename.LastIndexOf('\\'));
                                if (!Directory.Exists(dirname)) { Directory.CreateDirectory(dirname); }
                            }

                            File.Copy(file, _appDir + "\\" + filename, true);

                            incrementProgress.Invoke(handledFilesCounter++, files.Count());
                            incrementStatus.Invoke("Updating File: " + filename);
                            Thread.Sleep(100);
                        }
                        catch
                        {
                            try{Directory.Delete(_tempDir, true); }catch{}
                            incrementStatus.Invoke("Error: " + filename + " is busy. Restart program or reboot your computer.");
                            UpdateFailedEvent?.Invoke();
                            return;
                        }
                    }

                    Directory.Delete(_tempDir, true); //Удаляем временную папку

                    incrementStatus.Invoke("Update Complete");
                    Thread.Sleep(1000);

                    System.Diagnostics.Process.Start("Spm.exe"); //Запускаем SPM Monitoring.

                    UpdateCompletedEvent?.Invoke();
                }
                catch (Exception ex)
                {
                    incrementStatus.Invoke("Exception: " + ex.Message);
                    UpdateFailedEvent?.Invoke();
                    return;
                }
            }
            else
            {
                incrementStatus.Invoke("Update source file not found or corrupted.");
                UpdateFailedEvent?.Invoke();
                return;
            }
        }
    }
}
