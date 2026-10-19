using System;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// Interface for logging service
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Log a message
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Clear all logs
        /// </summary>
        void Clear();

        /// <summary>
        /// Event raised when a log message is received
        /// </summary>
        event EventHandler<string> LogMessageReceived;
    }
}
