using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;
using System.Linq;

namespace New_Tradegy.Library.Trackers
{
    public class TradePanelRenderer
    {
        private readonly DataGridView _view;
        private readonly DataTable _table;

        public TradePanelRenderer(DataGridView view, DataTable table)
        {
            _view = view;
            _table = table;

            _view.CellFormatting += Dgv_CellFormatting;
            _view.CellMouseClick += Dgv_CellMouseClick;
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

        // call TradePanelRenderer.Initialize(this); in the MainForm
        public static void SetupAndAttachTradePanel(Control container)
        {
            var dgv = new DataGridView
            {
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
                Dock = DockStyle.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    ForeColor = Color.Black
                }
            };

            dgv.RowTemplate.Height = g.DgvCellHeight - 1;
            dgv.Size = new Size(g.screenWidth / g.nCol - SystemInformation.VerticalScrollBarWidth - 3, g.DgvCellHeight * 10 - 7);
            dgv.Location = new Point(g.screenWidth / g.rqwey_nCol + 10, g.screenHeight / 3 + g.DgvCellHeight * 3 + 3);

            var dtb = new DataTable();
            dtb.Columns.Add("0");
            dtb.Columns.Add("1");
            dtb.Columns.Add("2");
            dtb.Columns.Add("3");
            for (int i = 0; i < 11; i++)
                dtb.Rows.Add("", "", "", "");

            dgv.DataSource = dtb;

            container.Controls.Add(dgv);
            dgv.Visible = true;

            g.매매.dgv = dgv;
            g.매매.dtb = dtb;

            g.매매.Renderer = new TradePanelRenderer(dgv, dtb);
    

           

            dgv.Columns[0].Width = (int)(dgv.Width * 0.20);
            dgv.Columns[1].Width = (int)(dgv.Width * 0.20);
            dgv.Columns[2].Width = (int)(dgv.Width * 0.25);
            dgv.Columns[3].Width = (int)(dgv.Width * 0.35);
        }

        private static void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;

            string stock = g.매매.dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
            if (e.ColumnIndex == 1 && g.connected && e.RowIndex < OrderItemTracker.OrderMap.Count)
            {
                DealManager.DealCancelRowIndex(e.RowIndex);
                Utils.SoundUtils.Sound("Keys", "cancel");
            }
        }

        private static void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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
    }
}
