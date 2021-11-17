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

        public event Action updateCompletedEvent;

        Action<long,long> incrementProgress;
        Action<string> incrementStatus;

        public Updater(Action<long, long> incrementProgress, Action<string> incrementStatus)
        {
            this.incrementProgress = incrementProgress;
            this.incrementStatus = incrementStatus;
        }


        public void UpdateAllFiles()
        {
            incrementStatus.Invoke("Starting Update...");            

            Thread.Sleep(7000);

            incrementStatus.Invoke("Downloading files");

            if (File.Exists("update.zip")) //Если файлы существуют удалим их
            {
                File.Delete("update.zip");
            }


            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\temp\\")) //Если директория существует удалим её
            {
                Directory.Delete(Directory.GetCurrentDirectory() + "\\temp\\", true);
            }
            try
            {

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted; ;
                    client.DownloadFileTaskAsync(new Uri("http://soft.piter-furry.ru/Software/Pinger/update/update.deploy"), "update.zip");
                }
            }
            catch
            {
                
                incrementStatus.Invoke("Error: Cannot connect to update server!");                
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


            string TempDirectoryPath = Directory.GetCurrentDirectory() + "\\temp\\";

            Directory.CreateDirectory(TempDirectoryPath);

            ZipFile.ExtractToDirectory("update.zip", TempDirectoryPath);

            File.Delete("update.zip");

            //Записываем в переменную количество файлов
            int FileCounter = Directory.GetFiles(TempDirectoryPath, "*.*", SearchOption.TopDirectoryOnly).Length;


            // Получаем полные имена (с путем) файлов 
            string[] filesArr = new string[FileCounter];
            filesArr = Directory.GetFiles(TempDirectoryPath, "*.*", SearchOption.TopDirectoryOnly);


            long handledFilesCounter = 1;
            foreach (string file in filesArr) //В цикле будем копировать каждый файл из временной папке в основную с заменой исходных
            {

                string filename = file.Substring(file.LastIndexOf('\\') + 1); // Отрезаем путь
                try
                {
                    File.Copy(TempDirectoryPath + filename, Directory.GetCurrentDirectory() + "\\" + filename, true);
                    incrementProgress.Invoke(handledFilesCounter++, filesArr.Count());
                    incrementStatus.Invoke("Updating File: " + filename);
                    Thread.Sleep(100);
                }
                catch
                {
                    Directory.Delete(Directory.GetCurrentDirectory() + "\\temp\\", true);
                    incrementStatus.Invoke("Error: Some files is busy. Restart program or reboot your computer.");
                    return;
                }
            }

            Directory.Delete(Directory.GetCurrentDirectory() + "\\temp\\", true); //Удаляем временную папку

            incrementStatus.Invoke("Update Complete");
            Thread.Sleep(1000);

            System.Diagnostics.Process.Start("Spm.exe"); //Запускаем SPM Monitoring.

            updateCompletedEvent?.Invoke();
        }

        
    }
}
