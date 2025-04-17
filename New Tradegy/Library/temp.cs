using CPSYSDIBLib;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library
{
    internal class temp
    {
        public static void Update(Chart chart, StockData data)
        {
            string name = data.Stock + " " + suffix;
            if (!chart.Series.IsUniqueName(name))
            {
                var series = chart.Series[name];
                double magnifier = 1.0;
                KodexMagnifier(data.Stock, int.Parse(suffix), ref magnifier);

                if (series.Points.Count == totalPoints)
                {
                    series.Points[totalPoints - 1].SetValueXY((data.Api.x[totalPoints - 1, 0] / g.HUNDRED).ToString(),
                        data.Api.x[totalPoints - 1, int.Parse(suffix)] * magnifier);
                    LabelIndex(chart, series);
                    MarkIndex(chart, totalPoints - 1, series);
                }
                else
                {
                    for (int i = series.Points.Count; i < totalPoints; i++)
                    {
                        if (chart.InvokeRequired)
                        {
                            chart.Invoke(new Action(() =>
                            {
                                series.Points[i - 1].Label = null;
                                series.Points.AddXY((data.Api.x[i, 0] / g.HUNDRED).ToString(),
                                    data.Api.x[i, int.Parse(suffix)] * magnifier);
                                LabelIndex(chart, series);
                                MarkIndex(chart, i, series);
                            }));
                        }
                        else
                        {
                            series.Points[i - 1].Label = null;
                            series.Points.AddXY((data.Api.x[i, 0] / g.HUNDRED).ToString(),
                                data.Api.x[i, int.Parse(suffix)] * magnifier);
                            LabelIndex(chart, series);
                            MarkIndex(chart, i, series);
                        }
                    }
                }
                return true;
            }
            return false;
        }



        public static void Update(Chart chart, StockData data)
        {
            int totalPoints = data.Api.nrow;
            if (data.Misc.ShrinkDraw)
                totalPoints -= g.NptsForShrinkDraw;

            string[] suffixes = { "1", "3", "4", "5", "6", "10", "11" };
            foreach (string suffix in suffixes)
            {
                if (!TryUpdateSeries(chart, data, suffix, totalPoints))
                    continue;
            }


            var area = chart.ChartAreas[data.Stock];
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = totalPoints - 1;
            area.AxisX.IntervalOffset = 1;
        }





    }
}

    
