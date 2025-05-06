using New_Tradegy.Library.Core;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;
using static System.Resources.ResXFileRef;

namespace New_Tradegy.Library.Trackers
{
    public class ChartGeneral
    {
        private Chart _chart;
        private Control _parent;
        private ChartGridLayout _layout;
        private BookBidManager _bookBidManager;

        private List<string> _prevWithBid = new List<string>();
        private List<string> _prevWithoutBid = new List<string>();
        private int MaxSpaces = 24;

        public void Initialize(Chart chart, Control parent)
        {
            _chart = chart;
            _parent = parent;
            _layout = new ChartGridLayout(g.nRow - 2, g.nCol);
            _bookBidManager = new BookBidManager(_layout);
        }


        //Categorizes stocks into:
        //withBookBid: holding + interested with bid
        //withoutBookBid: interested only + ranking(excluding duplicates)
        //Enforces layout constraints:
        //Ensures withBookBid.Count * 2 + withoutBookBid.Count <= MaxSpaces
        //Triggers layout refresh only when there’s a meaningful change (via SequenceEqual checks).
        public void UpdateLayoutIfChanged()
        {
            var holdings = g.StockManager.HoldingList;
            var interestedWithBid = g.StockManager.InterestedWithBidList;
            var interestedOnly = interestedWithBid.Except(holdings).ToList();
            var rankedStocks = g.StockManager.StockRankingList;

            var withBookBid = holdings
                .Concat(interestedOnly)
                .Distinct()
                .Take(MaxSpaces / 2)
                .ToList();

            int usedSpaces = withBookBid.Count * 2;
            int remainingSpaces = MaxSpaces - usedSpaces;

            var withoutBookBid = rankedStocks
                .Where(s => !withBookBid.Contains(s))
                .Distinct()
                .Take(remainingSpaces)
                .ToList();

            bool changed = !_prevWithBid.SequenceEqual(withBookBid)
                        || !_prevWithoutBid.SequenceEqual(withoutBookBid);

            if (changed)
            {
                RefreshGeneralLayout(withBookBid, withoutBookBid);
                _prevWithBid = withBookBid.ToList();
                _prevWithoutBid = withoutBookBid.ToList();
            }
        }

        //Fully clears and resets the chart(Series, ChartAreas, Annotations) and bookbid grid.
        //Uses ChartGridLayout to determine where to place:
        //stocks with bookbid: takes 2 columns(chart + bookbid)
        //stocks without bookbid: takes 1 column
        //Invokes CreateChartArea() and BookBidManager.GetOrCreate() to place charts and bookbids.
        public void RefreshGeneralLayout(List<string> withBookBid, List<string> withoutBookBid)
        {
            _layout.Reset();
            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Annotations.Clear();
            _bookBidManager.Clear();

            string[,] gridMap = new string[3, 8];

            foreach (var stock in withBookBid)
            {
                var (row, col) = _layout.GetNext(true);
                CreateChartArea(stock, row, col);
                _bookBidManager.GetOrCreate(stock, row, col);
                gridMap[row, col] = stock;
                gridMap[row, col + 1] = " "; // bookbid placeholder
            }

            foreach (var stock in withoutBookBid)
            {
                var (row, col) = _layout.GetNext(false);
                CreateChartArea(stock, row, col);
                gridMap[row, col] = stock;
            }

            // Optionally store or log gridMap for debugging or UI interaction mapping
        }


        //Retrieves the StockData object via TryGetStockOrNull() from StockRepository.
        //Delegates the rendering logic to ChartRenderer.CreateOrUpdateChart(...).
        public void CreateChartArea(string stock, int row, int col)
        {
            var data = g.StockRepository.TryGetStockOrNull(stock);
            ChartRenderer.CreateOrUpdateChart(_chart, data, row, col); // CreateChartArea(string stock, int row, int col)
        }
    }

    public class ChartGridLayout
    {
        private int _rows, _cols;
        private int _currentRow = 0;
        private int _currentCol = 2;

        //itialize _rows and _cols based on the layout settings(usually g.nRow - 2 and g.nCol).
        public ChartGridLayout(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
        }

        //resets placement.Always starts from (0, 2) — reserving col 0, 1 
        //for index chartareas — consistent with your layout rule
        public void Reset()
        {
            _currentRow = 0;
            _currentCol = 2;
        }


        //Bookbid entries take 2 columns.
        //Non-bookbid take 1 column.
        //When row exceeds limit, wrap to next column.
        public (int row, int col) GetNext(bool hasBookBid)
        {
            int row = _currentRow;
            int col = _currentCol;

            _currentRow++;
            if (_currentRow >= _rows)
            {
                _currentRow = 0;
                _currentCol += hasBookBid ? 2 : 1;
            }

            return (row, col);
        }

        //Converts grid cell to pixel coordinates using:
        //screen width / grid columns
        //screen height / grid rows
        //Adds a margin(+ chartWidth + 10) to move the bookbid next to its chart.All good.
        public Point GetBookBidLocation(int row, int col)
        {
            int chartWidth = g.screenWidth / g.nCol;
            int chartHeight = g.screenHeight / g.nRow;
            return new Point(chartWidth * col + chartWidth + 10, chartHeight * row);
        }
    }

    public static class StockManagerEvents
    {
        public static event Action ListsChanged;

        public static void NotifyChanged()
        {
            ListsChanged?.Invoke();
        }
    }
}