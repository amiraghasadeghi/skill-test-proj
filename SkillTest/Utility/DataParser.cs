using Mma.Common.Constants;
using Mma.Common.Exceptions;
using Mma.Common.Interfaces;
using Mma.Common.IServices;
using Mma.Common.models;
using Mma.Common.Validators;
using System;
using System.Reflection;

namespace Mma.Common.Helpers {
    public class DataParser : IDataParser {
        private readonly ILoggingService _loggingService;
        private readonly IParsingValidator _parsingValidator;
        public DataParser(ILoggingService logger, IParsingValidator parsingValidator) {
            _loggingService = logger;
            _parsingValidator = parsingValidator;
        }

        /// <summary>
        /// Determines if the wind is calm based on the given wind data.
        /// </summary>
        /// <param name="windData">The wind data to check.</param>
        /// <returns>True if the wind is calm; otherwise, false.</returns>
        public bool IsCalmWind(WindData windData) {
            return windData.AverageWindSpeed.HasValue && windData.AverageWindSpeed <= 1;
        }

        /// <summary>
        /// Formats the gust speed if it exceeds the average wind speed by a significant margin.
        /// </summary>
        /// <param name="averageWindSpeed">The average wind speed over a specified period.</param>
        /// <param name="maximumWindSpeed">The maximum gust speed observed during the same period.</param>
        /// <returns>A formatted string representing the gust speed if it is significantly higher than the average speed; otherwise, an empty string.</returns>
        public string FormatGustSpeed(double? averageWindSpeed, double? maximumWindSpeed) {
            if (averageWindSpeed.HasValue && maximumWindSpeed.HasValue) {
                if (IsGust(maximumWindSpeed, averageWindSpeed)) {
                    return $"{WindConstants.Gust}{FormatWindSpeed(RoundWindSpeed(maximumWindSpeed))}";
                }
            }
            return "";
        }

        private bool IsGust(double? maxSpeed, double? averageSpeed) {
            return RoundWindSpeedToTheNearestKnot(maxSpeed.Value) - RoundWindSpeedToTheNearestKnot(averageSpeed.Value) >= 10;
        }

        /// <summary>
        /// Formats the variation in wind direction when the total variation is significant and the average wind speed is above a certain threshold.
        /// </summary>
        /// <param name="minWindDirection">The minimum wind direction observed during the specified period.</param>
        /// <param name="maxWindDirection">The maximum wind direction observed during the same period.</param>
        /// <param name="averageWindSpeed">The average wind speed over the specified period.</param>
        /// <returns>A formatted string representing the range of wind direction variation if conditions are met; otherwise, an empty string.</returns>
        /// <remarks>
        /// The method checks if the average wind speed is above 3 knots and the total variation in wind direction is between 60 and 180 degrees.
        /// If these conditions are met, it returns a formatted string showing the range of variation; otherwise, it returns an empty string.
        /// </remarks>
        public string FormatVariationInDirectionForSpeedMoreThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            if (ClassifyAverageSpeed(averageWindSpeed) == AverageSpeedCategory.Above3Knots) {
                var (success, variation) = FormatWindDirectionBasedOnVariation(minWindDirection, maxWindDirection, averageWindSpeed);
                if (success) return variation;
            }
            return "";
        }

