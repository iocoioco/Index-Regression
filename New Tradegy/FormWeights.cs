using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Drawing;
using New_Tradegy.Library;

namespace New_Tradegy
{
    public partial class FormWeights : Form
    {
        private Dictionary<string, double> _weights = new Dictionary<string, double>();
        private readonly string _path = @"C:\병신\data\Weights.txt";
        private readonly string[] _keys = new[] { "푀분", "거분", "배차", "배합", "푀누", "종누", "피로" };
        private DataGridView dgv;

        // Individual variables for external access
        public double _푀분 => _weights["푀분"];
        public double _거분 => _weights["거분"];
        public double _배차 => _weights["배차"];
        public double _배합 => _weights["배합"];
        public double _푀누 => _weights["푀누"];
        public double _종누 => _weights["종누"];
        public double _피로 => _weights["피로"];

        public FormWeights()
        {
            InitializeDataGridView();
            

           

        }

        private void InitializeDataGridView()
        {
            dgv = new DataGridView
            {
                // Dock = DockStyle.Fill,
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
            LoadWeightsFromFile();

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

            return;

            // Column 0 (i.e., first visible column)
            int colWidth = g.screenWidth / g.nCol;
            int x = colWidth;
            int y = 300;
            this.Location = new Point(x, y);
            this.Size = new Size(g.screenWidth / g.nCol - 20, 300);

            dgv.Location = new Point(0, 0);
            dgv.Size = this.Size;

            // Position this form at 1/10th width of main form (column 1)
            //int x = g.screenWidth / g.nCol + 20;
            //int y = 300;  // small vertical offset if needed

            //this.Location = new Point(x, y);
            //this.Size = new Size(g.screenWidth / g.nCol - 20, 300);

            //dgv.Location = this.Location;
            //dgv.Size = this.Size;


        }

        private void LoadWeightsFromFile()
        {
            if (!File.Exists(_path)) return;

            string[] lines = File.ReadAllLines(_path);
            foreach (string line in lines)
            {
                string[] parts = line.Trim().Split();
                if (parts.Length == 2 && double.TryParse(parts[1], out double val))
                    _weights[parts[0]] = val;
            }

            // Fill missing keys with 1.0 default
            foreach (var key in _keys)
                if (!_weights.ContainsKey(key))
                    _weights[key] = 1.0;

            dgv.Rows.Clear();
            foreach (var key in _keys)
                dgv.Rows.Add(key, _weights[key].ToString("F1"));
        }

        private void SaveWeightsToFile()
        {
            using (StreamWriter sw = new StreamWriter(_path))
            {
                foreach (var key in _keys)
                    sw.WriteLine($"{key} {_weights[key]:F1}");
            }
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string key = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
            string valueStr = dgv.Rows[e.RowIndex].Cells[1].Value.ToString();

            if (_keys.Contains(key) && double.TryParse(valueStr, out double val))
            {
                _weights[key] = val;
                SaveWeightsToFile();
            }
        }

        public double GetWeight(string key)
        {
            return _weights.TryGetValue(key, out double val) ? val : 1.0;
        }
    }

    // Access from RankLogic
    //public class RankLogic
    //{
    //    private readonly FormWeights _formWeights;

    //    public RankLogic(FormWeights weightsForm)
    //    {
    //        _formWeights = weightsForm;
    //    }

    //    public void UseWeights()
    //    {
    //        double score1 = _formWeights._푀분;
    //        double score2 = _formWeights._피로;
    //        ... use as needed
    //    }
    //}
}
