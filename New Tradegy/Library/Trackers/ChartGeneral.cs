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
    public class ChartGeneral
    {
        private Chart _chart;
        private Control _parent;
        private ChartGridLayout _layout;
        private BookBidManager _bookBids;

        private List<string> _prevWithBid = new List<string>();
        private List<string> _prevWithoutBid = new List<string>();
        private int MaxSpaces = 24;

        public void Initialize(Chart chart, Control parent)
        {
            _chart = chart;
            _parent = parent;
            _layout = new ChartGridLayout(g.nRow - 2, g.nCol);
            _bookBids = new BookBidManager(_layout);
        }


        public void UpdateLayoutIfChanged()
        {
            var holdings = g.StockManager.HoldingList;
            var interestedWithBid = g.StockManager.InterestedWithBidList;
            var interestedOnly = interestedWithBid.Except(holdings).ToList();
            var ranking = g.StockManager.RankingList;

            var withBookBid = holdings.Concat(interestedWithBid.Except(holdings)).Distinct().ToList();
            var withoutBookBid = interestedOnly.Concat(ranking).Where(s => !withBookBid.Contains(s)).Distinct().ToList();

            // Enforce max space constraint: withBookBid.Count * 2 + withoutBookBid.Count <= 24
            int availableSpaces = MaxSpaces;
            int maxWithBookBid = Math.Min(withBookBid.Count, MaxSpaces / 2);
            withBookBid = withBookBid.Take(maxWithBookBid).ToList();
            availableSpaces -= withBookBid.Count * 2;
            withoutBookBid = withoutBookBid.Take(availableSpaces).ToList();

            bool changed =
                !_prevWithBid.SequenceEqual(withBookBid) ||
                !_prevWithoutBid.SequenceEqual(withoutBookBid);

            if (changed)
            {
                RefreshChart1Layout(withBookBid, withoutBookBid);
                _prevWithBid = withBookBid.ToList();
                _prevWithoutBid = withoutBookBid.ToList();
            }
        }

        public void RefreshChart1Layout(List<string> withBookBid, List<string> withoutBookBid)
        {
            _layout.Reset();
            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Annotations.Clear();
            _bookBids.Clear();

            string[,] gridMap = new string[3, 8];

            foreach (var stock in withBookBid)
            {
                var (row, col) = _layout.GetNext(true);
                CreateChartArea(stock, row, col);
                _bookBids.GetOrCreate(stock, row, col);
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

        public void CreateChartArea(string stock, int row, int col)
        {
            var data = StockRepository.Instance.GetOrThrow(stock);
            ChartRenderer.CreateOrUpdateChart(_chart, data, row, col);
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
        private readonly Dictionary<string, jp> _jpMap = new Dictionary<string, jp>();
        private readonly Dictionary<string, DataGridView> _gridMap = new Dictionary<string, DataGridView>();
        private readonly ChartGridLayout _layout;

        public BookBidManager(ChartGridLayout layout)
        {
            _layout = layout;
        }

        public DataGridView GetOrCreate(string stock, int row, int col)
        {
            if (_gridMap.TryGetValue(stock, out var existingGrid))
                return existingGrid;

            var jpInstance = new jp();
            _jpMap[stock] = jpInstance;

            var grid = jpInstance.Generate(stock);
            _gridMap[stock] = grid;

            grid.Location = _layout.GetBookBidLocation(row, col);
            grid.Visible = true;

            g.MainForm.Invoke((MethodInvoker)(() => g.MainForm.Controls.Add(grid)));

            return grid;
        }

        public void Remove(string stock)
        {
            if (_gridMap.TryGetValue(stock, out var grid))
            {
                if (grid.Parent != null)
                    g.MainForm.Invoke((MethodInvoker)(() => g.MainForm.Controls.Remove(grid)));
                grid.Dispose();
                _gridMap.Remove(stock);
            }

            _jpMap.Remove(stock);
        }

        public bool Exists(string stock)
        {
            return _gridMap.ContainsKey(stock);
        }

        public void OpenConfirmation(string stock, bool isSell, int qty, int price, int urgency, string message)
        {
            if (_jpMap.TryGetValue(stock, out var jpInstance))
            {
                jpInstance.OpenOrUpdateConfirmationForm(isSell, stock, qty, price, urgency, message);
            }
        }

        public void Clear()
        {
            foreach (var grid in _gridMap.Values)
            {
                if (grid.Parent != null)
                    grid.Parent.Controls.Remove(grid);
                grid.Dispose();
            }

            _gridMap.Clear();
            _jpMap.Clear();
            _layout.Reset();
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