using New_Tradegy.Library;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.IO;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.UI.ChartClickHandlers;
using New_Tradegy.Library.UI.KeyBindings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace New_Tradegy
{
    public partial class Form_보조_차트 : Form
    {
        private int dataGridView1Height = 25;
        public int nRow;
        public int nCol;

        public static List<string> displayList = new List<string>();
        private DataTable dtb;

        private int _maxSpace = 15;

        private void Form_보조_차트_Shown(object sender, EventArgs e)
        {
            int dataGridView1Height = 25; // Your actual value

            dataGridView1.Dock = DockStyle.Top;
            dataGridView1.Height = dataGridView1Height;

            chart2.Dock = DockStyle.Fill;
            chart2.Margin = new Padding(0);

            this.Padding = new Padding(0);
            this.AutoScroll = false;

            // 🔥 groupPane layout after chart2 is docked and measured
            if (g.groupPane?.View != null)
            {
                var view = g.groupPane.View;

                int cellWidth = chart2.Width / 5;
                int cellHeight = chart2.Height / 3;
                int x = cellWidth * 4;
                int y = cellHeight;

                view.Location = new Point(x, y);
                view.Size = new Size(cellWidth, cellHeight);
                view.BringToFront();  // Optional, depending on layer
            }

            // 🔥 This forces WinForms to recalculate layout
            this.PerformLayout();
        }


        public Form_보조_차트()
        {
            InitializeComponent();
            this.Shown += Form_보조_차트_Shown;
        }

        private void Form_보조_차트_Load(object sender, EventArgs e)
        {
            g.ChartManager.SetChart2(chart2);

            
            // Configure DataGridView appearance
            ConfigureDataGridView();
            ConfigureChartAndGridSize();

            // Initialize DataTable
            InitializeDataTable();

            Form_보조_차트_DRAW();

            // defer the call until after Form_보조_차트_Load completes
            this.BeginInvoke((Action)(() =>
            {
                if (g.groupPane?.View != null)
                    g.groupPane.BindGrid(g.groupPane.View);
            }));
        }

        private void ConfigureChartAndGridSize()
        {
            Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
            
            this.Size = workingRectangle.Size;
            this.Width /= 2;
            this.Width += 10;
            this.Height += 10;
            this.Padding = new Padding(0);
            this.AutoScroll = false;

            this.StartPosition = FormStartPosition.Manual;
            if (Environment.MachineName == "HP")
            {
                this.Location = new Point(-workingRectangle.Width / 2, 0);
            }
            else
            {
                this.Location = new Point(-workingRectangle.Width / 2, 0);
                if (Screen.AllScreens.Count() == 1)
                    this.Location = new Point(workingRectangle.Width / 2, 0); // one screen
            }

            // groupPane setting
            var groupDgv = new DataGridView();
            var groupDtb = new DataTable();
            this.Controls.Add(groupDgv); // ✅ added to Form
            g.groupPane = new GroupPane(groupDgv, groupDtb); // logic wrapper
            var view = g.groupPane.View;
            view.BringToFront();

            //int cellWidth = chart2.Width / 5;   // width % per column
            //int cellHeight = chart2.Height / 3;  // height % per row
            //int x = cellWidth * 4;
            //int y = cellHeight;
            //view.Location = new Point(x, y);
            //view.Size = new Size(cellWidth, cellHeight);
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.DataError += (s, f) => FileOut.DataGridView_DataError(s, f, "보조차트 dgv");
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridView1.RowTemplate.Height = dataGridView1Height;
            dataGridView1.ForeColor = Color.Black;
            dataGridView1.ScrollBars = ScrollBars.None;
        }

        private void InitializeDataTable()
        {
            dtb = new DataTable();
            dtb.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("0"), new DataColumn("1"), new DataColumn("2"),
                new DataColumn("3"), new DataColumn("4"), new DataColumn("5"),
                new DataColumn("6"), new DataColumn("7")
            });

            dtb.Rows.Add("상관", "보유", "그순", "관심", "닥올", "피올", "절친", "푀손");
            dataGridView1.DataSource = dtb;

            for (int i = 0; i < 8; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[i].Width = this.Width / 8;
                dataGridView1.Height = dataGridView1Height;
            }
        }

        public void Form_보조_차트_DRAW()
        {

            DisplayListGivenDisplayMode(g.v.SubChartDisplayMode, displayList, g.clickedStock, g.clickedTitle);

            // Update form title 
            UpdateFormTitle();

            // Determine grid layout based on the number of displayList
            SetGridDimensions();

            var chartAreas = new List<ChartArea>();
            var annotations = new List<Annotation>();
            for (int i = 0; i < displayList.Count; i++)
            {
                if (i >= _maxSpace)
                    break;
                string stock = displayList[i];

                var data = g.StockRepository.TryGetDataOrNull(stock);
                if (data == null)
                {
                    continue;
                }
                // Index
                if (g.StockManager.IndexList.Contains(data.Stock))
                {
                    var area = ChartIndex.UpdateChartArea(chart2, data);
                    if (area != null)
                        chartAreas.Add(area);
                }
                // General
                else
                {
                    var (area, anno) = ChartGeneral.UpdateChartArea(chart2, data);
                    if (area != null)
                    {
                        chartAreas.Add(area);
                        annotations.Add(anno);
                    }
                }
            }

            RelocateChart2AreasAndAnnotations();

            CleanupChart2();

            dataGridView1.Refresh();

            g.ChartManager.Chart2.Invalidate();

            int areasCount = g.ChartManager.Chart2.ChartAreas.Count;
            int annotationsCount = g.ChartManager.Chart2.Annotations.Count;
            int seriesCount = g.ChartManager.Chart2.Series.Count;

        }

        public void RelocateChart2AreasAndAnnotations()
        {
            float cellWidth = 100f / nCol;   // width % per column
            float cellHeight = 100f / nRow;  // height % per row

            for (int i = 0; i < displayList.Count; i++)
            {
                if (i >= _maxSpace)
                    break;

                string stock = displayList[i];
                string areaName = stock;

                int row = i % nRow;
                int col = i / nRow;

                float x = col * cellWidth;
                float y = row * cellHeight;
                float width = cellWidth;
                float height = cellHeight;

                // Move ChartArea (always)
                if (chart2.ChartAreas.IndexOf(areaName) >= 0)
                {
                    var area = chart2.ChartAreas[areaName];
                    if (area == null)
                        continue; //??
                    area.Position = new ElementPosition(x, y, width, height);
                    area.InnerPlotPosition = new ElementPosition(5, 10, 90, 80);

                    if (!area.Visible)
                        area.Visible = true;
                }

                // Move Annotation (only for non-index stocks)
                if (!g.StockManager.IndexList.Contains(stock))
                {
                    var anno = chart2.Annotations.FirstOrDefault(a => a.Name == areaName);
                    if (anno == null)
                        continue;
                    if (anno is RectangleAnnotation rect)
                    {
                        rect.X = x;
                        rect.Y = y; // + height; // directly below chart
                        rect.Width = width;
                        rect.Height = 5.155f + 2f; // standard height
                    }
                    if (!anno.Visible)
                        anno.Visible = true;
                }
            }

            chart2.Invalidate(); // Redraw the chart
        }


        private void CleanupChart2()
        {
            var validStocks = new HashSet<string>(displayList.Take(_maxSpace));

            // Remove unnecessary chart areas (name == stock)
            var areasToRemove = g.ChartManager.Chart2.ChartAreas
                .Where(area => !validStocks.Contains(area.Name))
                .ToList();

            foreach (var area in areasToRemove)
            {
                if (area == null)
                    return; //??
                g.ChartManager.Chart2.ChartAreas.Remove(area);
            }
                

            // Remove unnecessary annotations (name == stock)
            var annotationsToRemove = g.ChartManager.Chart2.Annotations
                .Where(anno => !validStocks.Contains(anno.Name))
                .ToList();

            foreach (var anno in annotationsToRemove)
            {
                if (anno == null) 
                    return; //??
                g.ChartManager.Chart2.Annotations.Remove(anno);
            }
                

            // Remove unnecessary series (name starts with stockName but stockName not in displayList)
            var seriesToRemove = g.ChartManager.Chart2.Series
                .Where(series =>
                {
                    foreach (var stock in validStocks)
                    {
                        if (series.Name.StartsWith(stock))
                            return false;
                    }
                    return true;
                })
                .ToList();

            foreach (var s in seriesToRemove)
            {
                if (s == null)
                    return; //??
                g.ChartManager.Chart2.Series.Remove(s);
            }
                
        }



        // 상관, 보유, 그순, 관심, 닥올, 피올, 절친, 푀손
        public static void DisplayListGivenDisplayMode(string SubChartDisplayMode, List<string> displayList, string clickedStock, string clickedTitle)
        {
            displayList.Clear();
            switch (SubChartDisplayMode)
            {
                case "지수":
                    for (int i = 0; i < 2; i++)
                    {
                        if (!displayList.Contains(g.StockManager.IndexList[i]))
                        {
                            displayList.Add(g.StockManager.IndexList[i]);
                        }
                    }
                    break;
                case "보유":
                    foreach (string s in g.StockManager.HoldingList)
                    {
                        if (!displayList.Contains(s))
                        {
                            displayList.Add(s);
                        }
                    }
                    break;

                case "그순":
                    RankLogic.RankByMode();
                    if (g.GroupManager.GroupRankingList.Count > 0)
                    {
                        var topStocks = g.GroupManager.GetTopStocksFromTopGroups(existing: displayList);
                        displayList.AddRange(topStocks);
                    }
                    break;

                case "상관":
                    displayList.AddRange(g.GroupManager.GetStocksByTitle(clickedTitle, displayList));
                    break;

                case "절친":
                    var stockData = g.StockManager.Repository.TryGetDataOrNull(g.clickedStock);
                    if (stockData != null && stockData.Misc.Friends.Count > 0)
                    {
                        displayList.Add(g.clickedStock);

                        foreach (var line in stockData.Misc.Friends)
                        {
                            var words = line.Split('\t');
                            if (words.Length == 2)
                            {
                                string friendStock = words[1];

                                if (!displayList.Contains(friendStock))
                                    displayList.Add(friendStock);
                            }
                        }
                    }
                    break;


                case "관심":
                    FileIn.read_파일관심종목();
                    foreach (string s in g.StockManager.InterestedInFile)
                    {
                        if (!displayList.Contains(s))
                        {
                            displayList.Add(s);
                        }
                    }
                    break;

                case "피올":
                    displayList.AddRange(g.StockManager.LeverageList);
                    foreach (string s in g.kospi_mixed.stock) { displayList.Add(s); }
                    break;

                case "닥올":
                    displayList.AddRange(g.StockManager.LeverageList);
                    foreach (string s in g.kosdaq_mixed.stock) { displayList.Add(s); }
                    break;


                case "푀손":
                    {
                        var a_tuple = new List<Tuple<double, string>>();

                        foreach (var data in g.StockRepository.AllGeneralDatas)
                        {
                            string stock = data.Stock;

                            int check_row = 0;
                            int nrow = data.Api.nrow;

                            if (nrow < 2)
                                continue;

                            if (g.connected)
                            {
                                check_row = nrow - 1;
                            }
                            else
                            {
                                check_row = g.Npts[1] - 1;
                                if (check_row > nrow - 1)
                                    check_row = nrow - 1;
                            }

                            if (data.Api.x[check_row, 4] < 0)
                                continue;

                            double value = 0.0;
                            var x = data.Api.x;

                            for (int i = check_row - 1; i >= 1; i--)
                            {
                                double deltaPrice = x[check_row, 1] - (x[i, 1] + x[i - 1, 1]) / 2.0;
                                int deltaVolume = x[i, 4] - x[i - 1, 4];
                                value += deltaPrice * deltaVolume;
                            }

                            a_tuple.Add(Tuple.Create(value, stock));
                        }

                        a_tuple = a_tuple.OrderBy(t => t.Item1).ToList();

                        foreach (var t in a_tuple)
                        {
                            if (!displayList.Contains(t.Item2))
                                displayList.Add(t.Item2);
                        }
                        break;
                    }

            }
        }

        private void UpdateFormTitle()
        {
            switch (g.v.SubChartDisplayMode)
            {
                case "상관":
                    this.Text = $"{g.v.SubChartDisplayMode} ({g.clickedTitle})";
                    break;
                case "절친":
                    this.Text = $"{g.v.SubChartDisplayMode} ({g.clickedStock})";
                    break;
                case "그순":
                    this.Text = $"{g.v.SubChartDisplayMode} ({g.oGl_data_selection})";
                    break;
                default:
                    this.Text = g.v.SubChartDisplayMode;
                    break;
            }
        }

        private void SetGridDimensions()
        {
            int count = displayList.Count;
            if (count <= 2) { nCol = 2; nRow = 1; }
            else if (count <= 4) { nCol = 2; nRow = 2; }
            else if (count <= 6) { nCol = 3; nRow = 2; }
            else if (count <= 9) { nCol = 3; nRow = 3; }
            else if (count <= 12) { nCol = 4; nRow = 3; }
            else { nCol = 5; nRow = 3; }
        }

        private void chart2_MouseClick(object sender, MouseEventArgs e)
        {
            string selection = "";


            int row_id = 0, col_id = 0;

            //chart2_info(e, ref selection, ref xval, ref yval, ref row_percentage,
            //    ref col_percentage, ref row_id, ref col_id, ref col_divider);
            g.clickedStock = ChartClickMapper.CoordinateMapping(chart2, nRow, nCol, displayList, e, ref selection, ref col_id, ref row_id);


            if (Control.ModifierKeys == Keys.Control)
            {
                ChartClickHandler.HandleControlClick(chart2, selection, row_id, col_id);
            }
            else
            {
                ChartClickHandler.HandleClick(chart2, selection, row_id, col_id);
            }
        }

        public void Form_보조_차트_ResizeEnd(object sender, EventArgs e)
        {
            chart2.Size = new Size((int)(this.Width), (int)(this.Height - dataGridView1Height));
            chart2.Location = new Point(0, dataGridView1Height);

            dataGridView1.Size = new Size(this.Width, dataGridView1Height);
            dataGridView1.Location = new Point(0, 0);
            dtb.Rows[0][0] = "상관";
            dtb.Rows[0][1] = "보유";
            dtb.Rows[0][2] = "그순";
            dtb.Rows[0][3] = "관심";
            dtb.Rows[0][4] = "닥올";
            dtb.Rows[0][5] = "피올";
            dtb.Rows[0][6] = "절친";
            dtb.Rows[0][7] = "푀손";

            dataGridView1.DataSource = dtb;
            for (int i = 0; i < 8; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[i].Width = this.Width / 8;
            }

            // group pane location and size change w.r.t Form_보조_차트 size change
            //int col = 4; // Zero-based → 5th column
            //int row = 1; // Zero-based → 2nd row

            //int totalCols = 5;
            //int totalRows = 3;

            //int cellWidth = this.ClientSize.Width / totalCols;
            //int cellHeight = this.ClientSize.Height / totalRows - 6;

            //g.groupPane.View.Location = new Point(col * cellWidth, row * cellHeight);
            //g.groupPane.View.Size = new Size(cellWidth, cellHeight); // Optional: full cell size

            //int padding = 7;

            //g.groupPane.View.Location = new Point(
            //    col * cellWidth + 20,
            //    row * cellHeight + 38
            //);
            //g.groupPane.View.Size = new Size(
            //    cellWidth - 2 * padding,
            //    cellHeight - 2 * padding
            //);

            int col = 4; // Zero-based → 5th column
            int row = 1; // Zero-based → 2nd row

            int totalCols = 5;
            int totalRows = 3;

            int cellWidth = this.ClientSize.Width / totalCols;
            int cellHeight = this.ClientSize.Height / totalRows - 6;

            g.groupPane.View.Location = new Point(col * cellWidth + 20, row * cellHeight + 38);
            g.groupPane.View.Size = new Size(cellWidth - 15, cellHeight - 14);
           


            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;

            int totalWidth = g.groupPane.View.Width - scrollBarWidth;
            g.groupPane.View.Columns[0].Width = totalWidth / 2;
            g.groupPane.View.Columns[1].Width = totalWidth / 4;
            g.groupPane.View.Columns[2].Width = totalWidth / 4;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    g.v.SubChartDisplayMode = "상관";
                    return;
                case 1:
                    g.v.SubChartDisplayMode = "보유";
                    break;
                case 2:
                    g.v.SubChartDisplayMode = "그순";
                    break;
                case 3:
                    g.v.SubChartDisplayMode = "관심";
                    break;
                case 4:
                    g.v.SubChartDisplayMode = "닥올";
                    break;
                case 5:
                    g.v.SubChartDisplayMode = "피올";
                    break;
                case 6:
                    g.v.SubChartDisplayMode = "절친";
                    break;
                case 7:
                    g.v.SubChartDisplayMode = "푀손";
                    break;
            }
            Form_보조_차트_DRAW();
        }

        // no use
        private void UpdateAnnotation(string stock, StockPoint stockData)
        {
            var annotationName = $"Annotation_{stock}";
            var existingAnnotation = g.ChartManager.Chart2.Annotations
                .FirstOrDefault(a => a.Name == annotationName);

            if (existingAnnotation != null)
            {
                existingAnnotation.Name = $"{stockData.Price}";
                existingAnnotation.X = stockData.Time;
                existingAnnotation.Y = stockData.Price;
            }
            else
            {
                var newAnnotation = new TextAnnotation
                {
                    Name = annotationName,
                    Text = $"{stockData.Price}",
                    X = stockData.Time,
                    Y = stockData.Price,
                    ClipToChartArea = stock,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.Black
                };
                g.ChartManager.Chart2.Annotations.Add(newAnnotation);
            }
        }

        // it runs automatically, when keys in
        // msg: Windows message (low-level OS event info).
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // KeyBindingManager.TryHandle encapsulates logics for what to do with certain keys.
            if (KeyBindingManager.TryHandle(keyData))
                return true;

            // custom logic doesn't handle
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }

    // no use ... used at UpdateAnnotation but not referenced
    public class StockPoint
    {
        public string Stock { get; set; } // Stock name
        public double Time { get; set; }  // XValue (e.g., timestamp)
        public double Price { get; set; } // YValue
    }
}
