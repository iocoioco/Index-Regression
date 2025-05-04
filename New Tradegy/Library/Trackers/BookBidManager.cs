using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using New_Tradegy.Library.Listeners;

namespace New_Tradegy.Library.Trackers
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;


    namespace New_Tradegy.Library.Trackers
    {
        /// <summary>
        /// Manages creation, layout, visibility, and interaction of book bid DataGridViews for stocks.
        /// </summary>
        public class BookBidManager
        {
            private readonly Dictionary<string, BookBidGenerator> _jpMap = new Dictionary<string, BookBidGenerator>();
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

                var generator = new BookBidGenerator();  // was "jp"
                _jpMap[stock] = generator;

                var grid = generator.GenerateBookBidView(stock);
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

                if (_jpMap.TryGetValue(stock, out var generator))
                {
                    generator.Unsubscribe();
                    _jpMap.Remove(stock);
                }
            }

            public bool Exists(string stock) => _gridMap.ContainsKey(stock);

            public void OpenConfirmation(string stock, bool isSell, int qty, int price, int urgency, string message)
            {
                if (_jpMap.TryGetValue(stock, out var generator))
                {
                    generator.OpenOrUpdateConfirmationForm(isSell, stock, qty, price, urgency, message);
                }
            }

            public void Clear()
            {
                foreach (var grid in _gridMap.Values)
                {
                    if (grid.Parent != null)
                        g.MainForm.Invoke((MethodInvoker)(() => grid.Parent.Controls.Remove(grid)));

                    grid.Dispose();
                }

                foreach (var generator in _jpMap.Values)
                {
                    generator.Unsubscribe();
                }

                _gridMap.Clear();
                _jpMap.Clear();
                _layout.Reset();
            }
        }
    }
}
