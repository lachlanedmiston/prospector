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
            string input = Prompt();
            
            while (input != "exit")
            {
                input = Prompt();

                switch (input)
                {
                    case "help":
                        break;
                    default:
                        break;
                }
            }
            
            string engine = "https://www.google.com.au/search?q=";
            string[] locales = { "site:.com.au", "site:.org.au" };
            string[] keywords = { "viagra" };
            string[] ignoreList = { "grapeshotmq.com.au", "www.betterhealthchannel.com.au", "www.impotenceaustralia.com.au", "www.smh.com.au", "www.artscentremelbourne.com.au", "www.theage.com.au", 
                                      "www.sexualhealthaustralia.com.au", "www.theatreroyal.com.au", "www.news.com.au" };
            string modifiers = "";

            const string DB = "db.bin";

            List<Site> sites = new List<Site>();

            #region db init
            //string[] resolved = { "www.gptt.com.au", "www.markwellgroup.com.au", "www.southwestpersonnel.com.au", "www.ladswa.com.au", "www.primemedical.com.au", "paintplace.com.au", "fightclubjiujitsu.com.au", "www.mhwaq.org.au", "www.cafs.org.au", "enactusunimelb.org.au", "www.buderimfoundation.org.au", "www.regen.org.au", "thebegavalley.org.au", "colourfest.com.au", "ccae.com.au", "www.caledon.com.au", "www.shantellvineyard.com.au", "balmainsportsmed.com.au", "www.citsa.com.au", "www.winequip.com.au", "taswellbeing.com.au", "www.womhealth.org.au", "www.nunku.org.au", "cr.org.au", "www.pnhsaa.org.au", "sufc.org.au", "speldnsw.org.au", "www.ashburtonwsc.org.au", "permaculturecairns.org.au"};
            //string[] notFound = { "grapeshotmq.com.au", "www.betterhealthchannel.com.au", "www.impotenceaustralia.com.au", "www.smh.com.au", "www.artscentremelbourne.com.au", "www.theage.com.au", "www.sexualhealthaustralia.com.au", "www.theatreroyal.com.au", "www.news.com.au" };
            //string[] prospect = { "farmhousemeats.com.au", "www.7dcinema.com.au", "tgsm.com.au", "www.brisbanetruckshow.com.au", "www.cavestrength.com.au", "centroc.com.au", "visitwellington.com.au", "fedlaw.com.au", "www.chinalane.com.au", "www.mahoganymotel.com.au", "www.fireupcoaching.com.au", "www.wamediaawards.com.au", "www.barwoncoast.com.au", "onenaturaltherapies.com.au", "www.windsor-ic.com.au", "capitoltheatre.com.au", "www.ngarrindjeri.org.au", "achieveaustralia.org.au", "perthcityfarm.org.au", "polish-spk-foundation.org.au", "www.hcca.org.au", "www.qldworldaidsday.org.au", "www.hopefoundation.org.au", "www.sgbcllen.org.au", "anzshm.org.au" };

            //foreach (string url in resolved)
            //{
            //    sites.Add(new Site(url, "", 0, "viagra", status.Resolved));
            //}

            //foreach (string url in notFound)
            //{
            //    sites.Add(new Site(url, "", 0, "viagra", status.Clean));
            //}

            //foreach (string url in prospect)
            //{
            //    sites.Add(new Site(url, "", 0, "viagra", status.Prospect));
            //}

            //SerpResults.ToFile(sites, "db.bin");
            # endregion

            List<Site> priorSites = SerpResults.FromFile(DB);
            SerpResults serps = new SerpResults(priorSites);
            SerpResults prospects = new SerpResults(priorSites);

            # region tests
            ////prospects.SendMail("Test", "success@simulator.amazonses.com", "www.google.com", "http://thing.io/do?1", "viagra");
            //string[] sites = { "http://visitwellington.com.au", "http://fedlaw.com.au", 
            //                     "http://www.youngcarersouthtas.com.au", "http://wtfn.com.au", "http://www.shantellvineyard.com.au", "http://www.chinalane.com.au", "http://www.mahoganymotel.com.au", 
            //                     "http://www.fireupcoaching.com.au", "http://www.wamediaawards.com.au", "http://www.barwoncoast.com.au", "http://www.citsa.com.au", 
            //                     "http://onenaturaltherapies.com.au", "http://www.windsor-ic.com.au", "http://capitoltheatre.com.au", 
            //                     "http://www.ngarrindjeri.org.au", "http://achieveaustralia.org.au", "http://perthcityfarm.org.au",
            //                     "http://polish-spk-foundation.org.au", "http://www.hcca.org.au", "http://cr.org.au", "http://www.qldworldaidsday.org.au",
            //                     "http://www.ashburtonwsc.org.au", "http://www.hopefoundation.org.au", "http://www.sgbcllen.org.au", 
            //                     "http://anzshm.org.au" }; 

            //foreach (string site in sites)
            //{
            //    serps.sites.Add(new Site(site, "viagra"));
            //}
            # endregion

            foreach (string locale in locales)
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
            SerpResults.ToFile(prospects.sites, DB);
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
