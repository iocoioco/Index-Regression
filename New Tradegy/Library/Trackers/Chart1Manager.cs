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

        public void DisplayAll()
        {
            _layout.Reset();

            foreach (var stock in g.StockManager.HoldingList)
                AddChart(stock, true);

            foreach (var stock in g.StockManager.InterestedWithBidList)
            {
                if (!g.StockManager.HoldingList.Contains(stock))
                    AddChart(stock, true);
            }

            foreach (var stock in g.StockManager.InterestedOnlyList)
            {
                if (!g.StockManager.HoldingList.Contains(stock) &&
                    !g.StockManager.InterestedWithBidList.Contains(stock))
                    AddChart(stock, false);
            }

            foreach (var stock in g.StockManager.RankingList)
            {
                if (!g.StockManager.HoldingList.Contains(stock) &&
                    !g.StockManager.InterestedWithBidList.Contains(stock) &&
                    !g.StockManager.InterestedOnlyList.Contains(stock))
                    AddChart(stock, false);
            }
        }

        private void AddChart(string stock, bool withBookBid)
        {
            var data = StockRepository.Instance.GetOrThrow(stock);
            var (row, col) = _layout.GetNext(withBookBid);

            ChartRenderer.CreateOrUpdateChart(_chart, data, row, col);

            if (withBookBid)
            {
                var bidView = _bookBids.GetOrCreate(stock);
                bidView.Location = _layout.GetBookBidLocation(row, col);
                _parent.Controls.Add(bidView);
            }
        }

        public void ClearUnused(List<string> activeStocks)
        {
            ChartRenderer.ClearUnused(_chart, activeStocks);
            _bookBids.RemoveUnused(activeStocks);
        }

        public void RefreshDisplay()
        {
            // regroup the lists as needed here or pass from outside
            var stocksWithBid = g.StockManager.InterestedWithBidList.Concat(g.StockManager.HoldingList).Distinct().ToList();
            var onlyCharts = g.StockManager.InterestedOnlyList.Concat(g.StockManager.RankingList).Distinct()
                               .Where(s => !stocksWithBid.Contains(s)).ToList();

            Display(stocksWithBid, onlyCharts); // calls Chart1Manager.Display()
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
