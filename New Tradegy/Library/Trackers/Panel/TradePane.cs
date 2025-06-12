using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;
using System.Linq;
using New_Tradegy.Library.Deals;

namespace New_Tradegy.Library.Trackers
{
    public class TradePane
    {
        private readonly DataGridView _view;
        private readonly DataTable _table;

        public TradePane(Form containerForm)
        {
            var dgv = new DataGridView();
            containerForm.Controls.Add(dgv);

            Initialize(dgv);

            _view = view;
            _table = table;

            _view.CellFormatting += CellFormatting;
            _view.CellMouseClick += CellMouseClick;
        }

        public TradePane(DataGridView dgv, DataTable dtb)
        {
            _table = dtb;
            _view = dgv;
            dgv.DataSource = _table;
            InitializeDgv(dgv);
            BindGrid(dgv); // has InitializeSetting()
        }

        private void InitializeDgv(DataGridView dgv, int width, int height)
        {
            int fontSize = 10;

            dgv.Location = new Point(0, 0);
            dgv.Size = new Size(width, height);

            dgv.DataError += (s, f) => wr.DataGridView_DataError(s, f, "매매 dgv");
            dgv.DataSource = g.제어.dtb;
            dgv.ColumnHeadersVisible = false;
            dgv.RowHeadersVisible = false;

            dgv.ReadOnly = true;
            dgv.DefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            dgv.RowTemplate.Height = g.formSize.ch - 1;
            dgv.ForeColor = Color.Black;

            dgv.ScrollBars = ScrollBars.None;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;

            dgv.Dock = DockStyle.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.TabIndex = 1;

            dgv.CellFormatting += new DataGridViewCellFormattingEventHandler(매매_CellFormatting);
            dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(매매_CellMouseClick);
            dgv.KeyPress += 매매_KeyPress;
        }

        private void BindGrid(DataGridView dgv)
        {
            // Setup columns
            _table.Columns.Clear();
            _table.Columns.Add("0"); // stock
            _table.Columns.Add("1"); // 매수/매도
            _table.Columns.Add("2"); // 가격
            _table.Columns.Add("3"); // 거래진행

            // Add rows
            int Rows = 10; // or configurable
            for (int j = 0; j < Rows; j++)
                _table.Rows.Add("", "", "", "");

            // Bind table to DataGridView
            dgv.DataSource = _table;

            // Optional: column widths
            dgv.Columns[0].Width = (int)(dgv.Width * 0.20); // stock
            dgv.Columns[1].Width = (int)(dgv.Width * 0.20); // 매수/매도
            dgv.Columns[2].Width = (int)(dgv.Width * 0.25); // 가격
            dgv.Columns[3].Width = (int)(dgv.Width * 0.35); // 거래진행

            dgv.Visible = true;
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

                    if (e.RowIndex < g.m_mapOrder.Count)
                    {
                        dl.DealCancelRowIndex(e.RowIndex); // cancel order
                        ms.Sound("Keys", "cancel");
                    }
                    break;

                case 2: // 매도
                    if (g.test) return;

                    {
                        string buySell = "매도";
                        int 거래가격 = hg.HogaGetValue(stock, 0, 1); // 0: 매도호가 라인, 1: 컬럼

                        int Urgency = 100;
                        if (g.optimumTrading)
                        {
                            Urgency = (int)(e.X / (double)_view.Columns[2].Width * 100);
                        }

                        dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;

                case 3: // 매수
                    if (g.test) return;

                    {
                        string buySell = "매수";
                        int 거래가격 = hg.HogaGetValue(stock, -1, 1); // -1: 매수호가 라인, 1: 컬럼

                        int Urgency = 100;
                        if (g.optimumTrading)
                        {
                            Urgency = (int)(e.X / (double)_view.Columns[2].Width * 100);
                        }

                        dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;
            }
        }


        private static void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            g.매매.dgv.Rows[0].DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
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

        public object GetCellValue(int row, int col)
        {
            return _table.Rows[row][col];
        }

        public bool HasRows()
        {
            return _table != null && _table.Rows.Count > 0;
        }
    }


}
