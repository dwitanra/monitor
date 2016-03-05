using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

namespace FileCopies
{
    class Program
    {
        const string ConsoleDateFormat = "yyyy/MM/dd HH:mm:ss";
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToString(ConsoleDateFormat) + " Program Running");

            List<FileCopy> FileCopies = new List<FileCopy>();
            //FileCopy a = new FileCopy();
            //a.FileSource = "src";
            //a.FileDestinations.Add("1");
            //a.FileDestinations.Add("2");
            //a.FileDestinations.Add("3");
            //a.DateFormat = "yyyymmdd";
            //a.IntervalMilliseconds = 1000;           
            //FileCopies.Add(a);
            //FileCopyXML.SerializeObject(FileCopies, @"FileCopies.xml");
            try
            {
                FileCopyXML.Deserialize(FileCopies, @"FileCopies.xml");

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString(ConsoleDateFormat) + " " + ex.Message);
            }

            foreach (FileCopy file in FileCopies)
            {
                file.Start();
            }

            while (true)
            {
                Thread.Sleep(1000);
                foreach (FileCopy file in FileCopies)
                {
                    if (file.Message != String.Empty)
                        Console.WriteLine(DateTime.Now.ToString(ConsoleDateFormat) + " " + file.Message);
                }
            }
        }      
    }

    public static class FileCopyXML
    {
        public static void SerializeObject(this List<FileCopy> list, string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<FileCopy>));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, list);
            }
        }

        public static void Deserialize(this List<FileCopy> list, string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<FileCopy>));
            using (var stream = File.OpenRead(fileName))
            {
                var other = (List<FileCopy>)(serializer.Deserialize(stream));
                list.Clear();
                list.AddRange(other);
            }
        }
    }
}
