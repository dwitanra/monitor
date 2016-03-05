using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Xml.Serialization;

namespace FileCopies
{
    public class FileCopy
    {
        private Timer Timer;
        public int IntervalMilliseconds;
        public string DateFormat;
        public string TimeFormat;
        public string FileSource;
        public List<string> FileDestinations;
        [XmlIgnore]
        public string Message = String.Empty;

        public FileCopy()
        {
            FileDestinations = new List<string>();
            Timer = new Timer();
            Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent); 
        }

        public void Start()
        {
            if (Timer.Interval != IntervalMilliseconds)
            Timer.Interval = IntervalMilliseconds;
            Timer.Start();
        }

        public void Stop()
        {
            Timer.Stop();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            foreach (string str in FileDestinations)
            {
                try
                {
                    string filename = str;
                    filename = filename.Replace("[Date]",DateTime.Now.ToString(DateFormat));
                    filename = filename.Replace("[Time]",DateTime.Now.ToString(TimeFormat));
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    File.Copy(FileSource, filename);
                }
                catch (Exception ex)
                {
                    Message = FileSource + " " + ex.Message;
                }
            }
        }
    }
}
