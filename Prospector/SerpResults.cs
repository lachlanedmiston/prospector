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
        public List<Site> priorSites;
        public string spamUrl;

        /// <summary>
        /// Creates a new SerpResults object
        /// </summary>
        /// <param name="priorSites"></param>
        public SerpResults(List<Site> priorSites)
        {
            web = new HtmlWeb();
            sites = new List<Site>();
            this.priorSites = priorSites;

            string accessKeyID = "AKIAJKIIPRINSIFBKMAQ";
            string secretKey = "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";
                        
            Amazon.Util.ProfileManager.RegisterProfile("Mail", accessKeyID, secretKey);
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

                    // ignore if the site was recorded in previous crawl
                    if (priorSites.Contains(newSite))
                    {
                        continue;
                    }

                    // finally, add if the site hasn't been seen *this* crawl
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
                    priorSites.Add(new Site(url, firstBad, 0, keyword, status.Clean));
                    return;
                }

                if (firstBad == "")
                {
                    firstBad = href;
                }
            }

            priorSites.Add(new Site(url, firstBad, 0, keyword, status.Prospect));
            sites.Add(new Site("http://" + url, keyword, firstBad));
        }

        /// <summary>
        /// Send a templated email to every 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="address"></param>
        /// <param name="url"></param>
        /// <param name="screenUrl"></param>
        /// <param name="keyword"></param>
        public void SendMail(string firstName, string address, string url, string screenUrl, string keyword )
        {
            string searchUrl = "https://www.google.com.au/search?q=site:" + url + " " + keyword;

            string subject = "Website security breach";
            string greeting = (firstName != "") ? "Hi " + firstName + "," : "Hi there,";
            string body =
                greeting + 
                "<br /><br />" +
                "I hate to be the bearer of bad news but your website - " + url + " - has been hacked. " +
                "The attackers have hidden an online store on the site which they are now using to sell illegal pharmaceuticals." +
                "<br /><br />" +
                "You can see a screenshot of the hidden store here: <a href='" + screenUrl + "'>link</a>. A quick google search " +
                "(<a href='" + searchUrl + "'>see here</a>) shows that a large number of pharmaceutical-related pages are now " +
                "appearing in the search results for your site." +
                "<br /><br />" +
                "I deal with problems like these on a regular basis. If you'd like me to resolve this issue for you, or you'd like to " +
                "talk more about it, please contact me at this email address. I'd be happy to help." +
                "<br /><br />" +
                "Regards," +
                "<br /><br />" +
                "Lachlan Edmiston.";

            Destination destination = new Destination();
            destination.ToAddresses = (new List<string>() { address });

            Content sub = new Content(subject);
            Body bod = new Body(new Content(body));

            Message email = new Message(sub, bod);

            SendEmailRequest request = new SendEmailRequest("lachlan.edmiston@gmail.com", destination, email);

            try
            {
                Amazon.Runtime.AWSCredentials credentials = new Amazon.Runtime.StoredProfileAWSCredentials("Mail");
                AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(credentials, Amazon.RegionEndpoint.USEast1);
                SendEmailResponse response = client.SendEmail(request);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, url);
            }
        }

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
        /// Load a previously saved session from file
        /// </summary>
        /// <param name="filename">Name of 'db' to access</param>
        /// <returns>The previous crawl DB</returns>
        public static List<Site> FromFile(string filename)
        {
            try
            {
                using (Stream stream = File.Open(filename, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    return (List<Site>)bin.Deserialize(stream);
                }
            }
            catch (IOException ex)
            {
                ErrorLogger.Log(ex, "DeSerialize error");
                return null;
            }
        }

        /// <summary>
        /// Write the current session to file
        /// </summary>
        /// <param name="sites">The current crawl DB</param>
        /// <param name="filename">Name of 'db' to write to</param>
        public static void ToFile(List<Site> sites, string filename)
        {
            try
            {
                using (Stream stream = File.Open(filename, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, sites);
                }
            }
            catch (IOException ex)
            {
                ErrorLogger.Log(ex, "Serialize error");
            }
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
