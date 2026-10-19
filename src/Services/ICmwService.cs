using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// Interface for CMW500 communication service
    /// </summary>
    public interface ICmwService : IDisposable
    {
        Task ConnectAsync(string ip, int port, CancellationToken ct = default);
        void Disconnect();
        bool Connected { get; }
        Task SendAsync(string command, CancellationToken ct = default);
        Task<string> QueryAsync(string command, int timeoutMs = 10000, CancellationToken ct = default);
        Task SendOpcAsync(string command, int timeoutMs = 10000, CancellationToken ct = default);
        Task WaitCellStatAsync(string expected, int timeoutMs, CancellationToken ct);

        bool ScpiLoggingEnabled { get; set; }
        IReadOnlyList<string> ScpiLog { get; }
        void ClearScpiLog();
    }
}
