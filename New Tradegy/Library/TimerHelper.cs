using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    public class TimerHelper
    {
        private DateTime _savedTime; // Stores the start time

        public void Start()
        {
            _savedTime = DateTime.Now; // Start the timer
        }

        public double Stop()
        {
            return (DateTime.Now - _savedTime).TotalMilliseconds; // Return elapsed time
        }
    }
}
