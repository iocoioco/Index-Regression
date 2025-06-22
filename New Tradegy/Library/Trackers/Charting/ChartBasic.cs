using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    internal class ChartBasic
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

        public static void RemoveChartBlock(Chart chart, string stockName)
        {
            // Remove related series (Name starts with stockName)
            var seriesToRemove = chart.Series
                .Where(s => s.ChartArea == stockName || s.Name.StartsWith(stockName + " "))
                .ToList();

            foreach (var s in seriesToRemove)
                chart.Series.Remove(s);

            // Remove chart area (Name == stockName)
            var area = chart.ChartAreas.FindByName(stockName);
            if (area != null)
                chart.ChartAreas.Remove(area);
        }
    }

    

}
