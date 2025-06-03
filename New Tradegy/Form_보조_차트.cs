using New_Tradegy.Library;
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
        private int dataGridView1Height = 22;
        public int nRow;
        public int nCol;

        public static List<string> displayList = new List<string>();
        private DataTable dtb;

        private string PreSubChartDisplayMode = "";

        public Form_보조_차트()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None; // Remove the default title bar
            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.LightGray // Customize appearance
            };
            this.Controls.Add(titleBar);

            //// Add the DataGridView to the custom title bar
            //DataGridView dgv = new DataGridView
            //{
            //    Dock = DockStyle.Fill,
            //    ColumnCount = 3
            //};
            //dgv.Columns[0].Name = "상관";
            //dgv.Columns[1].Name = "보유";
            //dgv.Columns[2].Name = "그순";

            titleBar.Controls.Add(dataGridView1); // Add DataGridView to the title bar

        }

        private void Form_보조_차트_Load(object sender, EventArgs e)
        {
            g.ChartManager.SetChart2(chart2);

            // Configure DataGridView appearance
            ConfigureDataGridView();
            ConfigureChartAndGridSize();

            // Initialize DataTable
            InitializeDataTable();

            // Draw initial charts
            Form_보조_차트_DRAW();
        }

        private void ConfigureChartAndGridSize()
        {
            if (Environment.MachineName == "HP")
            {
                this.Location = new Point(-g.screenWidth / 2, 0);
            }
            else
            {
                this.Location = new Point(g.screenWidth, 0);
                if (Screen.AllScreens.Count() == 1) this.Location = new Point(g.screenWidth / 2, 0); // one screen
            }
            this.Size = new Size(g.screenWidth / 2, g.screenHeight);
            chart2.Size = new Size(this.Width, this.Height);
            chart2.Location = new Point(0, dataGridView1Height);
            dataGridView1.Size = new Size(this.Width, dataGridView1Height);
            dataGridView1.Location = new Point(0, 0);
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

            if (g.v.SubChartDisplayMode != PreSubChartDisplayMode) // if g.v.SubChartDisplayMode changes, Clear Chart
            {
                PreSubChartDisplayMode = g.v.SubChartDisplayMode;
                g.ChartManager.Chart2Handler.Clear();
            }

            for (int i = 0; i < nRow * nCol; i++)
            {
                if (i >= displayList.Count)
                    break;
                string stock = displayList[i];

                var data = g.StockRepository.TryGetStockOrNull(stock);
                if (data == null)
                {
                    continue;
                }

                if (!ChartHandler.ChartAreaExists(g.ChartManager.Chart2, stock) || data.Misc.ShrinkDraw || g.test)
                {
                    if (g.StockManager.IndexList.Contains(data.Stock))
                    {
                        ChartIndex.UpdateChartArea(g.ChartManager.Chart1, data);
                    }
                    else
                    {
                        g.ChartGeneral2.CreateChartArea(data.Stock, i % 3, i / 3);
                    }

                }
                else
                {
                    if (g.StockManager.IndexList.Contains(data.Stock))
                    {
                        ChartIndex.UpdateSeries(g.ChartManager.Chart1, data);
                    }
                    else
                    {
                        ChartGeneralRenderer.UpdateSeries(g.ChartManager.Chart2, data);
                    }
                }

            }

            RelocateChart2AreasAndAnnotations();

            dataGridView1.Refresh();

            g.ChartManager.Chart2.Invalidate();

            int areasCount = g.ChartManager.Chart2.ChartAreas.Count;
            int annotationsCount = g.ChartManager.Chart2.Annotations.Count;
            int seriesCount = g.ChartManager.Chart2.Series.Count;

        }

        public void RelocateChart2AreasAndAnnotations()
        {
            int totalAreas = Math.Min(displayList.Count, nRow * nCol);
            float cellWidth = 100.0F / nCol; // Width percentage per column
            float cellHeight = 100.0F / nRow; // Height percentage per row
      
            for (int i = 0; i < totalAreas; i++)
            {
                string stock = displayList[i];
                string areaName = stock;

                // Calculate row and column
                int row = i % nRow;
                int col = i / nRow;

                // Calculate chart area position
                float x = col * cellWidth;
                float y = row * cellHeight;

                // Relocate the ChartArea
                if (chart2.ChartAreas.IndexOf(areaName) >= 0)
                {
                    var chartArea = chart2.ChartAreas[areaName];
                    if (areaName.Contains("KODEX"))
                    {
                        chartArea.Position = new ElementPosition(
                        x,    // X position 20
                        y,   // Y position
                        cellWidth,          // Width
                        cellHeight); //cellHeight       // Height
                    }
                    else
                    {
                        chartArea.Position = new ElementPosition(
                        x,    // X position 20
                        y + 5.155F,   // Y position
                        cellWidth,          // Width
                        cellHeight - 5.155F); //cellHeight       // Height
                    }

                    // Set InnerPlotPosition for plot area
                    chartArea.InnerPlotPosition = new ElementPosition(
                        5, // Leave a 10% margin on the left
                        10, // Leave a 10% margin at the top
                        75, // Use 80% of the ChartArea's width for the plot
                        80  // Use 80% of the ChartArea's height for the plot
                    );

                    // Adjust corresponding Annotation's position (independently)
                    Annotation annotation = chart2.Annotations.FirstOrDefault(a => a.Name == areaName);
                    if (annotation is RectangleAnnotation rectangleAnnotation)
                    {
                        rectangleAnnotation.X = chartArea.Position.X; // Match the ChartArea's X
                        rectangleAnnotation.Y = row * cellHeight; //chartArea.Position.Y; // Place annotation slightly above ChartArea
                        rectangleAnnotation.Width = chartArea.Position.Width; // Match ChartArea's width
                        rectangleAnnotation.Height = 5.155 + 2; // Annotation height (adjust as needed)
                    }
                }
            }
            chart2.Invalidate(); // Redraw the chart
        }

        public void RelocateChart2AreasAndAnnotations_old()
        {
            int totalAreas = Math.Min(displayList.Count, nRow * nCol);
            double cellWidth = 100.0 / nCol; // Width percentage per column
            double cellHeight = 100.0 / nRow; // Height percentage per row
            float annotationHeight = 5.155F;
            for (int i = 0; i < totalAreas; i++)
            {
                string stock = displayList[i];
                string areaName = stock;

                // Calculate row and column
                int row = i % nRow;
                int col = i / nRow;

                // Calculate chart area position
                double x = col * cellWidth;
                double y = row * cellHeight;


                // Relocate the ChartArea
                if (chart2.ChartAreas.IndexOf(areaName) >= 0)
                {
                    var chartArea = chart2.ChartAreas[areaName];
                    if (areaName.Contains("KODEX"))
                    {
                        chartArea.Position = new ElementPosition((float)x, (float)(y),
                        (float)cellWidth, (float)(cellHeight));
                    }
                    else
                    {
                        var annotation = chart2.Annotations.FirstOrDefault(a => a.Name == areaName);

                        if (annotation != null)
                        {
                            // Explicitly cast to TextAnnotation
                            if (annotation is TextAnnotation textAnnotation)
                            {
                                string text = textAnnotation.Text; // Now safe to access Text
                                annotationHeight = text.Split('\n').Length - 1 + 1;

                                annotation.X = x;
                                annotation.Y = y;
                                annotation.Width = 20;
                                //annotation.Height = annotationHeight;

                                chartArea.Position = new ElementPosition((float)x, (float)(y + annotationHeight),
                                        (float)cellWidth, (float)(cellHeight - annotationHeight));
                            }
                        }

                        chartArea.Position = new ElementPosition((float)x, (float)(y + annotationHeight),
                        (float)cellWidth, (float)(cellHeight - annotationHeight));
                    }

                    chartArea.InnerPlotPosition = new ElementPosition(5, 3, 55, 87);
                }
            }
            chart2.Invalidate(); // Redraw the chart
        }

        // 상관, 보유, 그순, 관심, 닥올, 피올, 절친, 푀손
        public static void DisplayListGivenDisplayMode(string MainChartDisplayMode, List<string> displayList, string clickedStock, string clickedTitle)
        {
            displayList.Clear();
            switch (MainChartDisplayMode)
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
                    var stockData = g.StockManager.Repository.TryGetStockOrNull(g.clickedStock);
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
                    displayList.Add("KODEX 레버리지");
                    displayList.Add("KODEX 코스닥150레버리지");
                    foreach (string s in g.kospi_mixed.stock) { displayList.Add(s); }
                    break;

                case "닥올":
                    displayList.Add("KODEX 레버리지");
                    displayList.Add("KODEX 코스닥150레버리지");
                    foreach (string s in g.kosdaq_mixed.stock) { displayList.Add(s); }
                    break;


                case "푀손":
                    {
                        var a_tuple = new List<Tuple<double, string>>();

                        foreach (var data in g.StockRepository.AllDatas)
                        {
                            string stock = data.Stock;

                            // Exclude index-related ETFs and already displayed/interested stocks
                            if (g.StockManager.IndexList.Contains(stock) ||
                                stock.Contains("KOSEF") ||
                                stock.Contains("HANARO") ||
                                stock.Contains("TIGER") ||
                                stock.Contains("KBSTAR") ||
                                stock.Contains("혼합") ||
                                g.StockManager.HoldingList.Contains(stock) ||
                                g.StockManager.InterestedWithBidList.Contains(stock))
                            {
                                continue;
                            }

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

        private void Form_보조_차트_ResizeEnd(object sender, EventArgs e)
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
