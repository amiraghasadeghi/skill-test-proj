using global::Mma.Common.IServices;
using global::Mma.Common.Services;
using global::Mma.Common.models;
using NUnit.Framework;
using System.Collections;
using Moq;
using NLog;

namespace Test.Mma.Common {
    [TestFixture]
    public class Wind_formatter_tests {
        private Mock<ILogger> _mockLogger;
        private IWindFormatterService windFormatter;

        [SetUp]
        public void SetUp() {
            _mockLogger = new Mock<ILogger>();
            windFormatter = new WindFormatterService();
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

        [TestCase(null, false)]
        public void Is_wind_formatter_null_returning_correct_boolean_value(WindData windData, bool expected) {
            bool result = windFormatter.WindFormatterHasData(windData);

            Assert.IsFalse(result);
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

        [TestCase(null, null)]
        [TestCase(0, 0)]
        [TestCase(0.4, 0)]
        [TestCase(0.5, 1)]
        [TestCase(0.9, 1)]
        [TestCase(1.4, 1)]
        public void Is_average_and_max_wind_speeds_rounded_to_the_nearest_knot(double? speed, int? expected) {
            int? result = windFormatter.RoundWindSpeedToTheNearestKnot(speed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null, null)]
        [TestCase(0, 0)]
        [TestCase(05, 0)]
        [TestCase(15, 10)]
        [TestCase(16, 20)]
        [TestCase(20, 20)]
        public void Is_round_direction_to_nearest_ten_degrees_returning_correct_value(double? direction, int? expected) {
            int? result = windFormatter.RoundDirectionToNearestTenDegrees(direction);

            Assert.AreEqual(expected, result);
        }

        [TestCase("05", false)]
        [TestCase("010", true)]
        [TestCase("100", true)]
        [TestCase("360", true)]
        [TestCase("365", false)]
        [TestCase("370", false)]
        [TestCase(null, false)]
        public void Is_surface_wind_direction_reported_between_010_and_360_degrees(string ddd, bool expected) {
            bool result = windFormatter.SurfaceWindDirectionIsInRange(ddd);

            Assert.AreEqual(expected, result);
        }

        [TestCase("05", true)]
        [TestCase("01", true)]
        [TestCase("01", true)]
        [TestCase("-05", false)]
        [TestCase("0", false)]
        [TestCase(null, false)]
        public void Is_surface_wind_speed_reported_greater_than_1(string ff, bool expected) {
            bool result = windFormatter.SurfaceWindSpeedIsGreaterThan1(ff);

            Assert.AreEqual(expected, result);
        }

        [TestCase("05", "05")]
        [TestCase("01", "01")]
        [TestCase("99", "99")]
        [TestCase("-05", "//")]
        [TestCase("00", "00")]
        [TestCase("100", "P99")]
        [TestCase("101", "P99")]
        [TestCase("//", "//")]
        [TestCase(null, "//")]
        public void Is_surface_wind_speed_greater_than_100_or_more_reported_as_P99(string ff, string expected) {
            string result = windFormatter.FormatWindSpeed(ff);

            Assert.AreEqual(expected, result);
        }

        [TestCase("005", "005", "05", "")]
        [TestCase(null, null, null, "")]
        [TestCase("-005", "065", "03", "")]
        [TestCase("005", "065", "00", "")]
        [TestCase("005", "065", "-01", "")]
        [TestCase("000", "059", "03", "")]
        [TestCase("000", "070", "03", "")]
        [TestCase("000", "180", "04", "")]
        [TestCase("000", "179", "P99", " 000V179")]
        [TestCase("000", "181", "05", "")]
        [TestCase("///", "181", "05", "")]
        [TestCase("000", "170", "04", " 000V170")]
        public void Is_variation_in_wind_direction_in_range_and_greater_than_3(string minWindDirection, string maxWindDirection, string averageWindSpeed, string expected) {
            string result = windFormatter.FormatVariationInDirectionIfVariant(minWindDirection, maxWindDirection, averageWindSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(0.5, 100, "GP99")]
        [TestCase(90.4, 100, "GP99")]
        [TestCase(90.6, 100, "")]
        [TestCase(5.7, 20, "G20")]
        [TestCase(null, 20, "")]
        [TestCase(null, null, "")]
        public void Is_wind_gust_reported(double? averageWindSpeed, double? maximumWindSpeed, string expected) {
            string result = windFormatter.FormatGust(averageWindSpeed, maximumWindSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase("005", "005", "05", "")]
        [TestCase(null, null, null, "")]
        [TestCase("-005", "065", "03", "")]
        [TestCase("005", "065", "00", "VRB")]
        [TestCase("005", "065", "-01", "")]
        [TestCase("000", "059", "03", "")]
        [TestCase("000", "070", "03", "VRB")]
        [TestCase("000", "180", "02", "")]
        [TestCase("000", "179", "01", "VRB")]
        [TestCase("000", "181", "05", "")]
        [TestCase("000", "170", "P99", "")]
        [TestCase("///", "181", "05", "")]
        public void Is_variation_in_wind_direction_in_range_and_less_than_3(string minWindDirection, string maxWindDirection, string averageWindSpeed, string expected) {
            string result = windFormatter.IsVariationInWindDirectionAndLessThan3Knots(minWindDirection, maxWindDirection, averageWindSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase("340", "160", "030", "VRB")]
        [TestCase("010", "180", "050", "050")]
        [TestCase("///", "///", "///", "///")]
        [TestCase("190", "000", "///", "VRB")]
        public void Is_variation_in_wind_direction_180_or_more(string minWindDirection, string maxWindDirection, string averageWindDirection, string expected) {

            string result = windFormatter.WindDirectionVariationIsGreaterThan180(minWindDirection, maxWindDirection, averageWindDirection);
            Assert.AreEqual(expected, result);
        }

        [Test, TestCaseSource(nameof(WindDataFullTestCases))]
        public void Is_wind_data_report_correct(WindData windData, string expected) {
            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }

        [TestCase("100", "200", "10", " 100V200")]
        [TestCase("110", "300", "100", "")]
        public void Format_variation_in_direction_when_directions_valid_should_calculate_correct_variation(string minDirection, string maxDirection, string averageSpeed, string expected) {
            string result = windFormatter.FormatVariationInDirectionIfVariant(minDirection, maxDirection, averageSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null, "200", "10", "")]
        public void Format_variation_in_direction_when_directions_invalid_should_return_empty_string(string minDirection, string maxDirection, string averageSpeed, string expected) {
            string result = windFormatter.FormatVariationInDirectionIfVariant(minDirection, maxDirection, averageSpeed);
            windFormatter.LogError(minDirection, maxDirection);

            Assert.AreEqual("", result);
        }

    }
}
