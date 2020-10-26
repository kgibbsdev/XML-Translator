using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace DotNetXMLConverter
{
    public class Record
    {
        public string UID { get; set; }
        public string OriginalText { get; set; }
        //TODO: Decide if there should be two separate classes for translated/non-translated records
        public string TranslatedText { get; set; }

        public Record(string uid, string originalText, string translatedText = "") 
        {
            UID = uid;
            OriginalText = originalText;
            TranslatedText = translatedText; 
        }
    }

    class ReadFromFile
    {
        //Convenience function that creates a line break.
        public static void LineBreak(int breaks = 1)
        {
            for (int i = 0; i <= breaks; i++)
            {
                Console.WriteLine('\n');
            }
        }

        //Convenience function that logs a string to the console.
        public static void Print(string text)
        {
            Console.WriteLine(text);
        }

        public static string GetFolderPath()
        {
            //Gets the path of the .exe file
            string path = System.IO.Directory.GetCurrentDirectory();

            //Get the parent folder that contains the project.
            path = Path.Combine(path, "..", "..", "..", "..");
            Print("Set path to: " + path);
            return path;
        }

        public static void GetTextBreakContent(XmlDocument document)
        {
            XmlNodeList textBreakNodeList;
            XmlNode root = document.DocumentElement;
            textBreakNodeList = document.GetElementsByTagName("text_break");

            for (int j = 0; j < textBreakNodeList.Count; j++)
            {
                Print(textBreakNodeList[j].FirstChild.InnerText);
                //Skip to the last node within the Text_Break tag
                XmlNode lastChild = textBreakNodeList[j].LastChild;
                Print(lastChild.InnerText);

                //The last node within a Text_Break will always be the soundeffect tag, and the previous sibling of the sound effect
                //node is the UID tag for the text break.
                string TextBreakUID = lastChild.PreviousSibling.InnerText;

                Print(TextBreakUID);
                LineBreak();
            }
        }

        public static void GetEventOptionContent(XmlDocument document)
        {
            XmlNodeList eventOptionNodeList;
            XmlNode root = document.DocumentElement;
            eventOptionNodeList = document.GetElementsByTagName("event_option");

            for (int i = 0; i < eventOptionNodeList.Count; i++)
            {
                string optionText = eventOptionNodeList[i].FirstChild.InnerText;
                string eventOptionUID = eventOptionNodeList[i].LastChild.InnerText;
            }
        }

        static void Main()
        {
           string path = GetFolderPath();
           string[] files = System.IO.Directory.GetFiles(path);

            //Console.WriteLine(path);

            foreach(string filePath in files)
            {
                if (filePath.EndsWith(".xml"))
                {
                    XmlDocument document = new XmlDocument();
                    string text = File.ReadAllText(filePath);

                    Print(filePath);

                    Print("BEGIN READ OF  " + filePath);
                    LineBreak();
                    document.Load(filePath);

                    GetTextBreakContent(document);
                    GetEventOptionContent(document);

                    Print("END READ OF  " + filePath);
                    LineBreak();
                }
            }

            // Keep the console window open in debug mode.
            Console.WriteLine("Done.");
            System.Console.ReadKey();
        }
    }
}
