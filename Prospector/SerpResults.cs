using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

using Browshot;
using HtmlAgilityPack;

namespace Prospector
{
    /// <summary>
    /// 
    /// </summary>
    public class SerpResults
    {
        private const string BrowshotAPIKey = "4E6vWgxOdFCAdJdxXOzDBKHBIRmOLhv";

        private HtmlWeb web;
        public List<Site> sites;
        public string spamUrl;

        /// <summary>
        /// Creates a new SerpResults object
        /// </summary>
        {
            web = new HtmlWeb();
            sites = new List<Site>();
        }

        /// <summary>
        /// Search for a keyword, storing X number of pages worth of site results
        /// </summary>
        /// <param name="query">The query to be run</param>
        /// <param name="keyword">The keyword you're searching for</param>
        /// <param name="pages">Number of results pages to crawl</param>
        public void DoSearch(string query, string keyword, int pages)
        {
            string completeQuery;

            for (int currentPage = 0; currentPage < pages; currentPage++)
            {
                Google.DelayToAvoidBan();

                // add pagination info to the query URL. eg:
                //   &num=100 is 100 results per page. 100 is ~max, though some spots may be lost to ads
                //   &start=0 would start at the 1st item of the 1st page, 101 would be 1st on 2nd page
                completeQuery = query + "&num=100&start=" + 100 * currentPage;

                HtmlDocument result = web.Load(completeQuery);

                // find and store each search result in the response. They're all in the format <li class="g">
                foreach (HtmlNode node in result.DocumentNode.SelectNodes("//li[@class='g']"))
                {
                    // for when the page doesn't match the expected format ie: google catches you crawling
                    if (node == null)
                    {
                        ErrorLogger.Log(new GooglingTooFrequentlyException());
                        break;
                    }
                    
                    string url = node.SelectSingleNode("div/div/cite").InnerHtml.Replace("https://", "").Split('/').First();

                    Site newSite = new Site("http://" + url, keyword);

                    if (!sites.Contains(newSite))
                    {
                        sites.Add(newSite);
                        newSite.IdentifyCMS();
                    }
                }
            }
        }

        /// <summary>
        /// Examines the serps of a given site for spammy pages
        /// </summary>
        /// <param name="query"></param>
        /// <param name="keyword"></param>
        /// <param name="url"></param>
        public void IsSpam(string query, string keyword, string url)
        {
            Google.DelayToAvoidBan();

            HtmlDocument result = web.Load(query + "&num=20");
            string firstBad = "";

            // SelectNodes() returns null when nothing is found instead of an empty list.
            foreach (HtmlNode node in result.DocumentNode.SelectNodes("//li[@class='g']") ??
                     new HtmlNodeCollection(null)) 
            {
                if (node == null)
                {
                    continue;
                }

                // grab elements from each search result
                string title = node.SelectSingleNode("h3/a").InnerHtml;
                string href = WebUtility.UrlDecode(node.SelectSingleNode("h3/a").GetAttributeValue("href", "")).Split('&').First().Replace("/url?q=", "");
                string snippet = node.SelectSingleNode("div/span").InnerHtml;

                // spam results repeat the keyword in the title and snippet. if a site has keyword serps that aren't spam then we aren't dealing with a 
                // compromised site.
                if (!title.ToLower().Contains(keyword.ToLower()) &&
                    !snippet.ToLower().Contains(keyword.ToLower()))
                {
                    return;
                }

                if (firstBad == "")
                {
                    firstBad = href;
                }
            }

            sites.Add(new Site("http://" + url, keyword, firstBad));
        }

        /// <summary>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public void GetScreenshots(bool cache = true)
        {
            BrowshotClient browshot = new BrowshotClient(BrowshotAPIKey);
            Dictionary<string, object> result;

            for(int i = sites.Count - 1; i >= 0; i--) 
            {
                Site site = sites[i];

                if (site.badURL == "")
                {
                    ErrorLogger.Log(new Exception(), site.url);
                    sites.RemoveAt(i);
                    continue;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site.badURL);
                request.AllowAutoRedirect = true;
                request.Referer = Google.Refer(site.badURL);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                string redirUrl = "";

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        redirUrl = response.ResponseUri.OriginalString;
                    }
                }
                catch (Exception e)
                {
                    ErrorLogger.Log(e, site.url);
                    sites.RemoveAt(i);
                    continue;
                }

                Hashtable arguments = new Hashtable();
                arguments.Add("instance_id", 64);
                arguments.Add("referer", Google.Refer(site.badURL));

                if (!cache)
                {
                    arguments.Add("cache", 0);
                }

                result = browshot.ScreenshotCreate(redirUrl, arguments);

                try
                {
                    site.browshotID = int.Parse(result["id"].ToString());
                }
                catch (Exception e)
                {
                    ErrorLogger.Log(new BrowshotCreditsException(), site.url);
                    sites.RemoveAt(i);
                }
            }

            for (int i = 0; i < sites.Count; i++)
            {
                if (sites[i].browshotID == 0)
                {
                    ErrorLogger.Log(new BrowshotException(), sites[i].url);
                    continue;
                }
                
                result = browshot.ScreenshotInfo(sites[i].browshotID);
                string status = result["status"].ToString();

                while (status != "finished" && status != "error")
                {
                    result = browshot.ScreenshotInfo(sites[i].browshotID);
                    status = result["status"].ToString();
                }

                if (status == "error")
                {
                    ErrorLogger.Log(new BrowshotException(), sites[i].url);
                    continue;
                }

                string imgLocation = result["screenshot_url"].ToString();
                string fileName = sites[i].url.Replace("http://", "").Replace("https://", "");
                WebClient webClient = new WebClient();
                webClient.DownloadFile(imgLocation, "img/" + fileName + ".png");
            }
        }

        /// <summary>
        /// Construct a web request which will pass for a legit request from Firefox
        /// </summary>
        {
        }

        /// <summary>
        /// Write the results out to a .csv file. At the moment, you have to manually check these in ms
        /// excel or similar. Some results may be false positives.
        /// </summary>
        public void ToCSV()
        {
            string csv = "";

            foreach (Site site in sites)
            {
                if (site.badURL == null)
                {
                    continue;
                }

                csv += site.url + ", " + site.badURL + Environment.NewLine;
                File.WriteAllText("out.csv", csv);
            }
        }
    }
}
