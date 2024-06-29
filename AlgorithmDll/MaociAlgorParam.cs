using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public class MaociAlgorParam : ObservableObject
    {
        public MaociAlgorParam(string name) 
        {
            Name = name;
        }

        private string name = "";

        [Category("1、名称")]
        [DisplayName("名称")]
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        [Category("算法参数")]
        [DisplayName("自适应阈值邻域大小")]
        public uint AdaptiveSize { get; set; } = 14;//自适应阈值邻域大小

        [Category("算法参数")]
        [DisplayName("自适应阈值增加值")]
        public int AdaptiveAddGray { get; set; } = 20;//自适应阈值增加值

        [Category("算法参数")]
        [DisplayName("过滤矩阵邻域大小")]
        public uint NeighbSize { get; set; } = 5;//过滤矩阵邻域大小

        [Category("算法参数")]
        [DisplayName("过滤矩阵邻域点数量限制")]
        public uint NeighbLightPoint { get; set; } = 30;//过滤矩阵邻域点数量限制

        [Category("算法参数")]
        [DisplayName("料区阈值")]
        public uint DarkThresh { get; set; } = 30;//料区阈值

        [Category("算法参数")]
        [DisplayName("铝层阈值")]
        public uint LightThresh { get; set; } = 80;//铝层阈值

        [Category("算法参数")]
        [DisplayName("铝层厚度")]
        public uint LightThick { get; set; } = 6;//铝层厚度
    };

    /// <summary>
    /// FPGA算法参数
    /// </summary>
    public class MaociAlgorParamFpga
    {
        public MaociAlgorParamFpga(string name)
        {
            Name = name;
        }

        [Category("1、名称")]
        [DisplayName("名称")]
        public string Name { get; set; } = "";

        [Category("算法参数")]
        [DisplayName("自适应阈值邻域大小")]
        public uint AdaptiveSize { get; set; } = 14;//自适应阈值邻域大小

        [Category("算法参数")]
        [DisplayName("自适应阈值增加值")]
        public int AdaptiveAddGray { get; set; } = 20;//自适应阈值增加值

        [Category("算法参数")]
        [DisplayName("过滤矩阵邻域大小")]
        public uint NeighbSize { get; set; } = 5;//过滤矩阵邻域大小

        [Category("算法参数")]
        [DisplayName("过滤矩阵邻域点数量限制")]
        public uint NeighbLightPoint { get; set; } = 30;//过滤矩阵邻域点数量限制

        [Category("算法参数")]
        [DisplayName("料区阈值")]
        public uint DarkThresh { get; set; } = 30;//料区阈值

        [Category("算法参数")]
        [DisplayName("铝层阈值")]
        public uint LightThresh { get; set; } = 80;//铝层阈值

        [Category("判定参数")]
        [DisplayName("料区厚度限制")]
        public uint DarkThickLimit { get; set; } = 30;//料区厚度限制，掉料检测

        [Category("判定参数")]
        [DisplayName("料区厚度NG连续长度限制")]
        public uint DarkThickContinueLen { get; set; } = 5;//料区厚度NG连续长度限制

        [Category("判定参数")]
        [DisplayName("料区厚度")]
        public uint DarkThick { get; set; } = 84;//料区厚度

        [Category("判定参数")]
        [DisplayName("铝层厚度限制")]
        public uint LightThickLimit { get; set; } = 7;//铝层厚度限制，毛刺检测

        [Category("判定参数")]
        [DisplayName("铝层厚度NG连续长度限制")]
        public uint LightThickContinueLen { get; set; } = 0;//铝层厚度NG连续长度限制

        [Category("判定参数")]
        [DisplayName("铝层厚度")]
        public uint LightThick { get; set; } = 6;//铝层厚度

        [Category("判定参数")]
        [DisplayName("铝层在料区中心位置限制上")]
        public uint PosLimitT { get; set; } = 20;//铝层在料区中心位置限制上

        [Category("判定参数")]
        [DisplayName("铝层在料区中心位置限制下")]
        public uint PosLimitB { get; set; } = 20;//铝层在料区中心位置限制下

        [Category("判定参数")]
        [DisplayName("铝层位置偏移值")]
        public int LightPosOffest { get; set; } = 0;//铝层位置偏移值
    }
}
