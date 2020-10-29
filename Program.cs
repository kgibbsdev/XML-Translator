using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

//This is where the convenience functions LineBreak(), Print(), GetFolderPath(), and PrintInstructions() come from.
using static DotNetXMLConverter.Convenience;

namespace DotNetXMLConverter
{
    public class Record
    {
        public string UID { get; set; }
        public string FileName { get; set; }
        public string OriginalText { get; set; }
      

        public Record(string _uid,  string _fileName, string _originalText)
        {
            UID = _uid;
            FileName = _fileName;
            OriginalText = _originalText;
        }
    }

    class ReadFromFile
    {
        public static string csvFilePath;
        public static string csvDirectoryPath;

        static List<Record> recordList = new List<Record>();
        public static void CreateCSV(string path)
        {
            Print("path: " + path);
            Print("Checking " + path + @" for csvtest.csv.");

            csvDirectoryPath = path + @"CSVs";
            csvFilePath = path + @"\CSVs\csvtest.csv";

            if (!Directory.Exists(csvDirectoryPath))
            {
                Print("Creating CSV directory.");
                Directory.CreateDirectory(csvDirectoryPath);
            }
            else
            {
                Print("CSV Directory already found");
            }

            if (!File.Exists(path + @"\CSVs\csvtest.csv"))
            {
                Print("Creating CSV File.");
                var csvFile = File.Create(csvFilePath);
                csvFile.Close();
            }
            else
            {
                Print("CSV File found");
            }
        }

        //Gets all of the content between Text_Break tags
        public static void GetTextBreakContent(XmlDocument document, string fileName)
        {
            XmlNodeList textBreakNodeList;
            XmlNode root = document.DocumentElement;
            textBreakNodeList = document.GetElementsByTagName("text_break");

            for (int j = 0; j < textBreakNodeList.Count; j++)
            {
                //Skip to the last node within the Text_Break tag
                XmlNode firstChild = textBreakNodeList[j].FirstChild;
                XmlNode lastChild = textBreakNodeList[j].LastChild;
                string textBreakContent = firstChild.InnerText;

                //The last node within a Text_Break will always be the soundeffect tag, and the previous sibling of the sound effect
                //node is the UID tag for the text break.
                string textBreakUID = lastChild.PreviousSibling.InnerText;
                Print(fileName);
                //If the text_break does not have any content, skip adding it to the list of records.
                if (textBreakContent != "")
                {
                    Record newRecord = new Record(textBreakUID, fileName, textBreakContent);
                    recordList.Add(newRecord);
                }
            }
        }

        //Gets all of the content within Event_Option tags
        public static void GetEventOptionContent(XmlDocument document, string fileName)
        {
            XmlNodeList eventOptionNodeList;
            XmlNode root = document.DocumentElement;
            eventOptionNodeList = document.GetElementsByTagName("event_option");

            for (int i = 0; i < eventOptionNodeList.Count; i++)
            {
                string optionContent = eventOptionNodeList[i].FirstChild.InnerText;

                string eventOptionUID;
                //There is one case where the Last Child of the event_option tag is NOT a UID
                //Therefore, If the Lastchild is empty, check the previous sibling.
                if (eventOptionNodeList[i].LastChild.InnerText != "")
                {
                    eventOptionUID = eventOptionNodeList[i].LastChild.InnerText;
                }
                else
                {
                    eventOptionUID = eventOptionNodeList[i].LastChild.PreviousSibling.InnerText;
                }

                //If the option_text does not have any content, skip adding it to the list of records.
                if (optionContent != "")
                {
                    Record newRecord = new Record(eventOptionUID, fileName, optionContent );
                    recordList.Add(newRecord);
                }
            }
        }

        public static void WriteRecordsToCSV()
        {
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(recordList);
            }
        }

        static void Main()
        {
            PrintInstructions();

            ConsoleKey option = Console.ReadKey().Key;

            while (option != ConsoleKey.D1 && option != ConsoleKey.D2)
            {
                LineBreak();
                PrintInstructions();
                LineBreak();
                option = Console.ReadKey().Key;
            }

            string path = GetFolderPath();
            string[] files = System.IO.Directory.GetFiles(path);
            CreateCSV(GetFolderPath());

            foreach (string filePath in files)
            {
                if (filePath.EndsWith(".xml"))
                {
                    XmlDocument document = new XmlDocument();
                    string text = File.ReadAllText(filePath);
                    string fileName = Path.GetFileName(filePath);
                    document.Load(filePath);

                    GetTextBreakContent(document, fileName);
                    GetEventOptionContent(document, fileName);
                    WriteRecordsToCSV();
                }
            }

            Print("Done.");

            // Keep the console window open in debug mode.
            Console.ReadKey();
        }
    }
}
