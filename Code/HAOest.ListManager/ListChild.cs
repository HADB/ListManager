using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using HAOest.Data.SQLite;

namespace HAOest.ListManager
{
    public class ListChild
    {
        public const string TABLE_NAME = "list_children";
        public const string FIELD_NAME_ID = "list_child_id";
        public const string FIELD_NAME_TITLE = "list_child_title";
        public const string FIELD_NAME_DETAIL = "list_child_detail";
        public const string FIELD_NAME_START_TIME = "list_child_start_time";
        public const string FIELD_NAME_END_TIME = "list_child_end_time";
        public const string FIELD_NAME_MARK = "list_child_mark";
        public const string FIELD_NAME_DISPLAY_WEIGHT = "list_child_display_weight";
        public const string FIELD_NAME_FAMILY_ID = "list_family_id";

        private static SQLite sqlite = new SQLite("data.db");


        #region 【ListChild 的属性】
        public int Id { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public MarkType Mark { get; set; }
        public int DisplayWeight { get; set; }
        public int FamilyId { get; set; }

        #endregion

        #region 【构造函数】
        /// <summary>
        /// 构造函数
        /// </summary>
        public ListChild()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.MaxValue;
            Mark = MarkType.None;
            DisplayWeight = 0;
        }
        #endregion

