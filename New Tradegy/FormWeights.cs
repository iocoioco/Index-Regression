using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using New_Tradegy.Library;

namespace New_Tradegy
{
    public partial class FormWeights : Form
    {
        private readonly string[] _keys = new[] { "푀분", "거분", "배차", "배합", "푀누", "종누", "피로" };
        private DataGridView dgv;

        // Access via WeightManager directly
        public double _푀분 => WeightManager.Get("푀분");
        public double _거분 => WeightManager.Get("거분");
        public double _배차 => WeightManager.Get("배차");
        public double _배합 => WeightManager.Get("배합");
        public double _푀누 => WeightManager.Get("푀누");
        public double _종누 => WeightManager.Get("종누");
        public double _피로 => WeightManager.Get("피로");

        public FormWeights()
        {
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dgv = new DataGridView
            {
                ColumnCount = 2,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.None
            };

            dgv.Columns[0].Name = "항목";
            dgv.Columns[1].Name = "가중치";
            dgv.CellEndEdit += Dgv_CellEndEdit;

            Controls.Add(dgv);

            dgv.Rows.Clear();
            foreach (var key in _keys)
            {
                double val = WeightManager.Get(key);
                dgv.Rows.Add(key, val.ToString("F1"));
            }

            dgv.AutoSize = true;
            this.ClientSize = dgv.PreferredSize;

            int totalHeight = dgv.ClientSize.Height - dgv.ColumnHeadersHeight;
            int rowCount = dgv.Rows.Count;

            if (rowCount > 0 && totalHeight > 0)
            {
                int rowHeight = totalHeight / rowCount;
                foreach (DataGridViewRow row in dgv.Rows)
                    row.Height = rowHeight;
            }
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string key = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
            string valueStr = dgv.Rows[e.RowIndex].Cells[1].Value.ToString();

            if (_keys.Contains(key) && double.TryParse(valueStr, out double val))
            {
                WeightManager.Set(key, val);
                WeightManager.Save();
            }
        }

        public double GetWeight(string key)
        {
            return WeightManager.Get(key);
        }
    }
}
