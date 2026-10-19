namespace LteRefSensTester.Models
{
    /// <summary>
    /// Centralized test configuration parameters
    /// </summary>
    public class TestConfig
    {
        /// <summary>
        /// CMW500 IP address
        /// </summary>
        public string CmwAddress { get; set; } = "192.0.2.10";

        /// <summary>
        /// Starting power level for search (dBm)
        /// </summary>
        public double StartLevel { get; set; } = -70.0;

        /// <summary>
        /// Minimum allowed power level (safety limit, dBm)
        /// </summary>
        public double MinLevel { get; set; } = -120.0;

        /// <summary>
        /// BLER threshold for 3× confirmation (default 5%)
        /// </summary>
        public double BlerThreshold { get; set; } = 0.05;

        /// <summary>
        /// Skip CMW initialization if UE already connected
        /// </summary>
        public bool SkipInit { get; set; } = true;

        /// <summary>
        /// Maximum search iterations (safety limit)
        /// </summary>
        public int MaxSearchCount { get; set; } = 200;

        /// <summary>
        /// Whether to sample UE TX power at each convergence point (PMONitor:AVERage).
        /// Default OFF — adds ~4 seconds per test point when enabled.
        /// </summary>
        public bool UePowerEnabled { get; set; } = false;

        /// <summary>
        /// 强制指定 UE 侧 USB CDC 串口（音乐 pa play/reset 与 SIM AT+CFUN=1 都走这个口）。
        /// null / 空串 = 自动枚举（原逻辑）。
        /// </summary>
        public string? ForcedUeComPort { get; set; }
    }
}
