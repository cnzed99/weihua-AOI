using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WH.LightControl;

namespace XRLightControl
{
    public class CXRPBL15048Controller
    {
        #region 常量定义（基于协议文档）

        /// <summary>
        /// 命令起始标识符 (Identifier)
        /// 文档明确指定为 0x40
        /// </summary>
        private const byte START_IDENTIFIER = 0x40;

        /// <summary>
        /// PBL系列设备的设备型号代码 (CODE)
        /// 文档明确指定PBL系列为 0x02
        /// </summary>
        private const byte DEVICE_CODE_PBL = 0x02;

        /// <summary>
        /// 广播地址。当设备ID为0xFF时，命令对所有设备有效，但设备不回复。
        /// 文档说明："如果控制包中的设备ID为0xFF，表示命令对所有设备ID的设备都起作用，否则仅对相同设备ID的设备起作用。"
        /// </summary>
        public const byte BROADCAST_ID = 0xFF;

        /// <summary>
        /// 广播地址。当通道号为0xFF时，命令对该设备所有通道生效。
        /// 文档说明："如果命令中的通道号为0xFF,表示该条命令对该设备的所有通道起作用,否则仅对指定通道起作用。"
        /// </summary>
        public const byte BROADCAST_CHANNEL = 0xFF;

        /// <summary>
        /// 命令码 (CMD) 枚举
        /// 对应说明书中第4节“命令集”表格
        /// </summary>
        private enum CommandCode : byte
        {
            SetID = 0x09,       // 设置设备ID
            SetTime = 0x1A,     // 设置通道点亮时间
            SetPercent = 0x22,  // 设置通道脉冲宽度百分比
            ReadParams = 0x31,  // 读取通道参数
            ResponseOneChannel = 0x5A, // 设备回复：1个指定通道的参数
            ResponseAllChannels = 0x60, // 设备回复：全部通道的参数
            ResponseSuccess = 0x00,     // 设备回复：控制包正确
            ResponseError = 0x01        // 设备回复：控制包有误
        }

        /// <summary>
        /// 设备回复状态枚举
        /// 对应命令集中“设备回复正确/错误”的说明
        /// </summary>
        public enum ResponseStatus
        {
            Success,
            Error,
            Timeout,
            ChecksumMismatch
        }

        private byte _currentDeviceId = 0; // 默认ID为0，可通过SetDeviceId修改

        #endregion

        #region 核心协议方法

