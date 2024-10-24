<div><center><b>
    <font color="34,63,93" size="7"> 
        算法插件编写规范
    </font>
</b></center></div>

# 1、概述

本文制定算法插件编写规范，目的是使插件开发规范化。包含插件接口（IAlgorithm）、算法参数基类（CAlgorithmParamBase、CPcParamBase）等。


# 2、依赖库和工程配置

开始编写前，需先引用项目

1. AlgorithmDll.dll、
2. WH.Entity.dll、
3. WH.Controls.dll、
4. WH.RecipeCellRootBase.dll、
5. WH.RunCell.dll。

* 如果插件项目是在毛刺仓库内部编写，将dll拷贝至插件文件夹添加到生成后事件。在工程.csproj增加如下内容：
  ```csharp
  <TargetName="PostBuild"AfterTargets="PostBuildEvent">

   <ExecCommand="xcopy $(OutDir)$(TargetFileName) $(SolutionDir)断面毛刺检测软件\bin\Debug\$(TargetFramework)\AlgorithmPlug\$(TargetName)\ /d /y" />

  ...其他依赖项

  </Target>
  ```
* 如插件项目是在外部编写，则需把生成的插件dll以及依赖项拷贝到\\AlgorithmPlug\\插件名\\文件夹内。

# 3、插件接口实现

* 实现插件接口（IAlgorithm）
* 实现CreateNewAlgorithm()方法，返回算法参数基类（CAlgorithmParamBase）的派生类

  ```csharp
  namespace SideAlgorithm
  {
      public class PlugIn : IAlgorithm
      {
          /// <summary>
          /// 2024.09.04 李焕彬
          /// 创建新算法参数
          /// </summary>
          /// <returns>算法参数</returns>
          public CAlgorithmParamBase CreateNewAlgorithm()
          {
              CAlgorithmParam cAlgorithmParam = new CAlgorithmParam();
              return cAlgorithmParam;
          }
      }
  }
  ```

# 4、PC算法参数基类实现

创建CPcParamBase（基类），包含如下要求：

* 包含一个无参构造函数和一个带参构造函数（string,Token）,需使用base（）调用基类的构造函数
* 该类包含算法需要设置的参数，支持int、double、string、enum等常用类型。

* 使用Mvvm模式编写，每个参数需包含ObservableProperty、Category、DisplayName、Description特性。

```csharp
namespace SideAlgorithm;

public partial class CPcParam : CPcParamBase
{
    public CPcParam()
        : base() { }

    public CPcParam(string name, Token token)
        : base(name, token) { }

    /// <summary>
    /// 过滤矩阵邻域大小
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("过滤矩阵邻域大小")]
    [property: Description("过滤矩阵说明")]
    private uint neighbSize = 7;

    /// <summary>
    /// 极片阈值
    /// </summary>
    [ObservableProperty]
    [property: Category("2.Algorithm")]
    [property: DisplayName("极片阈值")]
    [property: Description("极片阈值说明")]
    private uint darkThresh = 200;
}
```

# 5、区域信息接口（IRegionInfo）实现

创建一个实现IRegionInfo接口的具体类/结构体，该接口功能为获取区域特征信息及合并区域。

* 获取区域特征信息函数：double GetValue(CFeacture feacture, SRegion region)

feacture为特征，region为传入区域，返回值为区域的对应特征值。

* 合并区域函数：SRegion Union(List`<SRegion>` regions)

regions为需要合并的区域，返回值为合并后的区域。

