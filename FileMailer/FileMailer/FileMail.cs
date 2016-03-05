using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Threading;


namespace FileMailer
{
    public class FileMail
    {
        public string Path;
        public string FileFilter;
        public string MailSMTPServer;
        public int MailPort;
        public string MailSMTPUsername;
        public string MailSMTPPassword;
        public string MailFrom;
        public string MailTo;
        public string MailSubject;
        public string MailBody;

        private FileSystemWatcher watcher;


        public FileMail()
        {
            watcher = new FileSystemWatcher();
        }

        public void Start()
        {
            watcher.Path = Path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            watcher.Filter = FileFilter;
            watcher.Changed += new FileSystemEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;
        }
        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //bool prevEnabled = watcher.EnableRaisingEvents;
            //watcher.EnableRaisingEvents = false;

            //try
            //{
                Console.WriteLine("File Changed " + e.FullPath);
                Thread.Sleep(1000);

                SmtpClient smtpClient = new SmtpClient();
                NetworkCredential basicCredential = new NetworkCredential(MailSMTPUsername, MailSMTPPassword);
                MailMessage message = new MailMessage();

                smtpClient.Host = MailSMTPServer;
                smtpClient.Port = MailPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = basicCredential;
                smtpClient.Timeout = (60 * 5 * 1000);

                message.From = new MailAddress(MailFrom);
                message.Subject = MailSubject;
                message.IsBodyHtml = false;
                message.Body = MailBody;
                message.To.Add(new MailAddress(MailTo));

                try
                {
                    Console.WriteLine("Sending file to " + MailTo);
                    message.Attachments.Add(new Attachment(e.FullPath));
                    smtpClient.Send(message);
                    Console.WriteLine("File Sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                smtpClient.Dispose();
                message.Dispose();
            //}
            //finally
            //{
            //    watcher.EnableRaisingEvents = prevEnabled;
            //}
        }
    }
}
