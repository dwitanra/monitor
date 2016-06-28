using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace WitanraSecurity
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting FTP Images");
            GetFTPImages();

            Console.WriteLine("Normalizing Files");
            NormalizeFiles();

            Console.WriteLine("Making Videos");
            MakeVideos();

            //Console.ReadLine();
            Environment.Exit(1);
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }

        private static void MakeVideos()
        {
            string Monitor_Dir = ConfigurationManager.AppSettings["Monitor_Dir"];
            string Temp_Dir = ConfigurationManager.AppSettings["Temp_Dir"];

            string today = DateTime.Today.ToString("yyyyMMdd");
            string[] cameras = Directory.GetDirectories(Monitor_Dir);
            foreach (string camera in cameras)
            {
                string[] dates = Directory.GetDirectories(camera);
                foreach (string date in dates)
                {
                    try
                    {
                        Directory.Delete(Temp_Dir, true);
                    }
                    catch { }
                    Directory.CreateDirectory(Temp_Dir);
                    string[] files = Directory.GetFiles(date, "*.jpg");
                    Array.Sort(files, StringComparer.InvariantCulture);
                    for (int i = 0; i < files.Length; i++)
                    {
                        AddTimestamp(files[i], files[i].Replace(Monitor_Dir, ""));
                        string oldFile = files[i];
                        string newFile = Temp_Dir + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg";
                        Console.WriteLine("Copying file from " + oldFile + " to " + newFile);
                        File.Copy(oldFile, newFile);
                    }

                    Console.WriteLine("Saving Video " + Temp_Dir + ".mp4");
                    LaunchCommandLineApp(Temp_Dir, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " + Temp_Dir + ".mp4");

                    if (date.Contains(today))
                    {
                        if (File.Exists(date + ".mp4"))
                            File.Delete(date + ".mp4");
                    }

                    if (File.Exists(Temp_Dir + ".mp4"))
                    {
                        if (File.Exists(date + ".mp4"))
                        {                          
                            int i = 1;
                            while (File.Exists(date + '(' + i.ToString() + ')' + ".mp4"))
                            {
                                i++;
                            }
                            File.Copy(Temp_Dir + ".mp4", date + '(' + i.ToString() + ')' + ".mp4");
                        }
                        else
                        {
                            File.Copy(Temp_Dir + ".mp4", date + ".mp4");
                        }
                    }
                    try
                    {
                        Directory.Delete(Temp_Dir, true);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    try
                    {
                        if (!date.Contains(today))
                        {
                            Directory.Delete(date, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
        }

        private static void LaunchCommandLineApp(string dir, string exe, string argument)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = exe;
            startInfo.Arguments = argument;
            startInfo.WorkingDirectory = dir;
            try
            {
                Process exeProcess = Process.Start(startInfo);

                exeProcess.WaitForExit();
                exeProcess.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {

            }
        }

        private static void AddTimestamp(string filename, string datetime)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                Image image = Image.FromStream(fs);
                fs.Close();

                Bitmap b = new Bitmap(image);
                Graphics graphics = Graphics.FromImage(b);
                graphics.DrawString(datetime, new Font(new FontFamily("Courier New"), 10, FontStyle.Regular), Brushes.Yellow, 0, 0);

                b.Save(filename, image.RawFormat);

                image.Dispose();
                b.Dispose();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public static void GetFTPImages()
        {
            List<string> files = new List<string>();
            string FTP_Server = ConfigurationManager.AppSettings["FTP_Server"];
            string FTP_User = ConfigurationManager.AppSettings["FTP_User"];
            string FTP_Password = ConfigurationManager.AppSettings["FTP_Password"];
            string Monitor_Dir = ConfigurationManager.AppSettings["Monitor_Dir"];

            Directory.CreateDirectory(Monitor_Dir);

            FTP ftp = new FTP(FTP_Server, FTP_User, FTP_Password);
            files = ftp.Get_List();

            for (int i = 0; i <= files.Count - 1; i++)
            {
                files[i] = files[i].Replace(FTP_Server, "/");
                if (files[i].Contains("."))
                {
                    string source = FTP_Server + files[i].ToString();
                    string destination = (Monitor_Dir + files[i].ToString()).Replace("/", "\\");
                    Console.WriteLine("Downloading " + source + " to " + destination);
                    ftp.Download(source, destination);
                    ftp.Delete(source);
                }
            }
        }

        private static void NormalizeFiles()
        {
            string Monitor_Dir = ConfigurationManager.AppSettings["Monitor_Dir"];
            string[] files = Directory.GetFiles(Monitor_Dir, "*.jpg", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.InvariantCulture);

            string oldfile;
            string newfile;
            for (int i = 0; i < files.Length; i++)
            {
                oldfile = files[i].Replace(Monitor_Dir, "");
                if (oldfile.Contains("back\\00626E46E698(Back)"))
                {
                    try
                    {
                        newfile = Monitor_Dir + "back\\" + oldfile.Substring(26, 8) + "\\" + oldfile.Substring(34, 6) + "_" + Convert.ToString(i) + ".jpg";
                        Directory.CreateDirectory(Path.GetDirectoryName(newfile));

                        Console.WriteLine("Renaming " + files[i] + " to " + newfile);

                        File.Copy(files[i], newfile, true);
                        File.Delete(files[i]);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }

                oldfile = files[i].Replace(Monitor_Dir, "");
                if (oldfile.Contains("front\\00626E4A420A(Front)"))
                {
                    try
                    {
                        newfile = Monitor_Dir + "front\\" + oldfile.Substring(28, 8) + "\\" + oldfile.Substring(36, 6) + "_" + Convert.ToString(i) + ".jpg";
                        Directory.CreateDirectory(Path.GetDirectoryName(newfile));

                        Console.WriteLine("Renaming " + files[i] + " to " + newfile);

                        File.Copy(files[i], newfile, true);
                        File.Delete(files[i]);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }

            }
        }
    }
}
