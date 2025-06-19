using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        public List<string> DisplayList { get; private set; }
        public void RefreshMainChart()
        {
            // Reference to the chart
            var chart = g.ChartManager.Chart1;

            // Constants and major collections (defined clearly at the top)
            const int MaxChartSlots = 24;

            var indexList = g.StockManager.LeverageList;
            var holdings = g.StockManager.HoldingList;
            var interestedWithBid = g.StockManager.InterestedWithBidList;
            var interestedOnly = g.StockManager.InterestedOnlyList;
            var rankedStocks = g.StockManager.StockRankingList;
            var excluded = g.StockManager.IndexList;

            // Step 1: Create withBookBid list (holdings + interestedWithBid - exclude index stocks)
            var withBookBid = holdings
                .Concat(interestedWithBid)
                .Distinct()
                .Where(stock => !excluded.Contains(stock))
                .Take(MaxChartSlots / 2)
                .ToList();

            // Step 2: Calculate remaining slots and build withoutBookBid from InterestedOnlyList + Ranking
            int usedSlots = withBookBid.Count * 2;
            int remainingSlots = MaxChartSlots - usedSlots;

            var withoutBookBid = interestedOnly
                .Concat(rankedStocks)
                .Where(s => !withBookBid.Contains(s) && !excluded.Contains(s))
                .Distinct()
                .Take(remainingSlots)
                .ToList();

            // Step 3: Combine into displayList (index first, then with/without bid)
            var displayList = new List<string>();
            displayList.AddRange(indexList);      // Fixed index stocks
            displayList.AddRange(withBookBid);    // Main with bid
            displayList.AddRange(withoutBookBid); // Main without bid

            // Step 4: Initialize used tracking lists
            var usedChartAreas = new List<string>();
            var usedAnnotations = new List<string>();
            var usedBookbids = new List<string>();

            // Step 5: Render each chart area and prepare book bids
            foreach (var stock in displayList)
            {
                var data = g.StockRepository.TryGetStockOrNull(stock);
                if (data == null) continue;

                if (indexList.Contains(stock))
                {
                    var area = ChartIndex.UpdateChartArea(chart, data);
                    if (area == null)
                    {
                        Console.WriteLine($"[Warning] Failed to update chart area for index stock: {stock}");
                        continue;
                    }
                    usedChartAreas.Add(area.Name);
                }
                else
                {
                    var (area, anno) = ChartGeneralRenderer.UpdateChartArea(chart, data);
                    if (area == null || anno == null)
                    {
                        Console.WriteLine($"[Warning] Failed to update chart area for stock: {stock}");
                        continue;
                    }
                    usedChartAreas.Add(area.Name);
                    usedAnnotations.Add(anno.Name);
                }

                // Add BookBid if stock is from withBookBid or index
                if (withBookBid.Contains(stock) || indexList.Contains(stock))
                {
                    int row = -1, col = -1;
                    if (stock == indexList[0])
                    {
                        row = 0;
                        col = 0;
                    }
                    else if (stock == indexList[1])
                    {
                        row = 0;
                        col = 2;
                    }
                    else
                    {
                        int index = withBookBid.IndexOf(stock);
                        row = index / 3;
                        col = index % 3 + 2;
                    }

                    if (row < 0) continue;
                    g.BookBidManager.GetOrCreate(stock, row, col);
                    usedBookbids.Add(stock);
                }
            }

            // Step 6: Layout and cleanup
            RelocateChartAreasAndAnnotations(
                g.StockManager.LeverageList,
                withBookBid,
                withoutBookBid);
            RelocateBookbids(usedBookbids);

            CleanupUnusedChartObjects(usedChartAreas, usedAnnotations, usedBookbids);

            DisplayList = GenerateDisplayList(indexList, withBookBid, withoutBookBid, g.nRow, g.nCol);

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

        private void RelocateChartAreasAndAnnotations(
    List<string> leverageStocks,
    List<string> withBookBid,
    List<string> withoutBookBid)
        {
            var chart = g.ChartManager.Chart1;

            int nCol = g.nCol;
            int nRow = g.nRow;
            float cellWidth = 100f / nCol;
            float cellHeight = 100f / nRow;

            bool[,] occupied = new bool[nCol, nRow]; // Track used cells

            // === 1. Leverage Stocks: column 0, rows 0 and 1 ===
            for (int i = 0; i < leverageStocks.Count && i < 2; i++)
            {
                string stock = leverageStocks[i];
                int row = i;
                int col = 0;
                float x = col * cellWidth;
                float y = row * 50f;

                occupied[0, row] = true; // chart
                occupied[1, row] = true; // bookbid

                if (chart.ChartAreas.IndexOf(stock) >= 0)
                {
                    var area = chart.ChartAreas[stock];
                    area.Position = new ElementPosition(x, y, cellWidth, 50f);
                    area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);
                }
            }

            // === 2. WithBookBid: use 2 columns per stock (chart + reserve for bookbid) ===
            int currentCol = 2;
            int currentRow = 0;

            foreach (var stock in withBookBid)
            {
                if (currentRow + 1 > nRow) // If row is full, move to next chart/bookbid column pair
                {
                    currentCol += 2;
                    currentRow = 0;
                }

                if (currentCol + 1 >= nCol) // Safety: no space left
                    break;

                float x = currentCol * cellWidth;
                float y = currentRow * cellHeight;

                occupied[currentCol, currentRow] = true;     // chart
                occupied[currentCol + 1, currentRow] = true; // reserve for bookbid

                if (chart.ChartAreas.IndexOf(stock) >= 0)
                {
                    var area = chart.ChartAreas[stock];
                    area.Position = new ElementPosition(x, y, cellWidth, cellHeight);
                    area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);
                }

                if (!g.StockManager.IndexList.Contains(stock))
                {
                    var annotation = chart.Annotations.FirstOrDefault(a => a.Name == stock);
                    if (annotation is RectangleAnnotation rect)
                    {
                        rect.X = x;
                        rect.Y = y; // + cellHeight;
                        rect.Width = cellWidth;
                        rect.Height = 5.155f + 2f;
                    }
                }

                currentRow++; // Go to next row
            }


            // === 3. WithoutBookBid: place in any unoccupied cell ===
            foreach (var stock in withoutBookBid)
            {
                bool placed = false;
                for (int col = 2; col < nCol && !placed; col++) // skip col 0, 1
                {
                    for (int row = 0; row < nRow && !placed; row++)
                    {

                        if (!occupied[col, row])
                        {
                            float x = col * cellWidth;
                            float y = row * cellHeight;

                            occupied[col, row] = true;

                            if (chart.ChartAreas.IndexOf(stock) >= 0)
                            {
                                var area = chart.ChartAreas[stock];
                                area.Position = new ElementPosition(x, y, cellWidth, cellHeight);
                                area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);
                            }

                            if (!g.StockManager.IndexList.Contains(stock))
                            {
                                var annotation = chart.Annotations.FirstOrDefault(a => a.Name == stock);
                                if (annotation is RectangleAnnotation rect)
                                {
                                    rect.X = x;
                                    rect.Y = y; // + cellHeight;
                                    rect.Width = cellWidth;
                                    rect.Height = 5.155f + 2f;
                                }
                            }

                            placed = true;
                        }
                    }
                }
            }

            chart.Invalidate();
        }



        private void RelocateBookbids(List<string> stockList)
        {
            // Position bookbids below or beside their corresponding chart area
            foreach (var stock in stockList)
            {
                g.BookBidManager.Relocate(stock); // assumes you have a Relocate method
            }
        }


        private List<string> GenerateDisplayList(
    List<string> indexList,
    List<string> withBookBid,
    List<string> withoutBookBid,
    int nRow,
    int nCol)
        {
            int total = 2 + (nCol - 2) * nRow;
            // Initialize fixed-size grid
            var displayList = new List<string>(new string[total]);

            // Step 1: Index stocks at column 0, rows 0 and 1
            if (indexList.Count > 0)
                displayList[0] = indexList[0];
            if (indexList.Count > 1)
                displayList[1] = indexList[1];

            // Step 2: Place withBookBid stocks and their bookbids in columns 2/3, 4/5, etc.
            int row = 0;
            int col = 2;

            foreach (var stock in withBookBid)
            {
                // Safety: if there's no room for bookbid column
                if (col + 1 >= nCol)
                    break;

                int chartIndex = (col - 2) * nRow + row + 2;
                int bookbidIndex = (col - 1) * nRow + row + 2;

                displayList[chartIndex] = stock;       // place stock chart
                displayList[bookbidIndex] = "BookBid";      // reserve bookbid spot

                row++;

                // Move to next chart/bookbid column pair if current column filled
                if (row == nRow)
                {
                    row = 0;
                    col += 2;
                }
            }


            // Step 3: Place remaining withoutBookBid stocks into empty cells (excluding column 0)

            foreach (var stock in withoutBookBid)
            {
                bool placed = false;
                for (int i = 0; i < total; i++)
                {
                    if (displayList[i] == null)
                    {
                        displayList[i] = stock;
                        placed = true;
                        break;
                    }
                }

                if (!placed) break; // no space left
            }

            return displayList;
        }



    }

}
