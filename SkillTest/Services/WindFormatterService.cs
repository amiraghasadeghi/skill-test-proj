using System;
using System.Reflection;
using System.Text;
using Mma.Common.IServices;
using Mma.Common.models;
using NLog;

namespace Mma.Common.Services {
    public class WindFormatterService : IWindFormatterService {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FormatWind(WindData windData) {
            if (!WindFormatterHasData(windData)) {
                return "/////KT"; // Return default value if no data
            }

            if (IsCalmWind(windData)) {
                return "00000KT"; // Calm wind
            }

            var ddd = FormatWindDirection(windData.AverageWindDirection);
            var ff = FormatWindSpeed(RoundWindSpeed(windData.AverageWindSpeed));
            var dnVdx = FormatVariationInDirectionIfVariant(FormatWindDirection(windData.MinimumWindDirection), FormatWindDirection(windData.MaximumWindDirection), ff);
            var gust = FormatGust(RoundWindSpeedToTheNearestKnot(windData.AverageWindSpeed), RoundWindSpeedToTheNearestKnot(windData.MaximumWindSpeed));
            var dnVdxAtLessThan3Knots = IsVariationInWindDirectionAndLessThan3Knots(FormatWindDirection(windData.MinimumWindDirection), FormatWindDirection(windData.MaximumWindDirection), ff);
            // If dbVdxAtLessThan3Knots has a value, use it instead of ddd
            string directionComponent = WindDirectionVariationIsGreaterThan180(FormatWindDirection(windData.MinimumWindDirection), FormatWindDirection(windData.MaximumWindDirection), !string.IsNullOrEmpty(dnVdxAtLessThan3Knots) ? dnVdxAtLessThan3Knots : ddd);
            //Here I can handle all the errors, I can validate all
            //the values coming in the model and if any doesn't match criteria I can return with error (specific error)
            return $"{directionComponent}{ff}{gust}KT{dnVdx}";
        }

        public string FormatGust(double? averageWindSpeed, double? maximumWindSpeed) {
            if (averageWindSpeed.HasValue && maximumWindSpeed.HasValue) {
                if (RoundWindSpeedToTheNearestKnot(maximumWindSpeed.Value) - RoundWindSpeedToTheNearestKnot(averageWindSpeed.Value) >= 10) {
                    return $"G{FormatWindSpeed(RoundWindSpeed(maximumWindSpeed))}";
                }
            }
            return "";
        }

        public string FormatVariationInDirectionIfVariant(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            if (averageWindSpeed == "P99" || int.TryParse(averageWindSpeed, out int avgSpeed) && avgSpeed > 3) {
                var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
                if (success && variation >= 60 && variation < 180) {
                    return $" {minWindDirection}V{maxWindDirection}";
                }
            }
            return "";
        }


        public string IsVariationInWindDirectionAndLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            if (averageWindSpeed != "P99" && int.TryParse(averageWindSpeed, out int avgSpeed) && avgSpeed <= 3 && avgSpeed >= 0) {
                var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
                if (success && variation >= 60 && variation < 180) {
                    return $"VRB";
                }
            }
            return "";
        }

        public string WindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection) {
            var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
            if (success && variation >= 180) {
                return "VRB";
            }
            return averageWindDirection;
        }


        public (bool success, int variation) TryCalculateWindDirectionVariation(string minWindDirection, string maxWindDirection) {
            try {
                if (int.TryParse(maxWindDirection, out int maxDir) && maxDir >= 0 &&
                    int.TryParse(minWindDirection, out int minDir) && minDir >= 0) {
                    int variation = Math.Abs(maxDir - minDir);
                    return (true, variation);
                }
            } catch (Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name, $"Error parsing wind direction: ex:{ex.Message}");
            }
            return (false, 0); // Indicate failure
        }



        private bool IsCalmWind(WindData windData) {
            return windData.AverageWindSpeed.HasValue && windData.AverageWindSpeed <= 1;
        }

        private string FormatWindDirection(double? windDirection) {
            if (!windDirection.HasValue) {
                return "///";
            }

            var roundedDirection = RoundDirectionToNearestTenDegrees(windDirection).Value;
            return SurfaceWindDirectionIsInRange(roundedDirection.ToString("D3"))
                   ? roundedDirection.ToString("D3")
                   : "///";
        }

        private string RoundWindSpeed(double? windSpeed) {
            if (!windSpeed.HasValue) {
                return "//";
            }

            var roundedSpeed = RoundWindSpeedToTheNearestKnot(windSpeed).Value;
            return roundedSpeed >= 1
                   ? roundedSpeed.ToString("D2")
                   : "//";
        }

        public string FormatWindSpeed(string ff) {
            if (string.IsNullOrEmpty(ff)) return "//";
            if (int.TryParse(ff, out int windSpeed)) {
                switch (windSpeed) {
                    case int n when n >= 100:
                        // Return "P99" for wind speeds of 100 knots or more
                        return "P99";
                    case int n when n >= 1:
                        // Return wind speed formatted as a string for speeds between 1 and 99 knots
                        return n.ToString("D2");
                    case 0:
                        // Return "00000" for calm wind
                        return "00";
                    default:
                        return "//"; // Return this if parsing fails or if wind speed is less than 1
                }
            }
            // Return "//" if parsing fails or if wind speed is less than 1
            LogError(MethodBase.GetCurrentMethod().Name, "Failed to format wind speed");
            return "//";
        }

        public bool SurfaceWindSpeedIsGreaterThan1(string ff) {
            if (string.IsNullOrEmpty(ff)) return false;

            // Check if the wind speed is greater than 1 knot
            if (int.TryParse(ff, out int windSpeed)) return windSpeed >= 1;

            // If the string cannot be parsed into an integer
            LogError(MethodBase.GetCurrentMethod().Name, "Failed to parse surface wind speed when checking speed is greater than 1");

            return false;
        }

        public bool SurfaceWindDirectionIsInRange(string ddd) {
            if (string.IsNullOrEmpty(ddd)) return false;

            // Check if the wind direction is within the valid range
            if (int.TryParse(ddd, out int windDirection)) return windDirection >= 10 && windDirection <= 360;

            // If the string cannot be parsed into an integer
            LogError(MethodBase.GetCurrentMethod().Name, "Failed to parse surface wind direction when checking if in range");
            return false;
        }

        public int? RoundWindSpeedToTheNearestKnot(double? windSpeed) {
            if (windSpeed.HasValue) {
                return (int)Math.Round(windSpeed.Value, MidpointRounding.AwayFromZero);
            }
            return null;
        }

        public bool WindFormatterHasData(WindData windData) {
            if (windData is null) LogError(MethodBase.GetCurrentMethod().Name, "WindData was null");
            return windData != null;
        }

        public int? RoundDirectionToNearestTenDegrees(double? direction) {
            if (direction.HasValue) {
                int unitsDigit = (int)(direction.Value % 10);

                if (unitsDigit <= 5) {
                    return (int)(direction.Value - unitsDigit);
                }

                return (int)(Math.Round(direction.Value / 10) * 10);
            }
            return null;
        }

        public void LogError(string functionName, string errorMessage) {
            string timestamp = DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:ss"); 
            logger.Error($"{functionName} - {errorMessage} [{timestamp} utc]");
        }
    }
}
