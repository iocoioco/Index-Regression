using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;



namespace New_Tradegy.Library.UI.ClickHandlers
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

            public static Form GetActiveForm()
            {
                var active = Form.ActiveForm;
                if (active != null) return active;

                foreach (Form f in Application.OpenForms)
                {
                    if (f.IsMdiContainer && f.ActiveMdiChild != null)
                        return f.ActiveMdiChild;
                }
                return null;
            }

            private static int RetryGetPrice(string stock, int maxTries, int delayMs)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    int price = hg.HogaGetValue(stock, -1, 1);
                    if (price > 0) return price;
                    System.Threading.Thread.Sleep(delayMs);
                }
                return -1;
            }

        public static string CoordinateMapping(Chart chart, int nRow, int nCol, List<string> displayList, MouseEventArgs e, ref string selection, ref int cellX, ref int cellY)
        {
            double x_max = chart.Bounds.Width;
            double y_max = chart.Bounds.Height;

            // Normalize click position to 0-100 coordinate system
            double norm_x = (double)e.X / x_max * 100.0;
            double norm_y = (double)e.Y / y_max * 100.0;

            // Clamp to bounds
            norm_x = Math.Max(0, Math.Min(100, norm_x));
            norm_y = Math.Max(0, Math.Min(100, norm_y));

            // Determine cell width and height
            double cellWidth = 100.0 / nCol;
            double cellHeight = 100.0 / nRow;

            if (chart.Name == "chart1")
            {
                if (cellX == 0)
                    cellHeight = 100.0 / 2.0;
                else
                    cellHeight = 100.0 / nRow;
            }

            cellX = (int)(norm_x / cellWidth);
            cellY = (int)(norm_y / cellHeight);

            // Clamp grid indices
            cellX = Math.Min(cellX, nCol - 1);
            cellY = Math.Min(cellY, nRow - 1);

            // Determine selection zone (1–9 style keypad mapping)
            double pctX = (norm_x % cellWidth) / cellWidth * 100.0;
            double pctY = (norm_y % cellHeight) / cellHeight * 100.0;

            int[,] zoneMap = new int[3, 3] {
                { 3, 6, 9 },
                { 2, 5, 8 },
                { 1, 4, 7 }
            };

            int zoneX = pctX > 66.66 ? 2 : pctX > 33.33 ? 1 : 0;
            int zoneY = pctY < 33.33 ? 0 : pctY < 66.66 ? 1 : 2;

            int finalAddress = zoneMap[zoneY, zoneX];
            selection = (e.Button == MouseButtons.Left ? "l" : "r") + finalAddress.ToString();

            // Resolve clicked stock
            string clickedStock = null;

            if (chart.Name == "chart1")
            {
                if (g.q == "h&s")
                {
                    clickedStock = g.clickedStock;
                }
                else
                {
                    if (cellX == 0)
                    {
                        clickedStock = cellY == 0 ? g.StockManager.IndexList[0] : g.StockManager.IndexList[2];
                    }
                    else if (cellX > 1)
                    {
                        int sequence = (cellX - 2) * nRow + cellY;
                        if (sequence >= 0 && sequence < displayList.Count)
                            clickedStock = displayList[sequence];
                    }
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
                    int index = nRow * cellX + cellY;
                    if (index >= 0 && index < displayList.Count)
                        clickedStock = displayList[index];
                }
            }

            return clickedStock;
        }
    }
}
