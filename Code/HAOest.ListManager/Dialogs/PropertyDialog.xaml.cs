using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HAOest.ListManager
{
    /// <summary>
    /// ListFamilyInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private ListFamily listFamily = new ListFamily();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public SettingsDialog(ListFamily listFamily)
        {
            InitializeComponent();
            this.listFamily = listFamily;
            chkShowFinishedItem.IsChecked = listFamily.ShowFinishedItem;
            chkShowOverdueItem.IsChecked = listFamily.ShowOverdueItem;
            chkShowAbandonedItem.IsChecked = listFamily.ShowAbandonedItem;
            switch (listFamily.SortType)
            {
                case SortType.Default:
                case SortType.Mark:
                    radMark.IsChecked = true;
                    break;
                case SortType.StartTime:
                    radStartTime.IsChecked = true;
                    break;
                case SortType.EndTime:
                    radEndTime.IsChecked = true;
                    break;
                case SortType.Title:
                    radTitle.IsChecked = true;
                    break;
                case SortType.Manual:
                    radManual.IsChecked = true;
                    break;
            }

            radApplyToCurrentList.IsChecked = true;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            //下面一段保存“显示内容”
            listFamily.ShowFinishedItem = chkShowFinishedItem.IsChecked.Value;
            listFamily.ShowOverdueItem = chkShowOverdueItem.IsChecked.Value;
            listFamily.ShowAbandonedItem = chkShowAbandonedItem.IsChecked.Value;

            //下面一段保存“列表排序”
            if (radMark.IsChecked.Value)
            {
                listFamily.SortType = SortType.Mark;
            }
            else if (radStartTime.IsChecked.Value)
            {
                listFamily.SortType = SortType.StartTime;
            }
            else if (radEndTime.IsChecked.Value)
            {
                listFamily.SortType = SortType.EndTime;
            }
            else if (radTitle.IsChecked.Value)
            {
                listFamily.SortType = SortType.Title;
            }
            else if (radManual.IsChecked.Value)
            {
                listFamily.SortType = SortType.Manual;
            }
            else
            {
                listFamily.SortType = SortType.Default;
            }

            if (radApplyToCurrentList.IsChecked.Value)
            {
                //下面更新数据库
                try
                {
                    ListFamily.UpdateListFamily(listFamily);
                }
                catch (Exception ex)
                {
                    App.writeLog.Error("保存失败", ex);
                    MessageBox.Show("项目设置保存失败！\n错误日志请查看Error.log文件！", "噢噢，出错了", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else if (radApplyToAllLists.IsChecked.Value)
            {
                List<string> columnsList = new List<string>
                {
                    ListFamily.FIELD_NAME_SHOW_FINISHED_ITEM,
                    ListFamily.FIELD_NAME_SHOW_OVERDUE_ITEM,
                    ListFamily.FIELD_NAME_SHOW_ABANDONED_ITEM,
                    ListFamily.FIELD_NAME_SORT_TYPE
                };

                List<string> valuesList = new List<string>
                {
                    Convert.ToInt16(listFamily.ShowFinishedItem).ToString(),
                    Convert.ToInt16(listFamily.ShowOverdueItem).ToString(),
                    Convert.ToInt16(listFamily.ShowAbandonedItem).ToString(),
                    Convert.ToInt16(listFamily.SortType).ToString()
                };

                try
                {
                    ListFamily.UpdateAllListFamilies(columnsList, valuesList);
                }
                catch (Exception ex)
                {
                    App.writeLog.Error("保存失败", ex);
                    MessageBox.Show("项目设置保存失败！\n错误日志请查看Error.log文件！", "噢噢，出错了", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
