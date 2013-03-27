using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using log4net;

namespace HAOest.ListManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SplashScreen s = new SplashScreen("/Resources/SplashScreenImage.png");
            s.Show(false);
            s.Close(new TimeSpan(0, 0, 5));
        }
        public static ILog writeLog = log4net.LogManager.GetLogger(typeof(MainWindow));
    }

}
