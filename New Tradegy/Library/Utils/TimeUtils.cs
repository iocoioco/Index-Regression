using New_Tradegy.Library.UI.KeyBindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library.Utils
{
    internal class TimeUtils
    {
        public static string six_digit_integer_time_to_string_time(int value)
        {
            int sec = value % 100;
            int min = value % 10000 / 100;
            int hour = value / 10000;
            return hour + ":" + min + ":" + sec;
        }


        public static int time_to_int(string value)
        {
            string[] words = value.Split(':');
            return Convert.ToInt32(words[0]) * 10000 +
                Convert.ToInt32(words[1]) * 100 +
                Convert.ToInt32(words[2]);
        }
        public static double total_Seconds(int from, int to)
        {
            string string_type_from = six_digit_integer_time_to_string_time(from);
            string string_type_to = six_digit_integer_time_to_string_time(to);
            double total_seconds = DateTime.Parse(string_type_to).Subtract(DateTime.Parse(string_type_from)).TotalSeconds;
            return total_seconds;
        }
        public static double total_Seconds(string from, string to)
        {
            return DateTime.Parse(to).Subtract(DateTime.Parse(from)).TotalSeconds;
        }

        public static void MinuteAdvanceRetreat(int advance_lines)
        {
            if (advance_lines == 0)
            {
                g.Npts[1] = g.EndNptsBeforeExtend;
                g.EndNptsBeforeExtend = 0;
                g.EndNptsExtendedOrNot = false;
            }
            else
            {
                g.EndNptsBeforeExtend = g.Npts[1];
                g.Npts[1] += advance_lines; // expedientH
                if (g.Npts[1] > g.MAX_ROW)
                    g.Npts[1] = g.MAX_ROW;

                g.EndNptsExtendedOrNot = true;
            }
        }
    }
}
