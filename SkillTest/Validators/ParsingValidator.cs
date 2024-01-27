using Mma.Common.Exceptions;
using Mma.Common.Interfaces;
using Mma.Common.IServices;
using System.Reflection;

namespace Mma.Common.Validators {
    public class ParsingValidator : IParsingValidator {
        private readonly ILoggingService _loggingService;
        public ParsingValidator(ILoggingService loggingService) {
            _loggingService = loggingService;
        }

        /// <summary>
        /// Validates the parsing of a string to an integer and throws an exception if parsing fails.
        /// </summary>
        /// <param name="valueToParse">The string value to parse as an integer.</param>
        /// <param name="parameterName">The name of the parameter being parsed, for use in the exception message.</param>
        /// <exception cref="ParsingException">Thrown when parsing fails.</exception>
        public void ValidateStringToInt(string valueToParse, string parameterName) {
            if (!int.TryParse(valueToParse, out _)) {
                _loggingService.LogError(MethodBase.GetCurrentMethod().Name, $"Failed to parse '{parameterName}' as an integer. Value: {valueToParse}");
                throw new ParsingException($"Failed to parse '{parameterName}' as an integer. Value: {valueToParse}");
            }
        }
    }
}
