using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using HAOest.ListManager.Dialogs;
using HAOest.ListManager;

namespace HAOest.ListManager
{
    class UpdateChecker
    {
        private ProjectVersion latestVersion = null;
        private ProjectVersion localVersion = null;
        private delegate void AskForUpdateEventHandler();
        private bool autoCheck = false;
        private MainWindow mainWindow = null;
        private MainWindow.ShowMessageBoxEventHandler ShowMessageBox;

        public UpdateChecker(MainWindow mainWindow, bool autoCheck)
        {
            this.autoCheck = autoCheck;
            this.mainWindow = mainWindow;
            ShowMessageBox = new MainWindow.ShowMessageBoxEventHandler(mainWindow.ShowMessageBox);
        }

        /// <summary>
        /// 检测版本
        /// </summary>
        public void Check()
        {
            try
            {
                latestVersion = ProjectVersion.GetLatestVersion();
                localVersion = ProjectVersion.GetLocalVersion();

                if (localVersion.BuildNumber < latestVersion.BuildNumber)
                {
                    mainWindow.Dispatcher.Invoke(new AskForUpdateEventHandler(AskForUpdate));
                }
                else
                {
                    if (!autoCheck)
                    {
                        mainWindow.Dispatcher.Invoke(ShowMessageBox, MessageBoxType.Infomation, "已经是最新版啦！");
                    }
                }
            }
            catch (WebException we)
            {
                if (!autoCheck)
                {
                    mainWindow.Dispatcher.Invoke(ShowMessageBox, MessageBoxType.Error, "检测新版本出现错误，请检查网络或稍后再试！");
                }
                App.writeLog.Error("检查版本失败", we);
            }
            catch (Exception ex)
            {
                App.writeLog.Error("检查版本失败", ex);
            }
        }

        /// <summary>
        /// 询问是否更新
        /// </summary>
        private void AskForUpdate()
        {
            if (MessageBox.Show("检测到新版本，是否更新？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                UpdateDialog updateDialog = new UpdateDialog(localVersion, latestVersion) { Owner = Application.Current.MainWindow };
                updateDialog.ShowDialog();
            }
        }
    }
}
