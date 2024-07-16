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

namespace MySqlOperatesApiWPF
{
    public partial class MySqlViewModel : ObservableObject
    {
        public CMysqlBLL mysqlExecute { get; set; }

        public MySqlViewModel()
        {
            mysqlExecute = SQLManagement.SqlLoad() as CMysqlBLL;

            mysqlExecute.connectStringCreateDB = string.Format(
                "Data Source={0};Port={1};User Id={2};PassWord={3};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                mysqlExecute.RemoteIP,
                mysqlExecute.RemotePort,
                mysqlExecute.UserID,
                mysqlExecute.PassWord
            );

            mysqlExecute.connectStringCreateTable = string.Format(
                "Database={0};Data Source={1};Port={2};User Id={3};PassWord={4};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                mysqlExecute.DataBaseName,
                mysqlExecute.RemoteIP,
                mysqlExecute.RemotePort,
                mysqlExecute.UserID,
                mysqlExecute.PassWord
            );
        }

        [RelayCommand]
        void SaveParam()
        {
            SQLManagement.SqlSave(mysqlExecute);

            mysqlExecute.connectStringCreateDB = string.Format(
                "Data Source={0};Port={1};User Id={2};PassWord={3};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                mysqlExecute.RemoteIP,
                mysqlExecute.RemotePort,
                mysqlExecute.UserID,
                mysqlExecute.PassWord
            );

            mysqlExecute.connectStringCreateTable = string.Format(
                "Database={0};Data Source={1};Port={2};User Id={3};PassWord={4};Charset=utf8;"
                    + "Persist Security Info=True;TreatTinyAsBoolean=true;allow zero datetime=true;",
                mysqlExecute.DataBaseName,
                mysqlExecute.RemoteIP,
                mysqlExecute.RemotePort,
                mysqlExecute.UserID,
                mysqlExecute.PassWord
            );
        }
    }
}
