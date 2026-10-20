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
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }

        public void Clear()
        {
            // 实现细节未公开（算法核心）
            throw new NotImplementedException("该方法的实现不在开源范围内");
        }
    }
}
