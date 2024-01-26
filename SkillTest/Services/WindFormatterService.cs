using Mma.Common.Constants;
using Mma.Common.Exceptions;
using Mma.Common.IServices;
using Mma.Common.models;
using Mma.Common.Utility;
using System;

namespace Mma.Common.Services {
    /// <summary>
    /// Service for formatting wind data.
    /// </summary>
    public class WindFormatterService : IWindFormatterService {
        private readonly IDataParser _windDataHelper;

        /// <summary>
        /// Initializes a new instance of the WindFormatterService.
        /// </summary>
        /// <param name="windDataHelper">The wind data helper to process wind data.</param>
        public WindFormatterService(IDataParser windDataHelper) {
            _windDataHelper = windDataHelper;
        }

        /// <summary>
        /// Formats the wind data into a specific string format.
        /// </summary>
        /// <param name="windData">The wind data to format.</param>
        /// <returns>Formatted wind data as a string.</returns>
        public string FormatWind(WindData windData) {
            // Return default value if no data is present.
            if (!_windDataHelper.WindFormatterHasData(windData)) return WindConstants.Default;
            // Check for calm wind condition and return accordingly.
            if (_windDataHelper.IsCalmWind(windData)) return WindConstants.CalmWinds;

            // Formatting individual components of wind data.
            var ddd = FormatAverageWindDirection(windData);
            var ff = FormatAverageWindSpeed(windData);
            var dnVdx = FormatForVariationInWindDirection(windData, ff);
            var gust = FormatGustSpeed(windData);
            var dnVdxAtLessThan3Knots = FormatVariationInWindDirectionForSpeedLessThan3(windData, ff);
            // Selecting the appropriate direction component based on the wind speed.
            string directionComponent = DetectAppropriateDirectionComponentBasedOnSpeed(windData, dnVdxAtLessThan3Knots, ddd);

            // Compiling the formatted wind data string.
            return $"{directionComponent}{ff}{gust}{WindConstants.Knot}{dnVdx}";
        }

        private string FormatAverageWindDirection(WindData windData) {
            return _windDataHelper.FormatWindDirection(windData.AverageWindDirection);
        }

        private string FormatAverageWindSpeed(WindData windData) {
            try {
                return _windDataHelper.FormatWindSpeed(
                _windDataHelper.RoundWindSpeed(windData.AverageWindSpeed));
            } catch (Exception ex) {
                if (ex is IntParseException) {
                    return "ERR";
                } else {
                    return "ERR";
                }
            }
        }

        private string FormatForVariationInWindDirection(WindData windData, string ff) {
            return _windDataHelper.FormatVariationInDirectionIfVariant(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
        }

        private string FormatGustSpeed(WindData windData) {
            return _windDataHelper.FormatGustSpeed(
                _windDataHelper.RoundWindSpeedToTheNearestKnot(windData.AverageWindSpeed),
                _windDataHelper.RoundWindSpeedToTheNearestKnot(windData.MaximumWindSpeed));
        }

        private string FormatVariationInWindDirectionForSpeedLessThan3(WindData windData, string ff) {
            return _windDataHelper.FormatVariationInDirectionForSpeedLessThan3Knots(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
        }

        private string DetectAppropriateDirectionComponentBasedOnSpeed(WindData windData, string dnVdxAtLessThan3Knots, string ddd) {
            return _windDataHelper.FormatWindDirectionVariationIsGreaterThan180(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection),
                !string.IsNullOrEmpty(dnVdxAtLessThan3Knots) ? dnVdxAtLessThan3Knots : ddd);
        }

    }
}
