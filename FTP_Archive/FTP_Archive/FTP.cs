using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Archive
{
    class FTP
    {
        private string _FTP_Server;
        private string _FTP_User;
        private string _FTP_Password;

        public FTP(string FTP_Server, string FTP_User, string FTP_Password)
        {
            _FTP_Server = FTP_Server;
            _FTP_User = FTP_User;
            _FTP_Password = FTP_Password;
        }

        public List<string> Get_List()
        {
            List<string> result = new List<string>();

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(_FTP_Server);
            ftpRequest.Credentials = new NetworkCredential(_FTP_User, _FTP_Password);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();

            StreamReader streamReader = new StreamReader(response.GetResponseStream());

            List<string> contents = new List<string>();

            string line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                contents.Add(line);
                line = streamReader.ReadLine();
            }

            response.Close();
            streamReader.Close();

            for (int i = 0; i <= contents.Count - 1; i++)
            {
                if (contents[i].Contains("."))
                {
                    result.Add(_FTP_Server + contents[i]);
                }
                else
                {
                    //its a folder
                    FTP list = new FTP(_FTP_Server + contents[i] + "/", _FTP_User, _FTP_Password);
                    result.AddRange(list.Get_List());                }
            }

            return result;
        }

        public void Download(string file, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            WebClient ftpClient = new WebClient();
            ftpClient.Credentials = new System.Net.NetworkCredential(_FTP_User, _FTP_Password);
            ftpClient.DownloadFile(file, destination);
        }

        public void Delete(string file)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(file);
            ftpRequest.Credentials = new NetworkCredential(_FTP_User, _FTP_Password);
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
        }      
    }
}
