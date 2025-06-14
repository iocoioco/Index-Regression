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

        public GroupPane(DataGridView dgv, DataTable dtb)
        {
            _table = dtb;
            _view = dgv;
  
            _groups = g.GroupManager.GetAll();

            InitializeDgv(_view);
            BindGrid(_view); // has InitializeSetting()
        }


        private void InitializeDgv(DataGridView _view)
        {
            
            int x = g.screenWidth / g.nCol + 10;
            int y = g.screenHeight / 3 + 2;
            _view.Location = new Point(x, y);
            int width = g.screenWidth / g.nCol +10;
            int height = g.CellHeight * 3;
            _view.Size = new Size(width, height);

            _view.ColumnHeadersVisible = false;
            _view.RowHeadersVisible = false;
            _view.ReadOnly = true;

            _view.ScrollBars = g.test ? ScrollBars.Vertical : ScrollBars.None;
            _view.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _view.AllowUserToResizeColumns = false;
            _view.AllowUserToResizeRows = false;
            _view.AllowUserToAddRows = false;
            _view.AllowUserToDeleteRows = false;

            _view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _view.Dock = DockStyle.Fill;
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

        private void BindGrid(DataGridView _view)
        {
            _table.Columns.Clear();

            // Columns: "0", "1", "2"
            for (int i = 0; i < 3; i++)
                _table.Columns.Add(i.ToString());

            // Determine row count based on mode
            int rows = g.test ? 9 : _groups.Count;

            // Add blank rows
            for (int j = 0; j < rows; j++)
                _table.Rows.Add("", "", "");

            _view.DataSource = _table;

            // Adjust column widths based on test mode
            for (int i = 0; i < 3; i++)
            {
                if (g.test)
                    _view.Columns[i].Width = this._view.Width / 3 - 10;
                else
                    _view.Columns[i].Width = this._view.Width / 3 - 6;
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
                ActionCode.New(true, false, eval: true, draw: 'B').Run(); // 상관 SubChart
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
                case "총점":
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
                    g.groupPane._view.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255); // cyan
                }
                else
                {
                    g.groupPane._view.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
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

                    // Current values in DataTable
                    string currentTitle = _table.Rows[i][0].ToString();
                    string current푀분 = _table.Rows[i][1].ToString();
                    string current총점 = _table.Rows[i][2].ToString();

                    // New values from GroupData
                    string newTitle = group.Title;
                    string new푀분 = group.푀분.ToString();
                    string new총점 = group.TotalScore.ToString();

                    // Only update if changed
                    bool changed =
                        currentTitle != newTitle ||
                        current푀분 != new푀분 ||
                        current총점 != new총점;

                    if (changed)
                    {
                        _table.Rows[i][0] = newTitle;
                        _table.Rows[i][1] = new푀분;
                        _table.Rows[i][2] = new총점;
                    }
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
