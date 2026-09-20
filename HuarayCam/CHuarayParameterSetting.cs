using System.ComponentModel;
using CameraModule;

namespace HuarayCam;

public enum HuarayTriggerLine
{
    Line0,
    Line1,
    Line2,
    Line3,
}

public enum HuarayTriggerActivation
{
    RisingEdge,
    FallingEdge,
}

public class CHuarayParameterSetting : CCameraParameterBase
{
    public CHuarayParameterSetting() { }

    public CHuarayParameterSetting(string serialNumber)
        : base(serialNumber, "HuarayCam") { }

    private HuarayTriggerLine triggerLine = HuarayTriggerLine.Line1;

    [Category("华睿相机参数")]
    [DisplayName("01.硬触发输入")]
    [Description("与相机实际接线一致；默认 Line1")]
    public HuarayTriggerLine TriggerLine
    {
        get => triggerLine;
        set
        {
            var previous = triggerLine;
            if (SetProperty(ref triggerLine, value) && Connected)
            {
                try
                {
                    CCameraManagement.CameraDict[SerialNumber].SetCustomParam(0);
                }
                catch (Exception ex)
                {
                    SetProperty(ref triggerLine, previous);
                    CCameraManagement.CamLogger.Error($"设置华睿触发输入失败: {ex.Message}");
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetCustomParam(0);
                    }
                    catch (Exception restoreError)
                    {
                        CCameraManagement.CamLogger.Error($"恢复华睿触发输入失败: {restoreError.Message}");
                    }
                }
            }
        }
    }

    private HuarayTriggerActivation triggerActivation = HuarayTriggerActivation.RisingEdge;

    [Category("华睿相机参数")]
    [DisplayName("02.硬触发极性")]
    [Description("默认上升沿")]
    public HuarayTriggerActivation TriggerActivation
    {
        get => triggerActivation;
        set
        {
            var previous = triggerActivation;
            if (SetProperty(ref triggerActivation, value) && Connected)
            {
                try
                {
                    CCameraManagement.CameraDict[SerialNumber].SetCustomParam(0);
                }
                catch (Exception ex)
                {
                    SetProperty(ref triggerActivation, previous);
                    CCameraManagement.CamLogger.Error($"设置华睿触发极性失败: {ex.Message}");
                    try
                    {
                        CCameraManagement.CameraDict[SerialNumber].SetCustomParam(0);
                    }
                    catch (Exception restoreError)
                    {
                        CCameraManagement.CamLogger.Error($"恢复华睿触发极性失败: {restoreError.Message}");
                    }
                }
            }
        }
    }
}
