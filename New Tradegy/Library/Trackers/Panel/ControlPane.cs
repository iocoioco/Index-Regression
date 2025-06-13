using New_Tradegy.Library.Deals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace New_Tradegy.Library.Trackers
{
    public class ControlPane
    {
        // to assign value to a cell
        // var panel = new ControlPane();
        // DataTable table = manager._table;
        // _table.Rows[rowIndex][columnIndex] = value;
        private class ControlSetting
        {
            public string Name { get; set; }
            public int[] Values { get; set; }
            public Func<int> Get { get; set; }
            public Action<int> Set { get; set; }
        }

        private readonly DataTable _table;
        private readonly DataGridView _view;

        private readonly Dictionary<string, ControlSetting> Settings = new Dictionary<string, ControlSetting>();

        public ControlPane(DataGridView dgv, DataTable dtb)
        {
            _table = dtb;
            _view = dgv;
            dgv.DataSource = _table;
            InitializeDgv(dgv);
            BindGrid(dgv); // has InitializeSetting()
        }

        private void InitializeDgv(DataGridView dgv)
        {
            int width = g.screenWidth / g.nCol - 20;
            int height = g.CellHeight * 3;
            int x = g.screenWidth / g.nCol + 10;
            int y = g.screenHeight / 3 + 2;

            dgv.Location = new Point(x, y);
            dgv.Size = new Size(width, height);

            dgv.ColumnHeadersVisible = false;
            dgv.RowHeadersVisible = false;
            dgv.ReadOnly = true;
            dgv.ScrollBars = ScrollBars.Vertical;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Dock = DockStyle.Fill;
            dgv.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
            dgv.RowTemplate.Height = g.CellHeight;
            dgv.ForeColor = Color.Black;
            dgv.TabStop = false;

            // Optional: You may also want to scroll to top
            if (dgv.Rows.Count > 0)
                dgv.FirstDisplayedScrollingRowIndex = 0;
        }

        public void BindGrid(DataGridView dgv)
        {
            if (Settings.Count == 0)
                InitializeSettings();


            int Rows = 15, Columns = 4;

            for (int i = 0; i < Columns; i++)
                _table.Columns.Add(i.ToString());

            for (int i = 0; i < Rows; i++)
                _table.Rows.Add("", "", "", "");

            // Initialize key rows
            int month = g.date % 10000 / 100;
            int day = g.date % 100;
            g.일회거래액 = 0;
            _table.Rows[0][0] = $"{month}/{day}";
            _table.Rows[0][2] = g.일회거래액;
            _table.Rows[0][3] = g.예치금;

            _table.Rows[1][0] = 0;
            _table.Rows[1][1] = 0;
            _table.Rows[1][2] = 0;
            _table.Rows[1][3] = 0;

            // Row 2–13 configuration
            _table.Rows[4][0] = "adv";
            _table.Rows[4][1] = g.v.q_advance_lines;

            _table.Rows[5][0] = "종거"; _table.Rows[5][1] = 100; g.v.종가기준추정거래액이상_천만원 = 100;
            _table.Rows[5][2] = "분거"; _table.Rows[5][3] = 10; g.v.분당거래액이상_천만원 = 10;

            _table.Rows[6][0] = "호가"; _table.Rows[6][1] = 10; g.v.호가거래액_백만원 = 10;
            _table.Rows[6][2] = "편차"; _table.Rows[6][3] = 1; g.v.편차이상 = 1;

            _table.Rows[7][0] = "배차"; _table.Rows[7][1] = 0; g.v.배차이상 = 0;
            _table.Rows[7][2] = "시총"; _table.Rows[7][3] = 0; g.v.시총이상 = 0;

            _table.Rows[8][0] = "수과"; _table.Rows[8][1] = 20; g.v.수급과장배수 = 20;
            _table.Rows[8][2] = "배과"; _table.Rows[8][3] = 1; g.v.배수과장배수 = 1;

            _table.Rows[9][0] = "평가"; _table.Rows[9][1] = 20; g.MarketeyeCountDivicer = 20;
            _table.Rows[9][2] = "초간"; _table.Rows[9][3] = 30; g.postInterval = 30;

            _table.Rows[10][0] = "Main"; _table.Rows[10][1] = g.v.font = 16; g.v.font /= 2.0f;

            _table.Rows[12][0] = "푀플"; _table.Rows[12][1] = "1"; g.v.푀플 = 1;
            _table.Rows[12][2] = "배플"; _table.Rows[12][3] = "1"; g.v.배플 = 1;

            _table.Rows[13][0] = "선폭"; _table.Rows[13][1] = 2; g.LineWidth = 2;

            // Bind to DataGridView
            dgv.DataSource = _table;

            // Appearance
            dgv.ReadOnly = true;
            dgv.ColumnHeadersVisible = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ScrollBars = ScrollBars.Vertical;

            dgv.CellMouseClick += CellMouseClick;

            // Ensure correct column count (if bound from existing table)
            if (dgv.Columns.Count < Columns)
            {
                for (int i = dgv.Columns.Count; i < Columns; i++)
                    dgv.Columns.Add(i.ToString(), i.ToString());
            }

            // Set column widths
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
            for (int i = 0; i < Columns; i++)
                dgv.Columns[i].Width = (g.screenWidth / 11 - scrollbarWidth) / Columns;
        }

        public void InitializeSettings()
        {
            Settings["adv"] = new ControlSetting
            {
                Name = "adv",
                Values = new[] { 0, 50, 100, 200, 500, 1000, 1500, 2000 },
                Get = () => g.v.q_advance_lines,
                Set = val =>
                {
                    g.v.종가기준추정거래액이상_천만원 = val * 10;
                    _table.Rows[5][1] = val;
                }
            };

            Settings["종거"] = new ControlSetting
            {
                Name = "종거",
                Values = new[] { 0, 50, 100, 200, 500, 1000, 1500, 2000 },
                Get = () => (int)(g.v.종가기준추정거래액이상_천만원 / 10),
                Set = val =>
                {
                    g.v.종가기준추정거래액이상_천만원 = val * 10;
                    _table.Rows[5][1] = val;
                }
            };

            Settings["분거"] = new ControlSetting
            {
                Name = "분거",
                Values = new[] { 0, 2, 5, 10, 20, 30, 50 },
                Get = () => g.v.분당거래액이상_천만원,
                Set = val =>
                {
                    g.v.분당거래액이상_천만원 = val;
                    _table.Rows[5][3] = val;
                }
            };

            Settings["호가"] = new ControlSetting
            {
                Name = "호가",
                Values = new[] { 0, 5, 10, 20, 30, 50 },
                Get = () => g.v.호가거래액_백만원,
                Set = val =>
                {
                    g.v.호가거래액_백만원 = val;
                    _table.Rows[6][1] = val;
                }
            };

            Settings["편차"] = new ControlSetting
            {
                Name = "편차",
                Values = new[] { 0, 1, 2, 3, 5, 7 },
                Get = () => g.v.편차이상,
                Set = val =>
                {
                    g.v.편차이상 = val;
                    _table.Rows[6][3] = val;
                }
            };

            Settings["배차"] = new ControlSetting
            {
                Name = "배차",
                Values = new[] { 0, 10, 25, 50, 75, 100 },
                Get = () => g.v.배차이상,
                Set = val =>
                {
                    g.v.배차이상 = val;
                    _table.Rows[7][1] = val;
                }
            };

            Settings["시총"] = new ControlSetting
            {
                Name = "시총",
                Values = new[] { 0, 10, 30, 50, 100, 200 },
                Get = () => g.v.시총이상,
                Set = val =>
                {
                    g.v.시총이상 = val;
                    _table.Rows[7][3] = val;
                }
            };

            Settings["수과"] = new ControlSetting
            {
                Name = "수과",
                Values = new[] { 5, 10, 15, 20, 25, 30, 40, 60, 100 },
                Get = () => g.v.수급과장배수,
                Set = val =>
                {
                    g.v.수급과장배수 = val;
                    _table.Rows[8][1] = val;
                }
            };

            Settings["배과"] = new ControlSetting
            {
                Name = "배과",
                Values = new[] { 1, 2, 3, 4, 5, 7, 10 },
                Get = () => (int)(1.0 / g.v.배수과장배수),
                Set = val =>
                {
                    g.v.배수과장배수 = val;
                    _table.Rows[8][3] = val;
                }
            };

            Settings["평가"] = new ControlSetting
            {
                Name = "평가",
                Values = new[] { 2, 5, 7, 10, 15, 20, 25, 40, 60 },
                Get = () => g.MarketeyeCountDivicer,
                Set = val =>
                {
                    g.MarketeyeCountDivicer = val;
                    _table.Rows[9][1] = val;
                }
            };

            Settings["초간"] = new ControlSetting
            {
                Name = "초간",
                Values = new[] { 10, 15, 20, 25, 30, 40, 50, 60, 70 },
                Get = () => g.MarketeyeCountDivicer,
                Set = val =>
                {
                    g.MarketeyeCountDivicer = val;
                    _table.Rows[9][3] = val;
                }
            };

            Settings["Main"] = new ControlSetting
            {
                Name = "Main",
                Values = new[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 },
                Get = () => (int)(g.v.font * 2),
                Set = val =>
                {
                    g.v.font = val / 2f;
                    _table.Rows[10][1] = val;
                }
            };

            Settings["푀플"] = new ControlSetting
            {
                Name = "푀플",
                Values = new[] { 0, 1 },
                Get = () => g.v.푀플,
                Set = val =>
                {
                    g.v.푀플 = val;
                    _table.Rows[12][1] = val.ToString();
                }
            };

            Settings["배플"] = new ControlSetting
            {
                Name = "배플",
                Values = new[] { 0, 1 },
                Get = () => g.v.배플,
                Set = val =>
                {
                    g.v.배플 = val;
                    _table.Rows[12][3] = val.ToString();
                }
            };

            Settings["선폭"] = new ControlSetting
            {
                Name = "선폭",
                Values = new[] { 1, 2, 3 },
                Get = () => g.LineWidth,
                Set = val =>
                {
                    g.LineWidth = val;
                    _table.Rows[13][1] = val;
                }
            };
        }

        private int GetNextValue(int[] array, int current, bool increase)
        {
            int index = Array.IndexOf(array, current);
            if (index == -1) return current;

            if (increase && index < array.Length - 1)
                return array[index + 1];
            if (!increase && index > 0)
                return array[index - 1];

            return current;
        }

        private void CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || (e.ColumnIndex > 3)) return;


            if (e.RowIndex < 5)
            {
                switch (e.RowIndex)
                {
                    case 0:
                        switch (e.ColumnIndex)
                        {
                            case 3:
                                DealManager.DealDeposit();
                                break;
                        }
                        break;

                    case 1:
                        switch (e.ColumnIndex)
                        {
                            case 0:
                                DealManager.DealProfit();
                                break;
                            case 1:
                                Process.Start("chrome.exe", "https://kr.investing.com/currencies/usd-krw");
                                break;
                            case 2:
                                Process.Start("https://www.investing.com/indices/nq-100-futures?cid=1175151");
                                break;
                            case 3:
                                Process.Start("chrome.exe", "https://finviz.com/map.ashx?t=sec");
                                break;
                        }
                        break;

                    case 2:
                        Process.Start("chrome.exe", "https://www.investing.com/indices/major-indices");
                        break;

                    case 3:
                        switch (e.ColumnIndex)
                        {
                            case 0:
                            case 1:
                                Process.Start("chrome.exe", "https://www.investing.com/crypto/bitcoin/chart");
                                break;
                        }
                        break;

                    case 4:
                        if (e.ColumnIndex == 0)
                        {
                            g.v.q_advance_lines -= 5;
                        }
                        if (e.ColumnIndex == 1)
                        {
                            g.v.q_advance_lines += 5;
                        }
                        _table.Rows[4][1] = g.v.q_advance_lines;
                        break;
                }
            }
            else
            {
                int selecteddColumnIndex = 0;
                if (e.ColumnIndex > 1)
                {
                    selecteddColumnIndex = 1;
                }
                string key = _view.Rows[e.RowIndex].Cells[selecteddColumnIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(key) || !Settings.ContainsKey(key)) return;


                var setting = Settings[key];
                int current = setting.Get();
                bool increase = (e.ColumnIndex == 1);
                int next = GetNextValue(setting.Values, current, increase);

                if (next != current)
                {
                    setting.Set(next);


                    _table.Rows[e.RowIndex][1] = next;
                    _view.Refresh();

                    //g.ChartManager.ClearAll();
                    //ev.eval_stock();
                    //mm.ManageChart1();
                    //mm.ManageChart2();
                }
            }

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
