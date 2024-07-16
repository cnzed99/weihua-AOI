using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MySqlOperatesApiWPF
{
    public class MySqlHelper
    {
        public string connString = string.Empty;

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 执行增 删 改语句
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, MySqlParameter[] paramArray = null)
        {

            if (connString != string.Empty)
            {
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = new MySqlCommand(cmdText, conn);
                if (paramArray != null)
                {
                    command.Parameters.AddRange(paramArray);
                }
                try
                {
                    conn.Open();
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("执行public static int ExecuteNonQuery(string cmdText, OleDbParameter[] paramArray = null)方法发生异常" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                    command.Dispose();
                    conn.Dispose();
                    command = null;
                    conn = null;
                }


            }
            else
            {
                throw new Exception("connString 连接字符串为空,请检查connString是否赋值");
            }
        }



        #region 查询
        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 返回单一结果的查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string cmdText, MySqlParameter[] paramArray = null)
        {

            if (connString != string.Empty)
            {
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = new MySqlCommand(cmdText, conn);
                if (paramArray != null)
                {
                    command.Parameters.AddRange(paramArray);
                }
                try
                {
                    conn.Open();
                    return command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行public static object ExecuteScalar(string cmdText, OleDbParameter[] paramArray = null)方法发生异常:{ex.Message}{ex.StackTrace}");
                }
                finally
                {
                    conn.Close();
                    command.Dispose();
                    conn.Dispose();
                    command = null;
                    conn = null;
                }

            }
            else
            {
                throw new Exception("connString 连接字符串为空,请检查connString是否赋值");
            }
        }


        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 执行返回一个只读结果集的查询
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public MySqlDataReader ExecuteReader(string cmdText, MySqlParameter[] paramArray = null)
        {

            if (connString != string.Empty)
            {
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = new MySqlCommand(cmdText, conn);
                if (paramArray != null)
                {
                    command.Parameters.AddRange(paramArray);
                }
                try
                {
                    conn.Open();
                    return command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    throw new Exception("执行public static MySqlDataReader ExecuteReader(string cmdText, OleDbParameter[] paramArray = null)方法发生异常" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                    command.Dispose();
                    conn.Dispose();
                    command = null;
                    conn = null;
                }

            }
            else
            {
                throw new Exception("connString 连接字符串为空,请检查connString是否赋值");
            }

        }

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 返回包含一张数据表的数据集的查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="tableName">数据表的名称</param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, string tableName = null)
        {

            if (connString != string.Empty)
            {
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(command);
                DataSet ds = new DataSet();

                try
                {
                    conn.Open();
                    if (tableName == null)
                        da.Fill(ds);
                    else
                        da.Fill(ds, tableName);
                    return ds;
                }
                catch (Exception ex)
                {
                    throw new Exception("执行 public DataSet GetDataSet(string sql, string tableName = null)方法发生异常：" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                    da.Dispose();
                    command.Dispose();
                    conn.Dispose();
                    da = null;
                    command = null;
                    conn = null;
                }

            }
            else
            {
                throw new Exception("connString 连接字符串为空,请检查connString是否赋值");
            }

        }


        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 执行查询，返回一个或多个表的DataSet
        /// </summary>
        /// <param name="dicTableAndSql"></param>
        /// <returns></returns>
        public DataSet GetDataSet(Dictionary<string, string> dicTableAndSql)
        {

            if (connString != string.Empty)
            {
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = new MySqlCommand();
                command.Connection = conn;
                MySqlDataAdapter da = new MySqlDataAdapter(command);
                DataSet ds = new DataSet();

                try
                {
                    conn.Open();
                    foreach (string tbName in dicTableAndSql.Keys)
                    {
                        command.CommandText = dicTableAndSql[tbName];
                        da.Fill(ds, tbName);
                    }
                    return ds;
                }
                catch (Exception ex)
                {
                    throw new Exception("执行 public DataSet GetDataSet(string sql, string tableName = null)方法发生异常：" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                    da.Dispose();
                    command.Dispose();
                    conn.Dispose();
                    da = null;
                    command = null;
                    conn = null;
                }

            }
            else
            {
                throw new Exception("connString 连接字符串为空,请检查connString是否赋值");
            }

        }
        #endregion

    }
}
