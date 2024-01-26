using Mma.Common.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mma.Common.IServices
{
    public interface IWindFormatterService
    {
        string FormatWind(WindData windData);
        bool IsCalmWind(WindData windData);
    }
}
