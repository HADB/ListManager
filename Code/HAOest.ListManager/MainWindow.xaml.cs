using System;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media.Animation;

using HAOest.Environment;
using HAOest.ListManager.Controls;
using HAOest.ListManager.Dialogs;

namespace HAOest.ListManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region 【声明成员变量、属性和委托】
        private ListFamily selectedListFamily = new ListFamily();
        private DataTable listFamiliesDataTable = new DataTable();
        private DataTable listChildrenDataTable = new DataTable();
        private int lstListFamiliesSelectedIndex;
        // private int selectedListChildIndex;
        private bool cancelClosing = true;

        public delegate void ShowMessageBoxEventHandler(MessageBoxType type, string msg);
        #endregion

        #region 【构造函数】
        public MainWindow()
        {
            InitializeComponent();
            ReadListFamiles();
            lstListFamilies.SelectedIndex = 0;
            lstListFamilies.ContextMenu = null;
            //  lstListChildren.ContextMenu = null;
            LbVersion.Text = string.Format("当前版本：{0}", Info.AssemblyVersion);
            App.writeLog.Info("启动ListManager");
        }
        #endregion

        #region【MainWindow 的事件】
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (FirstTimeOpen())
            {
                InsertHelpLists();
            }
            UpdateChecker updateChecker = new UpdateChecker(this, true);
            Thread checkUpdateThread = new Thread(updateChecker.Check);
            checkUpdateThread.Start();
        }

        /// <summary>
        /// 鼠标左键按下，为了拖动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = cancelClosing;
            cancelClosing = false;
            if (cancelClosing)
            {
                UpdateListFamiliesDisplayWeight();
                if (selectedListFamily.SortType == SortType.Manual)
                {
                    UpdateListChildrenDisplayWeight();
                }
            }

            //以下实现动画效果
            Storyboard sb = new Storyboard();
            DoubleAnimation dh = new DoubleAnimation();
            DoubleAnimation dw = new DoubleAnimation();
            DoubleAnimation dop = new DoubleAnimation
                {
                    Duration = dh.Duration = dw.Duration = sb.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 800)),
                    To = dh.To = dw.To = 0
                };
            Storyboard.SetTarget(dop, this);
            Storyboard.SetTarget(dh, this);
            Storyboard.SetTarget(dw, this);
            Storyboard.SetTargetProperty(dop, new PropertyPath("Opacity", new object[] { }));
            Storyboard.SetTargetProperty(dh, new PropertyPath("Height", new object[] { }));
            Storyboard.SetTargetProperty(dw, new PropertyPath("Width", new object[] { }));
            sb.Children.Add(dw);
            sb.Children.Add(dh);
            sb.Children.Add(dop);
            sb.Completed += ClosingAnimationCompleted;
            sb.Begin();
        }

        private void ClosingAnimationCompleted(object sender, EventArgs e)
        {
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion

        #region 【ListFamily 的事件】
        /// <summary>
        /// lstListFamilies 的 SelectionChanged 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstListFamilies_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstListFamilies.SelectedIndex != -1)
            {
                if (selectedListFamily.SortType == SortType.Manual)
                {
                    UpdateListChildrenDisplayWeight();
                }

                DataRowView fd = lstListFamilies.SelectedItem as DataRowView;
                selectedListFamily = ListFamily.GetListFamilyById(fd[ListFamily.FIELD_NAME_ID].ToString());
                lstListFamiliesSelectedIndex = lstListFamilies.SelectedIndex;
                listChildrenDisplayPart.SelectionIndex = 0;
                tbListFamilyName.Text = string.Format("{0}：", selectedListFamily.Title);

                ReadListChildren();
            }
        }

        /// <summary>
        /// 双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstListFamilies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GoodPointPosition(sender, e))
            {
                EditListFamily(DialogType.Edit);
            }
        }

        /// <summary>
        /// 右击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstListFamilies_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GoodPointPosition(sender, e))
            {
                lstListFamilies.ContextMenu = LstListFamilyContextMenu;
            }
            else
            {
                lstListFamilies.ContextMenu = null;
            }
        }

        private void lstListFamilyContextMenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            EditListFamily(DialogType.Edit);
        }

        private void lstListFamilyContextMenuItemMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveListFamily(MoveDirection.Up);
        }

        private void lstListFamilyContextMenuItemMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveListFamily(MoveDirection.Down);
        }

        private void lstListFamilyContextMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteListFamily();
        }

        private void btnAddListFamily_Click(object sender, RoutedEventArgs e)
        {
            EditListFamily(DialogType.Add);
        }

        private void btnEditListFamily_Click(object sender, RoutedEventArgs e)
        {
            EditListFamily(DialogType.Edit);
        }

        private void btnDeleteListFamily_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show(string.Format("确定要删除这个列表吗？它里面的项目都会被删除哟~\n列表标题：{0}", selectedListFamily.Title), "三思而后行", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation))
            {
                case MessageBoxResult.OK:
                    ListFamily.DeleteListFamily(selectedListFamily);
                    ReadListFamiles();
                    ReadListChildren();
                    break;
            }
        }

        /// <summary>
        /// ListFamily属性按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnListFamilyProperty_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settingsDialog = new SettingsDialog(selectedListFamily) { Owner = this };
            var showDialog = settingsDialog.ShowDialog();
            if (showDialog != null && showDialog.Value)
            {
                ReadListFamiles();
            }
        }
        #endregion

        #region 【ListChild 的事件】
        private void lstListChildrenColumnNameTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSortType(SortType.Title);
        }

        private void lstListChildrenColumnNameStartTime_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSortType(SortType.StartTime);
        }

        private void lstListChildrenColumnNameMark_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSortType(SortType.Mark);
        }

        private void lstListChildren_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GoodPointPosition(sender, e))
            {
                //lstListChildren.ContextMenu = LstListChildrenContextMenu;
            }
            else
            {
                // lstListChildren.ContextMenu = null;
            }
        }

        private void lstListChildContextMenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            ShowListChildInfoDialog(DialogType.Edit);
        }

        private void lstListChildContextMenuItemMarkStarred_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Starred);
        }

        private void lstListChildContextMenuItemMarkFinished_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Finished);
        }

        private void lstListChildContextMenuItemMarkNone_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.None);
        }

        private void lstListChildContextMenuItemMarkAbandoned_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Abandoned);
        }

        private void lstListChildContextMenuItemMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveListChild(MoveDirection.Up);
        }

        private void lstListChildContextMenuItemMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveListChild(MoveDirection.Down);
        }

        private void lstListChildContextMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteListChild();
        }

        private void btnAddListChild_Click(object sender, RoutedEventArgs e)
        {
            ShowListChildInfoDialog(DialogType.Add);
        }

        private void btnEditListChild_Click(object sender, RoutedEventArgs e)
        {
            ShowListChildInfoDialog(DialogType.Edit);
        }

        private void btnMarkListChild_Click(object sender, RoutedEventArgs e)
        {
            popupMark.IsOpen = true;
        }

        private void menuItemStarred_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Starred);
        }

        private void menuItemFinished_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Finished);
        }

        private void menuItemNone_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.None);
        }

        private void menuItemAbandoned_Click(object sender, RoutedEventArgs e)
        {
            UpdateListChildMark(MarkType.Abandoned);
        }

        private void btnDeleteListChild_Click(object sender, RoutedEventArgs e)
        {
            DeleteListChild();
        }

        private void btnMoveUpListChild_Click(object sender, RoutedEventArgs e)
        {
            MoveListChild(MoveDirection.Up);
        }

        private void btnMoveDownListChild_Click(object sender, RoutedEventArgs e)
        {
            MoveListChild(MoveDirection.Down);
        }
        #endregion

        #region 【方法】
        /// <summary>
        /// 编辑 ListFamily
        /// </summary>
        /// <param name="dialogType"></param>
        private void EditListFamily(DialogType dialogType)
        {
            ListFamilyInfoDialog listFamilyInfoDialog = null;
            switch (dialogType)
            {
                case DialogType.Add:
                    listFamilyInfoDialog = new ListFamilyInfoDialog(listFamiliesDataTable.Rows.Count);
                    break;
                case DialogType.Edit:
                    listFamilyInfoDialog = new ListFamilyInfoDialog(selectedListFamily);
                    break;
            }
            if (listFamilyInfoDialog != null)
            {
                listFamilyInfoDialog.Owner = this;
                listFamilyInfoDialog.ShowDialog();
            }
            ReadListFamiles();
        }

        /// <summary>
        /// 删除 ListFamily
        /// </summary>
        private void DeleteListFamily()
        {
            if (MessageBox.Show(string.Format("确定要删除这个列表吗？它里面的项目都会被删除哟~\n列表标题：{0}", selectedListFamily.Title), "三思而后行", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                ListFamily.DeleteListFamily(selectedListFamily);
                ReadListFamiles();
                ReadListChildren();
            }
        }

        /// <summary>
        /// 显示 ListChildInfoDialog
        /// </summary>
        private void ShowListChildInfoDialog(DialogType dialogType)
        {
            ListChildInfoDialog.ShowDialog(this, dialogType, selectedListFamily.Id, listChildrenDisplayPart.SelectedListChild);
            ReadListChildren();
        }

        /// <summary>
        /// 删除 ListChild
        /// </summary>
        private void DeleteListChild()
        {
            if (MessageBox.Show(string.Format("确定要删除这个项目吗？\n项目标题：{0}", listChildrenDisplayPart.SelectedListChild.Title), "三思而后行", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                ListChild.DeleteListChild(listChildrenDisplayPart.SelectedListChild);
                ReadListChildren();
            }
        }

        /// <summary>
        /// 读取 ListFamilies
        /// </summary>
        private void ReadListFamiles()
        {
            try
            {
                listFamiliesDataTable = ListFamily.GetListFamiliesTable();
                lstListFamilies.DataContext = listFamiliesDataTable;

                if (lstListFamiliesSelectedIndex == lstListFamilies.Items.Count)
                {
                    lstListFamilies.SelectedIndex = lstListFamiliesSelectedIndex - 1;
                }
                else if (lstListFamiliesSelectedIndex >= 0)
                {
                    lstListFamilies.SelectedIndex = lstListFamiliesSelectedIndex;
                }
                else lstListFamilies.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                App.writeLog.Warn("未能从数据库中读取到ListFamilies", ex);
            }
        }

        /// <summary>
        /// 读取 ListChildren
        /// </summary>
        private void ReadListChildren()
        {
            try
            {
                listChildrenDataTable = ListChild.GetListChildrenTable(selectedListFamily);
                if (listChildrenDataTable.Rows.Count == 0)
                {
                    btnEditListChild.IsEnabled = false;
                    btnDeleteListChild.IsEnabled = false;
                    btnMarkListChild.IsEnabled = false;
                }
                else
                {
                    btnEditListChild.IsEnabled = true;
                    btnDeleteListChild.IsEnabled = true;
                    btnMarkListChild.IsEnabled = true;
                    listChildrenDisplayPart.LoadListChildItems(listChildrenDataTable);

                    if (listChildrenDisplayPart.SelectionIndex == listChildrenDataTable.Rows.Count)
                    {
                        listChildrenDisplayPart.SelectionIndex -= 1;
                    }
                    else if (listChildrenDisplayPart.SelectionIndex < 0)
                    {
                        listChildrenDisplayPart.SelectionIndex = 0;
                    }

                }

                ListChildrenTitleUpTriangle.Visibility = Visibility.Collapsed;
                ListChildrenTitleDownTriangle.Visibility = Visibility.Collapsed;
                ListChildrenTimeUpTriangle.Visibility = Visibility.Collapsed;
                ListChildrenTimeDownTriangle.Visibility = Visibility.Collapsed;
                ListChildrenMarkUpTriangle.Visibility = Visibility.Collapsed;
                ListChildrenMarkDownTriangle.Visibility = Visibility.Collapsed;

                switch (selectedListFamily.SortType)
                {
                    case SortType.Title:
                        ListChildrenTitleUpTriangle.Visibility = Visibility.Visible;
                        ListChildrenTitleDownTriangle.Visibility = Visibility.Visible;
                        if (selectedListFamily.SortSequence == SortSequence.Asc)
                        {

                        }
                        else
                        {

                        }
                        break;
                    case SortType.StartTime:
                        ListChildrenTimeUpTriangle.Visibility = Visibility.Visible;
                        ListChildrenTimeDownTriangle.Visibility = Visibility.Visible;
                        break;
                    case SortType.Mark:
                        ListChildrenMarkUpTriangle.Visibility = Visibility.Visible;
                        ListChildrenMarkDownTriangle.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                App.writeLog.Warn("未能从数据库中读取ListChildren", ex);
            }
        }

        /// <summary>
        /// 更新ListFamilies的DisplayWeight
        /// </summary>
        private void UpdateListFamiliesDisplayWeight()
        {
            ListFamily.BeginTransaction();
            for (int i = 0; i < lstListFamilies.Items.Count; i++)
            {
                DataRowView fd = lstListFamilies.Items[i] as DataRowView;
                ListFamily listFamily = ListFamily.GetListFamilyById(fd[ListFamily.FIELD_NAME_ID].ToString());
                listFamily.DisplayWeight = i;
                ListFamily.UpdateListFamily(listFamily);
            }
            ListFamily.CommitTransaction();
        }

        /// <summary>
        /// 移动ListFamily
        /// </summary>
        /// <param name="moveDirection"></param>
        private void MoveListFamily(MoveDirection moveDirection)
        {
            int flag = Convert.ToInt16(moveDirection);

            if ((lstListFamiliesSelectedIndex - flag > -1) && (lstListFamiliesSelectedIndex - flag < listFamiliesDataTable.Rows.Count))
            {
                selectedListFamily.DisplayWeight -= flag;
                ListFamily listFamily = ListFamily.GetListFamilyById(listFamiliesDataTable.Rows[lstListFamiliesSelectedIndex - flag][ListFamily.FIELD_NAME_ID].ToString());
                listFamily.DisplayWeight += flag;

                ListFamily.UpdateListFamily(selectedListFamily);
                ListFamily.UpdateListFamily(listFamily);

                lstListFamiliesSelectedIndex -= flag;
                ReadListFamiles();
            }
        }

        /// <summary>
        /// 移动ListChild
        /// </summary>
        /// <param name="moveDirection"></param>
        private void MoveListChild(MoveDirection moveDirection)
        {
            int flag = Convert.ToInt16(moveDirection);

            if ((listChildrenDisplayPart.SelectionIndex - flag > -1) && (listChildrenDisplayPart.SelectionIndex - flag < listChildrenDataTable.Rows.Count))
            {
                if (selectedListFamily.SortType != SortType.Manual)
                {
                    UpdateListChildrenDisplayWeight();
                    ReadListChildren();
                    selectedListFamily.SortType = SortType.Manual;
                    ListFamily.UpdateListFamily(selectedListFamily);
                }

                listChildrenDisplayPart.SelectedListChild.DisplayWeight -= flag;
                ListChild listChild = ListChild.GetListChildById(Convert.ToInt32(listChildrenDataTable.Rows[listChildrenDisplayPart.SelectionIndex - flag][ListChild.FIELD_NAME_ID]));
                listChild.DisplayWeight += flag;

                ListChild.UpdateListChild(listChildrenDisplayPart.SelectedListChild);
                ListChild.UpdateListChild(listChild);

                listChildrenDisplayPart.SelectionIndex -= flag;

                ReadListChildren();
            }
        }

        /// <summary>
        /// 更新ListChildren的DisplayWeight
        /// </summary>
        private void UpdateListChildrenDisplayWeight()
        {
            try
            {
                ListChild.BeginTransaction();
                for (int i = 0; i < listChildrenDataTable.Rows.Count; i++)
                {
                    ListChild listChild = ListChild.GetListChildById(Convert.ToInt32(listChildrenDataTable.Rows[i][ListChild.FIELD_NAME_ID]));
                    listChild.DisplayWeight = i;
                    ListChild.UpdateListChild(listChild);
                }
                ListChild.CommitTransaction();
            }
            catch (Exception ex)
            {
                App.writeLog.Warn("更新ListChildren的DisplayWeight失败，可能是由于删除列表导致", ex);
            }
        }

        /// <summary>
        /// 更新ListChild 的 Mark
        /// </summary>
        /// <param name="markType"></param>
        private void UpdateListChildMark(MarkType markType)
        {
            listChildrenDisplayPart.SelectedListChild.Mark = markType;
            ListChild.UpdateListChild(listChildrenDisplayPart.SelectedListChild);
            ReadListChildren();
            popupMark.IsOpen = false;
        }

        /// <summary>
        /// 修改排序类型
        /// </summary>
        /// <param name="sortType"></param>
        private void ChangeSortType(SortType sortType)
        {
            if (selectedListFamily.SortSequence == SortSequence.Asc)
            {
                selectedListFamily.SortSequence = SortSequence.Desc;
            }
            else
            {
                selectedListFamily.SortSequence = SortSequence.Asc;
            }
            selectedListFamily.SortType = sortType;
            ReadListChildren();
        }

        /// <summary>
        /// 获取鼠标位置的元素
        /// </summary>
        /// <param name="itemsControl"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private object GetElementFromPoint(ItemsControl itemsControl, Point point)
        {
            UIElement element = itemsControl.InputHitTest(point) as UIElement;
            while (element != null)
            {
                if (Equals(element, itemsControl))
                    return null;
                object item = itemsControl.ItemContainerGenerator.ItemFromContainer(element);
                if (!item.Equals(DependencyProperty.UnsetValue))
                    return item;
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        /// <summary>
        /// 鼠标位置有子控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool GoodPointPosition(object sender, MouseButtonEventArgs e)
        {
            return (GetElementFromPoint((ItemsControl)sender, e.GetPosition((ItemsControl)sender)) != null);
        }

        /// <summary>
        /// 生成本地版本XML
        /// </summary>
        private void GenerateLocalXml()
        {
            try
            {
                ProjectVersion localProjectVersion = new ProjectVersion
                    {
                        AssemblyVersion = Info.AssemblyVersion,
                        BuildNumber = Info.BuildNumber
                    };
                FilesPackage filesPackage = new FilesPackage
                    {
                        FileName = "ListManagerUpdate_" + Info.BuildNumber + ".zip",
                        FileUrl = ""
                    };
                localProjectVersion.NewFilesPackage = filesPackage;

                FileStream fileStream = new FileStream("LocalVersion.xml", FileMode.Create);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectVersion));
                xmlSerializer.Serialize(fileStream, localProjectVersion);
            }
            catch (Exception ex)
            {
                App.writeLog.Error("生成本地版本XML失败！", ex);
            }
        }

        /// <summary>
        /// 判断是否第一次打开该版本
        /// </summary>
        /// <returns></returns>
        private bool FirstTimeOpen()
        {
            if (!File.Exists("localVersion.xml"))
            {
                return true;
            }
            try
            {
                ProjectVersion lastVersion = ProjectVersion.Deserialize("localVersion.xml");
                if (lastVersion.BuildNumber < ProjectVersion.GetLocalVersion().BuildNumber)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                App.writeLog.Error("判断是否第一次打开失败！", ex);
                return false;
            }
        }

        /// <summary>
        /// 插入帮助列表
        /// </summary>
        private void InsertHelpLists()
        {
            ListFamily whatsNewListFamily = new ListFamily
                {
                    Title = "最新更新 ",
                    Detail = ProjectVersion.GetLocalVersion().AssemblyVersion,
                    DisplayWeight = listFamiliesDataTable.Rows.Count
                };
            ListFamily.AddListFamily(whatsNewListFamily);

            ListChild.AddListChild(new ListChild
                 {
                     Title = "修复bug",
                     Detail = "",
                     DisplayWeight = 0,
                     Mark = MarkType.Finished,
                     StartTime = new DateTime(2013, 1, 6, 11, 52, 00),
                     EndTime = DateTime.MaxValue,
                     FamilyId = ListFamily.GetListFamilyByWeight(whatsNewListFamily.DisplayWeight).Id
                 });

            ListChild.AddListChild(new ListChild
                {
                    Title = "细节调整",
                    Detail = "将设置按钮名称改为属性",
                    DisplayWeight = 1,
                    Mark = MarkType.Finished,
                    StartTime = new DateTime(2013, 1, 8, 9, 26, 00),
                    EndTime = DateTime.MaxValue,
                    FamilyId = ListFamily.GetListFamilyByWeight(whatsNewListFamily.DisplayWeight).Id
                });
            ListChild.AddListChild(new ListChild
                {
                    Title = "细节调整",
                    Detail = "在下方按钮中添加了上移和下移，方便移动",
                    DisplayWeight = 1,
                    Mark = MarkType.Finished,
                    StartTime = new DateTime(2013, 1, 8, 9, 30, 00),
                    EndTime = DateTime.MaxValue,
                    FamilyId = ListFamily.GetListFamilyByWeight(whatsNewListFamily.DisplayWeight).Id
                });

            ListChild.AddListChild(new ListChild
                {
                    Title = "功能增加",
                    Detail = "现在可以调整窗口大小了",
                    DisplayWeight = 2,
                    Mark = MarkType.Finished,
                    StartTime = new DateTime(2013, 1, 8, 14, 50, 00),
                    EndTime = DateTime.MaxValue,
                    FamilyId = ListFamily.GetListFamilyByWeight(whatsNewListFamily.DisplayWeight).Id
                });

            ReadListFamiles();
            lstListFamilies.SelectedIndex = whatsNewListFamily.DisplayWeight;

            GenerateLocalXml();
        }
        #endregion

        #region 底部链接代码
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateChecker updateChecker = new UpdateChecker(this, false);
            Thread checkUpdateThread = new Thread(updateChecker.Check);
            checkUpdateThread.Start();
        }

        /// <summary>
        /// 提点建议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendAdvice_Click(object sender, RoutedEventArgs e)
        {
            StartProcess("mailto:hadb@haoest.com?subject=【ListManager】反馈意见");
        }

        /// <summary>
        /// 产品首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectHome_Click(object sender, RoutedEventArgs e)
        {
            StartProcess("http://listmanager.haoest.com");
        }

        /// <summary>
        /// 好易思特HAOest
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompanyHome_Click(object sender, RoutedEventArgs e)
        {
            StartProcess("http://www.haoest.com");
        }

        /// <summary>
        /// 【方法】调用进程
        /// </summary>
        /// <param name="path"></param>
        private void StartProcess(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxType.Error, "调用系统进程失败，是不是权限不够？\n你可以直接发邮件给hadb@haoest.com反馈");
                App.writeLog.Error("HAOest.ListManager.MainWindow.StartProcess: 调用进程失败", ex);
            }
        }

        /// <summary>
        /// 显示提示框
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void ShowMessageBox(MessageBoxType type, string msg)
        {
            switch (type)
            {
                case MessageBoxType.Infomation:
                    MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case MessageBoxType.Warning:
                    MessageBox.Show(msg, "警告", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    break;
                case MessageBoxType.Error:
                    MessageBox.Show(msg, "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
        #endregion
    }
}