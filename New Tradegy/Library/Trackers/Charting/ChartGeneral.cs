using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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

        private int _nrow, _ncol;

        public void Initialize(Chart chart, Control parent, int nrow, int ncol)
        {
            _chart = chart;
            _parent = parent;
            _nrow = nrow;
            _ncol = ncol;
            _layout = new ChartGridLayout();
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
                .Concat(interestedWithBid)
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

            RefreshLayout(withBookBid, withoutBookBid);
            _prevWithBid = withBookBid.ToList();
            _prevWithoutBid = withoutBookBid.ToList();

            // chartarea exist ... change end data in each series
            // chartarea not exist ... generate chartarea
            // relocation needed

            //if (changed)
            //{
            //    RefreshLayout(withBookBid, withoutBookBid);
            //    _prevWithBid = withBookBid.ToList();
            //    _prevWithoutBid = withoutBookBid.ToList();
            //}

        }

        //Fully clears and resets the chart(Series, ChartAreas, Annotations) and bookbid grid.
        //Uses ChartGridLayout to determine where to place:
        //stocks with bookbid: takes 2 columns(chart + bookbid)
        //stocks without bookbid: takes 1 column
        //Invokes CreateChartArea() and BookBidManager.GetOrCreate() to place charts and bookbids.
        public void RefreshLayout(List<string> withBookBid, List<string> withoutBookBid)
        {
            _layout.Reset();
            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Annotations.Clear();
            _bookBidManager.Clear();

            string[,] gridMap = new string[3, 8];

            foreach (var stock in withBookBid)
            {
                try
                {
                    var (row, col) = _layout.GetNext(true);
                    CreateChartArea(stock, row, col);
                    _bookBidManager.GetOrCreate(stock, row, col);
                    gridMap[row, col] = stock;
                    gridMap[row, col + 1] = " ";
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Chart layout full — skipping extra withBookBid stocks.");
                    break;
                }
            }

            foreach (var stock in withoutBookBid)
            {
                try
                {
                    var (row, col) = _layout.GetNext(false); // 🛠 corrected: false
                    CreateChartArea(stock, row, col);
                    gridMap[row, col] = stock;
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Chart layout full — skipping extra withoutBookBid stocks.");
                    break;
                }
            }



            // Optionally store or log gridMap for debugging or UI interaction mapping
        }

        //Retrieves the StockData object via TryGetStockOrNull() from StockRepository.
        //Delegates the rendering logic to ChartRenderer.CreateOrUpdateChartarea(...).
        //public void CreateChartArea(string stock, int row, int col)
        //{
        //    var data = g.StockRepository.TryGetStockOrNull(stock);
        //    if (data == null) return;

            
        //    ChartGeneralRenderer.UpdateChartArea(_chart, data, row, col); // Create Chart Area(string stock, int row, int col)
        //}
    }

    public class ChartGridLayout
    {
        private readonly int _rows = 3;
        private readonly int _cols = 8;
        private readonly int _colOffset = 2;  // Reserve first 2 columns
        private bool[,] _occupied;

        public ChartGridLayout()
        {
            _occupied = new bool[_rows, _cols];
        }

        public void Reset()
        {
            _occupied = new bool[_rows, _cols];
        }

        public (int row, int col) GetNext(bool hasBookBid)
        {
            for (int col = _colOffset; col < _cols; col++)
            {
                for (int row = 0; row < _rows; row++)
                {
                    if (hasBookBid)
                    {
                        // Make sure we have room for two columns and both are free
                        if (col + 1 < _cols &&
                            !_occupied[row, col] &&
                            !_occupied[row, col + 1])
                        {
                            _occupied[row, col] = true;
                            _occupied[row, col + 1] = true;
                            return (row, col);
                        }
                    }
                    else
                    {
                        if (!_occupied[row, col])
                        {
                            _occupied[row, col] = true;
                            return (row, col);
                        }
                    }
                }
            }

            throw new InvalidOperationException("No space available for chart layout.");
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