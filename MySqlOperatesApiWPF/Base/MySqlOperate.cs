using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using WH.RunCell;

namespace MySqlOperatesApi
{
    public class MySqlOperate : SQLBase
    {
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        public string connectStringCreateDB;

        /// <summary>
        /// 连接表字符串
        /// </summary>
        public string connectStringCreateTable;

        /// <summary>
        /// 数据库执行对象
        /// </summary>
        public MySqlHelper _mySqlHelper = new MySqlHelper();

        public override void AddData(Cell cell, string date)
        {
            //在子类中实现写数据
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 创建数据库
        /// </summary>
        /// <param name="DBName">数据库路径和名称</param>
        /// <returns></returns>
        public override bool CreateDatabase(string DBName)
        {
            bool result = false;
            try
            {
                _mySqlHelper.connString = connectStringCreateDB;

                string sql =
                    @"CREATE DATABASE  IF NOT EXISTS {0} CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci";
                sql = string.Format(sql, DBName);
                int linecount = _mySqlHelper.ExecuteNonQuery(sql);
                result = linecount > 0 ? true : false;
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 创建表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public override void CreateTable(string sql, string tablename)
        {
            string createTableQuery = string.Format(
                @"CREATE TABLE IF NOT EXISTS {0} ({1}) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 ;",
                tablename,
                sql
            );
            _mySqlHelper.connString = connectStringCreateTable;

            int linecount = _mySqlHelper.ExecuteNonQuery(createTableQuery);
        }

        public override DataSet QueryData(
            List<string> date,
            string start,
            string end,
            List<string> defectList
        )
        {
            //在子类中实现查询数据
            return null;
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 写入单条sql语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableHeader">表头</param>
        /// <param name="Datastr">值</param>
        /// <returns></returns>
        public override bool Insert(string tableName, string tableHeader, string Datastr)
        {
            try
            {
                string sql = $"insert into {tableName} ({tableHeader}) values ({Datastr})";
                _mySqlHelper.connString = connectStringCreateTable;
                int lcount = _mySqlHelper.ExecuteNonQuery(sql);
                if (lcount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // private static object _lockDatabase = new object();
        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 判断数据库是否已经存在
        /// </summary>
        /// <returns></returns>
        public bool IsDatabaseExists(string dbName)
        {
            string sqlIsExists = "SELECT DATABASE() = '{0}';";
            sqlIsExists = string.Format(sqlIsExists, dbName);
            bool Exists = false;
            try
            {
                _mySqlHelper.connString = connectStringCreateTable;
                object ob = _mySqlHelper.ExecuteScalar(sqlIsExists);
                if (ob != null)
                {
                    Exists = true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return Exists;
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 判断表是否已经存在
        /// </summary>
        /// <returns></returns>
        public bool IsTableExists(string tablename)
        {
            string sqlIsExists =
                "SELECT COUNT(*)  FROM information_schema.TABLES WHERE table_schema='{0}' AND table_name='{1}';";
            sqlIsExists = string.Format(sqlIsExists, this.DataBaseName, tablename);

            //string connectionString = string.Format("Database={0};Data Source={1};Port=3306;User Id={2};PassWord={3};Charset=utf8;" +
            //         "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;", this.DataBaseName, this.IP, this.UserID, this.Password);
            bool Exists = false;
            try
            {
                _mySqlHelper.connString = connectStringCreateTable;
                object ob = _mySqlHelper.ExecuteScalar(sqlIsExists);
                int.TryParse(ob.ToString(), out int count);
                if (count > 0)
                {
                    Exists = true;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Exists;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public override bool UpDate(string sql)
        {
            try
            {
                _mySqlHelper.connString = connectStringCreateTable;
                int lcount = _mySqlHelper.ExecuteNonQuery(sql);
                if (lcount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 获取其他信息
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="columName">表头名</param>
        /// <param name="ColumnsDatas">值</param>
        public void GetOtherRevInfo(Cell cell, out string[] columName, out string[] ColumnsDatas)
        {
            List<string> columNameList = new List<string>();
            List<string> ColumnsDatasList = new List<string>();
            if (cell.OtherInfoRecv != null)
            {
                foreach (var item in cell.OtherInfoRecv.Keys)
                {
                    columNameList.Add(item);
                }
                foreach (var item in cell.OtherInfoRecv.Values)
                {
                    ColumnsDatasList.Add(item);
                }
            }
            columName = columNameList.ToArray();
            ColumnsDatas = ColumnsDatasList.ToArray();
        }
    }
}
