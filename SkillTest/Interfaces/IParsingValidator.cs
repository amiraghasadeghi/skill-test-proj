using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.Interfaces {
    public interface IParsingValidator {
        void ValidateStringToInt(string valueToParse, string parameterName);
    }
}
