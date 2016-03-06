using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using System.Linq;

namespace FTP_Archive
{
    class Program
    {
        static string FTP_Server = "";
        static string FTP_User = "";
        static string FTP_Password = "";
        static string Archive_Temporary_Dir = "";
        static string Archive_Permanent_Dir = "";
        static string Temp_Dir = "";
        static string Archive_File_Format = "";
        static int Archive_Temporary_Count = 0;
        static string Archive_Permanent_Match_Patten = "";

        static List<string> files;

        static void Main(string[] args)
        {
            files = new List<string>();

            FTP_Server = ConfigurationManager.AppSettings["FTP_Server"];
            FTP_User = ConfigurationManager.AppSettings["FTP_User"];
            FTP_Password = ConfigurationManager.AppSettings["FTP_Password"];
            Archive_Temporary_Dir = ConfigurationManager.AppSettings["Archive_Temporary_Dir"];
            Archive_Permanent_Dir = ConfigurationManager.AppSettings["Archive_Permanent_Dir"];
            Temp_Dir = ConfigurationManager.AppSettings["Temp_Dir"];
            Archive_File_Format = ConfigurationManager.AppSettings["Archive_File_Format"];
            Archive_Temporary_Count = Convert.ToInt32(ConfigurationManager.AppSettings["Archive_Temporary_Count"]);
            Archive_Permanent_Match_Patten = ConfigurationManager.AppSettings["Archive_Permanent_Match_Patten"];

            Directory.CreateDirectory(Archive_Temporary_Dir);
            Directory.CreateDirectory(Archive_Permanent_Dir);            
            Directory.CreateDirectory(Temp_Dir);

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
                    string destination = (Temp_Dir + files[i].ToString()).Replace("/", "\\");
                    ftp.Download(source, destination);
                    ftp.Delete(source);
                    //Console.WriteLine(files[i].ToString());
                }
            }

            string Filename = DateTime.Now.ToString(Archive_File_Format) + ".zip";
            //compress into zip
            ZipFile.CreateFromDirectory(Temp_Dir, Archive_Temporary_Dir + Filename);

            //copy to Long Dir if matches
            if (Filename.Contains(Archive_Permanent_Match_Patten))
            {
                File.Copy(Archive_Temporary_Dir + Filename, Archive_Permanent_Dir + DateTime.Now.ToString(Archive_File_Format) + ".zip");
            }

            //delete old files in Archive_Temporary_Dir
            var tempfiles = Directory.GetFiles(Archive_Temporary_Dir, "*.zip").OrderByDescending(d => new FileInfo(d).CreationTime);
            int j = 1;
            foreach (string file in tempfiles)
            {
                if (j > Archive_Temporary_Count)
                {
                    File.Delete(file);
                }
                j++;
            }

            //delete temp
            Directory.Delete(Temp_Dir, true);
        }
    }
}
