using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace New_Tradegy.Library.Trackers
{
    public class ControlPanelRenderer
    {
        private class ControlSetting
        {
            public string Name { get; set; }
            public int[] Values { get; set; }
            public Func<int> Get { get; set; }
            public Action<int> Set { get; set; }
        }
        

        public DataTable _table { get; private set; } = new DataTable();
        public DataGridView _view { get; private set; }

        private readonly Dictionary<string, ControlSetting> Settings = new Dictionary<string, ControlSetting>();

        // usage
        // ControlPanelRenderer.SetupAndAttachControlPanel(this);
        public static void SetupAndAttachControlPanel(Form containerForm)
        {
            DataGridView dgv = new DataGridView();
            containerForm.Controls.Add(dgv);
            Initialize(dgv);
            ControlPanelRenderer renderer = new ControlPanelRenderer();
            renderer.BindGrid(dgv);
        }

        public void InitializeSettings()
        {
            Settings["종거"] = new ControlSetting
            {
                Name = "종거",
                Values = new[] { 0, 50, 100, 200, 500, 1000, 1500, 2000 },
                Get = () => (int)g.v.종가기준추정거래액이상_천만원 / 10,
                Set = val => g.v.종가기준추정거래액이상_천만원 = val * 10
            };

            Settings["분거"] = new ControlSetting
            {
                Name = "분거",
                Values = new[] { 0, 2, 5, 10, 20, 30, 50 },
                Get = () => (int)g.v.분당거래액이상_천만원,
                Set = val => g.v.분당거래액이상_천만원 = val
            };

            Settings["Font"] = new ControlSetting
            {
                Name = "Font",
                Values = new[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 },
                Get = () => (int)(g.v.font * 2.0),
                Set = val => g.v.font = val / 2.0f
            };

            // Add more settings here as needed...
        }

        public void BindGrid(DataGridView dgv)
        {
            if (Settings.Count == 0)
                InitializeSettings();

            _view = dgv;
            _table = new DataTable();

            _table.Columns.Add("Label");
            _table.Columns.Add("Value");

            foreach (var setting in Settings.Values)
            {
                var row = _table.NewRow();
                row[0] = setting.Name;
                row[1] = setting.Get();
                _table.Rows.Add(row);
            }

            dgv.DataSource = _table;
            dgv.ReadOnly = true;
            dgv.ColumnHeadersVisible = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;

            dgv.CellMouseClick += Dgv_CellMouseClick;
        }

        private void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || (e.ColumnIndex != 0 && e.ColumnIndex != 1)) return;

            string key = _view.Rows[e.RowIndex].Cells[0].Value?.ToString();
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


        public static void Initialize(DataGridView dgv)
        {
            int width = g.screenWidth / g.nCol - 20;
            int height = g.DgvCellHeight * 3;
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
            dgv.Dock = DockStyle.None;
            dgv.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
            dgv.RowTemplate.Height = g.DgvCellHeight;
            dgv.ForeColor = Color.Black;
            dgv.TabStop = false;

            // Optional: You may also want to scroll to top
            if (dgv.Rows.Count > 0)
                dgv.FirstDisplayedScrollingRowIndex = 0;
        }

    }
}
