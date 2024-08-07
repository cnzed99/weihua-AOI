using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;
using Microsoft.Win32;

namespace WH.Entity.IIService
{
    public class WH_IIService
    {
        /// <summary>
        /// 打开帮组文档
        /// </summary>
        /// <param name="path">文档路径</param>
        /// <param name="name">站点名称</param>
        /// <param name="port">端口号</param>
        public static void OpenHelpFile(
            string path = @"C:\dist",
            string name = "WHEditor_Run",
            int port = 89
        )
        {
            if (!IsIISInstalled())
                InstallIIS();
            OpenIISService();
            var siteName = name;
            var physicalPath = path; // 修改为你的网站代码路径
            //var bindingInformation = "*:8080:localhost"; // 绑定到localhost的8080端口
            SetClassicModeForAppPool("DefaultAppPool");
            using (ServerManager serverManager = new ServerManager())
            {
                if (serverManager.Sites.ToList().Find(s => s.Name == siteName) is null)
                {
                    // 创建站点
                    Site site = serverManager.Sites.Add(siteName, physicalPath, port);

                    // 设置应用程序池
                    site.ApplicationDefaults.ApplicationPoolName = "DefaultAppPool"; // 或者其他合适的应用池

                    // 提交更改并关闭
                    serverManager.CommitChanges();
                }
            }

            Console.WriteLine($"网站 {siteName} 已创建。");
        }

        private static bool IsIISInstalled()
        {
            // 检查IIS的安装状态通过查询注册表
            try
            {
                // 获取IIS的安装状态
                string iisKey = @"SOFTWARE\Microsoft\InetStp";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(iisKey);
                if (rk == null)
                {
                    // 如果注册表项不存在，则IIS可能未安装
                    return false;
                }
                else
                {
                    // 如果注册表项存在，则可以通过其他值来判断IIS的安装状态
                    // 例如，可以检查MajorVersion值
                    string majorVersion = rk.GetValue("MajorVersion", "").ToString();
                    return !string.IsNullOrEmpty(majorVersion);
                }
            }
            catch
            {
                // 如果出现任何异常，可以假定IIS未安装
                return false;
            }
        }

        private static void InstallIIS()
        {
            ManagementClass mc = new ManagementClass("Win32_ServerFeature");
            ManagementBaseObject inParams = mc.GetMethodParameters("Install");
            inParams["FeatureID"] = 3; // 3 是IIS的FeatureID
            ManagementBaseObject outParams = mc.InvokeMethod("Install", inParams, null);
            uint returnValue = (uint)(outParams["ReturnValue"]);
            if (returnValue == 0)
            {
                Console.WriteLine("IIS installed successfully.");
            }
            else
            {
                Console.WriteLine("Failed to install IIS. Return Value: " + returnValue);
            }
        }

        private static void OpenIISService()
        {
            ServiceController iisService = new ServiceController("IIS Admin");

            try
            {
                // 检查服务是否已经运行
                if (iisService.Status != ServiceControllerStatus.Running)
                {
                    // 打开服务
                    iisService.Start();

                    // 等待服务启动
                    iisService.WaitForStatus(ServiceControllerStatus.Running);
                }

                Console.WriteLine("IIS服务已启动。");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("无法找到IIS服务。");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误: " + ex.Message);
            }
        }

        public static void SetClassicModeForAppPool(string appPoolName)
        {
            // 创建一个ServerManager的实例
            using (ServerManager serverManager = new ServerManager())
            {
                // 获取指定的应用程序池
                ApplicationPool appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool == null)
                {
                    throw new Exception($"应用程序池 {appPoolName} 不存在。");
                }

                // 设置托管模式为经典
                appPool.ManagedPipelineMode = ManagedPipelineMode.Classic;

                // 保存更改
                serverManager.CommitChanges();
            }
        }
    }
}
