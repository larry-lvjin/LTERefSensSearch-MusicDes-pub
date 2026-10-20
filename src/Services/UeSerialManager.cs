using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LteRefSensTester.Protocol;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// 全局唯一 UE 串口管理器（Singleton + SemaphoreSlim）。
    ///
    /// 目的：把之前散落在 TestService 里 3 条并存的写串口路径
    ///   (a) SerialClient 帧协议
    ///   (b) P/Invoke CreateFileW 裸 ASCII
    ///   (c) 旧 SendUeSerialCommandAsync
    /// 合并成一条串行化通道。所有 UE 侧命令 (reset / pa play / cd uart3_proxy / send AT+*)
    /// 必须走这里，杜绝任何路径绕开单例直接 new SerialPort()，从根源上消除
    /// err=5 (Access denied) 与端口重入竞争。
    ///
    /// 两种发送姿势：
    ///   SendAsync(cmd)   — 普通命令，短 read，close 后 150ms grace，err=5 三次退避重试。
    ///   ResetAsync(cmd)  — reset 类命令：写入 → 等端口从 GetPortNames 消失 (≤disappearTimeoutMs)
    ///                     → 等端口重现 (≤reappearTimeoutMs) → 额外 grace 让 uart3_proxy 起来。
    ///                     快场景 (设备秒重启) 不浪费固定 sleep；慢场景不会打空后续命令。
    ///                     消失超时视为"设备未重启"，直接返回不做 grace。
    /// </summary>
    public sealed class UeSerialManager
    {
        private static readonly Lazy<UeSerialManager> _inst = new(() => new UeSerialManager());
        public static UeSerialManager Instance => _inst.Value;

        private readonly SemaphoreSlim _lock = new(1, 1);

        // 230400 8N1，与旧 SimSerialBaudRate/SerialClient 保持一致
        private const int BaudRate = 230400;

        // close() 后驱动释放 handle 需要一小段时间，否则下一次 Open() 会 err=5
        private const int PostCloseGraceMs = 150;

        private string? _forcedPort;
        private Action<string>? _logger;

        // 帧协议 seqNum —— 与 sim_simulator_tool.py 的 global seq_num / next_seq() 对齐，
        // Manager 生命周期内严格单调递增。
        private int _seq;
        private int NextSeq() => Interlocked.Increment(ref _seq);

        private UeSerialManager() { }

        /// <summary>
        /// 每轮测试开始前由 TestService 调用一次。
        /// forcedPort=null/空 → 自动模式（枚举注册表 SERIALCOMM 里 USB 前缀口，取第一个可用）。
        /// forcedPort="COM10" → 强制模式（只用该口，等它出现在 GetPortNames 里）。
        /// </summary>
        public void Configure(string? forcedPort, Action<string>? logger)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// 发普通命令：拿锁 → 等口 → Open → WriteLine → ReadExisting → Close → 150ms grace。
        /// 遇到 UnauthorizedAccessException (err=5) 三次指数退避重试。
        /// </summary>
        public async Task<string> SendAsync(string cmd, int readTimeoutMs = 800,
                                             int portWaitTimeoutMs = 20000,
                                             CancellationToken ct = default)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// Reset 类命令的智能等待：写入 → 等端口消失 (≤disappearTimeoutMs, 超时=未重启, 提前返回)
        /// → 等端口重现 (≤reappearTimeoutMs, 超时抛 IOException) → 额外 grace 让 uart3_proxy 就绪。
        /// </summary>
        public async Task ResetAsync(string cmd,
                                     int disappearTimeoutMs = 5000,
                                     int reappearTimeoutMs = 20000,
                                     int graceMs = 2500,
                                     CancellationToken ct = default)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // 已持锁；不会自锁。portWaitTimeoutMs=0 表示"不等，只查一次"。
        //
        // ★ 幽灵端口修复：每次 attempt 都重新解析端口。
        //   Windows 侧 SERIALCOMM 注册表 / GetPortNames() 在 USB 热插拔窗口可能残留死口 —— 首次解析拿到 COM9
        //   但 Open() 抛 FileNotFound。旧实现在循环外解析一次，3 次重试全打同一个死口。
        //   现在：每轮循环重解析；auto 模式把抛 IOException 的口加入 deadPorts 排除，尝试下一个 USB CDC 候选；
        //   forced 模式保持用户指定的口不换（err=5/2 都只 backoff 重试同口）。
        private async Task<string> SendUnlockedAsync(string cmd, int readTimeoutMs, int portWaitTimeoutMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // 等待目标口出现在 GetPortNames() 里；返回具体的端口字符串。excluded 内的口一律跳过。
        private async Task<string> WaitForPortAsync(int timeoutMs, CancellationToken ct, HashSet<string>? excluded = null)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // 强制模式：只认 _forcedPort，且必须在 GetPortNames() 里；excluded 命中直接返回 null（等价 timeout）
        // 自动模式：读注册表 USB CDC，返回第一个当前枚举可见且未被 excluded 排除的
        private string? ResolveCurrentPort(HashSet<string>? excluded = null)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private bool PortListed(string port) =>
            SerialPort.GetPortNames().Contains(port, StringComparer.OrdinalIgnoreCase);

        // 走 SetupAPI 枚举当前 present 的 USB CDC 端口。语义对齐 pyserial `list_ports.comports()` +
        // `is_usb_port(p) = "USB" in hwid.upper()` —— 与 sim_simulator_tool.exe 反编译结果一致。
        // 相比旧版读注册表 HKLM\HARDWARE\DEVICEMAP\SERIALCOMM 过滤设备名含 "USB"：
        //   - 不受 stale 注册表条目污染
        //   - 判断标准是 USB VID/PID hwid 而非驱动写的 Device Name
        //   - 覆盖驱动名为 \Device\Serial* 的 USB CDC（旧逻辑会漏）
        private static IEnumerable<(string Port, string Device)> EnumerateUsbComPorts()
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }
    }
}
