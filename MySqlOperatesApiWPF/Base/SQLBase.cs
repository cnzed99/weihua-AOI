
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.RunCell;


namespace MySqlOperatesApiWPF
{
    public abstract class SQLBase: SQLParamBase
    {
        public static object LockObj = new object();

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="DBName">数据库路径和名称</param>
        /// <returns></returns>
        public abstract bool CreateDatabase(string DBName);
       /// <summary>
       /// 创建表
       /// </summary>
       /// <param name="sql"></param>
       /// <returns></returns>
        public abstract void  CreateTable(string sql,string tablename);

       /// <summary>
       /// 添加数据到表中
       /// </summary>
       /// <param name="cell">产品</param>
       /// <param name="date">日期</param>
        public abstract void AddData(Cell cell,string date);

        /// <summary>
        /// 从表中读取数据
        /// </summary>
        /// <param name="date">日期集合</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        public abstract DataSet QueryData(List<string> date,string start,string end);
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableHeader">表头</param>
        /// <param name="Datastr">值</param>
        /// <returns></returns>
        public abstract bool Insert(string tableName,string tableHeader, string Datastr);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public abstract bool UpDate(string sql);



    }
}


