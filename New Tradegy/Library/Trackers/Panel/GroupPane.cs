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
            dgv.DataSource = _table;
            _groups = g.GroupManager.GetAll();
            InitializeDgv(dgv);
            BindGrid(dgv); // has InitializeSetting()
        }


        private void InitializeDgv(DataGridView dgv)
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

            dgv.ScrollBars = g.test ? ScrollBars.Vertical : ScrollBars.None;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;

            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Dock = DockStyle.Fill;
            dgv.DefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgv.RowTemplate.Height = g.formSize.ch;
            dgv.ForeColor = Color.Black;
            dgv.TabStop = false;

            dgv.CellFormatting += 그룹_CellFormatting;
            dgv.CellMouseClick += 그룹_CellMouseClick;
            dgv.KeyPress += 그룹_KeyPress;

            if (dgv.Rows.Count > 0)
                dgv.FirstDisplayedScrollingRowIndex = 0;
        }

        private void BindGrid(DataGridView dgv)
        {
            _table.Clear();
            _table.Columns.Clear();

            // Columns: "0", "1", "2"
            for (int i = 0; i < 3; i++)
                _table.Columns.Add(i.ToString());

            // Determine row count based on mode
            int rows = g.test ? 9 : _groups.Count;

            // Add blank rows
            for (int j = 0; j < rows; j++)
                _table.Rows.Add("", "", "");

            // Adjust column widths based on test mode
            for (int i = 0; i < 3; i++)
            {
                if (g.test)
                    dgv.Columns[i].Width = this._view.Width / 3 - 10;
                else
                    dgv.Columns[i].Width = this._view.Width / 3 - 6;
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

}
