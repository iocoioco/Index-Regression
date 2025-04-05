using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    public class ChartManager
    {
        // Chart references
        public Chart Chart1 { get; private set; }
        public Chart Chart2 { get; private set; }

        public ChartHandler Chart1Handler { get; private set; }
        public ChartHandler Chart2Handler { get; private set; }



        public ChartManager() { }

        // Constructor
        public void SetChart1(Chart chart)
        {
            Chart1 = chart;
            Chart1Handler = new ChartHandler(chart); // ✅ initialize handler here
        }

        public void SetChart2(Chart chart)
        {
            Chart2 = chart;
            Chart2Handler = new ChartHandler(chart); // ✅ initialize handler here
        }

       

        public void ClearAll()
        {
            Chart1Handler.Clear();
            Chart2Handler.Clear();
        }







        private void SetupChart(Chart chart)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(new ChartArea("DefaultArea"));
            // Add more initial setup as needed
        }

        // ================================
        // 2. Drawing / Updating
        // ================================
        public void DrawStockLine(Chart chart, string stockCode, List<double> prices)
        {
            var series = new Series(stockCode)
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2
            };

            for (int i = 0; i < prices.Count; i++)
            {
                series.Points.AddXY(i, prices[i]);
            }

            chart.Series.Add(series);
        }

        public void UpdatePriceLine(Chart chart, string stockCode, double newPrice)
        {
            var series = chart.Series
                              .FirstOrDefault(s => s.Name.Contains(stockCode));

            if (series != null)
            {
                series.Points.AddY(newPrice);
            }
        }

        // ================================
        // 3. Styling
        // ================================
        public void SetDarkTheme(Chart chart)
        {
            chart.BackColor = System.Drawing.Color.Black;
            foreach (var area in chart.ChartAreas)
            {
                area.BackColor = System.Drawing.Color.Black;
                area.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
                area.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            }
        }

        // ================================
        // 4. Interaction
        // ================================
        public void HighlightStock(Chart chart, string stockCode)
        {
            //if (chart.Series.Contains(stockCode))
            //{
            //    chart.Series[stockCode].BorderWidth = 4;
            //    chart.Series[stockCode].Color = System.Drawing.Color.Red;
            //}
        }

        // ================================
        // 5. Clearing / Resetting
        // ================================


    }
}
