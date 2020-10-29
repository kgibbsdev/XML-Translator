using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;

namespace DotNetXMLConverter
{
    class Convenience
    {
        //Convenience function that creates a line break.
        public static void LineBreak(int breaks = 1)
        {
            for (int i = 0; i < breaks; i++)
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
            path = Path.Combine(path, "..", "..", "..", @"..\");
            Print("Set path to: " + path);
            return path;
        }

        public static void PrintInstructions()
        {
            Print("Press 1 to generate a CSV file for translation.");
            Print("Press 2 to read an already-translated CSV file.");
        }
    }
}
