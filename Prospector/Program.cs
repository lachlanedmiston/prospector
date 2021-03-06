﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prospector
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintHeader();
            Prompt();

            string engine = "https://www.google.com.au/search?q=";
            string[] gTLD = { ".com.au" };
            string[] keywords = { "cialis" };
            string[] ignoreList = { "grapeshotmq.com.au", "www.betterhealthchannel.com.au", "www.impotenceaustralia.com.au", 
                                      "www.smh.com.au", "www.theage.com.au", "www.sexualhealthaustralia.com.au", "www.news.com.au" ,
                                      "www.pharmacydirect.com.au", "www.chemistwarehouse.com.au", "www.markwellgroup.com.au"};
            string modifiers = " -site:" + string.Join(" -site:", ignoreList);

            SerpResults serps = new SerpResults();
            SerpResults prospects = new SerpResults();

            // crawl all combinations of search criteria, record all sites in the results
            foreach (string locale in gTLD)
            {
                foreach (string keyword in keywords)
                {
                    // eg: 'www.google.com.au/search?q=site:.com.au cialis'
                    string query = engine + "site:" + locale + " " + keyword + " " + modifiers;
                    serps.DoSearch(query, keyword, 1);
                }
            }

            // check all discovered sites for evidence of spam
            foreach (Site site in serps.sites)
            {
                // eg: 'www.google.com.au/search?q=site:news.com.au cialis'
                string query = engine + "site:" + site.url + " " + site.keyword;
                prospects.IsSpam(query, site.keyword, site.url.Replace("http://", ""));
            }

            prospects.GetScreenshots(false);
            prospects.WriteScreenshots();
            prospects.ToCSV();

            // end program, show the user the resulting files
            Process.Start("explorer.exe", "out");
        }

        /// <summary>
        /// WriteLine() but with word wrap at the max width of the console window
        /// </summary>
        /// <param name="line">The line to be writen</param>
        public static void WriteWrappedLine(string line)
        {
            if (line.Count() == 0)
            {
                throw new ArgumentNullException();
            }

            WrapRecurse(
                new List<string>(Regex.Split(line, "( )")),
                new List<string>(){ "" });
        }

        // only after writing this did I realise that the iterative version is much smaller/simpler. 
        private static void WrapRecurse(List<string> wordsToWrap, List<string> lines)
        {
            string currentWord = wordsToWrap.FirstOrDefault() ?? "";
            string currentLine = lines.LastOrDefault() ?? "";

            if (wordsToWrap.Count == 0)
            {
                Console.WriteLine(currentLine);
                return;
            }

            wordsToWrap.RemoveAt(0);

            // halt line -1 from max width because it looks nicer
            if (currentLine.Count() + currentWord.Count() > Console.BufferWidth - 1)
            {
                Console.WriteLine(currentLine);
                lines.Add(currentWord);
            }
            else
            {
                lines[lines.Count - 1] += currentWord;
            }

            WrapRecurse(wordsToWrap, lines);
        }

        /// <summary>
        /// Console.Write() in technicolor.
        /// </summary>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        /// <param name="text"></param>
        public static void WriteColor(ConsoleColor foreColor, ConsoleColor backColor, string text)
        {
            ConsoleColor oldForeColor = Console.ForegroundColor;
            ConsoleColor oldBackColor = Console.BackgroundColor;

            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
            Console.Write(text);

            // return to default
            Console.ForegroundColor = oldForeColor;
            Console.BackgroundColor = oldBackColor;
        }

        public static void PrintHeader()
        {
            Console.WriteLine();

            string line1 =
                "This is a demonstration of a web crawler that identifies hacked sites that are unknowingly serving " +
                "spam. In this example, the program will find sites which sell the prescription medication Cialis. " +
                "The crawler can target any keyword or combination of keywords, as well as filter by gTLD.";

            WriteWrappedLine(line1);

            Console.WriteLine();

            string line2 =
                "Positive results are indicated by the repetitive occurrence of the keyword in many successive search "+
                "engine results. For example, a site which has 'Cialis' in every title, URL and snippet accross 20+ " +
                "pages is 95% of the time a hacked site. ";

            WriteWrappedLine(line2);

            Console.WriteLine();

            string line3 =
                "The demonstration will take about 5 minutes to complete and will output a .csv file and a number " +
                "of .pngs. These files will be displayed in windows explorer after execution. Please close the " +
                "window now if you do not wish to run the program, ";

            WriteWrappedLine(line3);

            Console.WriteLine();
        }

        public static void Prompt()
        {
            Console.Write("Hit ");
            WriteColor(ConsoleColor.Black, ConsoleColor.White, " enter ");
            Console.Write(" to continue.");
            Console.Read();
        }
    }
}
