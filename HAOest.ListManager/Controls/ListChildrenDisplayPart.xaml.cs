using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HAOest.ListManager.Controls
{
    /// <summary>
    /// ListChildrenDisplayPart.xaml 的交互逻辑
    /// </summary>
    public partial class ListChildrenDisplayPart : UserControl
    {
        private int selectionIndex = -1;
        private ListChild selectedListChild = null;
        public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);
        public event SelectionChangedEventHandler SelectionChanged;

        public ListChild SelectedListChild
        {
            get
            {
                return selectedListChild;
            }
            set
            {
                selectedListChild = value;
            }
        }

        public int SelectionIndex
        {
            get
            {
                return selectionIndex;
            }
            set
            {
                SelectListChildItem(value);
            }
        }

        public ListChildrenDisplayPart()
        {
            InitializeComponent();
            this.SelectionChanged += OnSelectionChanged;
        }

        public void LoadListChildItems(DataTable listChildrenDataTable)
        {
            listChildrenStackPanel.Children.Clear();
            for (int i = 0; i < listChildrenDataTable.Rows.Count; i++)
            {
                ListChild listChild = ListChild.GetListChildByDataRow(listChildrenDataTable.Rows[i]);
                ListChildItem listChildItem = new ListChildItem(listChild);
                listChildItem.HorizontalAlignment = HorizontalAlignment.Stretch;

                if (i == selectionIndex)
                {
                    listChildItem.Selected = true;
                }
                listChildItem.MouseDown += listChildItem_MouseDown;
                listChildItem.MouseDoubleClick += listChildItem_MouseDoubleClick;
                listChildrenStackPanel.Children.Add(listChildItem);
            }
        }

        private void listChildItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListChildInfoDialog.ShowDialog(Application.Current.MainWindow, DialogType.Edit, SelectedListChild.FamilyId, SelectedListChild);
        }

        private void SelectListChildItem(int index)
        {
            for (int i = 0; i < listChildrenStackPanel.Children.Count; i++)
            {
                ((ListChildItem)listChildrenStackPanel.Children[i]).Selected = false;
            }
            if (index < listChildrenStackPanel.Children.Count)
            {
                ((ListChildItem)listChildrenStackPanel.Children[index]).Selected = true;

                if (selectionIndex != index)
                {
                    SelectionChanged(null, new SelectionChangedEventArgs(index, ((ListChildItem)listChildrenStackPanel.Children[index]).ListChild));
                }
            }
        }

        private void listChildItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < listChildrenStackPanel.Children.Count; i++)
            {
                ((ListChildItem)listChildrenStackPanel.Children[i]).Selected = false;
            }
            ((ListChildItem)sender).Selected = true;

            for (int i = 0; i < listChildrenStackPanel.Children.Count; i++)
            {
                if (((ListChildItem)listChildrenStackPanel.Children[i]).Selected == true)
                {
                    if (selectionIndex != i)
                    {
                        SelectionChanged(null, new SelectionChangedEventArgs(i, ((ListChildItem)sender).ListChild));
                    }
                }
            }
        }

        private void listChildrenDisplayPart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            listChildrenStackPanel.Width = ListChildrenScrollViewer.RenderSize.Width;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionIndex = e.SelectionIndex;
            selectedListChild = e.SelectedListChild;
        }
    }
}
