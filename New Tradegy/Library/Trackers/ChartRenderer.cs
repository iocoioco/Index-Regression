using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Utils;

namespace New_Tradegy.Library.Trackers
{
    public static class ChartRenderer
    {
        // 0, 1(가격), 2(수급), 3(체강), 4(프로그램), 5(외인), 6(기관)
        private static Color[] colorGeneral = { Color.White, Color.Red, Color.DarkGray,
        Color.LightCoral, Color.DarkBlue, Color.Magenta, Color.Cyan };
        // 0, 1(가격), 2, 3(지수합), 4(기관), 5(외인), 6(개인), 7, 8, 9, 10(나스닥), 11(연기금)
        private static Color[] colorKODEX = { Color.White, Color.Red,
        Color.White, Color.Black, Color.Brown, Color.Magenta, Color.Green,
        Color.White, Color.White, Color.White, Color.Blue, Color.Brown };

        public static void CreateOrUpdateChart(Chart chart, StockData data, int row, int col)
        {
            string areaName = data.Stock;

            ChartArea area;
            if (!chart.ChartAreas.IsNameUnique(areaName))
            {
                area = chart.ChartAreas[areaName];
                UpdateSeries(chart, data);
                RelocateArea(area, row, col);
                UpdateAnnotation(chart, data, row, col);
                return;
            }

            area = new ChartArea(areaName);
            chart.ChartAreas.Add(area);
            InitializeChartArea(area);
            AddSeries(chart, area, data);
            AddAnnotation(chart, data);
            RelocateArea(area, row, col);
        }

        public static void UpdateSeries(Chart chart, StockData data)
        {
            var series = chart.Series[data.Stock];
            series.Points.Clear();
            series.Points.AddY(data.Api.현재가); // Placeholder
        }

        public static void AddSeries(Chart chart, ChartArea area, StockData data)
        {
            var series = new Series(data.Stock)
            {
                ChartType = SeriesChartType.Line,
                ChartArea = area.Name,
                BorderWidth = 2,
                Color = Color.Blue
            };
            series.Points.AddY(data.Api.현재가);
            chart.Series.Add(series);
        }

        public static void AddAnnotation(Chart chart, StockData data)
        {
            var annotation = new TextAnnotation
            {
                Name = data.Stock + "_anno",
                Text = data.Api.현재가.ToString(),
                AnchorDataPoint = chart.Series[data.Stock].Points.LastOrDefault(),
                AxisX = chart.ChartAreas[data.Stock].AxisX,
                AxisY = chart.ChartAreas[data.Stock].AxisY,
                Font = new Font("Consolas", 8),
                ForeColor = Color.DarkGreen
            };
            chart.Annotations.Add(annotation);
        }

        public static void UpdateAnnotation(Chart chart, StockData data, int row, int col)
        {
            string name = data.Stock + "_anno";
            if (!chart.Annotations.IsNameUnique(name))
            {
                var anno = chart.Annotations[name] as TextAnnotation;
                if (anno != null)
                {
                    anno.Text = data.Api.현재가.ToString();
                    anno.AnchorDataPoint = chart.Series[data.Stock].Points.LastOrDefault();
                }
            }
        }

        public static void RelocateArea(ChartArea area, int row, int col)
        {
            float width = 100f / g.nCol;
            float height = 100f / g.nRow;
            area.Position.X = col * width;
            area.Position.Y = row * height;
            area.Position.Width = width;
            area.Position.Height = height;
        }

        public static void InitializeChartArea(ChartArea area)
        {
            area.AxisX.LabelStyle.Enabled = false;
            area.AxisY.LabelStyle.Enabled = false;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.IsStartedFromZero = false;
        }

        public static void ClearUnused(Chart chart, List<string> activeStocks)
        {
            var unusedAreas = chart.ChartAreas
                .Cast<ChartArea>()
                .Where(a => !activeStocks.Contains(a.Name))
                .ToList();

            foreach (var area in unusedAreas)
            {
                chart.ChartAreas.Remove(area);
                if (!chart.Series.IsNameUnique(area.Name))
                    chart.Series.Remove(chart.Series[area.Name]);
            }

            var unusedAnnotations = chart.Annotations
                .Cast<Annotation>()
                .Where(a => !activeStocks.Contains(a.Name.Replace("_anno", "")))
                .ToList();

            foreach (var anno in unusedAnnotations)
                chart.Annotations.Remove(anno);
        }


