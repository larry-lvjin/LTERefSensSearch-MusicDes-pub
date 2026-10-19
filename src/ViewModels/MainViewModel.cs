using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using LteRefSensTester.Commands;
using LteRefSensTester.Helpers;
using LteRefSensTester.Models;
using LteRefSensTester.Services;
using Microsoft.Win32;

namespace LteRefSensTester.ViewModels
{
    /// <summary>
    /// Main window ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        // Band configuration table. RefStart is the default per-band starting level
        // (currently -110 dBm for all bands, temporarily uniform). It is copied to
        // RefStartByChannel[Low/Mid/High] on startup and can be overridden per-channel
        // by importing 初始电平模板.xlsx via the RefSens import command.
        public static readonly List<BandConfig> AllBands = InitializeBands();

        private static List<BandConfig> InitializeBands()
        {
            var bands = new List<BandConfig>
            {
                new(){Band=1,  Mode=DuplexMode.FDD, CmwCode="OB1",  DlRb=50, UlRb=50, EarfcnLow=18050, EarfcnMid=18300, EarfcnHigh=18550, RefStart=-110.0},
                new(){Band=2,  Mode=DuplexMode.FDD, CmwCode="OB2",  DlRb=50, UlRb=50, EarfcnLow=18650, EarfcnMid=18900, EarfcnHigh=19150, RefStart=-110.0},
                new(){Band=3,  Mode=DuplexMode.FDD, CmwCode="OB3",  DlRb=50, UlRb=50, EarfcnLow=19250, EarfcnMid=19575, EarfcnHigh=19900, RefStart=-110.0},
                new(){Band=4,  Mode=DuplexMode.FDD, CmwCode="OB4",  DlRb=50, UlRb=50, EarfcnLow=20000, EarfcnMid=20175, EarfcnHigh=20350, RefStart=-110.0},
                new(){Band=5,  Mode=DuplexMode.FDD, CmwCode="OB5",  DlRb=50, UlRb=25, EarfcnLow=20450, EarfcnMid=20525, EarfcnHigh=20600, RefStart=-110.0},
                new(){Band=7,  Mode=DuplexMode.FDD, CmwCode="OB7",  DlRb=50, UlRb=50, EarfcnLow=20800, EarfcnMid=21100, EarfcnHigh=21400, RefStart=-110.0},
                new(){Band=8,  Mode=DuplexMode.FDD, CmwCode="OB8",  DlRb=50, UlRb=25, EarfcnLow=21500, EarfcnMid=21625, EarfcnHigh=21750, RefStart=-110.0},
                new(){Band=12, Mode=DuplexMode.FDD, CmwCode="OB12", DlRb=50, UlRb=20, EarfcnLow=23060, EarfcnMid=23095, EarfcnHigh=23130, RefStart=-110.0},
                new(){Band=13, Mode=DuplexMode.FDD, CmwCode="OB13", DlRb=50, UlRb=20, EarfcnLow=23230, EarfcnMid=23230, EarfcnHigh=23230, RefStart=-110.0},
                new(){Band=17, Mode=DuplexMode.FDD, CmwCode="OB17", DlRb=50, UlRb=20, EarfcnLow=23780, EarfcnMid=23790, EarfcnHigh=23800, RefStart=-110.0},
                new(){Band=18, Mode=DuplexMode.FDD, CmwCode="OB18", DlRb=50, UlRb=25, EarfcnLow=23900, EarfcnMid=23925, EarfcnHigh=23950, RefStart=-110.0},
                new(){Band=19, Mode=DuplexMode.FDD, CmwCode="OB19", DlRb=50, UlRb=25, EarfcnLow=24050, EarfcnMid=24075, EarfcnHigh=24100, RefStart=-110.0},
                new(){Band=20, Mode=DuplexMode.FDD, CmwCode="OB20", DlRb=50, UlRb=20, EarfcnLow=24200, EarfcnMid=24300, EarfcnHigh=24400, RefStart=-110.0},
                new(){Band=25, Mode=DuplexMode.FDD, CmwCode="OB25", DlRb=50, UlRb=50, EarfcnLow=26090, EarfcnMid=26365, EarfcnHigh=26640, RefStart=-110.0},
                new(){Band=26, Mode=DuplexMode.FDD, CmwCode="OB26", DlRb=50, UlRb=25, EarfcnLow=26740, EarfcnMid=26865, EarfcnHigh=26990, RefStart=-110.0},
                new(){Band=28, Mode=DuplexMode.FDD, CmwCode="OB28", DlRb=50, UlRb=25, EarfcnLow=27260, EarfcnMid=27435, EarfcnHigh=27610, RefStart=-110.0},
                new(){Band=34, Mode=DuplexMode.TDD, CmwCode="OB34", DlRb=50, UlRb=50, EarfcnLow=36250, EarfcnMid=36275, EarfcnHigh=36300, RefStart=-110.0},
                new(){Band=38, Mode=DuplexMode.TDD, CmwCode="OB38", DlRb=50, UlRb=50, EarfcnLow=37800, EarfcnMid=38000, EarfcnHigh=38200, RefStart=-110.0},
                new(){Band=39, Mode=DuplexMode.TDD, CmwCode="OB39", DlRb=50, UlRb=50, EarfcnLow=38300, EarfcnMid=38450, EarfcnHigh=38600, RefStart=-110.0},
                new(){Band=40, Mode=DuplexMode.TDD, CmwCode="OB40", DlRb=50, UlRb=50, EarfcnLow=38700, EarfcnMid=39150, EarfcnHigh=39600, RefStart=-110.0},
                new(){Band=41, Mode=DuplexMode.TDD, CmwCode="OB41", DlRb=50, UlRb=50, EarfcnLow=39700, EarfcnMid=40620, EarfcnHigh=41540, RefStart=-110.0},
                new(){Band=66, Mode=DuplexMode.FDD, CmwCode="OB66", DlRb=50, UlRb=50, EarfcnLow=132022,EarfcnMid=132322,EarfcnHigh=132622,RefStart=-110.0},
            };

            // Seed per-channel start levels from the band-level RefStart; the RefSens
            // import command will overwrite these per (Band, Channel) when a template is loaded.
            foreach (var band in bands)
            {
                band.RefStartByChannel["Low"]  = band.RefStart;
                band.RefStartByChannel["Mid"]  = band.RefStart;
                band.RefStartByChannel["High"] = band.RefStart;
            }

            return bands;
        }

