using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;



namespace New_Tradegy.Library.UI.ChartClickHandlers
{
    internal class ChartClickMapper
    {
        private static CPUTILLib.CpCybos _cpcybos;

        public static string GetClickedStockFromChart(Chart chart, int rows, int columns, List<string> displayList, MouseEventArgs e, ref string selection, ref int cellX, ref int cellY)
        {
            double width = chart.Bounds.Width;
            double height = chart.Bounds.Height;

            double normX = (e.X / width) * 100.0;
            double normY = (e.Y / height) * 100.0;

            normX = Math.Max(0, Math.Min(100, normX));
            normY = Math.Max(0, Math.Min(100, normY));


            double cellWidth = 100.0 / columns;
            cellX = (int)(normX / cellWidth);

            double cellHeight = 100.0 / rows;
            if (chart.Name == "chart1" && cellX == 0)
                cellHeight = 100.0 / 2.0;

            cellY = (int)(normY / cellHeight);

            cellX = Math.Min(cellX, columns - 1);
            cellY = Math.Min(cellY, rows - 1);

            double percentX = (normX % cellWidth) / cellWidth * 100.0;
            double percentY = (normY % cellHeight) / cellHeight * 100.0;

            selection = e.Button == MouseButtons.Left ? "l" : "r";

            int section = 9;

            if (percentX > 66.66)
            {
                if (percentY < 33.33)
                    section = 1;
                else if (percentY < 66.66)
                    section = 4;
                else
                    section = 7;
            }
            else if (percentX > 33.33)
            {
                if (percentY < 33.33)
                    section = 2;
                else if (percentY < 66.66)
                    section = 5;
                else
                    section = 8;
            }
            else
            {
                if (percentY < 33.33)
                    section = 3;
                else if (percentY < 66.66)
                    section = 6;
            }


            selection += section.ToString();

            string clickedStock = null;
            if (chart.Name == "chart1")
            {
                if (g.q == "h&s") return g.clickedStock;
                if (cellX == 0)
                    clickedStock = cellY == 0 ? g.StockManager.IndexList[0] : g.StockManager.IndexList[2];
                else if (cellX >= 2)
                {
                    int seq = (cellX - 2) * rows + cellY;
                    if (seq < displayList.Count)
                        clickedStock = displayList[seq];
                }
            }
            else
            {
                if (g.q == "h&s") return g.clickedStock;
                int seq = rows * cellX + cellY;
                if (seq < displayList.Count)
                    clickedStock = displayList[seq];
            }

            return clickedStock;
        }



        public static string CoordinateMapping(Chart chart, int nRow, int nCol, List<string> displayList, MouseEventArgs e, ref string selection, ref int cellX, ref int cellY)
        {
            double x_max = chart.Bounds.Width;
            double y_max = chart.Bounds.Height;

            // Normalize to 0–100 coordinates
            double norm_x = Math.Max(0, Math.Min(100, (double)e.X / x_max * 100.0));
            double norm_y = Math.Max(0, Math.Min(100, (double)e.Y / y_max * 100.0));

            // Base cell size
            double cellWidth = 100.0 / nCol;
            double cellHeight = 100.0 / nRow;

            // Special case for chart1: left 2 columns fixed
            if (chart.Name == "chart1" && cellX == 0)
                cellHeight = 100.0 / 2.0;

            // Determine cellX/Y index
            int rawX = (int)(norm_x / cellWidth);
            int rawY = (int)(norm_y / cellHeight);
            rawX = Math.Min(rawX, nCol - 1);
            rawY = Math.Min(rawY, nRow - 1);

            cellX = rawX;
            cellY = rawY;

            // Determine 3x3 zone within cell
            double pctX = (norm_x % cellWidth) / cellWidth * 100.0;
            double pctY = (norm_y % cellHeight) / cellHeight * 100.0;

            int[,] zoneMap = new int[3, 3] {
        { 3, 2, 1 },
        { 6, 5, 4 },
        { 9, 8, 7 }
    };

            int zoneX = pctX > 66.66 ? 2 : pctX > 33.33 ? 1 : 0;
            int zoneY = pctY < 33.33 ? 0 : pctY < 66.66 ? 1 : 2;
            int finalZone = zoneMap[zoneY, zoneX];

            selection = (e.Button == MouseButtons.Left ? "l" : "r") + finalZone.ToString();

            // Resolve clicked stock
            string clickedStock = null;

            if (chart.Name == "chart1")
            {
                if (g.q == "h&s")
                {
                    clickedStock = g.clickedStock;
                }
                else if (cellX == 0)
                {
                    clickedStock = cellY == 0 ? g.StockManager.IndexList[0] : g.StockManager.IndexList[2];
                }
                else if (cellX > 1)
                {
                    int seq = (cellX - 2) * nRow + cellY;
                    if (seq >= 0 && seq < displayList.Count)
                        clickedStock = displayList[seq];
                }
            }
            else // chart2
            {
                if (g.q == "h&s")
                {
                    clickedStock = g.clickedStock;
                }
                else
                {
                    int seq = cellX * nRow + cellY;
                    if (seq >= 0 && seq < displayList.Count)
                        clickedStock = displayList[seq];
                }
            }

            return clickedStock;
        }

    }
}
