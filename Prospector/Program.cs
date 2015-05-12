using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prospector
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintHeader();

            string engine = "https://www.google.com.au/search?q=";
            {
                foreach (string keyword in keywords)
                {
                    serps.DoSearch(engine + locale + " " + keyword + " " + modifiers, keyword, 4);
                }
            }

            foreach (Site site in serps.sites)
            {
                prospects.IsSpam(engine + "site:" + site.url + " " + site.keyword, site.keyword, site.url.Replace("http://", ""));
            }

            prospects.GetScreenshots(false);
            prospects.ToCSV();
        }

        public static void PrintHeader()
        {
            Console.WriteLine();
            WriteColor(ConsoleColor.Green, "Prospecter interactive shell");
            WriteColor(ConsoleColor.Yellow, " ~");
            Console.WriteLine();
            Console.WriteLine(" type 'help' for options");
            Console.WriteLine();
        }

        public static void WriteColor(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);

            // return to default
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string Prompt()
        {
            Console.Write("$ ");
            string input = Console.ReadLine();
            Console.WriteLine();
            return input;
        }
    }
}
