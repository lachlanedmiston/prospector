using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Prospector
{
    /// <summary>
    /// Utility methods for sending HTTP requests to or from Google's search pages
    /// </summary>
    public static class Google
    {
        /// <summary>
        /// Creates a HTTP referal header for a specified link, making it look like you've come from a Google search 
        /// page. Some of the cleverer spam will only appear when you arrive straight from the serps.
        /// </summary>
        /// <param name="link">The page you would like to visit, 'via Google'</param>
        /// <returns>A dummy HTTP referer field</returns>
        public static string Refer(string link)
        {
            return "http://www.google.com.au/url?sa=t&rct=j&q=&esrc=s&source=web&cd=2&ved=0CCoQFjAB&url=" +
                WebUtility.UrlEncode(link) + "&ei=tOPJVKz3EYOY8QXdmYHYAg&usg=AFQjCNHcqrnJIXBy8LctY2VlIPYzYVAJWQ";
        }

        /// <summary>
        /// Pause for a short time in between requests to Google. If you don't do this they'll get stroppy and reply
        /// with 'suspicious traffic' warnings instead of the page you want. 
        /// </summary>
        public static void DelayToAvoidBan()
        {
            System.Threading.Thread.Sleep(30000);
        }
    }
}
