using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WH.Entity
{
    /// <summary>
    /// 20241021 TCG
    /// APP配置文件 自动对焦 打标等模块配置
    /// </summary>
    public static class AppConfig
    {
        public static IConfiguration Config = new ConfigurationBuilder()
            .AddJsonFile("appConfig.json")
            .Build();

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 是否有打标设置
        /// </summary>
        /// <returns>true则有</returns>
        public static bool HasMarkConfig()
        {
            try
            {
                if (
                    bool.TryParse(
                        AppConfig.Config["App:Config:hasMarkConfig"],
                        out bool hasMarkConfig
                    ) && hasMarkConfig
                )
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 是否有对焦控制
        /// </summary>
        /// <returns>true则有</returns>
        public static bool HasFocusConfig()
        {
            try
            {
                if (
                    bool.TryParse(
                        AppConfig.Config["App:Config:hasFocusConfig"],
                        out bool hasFocusConfig
                    ) && hasFocusConfig
                )
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 2025.3.6 李焕彬
        /// 是否有plc控制
        /// </summary>
        /// <returns></returns>
        public static bool HasMotionConfig()
        {
            try
            {
                if (
                    bool.TryParse(
                        AppConfig.Config["App:Config:hasMotionConfig"],
                        out bool hasMotionConfig
                    ) && hasMotionConfig
                )
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 获取主题是否暗色
        /// </summary>
        /// <returns>true则是</returns>
        public static bool IsThemeDark()
        {
            try
            {
                if (bool.TryParse(AppConfig.Config["App:Config:IsDark"], out bool isDark) && isDark)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 设置主题是否暗色
        /// </summary>
        /// <param name="isDark">是否暗色</param>
        public static void SetThemeDark(bool isDark)
        {
            AppConfig.Config["App:Config:IsDark"] = isDark.ToString();
            UpdateJsonFile("IsDark", isDark);
        }

        /// <summary>
        /// 2024.10.22 李焕彬
        /// 修改appConfig.json文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键名</param>
        /// <param name="newValue">新值</param>
        public static void UpdateJsonFile<T>(string key, T newValue)
        {
            try
            {
                string path = "appConfig.json";
                var jsonText = File.ReadAllText(path);
                dynamic jsonObj = JsonConvert.DeserializeObject<dynamic>(jsonText);
                jsonObj["APP"]["Config"][key] = newValue;
                jsonText = JsonConvert.SerializeObject(jsonObj);
                File.WriteAllText(path, jsonText);
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
        }
    }
}