        private readonly ICmwService _cmwService;
        private readonly ITestService _testService;
        private readonly ILogService _logService;

        private CancellationTokenSource? _cts;

        // ═══════════════════════════════════════════════════════════
        //  Properties
        // ═══════════════════════════════════════════════════════════

        private TestConfig _config;
        public TestConfig Config
        {
            get => _config;
            set => SetProperty(ref _config, value);
        }

        private ObservableCollection<RefSensRow> _results;
        public ObservableCollection<RefSensRow> Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        private string _logText = "";
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    OnPropertyChanged(nameof(CanStart));
                    OnPropertyChanged(nameof(CanStop));
                }
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    OnPropertyChanged(nameof(ConnectionStatus));
                    OnPropertyChanged(nameof(ConnectionColor));
                    OnPropertyChanged(nameof(CanStart));
                }
            }
        }

        public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";

        private static readonly Brush ConnectedBrush = new SolidColorBrush(Color.FromRgb(0x4E, 0xC9, 0x4E));
        private static readonly Brush DisconnectedBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x47, 0x47));

        public Brush ConnectionColor => IsConnected ? ConnectedBrush : DisconnectedBrush;

        private int _currentProgress;
        public int CurrentProgress
        {
            get => _currentProgress;
            set
            {
                if (SetProperty(ref _currentProgress, value))
                {
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }

        private int _totalProgress;
        public int TotalProgress
        {
            get => _totalProgress;
            set
            {
                if (SetProperty(ref _totalProgress, value))
                {
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }

        public string ProgressText => $"{CurrentProgress}/{TotalProgress}";

        // REFSENS import status
        private string _refSensImportStatus = "设置各信道电平初始值";
        public string RefSensImportStatus
        {
            get => _refSensImportStatus;
            set => SetProperty(ref _refSensImportStatus, value);
        }

        private Brush _refSensImportStatusColor = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)); // Gray
        public Brush RefSensImportStatusColor
        {
            get => _refSensImportStatusColor;
            set => SetProperty(ref _refSensImportStatusColor, value);
        }

        // Loss table enable/disable
        private bool _isLossTableEnabled;
        public bool IsLossTableEnabled
        {
            get => _isLossTableEnabled;
            set
            {
                if (SetProperty(ref _isLossTableEnabled, value))
                {
                    if (!value)
                    {
                        LossTableStatus = "不设置线损表";
                        LossTableStatusColor = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                    }
                    else
                    {
                        LossTableStatus = "请导入线损表";
                        LossTableStatusColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00));
                    }
                }
            }
        }

        private string _lossTableStatus = "不设置线损表";
        public string LossTableStatus
        {
            get => _lossTableStatus;
            set => SetProperty(ref _lossTableStatus, value);
        }

        private Brush _lossTableStatusColor = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
        public Brush LossTableStatusColor
        {
            get => _lossTableStatusColor;
            set => SetProperty(ref _lossTableStatusColor, value);
        }

        // Band selection
        private ObservableCollection<BandSelectionItem> _bandSelections;
        public ObservableCollection<BandSelectionItem> BandSelections
        {
            get => _bandSelections;
            set => SetProperty(ref _bandSelections, value);
        }

        // CMW connection parameters
        private string _cmwIpAddress = "192.0.2.10";
        public string CmwIpAddress
        {
            get => _cmwIpAddress;
            set => SetProperty(ref _cmwIpAddress, value);
        }

        private int _cmwPort = 5025;
        public int CmwPort
        {
            get => _cmwPort;
            set => SetProperty(ref _cmwPort, value);
        }

        private bool _isScpiLogEnabled;
        public bool IsScpiLogEnabled
        {
            get => _isScpiLogEnabled;
            set
            {
                if (SetProperty(ref _isScpiLogEnabled, value))
                    _cmwService.ScpiLoggingEnabled = value;
            }
        }

        // TPC 相关设置已取消 — CMW500 面板 TPC 保持用户配置，程序不再改写。

        public bool UePowerEnabled
        {
            get => Config.UePowerEnabled;
            set
            {
                if (Config.UePowerEnabled != value)
                {
                    Config.UePowerEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  UE COM 口选择：Auto = 自动枚举 USB CDC；COMx = 强制此口。
        // ═══════════════════════════════════════════════════════════
        private const string AutoComPortLabel = "自动 (Auto)";
        public ObservableCollection<string> ComPortOptions { get; } = BuildComPortOptions();
        private static ObservableCollection<string> BuildComPortOptions()
        {
            var list = new ObservableCollection<string> { AutoComPortLabel };
            for (int i = 1; i <= 50; i++) list.Add($"COM{i}");
            return list;
        }

        private string _selectedComPort = AutoComPortLabel;
        public string SelectedComPort
        {
            get => _selectedComPort;
            set
            {
                if (SetProperty(ref _selectedComPort, value))
                {
                    Config.ForcedUeComPort = value == AutoComPortLabel ? null : value;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Commands
        // ═══════════════════════════════════════════════════════════

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartTestCommand { get; }
        public ICommand StartMidOnlyCommand { get; }
        public ICommand StopTestCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand SelectAllBandsCommand { get; }
        public ICommand SelectNoneBandsCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ExportTemplateCommand { get; }
        public ICommand ImportTemplateCommand { get; }
        public ICommand ExportLogCommand { get; }
        public ICommand ExportLossTemplateCommand { get; }
        public ICommand ImportLossTableCommand { get; }

        public bool CanStart => IsConnected && !IsRunning;
        public bool CanStop => IsRunning;

        // ═══════════════════════════════════════════════════════════
        //  Constructor
        // ═══════════════════════════════════════════════════════════

        public MainViewModel(
            ICmwService cmwService,
            ITestService testService,
            ILogService logService)
        {
            _cmwService = cmwService ?? throw new ArgumentNullException(nameof(cmwService));
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Initialize collections
            _config = new TestConfig
            {
                CmwAddress = "192.0.2.10",
                StartLevel = -70.0,
                MinLevel = -140.0,
                BlerThreshold = 0.05,
                SkipInit = true
            };

            _results = new ObservableCollection<RefSensRow>();

            // Initialize band selections
            _bandSelections = new ObservableCollection<BandSelectionItem>(
                AllBands.Select(b => new BandSelectionItem
                {
                    Band = b,
                    IsSelected = true,
                    DisplayName = $"B{b.Band}",
                    ToolTip = $"B{b.Band} ({b.Mode}) DL={b.DlRb} UL={b.UlRb}RB"
                }));

            // Initialize commands
            ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => !IsConnected);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
            StartTestCommand = new AsyncRelayCommand(() => StartTestAsync(false), () => CanStart);
            StartMidOnlyCommand = new AsyncRelayCommand(() => StartTestAsync(true), () => CanStart);
            StopTestCommand = new RelayCommand(StopTest, () => CanStop);
            PauseCommand = new RelayCommand(Pause, () => IsRunning && !IsPaused);
            ResumeCommand = new RelayCommand(Resume, () => IsRunning && IsPaused);
            ClearLogCommand = new RelayCommand(ClearLog);
            SelectAllBandsCommand = new RelayCommand(SelectAllBands);
            SelectNoneBandsCommand = new RelayCommand(SelectNoneBands);
            ExportResultsCommand = new RelayCommand(ExportResults, () => Results.Count > 0);
            ExportTemplateCommand = new RelayCommand(ExportTemplate);
            ImportTemplateCommand = new RelayCommand(ImportTemplate);
            ExportLogCommand = new RelayCommand(ExportLog, () => _fullLog.Count > 0);
            ExportLossTemplateCommand = new RelayCommand(ExportLossTemplate);
            ImportLossTableCommand = new AsyncRelayCommand(ImportLossTableAsync, () => IsConnected);

            // Subscribe to log events
            _logService.LogMessageReceived += OnLogMessageReceived;
        }

        // ═══════════════════════════════════════════════════════════
        //  Command Implementations
        // ═══════════════════════════════════════════════════════════

        private async Task ConnectAsync()
        {
            if (BuildInfo.IsExpired) return;

            try
            {
                StatusText = "Connecting...";
                await _cmwService.ConnectAsync(CmwIpAddress, CmwPort);

                var idn = await _cmwService.QueryAsync("*IDN?");
                _logService.Log($"Connected → {idn}");

                // 一建立连接立刻下发 MSUBframes 0,10,2 并保留状态（后续流程不再修改）
                try
                {
                    await _cmwService.SendAsync("CONFigure:LTE:MEAS:MEValuation:MSUBframes 0,10,2");
                    _logService.Log("Global init: CONFigure:LTE:MEAS:MEValuation:MSUBframes 0,10,2");
                }
                catch (Exception ex)
                {
                    _logService.Log($"[WARN] Set MSUBframes 0,10,2 failed: {ex.Message}");
                }

                // 连接后立即锁定 MEV 触发源为 FrameTrigger（后续流程禁止修改 FRAME 相关设置）
                try
                {
                    await _cmwService.SendAsync("TRIG:LTE:MEAS:MEV:SOUR \"LTE Sig1: FrameTrigger\"");
                    _logService.Log("Global init: TRIG:LTE:MEAS:MEV:SOUR \"LTE Sig1: FrameTrigger\"");
                }
                catch (Exception ex)
                {
                    _logService.Log($"[WARN] Set MEV trigger source failed: {ex.Message}");
                }

                // 启用 UE 测量上报（RSRP/RSRQ/等），供 MeasureRsrpAsync 使用
                try
                {
                    await _cmwService.SendAsync("CONFigure:LTE:SIGN1:UEReport:ENABle ON");
                    _logService.Log("Global init: CONFigure:LTE:SIGN1:UEReport:ENABle ON");
                }
                catch (Exception ex)
                {
                    _logService.Log($"[WARN] Enable UeReport failed: {ex.Message}");
                }

                // 探底时电平频繁变化 → 640ms 上报周期，让 RSRP 尽快跟上
                try
                {
                    await _cmwService.SendAsync("CONFigure:LTE:SIGN1:UEReport:RINTerval I640");
                    _logService.Log("Global init: CONFigure:LTE:SIGN1:UEReport:RINTerval I640");
                }
                catch (Exception ex)
                {
                    _logService.Log($"[WARN] Set UeReport RINTerval failed: {ex.Message}");
                }

                IsConnected = true;
                StatusText = "Connected";
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Connection failed: {ex.Message}");
                IsConnected = false;
                StatusText = "Connection failed";
            }
        }

        private void Disconnect()
        {
            _cts?.Cancel();
            _cmwService.Disconnect();
            IsConnected = false;
            IsRunning = false;
            StatusText = "Disconnected";
            _logService.Log("Disconnected from CMW500");
        }

        private async Task StartTestAsync(bool midOnly)
        {
            if (!IsConnected) return;

            _cts = new CancellationTokenSource();
            Results.Clear();
            IsRunning = true;
            IsPaused = false;
            CurrentProgress = 0;
            TotalProgress = 0;

            // 每次运行强制开启 SCPI 记录并清空旧缓冲，保证 TESTDATA 下 SCPI.log 总有内容
            _cmwService.ClearScpiLog();
            _cmwService.ScpiLoggingEnabled = true;

            try
            {
                StatusText = "Running test...";

                // Build channel list from selected bands
                var selectedBands = BandSelections.Where(b => b.IsSelected).Select(b => b.Band).ToList();
                var channels = selectedBands.SelectMany(b =>
                {
                    if (midOnly)
                        return new[] { new ChannelPoint(b, "Mid", b.EarfcnMid) };
                    else
                        return new[]
                        {
                            new ChannelPoint(b, "Low", b.EarfcnLow),
                            new ChannelPoint(b, "Mid", b.EarfcnMid),
                            new ChannelPoint(b, "High", b.EarfcnHigh)
                        };
                }).ToList();

                // Sort: FDD first, then TDD
                channels = channels
                    .OrderBy(ch => ch.Band.Mode == DuplexMode.TDD ? 1 : 0)
                    .ThenBy(ch => ch.Band.Band)
                    .ToList();

                TotalProgress = channels.Count;

                // Create empty loss tables (loss functionality removed)
                var uplinkLoss = new LossTable { Name = "Uplink" };
                var downlinkLoss = new LossTable { Name = "Downlink" };

                // Progress reporter — adds result rows in real-time
                var progress = new Progress<TestProgress>(p =>
                {
                    if (p.CurrentChannel > 0)
                        CurrentProgress = p.CurrentChannel;
                    if (!string.IsNullOrEmpty(p.Status))
                        StatusText = p.Status;
                    if (!string.IsNullOrEmpty(p.LogMessage))
                        _logService.Log(p.LogMessage);
                    if (p.ResultRow != null)
                        Results.Add(p.ResultRow);
                });

                // Initialize CMW if needed
                if (!Config.SkipInit)
                {
                    _logService.Log("Initializing CMW500...");
                    await _testService.InitializeCmwAsync(Config, _cts.Token);
                }

                // Run test
                var result = await _testService.RunRefSensTestAsync(
                    channels, Config, uplinkLoss, downlinkLoss, progress, _cts.Token);

                StatusText = result.Success ? "Test completed" : "Test failed";
                _logService.Log($"=== {StatusText} ===");
            }
            catch (OperationCanceledException)
            {
                StatusText = "Test cancelled";
                _logService.Log("Test cancelled by user");
            }
            catch (Exception ex)
            {
                StatusText = "Test error";
                _logService.Log($"[ERROR] {ex.Message}");
            }
            finally
            {
                IsRunning = false;
                IsPaused = false;
                AutoSaveTestArtifacts();
            }
        }

        private void StopTest()
        {
            _cts?.Cancel();
            StatusText = "Stopping...";
        }

        private void Pause()
        {
            if (_testService is TestService ts)
            {
                ts.Pause();
                IsPaused = true;
                StatusText = "Paused";
                _logService.Log("Test paused");
            }
        }

        private void Resume()
        {
            if (_testService is TestService ts)
            {
                ts.Resume();
                IsPaused = false;
                StatusText = "Running...";
                _logService.Log("Test resumed");
            }
        }

        private void ClearLog()
        {
            _logBuffer.Clear();
            _fullLog.Clear();
            LogText = "";
        }

        private void SelectAllBands()
        {
            foreach (var item in BandSelections)
                item.IsSelected = true;
        }

        private void SelectNoneBands()
        {
            foreach (var item in BandSelections)
                item.IsSelected = false;
        }

        private void WriteResultsExcel(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RefSens Report");

            var headers = new[] { "Band", "Mode", "Channel", "EARFCN", "DL RB", "UL RB",
                "EPRE_音乐前", "EPRE_音乐中", "UEPwr_音乐前(dBm)", "UEPwr_音乐中(dBm)",
                "RSRP_音乐前(dBm)", "RSRP_音乐中(dBm)",
                "RSRP_音乐前_Idx", "RSRP_音乐中_Idx",
                "音乐差值(dB)", "Probes", "Result", "Note" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(1, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#007ACC");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            for (int i = 0; i < Results.Count; i++)
            {
                var r = Results[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = r.BandDisplay;
                ws.Cell(row, 2).Value = r.DuplexMode;
                ws.Cell(row, 3).Value = r.Channel;
                ws.Cell(row, 4).Value = r.Earfcn;
                ws.Cell(row, 5).Value = r.DlRb;
                ws.Cell(row, 6).Value = r.UlRb;

                if (!double.IsNaN(r.RefSensBefore)) ws.Cell(row, 7).Value = r.RefSensBefore;
                if (!double.IsNaN(r.RefSensDuring)) ws.Cell(row, 8).Value = r.RefSensDuring;
                if (!double.IsNaN(r.UePowerBefore)) ws.Cell(row, 9).Value = r.UePowerBefore;
                if (!double.IsNaN(r.UePowerDuring)) ws.Cell(row, 10).Value = r.UePowerDuring;
                if (!double.IsNaN(r.RsrpBefore)) ws.Cell(row, 11).Value = r.RsrpBefore;
                if (!double.IsNaN(r.RsrpDuring)) ws.Cell(row, 12).Value = r.RsrpDuring;
                if (r.RsrpBeforeIdx >= 0) ws.Cell(row, 13).Value = r.RsrpBeforeIdx;
                if (r.RsrpDuringIdx >= 0) ws.Cell(row, 14).Value = r.RsrpDuringIdx;
                if (!double.IsNaN(r.RefSensDiff)) ws.Cell(row, 15).Value = r.RefSensDiff;

                ws.Cell(row, 16).Value = r.SearchCount;
                ws.Cell(row, 17).Value = r.ResultText;
                ws.Cell(row, 18).Value = r.Note ?? "";

                var resultCell = ws.Cell(row, 17);
                resultCell.Style.Font.Bold = true;
                switch (r.ResultText)
                {
                    case "PASS":  resultCell.Style.Font.FontColor = XLColor.Green; break;
                    case "FAIL":  resultCell.Style.Font.FontColor = XLColor.Red;   break;
                    case "ERROR": resultCell.Style.Font.FontColor = XLColor.Red;   break;
                    case "SKIP":  resultCell.Style.Font.FontColor = XLColor.Gray;  break;
                }
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(filePath);
        }

        /// <summary>
        /// Auto-save Excel report + SCPI log + Run log on test completion.
        /// 每次运行独立子文件夹：D:\SenSearchReport\Run_YYYYMMDD_HHmmss\
        ///   ├── RefSens.xlsx  — 灵敏度结果表
        ///   ├── SCPI.log      — CMW500 底层 SCPI 通讯记录
        ///   └── RunLog.log    — 应用运行日志（等同"导出日志"按钮的内容）
        /// </summary>
        private const string AutoSaveRoot = @"D:\SenSearchReport";

        private void AutoSaveTestArtifacts()
        {
            if (Results.Count == 0) return;
            try
            {
                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string runDir = Path.Combine(AutoSaveRoot, $"Run_{ts}");
                Directory.CreateDirectory(runDir);

                // 1. Excel 报告
                string xlsxPath = Path.Combine(runDir, "RefSens.xlsx");
                WriteResultsExcel(xlsxPath);
                _logService.Log($"✓ Auto-saved report: {xlsxPath}");

                // 2. SCPI 通讯日志
                var scpi = _cmwService.ScpiLog ?? (IReadOnlyList<string>)Array.Empty<string>();
                string scpiPath = Path.Combine(runDir, "SCPI.log");
                File.WriteAllLines(scpiPath, scpi, System.Text.Encoding.UTF8);
                _logService.Log($"✓ Auto-saved SCPI log ({scpi.Count} lines): {scpiPath}");

                // 3. 应用运行日志（快照写盘，避免运行中的日志追加导致文件抖动）
                var runLog = _fullLog.ToArray();
                string runLogPath = Path.Combine(runDir, "RunLog.log");
                File.WriteAllLines(runLogPath, runLog, System.Text.Encoding.UTF8);
                _logService.Log($"✓ Auto-saved run log ({runLog.Length} lines): {runLogPath}");
            }
            catch (Exception ex)
            {
                _logService.Log($"[WARN] Auto-save failed: {ex.Message}");
            }
        }

        private void ExportResults()
        {
            try
            {
                if (Results.Count == 0)
                {
                    _logService.Log("No results to export");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"RefSens_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (dialog.ShowDialog() != true)
                    return;

                WriteResultsExcel(dialog.FileName);
                _logService.Log($"Test report exported: {Results.Count} rows → {dialog.FileName}");
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Export results failed: {ex.Message}");
            }
        }

        private void ExportLog()
        {
            try
            {
                if (_fullLog.Count == 0)
                {
                    _logService.Log("Run log is empty");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"RunLog_{DateTime.Now:yyyyMMdd_HHmmss}.log",
                    DefaultExt = ".log"
                };

                if (dialog.ShowDialog() != true)
                    return;

                // Snapshot to avoid mutation during write (log events fire from worker threads)
                var snapshot = _fullLog.ToArray();
                File.WriteAllLines(dialog.FileName, snapshot, System.Text.Encoding.UTF8);
                _logService.Log($"Run log exported: {snapshot.Length} lines → {dialog.FileName}");
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Run log export failed: {ex.Message}");
            }
        }

        private const string RefSensTemplateDir = @"D:\RFtest\RFtestCabloss";
        private const string RefSensTemplateFile = "初始电平模板.xlsx";

        private void ExportTemplate()
        {
            try
            {
                Directory.CreateDirectory(RefSensTemplateDir);

                var dialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    InitialDirectory = RefSensTemplateDir,
                    FileName = RefSensTemplateFile,
                    DefaultExt = ".xlsx"
                };

                if (dialog.ShowDialog() != true)
                    return;

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("RefSens Template");

                var headers = new[] { "Band", "Mode", "Channel", "EARFCN", "RefSens (dBm)" };
                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cell(1, c + 1);
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#007ACC");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                foreach (var band in AllBands)
                {
                    void WriteRow(string ch, int earfcn, double refSens)
                    {
                        ws.Cell(row, 1).Value = $"B{band.Band}";
                        ws.Cell(row, 2).Value = band.Mode.ToString();
                        ws.Cell(row, 3).Value = ch;
                        ws.Cell(row, 4).Value = earfcn;
                        ws.Cell(row, 5).Value = refSens;
                        ws.Cell(row, 5).Style.NumberFormat.Format = "0.0";
                        row++;
                    }
                    WriteRow("Low", band.EarfcnLow, band.GetRefStart("Low"));
                    WriteRow("Mid", band.EarfcnMid, band.GetRefStart("Mid"));
                    WriteRow("High", band.EarfcnHigh, band.GetRefStart("High"));
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(dialog.FileName);

                _logService.Log($"Template exported to: {dialog.FileName}");
                _logService.Log($"Total: {AllBands.Count} bands × 3 channels = {AllBands.Count * 3} rows");
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Export template failed: {ex.Message}");
            }
        }

        private void ImportTemplate()
        {
            try
            {
                Directory.CreateDirectory(RefSensTemplateDir);

                var dialog = new OpenFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    InitialDirectory = RefSensTemplateDir,
                    FileName = RefSensTemplateFile,
                    DefaultExt = ".xlsx"
                };

                if (dialog.ShowDialog() != true)
                    return;

                int updatedCount = 0;
                var channelUpdates = new Dictionary<string, double>(); // Key: "B1-Low", "B1-Mid", etc.

                using var wb = new XLWorkbook(dialog.FileName);
                var ws = wb.Worksheets.First();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                for (int r = 2; r <= lastRow; r++)
                {
                    var bandStr = ws.Cell(r, 1).GetString().Trim().TrimStart('B', 'b');
                    if (!int.TryParse(bandStr, out int bandNum))
                        continue;

                    var channel = ws.Cell(r, 3).GetString().Trim();
                    if (string.IsNullOrEmpty(channel))
                        continue;

                    var refSensStr = ws.Cell(r, 5).GetString().Trim();
                    if (!double.TryParse(refSensStr, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double refSens))
                    {
                        if (!ws.Cell(r, 5).TryGetValue(out refSens))
                            continue;
                    }

                    var band = AllBands.FirstOrDefault(b => b.Band == bandNum);
                    if (band != null)
                    {
                        band.SetRefStart(channel, refSens);
                        channelUpdates[$"B{bandNum}-{channel}"] = refSens;
                        updatedCount++;
                    }
                }

                _logService.Log($"=== REFSENS Template Import ===");
                _logService.Log($"File: {System.IO.Path.GetFileName(dialog.FileName)}");
                _logService.Log($"Updated {channelUpdates.Count} channel start levels");

                // Show updated channels grouped by band
                if (channelUpdates.Count > 0)
                {
                    _logService.Log("Updated channel start levels:");

                    // Group by band number
                    var grouped = channelUpdates
                        .Select(kvp => new {
                            Key = kvp.Key,
                            Value = kvp.Value,
                            Band = int.Parse(kvp.Key.Split('-')[0].TrimStart('B')),
                            Channel = kvp.Key.Split('-')[1]
                        })
                        .GroupBy(x => x.Band)
                        .OrderBy(g => g.Key);

                    foreach (var group in grouped)
                    {
                        var items = group.OrderBy(x => x.Channel == "Low" ? 0 : x.Channel == "Mid" ? 1 : 2);
                        foreach (var item in items)
                        {
                            _logService.Log($"  B{item.Band} {item.Channel}: {item.Value:F1} dBm");
                        }
                    }
                    _logService.Log($"✓ These values will be used as starting levels in next test");

                    // Update import status - success
                    RefSensImportStatus = "各信道初始电平值设置完成";
                    RefSensImportStatusColor = new SolidColorBrush(Color.FromRgb(0x4E, 0xC9, 0x4E)); // Green
                }
                else
                {
                    _logService.Log("[WARNING] No valid REFSENS values found in file");

                    // Keep default status if import failed
                    RefSensImportStatus = "设置各信道电平初始值";
                    RefSensImportStatusColor = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)); // Gray
                }

                _logService.Log("Note: Each channel (Low/Mid/High) now has its own RefStart value");
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Import template failed: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Loss Table — Export Template / Import & Upload to CMW500
        // ═══════════════════════════════════════════════════════════

        private void ExportLossTemplate()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "LossTable_Template.csv",
                    DefaultExt = ".csv"
                };
                if (dialog.ShowDialog() != true) return;

                // Layout matches D:\RFtest\RFtestCabloss\初始线损模板.csv (BOM + trailing-comma header + ",," separator)
                using var writer = new StreamWriter(dialog.FileName, false, new System.Text.UTF8Encoding(true));
                writer.WriteLine("TableName,Cable_Loss,");
                writer.WriteLine(",,");
                writer.WriteLine("Freq(MHz),Input(dB),Output(dB)");
                writer.WriteLine("100,0.5,0.5");
                writer.WriteLine("700,0.5,0.5");
                writer.WriteLine("900,0.5,0.5");
                writer.WriteLine("1000,0.8,0.8");
                writer.WriteLine("1500,0.8,0.8");
                writer.WriteLine("1800,0.8,0.8");
                writer.WriteLine("2100,1.2,1.2");
                writer.WriteLine("2600,1.2,1.2");
                writer.WriteLine("3500,1.2,1.2");
                writer.WriteLine("6000,1.2,1.2");

                _logService.Log($"Loss table template exported: {dialog.FileName}");
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Export loss template failed: {ex.Message}");
            }
        }

        private async Task ImportLossTableAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv"
                };
                if (dialog.ShowDialog() != true) return;

                var lines = File.ReadAllLines(dialog.FileName, System.Text.Encoding.UTF8);
                if (lines.Length < 3)
                {
                    _logService.Log("[ERROR] Loss table CSV too short");
                    return;
                }

                string tableName = "Cable_Loss";
                var freqLossPairs = new List<(double freqHz, double lossDb)>();
                bool inDataSection = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    if (trimmed.StartsWith("TableName,", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split(',');
                        if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                            tableName = parts[1].Trim();
                        continue;
                    }

                    if (trimmed.StartsWith("Freq", StringComparison.OrdinalIgnoreCase))
                    {
                        inDataSection = true;
                        continue;
                    }

                    if (!inDataSection) continue;

                    var cols = trimmed.Split(',');
                    if (cols.Length >= 2 &&
                        double.TryParse(cols[0].Trim(), out double freqMHz) &&
                        double.TryParse(cols[1].Trim(), out double lossDb))
                    {
                        freqLossPairs.Add((freqMHz * 1_000_000, lossDb));
                    }
                }

                if (freqLossPairs.Count < 2)
                {
                    _logService.Log("[ERROR] Need at least 2 frequency-loss pairs");
                    return;
                }

                freqLossPairs.Sort((a, b) => a.freqHz.CompareTo(b.freqHz));

                _logService.Log($"=== Upload Loss Table to CMW500 ===");
                _logService.Log($"Table: \"{tableName}\"  ({freqLossPairs.Count} points)");
                foreach (var (f, l) in freqLossPairs)
                    _logService.Log($"  {f / 1e6:F1} MHz → {l:F2} dB");

                var ct = CancellationToken.None;

                await _cmwService.SendAsync("CONF:BASE:FDC:RCL", ct);

                var exists = await _cmwService.QueryAsync(
                    $"CONFigure:BASE:FDCorrection:CTABle:EXISt? \"{tableName}\"", 5000, ct);
                if (exists.Trim() == "1")
                {
                    _logService.Log($"  Table \"{tableName}\" exists — deleting...");
                    await _cmwService.SendAsync(
                        $"CONFigure:BASE:FDCorrection:CTABle:DELete \"{tableName}\"", ct);
                }

                var sb = new System.Text.StringBuilder();
                sb.Append($"CONFigure:BASE:FDCorrection:CTABle:CREate \"{tableName}\"");
                foreach (var (f, l) in freqLossPairs)
                    sb.Append($", {f:F0}, {l:F2}");

                await _cmwService.SendAsync(sb.ToString(), ct);
                _logService.Log("  Table created");

                await _cmwService.SendAsync("CONF:BASE:FDCorrection:SAV", ct);
                _logService.Log("  Tables saved to CMW500 persistent memory");

                await _cmwService.SendAsync("CONFigure:FDCorrection:DEACtivate RF1C", ct);
                await _cmwService.SendAsync(
                    $"CONFigure:FDCorrection:ACTivate RF1C, '{tableName}', RX", ct);
                await _cmwService.SendAsync(
                    $"CONFigure:FDCorrection:ACTivate RF1C, '{tableName}', TX", ct);
                _logService.Log("  Activated on RF1C (RX + TX)");

                var usage = await _cmwService.QueryAsync(
                    "CONFigure:FDCorrection:USAGe? RF1C", 5000, ct);
                _logService.Log($"  RF1C usage: {usage}");

                try { await _cmwService.QueryAsync("SYST:ERR:ALL?", 5000, ct); } catch { }

                _logService.Log("=== Loss table upload complete ===");

                LossTableStatus = "线损表设置成功";
                LossTableStatusColor = new SolidColorBrush(Color.FromRgb(0x4E, 0xC9, 0x4E));
            }
            catch (Exception ex)
            {
                _logService.Log($"[ERROR] Import loss table failed: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Event Handlers
        // ═══════════════════════════════════════════════════════════

        // Rolling 20-line buffer for the log display panel (reduces TextBox lag on long runs)
        private const int LogBufferLines = 20;
        private readonly Queue<string> _logBuffer = new();

        // Full runtime log — accumulated for export ("导出 LOG")
        private readonly List<string> _fullLog = new();

        private void OnLogMessageReceived(object? sender, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logBuffer.Clear();
                _fullLog.Clear();
                LogText = "";
                return;
            }

            _fullLog.Add(message);

            _logBuffer.Enqueue(message);
            while (_logBuffer.Count > LogBufferLines)
                _logBuffer.Dequeue();

            LogText = string.Join(Environment.NewLine, _logBuffer);
        }
    }

    /// <summary>
    /// Band selection item for UI binding
    /// </summary>
    public class BandSelectionItem : ViewModelBase
    {
        public BandConfig Band { get; set; } = null!;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string DisplayName { get; set; } = "";
        public string ToolTip { get; set; } = "";
    }
}
