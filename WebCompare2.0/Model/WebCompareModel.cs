using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WebCompare2_0.Model
{
    public class WebCompareModel
    {
        // Websites
        private static string[] websites = {   "https://stocktwits.com/symbol/DOW",
      "https://stocktwits.com/symbol/SPX", "https://stocktwits.com/symbol/GOOG",
      "https://stocktwits.com/symbol/AAPL", "https://stocktwits.com/symbol/MSFT",
      "https://stocktwits.com/symbol/NVDA", "https://stocktwits.com/symbol/TWTR",
      "https://stocktwits.com/symbol/FB", "https://stocktwits.com/symbol/BBRY",
      "https://stocktwits.com/symbol/ORCL"   };

        public static string[] Websites
        {
            get
            {
                return websites;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Parse the data using regex and weighted delimiters
        /// </summary>
        /// <param name="data">data to parse</param>
        public static string[] Parser(string[] data)
        {
            if (data == null) return null;
            string[] output = null;
            try
            {
                string temp = "";

                //string regex = @"(body&quot;:&quot;).*(&quot;,&quot;links)";
                //string regexWiki = @"<p>\w+</p>";
                //string regexStock = "(?<=(body&quot;:&quot;)).*(?=(&quot;,&quot;links))";
                string regexStock2 = @"(?<=(body&quot;:&quot;))(\w|\d|\n|[().,\-:;@#$%^&*\[\]'+–/\/®°⁰!?{}|`~]| )+?(?=(&quot;,&quot;links))";
                //string regexStock3 = "(?<=(<ol class='stream-list.*&quot;body&quot;:&quot;))(\\w|\\d|\n|^quot|[().,\\-:;@#$%^&*\\[\\]'+–/\\/®°⁰!?{}|`~]| )+?(?=(&quot;,&quot;links))";

                // Start at slot 3, first 3 elements are meta data
                for (int s = 3; s < data.Length; ++s)
                {
                    var tempParsed = Regex.Matches(data[s], regexStock2, RegexOptions.Singleline);
                    foreach (var msg in tempParsed)
                    {
                        // append each message as a string 
                        temp += msg.ToString() + ' ';
                    }
                }

                // Remove random characters
                temp = new string(temp
                        .Where(x => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x) || x == '$')
                        .ToArray());
                // Split into array
                output = temp.Split(' ');
            }
            catch { }

            return output;
        }

        // Similarity Calculation
        // Build Vectors
        public static List<object>[] BuildVector(HTable tableA, HTable tableB)
        {
            // Guard
            if (tableA == null || tableB == null) return null;
            // New vector
            List<object>[] vector = new List<object>[3];
            vector[1] = new List<object>();
            vector[2] = new List<object>();
            try
            {
                // Get word lists together, remove duplicates
                var words = tableA.ToList().Union(tableB.ToList());
                // Sort words
                words = words.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
                // Add key words to the vector
                vector[0] = words.ToList<object>();
                // Add the frequencies to the vector
                foreach (string keyword in vector[0])
                {
                    vector[1].Add(tableA.GetValue(keyword));
                    vector[2].Add(tableB.GetValue(keyword));
                }
            }
            catch (Exception e) { Console.WriteLine("Error building vector: " + e); }

            return vector;

        }

        // Cosine Similarity
        public static double CosineSimilarity(List<object>[] vector)
        {
            // convert lists to double arrays
            double[] tableA = vector[1].Select(item => Convert.ToDouble(item)).ToArray();
            double[] tableB = vector[2].Select(item => Convert.ToDouble(item)).ToArray();
            double dotProduct = 0.0, normA = 0.0, normB = 0.0;
            // calculate
            for (int i = 0; i < tableA.Length; i++)
            {
                dotProduct += tableA[i] * tableB[i];
                normA += Math.Pow(tableA[i], 2);
                normB += Math.Pow(tableB[i], 2);
            }
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));

        }
        #endregion
    }
}

/* Methods No longer used - Now use HTML Agility Pack instead
 * 
//public static string RandomWebsite
        //{
        //    get
        //    {
        //        return "https://en.wikipedia.org/wiki/Special:Random";
        //    }
        //}

/// <summary>
/// Get the data from a website as a string
/// </summary>
/// <param name="url">website to pull data from</param>
 public static string GetWebData(string url)
{

    try
    {
        using (WebClient client = new WebClient())
        {
            string s = client.DownloadString(url);
            return s;
        }
    }
    catch (Exception e)
    {
        MessageBox.Show("Exception caught: " + e, "Exception:Session:GetWebData()", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    return null;
}

       public static string GetWebData2(string url)
        {
            try
            {
                string line = "";
                string parsed = "";
                WebRequest webRequest;
                webRequest = WebRequest.Create(url);

                Stream objStream;
                objStream = webRequest.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                while (objReader != null)
                {
                    line = objReader.ReadLine();
                    parsed += Parser(line) + "\n";
                }
                return parsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception caught: " + e, "Exception:Session:GetWebData()", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return null;
        }*/

