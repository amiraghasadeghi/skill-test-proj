using Mma.Common.models;

namespace Mma.Common.Utility {
    public interface IDataParser {
        int? RoundWindSpeedToTheNearestKnot(double? windSpeed);
        int? RoundDirectionToNearestTenDegrees(double? direction);
        bool WindFormatterHasData(WindData windData);
        bool IsCalmWind(WindData windData);
        bool SurfaceWindDirectionIsInRange(string ddd);
        bool SurfaceWindSpeedIsGreaterThan1(string ff);
        string FormatVariationInDirectionForSpeedLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string FormatWindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection);
        string RoundWindSpeed(double? windSpeed);
        string FormatWindSpeed(string ff);
        string FormatVariationInDirectionForSpeedMoreThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string FormatGustSpeed(double? averageWindSpeed, double? maximumWindSpeed);
        string FormatWindDirection(double? windDirection);
    }
}
