using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
 

    public class ChartHandler
    {
        private Chart _chart;

        public ChartHandler(Chart chart)
        {
            _chart = chart;
        }

        public void Clear()
        {
            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Annotations.Clear();
        }

        // You can add more methods later, like UpdateChart(), DrawLine(), etc.
    }

}
