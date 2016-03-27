using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace WitanraSecurity
{
    class Program
    {
        static void Main(string[] args)
        {
            GetFTPImages();

            NormalizeFiles();

            MakeVideos();
        }
        
        private static void MakeVideos()
        {
            string Monitor_Dir = ConfigurationManager.AppSettings["Monitor_Dir"];
            string today = DateTime.Today.ToString("yyyyMMdd");
            string[] cameras = Directory.GetDirectories(Monitor_Dir);
            foreach (string camera in cameras)
            {
                string[] dates = Directory.GetDirectories(camera);
                foreach (string date in dates)
                {
                    //if (!date.Contains(today))
                    {
                        string[] files = Directory.GetFiles(date, "*.jpg");
                        Array.Sort(files, StringComparer.InvariantCulture);
                        for (int i = 0; i < files.Length; i++)
                        {
                            File.Move(files[i], date + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg");
                        }

                        LaunchCommandLineApp(date, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " + date + ".mp4");

                        try
                        {
                            Directory.Delete(date, true);
                        }
                        catch { }                       
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
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = argument;
            startInfo.WorkingDirectory = dir;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            try
            {
                Process exeProcess = Process.Start(startInfo);
                exeProcess.Start();

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

        private static void AddTimestamp()
        {
            throw new NotImplementedException();
        }



        public static void GetFTPImages()
        {
            List<string> files = new List<string>();
            string FTP_Server = ConfigurationManager.AppSettings["FTP_Server"];
            string FTP_User = ConfigurationManager.AppSettings["FTP_User"];
            string FTP_Password = ConfigurationManager.AppSettings["FTP_Password"];
            string Monitor_Dir = ConfigurationManager.AppSettings["Monitor_Dir"];

            Directory.CreateDirectory(Monitor_Dir);
            //get list of files at ftp
            FTP ftp = new FTP(FTP_Server, FTP_User, FTP_Password);
            files = ftp.Get_List();

            //download and delete
            for (int i = 0; i <= files.Count - 1; i++)
            {
                files[i] = files[i].Replace(FTP_Server, "/");
                if (files[i].Contains("."))
                {
                    string source = FTP_Server + files[i].ToString();
                    string destination = (Monitor_Dir + files[i].ToString()).Replace("/", "\\");
                    ftp.Download(source, destination);
                    //ftp.Delete(source);
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
                if (oldfile.Contains("back\\00626E46E698(Back)_1_"))
                {
                    newfile = Monitor_Dir + "back\\" + oldfile.Substring(26, 8) + "\\" + oldfile.Substring(34, 6) + "_" + Convert.ToString(i) + ".jpg";
                    Directory.CreateDirectory(Path.GetDirectoryName(newfile));

                    File.Copy(files[i], newfile, true);
                    File.Delete(files[i]);
                }

                oldfile = files[i].Replace(Monitor_Dir, "");
                if (oldfile.Contains("front\\00626E4A420A(Front)"))
                {
                    newfile = Monitor_Dir + "front\\" + oldfile.Substring(28, 8) + "\\" + oldfile.Substring(36, 6) + "_" + Convert.ToString(i) + ".jpg";
                    Directory.CreateDirectory(Path.GetDirectoryName(newfile));

                    File.Copy(files[i], newfile, true);
                    File.Delete(files[i]);
                }
            }
        }
    }
}
