using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.Exceptions {
    public class IntParseException : Exception {
        public const string DefaultMessage = "Error parsing integer";
        public string DefaultMessageWithParam { get; set; }

        public IntParseException(string message) : base(message) {
            DefaultMessageWithParam = message;
        }
    }
}
