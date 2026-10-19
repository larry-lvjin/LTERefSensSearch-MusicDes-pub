using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using LteRefSensTester.Services;
using LteRefSensTester.ViewModels;

namespace LteRefSensTester
{
    /// <summary>
    /// Application entry point with dependency injection
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        // 崩溃日志固定路径：写入 %LOCALAPPDATA%\LteRefSensTester\crash.log
        // 三层兜底（UI Dispatcher / AppDomain / TaskScheduler UnobservedTask）触发时都追加到这里，
        // 目标：即使 CMW500 / UE 侧长时间不响应或串口异常，主进程也不因未捕获异常而闪退。
        private static readonly string CrashLogPath = BuildCrashLogPath();

        private static string BuildCrashLogPath()
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LteRefSensTester");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "crash.log");
            }
            catch
            {
                // Fallback：当前目录（一定可写的兜底）
                return Path.Combine(AppContext.BaseDirectory, "crash.log");
            }
        }

        private static void AppendCrashLog(string source, Exception? ex, bool handled)
        {
            try
            {
                var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var body = ex?.ToString() ?? "(no exception object)";
                File.AppendAllText(CrashLogPath,
                    $"[{ts}] {source} handled={handled}\n{body}\n\n");
            }
            catch { /* 日志失败也不能让兜底自身炸 */ }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // ── 全局异常兜底：三层（UI / AppDomain / Task）──
            // 目的：CMW500 长时间断连 / UE 串口异常 / RRC 无法恢复 → 保证进程不闪退，
            // 用户能读日志并手动重启测试。
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppendCrashLog("Dispatcher", e.Exception, handled: true);
            // Handled=true 阻止 WPF 结束进程；用户能继续操作
            e.Handled = true;
            try
            {
                MessageBox.Show(
                    $"程序捕获到一处未预期异常，已记录到日志：\n{CrashLogPath}\n\n消息：{e.Exception.Message}",
                    "程序保护性兜底",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch { }
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // AppDomain 层已无救 —— IsTerminating 时进程即将退出，只能落盘
            AppendCrashLog("AppDomain", e.ExceptionObject as Exception, handled: !e.IsTerminating);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            AppendCrashLog("UnobservedTask", e.Exception, handled: true);
            // .NET 4.5+ 默认 UnobservedTaskException 不会终止进程，但显式 SetObserved 更保险
            e.SetObserved();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register Services as Singletons
            services.AddSingleton<ICmwService, CmwService>();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ILogService, LogService>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();

            // Register Views
            services.AddSingleton<MainWindow>(sp =>
            {
                var window = new MainWindow();
                var viewModel = sp.GetRequiredService<MainViewModel>();
                window.DataContext = viewModel;
                return window;
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup resources
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
