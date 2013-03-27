using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace HAOest.ListManager.AutoUpdater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private string directoryPath = null;
        private string newfilesDirectoryPath = null;
        private delegate void UpdateUiEventHandler(string stateText, double progress);

        public MainWindow(string newfilesDirectoryPath, string directoryPath)
        {
            InitializeComponent();
            this.directoryPath = directoryPath;
            this.newfilesDirectoryPath = newfilesDirectoryPath;
        }

        private void autoUpdater_Loaded(object sender, RoutedEventArgs e)
        {
            Thread updateThread = new Thread(RunUpdate);
            updateThread.Start();
        }

        private void UpdateUiMethod(string stateText, double progress)
        {
            LbState.Content = stateText;
            ProgressBar.Value = progress;

            if (progress == 100)
            {
                if (MessageBox.Show("恭喜！更新完毕！") == MessageBoxResult.OK)
                {
                    Process newProcess = new Process();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(directoryPath + "\\ListManager.exe");
                    newProcess.StartInfo = processStartInfo;
                    newProcess.StartInfo.UseShellExecute = false;
                    newProcess.Start();
                    Close();
                }
            }
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        private void RunUpdate()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower().StartsWith("listmanager"))
                {
                    process.Kill();
                }
            }
            try
            {
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            object[] args = new object[2];

            FileInfo[] newfiles = new DirectoryInfo(newfilesDirectoryPath).GetFiles();
            int newfilesSum = newfiles.Length;

            for (int i = 0; i < newfilesSum; i++)
            {
                FileInfo fileInfo = newfiles[i];

                args[0] = "正在更新文件 " + fileInfo.Name;
                args[1] = (double)i * 100 / newfilesSum;

                Dispatcher.Invoke(new UpdateUiEventHandler(UpdateUiMethod), args);
                string fileNewPath = directoryPath + "\\" + fileInfo.Name;
                try
                {
                    if (File.Exists(fileNewPath))
                    {
                        File.Delete(fileNewPath);
                    }
                    File.Move(fileInfo.FullName, fileNewPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Application.Current.Shutdown();
                }
            }

            args[0] = "更新完毕！";
            args[1] = 100;
            Dispatcher.Invoke(new UpdateUiEventHandler(UpdateUiMethod), args);
        }
    }
}
