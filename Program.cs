using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace DotNetXMLConverter
{
    class ReadFromFile
    {
        public static void LineBreak(int breaks = 1)
        {
            for (int i = 0; i <= breaks; i++)
            {
                Console.WriteLine('\n');
            }
        }

        public static void Print(string text)
        {
            Console.WriteLine(text);
        }

        static void Main()
        {

            string path = System.IO.Directory.GetCurrentDirectory();
            path = Path.Combine(path, "..", "..", "..", "..");
            Print(path);
            string[] files = System.IO.Directory.GetFiles(path);

            //Console.WriteLine(path);

            foreach(string filePath in files)
            {
                if (filePath.EndsWith(".xml"))
                {
                    XmlDocument document = new XmlDocument();
                    string text = File.ReadAllText(filePath);

                    Print(filePath);
                    
                    Print(String.Concat("BEGIN READ OF  ", filePath));
                    LineBreak();
                    //Print(text);
                    document.Load(filePath);
                    
                    XmlNodeList nodeList;
                    XmlNode root = document.DocumentElement;
                    nodeList = document.GetElementsByTagName("text_break");
                    
                    for(int j = 0; j < nodeList.Count; j++)
                    {
                        Print(nodeList[j].FirstChild.InnerText);
                        //Skip to the last node within the Text_Break tag
                        XmlNode lastChild = nodeList[j].LastChild;
                        
                        //The last node within a Text_Break will always be the soundeffect tag, and the previous sibling of the sound effect
                        //node is the UID tag for the text break.
                        string TextBreakUID = lastChild.PreviousSibling.InnerText;

                        Print(TextBreakUID);
                        LineBreak();
                    }

                    Print(String.Concat("END READ OF  ", filePath));
                    LineBreak();
                }
            }

            // Keep the console window open in debug mode.

            Console.WriteLine("Done.");
            System.Console.ReadKey();
        }
    }
}
