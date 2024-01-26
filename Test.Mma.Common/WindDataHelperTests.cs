﻿using global::Mma.Common.IServices;
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
    public class Wind_data_helper_tests {
        private Mock<ILogger> _mockLogger;
        private LoggingService _loggingService;
        private WindDataHelper _windDataHelper;

        [SetUp]
        public void SetUp() {
            _mockLogger = new Mock<ILogger>();
            _loggingService = new LoggingService(_mockLogger.Object);
            _windDataHelper = new WindDataHelper(_loggingService);
        }

        [TestCase(null, false)]
        public void Is_wind_formatter_null_returning_correct_boolean_value(WindData windData, bool expected) {
            bool result = _windDataHelper.WindFormatterHasData(windData);

            Assert.IsFalse(result);
        }

        [TestCase(null, null)]
        [TestCase(0, 0)]
        [TestCase(0.4, 0)]
        [TestCase(0.5, 1)]
        [TestCase(0.9, 1)]
        [TestCase(1.4, 1)]
        public void Is_average_and_max_wind_speeds_rounded_to_the_nearest_knot(double? speed, int? expected) {
            int? result = _windDataHelper.RoundWindSpeedToTheNearestKnot(speed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null, null)]
        [TestCase(0, 0)]
        [TestCase(05, 0)]
        [TestCase(15, 10)]
        [TestCase(16, 20)]
        [TestCase(20, 20)]
        public void Is_round_direction_to_nearest_ten_degrees_returning_correct_value(double? direction, int? expected) {
            int? result = _windDataHelper.RoundDirectionToNearestTenDegrees(direction);

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
            bool result = _windDataHelper.SurfaceWindDirectionIsInRange(ddd);

            Assert.AreEqual(expected, result);
        }

        [TestCase("05", true)]
        [TestCase("01", true)]
        [TestCase("01", true)]
        [TestCase("-05", false)]
        [TestCase("0", false)]
        [TestCase(null, false)]
        public void Is_surface_wind_speed_reported_greater_than_1(string ff, bool expected) {
            bool result = _windDataHelper.SurfaceWindSpeedIsGreaterThan1(ff);

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
            string result = _windDataHelper.FormatWindSpeed(ff);

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
            string result = _windDataHelper.FormatVariationInDirectionIfVariant(minWindDirection, maxWindDirection, averageWindSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(0.5, 100, "GP99")]
        [TestCase(90.4, 100, "GP99")]
        [TestCase(90.6, 100, "")]
        [TestCase(5.7, 20, "G20")]
        [TestCase(null, 20, "")]
        [TestCase(null, null, "")]
        public void Is_wind_gust_reported(double? averageWindSpeed, double? maximumWindSpeed, string expected) {
            string result = _windDataHelper.FormatGustSpeed(averageWindSpeed, maximumWindSpeed);

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
            string result = _windDataHelper.FormatVariationInDirectionForSpeedLessThan3Knots(minWindDirection, maxWindDirection, averageWindSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase("340", "160", "030", "VRB")]
        [TestCase("010", "180", "050", "050")]
        [TestCase("///", "///", "///", "///")]
        [TestCase("190", "000", "///", "VRB")]
        public void Is_variation_in_wind_direction_180_or_more(string minWindDirection, string maxWindDirection, string averageWindDirection, string expected) {

            string result = _windDataHelper.FormatWindDirectionVariationIsGreaterThan180(minWindDirection, maxWindDirection, averageWindDirection);
            Assert.AreEqual(expected, result);
        }


        [TestCase("100", "200", "10", " 100V200")]
        [TestCase("110", "300", "100", "")]
        public void Format_variation_in_direction_when_directions_valid_should_calculate_correct_variation(string minDirection, string maxDirection, string averageSpeed, string expected) {
            string result = _windDataHelper.FormatVariationInDirectionIfVariant(minDirection, maxDirection, averageSpeed);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null, "200", "10", "")]
        public void Format_variation_in_direction_when_directions_invalid_should_return_empty_string(string minDirection, string maxDirection, string averageSpeed, string expected) {
            string result = _windDataHelper.FormatVariationInDirectionIfVariant(minDirection, maxDirection, averageSpeed);

            Assert.AreEqual("", result);
        }
    }
}
