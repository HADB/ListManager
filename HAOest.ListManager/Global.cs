using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAOest.ListManager
{
    public class Global
    {
        public const string TIME_FORMAT_STRING = "yyyy-MM-dd ddd HH:mm:ss";
    }

    public enum SortType
    {
        Default = 0x0000,
        Mark = 0x0001,
        StartTime = 0x0002,
        EndTime = 0x0003,
        Title = 0x0004,
        Manual = 0x0005
    }

    public enum SortSequence
    {
        Asc = 0x0000,
        Desc = 0x0001
    }

    public enum MarkType
    {
        Starred = 0x0000,
        None = 0x0001,
        Finished = 0x0002,
        Overdue = 0x0003,
        Abandoned = 0x0004
    }

    public enum MoveDirection
    {
        Up = 0x0001,
        Down = -0x0001
    }

    public struct MarkItem
    {
        public int Id { get; set; }
        public MarkType Mark { get; set; }
        public string MarkName { get; set; }
        public static List<MarkItem> MarkItemList = new List<MarkItem> { new MarkItem(0), new MarkItem(1), new MarkItem(2), new MarkItem(3), new MarkItem(4) };

        public MarkItem(int id)
            : this()
        {
            Id = id;
            switch (id)
            {
                case 0:
                    Mark = MarkType.Starred;
                    MarkName = "重要";
                    break;
                case 1:
                    Mark = MarkType.None;
                    MarkName = "未完成";
                    break;
                case 2:
                    Mark = MarkType.Finished;
                    MarkName = "已完成";
                    break;
                case 3:
                    Mark = MarkType.Overdue;
                    MarkName = "过期";
                    break;
                case 4:
                    Mark = MarkType.Abandoned;
                    MarkName = "放弃";
                    break;
            }
        }
    }

    public enum MessageBoxType
    {
        Infomation = 0x0000,
        Warning = 0x0001,
        Error = 0x0002
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        private int selectionIndex;
        private ListChild selectedListChild;

        public int SelectionIndex
        {
            get { return selectionIndex; }
        }

        public ListChild SelectedListChild
        {
            get { return selectedListChild; }
        }

        public SelectionChangedEventArgs(int selectionIndex,ListChild selectedListChild)
        {
            this.selectionIndex = selectionIndex;
            this.selectedListChild = selectedListChild;
        }
    }
}
