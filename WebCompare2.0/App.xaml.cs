using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WebCompare2._0.View;

namespace WebCompare2_0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static object lockObj = new object();
        private static volatile App instance;
        public static App Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new App();
                    }
                }
                return instance;
            }
        }

        Loader lw = new Loader();

        /// <summary>
        /// Display the Main Window
        /// </summary>
        private void AppStartup(object sender, StartupEventArgs e)
        {
            lw.DataContext = ViewModel.LoaderViewModel.Instance;
            lw.Show();
        }

        public void CloseLoader()
        {
            lw.Close();
        }
        /// <summary>
        /// Global exception handler, used as fallback when local exceptions are missed
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "DispatcherUnhandledException", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }


    }
}