        /// <summary>
        /// Formats the variation in wind direction for cases where the wind speed is less than or equal to 3 knots.
        /// </summary>
        /// <param name="minWindDirection">The minimum wind direction observed during the specified period.</param>
        /// <param name="maxWindDirection">The maximum wind direction observed during the same period.</param>
        /// <param name="averageWindSpeed">The average wind speed over the specified period.</param>
        /// <returns>A formatted string indicating variable wind direction ('VRB') if conditions are met; otherwise, an empty string.</returns>
        /// <remarks>
        /// This method checks if the average wind speed is less than or equal to 3 knots and if the total variation in wind direction is between 60 and 180 degrees.
        /// If these conditions are met, it returns 'VRB' to indicate variable wind direction; otherwise, it returns an empty string.
        /// This is in accordance with reporting standards for cases where the wind speed is very low and wind direction is highly variable.
        /// </remarks>
        public string FormatVariationInDirectionForSpeedLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            if (ClassifyAverageSpeed(averageWindSpeed) == AverageSpeedCategory.BelowOrEqual3Knots) {
                var (success, variation) = FormatWindDirectionBasedOnVariation(minWindDirection, maxWindDirection, averageWindSpeed);
                if (success) return variation;
            }
            return "";
        }

        private AverageSpeedCategory ClassifyAverageSpeed(string averageWindSpeed) {
            if (!string.IsNullOrWhiteSpace(averageWindSpeed) && averageWindSpeed != WindConstants.MissingSpeed) {
                if (averageWindSpeed == WindConstants.HundredAndOver) {
                    return AverageSpeedCategory.Above3Knots;
                }

                int avgSpeed = IntToStringParser(averageWindSpeed, nameof(averageWindSpeed));
                if (avgSpeed > 3) {
                    return AverageSpeedCategory.Above3Knots;
                }
                if (avgSpeed <= 3 && avgSpeed >= 0) {
                    return AverageSpeedCategory.BelowOrEqual3Knots;
                }
            }

            return AverageSpeedCategory.Invalid;
        }

        private (bool success, string formattedString) FormatWindDirectionBasedOnVariation(string minWindDirection, string maxWindDirection, string averageWindSpeed) {
            var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);

            if (success && variation >= 60 && variation < 180) {
                // If average speed is less than or equal to 3 knots, return VRB.
                if (ClassifyAverageSpeed(averageWindSpeed) == AverageSpeedCategory.BelowOrEqual3Knots) {
                    return (true, WindConstants.VariableSpeed);
                }

                // Otherwise, return formatted string with variation.
                return (true, $" {minWindDirection}V{maxWindDirection}");
            }

            return (false, string.Empty);
        }





        /// <summary>
        /// Formats the wind direction data when the variation in wind direction is greater than or equal to 180 degrees.
        /// </summary>
        /// <param name="minWindDirection">The minimum wind direction observed during the specified period.</param>
        /// <param name="maxWindDirection">The maximum wind direction observed during the same period.</param>
        /// <param name="averageWindDirection">The average wind direction over the specified period.</param>
        /// <returns>A formatted string indicating variable wind direction ('VRB') if the variation is 180 degrees or more; otherwise, returns the average wind direction.</returns>
        /// <remarks>
        /// This method evaluates the difference between the maximum and minimum wind directions. If this variation is 180 degrees or more, it indicates a highly variable wind direction, which is represented by 'VRB'.
        /// In situations where the wind direction variation is less than 180 degrees, it returns the average wind direction as is, assuming it's a valid value.
        /// </remarks>
        public string FormatWindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection) {
            var (success, variation) = TryCalculateWindDirectionVariation(minWindDirection, maxWindDirection);
            if (success && variation >= 180) {
                return WindConstants.VariableSpeed;
            }
            return averageWindDirection;
        }

        /// <summary>
        /// Attempts to calculate the variation in wind direction based on the minimum and maximum wind directions.
        /// </summary>
        /// <param name="minWindDirection">The minimum wind direction observed during the specified period.</param>
        /// <param name="maxWindDirection">The maximum wind direction observed during the same period.</param>
        /// <returns>A tuple containing a boolean indicating success or failure and an integer representing the calculated variation in wind direction.</returns>
        /// <remarks>
        /// This method tries to parse the minimum and maximum wind direction strings into integers and calculates the absolute difference between them, representing the variation in wind direction.
        /// The method returns a tuple where the first element is a boolean indicating whether the calculation was successful, and the second element is the calculated variation.
        /// If either the minimum or maximum wind direction cannot be parsed into an integer, or if they are invalid values (less than 0), the method logs an error and indicates failure by returning (false, 0).
        /// </remarks>
        public (bool success, int variation) TryCalculateWindDirectionVariation(string minWindDirection, string maxWindDirection) {
            if (!string.IsNullOrWhiteSpace(minWindDirection) && !string.IsNullOrWhiteSpace(maxWindDirection)
                && minWindDirection != WindConstants.MissingDirection 
                && maxWindDirection != WindConstants.MissingDirection) {

                int maxDir = IntToStringParser(maxWindDirection, nameof(maxWindDirection));
                int minDir = IntToStringParser(minWindDirection, nameof(minWindDirection));

                if (maxDir >= 0 && minDir >= 0) {
                    int variation = Math.Abs(maxDir - minDir);
                    return (true, variation);
                }
            }
            
            return (false, 0);
        }

        /// <summary>
        /// Formats the wind direction to a standardised string representation.
        /// </summary>
        /// <param name="windDirection">The wind direction in degrees to format.</param>
        /// <returns>A string representing the formatted wind direction.</returns>
        /// <remarks>
        /// This method formats a given wind direction value into a three-digit string representation.
        /// If the wind direction is not provided (null), the method returns a placeholder string "///".
        /// If the wind direction is provided, the method rounds it to the nearest ten degrees using the RoundDirectionToNearestTenDegrees method.
        /// After rounding, the method checks if the wind direction is within the valid range (010 to 360 degrees). 
        /// If it is within range, it returns the direction as a zero-padded three-digit string. Otherwise, it returns the placeholder string.
        /// </remarks>
        public string FormatWindDirection(double? windDirection) {
            if (!windDirection.HasValue) {
                return WindConstants.MissingDirection;
            }

            var roundedDirection = RoundDirectionToNearestTenDegrees(windDirection).Value;
            return SurfaceWindDirectionIsInRange(roundedDirection.ToString("D3"))
                   ? roundedDirection.ToString("D3")
                   : WindConstants.MissingDirection;
        }

        /// <summary>
        /// Rounds the given wind speed to the nearest knot and formats it into a string.
        /// </summary>
        /// <param name="windSpeed">The wind speed in knots to be rounded.</param>
        /// <returns>A string representing the rounded wind speed.</returns>
        /// <remarks>
        /// This method is used for rounding the wind speed to the nearest whole number (knot) and formatting it for presentation or reporting.
        /// If the wind speed is not provided (null), it returns a placeholder string "//" indicating missing data.
        /// The wind speed is rounded using the RoundWindSpeedToTheNearestKnot method. 
        /// If the rounded speed is greater than or equal to 1, it returns a zero-padded two-digit string representation of the speed.
        /// If the rounded speed is less than 1, it returns the placeholder string "//" to indicate an effectively calm or negligible wind speed.
        /// </remarks>
        public string RoundWindSpeed(double? windSpeed) {
            if (!windSpeed.HasValue) {
                return WindConstants.MissingSpeed;
            }

            var roundedSpeed = RoundWindSpeedToTheNearestKnot(windSpeed).Value;
            return roundedSpeed >= 1
                   ? roundedSpeed.ToString("D2")
                   : WindConstants.MissingSpeed;
        }

        /// <summary>
        /// Formats a given wind speed string into a standardised representation.
        /// </summary>
        /// <param name="ff">The wind speed as a string to be formatted.</param>
        /// <returns>A standardised string representing the wind speed.</returns>
        /// <remarks>
        /// This method takes a wind speed string and formats it according to aviation standards.
        /// If the input string is null or empty, it returns "//" to indicate missing data.
        /// If the wind speed can be parsed as an integer:
        /// - For wind speeds of 100 knots or more, it returns "P99" to indicate extremely high wind speeds.
        /// - For wind speeds between 1 and 99 knots, it returns a zero-padded two-digit string representing the speed.
        /// - For a wind speed of 0 (calm wind), it returns "00".
        /// If the wind speed cannot be parsed or is less than 1 knot (not calm), it logs an error and returns "//".
        /// </remarks>
        public string FormatWindSpeed(string ff) {
            if (string.IsNullOrEmpty(ff) || ff == WindConstants.MissingSpeed) return WindConstants.MissingSpeed;
            int windSpeed = IntToStringParser(ff, nameof(ff));

            switch (windSpeed) {
                case int n when n >= 100:
                    return WindConstants.HundredAndOver;
                case int n when n >= 1:
                    return n.ToString("D2");
                case 0:
                    return "00";
                default:
                    return WindConstants.MissingSpeed;
            }
        }

        /// <summary>
        /// Determines if the given wind speed string represents a wind speed greater than 1 knot.
        /// </summary>
        /// <param name="ff">The wind speed as a string to be evaluated.</param>
        /// <returns>True if the wind speed is greater than 1 knot, otherwise false.</returns>
        /// <remarks>
        /// This method checks if the provided wind speed string (ff) represents a value greater than 1 knot. 
        /// It handles the following scenarios:
        /// - If the input string is null or empty, the method returns false, indicating the wind speed is not greater than 1 knot.
        /// - If the string can be parsed into an integer, the method checks if this integer value is greater than or equal to 1, 
        ///   returning true if it is, and false if it isn't.
        /// - If the string cannot be parsed into an integer, an error is logged, and the method returns false.
        /// </remarks>
        public bool SurfaceWindSpeedIsGreaterThan1(string ff) {
            if (string.IsNullOrEmpty(ff)) return false;

            int windSpeed = IntToStringParser(ff, nameof(ff));
            return windSpeed >= 1;
        }

        /// <summary>
        /// Checks if the provided wind direction string is within the valid range.
        /// </summary>
        /// <param name="ddd">The wind direction as a string to be evaluated.</param>
        /// <returns>True if the wind direction is within the range of 10 to 360 degrees, otherwise false.</returns>
        /// <remarks>
        /// This method evaluates if the wind direction specified in the string 'ddd' falls within the acceptable range of 10 to 360 degrees, inclusive.
        /// The method operates under the following conditions:
        /// - If the input string is null or empty, it returns false, indicating the wind direction is not within the specified range.
        /// - If the string can be successfully parsed to an integer, the method checks if this integer falls within 10 to 360 degrees. 
        ///   It returns true if it does, and false if it doesn't.
        /// - If the string cannot be parsed into an integer, an error is logged, and the method returns false.
        /// </remarks>
        public bool SurfaceWindDirectionIsInRange(string ddd) {
            if (string.IsNullOrEmpty(ddd)) return false;

            int windDirection = IntToStringParser(ddd, nameof(ddd));
            return windDirection >= 10 && windDirection <= 360;
        }

        /// <summary>
        /// Rounds the given wind speed to the nearest knot.
        /// </summary>
        /// <param name="windSpeed">The wind speed to round, represented as a nullable double.</param>
        /// <returns>The rounded wind speed in knots as an integer, or null if the wind speed is not provided.</returns>
        /// <remarks>
        /// This method rounds the provided wind speed value to the nearest whole number using the standard rounding method (away from zero).
        /// </remarks>
        public int? RoundWindSpeedToTheNearestKnot(double? windSpeed) {
            if (windSpeed.HasValue) {
                return (int)Math.Round(windSpeed.Value, MidpointRounding.AwayFromZero);
            }
            return null;
        }

        /// <summary>
        /// Checks if the provided WindData object contains any data.
        /// </summary>
        /// <param name="windData">The WindData object to check.</param>
        /// <returns>True if the WindData object is not null; otherwise, false.</returns>
        /// <remarks>
        /// This method is used to verify that a WindData object is not null before processing its contents.
        /// </remarks>
        public bool WindFormatterHasData(WindData windData) {
            return windData != null;
        }

        /// <summary>
        /// Rounds the wind direction to the nearest ten degrees.
        /// </summary>
        /// <param name="direction">The wind direction as a nullable double, measured in degrees.</param>
        /// <returns>The rounded direction in degrees as an integer, or null if the direction is not provided.</returns>
        /// <remarks>
        /// This method rounds a given wind direction to the nearest ten degrees, a common practice in meteorological reporting.
        /// </remarks>
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

        /// <summary>
        /// Parses a string to an integer after validating it with the ParsingValidator.
        /// </summary>
        /// <param name="valueToParse">The string value that needs to be parsed into an integer.</param>
        /// <param name="parameterName">The name of the parameter being parsed, used for error logging and exception messages.</param>
        /// <returns>The parsed integer value of the input string.</returns>
        /// <exception cref="ParsingException">Thrown when the string cannot be parsed into an integer.</exception>
        /// <remarks>
        /// This method ensures that the string is a valid integer before attempting to parse it.
        /// The ParsingValidator is used to validate the string, and if it fails to validate, a ParsingException is thrown.
        /// If the validation is successful, the method safely parses the string to an integer and returns it.
        /// </remarks>
        private int IntToStringParser(string valueToParse, string parameterName) {
            _parsingValidator.ValidateStringToInt(valueToParse, parameterName);
            return int.Parse(valueToParse); // Safe to parse here as validator has already checked.
        }
    }
}
