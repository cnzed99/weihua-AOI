<div><center><b>
    <font color="34,63,93" size="7"> 
        自动对焦插件编写规范
    </font>
</b></center></div>

# 1、概述

本文制定对焦插件编写规范，目的是使插件开发规范化。包含插件接口（IFocus）、对焦参数基类（CFocusConfigBase、CFocusCtrlVMBase）等。

# 2、依赖库和工程配置

开始编写前，需先引用项目

1. FocusControl.dll、
2. WH.Entity.dll、
3. WH.Controls.dll、
4. LanguageManager.dll、
5. CameraModule.dll。(相机插件需要实现清晰度计算函数GetDistinctFunc)

* 如果插件项目是在毛刺仓库内部编写，将dll拷贝至插件文件夹添加到生成后事件。在工程.csproj增加如下内容：
  ```csharp
  <TargetName="PostBuild"AfterTargets="PostBuildEvent">

   <ExecCommand="xcopy $(OutDir)$(TargetFileName) $(SolutionDir)断面毛刺检测软件\bin\Debug\$(TargetFramework)\FocusPlug\$(TargetName)\ /d /y" />

  ...其他依赖项

  </Target>
  ```
* 如插件项目是在外部编写，则需把生成的插件dll以及依赖项拷贝到\\FocusPlug\\插件名\\文件夹内。

# 3、插件接口实现

* 实现插件接口（IFocus）
* 实现CreateNewfocus()方法，返回对焦管理基类（CFocusConfigBase）的派生类

```csharp
namespace MotionControl
{
    public class PlugIn : IFocus
    {
        /// <summary>
        /// 创建新对焦
        /// </summary>
        public CFocusConfigBase CreateNewfocus()
        {
            CMotionConfig motionConfig = new CMotionConfig();
            return motionConfig;
        }
    }
}
```

# 4、对焦控件VM基类（CFocusCtrlVMBase）实现

* 创建CFocusCtrlVMBase（基类）的派生类，该类功能为控制对焦，包含如下要求：
* 包含一个构造函数，需在构造时实例化TestControl对象。TestControl为对焦控制操作界面，为UserControl，每个对焦插件都不一样。
* 可重写SetRunning函数，在制程启动时通知对焦控件。
* 重写InitControl函数，初始化控制函数，包含连接、写入初始参数等操作。
* 可重写Reset函数，函数功能为复位（清除报警信息）功能。

```csharp
namespace MotionControl
{
    /// <summary>
    /// 运动控件VM
    /// </summary>
    public partial class CMotionCtrlVM : CFocusCtrlVMBase
    {
        public CMotionCtrlVM()
            : base()
        {
            TestControl = new MotionCtrl(this);
            UpdateInfoAlarm();
        }

        /// <summary>
        /// 设置当前制程是否启动
        /// </summary>
        /// <param name="isRuning">是否启动</param>
        public override void SetRunning(bool isRuning)
        {
            base.SetRunning(isRuning);
            modbusTcp?.WriteSingleCoil(MotionConfig.AddrStartFocus, isRuning);
        }

        /// <summary>
        /// 运动控制配置
        /// </summary>
        [ObservableProperty]
        private CMotionConfig motionConfig;

	//其他代码
    }
}
```

对焦控件父类：

```csharp
/// <summary>
/// 对焦控件VM
/// </summary>
public partial class CFocusCtrlVMBase : ObservableObject
{
    /// <summary>
    /// 显示控件
    /// </summary>
    [ObservableProperty]
    UserControl testControl;

    /// <summary>
    /// 权限信息，启动暂停、账户登录时切换
    /// </summary>
    [ObservableProperty]
    CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

    /// <summary>
    /// 设置相机序列号
    /// </summary>
    /// <param name="cameraSerial">序列号</param>
    public void SetCameraSerial(string cameraSerial)
    {
        this.CameraSerial = cameraSerial;
    }

    /// <summary>
    /// 设置当前制程是否启动
    /// </summary>
    /// <param name="isRuning">是否启动</param>
    public virtual void SetRunning(bool isRuning)
    {
        this.IsRuning = isRuning;
    }

    /// <summary>
    /// 相机序列号 图像来源相机
    /// </summary>
    protected string CameraSerial { get; set; } = "";

    /// <summary>
    /// 清晰度算法 主程序会从算法插件中获取 
    /// this.FocusCtrlVM.FuncDistinct = MaociAlgorParamConfig.GetDistinctFunc();
    /// </summary>
    public Func<CImage, float> FuncDistinct { get; set; }

    /// <summary>
    /// 当前制程是否启动
    /// </summary>
    [ObservableProperty]
    private bool isRuning = false;

    /// <summary>
    /// 运动控制配置
    /// </summary>
    [ObservableProperty]
    private CFocusConfigBase config;

    /// <summary>
    /// 报警信息
    /// </summary>
    [ObservableProperty]
    private string infoAlarm;

    /// <summary>
    /// 正在对焦状态
    /// </summary>
    [ObservableProperty]
    private bool isFocusing = false;

    /// <summary>
    /// 是否已经对焦状态
    /// </summary>
    [ObservableProperty]
    private bool isFocused = false;

    /// <summary>
    /// 通道数
    /// </summary>
    private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(10)
    {
        FullMode = BoundedChannelFullMode.Wait
    };

    /// <summary>
    /// 对焦采集 图像队列 用于计算清晰度
    /// </summary>
    public Channel<Cell> FocusWaitGetImageChannel = Channel.CreateBounded<Cell>(channelOptions);

    /// <summary>
    /// 运行日志
    /// </summary>
    public static CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

    /// <summary>
    /// 初始化控制
    /// </summary>
    public virtual void InitControl() { }

    /// <summary>
    /// 复位
    /// </summary>
    public virtual void Reset() { }
}
```

