using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationModule;
using Modbus;

namespace ZipperInfo
{
    //名称                  PLC地址   数据类型  Modbus地址(10进制)         地址内容说明
    //手动模式                  M6       BOOL	          6	
    //自动模式                  M7       BOOL	          7	
    //复位                      M8       BOOL	          8	
    //暂停                      M9       BOOL	          9	
    //停止                      M10      BOOL	          10	
    //自动运行中                M11      BOOL	          11	         连续自动运行
    //半自动运行                M12      BOOL	          12	         启动后只运行一次
    //换长度测试                M13      BOOL	          13	         更换长度时用于相机拍照测试
    //设备有故障                M41      BOOL	          41	
    //手动触发光源A路+相机1拍照 M523     BOOL	         523	
    //手动触发光源B路+相机1拍照 M524     BOOL	         524	
    //手动触发光源A路+相机2拍照 M526     BOOL	         526	
    //手动触发光源B路+相机2拍照 M527     BOOL	         527	
    //手动连续拍照或单次拍照    M528     BOOL	         528	        OFF单次拍照，ON连续拍照
    //自动触发光源A路+相机1拍照 M623     BOOL	         623	        自动触发A路光源+相机1拍照后在触发相机2拍照
    //自动触发光源B路+相机1拍照 M623     BOOL	         624	        自动触发B路光源+相机1拍照后在触发相机2拍照
    //相机1AB路光源切换         HM12     BOOL	         49420	        OFF 自动时拍照使用A路光源，ON自动时拍照使用B路光源
    //相机2AB路光源切换         HM13     BOOL	         49421	        OFF 自动时拍照使用A路光源，ON自动时拍照使用B路光源
    //产品最后一次拍照          M628     BOOL	         628	        产品拉料完成后最后一次拍照信号，ON_-> OFF时拍照完成
    //相机拍照计数(产品ID)      HD104    DINT	         41192	        拍照完成后加1，累加到2,147,483,647后清零重新计数
    //图片编号（图片ID）	    HD106    DINT	         41194	        图片编号根据拉链的长度计算需要拍多少张,一条拉链的头部第一张图片为1,第二张图片为2,以此类推;特殊情况:是拉头位置时,编号为100
    //检测结果                  HD108    DINT	         41196	        1:OK 2:NG
    //拉头位置脉冲坐标          HD110    DINT	         41198	        夹爪缓慢拉动拉头，视觉识别到拉头位置时写入PLC
    //轴当前的脉冲坐标	        HD112	 DINT	         41200	视觉读取轴当前的脉冲坐标值
    //单条拉链的拍照图片总张数  HD114    DINT	         41202	        单条拉链的拍照图片总张数
    //拉链长度                  HD116    DINT	         41204	        拉链的实际长度，在触摸屏上设置，视觉软件读取显示，单位cm
    //拍照位置1                 HD250    DINT	         41438	        设定第1段拍照位置
    //拍照位置2                 HD252    DINT	         41440	        设定第2段拍照位置
    //拍照位置3                 HD254    DINT	         41442	        设定第3段拍照位置
    //拍照位置4                 HD256    DINT	         41444	        设定第4段拍照位置
    //拍照位置5                 HD258    DINT	         41446	        设定第5段拍照位置
    //拍照位置6                 HD260    DINT	         41448	        设定第6段拍照位置
    //拍照位置7                 HD262    DINT	         41450	        设定第7段拍照位置
    //拍照位置8                 HD264    DINT	         41452	        设定第8段拍照位置
    //拍照位置9                 HD266    DINT	         41454	        设定第9段拍照位置
    //拍照位置10                HD268    DINT	         41456	        设定第10段拍照位置

    public class CZipperCommunicate
    {

        static object lockobj = new object();
        public static CModbusCommPart com;
        /// <summary>
        /// 获取拉链的ID信息 2025-5-29 鲍赞宝
        /// </summary>
        /// <param name="productID">产品ID</param>
        /// <param name="photoID">图片ID</param>
        public static void GetID(out int productID, out int photoID)
        {
            try
            {
                lock (lockobj)
                {
                    productID = -1;
                    photoID = -1;
                    if (com != null)
                    {
                        productID = com.ReadHoldingRegisterInt32(41192);
                        photoID = com.ReadHoldingRegisterInt32(41194);
                    }
                }
            }
            catch (Exception)
            {
                productID = -1;
                photoID = -1;
            }


        }


        /// <summary>
        /// 获取单条拉链拍照的总张数 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetPhotoCount()
        {
            try
            {
                lock (lockobj)
                {
                    if (com != null)
                    {
                        return com.ReadHoldingRegisterInt32(41202);

                    }
                    else
                    {
                        return -1;
                    }
                }

            }
            catch (Exception)
            {
                return -1;
            }


        }
        /// <summary>
        /// 获取单条拉链的长度 2025-5-29 鲍赞宝
        /// </summary>
        public static int GetZipperLenght()
        {
            try
            {
                if (com != null)
                {
                    return com.ReadHoldingRegisterInt32(41204);

                }
                else
                {
                    return -1;
                }

            }
            catch (Exception)
            {
                return -1;
            }

        }
        /// <summary>
        /// 向PLC写入结果
        /// </summary>
        /// <param name="result">OK:1 NG:2</param>
        public static void SendResult(ZIPPERESULT result)
        {
            try
            {
                if (com != null)
                {
                    com.WriteSingleRegisterInt32(41196, (int)result);
                }
            }
            catch (Exception)
            {
            }
         

        }

    }

    public enum ZIPPERESULT
    {
        OK = 1,
        NG = 2

    }

}