        static ChartArea GeneralArea(Chart chart, string stockName, int nRow, int nCol, bool isMainChart)
        {
            var data = StockRepository.Instance.GetOrThrow(stockName);
            if (data.Api.nrow < 2) return null;

            double y_min = 100000;
            double y_max = -100000;

            int StartNpts = g.Npts[0];
            int EndNpts = (!g.test) ? data.Api.nrow : Math.Min(g.Npts[1], data.Api.nrow);

            if (data.Misc.ShrinkDraw)
            {
                StartNpts = Math.Max(EndNpts - g.NptsForShrinkDraw, g.Npts[0]);
            }

            if (data.Api.x[EndNpts - 1, 3] == 0) return null;

            string area = stockName;
            if (chart.InvokeRequired)
            {
                chart.Invoke(new Action(() => chart.ChartAreas.Add(area)));
            }
            else
            {
                chart.ChartAreas.Add(area);
            }

            bool drawSuccess = DrawSeriesLines(chart, data, stockName, area, StartNpts, EndNpts, ref y_min, ref y_max);
            if (!drawSuccess) return null;

            string annotation = AnnotationGeneral(chart, data, data.Api.x, StartNpts, EndNpts, data.Api.nrow);

            CalculateHeights(g.v.font, 5, g.nRow, out double annotationHeight, out double chartAreaHeight);

            Color BackColor = Color.White;
            foreach (int threshold in new[] { 90, 70, 50, 30, 10 })
            {
                if (data.Score.총점 > threshold)
                {
                    BackColor = g.Colors[Array.IndexOf(new[] { 90, 70, 50, 30, 10 }, threshold)];
                    break;
                }
            }

            float yOffset = isMainChart ? 0 : 3;
            AddRectangleAnnotationWithText(chart, annotation, new RectangleF(0, yOffset, 100 / nCol, (int)annotationHeight + 2), area, Color.Black, BackColor);

            var chartArea = chart.ChartAreas[area];
            chartArea.AxisY.LabelStyle.Enabled = false;
            chartArea.AxisY.MajorTickMark.Enabled = false;
            chartArea.AxisY.MinorTickMark.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            chartArea.AxisY.MinorGrid.Enabled = false;

            chartArea.InnerPlotPosition = new ElementPosition(20, 5, 55, 100);
            double padding = (y_max - y_min) * 0.05;
            chartArea.AxisY.Minimum = y_min - padding;
            chartArea.AxisY.Maximum = y_max + padding * 1.5;

            int TotalNumberPoint = EndNpts - StartNpts;
            chartArea.AxisX.LabelStyle.Enabled = true;
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisX.Interval = TotalNumberPoint - 1;
            chartArea.AxisX.IntervalOffset = 1;

            chartArea.AxisX.LabelStyle.Font = new Font("Arial", 7);
            chartArea.AxisY.LabelStyle.Font = new Font("Arial", 7);

            if (g.q == "o&s" && data.Score.그순 < 5)
            {
                chartArea.BackColor = g.Colors[data.Score.그순];
            }

            if (data.Api.분프로천[0] > 5 && data.Api.분외인천[0] > 5 && data.Api.분배수차[0] > 0)
            {
                chartArea.BackColor = g.Colors[5];
            }

            return chartArea;
        }


        private static bool DrawSeriesLines(Chart chart, StockData o, string stockName, string area,
                                 int StartNpts, int EndNpts, ref double y_min, ref double y_max)
        {
            var lineTypes = new List<(int id, string label, Color color, int width)>
    {
        (1, "price", colorGeneral[1], g.LineWidth),
        (2, "amount", colorGeneral[2], 1),
        (3, "intensity", colorGeneral[3], 1),
        (4, "program", colorGeneral[4], g.LineWidth),
        (5, "foreign", colorGeneral[5], g.LineWidth),
        (6, "institute", colorGeneral[6], g.LineWidth)
    };

            for (int i = 0; i < lineTypes.Count; i++)
            {
                var (typeId, label, color, width) = lineTypes[i];
                string sid = $"{stockName} {typeId}";

                var series = new Series(sid)
                {
                    ChartArea = area,
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.Date,
                    IsVisibleInLegend = false,
                    Color = color,
                    BorderWidth = width
                };

                if (chart.InvokeRequired)
                    chart.Invoke(new Action(() => chart.Series.Add(series)));
                else
                    chart.Series.Add(series);

                int pointsCount = 0;

                for (int k = StartNpts; k < EndNpts; k++)
                {
                    if (o.Api.x[k, 0] == 0)
                    {
                        if (pointsCount < 2) return false;
                        else break;
                    }

                    int value = GeneralValue(o, k, typeId);
                    string xLabel = ((int)(o.Api.x[k, 0] / g.HUNDRED)).ToString();

                    if (chart.InvokeRequired)
                        chart.Invoke(new Action(() => series.Points.AddXY(xLabel, value)));
                    else
                        series.Points.AddXY(xLabel, value);

                    pointsCount++;
                    y_min = Math.Min(y_min, value);
                    y_max = Math.Max(y_max, value);
                }

                if (pointsCount < 2) return false;

                int markStart = StartNpts + 1;
                MarkGeneral(chart, markStart, series);
                LabelGeneral(chart, series);
            }

            return true;
        }

        private static int GeneralValue(StockData o, int k, int id)
        {
            int value = 0;
            switch (id)
            {
                case 1:
                    value = o.Api.x[k, 1];
                    break;
                case 2:
                    value = (int)(Math.Sqrt(o.Api.x[k, 2]) * 10);
                    if (value > 500)
                        value = 500;
                    break;
                case 3:
                    value = (int)(Math.Sqrt(o.Api.x[k, 3] / (double)g.HUNDRED) * 10);
                    if (value > 500)
                        value = 500;
                    break;
                case 4:
                case 5:
                case 6:
                    double multiplier = 1;
                    if (MathUtils.IsSafeToDivide(o.Api.x[o.Api.nrow - 1, 7]))
                    {
                        multiplier = 100.0 / o.Api.x[o.Api.nrow - 1, 7] * g.v.수급과장배수 * o.Misc.수급과장배수;
                    }
                    value = (int)(o.Api.x[k, id] * multiplier);
                    break;
            }
            return value;
        }

    }
}
