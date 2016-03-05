using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace FileMailer
{
    class Program
    {
        const string ConsoleDateFormat = "yyyy/MM/dd HH:mm:ss";
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToString(ConsoleDateFormat) + " Program Running");

            List<FileMail> FileMails = new List<FileMail>();

            //FileMail a = new FileMail();
            //a.Path = "C:\\temp\\";
            //a.FileFilter= "*.*";
            //a.MailSMTPServer = "smtp.1and1.com";
            //a.MailPort = 80;
            //a.MailSMTPUsername = "user";
            //a.MailSMTPPassword = "pass";
            //a.MailFrom = "from@d.com";
            //a.MailTo = "to@d.com";
            //a.MailSubject = "Subject";
            //a.MailBody = "Body";

            //FileMails.Add(a);
            //FileMailXML.SerializeObject(FileMails, @"FileMail.xml");


            try
            {
                FileMailXML.Deserialize(FileMails, @"FileMail.xml");

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString(ConsoleDateFormat) + " " + ex.Message);
            }

            foreach (FileMail filemail in FileMails)
            {
                filemail.Start();
                Console.WriteLine("Monitoring " + filemail.Path + " for files that match " + filemail.FileFilter);
            }

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }

    public static class FileMailXML
    {
        public static void SerializeObject(this List<FileMail> list, string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<FileMail>));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, list);
            }
        }

        public static void Deserialize(this List<FileMail> list, string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<FileMail>));
            using (var stream = File.OpenRead(fileName))
            {
                var other = (List<FileMail>)(serializer.Deserialize(stream));
                list.Clear();
                list.AddRange(other);
            }
        }
    }
}
