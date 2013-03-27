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
    public partial class ListFamilyInfoDialog : Window
    {
        private ListFamily listFamily = new ListFamily();
        private DialogType dialogType = DialogType.Undefined;
        private int displayWeight = 0;

        public ListFamilyInfoDialog(int displayWeight)
        {
            InitializeComponent();
            this.displayWeight = displayWeight;
            dialogType = DialogType.Add;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public ListFamilyInfoDialog(ListFamily listFamily)
        {
            InitializeComponent();
            dialogType = DialogType.Edit;
            this.listFamily = listFamily;
            this.tbListFamilyTitle.Text = listFamily.Title;
            this.tbListFamilyDetail.Text = listFamily.Detail;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listFamily.Title = tbListFamilyTitle.Text;
                listFamily.Detail = tbListFamilyDetail.Text;
                switch (dialogType)
                {
                    case DialogType.Add:
                        listFamily.DisplayWeight = displayWeight;
                        ListFamily.AddListFamily(listFamily);
                        break;
                    case DialogType.Edit:
                        ListFamily.UpdateListFamily(listFamily);
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
    }
}
