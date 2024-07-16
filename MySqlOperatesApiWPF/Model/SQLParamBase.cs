using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MySqlOperatesApiWPF
{
    public partial class SQLParamBase : ObservableObject
    {
        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 数据库名称
        /// </summary>
        [ObservableProperty]
        private string dataBaseName = "Total";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 远程数据库IP
        /// </summary>
        [ObservableProperty]
        private string remoteIP = "127.0.0.1";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 远程数据库端口
        /// </summary>
        [ObservableProperty]
        private string remotePort = "3306";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 用户名
        /// </summary>
        [ObservableProperty]
        private string userID = "root";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 密码
        /// </summary>
        [ObservableProperty]
        private string passWord = "123456";

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 线别
        /// </summary>
        [ObservableProperty]
        private string lineName;

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 设备名
        /// </summary>
        [ObservableProperty]
        private string dbDeviceName;

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 开启上传MES数据
        /// </summary>
        [ObservableProperty]
        private bool sqlEnable = true;

        /// <summary>
        /// 2024.6.23 鲍赞宝
        /// 用户
        /// </summary>
        [ObservableProperty]
        private string useSelect = "通用";

        public void Clone(ref SQLBase newSQL)
        {
            newSQL.DataBaseName = this.DataBaseName;
            newSQL.RemoteIP = this.RemoteIP;
            newSQL.RemotePort = this.RemotePort;
            newSQL.UseSelect = this.UseSelect;
            newSQL.SqlEnable = this.SqlEnable;
            newSQL.LineName = this.LineName;
            newSQL.UserID = this.UserID;
            newSQL.DbDeviceName = this.DbDeviceName;
            newSQL.PassWord = this.PassWord;
        }
    }
}
