using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SDFilter;
using WH.RunCell;

namespace MySqlOperatesApi
{
    public class CMysqlBLL : MySqlOperate
    {
        bool DBExists = false;
        bool totalExists = false; //避免重复读取表是否存在
        #region 表头名

        string tableName_total = "TotalRecord";

        string Tital = "流水ID,WaferID,创建时间,图像采集,预处理时间,算法时间,过滤时间,显示时间,检测耗时,质量等级,质量等级信号,结果,不良类型,定级缺陷";

        #endregion

        private static readonly object _addLock = new object();
        private static readonly object _readLock = new object();

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 添加数据到数据库
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="date">当前班次</param>
        public override void AddData(Cell cell, string date)
        {
            lock (_addLock)
            {
                string daname = this.DataBaseName;
                if (!DBExists)
                {
                    bool bDBExists = IsDatabaseExists(daname);
                    DBExists = bDBExists;
                    if (!bDBExists)
                    {
                        CreateDatabase(daname);
                    }
                }
                if (tableName_total != date)
                {
                    string titalname = GetTableNameStr(cell, out _); //表抬头

                    bool tableExists = IsTableExists(date);

                    if (!tableExists)
                    {
                        CreateTable(ComTableString(titalname), date); //创建toatlrecord表
                    }
                    tableName_total = date;
                }
                //if (!pereExists)
                //{
                //    string titalname = GetTableNameStr(cell, out _);//表抬头
                //    titalname = titalname + "," + PreStr;

                //    bool tableExists2 = IsTableExists(tableName_pere);
                //    pereExists = tableExists2;
                //    if (!tableExists2)
                //    {
                //        CreateTable(ComTableString(titalname), tableName_pere);//创建perecord表
                //    }

                //}
                //if (!summarylExists)
                //{

                //    bool tableExists3 = IsTableExists(tableName_summ);
                //    summarylExists = tableExists3;
                //    if (!tableExists3)
                //    {
                //        CreateTable(ComTableString2(SummaryStr), tableName_summ);//创建summary表
                //    }
                //}

                InsertToatlRecord(cell);
                //InsertSummary(cell);
            }
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 添加数据到表中
        /// </summary>
        /// <param name="cell">cell</param>
        private void InsertToatlRecord(Cell cell)
        {
            //总表

            try
            {
                //"流水ID,WaferID,创建时间,图像采集,预处理时间,算法时间,过滤时间,显示时间,检测耗时,质量等级,质量等级信号,结果，不良类型,定级缺陷";
                string strSql =
                    "'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}'";
                string valuestrSql = string.Format(
                    strSql,
                    cell.ID,
                    cell.WaferID,
                    string.Format("{0:yyyy-MM-dd HH:mm:ss:fff}", cell.CreateTime),
                    cell.GetImageTime.TotalMilliseconds.ToString("F1"),
                    cell.PreTime.TotalMilliseconds.ToString("F1"),
                    cell.RecipeTime.TotalMilliseconds.ToString("F1"),
                    cell.FilterTime.TotalMilliseconds.ToString("F1"),
                    cell.ShowTime.TotalMilliseconds.ToString("F1"),
                    cell.ProcessTime.TotalMilliseconds.ToString("F1"),
                    cell.Quality.Name,
                    cell.Quality.Signal,
                    cell.IsOK ? "OK" : "NG",
                    cell.Detection?.Type,
                    cell.Detection?.DefectFilter?.Name
                );

                string colname = GetTableNameStr(cell, out string othervalue);
                valuestrSql = valuestrSql + othervalue;
                Insert(tableName_total, colname, valuestrSql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 获取表头sql语句
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="otherValuestr"></param>
        /// <returns></returns>
        private string GetTableNameStr(Cell cell, out string otherValuestr)
        {
            GetOtherRevInfo(cell, out string[] colunmName, out string[] colunmvalues);
            string columnNamestr = string.Empty;
            otherValuestr = string.Empty;
            for (int i = 0; i < colunmName.Length; i++)
            {
                columnNamestr = columnNamestr + "," + colunmName[i];
                otherValuestr = otherValuestr + "," + $"'{colunmvalues[i]}'";
            }
            columnNamestr = Tital + columnNamestr;
            return columnNamestr;
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 创建表头sql语句
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        private string ComTableString(string strs)
        {
            string[] arrStr = strs.Split(',');
            StringBuilder sb = new StringBuilder();
            sb.Append("`NO` int NOT NULL AUTO_INCREMENT PRIMARY KEY,");
            for (int i = 0; i < arrStr.Length; i++)
            {
                string str;

                if (i == arrStr.Length - 1)
                {
                    str = $"`{arrStr[i]}` varchar(45) DEFAULT NULL"; //如果是最后一个不加逗号
                }
                else
                {
                    str = $"`{arrStr[i]}` varchar(45) DEFAULT NULL,";
                }

                sb.Append(str);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 判断时间段是否存在
        /// </summary>
        /// <returns></returns>
        private bool CreatedTimeExists(string createdTime)
        {
            _mySqlHelper.connString = connectStringCreateTable;
            object obj = _mySqlHelper.ExecuteScalar(
                string.Format("select No from summary where CreatedTime='{0}' limit 1", createdTime)
            );
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 2024.7.7 鲍赞宝
        /// 查询数据 tableList入参为空时查询可能包含该时间段内数据的表 返回查询到的表名
        /// </summary>
        /// <param name="dateNow">待查询表名称数组</param>
        /// <returns></returns>
        public override DataSet QueryData(List<string> tableList, string start, string end)
        {
            string sqlconn = string.Empty;
            string strconn = "";

            var startStr = $"'{start}'";
            var endStr = $"'{end}'";
            if (tableList.Count == 0)
            {
                string closestTable = "";
                string searchClosestTable = string.Format(
                    @"SELECT TABLE_NAME, CREATE_TIME
                    FROM information_schema.tables
                    WHERE TABLE_SCHEMA = '{0}'
                    AND CREATE_TIME <= '{1}'
                    ORDER BY ABS(TIMESTAMPDIFF(SECOND, CREATE_TIME, '{1}')) ASC
                    LIMIT 1",
                    DataBaseName,
                    start
                );
                string searchTables = string.Format(
                    @"SELECT TABLE_NAME, CREATE_TIME 
                    FROM information_schema.tables 
                    WHERE TABLE_SCHEMA = '{0}' 
                    AND CREATE_TIME BETWEEN {1} AND {2}",
                    DataBaseName,
                    startStr,
                    endStr
                );
                _mySqlHelper.ExecuteReader(
                    searchClosestTable,
                    new Action<MySql.Data.MySqlClient.MySqlDataReader>(reader =>
                    {
                        while (reader.Read())
                        {
                            closestTable = reader["TABLE_NAME"].ToString();
                        }
                    })
                );
                tableList = new List<string>();
                if (!string.IsNullOrEmpty(closestTable))
                {
                    tableList.Add(closestTable);
                }
                _mySqlHelper.ExecuteReader(
                    searchTables,
                    new Action<MySql.Data.MySqlClient.MySqlDataReader>(reader =>
                    {
                        while (reader.Read())
                        {
                            tableList.Add(reader["TABLE_NAME"].ToString());
                        }
                    })
                );
            }

            if (tableList.Count > 0)
            {
                if (tableList.Count == 1)
                {
                    sqlconn = tableList[0];
                }
                else
                {
                    strconn = "AS combined_tables";
                    for (int i = 0; i < tableList.Count; i++)
                    {
                        if (i == (tableList.Count - 1))
                        {
                            sqlconn += $" SELECT * FROM {tableList[i]}";
                        }
                        else
                        {
                            sqlconn += $" SELECT * FROM {tableList[i]} UNION ALL ";
                        }
                    }
                }

                _mySqlHelper.connString = connectStringCreateTable;
                DataSet datatable;
                StringBuilder queryStr = new StringBuilder(
                    "SELECT DATE_FORMAT(创建时间, '%Y-%m-%d %H:00') AS 时段,"
                );
                queryStr.Append("COUNT(*) AS 生产数,");
                queryStr.Append("SUM(CASE WHEN 结果 = 'OK' THEN 1 ELSE 0 END) AS OK数量,");
                queryStr.Append("SUM(CASE WHEN 结果 = 'NG' THEN 1 ELSE 0 END) AS NG数量,");
                queryStr.Append(
                    "CONCAT(FORMAT(IFNULL((SUM(CASE WHEN 结果 = 'NG' THEN 1 ELSE 0 END) / COUNT(*)) * 100, 0), 2), '%') AS 总缺陷占比"
                );
                foreach (var de in defectList)
                {
                    queryStr.Append(
                        string.Format(
                            ",SUM(CASE WHEN 定级缺陷 = '{0}' THEN 1 ELSE 0 END) AS {0}数量 ",
                            de.Name
                        )
                    );
                }
                queryStr.Append(
                    " FROM ({0}){1} WHERE 创建时间 >= {2} AND 创建时间 <= {3} GROUP BY DATE_FORMAT(创建时间, '%Y-%m-%d %H:00')"
                );
                string querySql = string.Format(
                    queryStr.ToString(),
                    sqlconn,
                    strconn,
                    startStr,
                    endStr
                );

                datatable = _mySqlHelper.GetDataSet(querySql);
                return datatable;
            }
            else
            {
                return new DataSet();
            }
        }
    }
}
