using global::Mma.Common;
using global::Mma.Common.IServices;
using global::Mma.Common.Services;
using global::Mma.Common.models;
using NUnit.Framework;
using System.Collections;

namespace Test.Mma.Common {
    //General format: ddd ff G fm fm KT dn dn dn V dx dx dx
    //Where
    //• ddd is the average surface wind direction over the previous 10 minutes 
    //• ff is the average surface wind speed over the previous 10 minutes 
    //• fm is the maximum surface wind gust speed over the previous 10 minutes
    //• dn and dx describe the variation in surface wind direction(in clockwise order) over the
    //previous 10 minutes of the surface wind speed


    //Requirements
    //1. The maximum wind(gust) within the last 10 minutes shall be reported only if it exceeds the
    //average speed by 10 knots or more.

    //2. Variations in wind direction shall be reported only when the total variation in direction over
    //the previous ten-minute period is 60 degrees or more or but less than 180 degrees and the
    //average wind speed is greater than 3 knots.Variations are reported in clockwise order(e.g.
    //290V090 or 170V250).

    //3. The average wind direction shall not be included for variable winds when the total variation
    //in direction over the previous ten-minute period is 60 degrees or more or but less than 180 
    //degrees and the wind speed is 3 knots or less; the wind in this case shall be reported as 
    //variable.

    //4. The average wind direction shall not be included for variable winds when the total variation
    //in direction over the previous ten-minute period is 180 degrees or more or where it is not
    //possible to report a average direction e.g.when a thunderstorm passes over the aerodrome.
    //The wind should be reported as variable and no reference should be made to the two
    //extreme directions between which the wind has varied.

    //5. When the wind speed is less than 1 knot, this should be reported as calm.


    //Range and increments
    //1. The surface wind direction average and variations in direction shall be rounded to the
    //nearest 10 degrees.
    //2. Wind directions of 005, 015 degrees etc. should be rounded down.
    //3. Surface wind direction is reported between 010 and 360 degrees.
    //4. The surface wind average speed and maximum speed shall be rounded to the nearest knot
    //in the METAR. Surface wind speed is reported between 01 and 99 knots.If the speed is 100 
    //knots or more, the wind speed should be encoded as “P99” (see example 7 below).
    //5. Calm is encoded as ‘00000KT’.
    //6. Variable is encoded ‘VRB’.
    //7. Missing values shall be encodes with /. 


    //Examples of METAR surface wind coding
    //1. 02008KT wind zero two zero degrees, 8 knots
    //2. 00000KT wind calm
    //3. VRB02KT wind variable, 2 knots (the variation in direction over the previous ten-minute
    //period has been 60 degrees or more but less than 180 degrees and the wind speed is 3 
    //knots or less)
    //4. 33022G34KT wind three three zero degrees, 22 knots, max 34 knots
    //5. 16016KT 120V190 wind one six zero degrees, sixteen knots, varying between 120 degrees
    //and 190 degrees
    //6. 21015G28KT 180V270 wind two one zero degrees, 15 knots, max 28 knots varying between 
    //180 degrees and 270 degrees
    //7. 27070GP99KT wind two seven zero degrees, 70 knots, max 100 knots or more
    //8. ///12KT when average wind direction is missing


    [TestFixture]
    public class Wind_formatter_tests {
        private IWindFormatterService windFormatter;

        [SetUp]
        public void SetUp() {
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

        //This test ensures that the wind speeds are rounded to the nearest knot
        [TestCase(null, null)]
        [TestCase(0, 0)]
        [TestCase(0.4, 0)]
        [TestCase(0.5, 1)]
        [TestCase(0.9, 1)]
        [TestCase(1.4, 1)]
        public void Is_average_and_max_wind_speeds_rounded_to_the_nearest_knot(double? speed, int? expected) {
            var windFormatter = new WindFormatterService();

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
        public void Is_variation_in_wind_direction_in_range_and_greater_than_3(string minWindDirection, string maxWindDirection, string averageWindSpeed,string expected) {
            string result = windFormatter.FormatVariationInDirection(minWindDirection, maxWindDirection, averageWindSpeed);

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

        [TestCase("340", "160", "030" ,"VRB")]
        [TestCase("010", "180", "050", "050")]
        [TestCase("///", "///","///", "///")]
        [TestCase("190", "000","///", "VRB")]
        public void Is_variation_in_wind_direction_180_or_more(string minWindDirection, string maxWindDirection, string averageWindDirection, string expected) {

            string result = windFormatter.WindDirectionVariationIsGreaterThan180(minWindDirection, maxWindDirection, averageWindDirection);
            Assert.AreEqual(expected, result);
        }

        [Test, TestCaseSource(nameof(WindDataFullTestCases))]
        public void Is_wind_data_report_correct(WindData windData, string expected) {
            string result = windFormatter.FormatWind(windData);

            Assert.AreEqual(expected, result);
        }
    }
}
