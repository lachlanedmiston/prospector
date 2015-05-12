using System;
using System.Collections.Generic;
using System.IO;

using HtmlAgilityPack;

namespace Prospector
{
    public enum CMS
    {
        Wordpress,
        Joomla,
        Drupal,
        IIS,
        Magento,
        Unknown
    };

    public enum status
    {
        Clean,
        Prospect,
        Resolved
    };

    /// <summary>
    /// Represents all information knwon about a crawled website 
    /// </summary>
    public class Site : IEquatable<Site>
    {

        public string url { get; set; }
        public string badURL { get; set; }
        public int browshotID { get; set; }
        public string keyword { get; set; }
        public CMS cmsUsed { get; set; }
        public status siteStatus { get; set; }

        public HtmlDocument doc;

        /// <summary>
        /// Returns a new Site object
        /// </summary>
        /// <param name="url">The url of the website</param>
        /// <param name="keyword">The keyword that the site was ranking for</param>
        /// <param name="badURL">The first known instance of spam on the site. Optional.</param>
        public Site(string url, string keyword, string badURL = "")
        {
            this.url = url;
            this.keyword = keyword;
            this.cmsUsed = CMS.Unknown;
            this.badURL = badURL;
        }

        public bool Equals(Site comparer)
        {
            if (comparer.url == this.url)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stub: may use a service for this instead of writing my own identification profiles
        /// </summary>
        public void IdentifyCMS()
        {
            HtmlWeb web = new HtmlWeb();

            try
            {
                this.doc = web.Load(url);
                HtmlNode metaGen = doc.DocumentNode.SelectSingleNode("//meta[@name='generator']");
                string linkPath = "//link[contains(@href, '" + this.url + "') and @rel='stylesheet']";
                HtmlNode lnkStyle = doc.DocumentNode.SelectSingleNode(linkPath);

                if ((metaGen != null && metaGen.GetAttributeValue("content", "").ToLower().Contains("wordpress")) ||
                    (lnkStyle != null && lnkStyle.GetAttributeValue("href", "").ToLower().Contains("wp-content")))
                {
                    cmsUsed = CMS.Wordpress;
                }
                //else if (IsDrupal())
                //{
                //    return CMS.Drupal;
                //}
                //else if (IsJoomla())
                //{
                //    return CMS.Joomla;
                //}
                //else if (IsIIS())
                //{
                //    return CMS.IIS;
                //}
                //else if (IsMagento())
                //{
                //    return CMS.Magento;
                //}
            }
            catch (Exception ex)
            {
                // http redirects are handled as exceptions instead of intelligently - must fix
                ErrorLogger.Log(ex, url);
            }
        }
    }
}
