using Mma.Common.Constants;
using Mma.Common.IServices;
using Moq;
using NUnit.Framework;

namespace Mma.Common.Tests {
    [TestFixture]
    public class Wind_constants_tests {
        private WindConstants _windConstants;
        private Mock<ILoggingService> _mockLoggingService;

        [SetUp]
        public void Setup() {
            _mockLoggingService = new Mock<ILoggingService>();
            _windConstants = new WindConstants(_mockLoggingService.Object);
        }

        [Test]
        public void Is_get_wind_constant_description_with_valid_value_returning_correct_description() {
            var result = _windConstants.GetWindConstantDescription(WindConstants.CalmWinds);
            Assert.AreEqual("Calm winds", result);
        }

        [Test]
        public void Is_get_wind_Constant_description_with_invalid_value_returning_fescription_not_found() {
            var result = _windConstants.GetWindConstantDescription("InvalidValue");
            Assert.AreEqual("Description not found", result);
        }

        [Test]
        public void Is_get_wind_constant_description_with_null_value_returns_invalid_input() {
            var result = _windConstants.GetWindConstantDescription(null);
            Assert.AreEqual("Invalid input", result);
        }
    }
}
