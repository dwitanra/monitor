using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WitanraSecurity
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting FTP Images");
            GetFTPImages();

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

                    string camera = "unknown";
                    string date = "unknown";
                    string time = "unknown";
                    var regex = new Regex(@"\\(.*)\\.*(2\d{7})(\d{6})");
                    var match = regex.Match(destination.Replace(Monitor_Dir, ""));
                    if (match.Success)
                    {
                        camera = match.Groups[1].Value;
                        date = match.Groups[2].Value;
                        time = match.Groups[3].Value;
                        destination = Monitor_Dir + "\\" + camera + "\\" + date + "\\" + time + "_" + i.ToString() + ".jpg";
                    }

                    Console.WriteLine("Downloading " + source + " to " + destination);
                    ftp.Download(source, destination);
                    AddTimestamp(destination, date + "_" + time);
                    ftp.Delete(source);
                }
            }
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
                    if (date.Contains(today))
                    {
                        //dont make todays video yet!
                        continue;
                    }
                    string temp = Temp_Dir + ".mp4";
                    string destination = date + ".mp4";
                    try
                    {
                        Directory.Delete(Temp_Dir, true);
                    }
                    catch
                    {
                    }
                    Directory.CreateDirectory(Temp_Dir);
                    string[] files = Directory.GetFiles(date, "*.jpg");
                    Array.Sort(files, StringComparer.InvariantCulture);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string oldFile = files[i];
                        string newFile = Temp_Dir + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg";
                        Console.WriteLine("Copying file from " + oldFile + " to " + newFile);
                        File.Copy(oldFile, newFile);
                    }

                    Console.WriteLine("Saving Video " + temp);
                    LaunchCommandLineApp(Temp_Dir, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " + temp);
                    CopySafe(temp, destination);

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
                        Directory.Delete(date, true);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
        }

        private static void CopySafe(string source, string destination)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(destination);
            string extension = Path.GetExtension(destination);
            string path = Path.GetDirectoryName(destination);

            while (File.Exists(destination))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                destination = Path.Combine(path, tempFileName + extension);
            }

            File.Copy(source, destination);
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
    }
}
