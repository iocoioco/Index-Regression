using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Regression
{
    public class IndexMinutePoint
    {
        public string HHmm;       // e.g., "0900"
        public double PriceDiff; // difference between HHmm and previous minute
    }
    internal class IndexMinuteReader
    {

        public static List<IndexMinutePoint> GetValidMinuteChanges(string filePath)
        {
            var points = new List<IndexMinutePoint>();

            if (!File.Exists(filePath)) return points;

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return points;

            string prevTime = null;
            double prevPrice = 0;

            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length < 2) continue;

                string rawTime = words[0].PadLeft(6, '0'); // ensure 6-digit
                if (rawTime.Length != 6) continue;

                string hhmm = rawTime.Substring(0, 4);
                if (!double.TryParse(words[1], out double currentPrice)) continue;

                if (prevTime != null)
                {
                    int deltaSec = TimeDiffInSeconds(rawTime, prevTime);
                    if (deltaSec > 50 && deltaSec < 70)
                    {
                        points.Add(new IndexMinutePoint
                        {
                            HHmm = hhmm,
                            PriceDiff = currentPrice - prevPrice
                        });
                    }
                }

                prevTime = rawTime;
                prevPrice = currentPrice;
            }

            return points;
        }

        private static int TimeDiffInSeconds(string time1, string time2)
        {
            time1 = time1.PadLeft(6, '0');
            time2 = time2.PadLeft(6, '0');

            int h1 = int.Parse(time1.Substring(0, 2));
            int m1 = int.Parse(time1.Substring(2, 2));
            int s1 = int.Parse(time1.Substring(4, 2));

            int h2 = int.Parse(time2.Substring(0, 2));
            int m2 = int.Parse(time2.Substring(2, 2));
            int s2 = int.Parse(time2.Substring(4, 2));

            var t1 = new TimeSpan(0, h1, m1, s1);
            var t2 = new TimeSpan(0, h2, m2, s2);
            return (int)Math.Abs((t1 - t2).TotalSeconds);
        }
    }
}

