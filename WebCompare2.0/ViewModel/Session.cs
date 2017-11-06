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
using System.Runtime.Serialization.Formatters.Binary;

namespace WebCompare2_0.ViewModel
{
    public class Session
    {
        #region Instance Variables & Constructor
        public readonly BackgroundWorker worker = new BackgroundWorker();
        private WebCompareViewModel wcViewModel = WebCompareViewModel.Instance;

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
        // public Btree
        private BTree tree = new BTree();
        public BTree Tree
        {
            get
            {
                return tree;
            }
            set
            {
                tree = value;
            }
        }
        /// <summary>
        /// Start method
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            worker.RunWorkerAsync();
            WebCompareViewModel.Instance.GoCommand.RaiseCanExecuteChanged();
        }

        public bool CanStart()
        {
            return !Instance.worker.IsBusy;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Build frequency table for user entered URL
            AddMessage("Building EnteredURL frequency table..");
            // Get data from websit and parse
            string[] parsedData = WebCompareModel.GetWebDataAgility(wcViewModel.UserURL);
            // Fill HTable
            HTable compareTable = new HTable();
            compareTable.URL = wcViewModel.UserURL;
            compareTable.Name = wcViewModel.UserURL.Substring(30);
            for (int w = 0; w < parsedData.Length; ++w)
            {
                compareTable.Put(parsedData[w], 1);
            }
            // Array of keyvalue pairs for the top 100 closest sites
            List<KeyValuePair<long, double>> topSites = null;
            // Foreach 1000 sites,
            AddMessage("Calculating Similarities for 1000 webistes..");
            for (int i = 0; i < 1000; ++i)
            {
                //// Build Vector
                HTable tempTable = LoadTable(i);
                List<object>[] vector = WebCompareModel.BuildVector(tempTable, compareTable);
                //// Calcualte similarity
                tempTable.Similarity = WebCompareModel.CosineSimilarity(vector);
                //// Update table
                tempTable.SaveTable(i);
                //// Maintain array of top 100 sites (IDs)
                topSites = AddTopSite(topSites, new KeyValuePair<long, double>(i, tempTable.Similarity));
                //// Check if the stored site needs updating
                DateTime compDate = DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0));
                if (tempTable.LastUpdated < compDate)
                {
                    //// Update the HTable
                    UpdateTable(ref tempTable);
                }

            }
            // For top 10 websites
            wcViewModel.Results = "Top 10 most similar: ";
            for (int i = 0; i < 10; ++i)
            {
                //// Get Name of site using Key
                //// Display 10 sites
                string siteName = Tree.SearchTree(topSites[i].Key, 0);
                wcViewModel.Results += "\n" + siteName;
            }

            // Calculate and Display most similar
            GetResult(topSites);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker completes his work
            wcViewModel.GoCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Send message to GUI
        /// </summary>
        /// <param name="s"></param>
        private void AddMessage(string s)
        {
            if (s.Equals("")) wcViewModel.Status = "";
            else wcViewModel.Status += s;
        }


        /// <summary>
        /// Get table from disk
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public HTable LoadTable(int num)
        {
            string FileName = $"tablebin\\table{num}.bin";
            HTable loadedTable = null;
            try
            {
                if (File.Exists(FileName))
                {
                    using (Stream filestream = File.OpenRead(FileName))
                    {
                        BinaryFormatter deserializer = new BinaryFormatter();
                        loadedTable = (HTable)deserializer.Deserialize(filestream);
                    }
                }
                else
                {
                    MessageBox.Show("Error", "Table does not exist", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception e) { Console.WriteLine("Error in LoadTable: " + e); }
            return loadedTable;
        }


        public void UpdateTable(ref HTable ht)
        {
            AddMessage("Updtating frequency table..");
            // Get data from websit and parse
            ht.Table = null;   // Clear current table
            ht.TableSize = 32;
            ht.Table = new Model.HTable.HEntry[32];
            string[] parsedData = WebCompareModel.GetWebDataAgility(ht.URL);
            // ReFill HTable
            for (int w = 0; w < parsedData.Length; ++w)
            {
                ht.Put(parsedData[w], 1);
            }

            AddMessage("COMPLETED updating frequency table..");
        }


        /// <summary>
        /// Kepp track of the most similar websites
        /// </summary>
        /// <param name="topSites"></param>
        /// <param name="kvp"></param>
        /// <returns></returns>
        private List<KeyValuePair<long, double>> AddTopSite(List<KeyValuePair<long, double>> topSites, KeyValuePair<long, double> kvp)
        {

            for (int c = 0; c < topSites.Count; ++c)
            {
                if (kvp.Value > topSites[c].Value)
                {
                    topSites.Insert(c, kvp);
                    // Only keep 100 sites
                    if (topSites.Count >= 100)
                        topSites.RemoveAt(99);   // Remove the last element
                }
                else if (topSites.Count < 100)
                {
                    topSites.Add(kvp);   // Add to end if list is not full
                }
            }


            return topSites;
        }


        /// <summary>
        /// Return results
        /// </summary>
        /// <returns></returns>
        private void GetResult(List<KeyValuePair<long, double>> topSites)
        {
            AddMessage("Getting Result Category");
            List<KeyValuePair<int, string>> results = new List<KeyValuePair<int, string>>();    // Array counting the most similar sites
            results.Add(new KeyValuePair<int, string>(0, "Sports"));
            results.Add(new KeyValuePair<int, string>(0, "Calculus"));
            results.Add(new KeyValuePair<int, string>(0, "Geography"));
            results.Add(new KeyValuePair<int, string>(0, "History"));
            results.Add(new KeyValuePair<int, string>(0, "Music"));
            try
            {
                for (int i = 0; i < topSites.Count; ++i)
                {
                    if (topSites[i].Key < 200)
                    {
                        results[0] = new KeyValuePair<int, string>(results[0].Key + 1, "Sports");
                    }
                    else if (topSites[i].Key >= 200 && topSites[i].Key < 400)
                    {
                        results[1] = new KeyValuePair<int, string>(results[1].Key + 1, "Calculus");
                    }
                    else if (topSites[i].Key >= 400 && topSites[i].Key < 600)
                    {
                        results[2] = new KeyValuePair<int, string>(results[2].Key + 1, "Geography");
                    }
                    else if (topSites[i].Key >= 600 && topSites[i].Key < 800)
                    {
                        results[3] = new KeyValuePair<int, string>(results[3].Key + 1, "History");
                    }
                    else if (topSites[i].Key >= 800)
                    {
                        results[4] = new KeyValuePair<int, string>(results[4].Key + 1, "Sports");
                    }    
                }   // End for

                results = results.OrderBy(o => o.Key).ToList();
                wcViewModel.Results += "MOST SIMILAR CATEGORY" + results[0].Value;

            }
            catch (Exception e) { Console.WriteLine("Error in GetResult(): " + e); }
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
