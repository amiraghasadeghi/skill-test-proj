using Mma.Common.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.IServices
{
    public interface IWindFormatterService
    {
        string FormatWind(WindData windData);
        int? RoundWindSpeedToTheNearestKnot(double? windSpeed);
        bool WindFormatterHasData(WindData windData);
        int? RoundDirectionToNearestTenDegrees(double? direction);
        bool SurfaceWindDirectionIsInRange(string ddd);
        bool SurfaceWindSpeedIsGreaterThan1(string ff);
        string FormatWindSpeed(string ff);
        string FormatVariationInDirection(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string IsVariationInWindDirectionAndLessThan3Knots(string minWindDirection, string maxWindDirection, string averageWindSpeed);
        string FormatGust(double? averageWindSpeed, double? maximumWindSpeed);
        string WindDirectionVariationIsGreaterThan180(string minWindDirection, string maxWindDirection, string averageWindDirection);
    }
}