        #region 【静态方法】
        /// <summary>
        /// 添加 ListChild
        /// </summary>
        /// <param name="listChild"></param>
        public static void AddListChild(ListChild listChild)
        {
            try
            {
                sqlite.CreateTableIfNotExists(
                    TABLE_NAME,                         //表名
                    FIELD_NAME_ID,                      //主键名
                    true,                               //自增
                    FIELD_NAME_TITLE,
                    FIELD_NAME_DETAIL,
                    FIELD_NAME_START_TIME,
                    FIELD_NAME_END_TIME,
                    FIELD_NAME_MARK,
                    FIELD_NAME_DISPLAY_WEIGHT,
                    FIELD_NAME_FAMILY_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            List<string> columnList = new List<string>
            { 
                FIELD_NAME_TITLE, 
                FIELD_NAME_DETAIL, 
                FIELD_NAME_START_TIME, 
                FIELD_NAME_END_TIME,
                FIELD_NAME_MARK, 
                FIELD_NAME_DISPLAY_WEIGHT,
                FIELD_NAME_FAMILY_ID
            };
            List<string> valueList = new List<string>
            {
                    listChild.Title,
                    listChild.Detail,
                    listChild.StartTime.ToString(Global.TIME_FORMAT_STRING),
                    listChild.EndTime.ToString(Global.TIME_FORMAT_STRING),
                    listChild.Mark.ToString("d"),
                    listChild.DisplayWeight.ToString(),
                    listChild.FamilyId.ToString()
            };

            sqlite.Insert(TABLE_NAME, columnList, valueList);
        }

        /// <summary>
        /// 更新ListChild
        /// </summary>
        /// <param name="listChild"></param>
        public static void UpdateListChild(ListChild listChild)
        {
            List<string> columnsList = new List<string>
                {
                    FIELD_NAME_TITLE,
                    FIELD_NAME_DETAIL,
                    FIELD_NAME_START_TIME,
                    FIELD_NAME_END_TIME,
                    FIELD_NAME_MARK,
                    FIELD_NAME_DISPLAY_WEIGHT,
                };

            List<string> valuesList = new List<string>
                {
                    listChild.Title,
                    listChild.Detail,
                    listChild.StartTime.ToString(Global.TIME_FORMAT_STRING),
                    listChild.EndTime.ToString(Global.TIME_FORMAT_STRING),
                    listChild.Mark.ToString("d"),
                    listChild.DisplayWeight.ToString()
                };

            sqlite.Update(TABLE_NAME, columnsList, valuesList, string.Format("{0} = '{1}'", FIELD_NAME_ID, +listChild.Id));
        }

        /// <summary>
        /// 删除ListChild
        /// </summary>
        /// <param name="listChild"></param>
        public static void DeleteListChild(ListChild listChild)
        {
            sqlite.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} ='{2}'", TABLE_NAME, FIELD_NAME_ID, listChild.Id));
        }

        /// <summary>
        /// 通过DataRow获取ListChild
        /// </summary>
        /// <param name="dataRow"></param>
        /// <returns></returns>
        public static ListChild GetListChildByDataRow(DataRow dataRow)
        {
            ListChild listChild = new ListChild();
            listChild.Id = Convert.ToInt16(dataRow[FIELD_NAME_ID].ToString());
            listChild.Title = Convert.ToString(dataRow[FIELD_NAME_TITLE]);
            listChild.Detail = Convert.ToString(dataRow[FIELD_NAME_DETAIL]);
            listChild.StartTime = Convert.ToDateTime(dataRow[FIELD_NAME_START_TIME].ToString());
            listChild.EndTime = Convert.ToDateTime(dataRow[FIELD_NAME_END_TIME].ToString());
            listChild.Mark = (MarkType)Enum.Parse(typeof(MarkType), dataRow[FIELD_NAME_MARK].ToString(), true);
            listChild.DisplayWeight = Convert.ToInt16(dataRow[FIELD_NAME_DISPLAY_WEIGHT].ToString());
            listChild.FamilyId = Convert.ToInt16(dataRow[FIELD_NAME_FAMILY_ID].ToString());
            return listChild;
        }

        /// <summary>
        /// 通过Id获取ListChild
        /// </summary>
        /// <param name="listFamilyId"></param>
        /// <param name="listChildId"></param>
        /// <returns></returns>
        public static ListChild GetListChildById(int listChildId)
        {
            DataRow dataRow = sqlite.GetDataRow("SELECT * FROM " + TABLE_NAME + " WHERE " + FIELD_NAME_ID + " = '" + listChildId + "'");
            return GetListChildByDataRow(dataRow);
        }

        /// <summary>
        /// 获取ListChildren列表
        /// </summary>
        /// <param name="familyId"></param>
        /// <returns></returns>
        public static DataTable GetListChildrenTable(ListFamily listFamily)
        {
            UpdateOverdueChildren();

            string sortString = null;
            switch (listFamily.SortType)
            {
                case SortType.Default:
                    sortString = ListChild.FIELD_NAME_MARK + " " + listFamily.SortSequence.ToString();
                    break;
                case SortType.Mark:
                    sortString = ListChild.FIELD_NAME_MARK + " " + listFamily.SortSequence.ToString();
                    break;
                case SortType.StartTime:
                    sortString = ListChild.FIELD_NAME_START_TIME + " " + listFamily.SortSequence.ToString();
                    break;
                case SortType.EndTime:
                    sortString = ListChild.FIELD_NAME_END_TIME + " " + listFamily.SortSequence.ToString();
                    break;
                case SortType.Title:
                    sortString = ListChild.FIELD_NAME_TITLE + " COLLATE PinYin" + " " + listFamily.SortSequence.ToString();
                    break;
                case SortType.Manual:
                    sortString = ListChild.FIELD_NAME_DISPLAY_WEIGHT + " " + listFamily.SortSequence.ToString();
                    break;
            }

            StringBuilder condition = new StringBuilder();
            if (!listFamily.ShowFinishedItem)
            {
                condition.Append(string.Format(" AND {0} <> '{1}'", ListChild.FIELD_NAME_MARK, Convert.ToInt16(MarkType.Finished)));
            }
            if (!listFamily.ShowOverdueItem)
            {
                condition.Append(string.Format(" AND {0} <> '{1}'", ListChild.FIELD_NAME_MARK, Convert.ToInt16(MarkType.Overdue)));
            }
            if (!listFamily.ShowAbandonedItem)
            {
                condition.Append(string.Format(" AND {0} <> '{1}'", ListChild.FIELD_NAME_MARK, Convert.ToInt16(MarkType.Abandoned)));
            }

            DataTable dt = sqlite.GetDataTable(string.Format("SELECT * FROM {0} WHERE {1} = '{2}' {3} ORDER BY {4}", ListChild.TABLE_NAME, ListChild.FIELD_NAME_FAMILY_ID, listFamily.Id, condition.ToString(), sortString));
            return dt;
        }

        public static void UpdateOverdueChildren()
        {
            DataTable dt = sqlite.GetDataTable(string.Format(("SELECT * FROM {0} WHERE {1} < '{2}'"), ListChild.TABLE_NAME, ListChild.FIELD_NAME_END_TIME, DateTime.Now.ToString(Global.TIME_FORMAT_STRING)));
            if (dt.Rows.Count > 0)
            {
                sqlite.BeginTransaction();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListChild listChild = ListChild.GetListChildById(Convert.ToInt32(dt.Rows[i][ListChild.FIELD_NAME_ID]));
                    if ((listChild.Mark != MarkType.Overdue) && (listChild.Mark != MarkType.Abandoned))
                    {
                        listChild.Mark = MarkType.Overdue;
                        UpdateListChild(listChild);
                    }
                }
                sqlite.CommitTransaction();
            }
        }

        /// <summary>
        /// 开启事务模式
        /// </summary>
        public static void BeginTransaction()
        {
            sqlite.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public static void CommitTransaction()
        {
            sqlite.CommitTransaction();
        }


        #endregion
    }
}
