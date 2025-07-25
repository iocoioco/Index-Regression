﻿using New_Tradegy.Library.Models;
using New_Tradegy.Library.PostProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.IO;
using System.Drawing;

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


    public class ChartMain
    {
        private Chart chart => g.ChartManager.Chart1;

        public List<string> DisplayList { get; private set; }
        public void RefreshMainChart()
        {
            // Reference to the chart
            var chart = g.ChartManager.Chart1;

            // Constants and major collections (defined clearly at the top)
            const int MaxChartSlots = 24;

            var leverageList = g.StockManager.LeverageList;
            var holdings = g.StockManager.HoldingList;
            var interestedWithBid = g.StockManager.InterestedWithBidList;
            var interestedOnly = g.StockManager.InterestedOnlyList;

            List<string> rankedStocks;
            if(g.v.MainChartDisplayMode == "등합")
                rankedStocks = g.StockManager.StockRankingList;
           else
                rankedStocks = g.StockManager.StockRankingByModesList;

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
                .Concat(rankedStocks.Skip(g.gid))
                .Where(s => !withBookBid.Contains(s) && !excluded.Contains(s))
                .Distinct()
                .Take(remainingSlots)
                .ToList();

            // Step 3: Combine into displayList (index first, then with/without bid)
            var displayList = new List<string>();
            displayList.AddRange(leverageList);      // Fixed index stocks
            displayList.AddRange(withBookBid);    // Main with bid
            displayList.AddRange(withoutBookBid); // Main without bid

            // Step 4: Initialize used tracking lists
            var usedChartAreas = new List<string>();
            var usedAnnotations = new List<string>();
            var usedBookbids = new List<string>();

            
            int areasCount = g.ChartManager.Chart1.ChartAreas.Count;
            int annotationsCount = g.ChartManager.Chart1.Annotations.Count;
            int seriesCount = g.ChartManager.Chart1.Series.Count;

            
            // Step 5: Render each chart area and prepare book bids
            foreach (var stock in displayList)
            {
                var data = g.StockRepository.TryGetDataOrNull(stock);
                if (data == null) continue;

                if (leverageList.Contains(stock))
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
                    var (area, anno) = ChartGeneral.UpdateChartArea(chart, data);
                    if (area == null || anno == null)
                    {
                        Console.WriteLine($"[Warning] Failed to update chart area for stock: {stock}");
                        continue;
                    }
                    usedChartAreas.Add(area.Name);
                    usedAnnotations.Add(anno.Name);
                }

                // Add BookBid if stock is from withBookBid or index
                if (withBookBid.Contains(stock) || leverageList.Contains(stock))
                {
                    int row = -1, col = -1;
                    if (stock == leverageList[0])
                    {
                        row = 0;
                        col = 0;
                    }
                    else if (stock == leverageList[1])
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
                    g.BookBidManager.GetOrCreate(stock);
                    usedBookbids.Add(stock);
                }
            }

            areasCount = g.ChartManager.Chart1.ChartAreas.Count;
            annotationsCount = g.ChartManager.Chart1.Annotations.Count;
            seriesCount = g.ChartManager.Chart1.Series.Count;

            // Step 6: Layout and cleanup
            RelocateChartAreasAndAnnotations( // done
                g.StockManager.LeverageList,
                withBookBid,
                withoutBookBid);
            RelocateBookbids(usedBookbids); // done

            CleanupUnusedChartObjects(usedChartAreas, usedAnnotations, usedBookbids); // done

            DisplayList = GenerateDisplayList(leverageList, withBookBid, withoutBookBid, g.nRow, g.nCol);
        
            chart.Invalidate();

            areasCount = g.ChartManager.Chart1.ChartAreas.Count;
            annotationsCount = g.ChartManager.Chart1.Annotations.Count;
            seriesCount = g.ChartManager.Chart1.Series.Count;
        }


        private void CleanupUnusedChartObjects(
    List<string> keepAreas,
    List<string> keepAnnotations,
    List<string> keepBookbids)
        {
            foreach (var area in chart.ChartAreas.ToList())
            {
                if (area == null || string.IsNullOrEmpty(area.Name))
                {
                    Console.WriteLine("[Warning] Null or unnamed ChartArea encountered during cleanup");
                    continue;
                }

                if (!keepAreas.Contains(area.Name))
                {
                    chart.ChartAreas.Remove(area);
                }
            }

            foreach (var anno in chart.Annotations.ToList())
            {
                if (anno == null || string.IsNullOrEmpty(anno.Name))
                {
                    Console.WriteLine("[Warning] Null or unnamed Annotation encountered during cleanup");
                    continue;
                }

                if (!keepAnnotations.Contains(anno.Name))
                {
                    chart.Annotations.Remove(anno);
                }
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
                if (string.IsNullOrEmpty(stock)) continue;

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
                    // area.InnerPlotPosition = new ElementPosition(10, 10, 80, 80); //(5, 10, 90, 80);


                    area.InnerPlotPosition = ChartBasic.CalculateInnerPlotPosition(cellWidth, 50f);

                    if (!area.Visible)
                        area.Visible = true;
                }
            }

            // === 2. WithBookBid: use 2 columns per stock (chart + reserve for bookbid) ===
            int currentCol = 2;
            int currentRow = 0;

            foreach (var stock in withBookBid)
            {
                if (string.IsNullOrEmpty(stock)) continue;

                if (currentRow + 1 > nRow)
                {
                    currentCol += 2;
                    currentRow = 0;
                }

                if (currentCol + 1 >= nCol)
                    break;

                float x = currentCol * cellWidth;
                float y = currentRow * cellHeight;

                occupied[currentCol, currentRow] = true;
                occupied[currentCol + 1, currentRow] = true;

                if (chart.ChartAreas.IndexOf(stock) >= 0)
                {
                    var area = chart.ChartAreas[stock];
                    area.Position = new ElementPosition(x, y, cellWidth, cellHeight);
                    
                    area.InnerPlotPosition = ChartBasic.CalculateInnerPlotPosition(cellWidth, cellHeight);

                    // area.InnerPlotPosition = new ElementPosition(20, 10, 60, 80); //(5, 10, 90, 80);

                    if (!area.Visible)
                        area.Visible = true;
                }

                if (!g.StockManager.IndexList.Contains(stock))
                {
                    var anno = chart.Annotations.FirstOrDefault(a => a.Name == stock);
                    if (anno is RectangleAnnotation rect)
                    {
                        rect.X = x;
                        rect.Y = y;
                        rect.Width = cellWidth;
                        rect.Height = 5.155f + 2f;
                    }

                    if (anno != null && !anno.Visible)
                        anno.Visible = true;
                }

                currentRow++;
            }

            // === 3. WithoutBookBid: place in any unoccupied cell ===
            foreach (var stock in withoutBookBid)
            {
                if (string.IsNullOrEmpty(stock)) continue;

                bool placed = false;
                for (int col = 2; col < nCol && !placed; col++)
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
                                // area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);
                                area.InnerPlotPosition = ChartBasic.CalculateInnerPlotPosition(cellWidth, cellHeight);
                                if (!area.Visible)
                                    area.Visible = true;
                            }

                            if (!g.StockManager.IndexList.Contains(stock))
                            {
                                var anno = chart.Annotations.FirstOrDefault(a => a.Name == stock);
                                if (anno is RectangleAnnotation rect)
                                {
                                    rect.X = x;
                                    rect.Y = y;
                                    rect.Width = cellWidth;
                                    rect.Height = 5.155f + 2f;
                                }

                                if (anno != null && !anno.Visible)
                                    anno.Visible = true;
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
            foreach (var stock in stockList)
            {
                if (string.IsNullOrEmpty(stock))
                {
                    Console.WriteLine("[Warning] Empty or null stock name in RelocateBookbids");
                    continue;
                }

                g.BookBidManager.Relocate(stock);
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


        public void DisplayDatewiseStockHistory(string stock, int offset)
        {
            g.ChartManager.Chart1Handler?.Clear();                 // ✅ Clear Chart1 only
            g.BookBidManager.CleanupAllExcept(Enumerable.Empty<string>());         // ✅ Clear all bookbids

            var chart = g.ChartManager.Chart1;

            var folders = Directory.GetDirectories(@"C:\병신\분\")
                .Select(Path.GetFileName)
                .Where(name => int.TryParse(name, out _))
                .OrderByDescending(name => name)
                .ToList();

            int startIndex = offset * 24;
            var selectedDates = folders.Skip(startIndex).Take(24).ToList();

            
            int row = 0, col = 0;

            foreach (var date in selectedDates)
            {
                string file = $@"C:\병신\분\{date}\{stock}.txt";
                if (!File.Exists(file)) continue;

                var sd = new StockData();
                sd.Stock = g.clickedStock;
                FileInStockData.LoadStockData(sd, file);  // ✅ now correct
                if(sd.Api.nrow < 2)
                {
                    row++;
                    if (row >= 3)
                    {
                        row = 0;
                        col++;
                    }
                    continue;
                }

                string MMdd = date.Length >= 8 ? date.Substring(4, 4) : stock;
                sd.Stock = MMdd;
                g.StockRepository.AddDateStock(MMdd, sd);

                PostProcessor.post(sd);  // ✅ process once
     
                var (area, anno) = ChartGeneral.UpdateChartArea(chart, sd);

                // === 🧭 Relocate immediately to col 2–9, row 0–2 ===
                float cellWidth = 100f / g.nCol;
                float cellHeight = 100f / g.nRow;
                float margin = 0.0f;

                int absCol = 2 + col;
                float x = absCol * cellWidth + margin;
                float y = row * cellHeight;
                float width = cellWidth - 2 * margin;

                if (area != null)
                {
                    area.Position = new ElementPosition(x, y, width, cellHeight);
                    //area.InnerPlotPosition = ChartBasic.CalculateInnerPlotPosition(width, cellHeight);
                    //area.BackColor = Color.FromArgb(240, 248, 255); // light blue-gray
                    //area.BorderColor = Color.Silver;
                    //area.BorderDashStyle = ChartDashStyle.Solid;
                    //area.BorderWidth = 1;
                    area.Visible = true;
                }

                if (anno is RectangleAnnotation rect)
                {
                    rect.X = x;
                    rect.Y = y;
                    //rect.Width = width;
                    //rect.Height = 7.155f;
                    //rect.LineColor = Color.Gray;
                    rect.BackColor = Color.White;
                    //rect.ForeColor = Color.Black;
                    //rect.Font = new Font("맑은 고딕", 8f, FontStyle.Bold);
                    rect.Visible = true;
                }

                // === Move down and to the right ===
                row++;
                if (row >= 3)
                {
                    row = 0;
                    col++;
                }

                g.StockRepository.RemoveStock(date);
            }

            chart.Invalidate();  // ✅ refresh the chart after drawing
        }
    }
}
