using System;

namespace Mma.Common.Exceptions {
    public class ParsingException : Exception {

        /// <summary>
        /// Gets the name of the parameter that caused the current exception.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Initialises a new instance of the ParsingException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParsingException(string message) : base(message) {
            ParameterName = message;
        }
    }
}
