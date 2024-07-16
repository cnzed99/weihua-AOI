using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;

namespace MySqlOperatesApiWPF
{
    public static class SQLManagement
    {
        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 数据库参数保存的路径
        /// </summary>
        public static string ParameterPath = "..\\SystemConfig\\MysqlParam.Json";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 加载数据库配置
        /// </summary>
        /// <returns></returns>
        public static SQLBase SqlLoad()
        {
            try
            {
                if (ParameterPath == null)
                {
                    return null;
                }
                if (!File.Exists(ParameterPath))
                {
                    return new CMysqlBLL();
                }
                else
                {
                    CMysqlBLL sQLBases = ConfigAPI.Load<CMysqlBLL>(ParameterPath);
                    if (sQLBases != null)
                    {
                        return sQLBases;
                    }
                    else
                    {
                        return new CMysqlBLL();
                    }
                }
            }
            catch (Exception)
            {
                return new CMysqlBLL();
            }
        }

        /// <summary>
        ///
        /// 保存数据库配置
        /// </summary>
        /// <param name="mySqlClient">Mysql数据库对象</param>
        public static void SqlSave(SQLBase mySqlClient)
        {
            if (ParameterPath == null)
            {
                return;
            }
            if (mySqlClient == null)
            {
                return;
            }
            ConfigAPI.Save(mySqlClient, ParameterPath);
        }
    }
}
