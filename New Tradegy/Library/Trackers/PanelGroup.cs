using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Trackers
{
    public static class PanelGroupSetup
    {
        public static void SetupAndAttachGroupPanel(Form containerForm)
        {
            var dgv = new DataGridView();
            containerForm.Controls.Add(dgv);
            GroupPanelInitializer.Initialize(dgv);
        }
    }

    public class PanelGroup
    {
        private DataGridView _view;
        private DataTable _table;
        private List<GroupData> _groups;

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
                mm.ManageChart2("상관");
            }
            else if (e.Button == MouseButtons.Right)
            {
                string url = $"http://google.com/search?q={title} 주식 뉴스 &tbs=qdr:d";
                Process.Start(url);
            }
        }
    }

    public static class GroupPanelInitializer
    {
        public static void Initialize(DataGridView dgv)
        {
            var manager = new GroupPanelManager();
            manager.BindGrid(dgv, g.GroupManager.RankingList);
        }
    }
}
