using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.UI.KeyBindings;

namespace New_Tradegy.Library.Trackers
{
    public class GroupPanelRenderer
    {
        private DataGridView _view;
        private DataTable _table;
        private List<GroupData> _groups;

        public static void SetupAndAttachGroupPanel(Form containerForm)
        {
            var dgv = new DataGridView();
            containerForm.Controls.Add(dgv);
            Initialize(dgv);
        }

        public void BindGrid(DataGridView view, List<GroupData> groups)
        {
            _view = view;
            _groups = groups;

            _table = new DataTable();
            _table.Columns.Add("총점");
            _table.Columns.Add("푀분");
            _table.Columns.Add("가증");

            foreach (var g in groups)
            {
                _table.Rows.Add(Math.Round(g.TotalScore, 2), Math.Round(g.푀분, 2), Math.Round(g.가증, 2));
            }

            _view.DataSource = _table;
            _view.RowTemplate.Height = g.DgvCellHeight;
            _view.DefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            _view.ColumnHeadersVisible = false;
            _view.RowHeadersVisible = false;
            _view.AllowUserToAddRows = false;
            _view.AllowUserToDeleteRows = false;
            _view.AllowUserToResizeColumns = false;
            _view.AllowUserToResizeRows = false;
            _view.ScrollBars = ScrollBars.Vertical;
            _view.Dock = DockStyle.Fill;

            int colWidth = _view.Width / 3 - 6;
            for (int i = 0; i < 3; i++)
            {
                _view.Columns[i].Width = colWidth;
            }

            _view.CellMouseClick += HandleCellMouseClick;
        }

        private void HandleCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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

        public static void Initialize(DataGridView dgv)
        {
            g.그룹.GroupRenderer = new GroupPanelRenderer();
            g.그룹.GroupRenderer.BindGrid(dgv, g.GroupManager.GroupRankingList);
        }

        public void Update(List<GroupData> updatedGroups)
        {
            if (_view == null || _table == null) return;

            _view.SuspendLayout();

            try
            {
                for (int i = 0; i < updatedGroups.Count && i < _table.Rows.Count; i++)
                {
                    var group = updatedGroups[i];

                    string currentTitle = _table.Rows[i][0].ToString();
                    string current푀분 = _table.Rows[i][1].ToString();
                    string current총점 = _table.Rows[i][2].ToString();

                    string newTitle = group.Title;
                    string new푀분 = ((int)group.푀분).ToString();
                    string new총점 = ((int)group.TotalScore).ToString();

                    bool changed = currentTitle != newTitle || current푀분 != new푀분 || current총점 != new총점;

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

    }

}