```csharp
 /// <summary>
 /// 区域信息
 /// </summary>
 public struct SRegionInfo : IRegionInfo
 {
     /// <summary>
     /// um垂直宽度
     /// </summary>
     public double WidthBound = 0;

     /// <summary>
     /// um垂直高度
     /// </summary>
     public double HeightBound = 0;

     /// <summary>
     /// um直角高度
     /// </summary>
     public double PeakHeight = 0;

     /// <summary>
     /// um低点高度
     /// </summary>
     public double BotHeight = 0;

     /// <summary>
     /// um长边长度
     /// </summary>
     public double LongLen = 0;

     /// <summary>
     /// um短边长度
     /// </summary>
     public double ShorLen = 0;

     /// <summary>
     /// 角度
     /// </summary>
     public double Phi = 0;

     /// <summary>
     /// um周长
     /// </summary>
     public double ContLen = 0;

     /// <summary>
     /// um²面积
     /// </summary>
     public double Area = 0;

     public SRegionInfo() { }

     public double GetValue(CFeacture feacture, SRegion region)
     {
         switch (feacture.Id)
         {
             case "PeakHeight":
                 return PeakHeight;
             case "BotHeight":
                 return BotHeight;
             case "Area":
                 return Area;
             case "LongLength":
                 return LongLen;
             case "ShortLength":
                 return ShorLen;
             case "Angle":
                 return Phi;
             case "ContLength":
                 return ContLen;
             case "Width":
                 return WidthBound;
             case "Height":
                 return HeightBound;
             default:
                 return 0;
         }
     }
     /// <summary>
     /// 检测设置中的区域合并操作，根据实际情况编写，如区域融合或检测数值叠加
     /// </summary>
     public SRegion Union(List<SRegion> regions)
     {
         SRegionInfo regionInfo = new SRegionInfo();
         regionInfo.WidthBound = regions
             .Select(o => ((SRegionInfo)o.regionInfo).WidthBound)
             .Sum();
         regionInfo.HeightBound = regions
             .Select(o => ((SRegionInfo)o.regionInfo).HeightBound)
             .Sum();
         regionInfo.PeakHeight = regions
             .Select(o => ((SRegionInfo)o.regionInfo).PeakHeight)
             .Max();
         regionInfo.BotHeight = regions.Select(o => ((SRegionInfo)o.regionInfo).BotHeight).Min();
         regionInfo.LongLen = regions.Select(o => ((SRegionInfo)o.regionInfo).LongLen).Sum();
         regionInfo.ShorLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ShorLen).Sum();
         regionInfo.Phi = regions.Select(o => ((SRegionInfo)o.regionInfo).Phi).Max();
         regionInfo.ContLen = regions.Select(o => ((SRegionInfo)o.regionInfo).ContLen).Sum();
         regionInfo.Area = regions.Select(o => ((SRegionInfo)o.regionInfo).Area).Sum();
         List<Point> pts = new List<Point>();
         for (int i = 0; i < regions.Count; i++)
         {
             pts.AddRange(regions[i].points);
         }
         return new SRegion(regionInfo, pts);
     }
 };
```

检测输出区域->SRegion:

```csharp
/// <summary>
/// 区域
/// </summary>
public struct SRegion
{
    public Rect rect;

    /// <summary>
    /// 区域信息
    /// </summary>
    public IRegionInfo regionInfo;

    /// <summary>
    /// 区域点集
    /// </summary>
    public List<Point> points;

    public SRegion(IRegionInfo _regionInfo, List<Point> _points)
    {
        regionInfo = _regionInfo;
        points = _points;
        rect = new Rect(
            new Point(points.Select(o => o.X).Min(), points.Select(o => o.Y).Min()),
            new Point(points.Select(o => o.X).Max(), points.Select(o => o.Y).Max())
        );
    }

    /// <summary>
    /// 获取区域矩形
    /// </summary>
    /// <returns>区域矩形</returns>
    public Rect GetRect()
    {
        return rect;
    }

    /// <summary>
    /// 获取区域中心
    /// </summary>
    /// <returns>区域中心</returns>
    public Point GetCenter()
    {
        return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    }
}
```

# 6、算法基类 CAlgorithmParamBase 实现

