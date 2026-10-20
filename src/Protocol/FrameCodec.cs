using System;
using System.Text;

namespace LteRefSensTester.Protocol
{
    /// <summary>协议模式：单层裸 UART 帧 或 外层 USB-CDC + 内层 UART 双层帧。</summary>
    public enum ProtoMode
    {
        Uart,
        UsbCdc,
    }

    /// <summary>
    /// PLAUD 私有 shell 协议帧编解码。
    /// - Shell JSON：{"seqNum":N,"cmdType":65535,"shellCmd":S,"details":{"operation":OP,"params":[...]}}
    /// - 内层 UART 头 16 字节：MAGIC(4) + ver1(2)=1 + ver2(2)=1 + msg_len(4) + crc_msg(2) + crc_hdr(2)
    /// - 外层 USB-CDC 头 16 字节：MAGIC(4) + pkt_len(4) + 0x01(1) + 0xFF(1) + json_len(2) + 0(2) + 0(2)
    /// - MAGIC = 0xFDFCFBFA；CRC = CRC-16/CCITT-FALSE
    /// </summary>
    public static class FrameCodec
    {
        public const uint Magic = 0xFDFCFBFA;
        public const int UartHeaderLen = 16;
        public const int UsbCdcHeaderLen = 16;

        /// <summary>按空格全拆：shellCmd=第 1 词, operation=第 2 词, params=剩下每词一个元素。
        /// 这是设备真正认的分解方式（reset / pa play 已实测验证）。</summary>
        public static string BuildShellJsonArray(int seq, string command)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>直接以 (shellCmd, operation, params[]) 三元组构建 shell JSON（紧凑无空格）。</summary>
        public static string BuildShellJsonParts(int seq, string shell, string op, string[] pars)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>构建整帧（可选双层）。json 是已编码好的 UTF-8 JSON body 字节。</summary>
        public static byte[] BuildFrame(byte[] json, ProtoMode mode)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>内层 UART 帧：16 字节头 + JSON body。
        /// 对齐 Python 参考实现：crc_hdr 计算时先把整 16 字节头（crc_hdr 槽置 0）
        /// 参与 CRC，而不是只算前 14 字节。这两者结果不同。</summary>
        private static byte[] BuildUartFrame(byte[] json)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>CRC-16/CCITT-FALSE：poly=0x1021, init=0xFFFF, refin=false, refout=false, xorout=0x0000。
        /// 校验向量：crc16("123456789") = 0x29B1。</summary>
        public static ushort Crc16CcittFalse(byte[] data, int offset, int length)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private static void WriteUInt16LE(byte[] buf, int offset, ushort v)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private static void WriteUInt32LE(byte[] buf, int offset, uint v)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private static string JStr(string s)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }
    }
}
