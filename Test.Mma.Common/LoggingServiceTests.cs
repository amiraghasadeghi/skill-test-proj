using global::Mma.Common.Services;
using NUnit.Framework;
using Moq;
using NLog;

namespace Test.Mma.Common {
    [TestFixture]
    public class Logging_service_tests {
        private Mock<ILogger> _mockLogger;
        private LoggingService _loggingService;

        [SetUp]
        public void SetUp() {
            _mockLogger = new Mock<ILogger>();
            _loggingService = new LoggingService(_mockLogger.Object);
        }


        [Test]
        public void LogError_WhenCalled_ShouldLogErrorWithCorrectFormat() {
            var mockLogger = new Mock<ILogger>();
            var loggingService = new LoggingService(mockLogger.Object);
            var functionName = "TestFunction";
            var errorMessage = "Test error message";

            loggingService.LogError(functionName, errorMessage);

            mockLogger.Verify(logger => logger.Error(It.Is<string>(message =>
                message.Contains(functionName) &&
                message.Contains(errorMessage) &&
                message.Contains("utc"))), Times.Once);
        }
    }
}
