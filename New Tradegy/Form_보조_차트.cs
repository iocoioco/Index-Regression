using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g;

namespace New_Tradegy
{
    public partial class Form_보조_차트 : Form
    {
        private int dataGridView1Height = 22;
        public int nRow;
        public int nCol;

        public string keyString = "그순";
        public static List<string> displayList = new List<string>();
        private DataTable dtb;

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
            g.chart2 = chart2;

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
            if (g.MachineName == "HP")
            {
                this.Location = new Point(-g.screenWidth / 2, 0);
            }
            else
            {
                this.Location = new Point(g.screenWidth, 0);
            }
            this.Size = new Size(g.screenWidth / 2, g.screenHeight);
            chart2.Size = new Size(this.Width, this.Height);
            chart2.Location = new Point(0, dataGridView1Height);
            dataGridView1.Size = new Size(this.Width, dataGridView1Height);
            dataGridView1.Location = new Point(0, 0);
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.DataError += (s, f) => wr.DataGridView_DataError(s, f, "보조차트 dgv");
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

            dtb.Rows.Add("상관", "보유", "그순", "관심", "코닥", "코피", "절친", "푀손");
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
            displayList.Clear();
            보조_차트_StocksGivenKeyString(keyString, displayList, g.clickedStock, g.clickedTitle);

            // Update form title
            UpdateFormTitle();

            // Determine grid layout
            SetGridDimensions();

            for (int i = 0; i < nRow * nCol; i++)
            {
                if (i >= displayList.Count)
                    break;
                string stock = displayList[i];
                if (!mm.ChartAreaExists(g.chart2, stock) || mm.isTotalPointsEqualSeriesPoints(g.chart2, stock))
                {
                    mm.ClearChartAreaAndAnnotations(g.chart2, stock);
                    mm.CreateChartAreaForStock(g.chart2, stock, g.nRow, g.nCol);
                }
                else if(mm.isTotalPointsEqualSeriesPoints(g.chart2, stock))
                {
                    mm.UpdateChartSeries(g.chart2, stock, g.nRow, g.nCol); // includes annotation update too
                }
                else
                {
                    continue;
                }
            }

            int areasCount = g.chart2.ChartAreas.Count;
            int annotationsCount = g.chart2.Annotations.Count;
            int seriesCount = g.chart2.Series.Count;

            RelocateChart2AreasAndAnnotations();
            mm.ClearUnusedChartAreasAndAnnotations(chart2, displayList);

            areasCount = g.chart2.ChartAreas.Count;
            areasCount = g.chart2.Annotations.Count;
            areasCount = g.chart2.Series.Count;

            dataGridView1.Refresh();

            g.chart2.Invalidate();
        }

        public void RelocateChart2AreasAndAnnotations()
        {
            int totalAreas = Math.Min(displayList.Count, nRow * nCol);
            float cellWidth = 100.0F / nCol; // Width percentage per column
            float cellHeight = 100.0F / nRow; // Height percentage per row
            float annotationHeight = 5.155F;
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
                        28.175F); //cellHeight       // Height
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

