using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebCompare2_0.Model;

namespace WebCompare2_0.ViewModel
{
    class LoaderViewModel : INotifyPropertyChanged
    {

        #region Instance Variables

        public readonly BackgroundWorker loadWorker = new BackgroundWorker();
        public readonly BackgroundWorker startWorker = new BackgroundWorker();
        private static object lockObj = new object();
        private static volatile LoaderViewModel instance;


        public static LoaderViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new LoaderViewModel();
                    }
                }
                return instance;
            }
        }

        public LoaderViewModel()
        {
            loadWorker.DoWork += loadWorker_DoWork;
            loadWorker.RunWorkerCompleted += loadWorker_RunWorkerCompleted;
            LoadCommand = new DelegateCommand(OnLoad, CanLoad);

            startWorker.DoWork += startWorker_DoWork;
            startWorker.RunWorkerCompleted += startWorker_RunWorkerCompleted;
            StartCommand = new DelegateCommand(OnStart, CanStart);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(str));
            }
        }
        #endregion

        #region Properties

        private string loadStatus = "Status...................";
        public string LoadStatus
        {
            get
            {
                return loadStatus;
            }
            set
            {
                loadStatus = value;
                NotifyPropertyChanged("LoadStatus");
            }
        }

        private bool updateIsChecked = true;
        public bool UpdateIsChecked
        {
            get
            {
                return updateIsChecked;
            }
            set
            {
                updateIsChecked = value;
                StartCommand.RaiseCanExecuteChanged();
                LoadCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged("UpdateIsChecked");
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command for the Load button in Loader Window
        /// </summary>
        public DelegateCommand LoadCommand { get; private set; }
        private void OnLoad()
        {
            NotifyPropertyChanged("LoadStatus");
            loadWorker.RunWorkerAsync();
            LoadCommand.RaiseCanExecuteChanged();
        }
        private bool CanLoad()
        {
            // Only allow button available if Update is checked and LoadWorker is not running
            return !loadWorker.IsBusy && UpdateIsChecked;
        }

        /// <summary>
        /// Command for the Start button in Loader Window
        /// </summary>
        public DelegateCommand StartCommand { get; private set; }
        private void OnStart()
        {
            startWorker.RunWorkerAsync();
            StartCommand.RaiseCanExecuteChanged();
        }
        private bool CanStart()
        {
            return !UpdateIsChecked;
        }

        #endregion

        #region Workers

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Create new tree
            string[] parsedData = null;
            int TableNumber = 0;
            try
            {
                // Get list of websites
                string[][] AllSites = new string[5][];
                for (int i = 0; i < AllSites.Length; ++i)
                {
                    AddMessage($"Getting site list for '{Enum.GetName(typeof (WebCompareModel.SitesEnum), i)}' cluster");
                    AllSites[i] = WebCompareModel.GetSiteList(WebCompareModel.Websites[i]);
                }

                // Build frequency tables from 1000 sites
                foreach (string[] sites in AllSites)
                {
                    AddMessage("Building frequency tables..");
                    foreach (string site in sites)
                    {
                        // Get data from website and parse
                        parsedData = WebCompareModel.GetWebDataAgility(site);
                        // Fill a new HTable (frequency table)
                        HTable table = new HTable();
                        table.URL = site;
                        table.Name = site.Substring(30);
                        for (int w = 0; w < parsedData.Length; ++w)
                        {
                            table.Put(parsedData[w], 1);
                        }
                        // Write HTable to file
                        table.SaveTable(TableNumber);
                        // Add HTable to BTree, including write to file
                        Session.Instance.Tree.Insert(TableNumber, table.Name);
                        ++TableNumber;
                    }
                    AddMessage("Completed building frequency tables..");
                } // End AllSites foreach
            } catch (Exception err) { MessageBox.Show("Exception caught: " + err, "Exception:Loader:loaderWorker_DoWork()", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void loadWorker_RunWorkerCompleted(object sender,
                                                 RunWorkerCompletedEventArgs e)
        {
            AddMessage("LOAD COMPLETED.");
            // Tell GUI everything is done updating
            UpdateIsChecked = false;
            StartCommand.RaiseCanExecuteChanged();
            LoadCommand.RaiseCanExecuteChanged();
            // ENABLE the start button
            StartCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Worker method for start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MainWindow mw = new MainWindow();
                mw.DataContext = WebCompareViewModel.Instance;
                mw.Show();
                // App.Instance.CloseLoader();
            } catch (Exception err) { Console.WriteLine("Error:startWorker_DoWork: " + err); }
        }

        private void startWorker_RunWorkerCompleted(object sender,
                                                 RunWorkerCompletedEventArgs e)
        {
            startWorker.Dispose();
        }

        #endregion

        #region HelperMethods

        //Send message to GUI
        public void AddMessage(string s)
        {
            LoadStatus += "\n" + s;
        }


        #endregion


    }
}
