using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Prospector
{
    public static class ErrorLogger
    {
        /// <summary>
        /// Write the exception and time it occured to the logfile
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public static void Log(Exception ex)
        {
            WriteEvent(ex);
        }
        
        /// <summary>
        /// Write the exception, time it occured and responsible crawl URL to the logfile
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        /// <param name="url">The URL of the site that caused the exception</param>
        public static void Log(Exception ex, string url)
        {
            WriteEvent(ex, url);
        }

        // Using overloads here instead of making the optional param public. I feel it provides
        // better info to IntelliSense
        private static void WriteEvent(Exception ex, string url = "")
        {
            try
            {
                FileStream stream = File.OpenWrite("log.csv");
                StreamWriter writer = new StreamWriter(stream);

                string[] data = { DateTime.Now.ToString(), ex.GetType().ToString(), url };
                string line = String.Join(",", data) + Environment.NewLine;

                writer.Write(line);
                writer.Close();
                stream.Close();
            }
            catch (Exception ex2)
            {
                // we're through the looking glass here, people...
            }
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class BrowshotException : Exception
    {
        public BrowshotException()
        {

        }
    }

    /// <summary>
    /// Each non-cached API call to retrieve a screenshot costs a credit. We have run out.
    /// </summary>
    public class BrowshotCreditsException : Exception
    {
        public BrowshotCreditsException()
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GooglingTooFrequentlyException : Exception
    {
        public GooglingTooFrequentlyException()
        {

        }
    }
}