        /// <summary>
        /// 计算校验和 (CHKSUM)
        /// 文档说明："校验和为包括标识符、长度、设备CODE、设备ID及所有命令的字节累加和。"
        /// 注意：文档示例中，校验和似乎是简单累加后的低字节（未明确说明溢出处理，示例采用简单累加）。
        /// 我们采用累加和取低8位的通用做法。
        /// </summary>
        /// <param name="data">需要计算校验和的数据字节数组</param>
        /// <returns>校验和字节</returns>
        private byte CalculateChecksum(byte[] data)
        {
            int sum = 0;
            foreach (byte b in data)
            {
                sum += b;
            }
            // 取累加和的低8位作为校验和
            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// 构建并发送命令包
        /// 文档第4.3节详细描述了命令包的结构：[标识符][长度][CODE][ID][命令组...][校验和]
        /// 长度LEN是除标识符、长度、校验和外，中间所有字节的个数。
        /// </summary>
        /// <param name="commandBytes">命令组的字节数组（一个或多个命令）</param>
        /// <param name="isBroadcast">是否为广播命令。如果为true，则忽略回复。</param>
        /// <returns>设备回复的状态和原始数据（如果不是广播命令）</returns>
        private byte[] SendCommand(List<byte> commandBytes, bool isBroadcast = false)
        {

            // 文档说明："一个控制包由标识符、长度、设备CODE、设备ID、命令组、校验和依次组成。"
            // 1. 构建基础数据块： [CODE][ID][命令组...]
            List<byte> dataBlock = new List<byte>
            {
                DEVICE_CODE_PBL,      // 设备型号 CODE
                isBroadcast ? BROADCAST_ID : _currentDeviceId // 设备ID
            };
            dataBlock.AddRange(commandBytes);

            // 2. 计算长度LEN: 文档说明"控制包长度为设备CODE、设备ID及所有命令的字节数"
            byte length = (byte)dataBlock.Count; // CODE(1) + ID(1) + 命令字节

            // 3. 构建完整数据包： [标识符][长度][...数据块...]
            List<byte> fullPacket = new List<byte>
            {
                START_IDENTIFIER, // 标识符
                length            // 长度
            };
            fullPacket.AddRange(dataBlock);

            // 4. 计算并附加校验和
            // 注意：校验和的计算范围是标识符开始到命令组结束
            byte checksum = CalculateChecksum(fullPacket.ToArray());
            fullPacket.Add(checksum);
            return fullPacket.ToArray();
            //// 5. 发送数据 (使用锁确保同一时间只有一个线程使用串口)
            //byte[] responseData = null;
            //lock (_comLock)
            //{
            //    _serialPort.DiscardInBuffer(); // 清空输入缓冲区
            //    _serialPort.Write(fullPacket.ToArray(), 0, fullPacket.Count);

            //    // 6. 如果不是广播命令，则等待并解析回复
            //    if (!isBroadcast)
            //    {
            //        // 根据文档，设备只会回复一次，且回复包格式固定
            //        // 成功回复示例: 0x40, 0x03, 0x01, 0x00, 0x00, 0x44
            //        responseData = WaitForResponse();
            //    }
            //}

            //// 7. 处理回复
            //if (isBroadcast)
            //{
            //    // 文档说明："当设备ID为0xFF时,该控制包为广播控制包,设备在收到后作相应处理,但不会回复。"
            //    return (ResponseStatus.Success, null);
            //}

            //if (responseData == null || responseData.Length < 5) // 最小回复包长度
            //{
            //    return (ResponseStatus.Timeout, responseData);
            //}

            //// 验证回复包的校验和
            //byte receivedChecksum = responseData[responseData.Length - 1];
            //// 使用 LINQ 的 Take 方法取前 n-1 个元素
            //byte calculatedChecksum = CalculateChecksum(responseData.Take(responseData.Length - 1).ToArray());
            //if (receivedChecksum != calculatedChecksum)
            //{
            //    return (ResponseStatus.ChecksumMismatch, responseData);
            //}

            //// 解析回复状态
            //// 回复包结构：[0x40][长度][CODE?/CMD?][DATA...][校验和]。根据文档，成功回复的第三个字节是0x00。
            //// 示例中成功回复(0x40,0x03,0x01,0x00,0x00,0x44)的第三个字节是0x01? 这里需要结合文档和示例分析。
            //// 文档表格“命令集”中，0x00和0x01是“设备回复正确/错误”的命令码。但在示例回复中，第三个字节是0x01。
            //// 这可能表示回复包中包含了“回复正确”的命令码(0x00)和可能的其他信息。为简化，我们检查第三个字节。
            //// 更健壮的方式是解析整个回复包，但根据任务，我们先实现基本功能。
            //byte responseCmd = responseData[2]; // 回复包的命令码
            //if (responseCmd == (byte)CommandCode.ResponseSuccess)
            //{
            //    return (ResponseStatus.Success, responseData);
            //}
            //else if (responseCmd == (byte)CommandCode.ResponseError)
            //{
            //    return (ResponseStatus.Error, responseData);
            //}
            //else
            //{
            //    // 可能是读取参数等命令的特定回复，这里暂时视为成功
            //    return (ResponseStatus.Success, responseData);
            //}
        }

        /// <summary>
        /// 等待并读取设备的回复
        /// </summary>
        //private byte[] WaitForResponse(int timeoutMs = 1000)
        //{
        //    List<byte> buffer = new List<byte>();
        //    DateTime startTime = DateTime.Now;

        //    try
        //    {
        //        while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
        //        {
        //            if (_serialPort.BytesToRead > 0)
        //            {
        //                byte readByte = (byte)_serialPort.ReadByte();
        //                buffer.Add(readByte);

        //                // 简单判断：如果收到起始字节0x40，且长度足够，则尝试解析包是否完整
        //                if (buffer.Count >= 2 && buffer[0] == START_IDENTIFIER)
        //                {
        //                    byte expectedLength = buffer[1]; // 第二个字节是长度LEN
        //                                                     // 完整包长度 = 标识符(1) + 长度(1) + LEN + 校验和(1)
        //                    int expectedTotalLength = 2 + expectedLength + 1;
        //                    if (buffer.Count >= expectedTotalLength)
        //                    {
        //                        // 包接收完整
        //                        return buffer.ToArray();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Thread.Sleep(10); // 避免CPU空转
        //            }
        //        }
        //    }
        //    catch (TimeoutException)
        //    {
        //        // 读取超时
        //    }
        //    return buffer.ToArray(); // 返回已读取的数据，可能不完整
        //}

        #endregion

        #region 公开控制命令

        /// <summary>
        /// 设置单个通道的曝光（点亮）时间
        /// 对应命令：0x1A
        /// 文档说明：时间参数范围0~999 (单位us)，但在命令参数中，时间值需要除以10 (N*10us)。
        /// 例如：设置10us，参数为0x01。设置50us，参数为0x05。
        /// </summary>
        /// <param name="channel">通道号 (0-3, 或 0xFF表示所有通道)</param>
        /// <param name="timeMicroseconds">曝光时间，单位微秒(us)，范围0-999</param>
        /// <returns>命令执行状态</returns>
        public List<byte[]> SetExposureTime(CLight Channel)
        {
            List<byte[]> messagebytes = new List<byte[]>();
            byte channel;
            switch (Channel.Channel)
            {
                case "1":
                    channel = 0x00;
                    break;
                case "2":
                    channel = 0x01;
                    break;
                case "3":
                    channel = 0x02;
                    break;
                case "4":
                    channel = 0x03;
                    break;
                default:
                    channel = 0x00;
                    break;
            }
            // 参数验证
            if (Channel.Value < 0 || Channel.Value > 999)
                throw new ArgumentOutOfRangeException(nameof(Channel.Value), "曝光时间必须在0-999微秒范围内");

            // 文档说明："设置时间"命令的参数是 N*10us。所以需要除以10。
            // 但要注意，设备内部存储的值是除以10后的整数。
            byte timeParam = (byte)(Channel.Value / 10);

            // 构建命令：[命令码][通道号][时间参数]
            List<byte> command = new List<byte>
            {
                (byte)CommandCode.SetTime,
                channel,
                timeParam
            };

            // 判断是否为广播命令（对所有通道）
            bool isBroadcast = (byte)channel == BROADCAST_CHANNEL;
            byte[] Exresult = SendCommand(command, isBroadcast);
            byte[] psresult = SetPulseWidthPercent(channel, 20);
            messagebytes.Add(Exresult);
            messagebytes.Add(psresult);
            return messagebytes;
        }

        /// <summary>
        /// 设置单个通道的脉冲宽度百分比
        /// 对应命令：0x22
        /// 文档说明：百分比参数范围0~10 (N*10%)。0=0%, 10=100%。
        /// 例如：设置100%，参数为0x0A。设置30%，参数为0x03。
        /// </summary>
        /// <param name="channel">通道号 (0-3, 或 0xFF表示所有通道)</param>
        /// <param name="percent">百分比，范围0-100。内部会除以10。</param>
        /// <returns>命令执行状态</returns>
        public byte[] SetPulseWidthPercent(byte channel, int percent)
        {
            // 参数验证
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), "百分比必须在0-100范围内");

