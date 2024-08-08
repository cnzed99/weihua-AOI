using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CameraModule
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机接口
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 创建相机新实例
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="cam">输出相机对象</param>
        /// <returns>相机参数</returns>
        CCameraParameterBase CreatNewCam(string serialNumber, out CCameraBase cam);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 从配置文件实例化相机
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="index">索引</param>
        /// <param name="cam">输出相机对象</param>
        /// <returns>相机参数</returns>
        CCameraParameterBase Init(string path, int index, out CCameraBase cam);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 枚举相机
        /// </summary>
        /// <returns>枚举列表</returns>
        List<WHCameraInfo> EnumCamrea();
    }

    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机插件加载
    /// </summary>
    public class LoadCamPlugs
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 加载相机插件
        /// </summary>
        public static void LoadCam()
        {
            foreach (var item in Directory.GetDirectories("CamPlug"))
            {
                if (File.Exists($"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"))
                {
                    Assembly ass = Assembly.LoadFrom(
                        $"{item}\\{Path.GetFileNameWithoutExtension(item)}.dll"
                    );
                    Type type = ass.GetTypes()
                        .ToList()
                        .Find(c => c.GetInterface("ICamera") != null);
                    if (
                        type != null
                        && !CCameraManagement.CameraHelpers.ContainsKey(
                            Path.GetFileNameWithoutExtension(item)
                        )
                    )
                    {
                        ICamera cam = Activator.CreateInstance(type) as ICamera;
                        CCameraManagement.CameraHelpers.Add(
                            Path.GetFileNameWithoutExtension(item),
                            cam
                        );
                    }
                }
            }
        }
    }
}
