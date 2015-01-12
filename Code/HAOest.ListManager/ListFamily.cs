using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using HAOest.Data.SQLite;

namespace HAOest.ListManager
{
    public class ListFamily
    {
        public const string TABLE_NAME = "list_families";
        public const string FIELD_NAME_ID = "list_family_id";
        public const string FIELD_NAME_TITLE = "list_family_title";
        public const string FIELD_NAME_DETAIL = "list_family_detail";
        public const string FIELD_NAME_MARK = "list_family_mark";
        public const string FIELD_NAME_DISPLAY_WEIGHT = "list_family_display_weight";
        public const string FIELD_NAME_SHOW_FINISHED_ITEM = "list_family_show_finished_item";
        public const string FIELD_NAME_SHOW_OVERDUE_ITEM = "list_family_show_overdue_item";
        public const string FIELD_NAME_SHOW_ABANDONED_ITEM = "list_family_show_abandoned_item";
        public const string FIELD_NAME_SORT_TYPE = "list_family_sort_type";

        private static SQLite sqlite = new SQLite("data.db");

        #region 【ListFamily 的属性】
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 详情
        /// </summary>
        public string Detail
        {
            get;
            set;
        }

        /// <summary>
        /// 标记
        /// </summary>
        public MarkType Mark
        {
            get;
            set;
        }

        /// <summary>
        /// 显示权重
        /// </summary>
        public int DisplayWeight
        {
            get;
            set;
        }

        public bool ShowFinishedItem
        {
            get;
            set;
        }

        public bool ShowOverdueItem
        {
            get;
            set;
        }

        public bool ShowAbandonedItem
        {
            get;
            set;
        }

        public SortType SortType
        {
            get;
            set;
        }

        public SortSequence SortSequence
        {
            get;
            set;
        }
        #endregion

        #region 【构造函数】
        public ListFamily()
        {
            Mark = MarkType.None;
            DisplayWeight = 0;
            ShowFinishedItem = true;
            ShowOverdueItem = true;
            ShowAbandonedItem = false;
            SortType = SortType.Mark;
            SortSequence = SortSequence.Asc;
        }
        #endregion

        #region 【静态方法】
        /// <summary>
        /// 获取ListFamilies列表
        /// </summary>
        /// <returns></returns>
        public static DataTable GetListFamiliesTable()
        {
            DataTable dt = sqlite.GetDataTable(string.Format("SELECT * FROM {0} ORDER BY {1} ASC", TABLE_NAME, FIELD_NAME_DISPLAY_WEIGHT));
            return dt;
        }

        /// <summary>
        /// 添加ListFamily
        /// </summary>
        /// <param name="listFamily"></param>
        public static void AddListFamily(ListFamily listFamily)
        {
            sqlite.CreateTableIfNotExists(
                TABLE_NAME,                    //表名
                FIELD_NAME_ID,                 //主键名
                true,                          //自增
                FIELD_NAME_TITLE,
                FIELD_NAME_DETAIL,
                FIELD_NAME_MARK,
                FIELD_NAME_DISPLAY_WEIGHT,
                FIELD_NAME_SHOW_FINISHED_ITEM,
                FIELD_NAME_SHOW_OVERDUE_ITEM,
                FIELD_NAME_SHOW_ABANDONED_ITEM,
                FIELD_NAME_SORT_TYPE);

            List<string> columnsList = new List<string> 
            { 
                FIELD_NAME_TITLE, 
                FIELD_NAME_DETAIL, 
                FIELD_NAME_MARK, 
                FIELD_NAME_DISPLAY_WEIGHT,
                FIELD_NAME_SHOW_FINISHED_ITEM,
                FIELD_NAME_SHOW_OVERDUE_ITEM,
                FIELD_NAME_SHOW_ABANDONED_ITEM,
                FIELD_NAME_SORT_TYPE
            };
            List<string> valuesList = new List<string>
            {
                listFamily.Title,
                listFamily.Detail,
                listFamily.Mark.ToString("d"),
                listFamily.DisplayWeight.ToString(),
                Convert.ToInt16(listFamily.ShowFinishedItem).ToString(),
                Convert.ToInt16(listFamily.ShowOverdueItem).ToString(),
                Convert.ToInt16(listFamily.ShowAbandonedItem).ToString(),
                Convert.ToInt16(listFamily.SortType).ToString()
            };
            sqlite.Insert(TABLE_NAME, columnsList, valuesList);
        }

