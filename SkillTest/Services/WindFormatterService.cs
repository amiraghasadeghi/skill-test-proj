using System;
using System.Reflection;
using Mma.Common.IHelpers;
using Mma.Common.IServices;
using Mma.Common.models;
using NLog;

namespace Mma.Common.Services {
    public class WindFormatterService : IWindFormatterService {
        private readonly IWindDataHelper _windDataHelper;
        public WindFormatterService(IWindDataHelper windDataHelper) {
            _windDataHelper = windDataHelper;
        }
        public string FormatWind(WindData windData) {
            if (!_windDataHelper.WindFormatterHasData(windData)) {
                return "/////KT"; // Return default value if no data
            }

            if (IsCalmWind(windData)) {
                return "00000KT"; // Calm wind
            }

            var ddd = _windDataHelper.FormatWindDirection(windData.AverageWindDirection);
            
            var ff = _windDataHelper.FormatWindSpeed(_windDataHelper.RoundWindSpeed(windData.AverageWindSpeed));
           
            var dnVdx = _windDataHelper.FormatVariationInDirectionIfVariant(_windDataHelper.FormatWindDirection(windData.MinimumWindDirection), _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
            
            var gust = _windDataHelper.FormatGust(_windDataHelper.RoundWindSpeedToTheNearestKnot(windData.AverageWindSpeed), _windDataHelper.RoundWindSpeedToTheNearestKnot(windData.MaximumWindSpeed));
            
            var dnVdxAtLessThan3Knots = _windDataHelper.IsVariationInWindDirectionAndLessThan3Knots(_windDataHelper.FormatWindDirection(windData.MinimumWindDirection), _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), ff);
            // If dbVdxAtLessThan3Knots has a value, use it instead of ddd
            string directionComponent = _windDataHelper.WindDirectionVariationIsGreaterThan180(_windDataHelper.FormatWindDirection(windData.MinimumWindDirection), _windDataHelper.FormatWindDirection(windData.MaximumWindDirection), !string.IsNullOrEmpty(dnVdxAtLessThan3Knots) ? dnVdxAtLessThan3Knots : ddd);
           
            return $"{directionComponent}{ff}{gust}KT{dnVdx}";
        }

        public bool IsCalmWind(WindData windData) {
            return windData.AverageWindSpeed.HasValue && windData.AverageWindSpeed <= 1;
        }

    }
}
