using Mma.Common.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mma.Common.Constants {
    /// <summary>
    /// Class representing wind constants and their associated operations.
    /// </summary>
    public class WindConstants {
        private readonly ILoggingService _loggingService;

        // Constants representing different wind speeds and conditions.
        public const string CalmWinds = "00000KT";
        public const string Default = "/////KT";
        public const string HundredAndOver = "P99";
        public const string Gust = "G";
        public const string Knot = "KT";
        public const string MissingSpeed = "//";
        public const string MissingDirection = "///";
        public const string VariableSpeed = "VRB";

        /// <summary>
        /// Initialises a new instance of the <see cref="WindConstants"/> class.
        /// </summary>
        /// <param name="loggingService">Logging service for error logging.</param>
        public WindConstants(ILoggingService loggingService) {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        // Dictionary mapping wind constant values to their descriptions.
        private static readonly Dictionary<string, string> ValuesAndDescriptions = new Dictionary<string, string> {
            {CalmWinds ,"Calm winds"},
            {Default,"Default value"},
            {HundredAndOver,"Hundred and over"},
            {Gust,"Gust"},
            {Knot,"Knot"},
            {MissingSpeed,"//"},
            {MissingDirection,"///"},
            {VariableSpeed,"VBR"},
        };

        // Reverse dictionary to map descriptions back to their constant values.
        private readonly Dictionary<string, string> DescriptionsAndValues =
            ValuesAndDescriptions.ToDictionary(pair => pair.Value, pair => pair.Key);

        /// <summary>
        /// Gets the description of a wind constant.
        /// </summary>
        /// <param name="value">The wind constant value.</param>
        /// <returns>The description of the wind constant if found; otherwise, an error message.</returns>
        public string GetWindConstantDescription(string value) {
            if (string.IsNullOrEmpty(value)) return "Invalid input";

            try {
                return ValuesAndDescriptions.TryGetValue(value, out var description) ? description : "Description not found";
            } catch (Exception ex) {
                _loggingService.LogError(MethodBase.GetCurrentMethod().Name, ex.Message, ex);
                return "Error processing request";
            }
        }

        /// <summary>
        /// Gets the constant value for a given wind description.
        /// </summary>
        /// <param name="description">The description of the wind constant.</param>
        /// <returns>The constant value if found; otherwise, an error message.</returns>
        public string GetWindConstantValue(string description) {
            if (string.IsNullOrEmpty(description)) {
                return "Invalid input";
            }

            try {
                return DescriptionsAndValues.TryGetValue(description, out var value) ? value : "Value not found";
            } catch (Exception ex) {
                _loggingService.LogError(MethodBase.GetCurrentMethod().Name, ExceptionConstants.UnhandledStamp, ex);
                return ExceptionConstants.DefaultParsingError;
            }
        }
    }
}
