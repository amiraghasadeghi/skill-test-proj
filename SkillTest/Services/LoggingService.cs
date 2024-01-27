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
        /// Logs an error with a timestamp, function name, and error message.
        /// </summary>
        /// <param name="functionName">The name of the function where the error occurred.</param>
        /// <param name="errorMessage">The error message to be logged.</param>
        /// <remarks>
        /// This method logs an error in a standardised format, including a timestamp, the name of the function, and the error message.
        /// In a startup class, this logging functionality can be configured to output logs to a specific location. 
        /// This can be a file for local or VM-based applications or integrated with Azure Application Insights for cloud-based services.
        /// When configured with Azure Application Insights, it enables enhanced logging capabilities such as real-time monitoring and analytics.
        /// </remarks>
        public void LogError(string functionName, string errorMessage) {
            string timestamp = DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:ss");
            _logger.Error($"{functionName} - {errorMessage} [{timestamp} utc]");
        }

    }
}
