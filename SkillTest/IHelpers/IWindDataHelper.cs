using Mma.Common.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.IHelpers {
    public interface IWindDataHelper {
        int? RoundWindSpeedToTheNearestKnot(double? windSpeed);
        bool WindFormatterHasData(WindData windData);
        int? RoundDirectionToNearestTenDegrees(double? direction);
        bool SurfaceWindDirectionIsInRange(string ddd);
        bool SurfaceWindSpeedIsGreaterThan1(string ff);
        string IsVariationInWindDirectionAndLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string WindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection);
        string RoundWindSpeed(double? windSpeed);
        string FormatWindSpeed(string ff);
        string FormatVariationInDirectionIfVariant(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string FormatGust(double? averageWindSpeed, double? maximumWindSpeed);
        string FormatWindDirection(double? windDirection);
    }
}
