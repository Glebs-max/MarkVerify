using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LabelDesigner.Services
{
    public static class UnitsService
    {
        public static double ToMM(double? value) => value * 0.2646 ?? 0;
        public static double FromMM(double? value) => value / 0.2646 ?? 0;
    }
}
