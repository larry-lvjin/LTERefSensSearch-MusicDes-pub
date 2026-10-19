using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LteRefSensTester.Models;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// Progress information for test execution
    /// </summary>
    public class TestProgress
    {
        public int CurrentChannel { get; set; }
        public int TotalChannels { get; set; }
        public string Status { get; set; } = "";
        public string LogMessage { get; set; } = "";
        public double CurrentLevel { get; set; }
        public double CurrentBler { get; set; }
        public RefSensRow? ResultRow { get; set; }
    }

    /// <summary>
    /// Test result container
    /// </summary>
    public class TestResult
    {
        public List<RefSensRow> Results { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Interface for test execution service
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// Run reference sensitivity test on channel list
        /// </summary>
        Task<TestResult> RunRefSensTestAsync(
            List<ChannelPoint> channels,
            TestConfig config,
            LossTable uplinkLoss,
            LossTable downlinkLoss,
            IProgress<TestProgress>? progress,
            CancellationToken ct);

        /// <summary>
        /// Initialize CMW500 for testing
        /// </summary>
        Task<bool> InitializeCmwAsync(TestConfig config, CancellationToken ct);

        /// <summary>
        /// Measure BLER at current power level
        /// </summary>
        Task<double> MeasureBlerAsync(int measurementMs, CancellationToken ct);

        /// <summary>
        /// Search for reference sensitivity on single channel
        /// </summary>
        Task<(double refSens, double bler, int searchCount)> SearchRefSensAsync(
            ChannelPoint channel,
            double startLevel,
            double minLevel,
            double blerThreshold,
            CancellationToken ct);
    }
}
