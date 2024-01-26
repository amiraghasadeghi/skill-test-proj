using global::Mma.Common.IServices;
using global::Mma.Common.Services;
using global::Mma.Common.models;
using NUnit.Framework;
using System.Collections;
using Moq;
using NLog;
using Mma.Common.IHelpers;
using Mma.Common.Helpers;

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

        public static IEnumerable WindDataFullTestCases {
            get {
                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 8,
                        MaximumWindDirection = null,
                        AverageWindDirection = 20,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    "02008KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 0,
                        MaximumWindDirection = 0,
                        AverageWindDirection = 0,
                        MaximumWindSpeed = 0,
                        MinimumWindDirection = 0
                    },
                    "00000KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 2,
                        MaximumWindDirection = 180,
                        AverageWindDirection = 50,
                        MaximumWindSpeed = 0,
                        MinimumWindDirection = 100
                    },
                    "VRB02KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 22,
                        MaximumWindDirection = null,
                        AverageWindDirection = 330,
                        MaximumWindSpeed = 34,
                        MinimumWindDirection = 100
                    },
                    "33022G34KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 16,
                        MaximumWindDirection = 190,
                        AverageWindDirection = 160,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = 120
                    },
                    "16016KT 120V190");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 15,
                        MaximumWindDirection = 270,
                        AverageWindDirection = 210,
                        MaximumWindSpeed = 28,
                        MinimumWindDirection = 180
                    },
                    "21015G28KT 180V270");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 70,
                        MaximumWindDirection = null,
                        AverageWindDirection = 270,
                        MaximumWindSpeed = 100,
                        MinimumWindDirection = null
                    },
                    "27070GP99KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 12,
                        MaximumWindDirection = null,
                        AverageWindDirection = null,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    "///12KT");
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
        [Test, TestCaseSource(nameof(WindDataTestCases))]
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


        [Test, TestCaseSource(nameof(WindDataFullTestCases))]
        public void Is_wind_data_report_correct(WindData windData, string expected) {
            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }

    }
}
