using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HAOest.ListManager.AutoUpdater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        void autoUpdater_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                MessageBox.Show("无参数");
                App.Current.Shutdown();
            }

            else
            {
                MainWindow mainWindow = new MainWindow(e.Args[0], e.Args[1]);
                mainWindow.Show();
                mainWindow.Activate();
            }
        }
    }
}
