using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Trackers
{
    public class IndexRangeTracker
    {
        private int lastIndexKospi = 0;
        private int lastIndexKosdaq = 0;

        public void CheckIndexAndSound(int indexPrice, string market)
        {
            int lastIndex = (market == "Kospi") ? lastIndexKospi : lastIndexKosdaq;
            int delta = indexPrice - lastIndex;
            int steps = Math.Abs(delta / 10); // number of "deng" or "dong" to play

            if (steps > 0)
            {
                string sound = delta > 0 ? "up" : "dn";

                for (int i = 0; i < steps; i++)
                {
                    Utils.SoundUtils.Sound("코스피 코스닥", sound);
                }

                // Update last index
                if (market == "Kospi")
                    lastIndexKospi = indexPrice;
                else
                    lastIndexKosdaq = indexPrice;
            }
        }

        // usage for Kospi, Kosdaq
        // private IndexRangeTrack kospiTracker;
        // IndexRangeCheck kospiTracker = new IndexRangeTrack();

        #region
        //private string currentRangeKospi = "00000"; // Track the last range for Kospi
        //private string currentRangeKosdaq = "00000"; // Track the last range for Kosdaq

        //public void CheckIndexAndSound(int indexPrice, string market)
        //{
        //    string rangeString = GetRangeString(indexPrice);
        //    string currentRange = (market == "Kospi") ? currentRangeKospi : currentRangeKosdaq;

        //    if (currentRange != rangeString) // Play sound only if range changes
        //    {
        //        if (market == "Kospi")
        //        {
        //            currentRangeKospi = rangeString;
        //            rangeString = indexPrice > 0 ? "p+" + rangeString : "p" + rangeString;
        //        }
        //        else
        //        {
        //            currentRangeKosdaq = rangeString;
        //            rangeString = indexPrice > 0 ? "+" + rangeString : rangeString;
        //        }

        //        Utils.SoundUtils.Sound("코스피 코스닥", rangeString);
        //    }
        //}

        //private string GetRangeString(int value)
        //{
        //    if (value >= -9 && value <= 9)
        //    {
        //        return value < 0 ? "-0" : "+0";
        //    }
        //    else
        //    {
        //        return (value / 10).ToString();
        //    }
        //}
        #endregion
    }
}
