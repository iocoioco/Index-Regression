using New_Tradegy.Library.Listeners;
using New_Tradegy.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    public static class ChartIndex
    {
        private static Color[] colorKODEX = { Color.White, Color.Red,
        Color.White, Color.Black, Color.Cyan, Color.Magenta, Color.Green,
        Color.White, Color.White, Color.White, Color.Blue, Color.Brown };

        //How to call
        //ChartIndex.UpdateIndexChart(g.chart1);
        //ChartIndex.UpdateIndexChart(g.chart2, isChart1: false);
        public static ChartArea UpdateChartArea(Chart chart, StockData data)
        {
            string areaName = data.Stock;
            ChartArea area;

            if (chart.ChartAreas.IndexOf(areaName) >= 0 && !data.Misc.CreateNewChartArea)
            {
                area = chart.ChartAreas[areaName];
                if (data != null && data.Api != null && data.Api.nrow > 0)
                    UpdateSeries(chart, data);
            }
            // Generate a new chartarea
            else
            {
                ChartBasic.RemoveChartBlock(chart, data.Stock); 
                area = CreateChartArea(chart, data);
                if (data.Misc.CreateNewChartArea)
                    data.Misc.CreateNewChartArea = false;
            }
            return area;
        }

        public static ChartArea CreateChartArea(Chart chart, StockData data)
        {
            string stock = data.Stock;
            string chartname = chart.Name; //?
            if (data.Api.nrow <= 1)
                return null;


            // Determine Start and End Row
            int start = 0;
            int end = g.test ? g.Npts[1] : data.Api.nrow;
            if (data.Misc.ShrinkDraw)
                start = Math.Max(end - g.NptsForShrinkDraw, g.Npts[0]);




            ChartArea area = new ChartArea(stock); 
            chart.ChartAreas.Add(area);
            area.Visible = false;




     
            int[] ids = { 1, 3, 4, 5, 6, 10, 11 };

            foreach (int id in ids)
            {
                string seriesName = stock + " " + id;
                double mag = 1.0;
                Magnifier(data.Stock, id, ref mag);

                Series series = new Series(seriesName)
                {
                    ChartArea = area.Name,
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.Date,
                    IsVisibleInLegend = false,
                    Color = colorKODEX[id],
                    BorderWidth = 2
                };
                chart.Series.Add(series);

                for (int i = start; i < end; i++)
                {
                    if (data.Api.x[i, 0] == 0) break;
                    int val =  (int)(data.Api.x[i, id] * mag);
                    series.Points.AddXY((data.Api.x[i, 0] / g.HUNDRED).ToString(), val);

                    data.Misc.y_min = Math.Min(data.Misc.y_min, val);
                    data.Misc.y_max = Math.Max(data.Misc.y_max, val);
                }

                if (series.Points.Count < 2)
                {
                    chart.Series.Remove(series);
                    continue;
                }

                Mark(chart, start + 1, series);
                Label(chart, series);
            }

            area.AxisY.LabelStyle.Enabled = false;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.MajorTickMark.Enabled = false;
            area.AxisY.MinorTickMark.Enabled = false;
            area.AxisY.MinorGrid.Enabled = false;

            double pad = (data.Misc.y_max - data.Misc.y_min) * 0.05;
            area.AxisY.Minimum = data.Misc.y_min - pad;
            area.AxisY.Maximum = data.Misc.y_max + pad;

            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = end - start;
            area.AxisX.IntervalOffset = 1;

            area.InnerPlotPosition = new ElementPosition(5, 5, 70, 90);
            area.AxisX.LabelStyle.Font = new Font("Arial", 7);
            area.AxisY.LabelStyle.Font = new Font("Arial", 7);

            int r = 255, b = 255;
            int diff = data.Post.분당가격차;
            if (diff > 35) r = 150;
            else if (diff > 28) r = 170;
            else if (diff > 21) r = 190;
            else if (diff > 14) r = 210;
            else if (diff > 7) r = 230;
            else if (diff > -7) { }
            else if (diff > -14) b = 230;
            else if (diff > -21) b = 210;
            else if (diff > -28) b = 190;
            else if (diff > -35) b = 170;
            else b = 150;

            area.BackColor = Color.FromArgb(b, r, 255);

            // Positioning
            if (chart == g.ChartManager.Chart1)
            {
                float width = 100 / 10f;
                area.Position = stock == "KODEX 레버리지"
                    ? new ElementPosition(0, 0, width, 50)
                    : new ElementPosition(0, 50, width, 50);
            }
            else
            {
                float w = 100 / 5f, h = 100 / 3f;
                area.Position = stock == "KODEX 레버리지"
                    ? new ElementPosition(0, 0, w, h)
                    : new ElementPosition(0, h, w, h);
            }

            return area;
        }

        private static int PointValueIndex(StockData data, int k, int id)
        {
            double magnifier = 1.0;
            Magnifier(data.Stock, id, ref magnifier);

            return (int)(data.Api.x[k, id] * magnifier);
        }

        // Index stock version: full chart update per call
        public static void UpdateSeries(Chart chart, StockData data)
        {
            int last = g.test ? g.Npts[1] - 1 : data.Api.nrow - 1;

            string stock = data.Stock;

            if (last < 0 || data.Api.x[last, 0] == 0)
                return;

            string xLabel = ((int)(data.Api.x[last, 0] / g.HUNDRED)).ToString();

            int[] seriesIds = { 1, 3, 4, 5, 6, 10, 11 };

            foreach (int typeId in seriesIds)
            {
                string seriesName = $"{stock} {typeId}";

                if (chart.Series.IsUniqueName(seriesName))
                    continue;

                var series = chart.Series[seriesName];
                int value = PointValueIndex(data, last, typeId);

                // Always clear the previous label
                int oldIndex = series.Points.Count - 1;
                if (oldIndex >= 0)
                    series.Points[oldIndex].Label = "";

                // Case 1: Update end point
                if (series.Points.Count > 0 && series.Points.Last().AxisLabel == xLabel)
                {
                    series.Points.Last().YValues[0] = value;
                }
                else // Case 2: Add new point
                {
                    series.Points.AddXY(xLabel, value);

                    if (value < data.Misc.y_min)
                        data.Misc.y_min = value;
                    if (value > data.Misc.y_max)
                        data.Misc.y_max = value;
                }

                // Add new label and marker at the current end point
                Label(chart, series);
                Mark(chart, series.Points.Count - 1, series);
            }


            if (!chart.ChartAreas.IsUniqueName(stock))
            {
                var area = chart.ChartAreas[stock];

                string seriesName = stock + " " + "1";
                var series = chart.Series[seriesName];
                int totalPoints = series.Points.Count;

                double padding = (data.Misc.y_max - data.Misc.y_min) * 0.05;
                area.AxisY.Minimum = data.Misc.y_min - padding;
                area.AxisY.Maximum = data.Misc.y_max + padding;

                // === AxisX interval ===
                area.AxisX.Interval = totalPoints - 1;
            }
        }


        public static void UpdateSeriesoldold(Chart chart, StockData data)
        {
            int totalPoints = data.Api.nrow;
            if (data.Misc.ShrinkDraw)
                totalPoints -= g.NptsForShrinkDraw;

            int[] seriesIds = { 1, 3, 4, 5, 6, 10, 11 };

            foreach (int id in seriesIds)
            {
                string seriesName = data.Stock + " " + id;

                // Skip if the series doesn't exist
                if (chart.Series.IsUniqueName(seriesName))
                    continue;

                var series = chart.Series[seriesName];
                double magnifier = 1.0;
                Magnifier(data.Stock, id, ref magnifier);

                int seriesCount = series.Points.Count;

                if (seriesCount == totalPoints)
                {
                    series.Points[totalPoints - 1].SetValueXY(
                        (data.Api.x[totalPoints - 1, 0] / g.HUNDRED).ToString(),
                        data.Api.x[totalPoints - 1, id] * magnifier);

                    Label(chart, series);
                    Mark(chart, totalPoints - 1, series);
                }
                else
                {
                    for (int i = seriesCount; i < totalPoints; i++)
                    {
                        if (i > 0 && i - 1 < series.Points.Count)
                            series.Points[i - 1].Label = null;

                        series.Points.AddXY(
                            (data.Api.x[i, 0] / g.HUNDRED).ToString(),
                            data.Api.x[i, id] * magnifier);

                        Label(chart, series);
                        Mark(chart, i, series);
                    }
                }
            }

            /// returns true if NO chart area exists with the name data.Stock.
            /// it's asking if data.Stock is a name that could be used safely for a new chart area
            /// return early not to throw an error
            if (!chart.ChartAreas.IsUniqueName(data.Stock))
            {
                var area = chart.ChartAreas[data.Stock];
                area.AxisX.Interval = totalPoints - 1;
            }
        }

        
        private static void Magnifier(string stock, int id, ref double magnifier)
        {
            magnifier = 1.0;
            int i = -1;
            switch (stock) // "가격", "지수", "기타", "나스닥"
            {
                case "KODEX 레버리지": i = 0; break;
                case "KODEX 코스닥150레버리지": i = 1; break;
            }

            int j = -1;
            switch (id)
            {
                case 1: j = 0; break;  // 가격
                case 10: j = 1; break; // 나스닥
            }

            if (i >= 0 && j >= 0)
                magnifier = g.KodexMagnifier[i, j];
        }

        public static void Mark(Chart chart, int markStartPoint, Series series)
        {
            string stockName = "";
            string chartArea = "";
            int columnIndex = 0;
            int endPoint = 0;

            ChartHandler.SeriesInfomation(series, ref stockName, ref chartArea, ref columnIndex, ref endPoint);

            var stockData = g.StockRepository.TryGetDataOrNull(stockName);
            if (stockData == null) return;

            var o = stockData.Api;

            if (columnIndex != 1 || o.x == null || o.nrow <= 1)
                return;

            for (int m = markStartPoint + 1; m <= endPoint; m++)
            {
                int priceChange = o.x[m, 1] - o.x[m - 1, 1];

                if (priceChange >= 20)
                {
                    Color markerColor = Color.Red;
                    if (priceChange > 40) markerColor = Color.Blue;
                    else if (priceChange > 30) markerColor = Color.Green;

                    series.Points[m].MarkerColor = markerColor;
                    series.Points[m].MarkerSize = 7;
                    series.Points[m].MarkerStyle = MarkerStyle.Circle;
                }
            }
        }

        public static void Label(Chart chart, Series series)
        {
            string stock = "";
            string area = "";
            int columnIndex = 0;
            int endPoint = 0;

            ChartHandler.SeriesInfomation(series, ref stock, ref area, ref columnIndex, ref endPoint);

            var data = g.StockRepository.TryGetDataOrNull(stock);
            if (data == null) return;

            int totalPoints = g.test ? g.Npts[1] : data.Api.nrow;
            string label = "      " + ((int)(data.Api.x[totalPoints - 1, columnIndex])).ToString();

            for (int i = totalPoints - 1; i >= totalPoints - 4; i--)
            {
                if (i - 1 < 0) break;
                double delta = data.Api.x[i, columnIndex] - data.Api.x[i - 1, columnIndex];
                label += delta >= 0 ? "+" + delta.ToString() : delta.ToString();
            }

            series.Points[endPoint].Label = label;
            series.LabelForeColor = colorKODEX[columnIndex];
            series.Font = new Font("Arial", g.v.font, FontStyle.Regular);
        }
    }
}