            // 文档说明："设置百分比"命令的参数是 N*10%。
            byte percentParam = (byte)(percent / 10);

            List<byte> command = new List<byte>
            {
                (byte)CommandCode.SetPercent,
                (byte)channel,
                percentParam
            };

            bool isBroadcast = (byte)channel == BROADCAST_CHANNEL;
            var result = SendCommand(command, isBroadcast);
            return result;
        }

        /// <summary>
        /// 设置设备ID
        /// 对应命令：0x09
        /// 文档说明：ID参数范围0~99。
        /// 重要：设置成功后，需要更新当前类的_deviceId，否则后续命令无法控制新ID的设备。
        /// </summary>
        /// <param name="newDeviceId">新的设备ID，范围0-99</param>
        /// <returns>命令执行状态</returns>
        public byte[] SetDeviceId(byte newDeviceId)
        {
            if (newDeviceId > 99)
                throw new ArgumentOutOfRangeException(nameof(newDeviceId), "设备ID必须在0-99范围内");

            // 注意：设置ID时，命令是发送给当前ID的设备，将其ID改为newDeviceId。
            // 所以这个命令不是广播命令。
            List<byte> command = new List<byte>
            {
                (byte)CommandCode.SetID,
                newDeviceId
            };

            var result = SendCommand(command, false);

            _currentDeviceId = newDeviceId; // 更新内部记录的ID

            return result;
        }

        /// <summary>
        /// 读取一个或多个通道的参数
        /// 对应命令：0x31
        /// 文档说明：读取指定通道（0-3）或所有通道（0xFF）的当前时间和百分比设置。
        /// 成功读取后，设备会回复一个数据包，其命令码为0x5A（单通道）或0x60（全通道）。
        /// 由于回复解析较为复杂，此方法暂不实现完整解析，仅返回原始数据。
        /// </summary>
        /// <param name="channel">通道号 (0-3, 或 0xFF表示所有通道)</param>
        /// <returns>包含状态和原始回复数据的元组</returns>
        public  byte[] ReadParameters(byte channel)
        {
            List<byte> command = new List<byte>
            {
                (byte)CommandCode.ReadParams,
                (byte)channel
            };

            bool isBroadcast = (byte)channel == BROADCAST_CHANNEL;
            return SendCommand(command, isBroadcast);
        }

        /// <summary>
        /// 一次性设置多个通道的时间（高效方法）
        /// 文档中提供了多命令包的示例，例如同时设置L1-L4的时间。
        /// 此方法将多个设置命令打包成一个数据包发送，提高效率。
        /// </summary>
        /// <param name="channelTimePairs">通道与时间的字典。键为通道，值为时间(us)。</param>
        /// <returns>命令执行状态</returns>
        //public ResponseStatus SetExposureTimeForChannels(Dictionary<LightChannel, int> channelTimePairs)
        //{
        //    List<byte> commands = new List<byte>();

        //    foreach (var pair in channelTimePairs)
        //    {
        //        if (pair.Value < 0 || pair.Value > 999)
        //            throw new ArgumentOutOfRangeException($"通道 {pair.Key} 的时间值无效，必须在0-999微秒内");

        //        byte timeParam = (byte)(pair.Value / 10);
        //        commands.Add((byte)CommandCode.SetTime);
        //        commands.Add((byte)pair.Key);
        //        commands.Add(timeParam);
        //    }

        //    // 多命令包不是广播，除非所有通道号都是0xFF（这在此处不合理）
        //    var result = SendCommand(commands, false);
        //    return result.Status;
        //}

        #endregion
    }
}
