using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Utils
{

    public static class MathUtils
    {
        public static bool IsSafeToDivide(double val)
        {
            return Math.Abs(val) > 0.0000001; // zero divider protector;
        }

        public static string SafePercentage(double up, double dn)
        {
            return IsSafeToDivide(dn)
                ? ((up - dn) / dn * 100.0).ToString("0.##")
                : "--";
        }
    }
}



