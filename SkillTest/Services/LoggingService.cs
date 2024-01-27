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

        /// <summary>
        /// Logs an error message along with exception details.
        /// </summary>
        /// <param name="functionName">The name of the function where the error occurred.</param>
        /// <param name="errorMessage">The error message to log.</param>
        /// <param name="exception">The exception object associated with the error, if available.</param>
        public void LogError(string functionName, string errorMessage, Exception exception = null) {
            string timestamp = DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:ss");
            string logMessage = $"{functionName} - {errorMessage} [{timestamp} utc]";

            if (exception != null) {
                logMessage += $"\nException: {exception}\n";

                // Include inner exception details if present
                if (exception.InnerException != null) {
                    logMessage += $"Inner Exception: {exception.InnerException}\n";
                }
            }

            _logger.Error(logMessage);
        }

    }
}
