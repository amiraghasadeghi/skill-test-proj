using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.IServices {
    public interface ILoggingService {
        void LogError(string functionName, string errorMessage);
    }
}
