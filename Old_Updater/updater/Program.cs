using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading;


namespace updater
{
    class Program
    {
        static void Main(string[] args)
        {
            

            if (args.Length < 1) // Если не указаны аргументы то приложение запустили не из GUI
            {
                Console.WriteLine("This is a part of Application. Do not launch this module directly. It will take no effect.");
                Console.ReadLine();
                return;
            }

            if (args[0] == "update") 
            {
                Console.WriteLine("Starting Update");
                Thread.Sleep(7000);

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
                        client.DownloadFile(new Uri("http://soft.piter-furry.ru/Software/Pinger/update/update.deploy"), "update.zip");
                    }
                }
                catch
                {
                    Console.WriteLine("Error: Cannot connect to update server!");
                    Console.ReadLine();
                    return;
                }
                                  
                    string TempDirectoryPath = Directory.GetCurrentDirectory() + "\\temp\\";

                    Directory.CreateDirectory(TempDirectoryPath);

                    ZipFile.ExtractToDirectory("update.zip", TempDirectoryPath);

                    File.Delete("update.zip");

                    //Записываем в переменную количество файлов
                    int FileCounter = Directory.GetFiles(TempDirectoryPath, "*.*", SearchOption.TopDirectoryOnly).Length;


                    // Получаем полные имена (с путем) файлов 
                    string[] filesArr = new string[FileCounter];
                    filesArr = Directory.GetFiles(TempDirectoryPath, "*.*", SearchOption.TopDirectoryOnly);

                    foreach (string file in filesArr) //В цикле будем копировать каждый файл из временной папке в основную с заменой исходных
                    {

                        string filename = file.Substring(file.LastIndexOf('\\') + 1); // Отрезаем путь
                        try
                        {
                            File.Copy(TempDirectoryPath + filename, Directory.GetCurrentDirectory() + "\\" + filename, true);
                        Console.WriteLine("Updating File: " + filename);
                        Thread.Sleep(100);
                        }
                        catch
                        {
                        Directory.Delete(Directory.GetCurrentDirectory() + "\\temp\\", true);
                        Console.WriteLine("Error: Some files is busy. Restart program or reboot your computer.");
                        Console.ReadLine();
                        return;
                        }
                    }

                    Directory.Delete(Directory.GetCurrentDirectory() + "\\temp\\", true); //Удаляем временную папку

                Console.WriteLine("Update Complete");
                Thread.Sleep(1000);

                System.Diagnostics.Process.Start("Spm.exe"); //Запускаем SPM Monitoring.

            }
        }
    }
}