5、对焦管理 CFocusConfigBase 实现
对焦管理父类：

```csharp
namespace FocusControl;

public partial class CFocusConfigBase : ConfigModifyObservableBase, IRecipient<OperateMessage>
{
    public CFocusConfigBase()
    {
        this.token = new Token("", "FocusControl");
    }

    /// <summary>
    /// 操作日志
    /// </summary>
    [property: JsonIgnore]
    [property: IgnoreModifyLog]
    public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

    /// <summary>
    /// 所属制程名
    /// </summary>
    [property: IgnoreModifyLog]
    public string PrcessName { get; set; }

    ///// <summary>
    ///// 类型，插件dll名
    ///// </summary>
    //public string FocusType { get; set; }

    /// <summary>
    /// 日志消息处理
    /// </summary>
    /// <param name="message">消息</param>
    public virtual void Receive(OperateMessage message) { }

    /// <summary>
    /// 创建VM
    /// </summary>
    /// <returns>VM</returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual CFocusCtrlVMBase CreateCtrlVM()
    {
        throw new NotImplementedException();
    }
}
```

参考实现如下：

```csharp
namespace MotionControl;

/// <summary>
/// 运动配置
/// </summary>
public partial class CMotionConfig : CFocusConfigBase
{
    [Browsable(false)]
    [JsonIgnore]
    [IgnoreModifyLog]
    string[] XIOName = new string[10]
    {
        "负限位",
        "正限位",
        "原点",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用"
    };

    [Browsable(false)]
    [JsonIgnore]
    [IgnoreModifyLog]
    string[] YIOName = new string[10]
    {
        "脉冲",
        "方向",
        "使能",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用",
        "备用"
    };

    public CMotionConfig()
        : base()
    {
  
        var signalIn = new ObservableCollection<CSignalIn>();
        for (int i = 0; i < 10; i++)
        {
            signalIn.Add(new CSignalIn(token, $"X{i}", i));
        }
        this.SignalIns = signalIn;
        var signalOut = new ObservableCollection<CSignalOut>();
        for (int i = 0; i < 10; i++)
        {
            signalOut.Add(new CSignalOut(token, $"Y{i}", i));
        }
        this.SignalOuts = signalOut;
        this.RegisterSets = new ObservableCollection<CElement>() { };
    }

    /// <summary>
    /// 日志消息处理
    /// </summary>
    /// <param name="message">消息</param>
    public override void Receive(OperateMessage message)
    {
        base.Receive(message);
        if (message.obj.GetType() == typeof(CMotionConfig))
        {
            OperateLog.Info($"{PrcessName}-运动控制-{message.message}");
            return;
        }
        foreach (var signal in SignalIns)
        {
            if (message.obj.GetType() == typeof(CSignalIn))
            {
                if (signal == message.obj)
                {
                    OperateLog.Info($"{PrcessName}-运动控制-{signal.Name}:{message.message}");
                    return;
                }
                continue;
            }
        }
        foreach (var signal in SignalOuts)
        {
            if (message.obj.GetType() == typeof(CSignalOut))
            {
                if (signal == message.obj)
                {
                    OperateLog.Info($"{PrcessName}-运动控制-{signal.Name}:{message.message}");
                    return;
                }
                continue;
            }
        }
        foreach (var reg in RegisterSets)
        {
            if (message.obj.GetType() == typeof(CElement))
            {
                if (reg == message.obj)
                {
                    OperateLog.Info($"{PrcessName}-运动控制-{reg.Name}:{message.message}");
                    return;
                }
                continue;
            }
        }
    }

    /// <summary>
    /// 创建VM
    /// </summary>
    /// <returns>VM</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override CFocusCtrlVMBase CreateCtrlVM()
    {
        var vm = new CMotionCtrlVM();
        vm.MotionConfig = this;
        return vm;
    }

    /// <summary>
    /// 2024.7.9 李焕彬
    /// IP地址
    /// </summary>
    [ObservableProperty]
    [property: Category("1.连接信息")]
    [property: DisplayName("11.IP")]
    [property: Description("11.IP")]
    private string iP = "192.168.1.88";

    /// <summary>
    /// 2024.7.9 李焕彬
    /// 端口号
    /// </summary>
    [ObservableProperty]
    [property: Category("1.连接信息")]
    [property: DisplayName("12.Port")]
    [property: Description("12.Port")]
    private int port = 502;
    }
}
```
