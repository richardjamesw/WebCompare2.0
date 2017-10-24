using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;    //for iNotifyPropertyChanged
using System.Windows.Input; // ICommand
using WebCompare2_0.Model;
using System.Text.RegularExpressions;
using System.IO;
using HtmlAgilityPack;
using System.Windows.Threading;
using Prism.Commands;

namespace WebCompare2_0.ViewModel
{
    public class Session
    {
        #region Instance Variables & Constructor
        public readonly BackgroundWorker worker = new BackgroundWorker();
        private WebCompareViewModel wcViewModel = WebCompareViewModel.Instance;
        HTable[] tables = new HTable[WebCompareModel.Websites.Length + 1];

        private static object lockObj = new object();
        private static volatile Session instance;
        public static Session Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new Session();
                    }
                }
                return instance;
            }
        }

        public Session()
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }
        #endregion

        #region Session

        /// <summary>
        /// Start method
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            worker.RunWorkerAsync();
        }

        public bool CanStart()
        {
            return !Session.Instance.worker.IsBusy;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            // Setup table for user entered url
            for (int t = 0; t < tables.Length; ++t)
            {
                tables[t] = new HTable();
                if (t < WebCompareModel.Websites.Length)
                    tables[t].URL = WebCompareModel.Websites[t];
                else
                    tables[t].URL = wcViewModel.UserURL;
            }

            // if nothings entered in url return
            if (wcViewModel.UserURL == "")
            {
                AddMessage("\nPlease enter a valid URL");
                return;
            }

            //string[] test = GetWebDataAgility(wcViewModel.UserURL);
            AddMessage("");
            string[] data = null;
            string[] parsedMessages = null;

            // Get Data from Websites
            for (int w = 0; w <= WebCompareModel.Websites.Length; ++w)
            {
                if (w != WebCompareModel.Websites.Length)
                {
                    // Get data
                    AddMessage("\nGETTING data from: " + WebCompareModel.Websites[w]);
                    data = GetWebDataAgility(WebCompareModel.Websites[w]);

                    // Parse each message into
                    AddMessage("\nPARSING data from: " + WebCompareModel.Websites[w]);
                    parsedMessages = WebCompareModel.Parser(data);

                    // Fill respective table
                    AddMessage("\nFILLING TABLE from: " + WebCompareModel.Websites[w] + "\n");
                    FillTables(data, parsedMessages, w);

                }
                else   // We are at the last table, aka the User entered table
                {
                    // Get data
                    AddMessage("\nGETTING data from USER entered webpage");
                    data = GetWebDataAgility(wcViewModel.UserURL);

                    // Parse each message into
                    AddMessage("\nPARSING data from USER entered webpage");
                    parsedMessages = WebCompareModel.Parser(data);

                    // Fill respective
                    AddMessage("\nFILLING TABLE from USER entered webpage\n");
                    FillTables(data, parsedMessages, w);
                }

            }    // End get data from websites

            // Calculate cosine vectors
            AddMessage("\nCALCULATING cosine vectors\n");

            for (int tab = 0; tab < tables.Length - 1; ++tab)
            {
                // get vector, last table is the user entered table
                List<object>[] vector = WebCompareModel.BuildVector(tables[tab], tables[tables.Length - 1]);
                // Calcualte similarity
                tables[tab].Similarity = WebCompareModel.CosineSimilarity(vector);
            }

            // Compare to the entered URL by the user
            //     and display results in order
            wcViewModel.Results = GetResults();
        }

        private void worker_RunWorkerCompleted(object sender,
                                                 RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            //wcViewModel.StartCommand.RaiseCanExecuteChanged();

        }

        #endregion

        #region Helper Files

        //Send message to GUI
        private void AddMessage(string s)
        {
            if (s.Equals("")) wcViewModel.Status = "";
            else wcViewModel.Status += s;
        }

        // HTMPAglityPack Get
        private static string[] GetWebDataAgility(string url)
        {
            string[] data = null;

            try
            {
                var webGet = new HtmlWeb();
                var doc = webGet.Load(url);
                string exch = "", price = "", chng = "";

                // Exchange
                var node = doc.DocumentNode.SelectSingleNode("//span[@class='exchange']");
                if (node != null) exch = node.InnerText;
                // Price
                node = doc.DocumentNode.SelectSingleNode("//span[@class='price']");
                if (node != null) price = node.InnerText;
                // Change
                chng = doc.DocumentNode.SelectSingleNode("//span[@class='change positive']").Attributes.FirstOrDefault().Value;
                if (chng == "")
                {
                    chng = "negative";
                }
                else
                {
                    chng = "positive";
                }


                // Get messages
                var nodes = doc.DocumentNode.SelectNodes("//*[@id=\"updates\"]//li");
                data = new string[nodes.Count() + 3];   // Add 3 for class, exchange and price
                data[0] = exch;
                data[1] = price;
                data[2] = chng;
                int i = 3;
                foreach (var n in nodes)
                {
                    data[i] = n.GetAttributeValue("data-src", null);
                    ++i;
                }

            }
            catch (Exception e) { Console.WriteLine("Error in GetWebDataAgility(): " + e); }

            return data;
        }

        // Put keywords into tables with Weighted values
        private void FillTables(string[] metaData, string[] messages, int tableNum)
        {
            try
            {
                // parsedMessages doesn't contain meta data
                // data[0] is Exchange
                if (metaData[0] != "")
                {
                    tables[tableNum].Put(metaData[0], 50);
                }
                // data[1] is Price
                double price = 0;
                try { price = Convert.ToDouble(metaData[1]); } catch { }
                if (0 < price && price <= 10)
                {
                    tables[tableNum].Put("CheapStock", 70);
                }
                else if (10 < price && price <= 100)
                {
                    tables[tableNum].Put("MediumStock", 70);
                }
                else if (100 < price && price <= 300)
                {
                    tables[tableNum].Put("MediumHighStock", 70);
                }
                else if (300 < price && price <= 700)
                {
                    tables[tableNum].Put("HighStock", 70);
                }
                else if (price > 700)
                {
                    tables[tableNum].Put("ExpensiveStock", 70);
                }
                // data[1] is Change neg/pos
                tables[tableNum].Put(metaData[2], 100);

                // Add words from messages to table
                int val = 0;
                if (messages != null)
                {
                    foreach (string word in messages.Where(x => x != ""))
                    {
                        if (word.Equals("bull") || word.Equals("bear") || word.Equals("bullish") || word.Equals("bearish"))
                            val += 40;
                        if (word.Contains("$"))
                            val += 25;
                        tables[tableNum].Put(word, val);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("Error in FillTable(): " + e); }
        }

        private string GetResults()
        {
            string results = "";
            HTable temp;
            try
            {
                // Sort similarities (bubble sort)
                for (int index = 0; index < tables.Length - 1; ++index)
                    for (int scan = index + 1; scan < tables.Length - 1; scan++)
                        if (tables[index].Similarity < tables[scan].Similarity)
                        {
                            //Swap the values
                            temp = tables[index];
                            tables[index] = tables[scan];
                            tables[scan] = temp;
                        }

                // Set string of tables ordered by similarity
                for (int t = 0; t < tables.Length - 1; ++t)
                {
                    results += tables[t].URL + ":     \t" + String.Format("{0:0.00}", tables[t].Similarity) + "\n";
                }
            }
            catch (Exception e) { Console.WriteLine("Error in FillTable(): " + e); }
            return results;
        }

        #endregion



    }// End Session class
}// Namespace




