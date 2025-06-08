using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Trackers.Charting
{
    // call StockManagerEvents.NotifyChanged();
    public static class StockManagerEvents
    {
        public static event Action ListsChanged;

        public static void NotifyChanged()
        {
            ListsChanged?.Invoke();
        }
    }


    public class ChartMainRenderer
    {
        private Chart chart => g.ChartManager.Chart1;

        public void RefreshMainChart()
        {
            var indexList = g.StockManager.LeverageList;
            var holdings = g.StockManager.HoldingList;
            var interestedWithBid = g.StockManager.InterestedWithBidList;
            var interestedOnly = interestedWithBid.Except(holdings).ToList();
            var rankedStocks = g.StockManager.StockRankingList;

            const int MaxChartSlots = 24; // Or dynamically from nRow * 8
            var withBookBid = holdings
                .Concat(interestedWithBid)
                .Distinct()
                .Take(MaxChartSlots / 2)
                .ToList();

            int usedSlots = withBookBid.Count * 2;
            int remainingSlots = MaxChartSlots - usedSlots;

            var withoutBookBid = rankedStocks
                .Where(s => !withBookBid.Contains(s))
                .Distinct()
                .Take(remainingSlots)
                .ToList();

            // Final displayList = index (2 fixed) + others
            var displayList = new List<string>();
            displayList.AddRange(indexList);        // Always at front
            displayList.AddRange(withBookBid);
            displayList.AddRange(withoutBookBid);

            var usedChartAreas = new List<string>();
            var usedAnnotations = new List<string>();
            var usedBookbids = new List<string>();


         
            foreach (var stock in displayList)
            {
                var data = g.StockRepository.TryGetStockOrNull(stock);
                if (data == null) continue;

                if (g.StockManager.IndexList.Contains(stock))
                {
                    var area = ChartIndex.UpdateChartArea(chart, data);
                    usedChartAreas.Add(area.Name);
                }
                else
                {
                    var (area, anno) = ChartGeneralRenderer.UpdateChartArea(chart, data);
                    usedChartAreas.Add(area.Name);
                    usedAnnotations.Add(anno.Name);
                }

                if (g.StockManager.InterestedWithBidList.Contains(stock) || g.StockManager.IndexList.Contains(stock))
                {
                    g.BookBidManager.GetOrCreate(stock, 0, 0);
                    usedBookbids.Add(stock);
                }
            }

            RelocateChartAreasAndAnnotations(displayList);
            RelocateBookbids(usedBookbids);

            CleanupUnusedChartObjects(usedChartAreas, usedAnnotations, usedBookbids);

            chart.Invalidate();
        }

        private void CleanupUnusedChartObjects(List<string> keepAreas, List<string> keepAnnotations, List<string> keepBookbids)
        {
            foreach (var area in chart.ChartAreas.ToList())
            {
                if (!keepAreas.Contains(area.Name))
                    chart.ChartAreas.Remove(area);
            }

            foreach (var anno in chart.Annotations.ToList())
            {
                if (!keepAnnotations.Contains(anno.Name))
                    chart.Annotations.Remove(anno);
            }

            g.BookBidManager.CleanupAllExcept(keepBookbids);
        }

        private void RelocateChartAreasAndAnnotations(List<string> displayList)
        {
            int nCol = g.nCol;
            int nRow = g.nRow;
            float cellWidth = 100f / nCol;
            float cellHeight = 100f / nRow;

            for (int i = 0; i < displayList.Count; i++)
            {
                string stock = displayList[i];
                int row = i / nCol;
                int col = i % nCol;

                float x = col * cellWidth;
                float y = row * cellHeight;

                if (chart.ChartAreas.IndexOf(stock) >= 0)
                {
                    var area = chart.ChartAreas[stock];
                    if (stock == g.StockManager.LeverageList[1])
                        y = 50f;
                    area.Position = new ElementPosition(x, y, cellWidth, cellHeight);
                    area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);
                }

                var annotation = chart.Annotations.FirstOrDefault(a => a.Name == stock);
                if (annotation is RectangleAnnotation rect)
                {
                    rect.X = x;
                    rect.Y = y + cellHeight;
                    rect.Width = cellWidth;
                    rect.Height = 5.155f + 2f;
                }
            }
        }

        private void RelocateBookbids(List<string> stockList)
        {
            // Position bookbids below or beside their corresponding chart area
            foreach (var stock in stockList)
            {
                g.BookBidManager.Relocate(stock); // assumes you have a Relocate method
            }
        }
    }

}
