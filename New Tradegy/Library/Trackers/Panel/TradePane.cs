using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;
using System.Linq;
using New_Tradegy.Library.Deals;

using New_Tradegy.Library.Utils;

namespace New_Tradegy.Library.Trackers
{
    public class TradePane
    {
        private readonly DataGridView _view;
        private readonly DataTable _table;

        public Control View => _view;

        public TradePane(DataGridView dgv, DataTable dtb)
        {
            _table = dtb;
            _view = dgv;
            
            InitializeDgv(_view);
            BindGrid(_view); // has InitializeSetting()
        }

        private void InitializeDgv(DataGridView _view)
        {
            // x 212
            int fontSize = 8; 
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;

            int x = g.screenWidth / g.nCol + 20;
            int y = 336 + g.CellHeight * 3 + 3; // + 30 deleted from width
            _view.Location = new Point(x, y);
            int width = g.screenWidth / 10 - 20;
            int height = 250;
            _view.Size = new Size(width, height);

              
            _view.ColumnHeadersVisible = false;
            _view.RowHeadersVisible = false;

            _view.ReadOnly = true;
            _view.DefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            _view.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            _view.RowTemplate.Height = g.cellHeight - 3;
            _view.ForeColor = Color.Black;

            _view.ScrollBars = ScrollBars.None;
            _view.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _view.AllowUserToResizeColumns = false;
            _view.AllowUserToResizeRows = false;
            _view.AllowUserToAddRows = false;
            _view.AllowUserToDeleteRows = false;

            _view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _view.TabIndex = 1;

            _view.CellFormatting += CellFormatting;
            _view.CellMouseClick += CellMouseClick;
        }

        private void BindGrid(DataGridView _view)
        {
            // Setup columns
            _table.Columns.Add("0"); // stock
            _table.Columns.Add("1"); // 매수/매도
            _table.Columns.Add("2"); // 가격
            _table.Columns.Add("3"); // 거래진행

            // Add rows
            int Rows = 10; // or configurable
            for (int j = 0; j < Rows; j++)
                _table.Rows.Add("", "", "", "");

            // Bind table to DataGridView
            _view.DataSource = _table;

            // Optional: column widths
            _view.Columns[0].Width = (int)(_view.Width * 0.20); // stock
            _view.Columns[1].Width = (int)(_view.Width * 0.20); // 매수/매도
            _view.Columns[2].Width = (int)(_view.Width * 0.25); // 가격
            _view.Columns[3].Width = (int)(_view.Width * 0.35); // 거래진행

            _view.Visible = true;
        }

        public void Update()
        {
            lock (OrderItemTracker.orderLock)
            {
                _view.SuspendLayout();

                int rowCount = 0;

                if (OrderItemTracker.OrderMap != null)
                {
                    foreach (var data in OrderItemTracker.OrderMap.Values)
                    {
                        _table.Rows[rowCount][0] = data.stock;
                        _table.Rows[rowCount][1] = data.buyorSell;
                        _table.Rows[rowCount][2] = data.m_nPrice;
                        _table.Rows[rowCount][3] = data.m_nContAmt + "/" + data.m_nAmt;
                        rowCount++;
                    }
                }

                FillEmptyRow(rowCount++);

                int 순서 = 0;
                foreach (var stock in g.StockManager.HoldingList.ToList())
                {
                    var data = g.StockRepository.TryGetStockOrNull(stock);
                    if (data == null) return;

                    if (data.Api.매수1호가 > 0)
                        data.Deal.수익률 = (double)(data.Api.매수1호가 - data.Deal.장부가) / data.Api.매수1호가 * 100;

                    _table.Rows[rowCount][0] = data.Stock;
                    _table.Rows[rowCount][1] = Math.Round((data.Api.매수1호가 / 10000.0), 4);
                    _table.Rows[rowCount][2] =
                        data.Api.최우선매도호가잔량 > 0
                        ? Math.Round((double)data.Api.최우선매수호가잔량 / data.Api.최우선매도호가잔량, 2).ToString()
                        : "";
                    _table.Rows[rowCount][3] = data.Deal.보유량 + "/" + Math.Round(data.Deal.수익률, 2);

                    UpdateColorAndSound(data, 순서, rowCount);

                    rowCount++;
                    순서++;
                    if (rowCount == 10) break;
                }

                for (int i = rowCount; i < _table.Rows.Count; i++)
                {
                    FillEmptyRow(i);
                }

                _view.ResumeLayout();
            }
        }



        private void CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.RowIndex < 0 || e.RowIndex >= _view.Rows.Count)
                return; // Ignore right-clicks or invalid rows

            string stock = _view.Rows[e.RowIndex].Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(stock)) return;

            switch (e.ColumnIndex)
            {
                case 1: // 체결중 주문 취소
                    if (g.test) return;

                    if (e.RowIndex < OrderItemTracker.OrderMap.Count)
                    {
                        DealManager.DealCancelRowIndex(e.RowIndex); // cancel order
                        SoundUtils.Sound("Keys", "cancel");
                    }
                    break;

                case 2: // 매도
                    if (g.test) return;
                    {
                        //string buySell = "매도";
                        //int 거래가격 = hg.HogaGetValue(stock, 0, 1); // 0: 매도호가 라인, 1: 컬럼

                        //int Urgency = 100;
                        //if (g.optimumTrading)
                        //{
                        //    Urgency = (int)(e.X / (double)_view.Columns[2].Width * 100);
                        //}

                        // DealManager.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;

                case 3: // 매수
                    if (g.test) return;
                    {
                        //string buySell = "매수";
                        //int 거래가격 = hg.HogaGetValue(stock, -1, 1); // -1: 매수호가 라인, 1: 컬럼

                        //int Urgency = 100;
                        //if (g.optimumTrading)
                        //{
                        //    Urgency = (int)(e.X / (double)_view.Columns[2].Width * 100);
                        //}

                        // dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;
            }
        }


        private void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            _view.Rows[0].DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);

        }

        private void FillEmptyRow(int row)
        {
            _table.Rows[row][0] = " ";
            _table.Rows[row][1] = " ";
            _table.Rows[row][2] = " ";
            _table.Rows[row][3] = " ";
        }

        private void UpdateColorAndSound(StockData o, int index, int row)
        {
            string[] names = { "one", "two", "three" };
            if (index < names.Length && o.Deal.보유량 * o.Api.현재가 > 500000)
            {
                string postfix = o.Deal.전수익률 == o.Deal.수익률 ? "" : (o.Deal.전수익률 < o.Deal.수익률 ? " up" : " down");
                Utils.SoundUtils.Sound("가", names[index] + postfix);
            }

            int red = 255, green = 255;
            if (o.Deal.수익률 > 0)
                red = Math.Max(0, 255 - (int)(255.0 * o.Deal.수익률 / 10.0));
            else if (o.Deal.수익률 < 0)
                green = Math.Max(0, 255 + (int)(255.0 * o.Deal.수익률 / 10.0));

            _view.Rows[row].DefaultCellStyle.BackColor = Color.FromArgb(red, green, 255);
            o.Deal.전수익률 = o.Deal.수익률;
        }



        public void SetCellValue(int row, int col, object value)
        {
            _table.Rows[row][col] = value;
        }

        public string GetCellValue(int row, int col)
        {
            return _table.Rows[row][col]?.ToString() ?? string.Empty;
        }

        public bool HasRows()
        {
            return _table != null && _table.Rows.Count > 0;
        }
    }


}
