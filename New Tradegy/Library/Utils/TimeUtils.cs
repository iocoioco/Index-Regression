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
        public static int ElapsedMillisecondsInteger(long t1, long t2)
        {
            // Parse long time (HHmmssfff) to DateTime
            DateTime Parse(long t)
            {
                int hour = (int)(t / 10000000);
                int minute = (int)((t / 100000) % 100);
                int second = (int)((t / 1000) % 100);
                int millisecond = (int)(t % 1000);
                return new DateTime(1, 1, 1, hour, minute, second, millisecond);
            }

            var dt1 = Parse(t1);
            var dt2 = Parse(t2);

            return (int)Math.Abs((dt2 - dt1).TotalMilliseconds);
        }

        public static double ElapsedMillisecondsDouble(long t1, long t2)
        {
            int h1 = (int)(t1 / 10000000);
            int m1 = (int)((t1 / 100000) % 100);
            int s1 = (int)((t1 / 1000) % 100);
            int f1 = (int)(t1 % 1000);

            int h2 = (int)(t2 / 10000000);
            int m2 = (int)((t2 / 100000) % 100);
            int s2 = (int)((t2 / 1000) % 100);
            int f2 = (int)(t2 % 1000);

            TimeSpan time1 = new TimeSpan(0, h1, m1, s1, f1);
            TimeSpan time2 = new TimeSpan(0, h2, m2, s2, f2);

            return (time1 - time2).TotalMilliseconds;
        }



        public static int TimeToInt(string value)
        {
            string[] words = value.Split(':');
            return Convert.ToInt32(words[0]) * 10000 +
                Convert.ToInt32(words[1]) * 100 +
                Convert.ToInt32(words[2]);
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
