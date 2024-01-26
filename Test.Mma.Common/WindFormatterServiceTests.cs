using global::Mma.Common.IServices;
using global::Mma.Common.Services;
using global::Mma.Common.models;
using NUnit.Framework;
using System.Collections;
using Moq;
using NLog;
using Mma.Common.IHelpers;
using Mma.Common.Helpers;
using Test.Mma.Common.TestCaseSources;

namespace Test.Mma.Common {
    [TestFixture]
    public class Wind_formatter_tests {
        private Mock<ILogger> _mockLogger;
        private LoggingService _loggingService;
        private WindDataHelper _mockWindDataHelper;
        private IWindFormatterService windFormatter;

        [SetUp]
        public void SetUp() {
            _mockLogger = new Mock<ILogger>();
            _loggingService = new LoggingService(_mockLogger.Object);
            _mockWindDataHelper = new WindDataHelper(_loggingService);
            windFormatter = new WindFormatterService(_mockWindDataHelper);
        }

        public static IEnumerable WindDataTestCases {
            get {
                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = null,
                        MaximumWindDirection = null,
                        AverageWindDirection = null,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    "/////KT");
            }
        }


        [TestCase(null, "///25KT")]
        [TestCase(10, "01025KT")]
        [TestCase(15, "01025KT")]
        [TestCase(350, "35025KT")]
        public void Average_wind_direction_is_correct(double? direction, string expected) {
            var data = new WindData {
                AverageWindDirection = direction,
                AverageWindSpeed = 25,
                MaximumWindSpeed = 28,
                MinimumWindDirection = direction,
                MaximumWindDirection = direction
            };

            var result = windFormatter.FormatWind(data);

            Assert.That(result, Is.EqualTo(expected));
        }

        //First I want to write some tests for the simplest cases
        [Test, TestCaseSource(typeof(WindDataTestSource), nameof(WindDataTestSource.WindDataFullTestCases))]
        public void Format_wind_wind_data_no_value_is_correct(WindData windData, string expected) {
            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null, "/////KT")]
        [TestCase(0, "00000KT")]
        [TestCase(0.5, "00000KT")]
        [TestCase(0.9, "00000KT")]
        public void Format_wind_should_return_is_calm_when_wind_speed_is_less_than_1(double? windSpeed, string expected) {
            var windData = new WindData {
                AverageWindDirection = 0,
                AverageWindSpeed = windSpeed,
                MaximumWindSpeed = 28,
                MinimumWindDirection = 0,
                MaximumWindDirection = 0
            };

            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }


        [Test, TestCaseSource(typeof(WindDataTestSource), nameof(WindDataTestSource.WindDataFullTestCases))]
        public void Is_wind_data_report_correct(WindData windData, string expected) {
            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }

    }
}