/* tests
 //// test

    //https://twitter.com/StockTwits
            System.IO.File.WriteAllText(@"TestText.html", "");
            string data = WebCompareModel.GetWebData("https://en.wikipedia.org/wiki/Buffalo_Bills");
            System.IO.File.AppendAllText(@"TestText.html", data);


            //System.IO.File.WriteAllText(@"TestText.html", "");
            //data = WebCompareModel.GetWebData(wcViewModel.UserURL);
            //System.IO.File.AppendAllText(@"TestText.html", data);
            //parsedData = WebCompareModel.Parser(data);



    // Title
    // get messages //*[@id="updates"]/li[1]
                //HtmlNode node = doc.DocumentNode.SelectSingleNode("//*[@id=\"updates\"]");

                //HtmlNode node = doc.DocumentNode.SelectSingleNode("//*[@id=\"updates\"]//li[1]");
                //if (node != null) data += node.OuterHtml;

                //node = doc.DocumentNode.SelectSingleNode("//title");
                node = doc.DocumentNode.SelectSingleNode("//*[@id=\"updates\"]");
                if (node != null) data += node.InnerHtml;
                // Messages
                while (node.HasChildNodes)
                {
                    data += "/n" + node.A;
                }

                //HtmlNodeCollection nodes = node.ChildAttributes.Where(["class"].Value = "box"); //= doc.DocumentNode.SelectNodes("//*[@id=\"updates\"]").Where(x => Attributes["class"].Value == "box");

                if (nodes != null)
                {
                    foreach (var x in nodes)
                    {
                        data += "/n" + x.InnerHtml;
                    }
                }
                //.Where(x => x.Attributes["class"].Value == "box"))
 */
