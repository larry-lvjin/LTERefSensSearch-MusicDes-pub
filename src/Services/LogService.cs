using System;

namespace LteRefSensTester.Services
{
    /// <summary>
    /// Simple logging service implementation
    /// </summary>
    public class LogService : ILogService
    {
        public event EventHandler<string>? LogMessageReceived;

        public void Log(string message)
        {
            var timestamped = $"[{DateTime.Now:HH:mm:ss.fff}]  {message}";
            LogMessageReceived?.Invoke(this, timestamped);
        }

        public void Clear()
        {
            // Clear event - UI layer responsible for clearing display
            LogMessageReceived?.Invoke(this, string.Empty);
        }
    }
}