```csharp
namespace FrontAlgorithm
{
    /// <summary>
    /// 算法参数派生类
    /// </summary>
    public class CAlgorithmParam : CAlgorithmParamBase
    {
        public CAlgorithmParam()
            : base()
        {
            //检测类、缺陷、缺陷类型预定义
            DefectSpecies = new()
            {
                new("铝层缺陷类", new() { new("毛刺", Category.区域) }),
                new("料区缺陷类", new() { new("掉料", Category.区域) }),
                new(
                    "异常类",
                    new()
                    {
                        new("铝层异常", Category.值),
                        new("料区异常", Category.值),
                        new("超时", Category.值),
                        new("失焦", Category.值)
                    }
                ),
            };
	    //缺陷特征集合
            DefectFeatures = new();
            DefectFeatures.Add(new("PeakHeight", "顶点高度", "PeakHeight", "um"));
            DefectFeatures.Add(new("BotHeight", "低点高度", "BotHeight", "um"));
            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("ContLength", "周长", "ContLength", "um"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
        }

        ///// <summary>
        ///// 正在使用的PC参数结构体
        ///// </summary>
        //public SMaociAlgorParam MaociAlgorParamUse { get; set; }

        /// <summary>
        /// 增加参数 框架中可以添加不同的参数组 以名称切换
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddPcParam(string name)
        {
            this.PcParams.Add(new CPcParam(name, token));
        }
	/// <summary>
	/// 更新算法参数结构体 暂时不用
	/// </summary>
	public override void UpdataMaociAlgorParamUse()
	{
  
	}

	/// <summary>
	/// 获取清晰度计算函数 如果用到自动对焦模块必须实现该方法！！！
	/// 如果没有用到自动对焦功能或不控制对焦 可以不用重写
	/// </summary>
	/// <returns>清晰度计算函数</returns>
	/// <exception cref="NotImplementedException"></exception>
	public override Func<CImage, float> GetDistinctFunc()
	{
	    return (CImage image) =>
	    {
	        CPcParam pcParam = (CPcParam)PcParams.FirstOrDefault(o => o.Name == PcSelect);
	        if (pcParam != null)
	        {
	            return CAlgorithmDll.CalcDistinct(
	                image.ImageWidth,
	                image.ImageHeight,
	                image.StrideWidth,
	                image.ImageData,
	                0,
	                (int)pcParam.DarkThresh
 	           );
	        }
	        else
	        {
	            return 0;
	        }
	    };
	}

 	/// <summary>
 	/// 测试PC算法
 	/// </summary>
 	/// <param name="cell">cell</param>
 	/// <returns>检测结果</returns>
 	public override void DetectImage(Cell cell)
 	{
	   //获取当前选择的参数组
  	   var param = (CPcParam)PcParams.FirstOrDefault(o => o.Name == PcSelect)
	   //调用算法传入参数 检测图像
	     EMDETECTRESULT result = CAlgorithmDll.Test(
 	        cell.Image,
 	        ref param,
  	        ref detectInfo,
    	     	cell.MmPerPixel * 1000, //像素与实际显示单位的转换比例，上述时μm，这里*1000
    	     	out var edgeDarkTop,
     	     	out var edgeDarkBottom,
      	     	out var edgeLightTop,
     	     	out var edgeLightBot,
      	     	out var maociRegion,
      	     	out var thickRegion
	     );
	    //解析为各个缺陷 参考
	     CellDetection cellDetection1 = new CellDetection();
	     cellDetection1.Type = "铝层缺陷类";
	     cellDetection1.RecipeDefectName = "毛刺";
	     cellDetection1.Category = Category.区域;
	     cellDetection1.regionOut = maociRegion;
	     cell.AlgorithmOut.Add(cellDetection1);
	     CellDetection cellDetection2 = new CellDetection();
	     cellDetection2.Type = "料区缺陷类";
	     cellDetection2.RecipeDefectName = "掉料";
	     cellDetection2.Category = Category.区域;
	     cellDetection2.regionOut = thickRegion;
	     cell.AlgorithmOut.Add(cellDetection2);

	     CellDetection cellDetection3 = new CellDetection();
	     cellDetection3.Type = "异常类";
	     cellDetection3.RecipeDefectName = "铝层异常";
	     cellDetection3.Category = Category.值;
	     cellDetection3.Value =
	         result == EMDETECTRESULT.EMDR_NG_LIGHTEDGE ? new() { 1.0f } : null;
	     cell.AlgorithmOut.Add(cellDetection3);

	     CellDetection cellDetection4 = new CellDetection();
	     cellDetection4.Type = "异常类";
	     cellDetection4.RecipeDefectName = "料区异常";
	     cellDetection4.Category = Category.值;
	     cellDetection4.Value =
         result == EMDETECTRESULT.EMDR_NG_DARKEDGE ? new() { 1.0f } : null;
	     cell.AlgorithmOut.Add(cellDetection4);

 	     CellDetection cellDetection5 = new CellDetection();
	     cellDetection5.Type = "异常类";
	     cellDetection5.RecipeDefectName = "超时";
	     cellDetection5.Category = Category.值;
	     cellDetection5.Value = result == EMDETECTRESULT.EMDR_TIMEOUT ? new() { 1.0f } : null;
	     cell.AlgorithmOut.Add(cellDetection5);

	     CellDetection cellDetection6 = new CellDetection();
	     cellDetection6.Type = "异常类";
	     cellDetection6.RecipeDefectName = "失焦";
	     cellDetection6.Category = Category.值;
	     cellDetection6.Value = result == EMDETECTRESULT.EMDR_LOSEFOCUS ? new() { 1.0f } : null;
	     cell.AlgorithmOut.Add(cellDetection6);

	     cell.DrawEdges.Add(new CEdgeDraw(edgeDarkTop, Brushes.Blue));
	     cell.DrawEdges.Add(new CEdgeDraw(edgeDarkBottom, Brushes.Blue));
	     cell.DrawEdges.Add(new CEdgeDraw(edgeLightTop, Brushes.Green));
	     cell.DrawEdges.Add(new CEdgeDraw(edgeLightBot, Brushes.Green));
 }
}
```
