using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    public class IndexRangeTrack
    {
        // usage for Kospi, Kosdaq
        // private IndexRangeTrack kospiTracker;
        // IndexRangeCheck kospiTracker = new IndexRangeTrack();

        private string currentRangeString = "00000"; // Track the last range

        public void CheckIndexAndSound(int indexPrice, string market)
        {
            string rangeString = GetRangeString(indexPrice);

            if (currentRangeString != rangeString) // Play sound only if range changes
            {
                currentRangeString = rangeString;
                if (market == "Kospi")
                    rangeString = "p" + rangeString;
                else
                    rangeString = "q" + rangeString;

                mc.Sound("코스피 코스닥", rangeString);
            }
        }

        private string GetRangeString(int value)
        {
            if (value >= -9 && value <= 9)
            {
                if (value < 0)
                    return "-0";
                else
                    return "+0";
            }
            else
                return (value / 10).ToString();
        }
    }
}
