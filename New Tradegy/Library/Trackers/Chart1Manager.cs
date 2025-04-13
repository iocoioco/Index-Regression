using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Utils;

namespace New_Tradegy.Library.Trackers
{
    public class Chart1Manager
    {
        private Chart _chart;
        private Control _parent;
        private ChartGridLayout _layout;
        private BookBidManager _bookBids;

        public void Initialize(Chart chart, Control parent)
        {
            _chart = chart;
            _parent = parent;
            _layout = new ChartGridLayout(g.nRow, g.nCol);
            _bookBids = new BookBidManager(_parent);
        }

        public void RefreshDisplay()
        {
            // Step 1: Holding
            var holding = g.StockManager.HoldingList.Distinct().ToList();

            // Step 2: InterestedWithBid (excluding holding)
            var withBid = g.StockManager.InterestedWithBidList
                            .Where(s => !holding.Contains(s))
                            .Distinct()
                            .ToList();

            // Step 3: InterestedOnly (excluding holding + withBid)
            var onlyChart = g.StockManager.InterestedOnlyList
                              .Where(s => !holding.Contains(s) && !withBid.Contains(s))
                              .Distinct()
                              .ToList();

            // Step 4: RankingList (excluding all prior)
            var ranked = g.StockManager.RankingList
                           .Where(s => !holding.Contains(s) && !withBid.Contains(s) && !onlyChart.Contains(s))
                           .Distinct()
                           .ToList();

            var stocksWithBid = holding.Concat(withBid).ToList();
            var onlyCharts = onlyChart.Concat(ranked).ToList();

            Display(stocksWithBid, onlyCharts);
        }

        public void Display(List<string> stocksWithBid, List<string> onlyChart)
        {
            _layout.Reset();

            foreach (string stock in stocksWithBid)
            {
                var data = StockRepository.Instance.GetOrThrow(stock);
                (int row, int col) = _layout.GetNext(true);

                ChartRenderer.CreateOrUpdateChart(_chart, data, row, col);

                var bidView = _bookBids.GetOrCreate(stock);
                bidView.Location = _layout.GetBookBidLocation(row, col);
                _parent.Controls.Add(bidView);
            }

            foreach (string stock in onlyChart)
            {
                var data = StockRepository.Instance.GetOrThrow(stock);
                (int row, int col) = _layout.GetNext(false);

                ChartRenderer.CreateOrUpdateChart(_chart, data, row, col);
            }
        }

        public void ClearUnused(List<string> activeStocks)
        {
            ChartRenderer.ClearUnused(_chart, activeStocks);
            _bookBids.RemoveUnused(activeStocks);
        }
    }

    public class ChartGridLayout
    {
        private int _rows, _cols;
        private int _currentRow = 0;
        private int _currentCol = 2;

        public ChartGridLayout(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
        }

        public void Reset()
        {
            _currentRow = 0;
            _currentCol = 2;
        }

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

        public Point GetBookBidLocation(int row, int col)
        {
            int chartWidth = g.screenWidth / g.nCol;
            int chartHeight = g.screenHeight / g.nRow;
            return new Point(chartWidth * col + chartWidth + 10, chartHeight * row);
        }
    }

    public class BookBidManager
    {
        private Dictionary<string, jp> _map = new();
        private Control _parent;

        public BookBidManager(Control parent)
        {
            _parent = parent;
        }

        public DataGridView GetOrCreate(string stock)
        {
            if (!_map.ContainsKey(stock))
            {
                var jpObj = new jp();
                _map[stock] = jpObj;
                return jpObj.Generate(stock);
            }
            return _map[stock].View;
        }

        public void RemoveUnused(List<string> activeStocks)
        {
            var removeList = new List<string>();

            foreach (var kv in _map)
            {
                if (!activeStocks.Contains(kv.Key))
                {
                    _parent.Controls.Remove(kv.Value.View);
                    removeList.Add(kv.Key);
                }
            }

            foreach (var key in removeList)
                _map.Remove(key);
        }

        public void ClearAll()
        {
            foreach (var item in _map.Values)
                _parent.Controls.Remove(item.View);
            _map.Clear();
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
