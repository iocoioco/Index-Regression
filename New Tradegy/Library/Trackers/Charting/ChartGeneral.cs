using New_Tradegy.Library.Models;
using New_Tradegy.Library.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers
{
    internal class ChartGeneral
    {
        // 0, 1(가격), 2(수급), 3(체강), 4(프로그램), 5(외인), 6(기관)
        private static Color[] colorGeneral = { Color.White, Color.Red, Color.DarkGray,
        Color.LightCoral, Color.DarkBlue, Color.Magenta, Color.Cyan };

        public static (ChartArea area, Annotation anno) UpdateChartArea(Chart chart, StockData data)
        {
            string areaName = data.Stock;
            ChartArea area = null;
            Annotation anno = null;

            // Update exiting chartarea
            if (chart.ChartAreas.IndexOf(areaName) >= 0) // stockName(areaName) exists, update series
            {
                area = chart.ChartAreas[areaName];
                UpdateSeries(chart, data);

            }
            // Generate a new chartarea
            else
            {
                ChartBasic.RemoveChartBlock(chart, data.Stock); // delete chartarea & related series
                area = CreateChartArea(chart, data);
            }
            if (area != null)
            {
                anno = RedrawAnnotation(chart, data); // existing annotation will be deleted in RedrawAnnotation
            }
            
            return (area, anno);
        }

        public static ChartArea CreateChartArea(Chart chart, StockData data)
        {
            var areaName = data.Stock;

            double y_min = 100000;
            double y_max = -100000;

            //  int StartNpts = g.Npts[0]; no need
            int StartNpts = 0;
            int EndNpts = g.test ? g.Npts[1] : data.Api.nrow;

            if (data.Misc.ShrinkDraw)
                StartNpts = Math.Max(EndNpts - g.NptsForShrinkDraw, g.Npts[0]);

            
            var area = new ChartArea(areaName);
            chart.ChartAreas.Add(area);
            area.Visible = false;

            bool success = AddSeriesLines(chart, data, data.Stock, area.Name, StartNpts, EndNpts, ref y_min, ref y_max);
            if (!success)
                return null;

            area.AxisY.LabelStyle.Enabled = false;
            area.AxisY.MajorTickMark.Enabled = false;
            area.AxisY.MinorTickMark.Enabled = false;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.MinorGrid.Enabled = false;

            //area.Position = new ElementPosition(20, 5, 55, 60); // outer chart area
            var cellWidth = 100f / g.nCol;
            var cellHeight = 100f / g.nRow;

            area.InnerPlotPosition = ChartBasic.CalculateInnerPlotPosition(cellWidth, cellHeight);  
                                     //new ElementPosition(20, 5, 55, 80);
            double padding = (y_max - y_min) * 0.1; // was 0.05
            area.AxisY.Minimum = y_min - 0.0 * padding; // was 1.0
            area.AxisY.Maximum = y_max + 2.5 * padding; // was 1.5

            int TotalNumberPoint = EndNpts - StartNpts;
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = TotalNumberPoint - 1;
            area.AxisX.IntervalOffset = 1;

            area.AxisX.LabelStyle.Font = new Font("Arial", 7);
            area.AxisY.LabelStyle.Font = new Font("Arial", 7);

            if (g.q == "o&s" && data.Score.그룹_등수 < 5)
            {
               // area.BackColor = g.Colors[data.Score.그룹_등수];
            }

            if (data.Api.분프로천[0] > 5 && data.Api.분외인천[0] > 5 && data.Api.분배수차[0] > 0)
            {
                area.BackColor = g.Colors[5];
            }

            return area;
        }


        // General stock version: incremental update only, minimal redraw
        public static void UpdateSeries(Chart chart, StockData data)
        {
            string stock = data.Stock;
            int last = data.Api.nrow - 1;
            if (last < 0 || data.Api.x[last, 0] == 0)
                return;

            string xLabel = ((int)(data.Api.x[last, 0] / g.HUNDRED)).ToString();
            int[] seriesIds = { 1, 2, 3, 4, 5, 6 };

            foreach (int typeId in seriesIds)
            {
                string seriesName = stock + " " + typeId;
                if (chart.Series.IsUniqueName(seriesName)) continue;

                var series = chart.Series[seriesName];
                int value = PointValue(data, last, typeId);

                if (series.Points.Count > 0 && series.Points.Last().AxisLabel == xLabel)
                {
                    series.Points.Last().YValues[0] = value;
                }
                else
                {
                    series.Points.AddXY(xLabel, value);
                }

                Label(chart, series);
                Mark(chart, series.Points.Count - 1, series);
            }

            if (!chart.ChartAreas.IsUniqueName(stock))
            {
                var area = chart.ChartAreas[stock];

                string seriesName = stock + " " + "1";
                var series = chart.Series[seriesName];
                int totalPoints = series.Points.Count;
                if (data.Misc.ShrinkDraw)
                    totalPoints -= g.NptsForShrinkDraw;
                area.AxisX.Interval = data.Api.nrow - 1;
            }
        }


      
        private static bool AddSeriesLines(Chart chart, StockData o, string stockName, string area,
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
                       
                        break;
                    }

                    int value = PointValue(o, k, typeId);
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
                Mark(chart, markStart, series);
                Label(chart, series);
            }

            return true;
        }

        private static int PointValue(StockData data, int k, int id)
        {
            int value = 0;
            switch (id)
            {
                case 1:
                    value = data.Api.x[k, 1];
                    break;
                case 2:
                    value = (int)(Math.Sqrt(data.Api.x[k, 2]) * 10);
                    if (value > 500)
                        value = 500;
                    break;
                case 3:
                    value = (int)(Math.Sqrt(data.Api.x[k, 3] / (double)g.HUNDRED) * 10);
                    if (value > 500)
                        value = 500;
                    break;
                case 4:
                case 5:
                case 6:
                    double multiplier = 1;
                    if (MathUtils.IsSafeToDivide(data.Api.x[data.Api.nrow - 1, 7]))
                    {
                        multiplier = 100.0 / data.Api.x[data.Api.nrow - 1, 7] * g.v.수급과장배수 * data.Misc.수급과장배수;
                    }
                    value = (int)(data.Api.x[k, id] * multiplier);
                    break;
            }
            return value;
        }

        public static void Mark(Chart chart, int MarkStartPoint, Series series)
        {
            // Extract series information
            string stock = "";
            string chartAreaName = "";
            int columnIndex = 0;
            int endPoint = 0;
            ChartHandler.SeriesInfomation(series, ref stock, ref chartAreaName, ref columnIndex, ref endPoint);

            var data = g.StockRepository.TryGetDataOrNull(stock);
            if (data == null) return;

            var x = data.Api.x;

            // Mark only for price, amount, intensity
            if (columnIndex > 3) return;

            for (int m = MarkStartPoint; m <= endPoint; m++)
            {
                if (columnIndex == 1) // price
                {
                    int priceChange = x[m, 1] - x[m - 1, 1];

                    if (priceChange >= 100)
                    {
                        if (priceChange > 300)
                            series.Points[m].MarkerColor = Color.Black;
                        else if (priceChange > 200)
                            series.Points[m].MarkerColor = Color.Blue;
                        else if (priceChange > 150)
                            series.Points[m].MarkerColor = Color.Green;
                        else
                            series.Points[m].MarkerColor = Color.Red;

                        series.Points[m].MarkerSize = 6;
                        series.Points[m].MarkerStyle = MarkerStyle.Cross;
                    }

                    if (x[m, 0] >= 90200)
                    {
                        int diff = x[m, 8] - x[m, 9];
                        if (diff > 100)
                        {
                            series.Points[m].MarkerSize = 9;
                            series.Points[m].MarkerStyle = MarkerStyle.Circle;

                            if (diff > 500)
                                series.Points[m].MarkerColor = Color.Black;
                            else if (diff > 300)
                                series.Points[m].MarkerColor = Color.Blue;
                            else if (diff > 200)
                                series.Points[m].MarkerColor = Color.Green;
                            else
                                series.Points[m].MarkerColor = Color.Red;
                        }
                    }

                    if (data.Api.분거래천[0] > 10)
                    {
                        int markSize = 0;
                        Color color = Color.White;
                        double val = data.Api.분거래천[0];

                        if (val < 50) { color = Color.Red; markSize = 10; }
                        else if (val < 100) { color = Color.Red; markSize = 15; }
                        else if (val < 200) { color = Color.Green; markSize = 15; }
                        else if (val < 300) { color = Color.Green; markSize = 20; }
                        else if (val < 500) { color = Color.Blue; markSize = 20; }
                        else if (val < 800) { color = Color.Blue; markSize = 30; }
                        else if (val < 1200) { color = Color.Black; markSize = 30; }
                        else if (val < 1700) { color = Color.Black; markSize = 40; }
                        else { color = Color.Black; markSize = 50; }

                        series.Points[0].MarkerColor = color;
                        series.Points[0].MarkerSize = markSize;

                        if (priceChange >= 0)
                            series.Points[0].MarkerStyle = MarkerStyle.Circle;
                        else
                            series.Points[0].MarkerStyle = MarkerStyle.Cross;
                    }
                }

                // amount or intensity mark
                if (columnIndex == 2 || columnIndex == 3)
                {
                    int threshold = g.npts_for_magenta_cyan_mark;
                    if (x[m, columnIndex + 8] >= threshold)
                    {
                        series.Points[m].MarkerColor = columnIndex == 2 ? Color.Magenta : Color.Cyan;
                        series.Points[m].MarkerStyle = MarkerStyle.Cross;
                        series.Points[m].MarkerSize = 7;
                    }
                }
            }
        }

        public static void Label(Chart chart, Series t)
        {
            // { 1, 2, 3, 4, 5, 6 }; price, amount, intensity, program, foreign, institute
            // 1, 4, 5 extened label
            // 2, 3 only 1 label
            // 6 no label
            string stock = "";
            string chartAreaName = "";
            int columnIndex = 0;
            int endPoint = 0;

            ChartHandler.SeriesInfomation(t, ref stock, ref chartAreaName, ref columnIndex, ref endPoint);

            var data = g.StockRepository.TryGetDataOrNull(stock);
            if (data == null) return;

            var api = data.Api;
            var post = data.Post;
            // int totalPoints = g.test ? g.Npts[1] : data.Api.nrow; //????

            string s = "";

            switch (columnIndex)
            {
                case 1: // price
                    s = "      " + api.x[endPoint - 1, columnIndex].ToString();
                    for (int k = 0; k < 4; k++)
                    {
                        if (endPoint - 2 - k < 0) break;
                        int d = api.x[endPoint - 1 - k, columnIndex] - api.x[endPoint - 2 - k, columnIndex];
                        s += (d >= 0 ? "+" : "") + d.ToString("F0");
                    }
                    break;

                case 2: // amount
                    s = api.x[endPoint - 1, columnIndex].ToString();
                    break;

                case 3: // intensity
                    s = (api.x[endPoint - 1, columnIndex] / 100).ToString();
                    break;

                case 4: // program
                    s = (post.푀누천 / 10.0).ToString("F1");
                    for (int k = 0; k < 4; k++)
                    {
                        if (endPoint - 2 - k < 0) break;
                        double d = api.분프로천[k] / 10.0;
                        s += (d >= 0 ? "+" : "") + d.ToString("F1");
                    }
                    break;

                case 5: // foreign
                    s = (post.외누천 / 10.0).ToString("F1");
                    for (int k = 0; k < 4; k++)
                    {
                        if (endPoint - 2 - k < 0) break;
                        double d = api.분외인천[k] / 10.0;
                        s += (d >= 0 ? "+" : "") + d.ToString("F1");
                    }
                    break;

                case 6:
                    return;
            }

            t.Points[endPoint].Label = s;
            t.LabelForeColor = colorGeneral[columnIndex];

            t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        public static Annotation RedrawAnnotation(Chart chart, StockData data)
        {
            // If annotation with the name of data.Stock exists, delete it
            var annoName = data.Stock;
            var existingAnno = chart.Annotations.FindByName(annoName);
            if (existingAnno != null)
            {
                chart.Annotations.Remove(existingAnno);
            }

            if (chart == null || data == null)
                return null;

            // Remove old annotation
            string annotationName = data.Stock;
            var previousAnnotation  = chart.Annotations.FirstOrDefault(a => a.Name == annotationName);
            if (previousAnnotation != null)
                chart.Annotations.Remove(previousAnnotation);

            // Determine range for annotation based on shrink setting
            int StartNpts = 0;
            int EndNpts = (!g.test) ? data.Api.nrow : Math.Min(g.Npts[1], data.Api.nrow);

            if (data.Misc.ShrinkDraw)
            {
                StartNpts = Math.Max(EndNpts - g.NptsForShrinkDraw, g.Npts[0]);
            }

            string annotation = AnnotationText(chart, data, data.Api.x, StartNpts, EndNpts, data.Api.nrow);

            AnnotationCalculateHeights(g.v.font, 5, g.nRow, out double annotationHeight, out double chartAreaHeight);

            Color BackColor = Color.White;
            if(data.Score.그룹_등수 < 5)
                BackColor = g.Colors[data.Score.그룹_등수];
            
            // Adjust vertical offset depending on chart
            float yOffset = chart.Name == "chart1" ? 0f : 3f;

            Annotation anno = AnnotationAddRectangleWithText(chart, annotation,
                new RectangleF(0, yOffset, 100 / g.nCol, (int)annotationHeight + 2f), annoName, Color.Black, BackColor);

            anno.Visible = false;

            if (g.q == "o&s" && data.Score.그룹_등수 < 5)
            {
                anno.BackColor = g.Colors[data.Score.그룹_등수];
            }

            return anno;
        }

        public static string AnnotationText(Chart chart, StockData o, int[,] x, int StartNpts, int EndNpts, int total_nrow)
        {
            string stock = o.Stock;
            string stock_title = "";

            // no need
            //if (EndNpts - 1 < 0)
            //    return stock_title;

            // 일반의 첫째 라인
            if (g.StockManager.HoldingList.Contains(stock))
                stock_title = "$$" + stock_title;
            else if (g.StockManager.InterestedWithBidList.Contains(stock))
                stock_title = "@ " + stock_title;
            else if (g.StockManager.InterestedOnlyList.Contains(stock))
                stock_title = "@ " + stock_title;


            stock_title += stock.Length >= 5 ? stock.Substring(0, 5) : stock;

            var groupTitle = g.GroupManager.FindGroupByStock(stock); // group title
            stock_title += groupTitle == null ? "%" : " ";
            stock_title += Math.Round(o.Post.종거천 / 10.0) + "  " +
                           (o.Post.푀누천 / 10.0).ToString("F1") + "  " +
                           (o.Post.외누천 / 10.0).ToString("F1") + "  " +
                           (o.Post.기누천 / 10.0).ToString("F1");

            stock_title += "\n" + AnnotationMinute(o, x, StartNpts, EndNpts);

            if (!g.StockManager.IndexList.Contains(stock))
            {
                stock_title += "\n";
                if (o.Api.분외인천[0] >= 0)
                    stock_title += o.Api.분프로천[0].ToString("F0") + "+" + o.Api.분외인천[0].ToString("F0") + "/" + o.Api.분거래천[0].ToString("F0");
                else
                    stock_title += o.Api.분프로천[0].ToString("F0") + o.Api.분외인천[0].ToString("F0") + "/" + o.Api.분거래천[0].ToString("F0");

                for (int i = 1; i < 5; i++)
                {
                    stock_title += "   " + (o.Api.분프로천[i] + o.Api.분외인천[i]).ToString("F0") + "/" + o.Api.분거래천[i].ToString("F0");
                }

                stock_title += "\n";
                stock_title += o.Score.푀분.ToString("F0");
                stock_title += o.Score.배차 >= 0 ? "+" : "";
                stock_title += o.Score.배차.ToString("F0");
                stock_title += o.Score.배합 >= 0 ? "+" : "";
                stock_title += o.Score.배합.ToString("F0");
                stock_title += "+" + o.Score.그룹_등수.ToString("F0");


                stock_title += " (" + o.Api.x[EndNpts - 1, 1].ToString() + " / " + o.Api.현재가.ToString("#,##0") + ")";
            }
            else
            {
                stock_title += "\n(" + o.Api.x[EndNpts - 1, 1].ToString() + "/" + o.Api.현재가.ToString("#,##0") + ")";
            }


            stock_title += " " + o.Api.x[EndNpts - 1, 10] + "/" + o.Api.x[EndNpts - 1, 11] + "  " + o.Statistics.일간변동평균편차;

            return stock_title;
        }

        private static void AnnotationCalculateHeights(float fontSize, int numLines, int numRows, out double annotationHeight, out double chartAreaHeight)
        {
            // Get screen height in pixels
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int screenHeight = workingArea.Height;

            // Approximate conversion from point size to pixels
            double pointToPixel = 1.33;
            double annotationPixelHeight = fontSize * numLines * pointToPixel;

            // Available space in chart terms (percentage-based)
            double totalHeightPercent = 100.0;
            double rowHeightPercent = totalHeightPercent / numRows;

            // Calculate how much of that row goes to annotation (in percent)
            annotationHeight = (annotationPixelHeight / screenHeight) * totalHeightPercent;
            chartAreaHeight = rowHeightPercent - annotationHeight;
        }

        private static Annotation AnnotationAddRectangleWithText(
        Chart chart,
        string text,
        RectangleF rect,
        string chartAreaName,
        Color textColor,
        Color backgroundColor)
        {
            // Retrieve the chart area safely
            var chartArea = chart.ChartAreas.FirstOrDefault(area => area.Name == chartAreaName);
            if (chartArea == null)
                throw new ArgumentException($"ChartArea '{chartAreaName}' does not exist."); //??

            // Calculate relative position within the ChartArea
            double relativeX = chartArea.Position.X + (rect.X * chartArea.Position.Width / 100.0);
            double relativeY = chartArea.Position.Y; // Align top of the area

            // Optionally scale width/height relative to ChartArea if needed
            double relativeWidth = rect.Width; // Could be scaled if necessary
            double relativeHeight = rect.Height; // Could be scaled if necessary

            // Create and configure the annotation
            var annotation = new RectangleAnnotation
            {
                Name = chartAreaName,  // Optional: reuse chart area name as annotation name
                Text = text,
                Font = new Font("Arial", g.v.font),
                X = relativeX,
                Y = relativeY,
                Width = relativeWidth,
                Height = relativeHeight,
                LineColor = Color.Transparent,
                BackColor = backgroundColor,
                ForeColor = textColor,
                ClipToChartArea = "", // set to chartAreaName if you want strict clipping
                AxisXName = chartAreaName + "\\X",
                AxisYName = chartAreaName + "\\Y",
                Alignment = ContentAlignment.TopLeft,
                ToolTip = "Rectangle Annotation" // Optional
            };

            // Add and redraw
            chart.Annotations.Add(annotation);

            return annotation;
        }

        public static string AnnotationMinute(StockData o, int[,] x, int StartNpts, int EndNpts)
        {
            var sb = new StringBuilder();
            string stock = o.Stock;

            // Handle special KODEX types with predefined tick data
            if (stock == "KODEX 레버리지")
            {
                for (int i = 0; i < 3; i++)
                {
                    sb.Append($"{(int)MajorIndex.Instance.KospiTickBuyPower[i]}/{(int)MajorIndex.Instance.KospiTickSellPower[i]}  ");
                }
                sb.AppendLine();
            }
            else if (stock == "KODEX 200선물인버스2X")
            {
                for (int i = 0; i < 3; i++)
                {
                    sb.Append($"{(int)MajorIndex.Instance.KospiTickSellPower[i]}/{(int)MajorIndex.Instance.KospiTickBuyPower[i]}  ");
                }
                sb.AppendLine();
            }
            else if (stock == "KODEX 코스닥150레버리지")
            {
                for (int i = 0; i < 3; i++)
                {
                    sb.Append($"{(int)MajorIndex.Instance.KosdaqTickBuyPower[i]}/{(int)MajorIndex.Instance.KosdaqTickSellPower[i]}  ");
                }
                sb.AppendLine();
            }
            else if (stock == "KODEX 코스닥150선물인버스")
            {
                for (int i = 0; i < 3; i++)
                {
                    sb.Append($"{(int)MajorIndex.Instance.KosdaqTickSellPower[i]}/{(int)MajorIndex.Instance.KosdaqTickBuyPower[i]}  ");
                }
                sb.AppendLine();
            }

            // Add latest 5 minutes of multiplier differences
            for (int i = EndNpts - 1; i >= EndNpts - 5; i--)
            {
                if (i < 1) break;
                sb.Append($"{x[i, 8]}/{x[i, 9]}  ");
            }

            return sb.ToString();
        }
    }
}
