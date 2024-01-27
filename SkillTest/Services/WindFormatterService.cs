using Mma.Common.Constants;
using Mma.Common.IServices;
using Mma.Common.models;
using Mma.Common.Utility;

namespace Mma.Common.Services {
    public class WindFormatterService : IWindFormatterService {
        private readonly IDataParser _windDataHelper;

        /// <summary>
        /// Initialises a new instance of the <see cref="WindFormatterService"/> class with a specified wind data helper.
        /// </summary>
        /// <param name="windDataHelper">A helper object for processing wind data.</param>
        public WindFormatterService(IDataParser windDataHelper) {
            _windDataHelper = windDataHelper;
        }

        /// <summary>
        /// Formats wind data into a standardised string format for reporting purposes.
        /// </summary>
        /// <param name="windData">Wind data to be formatted.</param>
        /// <returns>A string representing formatted wind data.</returns>
        public string FormatWind(WindData windData) {
            if (!_windDataHelper.WindFormatterHasData(windData))
                return WindConstants.Default;

            if (_windDataHelper.IsCalmWind(windData))
                return WindConstants.CalmWinds;

            var ddd = FormatAverageWindDirection(windData);
            var ff = FormatAverageWindSpeed(windData);
            var dnVdx = FormatForVariationInWindDirection(windData, ff);
            var gust = FormatGustSpeed(windData);
            var dnVdxAtLessThan3Knots = FormatVariationInWindDirectionForSpeedLessThan3(windData, ff);
            string directionComponent = DetectAppropriateDirectionComponentBasedOnSpeed(windData, dnVdxAtLessThan3Knots, ddd);

            return $"{directionComponent}{ff}{gust}{WindConstants.Knot}{dnVdx}";
        }

        /// <summary>
        /// Formats the average wind direction from wind data.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <returns>Formatted average wind direction.</returns>
        private string FormatAverageWindDirection(WindData windData) {
            return _windDataHelper.FormatWindDirection(windData.AverageWindDirection);
        }

        /// <summary>
        /// Formats the average wind speed from wind data.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <returns>Formatted average wind speed.</returns>
        private string FormatAverageWindSpeed(WindData windData) {
            return _windDataHelper.FormatWindSpeed(
                _windDataHelper.RoundWindSpeed(windData.AverageWindSpeed));
        }

        /// <summary>
        /// Formats the variation in wind direction if it varies significantly.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <param name="ff">Formatted average wind speed.</param>
        /// <returns>Formatted variation in wind direction.</returns>
        private string FormatForVariationInWindDirection(WindData windData, string ff) {
            return _windDataHelper.FormatVariationInDirectionForSpeedMoreThan3Knots(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
        }

        /// <summary>
        /// Formats the gust speed if it significantly exceeds the average wind speed.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <returns>Formatted gust speed, if applicable.</returns>
        private string FormatGustSpeed(WindData windData) {
            return _windDataHelper.FormatGustSpeed(
                _windDataHelper.RoundWindSpeedToTheNearestKnot(windData.AverageWindSpeed),
                _windDataHelper.RoundWindSpeedToTheNearestKnot(windData.MaximumWindSpeed));
        }

        /// <summary>
        /// Formats the variation in wind direction for wind speeds less than 3 knots.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <param name="ff">Formatted average wind speed.</param>
        /// <returns>Formatted variation in wind direction for low speeds.</returns>
        private string FormatVariationInWindDirectionForSpeedLessThan3(WindData windData, string ff) {
            return _windDataHelper.FormatVariationInDirectionForSpeedLessThan3Knots(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
        }

        /// <summary>
        /// Determines the appropriate direction component based on the wind speed.
        /// </summary>
        /// <param name="windData">The wind data to process.</param>
        /// <param name="dnVdxAtLessThan3Knots">Direction variation for speeds less than 3 knots.</param>
        /// <param name="ddd">Formatted average wind direction.</param>
        /// <returns>The appropriate wind direction component.</returns>
        private string DetectAppropriateDirectionComponentBasedOnSpeed(WindData windData, string dnVdxAtLessThan3Knots, string ddd) {
            return _windDataHelper.FormatWindDirectionVariationIsGreaterThan180(
                _windDataHelper.FormatWindDirection(windData.MinimumWindDirection),
                _windDataHelper.FormatWindDirection(windData.MaximumWindDirection),
                !string.IsNullOrEmpty(dnVdxAtLessThan3Knots) ? dnVdxAtLessThan3Knots : ddd);
        }
    }
}
