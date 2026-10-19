namespace LteRefSensTester.Models
{
    /// <summary>
    /// Represents a test channel point with band, label (Low/Mid/High), and EARFCN
    /// </summary>
    public record ChannelPoint(BandConfig Band, string Label, int Earfcn);
}
