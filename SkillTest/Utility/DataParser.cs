﻿using Mma.Common.Exceptions;
using Mma.Common.IServices;
using Mma.Common.models;
using Mma.Common.Utility;
using System;
using System.Reflection;

namespace Mma.Common.Helpers
{
    public class DataParser : IDataParser {
        private readonly ILoggingService _loggingService;
        public DataParser(ILoggingService logger) {
            _loggingService = logger;
        }

        /// <summary>
        /// Determines if the wind is calm based on the given wind data.
        /// </summary>
        /// <param name="windData">The wind data to check.</param>
        /// <returns>True if the wind is calm; otherwise, false.</returns>
        public bool IsCalmWind(WindData windData) {
            return windData.AverageWindSpeed.HasValue && windData.AverageWindSpeed <= 1;
        }

        public string FormatGustSpeed(double? averageWindSpeed, double? maximumWindSpeed) {
            if (averageWindSpeed.HasValue && maximumWindSpeed.HasValue) {
                if (IsGust(maximumWindSpeed, averageWindSpeed)) {
                    return $"G{FormatWindSpeed(RoundWindSpeed(maximumWindSpeed))}";
                }
            }
            return "";
        }

        private bool IsGust(double? maxSpeed, double? averageSpeed) {
            return RoundWindSpeedToTheNearestKnot(maxSpeed.Value) - RoundWindSpeedToTheNearestKnot(averageSpeed.Value) >= 10;
        }

        public string FormatVariationInDirectionIfVariant(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            //put this into a private function
            if (averageWindSpeed == "P99" || int.TryParse(averageWindSpeed, out int avgSpeed) && avgSpeed > 3) {
                var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
                if (success && variation >= 60 && variation < 180) {
                    return $" {minWindDirection}V{maxWindDirection}";
                }
            }
            return "";
        }


        public string FormatVariationInDirectionForSpeedLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            if (averageWindSpeed != "P99" && int.TryParse(averageWindSpeed, out int avgSpeed) && avgSpeed <= 3 && avgSpeed >= 0) {
                var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
                if (success && variation >= 60 && variation < 180) {
                    return $"VRB";
                }
            }
            return "";
        }

        public string FormatWindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection) {
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
                _loggingService.LogError(MethodBase.GetCurrentMethod().Name, $"Error parsing wind direction: ex:{ex.Message}");
            }
            // Indicate failure
            return (false, 0);
        }

        public string FormatWindDirection(double? windDirection) {
            if (!windDirection.HasValue) {
                return "///";
            }

            var roundedDirection = RoundDirectionToNearestTenDegrees(windDirection).Value;
            return SurfaceWindDirectionIsInRange(roundedDirection.ToString("D3"))
                   ? roundedDirection.ToString("D3")
                   : "///";
        }

        public string RoundWindSpeed(double? windSpeed) {
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
            try {
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
            } catch(Exception ex) {
                _loggingService.LogError(MethodBase.GetCurrentMethod().Name, "Failed to format wind speed");
                if (ex is IntParseException) throw new IntParseException(string.Empty);
                throw new IntParseException(ex.Message);
            }
            
            // Return "//" if wind speed is less than 1
            return "//";
        }

        public bool SurfaceWindSpeedIsGreaterThan1(string ff) {
            if (string.IsNullOrEmpty(ff)) return false;

            // Check if the wind speed is greater than 1 knot
            if (int.TryParse(ff, out int windSpeed)) return windSpeed >= 1;

            // If the string cannot be parsed into an integer
            _loggingService.LogError(MethodBase.GetCurrentMethod().Name, "Failed to parse surface wind speed when checking speed is greater than 1");
            return false;
        }

        public bool SurfaceWindDirectionIsInRange(string ddd) {
            if (string.IsNullOrEmpty(ddd)) return false;

            // Check if the wind direction is within the valid range
            if (int.TryParse(ddd, out int windDirection)) return windDirection >= 10 && windDirection <= 360;

            // If the string cannot be parsed into an integer
            _loggingService.LogError(MethodBase.GetCurrentMethod().Name, "Failed to parse surface wind direction when checking if in range");
            return false;
        }

        public int? RoundWindSpeedToTheNearestKnot(double? windSpeed) {
            if (windSpeed.HasValue) {
                return (int)Math.Round(windSpeed.Value, MidpointRounding.AwayFromZero);
            }
            return null;
        }

        public bool WindFormatterHasData(WindData windData) {
            if (windData is null) _loggingService.LogError(MethodBase.GetCurrentMethod().Name, "WindData was null");
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
    }
}