        // 상관, 보유, 그순, 관심, 코닥, 코피, 절친, 푀손
        public static void 보조_차트_StocksGivenKeyString(string keyString, List<string> displayList, string clickedStock, string clickedTitle)
        {
            switch (keyString)
            {
                case "지수":
                    for (int i = 0; i < 2; i++)
                    {
                        if (!displayList.Contains(g.KODEX4[i]))
                        {
                            displayList.Add(g.KODEX4[i]);
                        }
                    }
                    break;
                case "보유":
                    foreach (string s in g.보유종목)
                    {
                        if (!displayList.Contains(s))
                        {
                            displayList.Add(s);
                        }
                    }
                    break;

                case "그순":
                    if (g.oGL_data.Count > 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int added_count = 0;
                            foreach (string s in g.oGL_data[i].stocks)
                            {
                                if (!displayList.Contains(s))
                                {
                                    displayList.Add(s);
                                }
                                else
                                {
                                    displayList.Add("");
                                }
                                if (++added_count == 3)
                                {
                                    break;
                                }
                            }
                            for (int j = added_count; j < 3; j++)
                            {
                                displayList.Add("");
                            }
                        }
                    }
                    break;

                case "상관":
                    int indexOfoGLdata = -1;
                    for (int i = 0; i < g.oGL_data.Count; i++)
                    {
                        if (g.oGL_data[i].title == clickedTitle)
                        {
                            indexOfoGLdata = i;
                            break;
                        }
                    }
                    if (indexOfoGLdata >= 0)
                    {
                        foreach (string s in g.oGL_data[indexOfoGLdata].stocks)
                        {
                            if (!displayList.Contains(s))
                                displayList.Add(s);
                        }
                    }
                    break;

                case "절친":
                    int index절친 = wk.return_index_of_ogldata(g.clickedStock);
                    if (index절친 < 0) // KODEX 제외
                    {
                    }
                    else
                    {
                        if (g.ogl_data[index절친].절친.Count > 0)
                        {
                            displayList.Add(g.clickedStock);

                            string stock = "";
                            foreach (var line in g.ogl_data[index절친].절친)
                            {
                                string[] words = line.Split('\t');
                                if (words.Length == 2)
                                    stock = words[1]; // words[0] 절친정도 수치 : 0.5 이상 양호

                                if (!displayList.Contains(stock))
                                    displayList.Add(stock);

                            }
                        }
                    }
                    break;

                case "관심":
                    rd.read_파일관심종목();
                    foreach (string s in g.파일관심종목)
                    {
                        if (!displayList.Contains(s))
                        {
                            displayList.Add(s);
                        }
                    }
                    break;

                case "코피":
                    displayList.Add("KODEX 레버리지");
                    displayList.Add("KODEX 코스닥150레버리지");
                    foreach (string s in g.kospi_mixed.stock) { displayList.Add(s); }
                    break;

                case "코닥":
                    displayList.Add("KODEX 레버리지");
                    displayList.Add("KODEX 코스닥150레버리지");
                    foreach (string s in g.kosdaq_mixed.stock) { displayList.Add(s); }
                    break;


                case "푀손":
                    var a_tuple = new List<Tuple<double, string>> { };

                    foreach (var o in g.ogl_data)
                    {
                        string stock = o.stock;
                        // if (!o.included) continue; Blocked on 20240406
                        // 레버리지 외 지수관련 모두 continue;
                        if (g.KODEX4.Contains(stock) ||
                        stock.Contains("KOSEF") ||
                        stock.Contains("HANARO") ||
                        stock.Contains("TIGER") ||
                        stock.Contains("KBSTAR") ||
                        stock.Contains("혼합") ||
                        g.보유종목.Contains(stock) ||
                        g.호가종목.Contains(stock))
                        {
                            continue;
                        }

                        int check_row = 0;
                        if (g.test)
                        {
                            check_row = g.time[1] - 1;
                            if (check_row > o.nrow - 1)
                                check_row = o.nrow - 1;
                        }
                        else
                            check_row = o.nrow - 1;
                        if (o.nrow < 2 || o.x[check_row, 4] < 0)
                            continue;



                        double value = 0.0;
                        {
                            for (int i = check_row - 1; i >= 1; i--)
                            {
                                //분간 프돈 매수 -> (현재가 - (후분가 + 전분가) / 2) * 분간프돈매수량 * 전일종가
                                value += (o.x[check_row, 1] - (o.x[i, 1] + o.x[i - 1, 1]) / 2.0) / 100.0 *
                                    (o.x[i, 4] - o.x[i - 1, 4]) * o.전일종가 / g.억원;
                            }
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


        // not used
        //public void Form_보조_차트_DRAW(string stock)
        //{
        //    if (displayList.Count == nRow * nCol)
        //    {
        //        ChartClear();
        //        displayList.Clear();
        //    }

        //    if (displayList.Contains(stock)) { return; }

        //    nCol = 4;
        //    nRow = 3;


        //    displayList.Add(stock);

        //    dr.draw_stock(chart2, nRow, nCol, displayList.Count - 1, stock);
        //}

        private void UpdateFormTitle()
        {
            switch (keyString)
            {
                case "상관":
                    this.Text = $"{keyString} ({g.clickedTitle})";
                    break;
                case "절친":
                    this.Text = $"{keyString} ({g.clickedStock})";
                    break;
                case "그순":
                    this.Text = $"{keyString} ({g.oGl_data_selection})";
                    break;
                default:
                    this.Text = keyString;
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

        public static void ChartClear()
        {
            g.chart2.Series.Clear();
            g.chart2.ChartAreas.Clear();
            g.chart2.Annotations.Clear();
        }

        /// <summary>
        /// Updates stock data selectively based on the provided stock data list.
        /// </summary>
        /// <param name="updatedStocks">List of updated stock data.</param>
        //public void UpdateStockData(List<StockPoint> updatedStocks)
        //{
        //    foreach (var stockData in updatedStocks)
        //    {
        //        if (!displayList.Contains(stockData.Stock)) continue;

        //        if (stockData.Stock.Contains("KODEX"))
        //        {
        //            mm.UpdateChartSeriesKodex(g.chart2, stockData.Stock);
        //        }
        //        else
        //        {
        //            mm.UpdateChartSeriesGeneral(g.chart2, stockData.Stock);
        //        }

        //        // Update or add annotation
        //        if (!stockData.Stock.Contains("KODEX"))
        //        {
        //            mm.UpdateAnnotation(g.chart2, stockData.Stock);
        //        }
        //    }

        //    g.chart2.Invalidate(); // Redraw the chart
        //}

        private void chart2_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);

        }

        private void chart2_MouseClick(object sender, MouseEventArgs e)
        {
            string selection = "";
            double xval = 0.0, yval = 0.0;
            double row_percentage = 0.0, col_percentage = 0.0;
            double col_divider = 0.0;
            int row_id = 0, col_id = 0;

            //chart2_info(e, ref selection, ref xval, ref yval, ref row_percentage,
            //    ref col_percentage, ref row_id, ref col_id, ref col_divider);
            g.clickedStock = cl.CoordinateMapping(chart2, nRow, nCol, displayList, e, ref selection, ref col_id, ref row_id);


            if (Control.ModifierKeys == Keys.Control)
            {

                cl.LeftRightAction(chart2, "l5", row_id, col_id);
                //Thread.Sleep(100);
                cl.CntlLeftRightAction(chart2, selection, row_id, col_id);
            }
            else
            {
                cl.LeftRightAction(chart2, selection, row_id, col_id);
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
            dtb.Rows[0][4] = "코닥";
            dtb.Rows[0][5] = "코피";
            dtb.Rows[0][6] = "절친";
            dtb.Rows[0][7] = "푀손";

            dataGridView1.DataSource = dtb;
            for (int i = 0; i < 8; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[i].Width = this.Width / 8;
            }
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    keyString = "상관";
                    return;
                case 1:
                    keyString = "보유";
                    break;
                case 2:
                    keyString = "그순";
                    break;
                case 3:
                    keyString = "관심";
                    break;
                case 4:
                    keyString = "코닥";
                    break;
                case 5:
                    keyString = "코피";
                    break;
                case 6:
                    keyString = "절친";
                    break;
                case 7:
                    keyString = "푀손";
                    break;
            }
            Form_보조_차트_DRAW();
        }

        private void UpdateAnnotation(string stock, StockPoint stockData)
        {
            var annotationName = $"Annotation_{stock}";
            var existingAnnotation = g.chart2.Annotations
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
                g.chart2.Annotations.Add(newAnnotation);
            }
        }
    }

    public class StockPoint
    {
        public string Stock { get; set; } // Stock name
        public double Time { get; set; }  // XValue (e.g., timestamp)
        public double Price { get; set; } // YValue
    }
}
