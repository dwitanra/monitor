using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Configuration;

namespace FTP_Archive
{
    class Program
    {
        static string FTP_Server = "";
        static string FTP_User = "";
        static string FTP_Password = "";
        static string Archive_Dir = "";
        static string Archive_Date = "";
        static string Temp_Dir = "";

        static List<string> files;

        static void Main(string[] args)
        {
            files = new List<string>();

            FTP_Server = ConfigurationManager.AppSettings["FTP_Server"];
            FTP_User = ConfigurationManager.AppSettings["FTP_User"];
            FTP_Password = ConfigurationManager.AppSettings["FTP_Password"];
            Archive_Dir = ConfigurationManager.AppSettings["Archive_Dir"];
            Archive_Date = ConfigurationManager.AppSettings["Archive_Date"];
            Temp_Dir = ConfigurationManager.AppSettings["Temp_Dir"];

            Directory.CreateDirectory(Archive_Dir + Temp_Dir);

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
                    string destination = (Archive_Dir + Temp_Dir + files[i].ToString()).Replace("/", "\\");
                    ftp.Download(source, destination);
                    ftp.Delete(source);
                    //Console.WriteLine(files[i].ToString());
                }
            }

            //compress into zip
            ZipFile.CreateFromDirectory(Archive_Dir + Temp_Dir, Archive_Dir + DateTime.Now.ToString(Archive_Date) + ".zip");

            //delete files on ftp
            Directory.Delete(Archive_Dir + Temp_Dir, true);
        }
    }
}
