using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LteRefSensTester.Models;
using LteRefSensTester.Protocol;
using Microsoft.Win32;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// Exception thrown when UE drops from CEST during measurement
    /// </summary>
    public class UeDisconnectedException : Exception
    {
        public UeDisconnectedException(string msg) : base(msg) { }
    }

    public class InterferenceDetectedException : Exception
    {
        public InterferenceDetectedException(string msg) : base(msg) { }
    }

    public class ReconnectExhaustedException : Exception
    {
        public ReconnectExhaustedException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Test execution service implementation
    /// </summary>
    public class TestService : ITestService
    {
        private readonly ICmwService _cmwService;
        private readonly ILogService _logService;

        // Pause state management
        private bool _paused;
        private CancellationTokenSource? _pauseCts;

        // Track previous channel mode for TDD/FDD switching detection
        private DuplexMode? _previousMode = null;

        // 强制 UE 侧 COM 口（null = 自动枚举）。由 RunRefSensTestAsync 从 TestConfig 每次运行前刷新，
        // 同时注入到 UeSerialManager.Instance.Configure() —— 全部 UE 命令都走单例 Manager。
        private string? _forcedUeComPort;

        public TestService(ICmwService cmwService, ILogService logService)
        {
            _cmwService = cmwService ?? throw new ArgumentNullException(nameof(cmwService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _pauseCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Set RS EPRE level with OPC confirmation and readback verification
        /// </summary>
        private async Task SetLevelAsync(double levelDbm, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// 在灵敏度命中点采样一次 UE 上行 TX 功率（PMONitor:AVERage）
        /// 已在 CMW500 v3.7.170 实测验证（13/13 PASS）
        /// 关键：CSPath 路由 + 单次 10-subframe 测量 + 状态轮询 + finally ABORt
        /// 返回 dBm；任何失败返回 NaN 不阻塞主探底流程
        /// </summary>
        private async Task<double> MeasureUePowerAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// 查询 UE 上报的 RSRP（PCC，Primary Component Carrier）— 同时读原始 index 和 RANGe。
        /// SCPI:
        ///   · SENSe:LTE:SIGN1:UEReport:PCC:RSRP?         → 3GPP 无量纲 index 0..97（INV/NCAP 表示无数据）
        ///   · SENSe:LTE:SIGN1:UEReport:PCC:RSRP:RANGe?   → "lo,hi" dBm 数值区间，如 "-103,-102"
        /// 返回 (idx, dbmMid)：
        ///   · idx  = -1 表示 index 查询失败或 INV；否则 0..97
        ///   · dbmMid = RANGe 中点 (lo+hi)/2；RANGe 无效时按 3GPP 映射用 idx 兜底得中点 (-140.5+idx)
        /// 深度探底时 UE 停发 Measurement Report 属正常现象；调用方应容忍 NaN。
        /// </summary>
        private async Task<(int idx, double dbm)> MeasureRsrpAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// Pause the running test
        /// </summary>
        public void Pause()
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// Resume the paused test
        /// </summary>
        public void Resume()
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        public async Task<TestResult> RunRefSensTestAsync(
            List<ChannelPoint> channels,
            TestConfig config,
            LossTable uplinkLoss,
            LossTable downlinkLoss,
            IProgress<TestProgress>? progress,
            CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        public async Task<bool> InitializeCmwAsync(TestConfig config, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        public async Task<double> MeasureBlerAsync(int measurementMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private async Task OpcWithRetryAsync(int timeoutMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        public async Task<(double refSens, double bler, int searchCount)> SearchRefSensAsync(
            ChannelPoint channel,
            double startLevel,
            double minLevel,
            double blerThreshold,
            CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  Complete Test Algorithm (v11 — 探底/寻值/确认/出值 四段流水线)
        //    Init : 起始电平 → 1000 帧 BLER
        //    Step 1 (探底 ±1 dB)
        //      Case 1  BLER < 2%       → -1 直到 2% <= BLER
        //      Case 2  2% <= BLER <=20% → 跳过
        //      Case 3  BLER > 20%      → +1 直到 BLER <= 20%
        //    Step 2 (寻值 ±0.5 dB, 依 Step 1 出口 BLER 分派)
        //      Case 1  BLER < 2%       → -0.5 直到 2% <= BLER
        //      Case 2  BLER > 20%      → +0.5 直到 BLER <= 20%
        //      其他                     → 跳过
        //    Step 3 (确认 ±0.1 dB, 依上游出口 BLER 分派, 找 5% 边界)
        //      Case 1  BLER < 5%       → -0.1 直到 5% <= BLER, 然后 RSEP+0.1 回补一步
        //      Case 2  BLER >= 5%      → +0.1 直到 BLER < 5%
        //    Step 4 (出值 3000 帧 extended BLER)
        //      b < 5%  → REFSENS = 当前电平
        //      否则     → RSEP+0.1 重测, 直到通过
        // ══════════════════════════════════════════════════════════════
        private async Task<(double level, double bler, int probes)> SearchRefSensAsync(
            ChannelPoint currentPt,
            ChannelPoint? fallbackPt,
            double startLevel,
            double minLevel,
            double blerThresh,
            double uLoss,
            double dLoss,
            CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  Helper Methods
        // ══════════════════════════════════════════════════════════════

        private async Task WaitIfPausedAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private async Task PausableDelay(int ms, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  LOCKED — 信道切换底层逻辑，禁止 AI 修改                       ║
        // ║  规则：                                                        ║
        // ║   1. 同 FDD→FDD 或同 TDD→TDD：仅设 BAND/信道/RB，不断开小区    ║
        // ║   2. FDD↔TDD 切换：断开小区 + InitEblFullAsync + 重启小区     ║
        // ║   3. 首通道 RRC=CEST 时：跳过 EBL 初始化，直接设参数           ║
        // ╚════════════════════════════════════════════════════════════════╝

        private async Task SwitchChannelAsync(ChannelPoint pt, double uLoss, double dLoss, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// [LOCKED] 首通道或 FDD↔TDD 模式切换时执行完整 EBL 初始化
        /// 设置 MEV 子帧偏移 + EBL 子帧数
        /// </summary>
        private async Task InitEblFullAsync(DuplexMode mode, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        /// <summary>
        /// [LOCKED] 同模式信道切换后执行 Extended BLER OFF → ON 刷新测量状态
        /// 不重新配置 SUBF，仅清除残留状态
        /// </summary>
        private async Task RefreshEblAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  Connection state helpers — RRC-only
        // ══════════════════════════════════════════════════════════════

        private async Task<bool> WaitForRrcConnAsync(int timeoutMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private async Task<bool> SendConnectAndWaitConnAsync(int timeoutMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private async Task RefreshEblAfterRecoveryAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  Post-switch RRC establishment — 只看 RRC state
        // ══════════════════════════════════════════════════════════════
        private async Task EnsureRrcAfterSwitchAsync(
            ChannelPoint currentPt,
            ChannelPoint? fallbackPt,
            bool afterModeSwitch,
            CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        private async Task ApplyChannelParams(ChannelPoint pt, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  重连环节 — 唯一的 RRC≠connected 恢复方式（探底算法内的掉线恢复）
        //  记录 RS EPRE → 设为 −85 → LTE signaling OFF/ON →
        //  循环 iter 1..16：等 10s 查 RRC，iter 4/8/12/16 IDLE 时追加 OFF/ON + SIM(AT+CFUN=1)
        //    RRC=CONN (connected)：还原 RS EPRE 返回
        //    RRC≠CONN：进入下一轮；16 轮 + 最终检测仍 IDLE → 抛异常跳过当前信道
        // ══════════════════════════════════════════════════════════════

        // 唯一的连接状态判据：SENSe:LTE:SIGN1:RRCState? 返回 "CONN..." 即视为已连接。
        private async Task<(bool connected, string status)> IsConnectedAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }
        // ══════════════════════════════════════════════════════════════
        //  SIM 卡模拟器复位 — 通过 UeSerialManager 统一发 AT 序列
        //  流程等价旧版 sim_simulator_tool.py，但走单例串口通道，杜绝端口重入：
        //    cd uart3_proxy                        (等 800ms  proxy 模式切换)
        //    send AT+ECSIMCFG="SimSimulator",1     (等 2000ms SIM 内部处理)
        //    send AT+CFUN=1                        (等 500ms  软复位命令回执)
        //  由 ReconnectOnIdleAsync 在 iter 4/8/12/16 触发；任何异常都吞掉不阻塞主流程。
        // ══════════════════════════════════════════════════════════════
        private const string SimResetCommand = "AT+CFUN=1";

        private async Task RunSimSimulatorToolAsync(CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  重连环节 — 16 轮定长循环，每轮 10s 窗口内 1s 轮询 RRC 提前检出
        //  1) 记录 RS EPRE → 设 -85 → LTE signaling OFF/ON（各一次）
        //  2) 循环 iter 1..16：
        //     · 10s 窗口内每 1s 轮询 RRC；一旦 CONN 立刻还原 RS EPRE 返回
        //     · IDLE 时：iter 4/8/12/16 触发【LTE signaling OFF/ON + SIM 复位 (AT+CFUN=1)】
        //  3) 16 轮末尾（iter 16 已跑过 OFF/ON + SIM）再做最终 10s 轮询
        //  4) 仍 IDLE → 抛 ReconnectExhaustedException → 主循环标 SKIP 切下一信道
        // ══════════════════════════════════════════════════════════════
        private const int ReconnectMaxIters = 16;
        private const int ReconnectWaitMs = 10000;
        private const int ReconnectPollIntervalMs = 1000;

        private async Task ReconnectOnIdleAsync(double savedRsep, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // LTE signaling OFF → 1s → ON
        private async Task CellOffOnAsync(string tag, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // 10s 窗口内每 1s 轮询 RRC，发现 CONN 立即返回 true；超时返回 false。
        private async Task<bool> PollForConnectedAsync(int totalMs, CancellationToken ct)
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        // ══════════════════════════════════════════════════════════════
        //  Test wrappers (for simulation only)
        // ══════════════════════════════════════════════════════════════
        public Task TestSwitchChannelAsync(ChannelPoint pt, CancellationToken ct)
            => SwitchChannelAsync(pt, 0, 0, ct);

        public Task TestEnsureRrcAsync(ChannelPoint pt, ChannelPoint? fallback, bool modeSwitch, CancellationToken ct)
            => EnsureRrcAfterSwitchAsync(pt, fallback, modeSwitch, ct);
    }
}
