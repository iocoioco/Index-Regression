using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.UI.KeyBindings;
using New_Tradegy.Library.Core;

namespace New_Tradegy.Library.Trackers
{
    public class GroupPane
    {
        private DataGridView _view;
        private DataTable _table;
        private List<GroupData> _groups;
        public DataGridView View => _view; // for easy access

        public GroupPane(DataGridView dgv, DataTable dtb)
        {
            _table = dtb;
            _view = dgv;

            _groups = g.GroupManager.GetAll();

            InitializeDgv(_view);
            //BindGrid(_view); // has InitializeSetting()
        }


        private void InitializeDgv(DataGridView _view)
        {
            _view.ColumnHeadersVisible = false;
            _view.RowHeadersVisible = false;
            _view.ReadOnly = true;

            _view.ScrollBars = ScrollBars.Vertical;

            _view.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _view.AllowUserToResizeColumns = false;
            _view.AllowUserToResizeRows = false;
            _view.AllowUserToAddRows = false;
            _view.AllowUserToDeleteRows = false;

            _view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // _view.Dock = DockStyle.Fill;
            _view.DefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            _view.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            _view.RowTemplate.Height = g.CellHeight;
            _view.ForeColor = Color.Black;
            _view.TabStop = false;

            _view.CellFormatting += CellFormatting;
            _view.CellMouseClick += CellMouseClick;


            if (_view.Rows.Count > 0)
                _view.FirstDisplayedScrollingRowIndex = 0;
        }

        public void BindGrid(DataGridView _view)
        {
            _table.Columns.Clear();

            // Determine row count based on mode
            int rows = _groups.Count;

            // Columns: "0", "1", "2"
            for (int i = 0; i < 3; i++)
                _table.Columns.Add(i.ToString());

            // Add blank rows
            for (int j = 0; j < rows; j++)
                _table.Rows.Add("", "", "");

            _view.DataSource = _table;

            _view.AutoGenerateColumns = false;

            int visibleWidth = _view.ClientSize.Width;
            visibleWidth -= SystemInformation.VerticalScrollBarWidth;

            int columnWidth = visibleWidth / 3;

            // Prevent out-of-range errors
            int colCount = Math.Min(3, _view.Columns.Count);
            for (int i = 0; i < colCount; i++)
            {
                _view.Columns[i].Width = columnWidth;
            }
            Form target = Application.OpenForms["Form_보조_차트"];
            if (target is Form_보조_차트 form)
            {
                form.Form_보조_차트_ResizeEnd(form, EventArgs.Empty);
            }
        }


        private void CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _groups.Count) return;

            string title = _groups[e.RowIndex].Title;
            g.clickedTitle = title;

            var group = _groups[e.RowIndex];
            if (group.Stocks.Count == 0) return;

            string firstStock = group.Stocks[0];
            g.clickedStock = firstStock;

            if (e.Button == MouseButtons.Left)
            {
                g.v.SubChartDisplayMode = "상관";
                ActionCode.New(true, false, eval: true, draw: 's').Run(); // 상관 SubChart
            }
            else if (e.Button == MouseButtons.Right)
            {
                string url = $"http://google.com/search?q={title} 주식 뉴스 &tbs=qdr:d";
                Process.Start(url);
            }
        }

        private void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int coloring_column = 0;
            switch (g.oGl_data_selection)
            {
                case "등합":
                    coloring_column = 0;
                    break;
                case "푀분":
                    coloring_column = 1;
                    break;
                case "가증":
                    coloring_column = 2;
                    break;
            }
            for (int i = 0; i < 3; i++)
            {
                if (i == coloring_column)
                {
                    _view.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255); // cyan
                }
                else
                {
                    _view.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                }

            }

            if (e.RowIndex == 0)
            {
                if (e.ColumnIndex == 0)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // red
                if (e.ColumnIndex == 1)
                    e.CellStyle.BackColor = Color.FromArgb(255, 255, 255); // red
                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // red
                if (e.ColumnIndex == 3)
                    e.CellStyle.BackColor = Color.FromArgb(255, 255, 255); // red
            }
        }

        public void Update(List<GroupData> updatedGroups)
        {
            if (_view == null || _table == null || updatedGroups == null)
                return;

            _view.SuspendLayout();

            try
            {
                int rowCount = Math.Min(updatedGroups.Count, _table.Rows.Count);

                for (int i = 0; i < rowCount; i++)
                {
                    var group = updatedGroups[i];

                    _table.Rows[i][0] = group.Title;
                    _table.Rows[i][1] = group.푀분.ToString("F0");
                    _table.Rows[i][2] = group.TotalScore.ToString("F0");
                }
            }
            finally
            {
                _view.ResumeLayout();
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
