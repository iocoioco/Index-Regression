using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    internal class ChartBasicRenderer
    {
        public static void RelocateChartArea(ChartArea area, int row, int col, int nRow, int nCol)
        {
            float width = 100f / nCol;
            float height = 100f / nRow;

            area.Position.X = col * width;
            area.Position.Y = row * height;
            area.Position.Width = width;
            area.Position.Height = height;
        }
    }
}
