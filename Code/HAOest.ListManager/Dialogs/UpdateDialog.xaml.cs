using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using HAOest.Data.SQLite;
using HAOest.Environment;
using HAOest.IO;
using Ionic.Zip;

namespace HAOest.ListManager.Dialogs
{
    /// <summary>
    /// UpdateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public const string LIST_FAMILIES_TABLE_NAME = "list_families";
        public const string LIST_CHILDREN_TABLE_NAME = "list_children";
        public const string FIELD_NAME_SHOW_FINISHED_ITEM = "list_family_show_finished_item";
        public const string FIELD_NAME_SHOW_OVERDUE_ITEM = "list_family_show_overdue_item";
        public const string FIELD_NAME_SHOW_ABANDONED_ITEM = "list_family_show_abandoned_item";
        public const string FIELD_NAME_SORT_TYPE = "list_family_sort_type";

        private string directoryPath = null;
        private string downloadPath = null;
        private string newfilesDirectoryPath = null;
        private ProjectVersion latestVersion = null;
        private ProjectVersion localVersion = null;
        private WebClient webClient = null;

        public UpdateDialog(ProjectVersion localVersion, ProjectVersion latestVersion)
        {
            InitializeComponent();
            this.localVersion = localVersion;
            this.latestVersion = latestVersion;

            lbLocalVersion.Content = localVersion.AssemblyVersion;
            lbLatestVersion.Content = latestVersion.AssemblyVersion;

            directoryPath = Info.GetDirectory();
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
        }

        private void updateDialog_Loaded(object sender, RoutedEventArgs e)
        {
            StartDownload();
        }

        private void updateDialog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void UpdateDatabase(int versionFrom, int versionTo)
        {
            FileBackup.BackupWithTime(Info.GetDirectory() + "\\data.db");
            SQLite sqlite = new SQLite("data.db");
            switch (versionTo)
            {
                case 2:
                    sqlite.AddColumns(LIST_FAMILIES_TABLE_NAME, new List<string> { FIELD_NAME_SHOW_FINISHED_ITEM, FIELD_NAME_SHOW_ABANDONED_ITEM, FIELD_NAME_SHOW_OVERDUE_ITEM, FIELD_NAME_SORT_TYPE }, new List<string> { "INTEGER", "INTEGER", "INTEGER", "INTEGER" });
                    sqlite.Update(LIST_FAMILIES_TABLE_NAME, new List<string> { FIELD_NAME_SHOW_FINISHED_ITEM, FIELD_NAME_SHOW_ABANDONED_ITEM, FIELD_NAME_SHOW_OVERDUE_ITEM, FIELD_NAME_SORT_TYPE }, new List<string> { "1", "0", "1", "1" }, "list_family_id > '0'");
                    break;
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            lbState.Content = "正在下载... (" + e.BytesReceived + "/" + e.TotalBytesToReceive + ")";
        }

        /// <summary>
        /// 文件下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            UnZip();
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        private void UnZip()
        {
            try
            {
                newfilesDirectoryPath = directoryPath + "\\Download\\Build" + latestVersion.BuildNumber;
                ZipFile zipFile = ZipFile.Read(downloadPath + "\\" + latestVersion.NewFilesPackage.FileName);
                foreach (ZipEntry zipEntry in zipFile)
                {
                    lbState.Content = "正在解压缩 " + zipEntry.FileName;
                    zipEntry.Extract(newfilesDirectoryPath, ExtractExistingFileAction.OverwriteSilently);
                }
                lbState.Content = "解压缩完毕！";
                if (MessageBox.Show("更新需要暂时关闭本软件，是否继续？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    RunUpdater();
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        private void StartDownload()
        {
            string packageFileName = latestVersion.NewFilesPackage.FileName;
            downloadPath = directoryPath + "\\Download";
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            Uri uri = new Uri(latestVersion.NewFilesPackage.FileUrl);
            webClient.DownloadFileAsync(uri, downloadPath + "\\" + packageFileName);
        }


        /// <summary>
        /// 启动更新程序
        /// </summary>
        private void RunUpdater()
        {
            try
            {
                string updaterArg = newfilesDirectoryPath + " " + directoryPath;
                Process updater = new Process();
                ProcessStartInfo updaterStartInfo = new ProcessStartInfo("AutoUpdater.exe", updaterArg);
                updater.StartInfo = updaterStartInfo;
                updater.StartInfo.UseShellExecute = false;
                updater.Start();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
