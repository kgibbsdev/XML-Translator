using CsvHelper;
using CsvHelper.Configuration;
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
        public string TagName { get; set; }
        public string Original { get; set; }
        public string Translated { get; set; }

        public Record()
        {
            //Empty Constructor Needed for CSVHelper
        }

        public Record(string _uid, string _fileName, string _tagName, string _original)
        {
            UID = _uid;
            FileName = _fileName;
            TagName = _tagName;
            Original = _original;
        }
    }

    public sealed class RecordMap : ClassMap<Record>
    {
        public RecordMap()
        {
            AutoMap(CultureInfo.InvariantCulture);

            //TODO: Change this to a variable
            Map(m => m.Translated).Name("Translated");
        }
    }

    class ReadFromFile
    {
        public static string ToBeTranslatedFilePath;
        public static string ToBeTranslatedDirectoryPath;
        public static string TranslatedDirectoryPath;

        static List<Record> recordList = new List<Record>();
        static List<Record> importedRecordsList = new List<Record>();
        public static void SetupDirectories(string path)
        {
            ToBeTranslatedDirectoryPath = path + @"CSVs\To Be Translated";
            ToBeTranslatedFilePath = path + @"CSVs\To Be Translated\" + "ToBeTranslated" + DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss") + ".csv";
            TranslatedDirectoryPath = path + @"CSVs\Translated\";

            if (!Directory.Exists(ToBeTranslatedDirectoryPath))
            {
                Print("Creating 'To Be Translated' directory.");
                Directory.CreateDirectory(ToBeTranslatedDirectoryPath);
            }
            else
            {
                Print("To Be Translated Directory already found.");
            }

            if (!Directory.Exists(TranslatedDirectoryPath))
            {
                Print("Creating 'Translated' directory.");
                Directory.CreateDirectory(TranslatedDirectoryPath);
            }
            else
            {
                Print("Translated Directory already found.");
            }

            if (!File.Exists(ToBeTranslatedFilePath))
            {
                Print("Creating CSV File.");
                var csvFile = File.Create(ToBeTranslatedFilePath);
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

            string textBreakTag = "text_break";

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
                    Record newRecord = new Record(textBreakUID, fileName, textBreakTag, textBreakContent);
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
                string hiddenMessageContent = eventOptionNodeList[i].FirstChild.NextSibling.InnerText;

                string optionTag = "option_text";
                string hiddenMessageTag = "hidden_message_text";
                
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
                    Record newRecord = new Record(eventOptionUID, fileName, optionTag, optionContent);
                    recordList.Add(newRecord);
                }

                if(hiddenMessageContent != "")
                {
                    Record newRecord = new Record(eventOptionUID, fileName, hiddenMessageTag, hiddenMessageContent);
                    recordList.Add(newRecord);
                }
            }
        }

        public static void WriteRecordsToCSV()
        {
            //
            using (var writer = new StreamWriter(ToBeTranslatedFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(recordList);
            }
        }

        public static void ReadRecordsFromCSV()
        {
            Print("Reading from CSV");
            string[] translatedFiles = System.IO.Directory.GetFiles(TranslatedDirectoryPath);

            foreach (string filePath in translatedFiles)
            {
                Print("FILEPATH: " + filePath);
                if (filePath.EndsWith(".csv"))
                {
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        //Register custom CSV mapping
                        csv.Configuration.RegisterClassMap<RecordMap>();
                        
                        //Set delimiter because these can differ per system
                        csv.Configuration.Delimiter = ",";

                        var recordsEnumerable = csv.GetRecords<Record>();

                        importedRecordsList = recordsEnumerable.ToList();
                    }
                }
            }
        }

        //Driver function
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

            SetupDirectories(GetFolderPath());

            //If the user presses the '1' key.
            if (option == ConsoleKey.D1)
            {
                string path = GetFolderPath();
                string[] files = System.IO.Directory.GetFiles(path);

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
            }

            //If the user presses the '2' key.
            else if (option == ConsoleKey.D2)
            {
                ReadRecordsFromCSV();

                string path = GetFolderPath();
                string[] files = System.IO.Directory.GetFiles(path);

                foreach (string filePath in files)
                {
                    if (filePath.EndsWith(".xml"))
                    {
                        XmlDocument document = new XmlDocument();
                        string text = File.ReadAllText(filePath);
                        string fileName = Path.GetFileName(filePath);
                        document.Load(filePath);

                        XmlNodeList uidNodeList;
                        XmlNode root = document.DocumentElement;
                        uidNodeList = document.GetElementsByTagName("uid");

                        foreach (Record r in importedRecordsList)
                        {
                            if (fileName == r.FileName)
                            {
                                for (int i = 0; i < uidNodeList.Count; i++)
                                {
                                    if (uidNodeList[i].InnerText == r.UID)
                                    {
                                        if (uidNodeList[i].ParentNode.Name.ToLower() == "text_break")
                                        {
                                            if (r.Translated != "")
                                            {
                                                uidNodeList[i].ParentNode.FirstChild.InnerText = r.Translated;
                                            }
                                            else
                                            {
                                                //do nothing
                                            }
                                            
                                        }
                                        else if (uidNodeList[i].ParentNode.Name.ToLower() == "event_option")
                                        {
                                            if(r.TagName == "option_text")
                                            {
                                                if(r.Translated != "")
                                                {
                                                    uidNodeList[i].ParentNode.FirstChild.InnerText = r.Translated;
                                                }
                                                else
                                                {
                                                    //do nothing
                                                }
                                                
                                            }
                                            else if(r.TagName == "hidden_message_text")
                                            {
                                                if(r.Translated != "")
                                                {
                                                    uidNodeList[i].ParentNode.FirstChild.NextSibling.InnerText = r.Translated;
                                                }
                                                else
                                                {
                                                    //do nothing
                                                }

                                            }
                                        }
                                    }
                                }

                                document.Save(filePath);
                            }
                        }
                    }
                }

            }

            Print("Done.");

            // Keep the console window open in debug mode.
            Console.ReadKey();
        }
    }
}
