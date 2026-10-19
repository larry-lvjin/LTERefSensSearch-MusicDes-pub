using System;
using System.ComponentModel;
using System.Windows.Media;

namespace LteRefSensTester.Models
{
    /// <summary>
    /// DataGrid row for reference sensitivity test results (Before / During music dual pass)
    /// </summary>
    public class RefSensRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void N(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public string BandDisplay { get; init; } = "";
        public string DuplexMode { get; init; } = "";
        public string Channel { get; init; } = "";
        public int Earfcn { get; init; }
        public int DlRb { get; init; }
        public int UlRb { get; init; }

        // ── 音乐前 (Before music) phase ────────────────────────────
        double _rsBefore = double.NaN;
        public double RefSensBefore
        {
            get => _rsBefore;
            set
            {
                _rsBefore = value;
                N(nameof(RefSensBefore));
                N(nameof(RefSensBeforeStr));
                N(nameof(SensColor));
                N(nameof(RefSensDiff));
                N(nameof(RefSensDiffStr));
                N(nameof(DiffColor));
                N(nameof(ResultText));
                N(nameof(ResultColor));
            }
        }
        public string RefSensBeforeStr => double.IsNaN(_rsBefore) ? "—" : $"{_rsBefore:F1}";

        // ── 音乐中 (During music) phase ────────────────────────────
        double _rsDuring = double.NaN;
        public double RefSensDuring
        {
            get => _rsDuring;
            set
            {
                _rsDuring = value;
                N(nameof(RefSensDuring));
                N(nameof(RefSensDuringStr));
                N(nameof(SensColor));
                N(nameof(RefSensDiff));
                N(nameof(RefSensDiffStr));
                N(nameof(DiffColor));
                N(nameof(ResultText));
                N(nameof(ResultColor));
            }
        }
        public string RefSensDuringStr => double.IsNaN(_rsDuring) ? "—" : $"{_rsDuring:F1}";

        // ── RSRP at convergence level (from CMW500 UE report) ────────────────
        // 显示格式："-86.5 (54)"  =  RANGe 中点 dBm + 原始 3GPP index (0..97)。
        // dBm 由 SENSe:LTE:SIGN1:UEReport:PCC:RSRP:RANGe? 取上下限中点，
        // Idx 由 SENSe:LTE:SIGN1:UEReport:PCC:RSRP? 得到（-1 = INV/未上报）。
        // 深度探底或失步时 UE 停发 report，显示 "—"，不参与音乐前/中差值计算。
        double _rsrpBefore = double.NaN;
        public double RsrpBefore
        {
            get => _rsrpBefore;
            set
            {
                _rsrpBefore = value;
                N(nameof(RsrpBefore));
                N(nameof(RsrpBeforeStr));
            }
        }

        int _rsrpBeforeIdx = -1;
        public int RsrpBeforeIdx
        {
            get => _rsrpBeforeIdx;
            set
            {
                _rsrpBeforeIdx = value;
                N(nameof(RsrpBeforeIdx));
                N(nameof(RsrpBeforeStr));
            }
        }

        public string RsrpBeforeStr =>
            double.IsNaN(_rsrpBefore) ? "—" :
            _rsrpBeforeIdx < 0        ? $"{_rsrpBefore:F1}" :
                                        $"{_rsrpBefore:F1} ({_rsrpBeforeIdx})";

        double _rsrpDuring = double.NaN;
        public double RsrpDuring
        {
            get => _rsrpDuring;
            set
            {
                _rsrpDuring = value;
                N(nameof(RsrpDuring));
                N(nameof(RsrpDuringStr));
            }
        }

        int _rsrpDuringIdx = -1;
        public int RsrpDuringIdx
        {
            get => _rsrpDuringIdx;
            set
            {
                _rsrpDuringIdx = value;
                N(nameof(RsrpDuringIdx));
                N(nameof(RsrpDuringStr));
            }
        }

        public string RsrpDuringStr =>
            double.IsNaN(_rsrpDuring) ? "—" :
            _rsrpDuringIdx < 0        ? $"{_rsrpDuring:F1}" :
                                        $"{_rsrpDuring:F1} ({_rsrpDuringIdx})";

        // ── UE TX power at convergence level (dBm) ────────────────
        double _uePwrBefore = double.NaN;
        public double UePowerBefore
        {
            get => _uePwrBefore;
            set
            {
                _uePwrBefore = value;
                N(nameof(UePowerBefore));
                N(nameof(UePowerBeforeStr));
            }
        }
        public string UePowerBeforeStr => double.IsNaN(_uePwrBefore) ? "—" : $"{_uePwrBefore:F1}";

        double _uePwrDuring = double.NaN;
        public double UePowerDuring
        {
            get => _uePwrDuring;
            set
            {
                _uePwrDuring = value;
                N(nameof(UePowerDuring));
                N(nameof(UePowerDuringStr));
            }
        }
        public string UePowerDuringStr => double.IsNaN(_uePwrDuring) ? "—" : $"{_uePwrDuring:F1}";

        // ── Diff: During − Before (dB) ─────────────────────────────
        // 音乐中 减 音乐前：正值表示音乐把灵敏度推高（性能变差）。
        public double RefSensDiff =>
            (double.IsNaN(_rsBefore) || double.IsNaN(_rsDuring)) ? double.NaN : (_rsDuring - _rsBefore);
        public string RefSensDiffStr =>
            double.IsNaN(RefSensDiff) ? "—" : $"{RefSensDiff:+0.0;-0.0;0.0}";

        public Brush SensColor =>
            (double.IsNaN(_rsBefore) && double.IsNaN(_rsDuring)) ? Brushes.White : Brushes.LightGreen;

        public Brush DiffColor => double.IsNaN(RefSensDiff) ? Brushes.White : Brushes.LightYellow;

        // ── Shared probe count / status / note ─────────────────────
        int _sc;
        public int SearchCount
        {
            get => _sc;
            set
            {
                _sc = value;
                N(nameof(SearchCount));
            }
        }

        string _st = "";
        public string StatusText
        {
            get => _st;
            set
            {
                _st = value;
                N(nameof(StatusText));
                N(nameof(StatusColor));
                N(nameof(ResultText));
                N(nameof(ResultColor));
            }
        }
        public Brush StatusColor => _st == "DONE" ? Brushes.LightGreen :
                                    _st == "ERROR" ? Brushes.Tomato :
                                    _st == "SKIP" ? Brushes.Gray : Brushes.White;

        // ── Result: PASS / FAIL based on Diff ∈ [-5, 5] ─────────────
        // 仅在 DONE 且 Diff 有效时判定；ERROR/SKIP 透传 StatusText
        public string ResultText
        {
            get
            {
                if (_st == "ERROR") return "ERROR";
                if (_st == "SKIP") return "SKIP";
                if (_st == "DONE")
                {
                    double d = RefSensDiff;
                    if (double.IsNaN(d)) return "—";
                    return (d >= -5.0 && d <= 5.0) ? "PASS" : "FAIL";
                }
                return "";
            }
        }

        public Brush ResultColor
        {
            get => ResultText switch
            {
                "PASS" => Brushes.LightGreen,
                "FAIL" => Brushes.Tomato,
                "ERROR" => Brushes.Tomato,
                "SKIP" => Brushes.Gray,
                _ => Brushes.White
            };
        }

        string _note = "";
        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                N(nameof(Note));
            }
        }
    }
}
