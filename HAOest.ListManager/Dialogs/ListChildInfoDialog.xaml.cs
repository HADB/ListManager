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
    public partial class ListChildInfoDialog : Window
    {
        private ListChild listChild = new ListChild();
        private DialogType dialogType = DialogType.Undefined;
        private int selectedListFamilyId;

        #region 【构造函数】
        public ListChildInfoDialog()
        {
            InitializeComponent();
            cmbMark.ItemsSource = MarkItem.MarkItemList;
            cmbMark.SelectedIndex = Convert.ToInt16(MarkType.None);
            dtpStartTime.Value = DateTime.Now;
            dtpEndTime.Value = DateTime.MaxValue;
        }

        public ListChildInfoDialog(int selectedListFamilyId)
            : this()
        {
            dialogType = DialogType.Add;
            this.selectedListFamilyId = selectedListFamilyId;
            this.chkNeverEnd.IsChecked = true;
        }

        public ListChildInfoDialog(ListChild listChild)
            : this()
        {
            dialogType = DialogType.Edit;
            this.listChild = listChild;
            this.tbListChildTitle.Text = listChild.Title;
            this.tbListChildDetail.Text = listChild.Detail;
            this.cmbMark.SelectedIndex = (int)listChild.Mark;
            this.dtpStartTime.Value = listChild.StartTime;
            this.dtpEndTime.Value = listChild.EndTime;
            if (listChild.EndTime.ToString(Global.TIME_FORMAT_STRING).Equals(DateTime.MaxValue.ToString(Global.TIME_FORMAT_STRING)))
            {
                chkNeverEnd.IsChecked = true;
            }
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listChild.Title = tbListChildTitle.Text;
                listChild.Detail = tbListChildDetail.Text;
                listChild.StartTime = (DateTime)dtpStartTime.Value;
                listChild.EndTime = (DateTime)dtpEndTime.Value;
                listChild.Mark = ((MarkItem)cmbMark.SelectedItem).Mark;
                switch (dialogType)
                {
                    case DialogType.Add:
                        listChild.FamilyId = selectedListFamilyId;
                        ListChild.AddListChild(listChild);
                        break;
                    case DialogType.Edit:
                        ListChild.UpdateListChild(listChild);
                        break;
                }
            }
            catch (Exception ex)
            {
                App.writeLog.Error("保存失败", ex);
                MessageBox.Show("项目保存失败！\n错误日志请查看Error.log文件！", "噢噢，出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void chkNeverEnd_Checked(object sender, RoutedEventArgs e)
        {
            dtpEndTime.Value = DateTime.MaxValue;
            dtpEndTime.IsEnabled = false;
        }

        private void chkNeverEnd_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpEndTime.Value = DateTime.Now;
            dtpEndTime.IsEnabled = true;
        }


        public static bool? ShowDialog(Window window, DialogType dialogType, int selectedListFamilyId, ListChild selectedListChild)
        {
            ListChildInfoDialog listChildInfoDialog = null;
            switch (dialogType)
            {
                case DialogType.Add:
                    listChildInfoDialog = new ListChildInfoDialog(selectedListFamilyId);
                    break;
                case DialogType.Edit:
                    listChildInfoDialog = new ListChildInfoDialog(selectedListChild);
                    break;
            }

            if (listChildInfoDialog != null)
            {
                listChildInfoDialog.Owner = window;
                return listChildInfoDialog.ShowDialog().Value;
            }
            return null;
        }
    }
}