        /// <summary>
        /// 更新ListFamily
        /// </summary>
        /// <param name="listFamily"></param>
        public static void UpdateListFamily(ListFamily listFamily)
        {
            List<string> columnsList = new List<string>
            {
                FIELD_NAME_TITLE,
                FIELD_NAME_DETAIL,
                FIELD_NAME_MARK,
                FIELD_NAME_DISPLAY_WEIGHT,
                FIELD_NAME_SHOW_FINISHED_ITEM,
                FIELD_NAME_SHOW_OVERDUE_ITEM,
                FIELD_NAME_SHOW_ABANDONED_ITEM,
                FIELD_NAME_SORT_TYPE
            };

            List<string> valuesList = new List<string>
            {
                listFamily.Title,
                listFamily.Detail,
                listFamily.Mark.ToString("d"),
                listFamily.DisplayWeight.ToString(),
                Convert.ToInt16(listFamily.ShowFinishedItem).ToString(),
                Convert.ToInt16(listFamily.ShowOverdueItem).ToString(),
                Convert.ToInt16(listFamily.ShowAbandonedItem).ToString(),
                Convert.ToInt16(listFamily.SortType).ToString()
            };

            sqlite.Update(TABLE_NAME, columnsList, valuesList, string.Format("{0} = '{1}'", FIELD_NAME_ID, listFamily.Id));
        }

        public static void UpdateAllListFamilies(List<string> columnsList, List<string> valuesList)
        {
            sqlite.Update(TABLE_NAME, columnsList, valuesList);
        }

        /// <summary>
        /// 删除ListFamily以及其所有ListChildren
        /// </summary>
        /// <param name="listFamily"></param>
        public static void DeleteListFamily(ListFamily listFamily)
        {
            sqlite.Delete(TABLE_NAME, string.Format("{0} = '{1}'", FIELD_NAME_ID, listFamily.Id));
            sqlite.Delete(ListChild.TABLE_NAME, string.Format("{0} = '{1}'", ListChild.FIELD_NAME_FAMILY_ID, listFamily.Id));
        }

        /// <summary>
        /// 通过Id获取ListFamily
        /// </summary>
        /// <param name="listFamilyId"></param>
        /// <returns></returns>
        public static ListFamily GetListFamilyById(string listFamilyId)
        {
            DataRow dr = sqlite.GetDataRow(string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, FIELD_NAME_ID, listFamilyId));

            ListFamily listFamily = new ListFamily
                {
                    Id = int.Parse(dr[FIELD_NAME_ID].ToString()),
                    Title = dr[FIELD_NAME_TITLE].ToString(),
                    Detail = dr[FIELD_NAME_DETAIL].ToString(),
                    Mark = (MarkType) Enum.Parse(typeof (MarkType), dr[FIELD_NAME_MARK].ToString(), true),
                    DisplayWeight = int.Parse(dr[FIELD_NAME_DISPLAY_WEIGHT].ToString()),
                    ShowFinishedItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_FINISHED_ITEM]),
                    ShowOverdueItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_OVERDUE_ITEM]),
                    ShowAbandonedItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_ABANDONED_ITEM]),
                    SortType = (SortType) Enum.Parse(typeof (SortType), dr[FIELD_NAME_SORT_TYPE].ToString(), true)
                };

            return listFamily;
        }

        public static ListFamily GetListFamilyByWeight(int displayWeight)
        {
            DataRow dr = sqlite.GetDataRow(string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", TABLE_NAME, FIELD_NAME_DISPLAY_WEIGHT, displayWeight));

            ListFamily listFamily = new ListFamily
                {
                    Id = int.Parse(dr[FIELD_NAME_ID].ToString()),
                    Title = dr[FIELD_NAME_TITLE].ToString(),
                    Detail = dr[FIELD_NAME_DETAIL].ToString(),
                    Mark = (MarkType) Enum.Parse(typeof (MarkType), dr[FIELD_NAME_MARK].ToString(), true),
                    DisplayWeight = int.Parse(dr[FIELD_NAME_DISPLAY_WEIGHT].ToString()),
                    ShowFinishedItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_FINISHED_ITEM]),
                    ShowOverdueItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_OVERDUE_ITEM]),
                    ShowAbandonedItem = Convert.ToBoolean(dr[FIELD_NAME_SHOW_ABANDONED_ITEM]),
                    SortType = (SortType) Enum.Parse(typeof (SortType), dr[FIELD_NAME_SORT_TYPE].ToString(), true)
                };

            return listFamily;
        }

        public static void BeginTransaction()
        {
            sqlite.BeginTransaction();
        }

        public static void CommitTransaction()
        {
            sqlite.CommitTransaction();
        }

        #endregion
    }
}
