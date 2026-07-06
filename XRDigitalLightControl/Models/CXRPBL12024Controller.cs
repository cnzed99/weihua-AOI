using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WH.LightControl;

namespace XRDigitalLightControl
{
    public class CXRPBL12024Controller
    {
        #region 写入
        /// <summary>
        /// 计算校验和：从标识符到最后一个命令参数的所有字节累加，取低8位
        /// </summary>
        internal byte CalculateChecksum(byte[] data)
        {
            int sum = data.Sum(b => (int)b);
            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// 构建设置亮度的控制包
        /// </summary>
        /// <param name="channel">通道号：0=L1,1=L2,2=L3,3=L4, 255=所有通道</param>
        /// <param name="brightness">亮度值 0~255</param>
        /// <returns>完整的待发送字节数组</returns>
        internal byte[] BuildSetBrightnessPacket(byte channel, byte brightness)
        {
            const byte deviceId = 0;            //设备ID(0~254, 255为广播) </ param >
            const byte header = 0x40;          // 标识符
            const byte deviceCode = 0x01;      // PDC系列
            const byte commandCode = 0x1A;     // 设置亮度命令

            // 长度 = 设备CODE + 设备ID + 命令码 + 通道号 + 亮度值 = 5字节
            byte length = 0x05;

            // 构造命令部分（不含标识符和长度）
            byte[] payload = new byte[]
            {
            deviceCode,
            deviceId,
            commandCode,
            channel,
            brightness
            };

            // 完整包：标识符 + 长度 + payload
            byte[] packet = new byte[2 + payload.Length];
            packet[0] = header;
            packet[1] = length;
            Array.Copy(payload, 0, packet, 2, payload.Length);

            // 计算校验和并追加
            byte checksum = CalculateChecksum(packet);
            byte[] finalPacket = new byte[packet.Length + 1];
            Array.Copy(packet, finalPacket, packet.Length);
            finalPacket[finalPacket.Length - 1] = checksum;

            return finalPacket;
        }

        #endregion

        #region 读取

        /// <summary>
        /// 构建读取请求包
        /// </summary>
        internal byte[] BuildReadRequestPacket(byte deviceId, byte channel)
        {
            const byte header = 0x40;
            const byte length = 0x04;          // 设备CODE + 设备ID + 命令码 + 通道号
            const byte deviceCode = 0x01;
            const byte commandCode = 0x31;

            byte[] packet = new byte[]
            {
        header, length, deviceCode, deviceId, commandCode, channel
            };

            byte checksum = CalculateChecksum(packet);
            return packet.Concat(new[] { checksum }).ToArray();
        }

        #endregion
    }
}
