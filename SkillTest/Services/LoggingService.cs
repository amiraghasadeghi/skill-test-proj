using Mma.Common.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.Services {
    public class LoggingService : ILoggingService {
        private readonly ILogger _logger;
        public LoggingService(ILogger logger) {
            _logger = logger;
        }

        public void LogError(string functionName, string errorMessage) {
            string timestamp = DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:ss");
            _logger.Error($"{functionName} - {errorMessage} [{timestamp} utc]");
        }
    }
}
