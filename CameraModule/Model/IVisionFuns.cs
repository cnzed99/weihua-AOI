using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraModule.Model
{
    public interface IVisionFuns
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化相机
        /// </summary>
        /// <returns>2024.7.23 李焕彬</returns>
        public bool OpenCamera();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 关闭相机，关闭之前执行
        /// </summary>
        public void CloseCamera();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 开始采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public bool StartGrab();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 停止采集
        /// </summary>
        /// <returns>true成功，false失败</returns>
        public bool StopGrab();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像宽度
        /// </summary>
        /// <param name="value">图像宽度</param>
        /// <returns>true成功，false失败</returns>
        public bool GetImageWidth(out int value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像高度
        /// </summary>
        /// <param name="value">图像高度</param>
        /// <returns>true成功，false失败</returns>
        public bool GetImageHeight(out int value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取图像类型
        /// </summary>
        /// <param name="cameraType">图像类型</param>
        /// <returns>true成功，false失败</returns>
        public bool GetCameraType(out EMCAMERATYPE cameraType);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机触发模式
        /// </summary>
        /// <param name="useTrigger">是否使用触发</param>
        /// <returns>true成功，false失败</returns>
        public bool GetTriggerMode(out EMTRIGGERMODE mode);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取当前曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        /// <returns>true成功，false失败</returns>
        public bool GetExposureTime(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置曝光值
        /// </summary>
        /// <param name="value">曝光值</param>
        public void SetExposureTime(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取增益值
        /// </summary>
        /// <param name="value">增益</param>
        /// <returns>true成功，false失败</returns>
        public bool GetGain(out float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置增益
        /// </summary>
        /// <param name="value">增益</param>
        public void SetGain(float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        /// <returns>true成功，false失败</returns>
        public bool GetGamma(out float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置Gamma值
        /// </summary>
        /// <param name="value">Gamma值</param>
        public void SetGamma(float value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 设置相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        public void SetTriggerDelay(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机触发延时时间
        /// </summary>
        /// <param name="value">触发延时时间</param>
        /// <returns>true成功，false失败</returns>
        public bool GetTriggerDelay(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        ///设置输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        public void SetTriggerPulseWidth(uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取输出脉冲宽度
        /// </summary>
        /// <param name="value">输出脉冲宽度</param>
        /// <returns>true成功，false失败</returns>
        public bool GetTriggerPulseWidth(out uint value);

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 保存用户参数
        /// </summary>
        public void UserSaveParam();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 加载用户参数
        /// </summary>
        public void UserLoadParam();

        /// <summary>
        /// 启用IO输出
        /// </summary>
        /// <param name="Enble"></param>
        public void SetStrobeEnable(bool Enable);

        /// <summary>
        ///选择输出线路
        /// </summary>
        /// <param name="Line">输出源</param>
        public void SetLineSelector(object Line);

        /// <summary>
        ///设置输出持续时间
        /// </summary>
        /// <param name="Value"></param>
        public void SetStrobeDuration(uint Value);

        /// <summary>
        ///选择输出触发类型
        /// </summary>
        /// <param name=""></param>
        public void SetLineSource(object source);

        /// <summary>
        ///信号反转
        /// </summary>
        public void SetLineInverter(bool Enable);

        /// <summary>
        ///设置输入输出模式
        /// </summary>
        public void SetLineMode(object LineMode);

        /// <summary>
        /// 输出触发
        /// </summary>
        public void LineTriggerSoftware();

        /// <summary>
        /// 设置Gamma使能
        /// </summary>
        /// <param name="Enable"></param>
        public void SetGammaEnable(bool Enable);

        /// <summary>
        /// 获取实时帧率
        /// </summary>
        /// <returns></returns>
        public float GetFps();

        /// <summary>
        /// 设置一次触发相机拍照次数
        /// </summary>
        public void SetFrameCount(int count);
    }
}
