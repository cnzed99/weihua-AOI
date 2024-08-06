using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MySqlOperatesApi
{
    public partial class CMySqlVM : ObservableObject
    {
        private CMysqlBLL mysqlBLL;
        public CMysqlBLL MysqlExecute
        {
            get => mysqlBLL;
            set
            {
                mysqlBLL = value;
                MysqlExecute.connectStringCreateDB = string.Format(
                    "Data Source={0};Port={1};User Id={2};PassWord={3};Charset=utf8;"
                        + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                    MysqlExecute.RemoteIP,
                    MysqlExecute.RemotePort,
                    MysqlExecute.UserID,
                    MysqlExecute.PassWord
                );

                MysqlExecute.connectStringCreateTable = string.Format(
                    "Database={0};Data Source={1};Port={2};User Id={3};PassWord={4};Charset=utf8;"
                        + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                    MysqlExecute.DataBaseName,
                    MysqlExecute.RemoteIP,
                    MysqlExecute.RemotePort,
                    MysqlExecute.UserID,
                    MysqlExecute.PassWord
                );
            }
        }

        public CMySqlVM() { }

        [RelayCommand]
        void SaveParam()
        {
            SQLManagement.SqlSave(MysqlExecute);

            MysqlExecute.connectStringCreateDB = string.Format(
                "Data Source={0};Port={1};User Id={2};PassWord={3};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                MysqlExecute.RemoteIP,
                MysqlExecute.RemotePort,
                MysqlExecute.UserID,
                MysqlExecute.PassWord
            );

            MysqlExecute.connectStringCreateTable = string.Format(
                "Database={0};Data Source={1};Port={2};User Id={3};PassWord={4};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                MysqlExecute.DataBaseName,
                MysqlExecute.RemoteIP,
                MysqlExecute.RemotePort,
                MysqlExecute.UserID,
                MysqlExecute.PassWord
            );
        }
    }
}
