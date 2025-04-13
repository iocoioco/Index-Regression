using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Models;

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

            foreach (var (typeId, label, color, width) in lineTypes)
            {
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
                    if (o.x[k, 0] == 0)
                    {
                        if (pointsCount < 2) return false;
                        else break;
                    }

                    int value = GeneralValue(o, k, typeId);
                    string xLabel = ((int)(o.x[k, 0] / g.HUNDRED)).ToString();

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

    }
}
