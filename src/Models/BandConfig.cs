using System.Collections.Generic;

namespace LteRefSensTester.Models
{
    /// <summary>
    /// LTE band configuration parameters
    /// </summary>
    public class BandConfig
    {
        public int Band { get; init; }
        public DuplexMode Mode { get; init; }
        public string CmwCode { get; init; } = "";
        public string DlBwCode { get; init; } = "B100";   // 10 MHz
        public int DlRb { get; init; } = 50;
        public int UlRb { get; init; } = 50;
        public int EarfcnLow { get; init; }
        public int EarfcnMid { get; init; }
        public int EarfcnHigh { get; init; }

        /// <summary>
        /// Per-band start level = measured refSens + 1 dBm (legacy, kept for backward compatibility)
        /// Can be modified by importing template
        /// </summary>
        public double RefStart { get; set; } = -100.0;

        /// <summary>
        /// Per-channel start levels: Low/Mid/High channels can have different initial levels
        /// Key: "Low", "Mid", "High"
        /// </summary>
        public Dictionary<string, double> RefStartByChannel { get; set; } = new()
        {
            { "Low", -100.0 },
            { "Mid", -100.0 },
            { "High", -100.0 }
        };

        /// <summary>
        /// Get RefStart value for a specific channel (Low/Mid/High)
        /// </summary>
        public double GetRefStart(string channel)
        {
            return RefStartByChannel.TryGetValue(channel, out var value) ? value : RefStart;
        }

        /// <summary>
        /// Set RefStart value for a specific channel (Low/Mid/High)
        /// </summary>
        public void SetRefStart(string channel, double value)
        {
            RefStartByChannel[channel] = value;
        }
    }
}
