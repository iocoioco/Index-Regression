using New_Tradegy.Library.Listeners;
using New_Tradegy.Library.Models;
using System;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    public static class ChartIndex
    {
        private static Color[] colorKODEX = { Color.White, Color.Red,
        Color.White, Color.Black, Color.Brown, Color.Magenta, Color.Green,
        Color.White, Color.White, Color.White, Color.Blue, Color.Brown };

        //How to call
        //ChartIndex.UpdateIndexChart(g.chart1);
        //ChartIndex.UpdateIndexChart(g.chart2, isChart1: false);
        public static void UpdateChart(Chart chart, bool isChart1 = true, bool includeIndex = true)
        {
            if (!includeIndex) return; // e.g., chart2 might not have index

            var indexStocks = new[] { "KODEX 레버리지", "KODEX 코스닥150레버리지" };
            int nRow = g.nRow;
            int nCol = isChart1 ? g.nCol : 5;

            for (int i = 0; i < indexStocks.Length; i++)
            {
                string stock = indexStocks[i];
                var data = g.StockRepository.TryGetStockOrNull(stock);
                if (data == null) continue;

                bool shouldUpdateSeries = ChartHandler.ChartAreaExists(chart, stock) &&
                    g.MarketeyeCount % g.MarketeyeCountDivicer != 1 &&
                    g.connected && !g.test && !data.Misc.ShrinkDraw;


                if (shouldUpdateSeries)
                    UpdateSeries(chart, data);
                else
                    UpdateChartArea(chart, data);

                if (isChart1 && g.connected && !Utils.FormUtils.DoesDataGridViewExist(Utils.FormUtils.FindFormByName("Form1"), stock))
                {
                    var b = new BookBidGenerator();
                    var bookbid = b.GenerateBookBidView(stock);
                    bookbid.Height = g.DgvCellHeight * 12;

                    int x = (g.screenWidth / nCol) + 10;
                    int y = (g.screenHeight / nRow) * (i == 0 ? 0 : 2) - 4 * (i == 0 ? 0 : 2);
                    bookbid.Location = new Point(x, y);
                }
            }

            chart.Invalidate();
        }

        public static ChartArea UpdateChartArea(Chart chart, StockData data)
        {
            string stock = data.Stock;
            if (data.Api.nrow <= 1)
                return null;

            // Determine Start and End Row
            int start = g.Npts[0];
            int end = g.test ? Math.Min(g.Npts[1], data.Api.nrow) : data.Api.nrow;
            if (data.Misc.ShrinkDraw)
                start = Math.Max(end - g.NptsForShrinkDraw, g.Npts[0]);

            // Create ChartArea and Removes default "ChartArea1"
            chart.ChartAreas.Remove(chart.ChartAreas.FindByName("ChartArea1"));
            ChartArea area = new ChartArea(stock);
            chart.ChartAreas.Add(area);

            int ymin = int.MaxValue, ymax = int.MinValue;
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
                    int val = id == 10
                        ? (int)(HandleUSFuture(data, i, end, start, id) * mag)
                        : (int)(data.Api.x[i, id] * mag);
                    series.Points.AddXY((data.Api.x[i, 0] / g.HUNDRED).ToString(), val);

                    ymin = Math.Min(ymin, val);
                    ymax = Math.Max(ymax, val);
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

            double pad = (ymax - ymin) * 0.05;
            area.AxisY.Minimum = ymin - pad;
            area.AxisY.Maximum = ymax + pad;

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

        public static void UpdateSeries(Chart chart, StockData data)
        {
            int totalPoints = data.Api.nrow;
            if (data.Misc.ShrinkDraw)
                totalPoints -= g.NptsForShrinkDraw;

            string[] seriesIds = { "1", "3", "4", "5", "6", "10", "11" };

            foreach (var idStr in seriesIds)
            {
                //Skips if the series does not already exist(implies series was not created).
                string seriesName = data.Stock + " " + idStr;
                if (chart.Series.IsUniqueName(seriesName))
                    continue;

                var series = chart.Series[seriesName];
                double magnifier = 1.0;
                Magnifier(data.Stock, int.Parse(idStr), ref magnifier);

                int seriesCount = series.Points.Count;

                if (seriesCount == totalPoints)
                {
                    series.Points[totalPoints - 1].SetValueXY(
                        (data.Api.x[totalPoints - 1, 0] / g.HUNDRED).ToString(),
                        data.Api.x[totalPoints - 1, int.Parse(idStr)] * magnifier);

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
                            data.Api.x[i, int.Parse(idStr)] * magnifier);

                        Label(chart, series);
                        Mark(chart, i, series);
                    }
                }
            }

            if (chart.ChartAreas.IsUniqueName(data.Stock)) return;

            var area = chart.ChartAreas[data.Stock];
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = totalPoints - 1;
            area.AxisX.IntervalOffset = 1;
        }

        private static double HandleUSFuture(StockData data, int k, int end, int start, int id)
        {
            // Placeholder: you can move your US index interpolation logic here
            return data.Api.x[k, id];
        }
        
        private static void Magnifier(string stock, int id, ref double magnifier)
        {
            int i = -1;
            switch (stock)
            {
                case "KODEX 레버리지": i = 0; break;
                case "KODEX 200선물인버스2X": i = 1; break;
                case "KODEX 코스닥150레버리지": i = 2; break;
                case "KODEX 코스닥150선물인버스": i = 3; break;
            }

            int j = -1;
            switch (id)
            {
                case 1: j = 0; break;
                case 3: j = 1; break;
                case 4:
                case 5:
                case 6:
                case 11: j = 2; break;
                case 10: j = 3; break;
            }

            if (i >= 0 && j >= 0)
                magnifier = g.kodex_magnifier[i, j];
        }

        public static void Mark(Chart chart, int markStartPoint, Series series)
        {
            string stockName = "";
            string chartArea = "";
            int columnIndex = 0;
            int endPoint = 0;

            ChartHandler.SeriesInfomation(series, ref stockName, ref chartArea, ref columnIndex, ref endPoint);

            var stockData = g.StockRepository.TryGetStockOrNull(stockName);
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

            var data = g.StockRepository.TryGetStockOrNull(stock);
            if (data == null) return;

            int totalPoints = g.test ? Math.Min(g.Npts[1], data.Api.nrow) : data.Api.nrow;
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
