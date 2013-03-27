using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HAOest.ListManager.Controls
{
    /// <summary>
    /// ListChildItem.xaml 的交互逻辑
    /// </summary>
    public partial class ListChildItem : UserControl
    {
        private string endTime;
        private bool selected;
        public ListChildItem()
        {
            InitializeComponent();
        }

        public ListChildItem(ListChild listChild)
        {
            InitializeComponent();
            ListChild = listChild;
            Id = listChild.Id;
            Title = listChild.Title;
            Detail = listChild.Detail;
            StartTime = listChild.StartTime.ToString(Global.TIME_FORMAT_STRING);
            EndTime = listChild.EndTime.ToString(Global.TIME_FORMAT_STRING);
            Mark = (new MarkItem(Convert.ToInt16(listChild.Mark))).MarkName;
            DataContext = this;
        }

        public ListChild ListChild { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string StartTime { get; set; }
        public string EndTime
        {
            get { return endTime; }
            set
            {
                if (value == DateTime.MaxValue.ToString(Global.TIME_FORMAT_STRING))
                {
                    endTime = "永不截止";
                }
                else endTime = value;
            }
        }
        public string Mark { get; set; }
        public int DisplayWeight { get; set; }
        public int FamilyId { get; set; }
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (selected)
                {
                    border.Background = (Brush)FindResource("ListBoxSelectedItemBackground");
                }
                else
                {
                    border.Background = null;
                }
            }
        }
    }
}