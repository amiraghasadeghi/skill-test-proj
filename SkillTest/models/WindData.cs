namespace Mma.Common.models
{
    public class WindData
    {
        public double? AverageWindSpeed { get; set; } // Value in knots

        public double? AverageWindDirection { get; set; } // Value in degrees

        public double? MaximumWindSpeed { get; set; } // Value in knots

        public double? MinimumWindDirection { get; set; } // Null if more than 360 deg

        public double? MaximumWindDirection { get; set; } // Null if more than 360 deg
    }
}
