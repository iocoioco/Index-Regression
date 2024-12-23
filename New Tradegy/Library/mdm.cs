using New_Tradegy;
using New_Tradegy.Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g;

class mm
{
    // 0, 1(가격), 2(수급), 3(체강), 4(프로그램), 5(외인), 6(기관)
    private static Color[] colorGeneral = { Color.White, Color.Red, Color.DarkGray,
        Color.LightCoral, Color.DarkBlue, Color.Magenta, Color.Cyan };
    // 0, 1(가격), 2, 3(지수합), 4(기관), 5(외인), 6(개인), 7, 8, 9, 10(나스닥), 11(연기금)
    private static Color[] colorKODEX = { Color.White, Color.Red,
        Color.White, Color.Black, Color.Brown, Color.Magenta, Color.Green,
        Color.White, Color.White, Color.White, Color.Blue, Color.Brown };
    // short and long time extension or restoration 
    // and then, draw

    //static List<string> g.dl = new List<string>();
    static int hogaCount = 0;

    static List<string> stocksWithBid = new List<string>();

    static float cellWidth = 100f / g.nCol;
    static float cellHeight = 100f / g.nRow;

    static float chartAreaWidth = cellWidth;
    static float chartAreaHeight = 28.178F;

    static float annotationWidth = cellWidth;
    static float annotationHeight = 33.333F - chartAreaHeight;

    static int dgvWidth = g.screenWidth / g.nCol;    // Position based on screen width
    static int dgvHeight = g.formSize.ch * 12;

    static int formWidth = g.screenWidth / g.nCol;    // Position based on screen width
    static int formHeight = g.screenHeight / g.nRow;
    static int DgvCellHeight = 27;

    public Form _parent;

    // List of fixed stocks and corresponding fixed row positions for forms
    public static string[] fixedStocks = { "KODEX 레버리지", "KODEX 코스닥150레버리지" };

    // Constructor to accept the parent form
    public void ParentPassing(Form parent)
    {
        _parent = parent;
    }


    // no stockName or Seriese          return -1
    // if totalPoints == SeriesePoints  return 0
    // if totalPoints > seriesePoints   return 1
    //public static bool isTotalPointsEqualSeriesPoints(Chart chart, string stockName)
    //{
    //    // Find the index of the stock data in ogl_data
    //    int index = wk.return_index_of_ogldata(stockName);
    //    if (index < 0)
    //    {
    //        return false; // Exit if the stock is not found
    //    }

    //    // Get the data for the stock
    //    g.stock_data o = g.ogl_data[index];
    //    int totalPoints = o.nrow;

    //    string seriesName = stockName + " " + "1";
    //    if (chart.Series.IndexOf(seriesName) == -1)
    //        return true; // Skip if series not found

    //    Series series = chart.Series[seriesName];
    //    int seriesPoints = series.Points.Count;

    //    if (seriesPoints == totalPoints)
    //        return true;
    //    else
    //        return false;
    //}


    static void InitializeFixedElements(Chart chart)
    {
        for (int i = 0; i < fixedStocks.Length; i++)
        {
            string stock = fixedStocks[i];

            // Check if the chart area exists; if not, create it\
            if (ChartAreaExists(chart, stock))
            {
                UpdateChartSeries(chart, stock, g.nRow, g.nCol);
            }
            else
            {
                CreateChartAreaForStock(chart, stock, g.nRow, g.nCol); // location 
            }

            // Check if the form exists; if not, create it and set the location
            Form form = fm.FindFormByName("se");
            if (!fm.DoesDataGridViewExist(form, stock))
            {
                if (!g.connected) continue;

                var a = new jp();

                DataGridView Dgv = a.Generate(stock);

                Dgv.Height = DgvCellHeight * 12;

                if (i == 0)
                    Dgv.Location = new Point((g.screenWidth / g.nCol) + 10, (g.screenHeight / g.nRow) * 0 - 4 * 0);
                else
                    Dgv.Location = new Point((g.screenWidth / g.nCol) + 10, (g.screenHeight / g.nRow) * 2 - 4 * 2);
            }
        }
        int areasCount = g.chart1.ChartAreas.Count;
        int annotationsCount = g.chart1.Annotations.Count;
        int seriesCount = g.chart1.Series.Count;
    }

    // Use SuspendLayout and ResumeLayout to batch updates on the chart.
    // Use data binding for the chart's Series if possible, avoiding manual updates.

    //    Further Optimization Suggestions
    //Virtualization:

    //If many stocks are offscreen, consider using virtualization techniques to avoid plotting them until they are visible.
    //Separate Threads for Plotting:

    //Offload the plotting to a separate thread or task to avoid UI blocking.
    //Limit Points per Series:

    //Retain only the most recent n points in each series to prevent unnecessary overhead from historical data.
    //GPU Acceleration:

    //Use libraries like LiveCharts or SciChart, which are optimized for high-performance data visualization.

    static void HogaCountDiplayList()
    {
        hogaCount = 0;
        stocksWithBid.Clear();
        g.dl.Clear(); // Clear the list

        int TotalSpaceCount = g.nRow * (g.nCol - 2); // Total available slots in the grid

        // Create lists for stocks with and without bid spaces
        // Stocks from 보유종목 and 호가종목
        List<string> remainingStocks = new List<string>();   // Stocks from g.sl (fill empty slots)

        // Helper function to add stocks with bid spaces
        void AddStocksWithBid(IEnumerable<string> stockList)
        {
            foreach (string stock in stockList)
            {
                if (!stocksWithBid.Contains(stock) && !stock.Contains("KODEX"))
                {
                    stocksWithBid.Add(stock);
                }
            }
        }

        // Add stocks from 보유종목 and 호가종목 (with bid spaces)
        AddStocksWithBid(g.보유종목);
        AddStocksWithBid(g.호가종목);

        for (int i = 0; i < TotalSpaceCount; i++)
        {
            g.dl.Add("empty");
        }

        // 보유종목, 호가종목

        for (int i = 0; i < stocksWithBid.Count; i++)
        {
            int Row = i % g.nRow;
            int Col = i / g.nRow;
            g.dl[(Col * 2 + 0) * g.nRow + Row] = stocksWithBid[i];
            g.dl[(Col * 2 + 1) * g.nRow + Row] = "";
            hogaCount++;
        }

        // 관심종목
        int 관심Count = 0;
        for (int i = 0; i < TotalSpaceCount; i++)
        {
            if (g.dl[i] != "empty")
                continue;

            if (관심Count < g.관심종목.Count)
            {
                g.dl[i] = g.관심종목[관심Count++];
            }
            else
            {
                break;
            }
        }

        // eval_stock 종목
        int slCount = g.gid;
        for (int i = 0; i < TotalSpaceCount; i++)
        {
            if (g.dl[i] != "empty")
                continue;

            for (int j = slCount; j < g.sl.Count; j++)
            {
                if (g.dl.Contains(g.sl[j]))
                {
                    continue;
                }
                else
                {
                    g.dl[i] = g.sl[j];
                    slCount = j + 1;
                    break;
                }
            }
        }
    }

    public static void ManageChart1()
    {
        // First, handle the fixed elements
        InitializeFixedElements(g.chart1);

        HogaCountDiplayList(); // 보유, 호가, 관심종목, sl



        g.dl.Clear();
        g.dl.Add("오픈엣지테크놀로지");
        int index = wk.return_index_of_ogldata("오픈엣지테크놀로지");
        g.stock_data o = g.ogl_data[index];

        o.nrow = 45;
        ps.post(o);




        // Now, handle the rest of the dynamically generated elements as before
        int currentRow = 0;
        int currentCol = 2; // Start from the third column since first two columns are occupied

        for (int i = 0; i < stocksWithBid.Count; i++) // hogaCount (chartareas and forms)
        {
            string stock = stocksWithBid[i];

            // Check if the form exists; if not, create it and set the location
            Form form = fm.FindFormByName("se");
            if (!fm.DoesDataGridViewExist(form, stock))
            {
                var a = new jp();
                DataGridView dgv = a.Generate(stock);
                dgv.Height = DgvCellHeight * 12;
            }

            // Check if the chart area exists; if not, create it\
            if (ChartAreaExists(g.chart1, stock))
            {
                UpdateChartSeries(g.chart1, stock, g.nRow, g.nCol);
            }
            else
            {
                ClearChartAreaAndAnnotations(g.chart1, stock);
                CreateChartAreaForStock(g.chart1, stock, g.nRow, g.nCol); // location 
            }

            // Relocate chart area and dataGridView for hogaCount stocks
            RelocateChart1AreaAndDataGridView(g.chart1, stock, currentRow, currentCol);

            currentRow++;
            if (currentRow >= g.nRow)
            {
                currentRow = 0;
                currentCol += 2;
                if (currentCol >= g.nCol)
                {
                    break;
                }
            }
        }

        int areasCount = g.chart1.ChartAreas.Count;
        int annotationsCount = g.chart1.Annotations.Count;
        int seriesCount = g.chart1.Series.Count;

        // Handle the remaining chart areas without forms
        for (int i = 0; i < g.dl.Count; i++)
        {
            string stock = g.dl[i];
            if (wk.isStock(stock) && !stocksWithBid.Contains(stock))
            {

                if (ChartAreaExists(g.chart1, stock))
                {
                    //if (isTotalPointsEqualSeriesPoints(g.chart1, stock))
                    //{
                    UpdateChartSeries(g.chart1, stock, g.nRow, g.nCol);
                    //}
                    //else
                    //{
                    //    ClearChartAreaAndAnnotations(g.chart1, stock);
                    //    CreateChartAreaForStock(g.chart1, stock, g.nRow, g.nCol); // location 
                    //}
                }
                else
                {
                    CreateChartAreaForStock(g.chart1, stock, g.nRow, g.nCol); // location 
                }

                RelocateChart1Area(g.chart1, stock, i % g.nRow, i / g.nRow + 2);
            }
        }
        ClearUnusedChartAreasAndAnnotations(g.chart1, g.dl);
        ClearUnusedDataGridViews(g.chart1, stocksWithBid);

        g.chart1.Invalidate();

        areasCount = g.chart1.ChartAreas.Count;
        annotationsCount = g.chart1.Annotations.Count;
        seriesCount = g.chart1.Series.Count;
    }

    public static void ManageChart2()
    {
        Form_보조_차트 Form_보조_차트 = (Form_보조_차트)System.Windows.Forms.Application.OpenForms["Form_보조_차트"];
        if (Form_보조_차트 != null)
        {
            Form_보조_차트.Form_보조_차트_DRAW();
        }
    }

    public static void ManageChart2(string keystring)
    {
        Form_보조_차트 Form_보조_차트 = (Form_보조_차트)System.Windows.Forms.Application.OpenForms["Form_보조_차트"];
        if (Form_보조_차트 != null)
        {
            Form_보조_차트.keyString = keystring;
            Form_보조_차트.Form_보조_차트_DRAW();
        }
    }



    public static void CreateChartAreaForStock(Chart chart, string stockName, int nRow, int nCol)
    {
        if (stockName.Contains("KODEX"))
        {
            CreateChartAreaForStockKodex(chart, stockName, nRow, nCol); // 
        }
        else
        {
            CreateChartAreaForStockGeneral(chart, stockName, nRow, nCol); // 
        }
    }

    static ChartArea CreateChartAreaForStockKodex(Chart chart, string stockName, int nRow, int nCol)
    {


        int index = wk.return_index_of_ogldata(stockName);



        if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
        {
            return null;
        }

        g.stock_data o = g.ogl_data[index];

        int magnifier_id = -1;
        for (int i = 0; i < g.KODEX4.Count; i++)
        {
            if (o.stock == g.KODEX4[i])
            {
                magnifier_id = i;
                break;
            }
        }

        int y_min = 100000;
        int y_max = -100000;

        // 크기, 위치 결정

        string sid = "";

        if (o.nrow <= 1) // no data yet, i.e. only 0859 
            return null;

        // g.draw_shrink_time is controlled by 'o' and 'O'
        int start_time = 0;
        int end_time = -1;
        if (o.shrink_draw == true)
        {
            start_time = o.nrow - g.draw_shrink_time;
            if (start_time < g.time[0])
            {
                start_time = g.time[0];
            }
        }
        else
        {
            start_time = g.time[0];
        }
        end_time = o.nrow;


        // Check if "ChartArea1" exists and remove it
        var defaultArea = chart.ChartAreas.FindByName("ChartArea1");
        if (defaultArea != null)
        {
            chart.ChartAreas.Remove(defaultArea);
        }
        string area = stockName;
        chart.ChartAreas.Add(area); //  error 0인 요소가 있습니다.

        // Initialize variables
        int EndPoint = 0;
        int[] idIndex = { 1, 3, 4, 5, 6, 10, 11 }; // Data types to draw: price, program, foreign, institute, Nasdaq, pension

        // Process each data type in idIndex
        foreach (int dataIndex in idIndex)
        {
            sid = stockName + " " + dataIndex.ToString();

            AddSeriesToChart(sid, chart, area, GetColorByIndex(dataIndex), GetBorderWidthByIndex(dataIndex));

            Series series = chart.Series[sid];
            EndPoint = 0;
            double magnifier = 1.0;
            KodexMagnifier(o, dataIndex, ref magnifier);
            int value = 0;
            for (int k = start_time; k < end_time; k++)
            {
                if (o.x[k, 0] == 0) break; // No data

                if (dataIndex == 10)
                {
                    // Nasdaq Value is not scraped, if(value == 0), interpolate
                    value = (int)(HandleUSIndex(o, k, end_time, start_time, dataIndex)
                        * magnifier);

                }
                else
                {
                    value = (int)(o.x[k, dataIndex] * magnifier);
                }
                series.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                EndPoint++;

                y_min = Math.Min(y_min, value);
                y_max = Math.Max(y_max, value);
            }

            if (EndPoint < 2)
            {
                chart.Series.Remove(series); // Remove if not enough data
                continue;
            }

            // Draw stock marker and apply styling
            MarkGeneral(chart, stockName, start_time + 1, dataIndex, EndPoint, o.x, series);
        }

        // Remove the y-axis labels
        chart.ChartAreas[area].AxisY.LabelStyle.Enabled = false;

        //// Optional: You can also hide the tick marks for a cleaner look
        chart.ChartAreas[area].AxisY.MajorTickMark.Enabled = false;
        chart.ChartAreas[area].AxisY.MinorTickMark.Enabled = false;

        // Disable grid lines if not already done
        chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
        chart.ChartAreas[area].AxisY.MinorGrid.Enabled = false;

        chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(5, 5, 70, 90);
        chart.ChartAreas[area].AxisY.Minimum = Math.Floor(y_min / 100.0) * 100;
        chart.ChartAreas[area].AxisY.Maximum = Math.Ceiling(y_max / 100.0) * 100;

        chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;
        chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
        chart.ChartAreas[area].AxisX.Interval = EndPoint - 1;
        chart.ChartAreas[area].AxisX.IntervalOffset = 1;

        if (chart == g.chart1)
        {
            float cellWidth = 100 / g.nCol;
            chart.ChartAreas[area].Position = stockName == "KODEX 레버리지" ?
                new ElementPosition(0, 0, cellWidth, 50) :
                new ElementPosition(0, 50, cellWidth, 50);

        }
        else
        {
            float cellWidth = 100 / 5;
            float cellHeight = 100 / 3;
            chart.ChartAreas[area].Position = stockName == "KODEX 레버리지" ?
                new ElementPosition(0, 0, cellWidth, cellHeight) :
                new ElementPosition(0, cellHeight, cellWidth, cellHeight);
        }

        chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        chart.ChartAreas[area].AxisY.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        int b = 255;
        int r = 255;
        if (o.분당가격차 > 35)
        {
            r = 150;
        }
        else if (o.분당가격차 > 28)
        {
            r = 170;
        }
        else if (o.분당가격차 > 21)
        {
            r = 190;
        }
        else if (o.분당가격차 > 14)
        {
            r = 210;
        }
        else if (o.분당가격차 > 7)
        {
            r = 230;
        }
        else if (o.분당가격차 <= 7 && o.분당가격차 >= -7)
        {

        }
        else if (o.분당가격차 > -14)
        {
            b = 230;
        }
        else if (o.분당가격차 > -21)
        {
            b = 210;
        }
        else if (o.분당가격차 > -28)
        {
            b = 190;
        }
        else if (o.분당가격차 > -35)
        {
            b = 170;
        }
        else
        {
            b = 150;
        }
        chart.ChartAreas[area].BackColor = Color.FromArgb(b, r, 255); // 사이안, 빨강

        return chart.ChartAreas[area];
    }

    static ChartArea CreateChartAreaForStockGeneral(Chart chart, string stockName, int nRow, int nCol)
    {

        int index = wk.return_index_of_ogldata(stockName);


        if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
        {
            return null;
        }

        g.stock_data o = g.ogl_data[index];

        if (o.nrow < 2)
            return null;

        int y_min = 100000;
        int y_max = -100000;

        string sid = "";

        // start time and end time
        int start_time = 0;
        int end_time = -1;

        // start time 
        if (o.shrink_draw == true)
        {
            start_time = o.nrow - g.draw_shrink_time;
            if (start_time < g.time[0])
            {
                start_time = g.time[0];
            }
        }
        else
        {
            start_time = g.time[0];
        }

        // end time & maximum of a, i.e. amount
        end_time = o.nrow;


        // 단일가거래 종목은 차트 포함시키지 않음
        // 동신건설 거래정지 종목으로 return
        if (o.x[end_time - 1, 3] == 0)
            return null;

        // The start of area and drawing of stock
        string area = stockName;


        if (chart.InvokeRequired)
        {
            chart.Invoke(new Action(() =>
            {
                // Update the chart here
                chart.ChartAreas.Add(area);
            }));
        }
        else
        {
            // Update the chart here
            chart.ChartAreas.Add(area);
        }

        int EndPoint = 0;
        int[] dataTypesToDraw = { 1, 2, 3 }; // Only price, amount, intensity, and centerline

        for (int i = 1; i < 10; i++)
        {
            if (i == 4 || i == 5 || i == 6 || i == 7 || i == 8 || i == 9) continue;

            sid = stockName + " " + i.ToString();
            Series t = new Series(sid)
            {
                ChartArea = area,
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Date,
                IsVisibleInLegend = false
            };

            if (chart.InvokeRequired)
            {
                chart1.Invoke(new Action(() =>
                {
                    // Update the chart here
                    chart.Series.Add(t);
                }));
            }
            else
            {
                // Update the chart here
                chart.Series.Add(t);
            }

            EndPoint = 0;

            for (int k = start_time; k < end_time; k++)
            {
                if (o.x[k, 0] == 0)
                {
                    if (EndPoint < 2) return null;
                    else break;
                }

                int value = 0;
                switch (i)
                {
                    case 1: // price
                        value = GeneralValue(o, k, 1);
                        break;
                    case 2: // amount
                        value = GeneralValue(o, k, 2);
                        break;
                    case 3: // intensity
                        value = GeneralValue(o, k, 3);
                        break;

                }

                //if (i != 9) // Skip further processing for centerline as it's already set
                //{

                if (chart.InvokeRequired)
                {
                    chart.Invoke(new Action(() =>
                    {
                        // Update the chart here
                        t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                    }));
                }
                else
                {
                    // Update the chart here
                    t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                }

                EndPoint++;

                y_min = Math.Min(y_min, value);
                y_max = Math.Max(y_max, value);
                //}
            }

            if (EndPoint < 2) return null;

            //DrawStockMark((chart, stockName, start_time, i, EndPoint, o.x, t);
            MarkGeneral(chart, stockName, start_time + 1, i, EndPoint, o.x, t);

            // Apply styles based on series type
            switch (i)
            {
                case 1: // Price
                    t.Color = colorGeneral[1];
                    t.BorderWidth = g.width.가격;
                    break;
                case 2: // Amount
                    t.Color = colorGeneral[2];
                    t.BorderWidth = 1;
                    break;
                case 3: // Intensity
                    t.Color = colorGeneral[3];
                    t.BorderWidth = 1;
                    break;
                case 9: // Centerline
                    t.Color = Color.Magenta;
                    t.BorderWidth = 1;
                    break;
            }
        }

        int[] id = { 4, 5, 6 }; // IDs for program, foreign, institute
        for (int i = 0; i < id.Length; i++)
        {
            sid = stockName + " " + id[i].ToString();

            // Initialize and configure the Series
            Series t = new Series(sid)
            {
                ChartArea = area,
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Date,
                IsVisibleInLegend = false
            };
            chart.Series.Add(t);

            // Populate data points for each ID
            for (int k = start_time; k < end_time; k++)
            {
                if (o.x[k, 0] == 0) // If time is 0, break as it indicates the end of data
                    break;

                int value = GeneralValue(o, k, id[i]);

                t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value); // Add data point

                // Update y-axis min and max for scaling
                y_min = Math.Min(y_min, value);
                y_max = Math.Max(y_max, value);
            }

            // Draw stock markers for visualization
            MarkGeneral(chart, stockName, start_time, id[i], EndPoint, o.x, t);

            // Set color and border width based on id type
            switch (id[i])
            {
                case 4: // Program
                    t.Color = colorGeneral[4];
                    t.BorderWidth = g.width.프돈;
                    break;
                case 5: // Foreign
                    t.Color = colorGeneral[5];
                    t.BorderWidth = g.width.외돈;
                    break;
                case 6: // Institute
                    t.Color = colorGeneral[6];
                    t.BorderWidth = g.width.기관;
                    break;
                default:
                    break;
            }
        }

        string annotation = AnnotationGeneral(chart, o, o.x, start_time, end_time, o.nrow); // g.test, o.nrow = g.MAX_ROW

        int numLines = 5;
        double annotationHeight;
        double chartAreaHeight;
        CalculateHeights(g.v.font, numLines, g.nRow, out annotationHeight, out chartAreaHeight); // 5.155, 28.178

        Color BackColor = Color.White;
        int[] scoreThresholds = { 90, 70, 50, 30, 10 };
        for (int i = 0; i < scoreThresholds.Length; i++)
        {
            if (o.점수.총점 > scoreThresholds[i])
            {
                BackColor = g.Colors[i];
                break;
            }
        }

        float[] location = new float[2];
        location[0] = 0; // X-coordinate (left edge)
        location[1] = 0; // Y-coordinate (top edge)

        if (chart == g.chart1)
            AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1], // 0, 0
                100 / nCol, (int)annotationHeight + 2), area, Color.Black, BackColor);
        else
            AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1] + 3,
                100 / nCol, (int)annotationHeight + 2), area, Color.Black, BackColor);






        // Remove the y-axis labels
        chart.ChartAreas[area].AxisY.LabelStyle.Enabled = false;

        // Optional: You can also hide the tick marks for a cleaner look
        chart.ChartAreas[area].AxisY.MajorTickMark.Enabled = false;
        chart.ChartAreas[area].AxisY.MinorTickMark.Enabled = false;

        // Disable grid lines if not already done
        chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
        chart.ChartAreas[area].AxisY.MinorGrid.Enabled = false;

        chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(20, 5, 55, 90); // (20, 10, 60, 80);
        int chartYMin = (int)Math.Floor(y_min / 100.0) * 100;
        int chartYMax = (int)Math.Ceiling(y_max / 100.0) * 100;
        chart.ChartAreas[area].AxisY.Minimum = (double)chartYMin;
        chart.ChartAreas[area].AxisY.Maximum = (double)chartYMax;


        // Disable the secondary Y-axis
        // chart.ChartAreas[area].AxisY2.Enabled = AxisEnabled.False;

        // Optional: Disable grid lines and tick marks for clarity (if enabled elsewhere)
        // chart.ChartAreas[area].AxisY2.MajorGrid.Enabled = false;
        // chart.ChartAreas[area].AxisY2.MajorTickMark.Enabled = false;
        // chart.ChartAreas[area].AxisY2.MinorGrid.Enabled = false;
        // chart.ChartAreas[area].AxisY2.LabelStyle.Enabled = false;


        chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;
        chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
        chart.ChartAreas[area].AxisX.Interval = EndPoint - 1; // total number of point = 2, while mir 0820 -> 217
        chart.ChartAreas[area].AxisX.IntervalOffset = 1;
        // chart.ChartAreas[area].Position.X = location[0]; // 0







        chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        chart.ChartAreas[area].AxisY.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        // 높은 등수 낮은 등수 : 낮은 등수로 eval_group에서 등록됨 : 수정 완료
        BackColor = Color.White;
        if (g.q == "o&s" && o.점수.그순 < 5)
        {
            chart.ChartAreas[area].BackColor = g.Colors[o.점수.그순]; //
        }

        if (o.분프로천[0] > 5 && o.분외인천[0] > 5 && o.분배수차[0] > 0)
            chart.ChartAreas[area].BackColor = g.Colors[5]; //

        return chart.ChartAreas[area];
    }

























    public static void MarkKodex(Chart chart, string stock, int StartPoint, int i, int EndPoint, int[,] x, Series t) // i (가격, 수급, 체강 순으로 Series)
    {
        // { 1, 3, 4, 5, 6, 10, 11 }; price, program, foreign, institute, individua, Nasdaq, pension
        // draw mark for price, amount, intensity

        int index = wk.return_index_of_ogldata(stock);

        if (index < 0)
            return;

        g.stock_data o = g.ogl_data[index];

        // if price increases more than the specified value, than circle mark
        for (int m = StartPoint; m <= EndPoint; m++)
        {
            if (i == 1) // price
            {

                int price_change = x[m, 1] - x[m - 1, 1];

                if (price_change >= 20) // KODEX 가격 변화가 20 이상 상승하는 경우 Circle로 표시
                {
                    if (price_change > 40)
                    {
                        t.Points[m].MarkerColor = Color.Blue; // KODEX
                    }
                    else if (price_change > 30)
                    {
                        t.Points[m].MarkerColor = Color.Green; // KODEX
                    }
                    else
                    {
                        t.Points[m].MarkerColor = Color.Red; // KODEX
                    }
                    t.Points[m].MarkerSize = 7;
                    t.Points[m].MarkerStyle = MarkerStyle.Circle;
                }

                if (o.분거래천[0] > 10) // if o.분거래천[0] <= 10 including g.q = "h&s"
                {
                    int mark_size = 0;
                    Color color = Color.White;
                    if (o.분거래천[0] < 10)
                    {
                    }
                    else if (o.분거래천[0] < 50)
                    {
                        color = Color.Red;
                        mark_size = 10;
                    }
                    else if (o.분거래천[0] < 100)
                    {
                        color = Color.Red;
                        mark_size = 15;
                    }
                    else if (o.분거래천[0] < 200)
                    {
                        color = Color.Green;
                        mark_size = 15;
                    }
                    else if (o.분거래천[0] < 300)
                    {
                        color = Color.Green;
                        mark_size = 20;
                    }
                    else if (o.분거래천[0] < 500)
                    {
                        color = Color.Blue;
                        mark_size = 20;
                    }
                    else if (o.분거래천[0] < 800)
                    {
                        color = Color.Blue;
                        mark_size = 30;
                    }
                    else if (o.분거래천[0] < 1200)
                    {
                        color = Color.Black;
                        mark_size = 30;
                    }
                    else if (o.분거래천[0] < 1700)
                    {
                        color = Color.Black;
                        mark_size = 40;
                    }
                    else if (o.분거래천[0] >= 1700)
                    {
                        color = Color.Black;
                        mark_size = 50;
                    }
                    t.Points[0].Color = color;

                    t.Points[0].MarkerSize = mark_size;

                    if (price_change >= 0)
                        t.Points[0].MarkerStyle = MarkerStyle.Circle;
                    else
                        t.Points[0].MarkerStyle = MarkerStyle.Cross;
                }
            }
        }



    }

    public static void MarkGeneral(Chart chart, string stock, int SartPoint, int i, int EndPoint, int[,] x, Series t) // i (가격, 수급, 체강 순으로 Series)
    {
        // { 1, 2, 3, 4, 5, 6 }; price, amount, intensity, program, foreign, institute

        // draw mark for price, amount, intensity only
        if (i > 3) return;

        int index = wk.return_index_of_ogldata(stock);

        if (index < 0)
            return;

        g.stock_data o = g.ogl_data[index];

        // if price increases more than the specified value, than circle mark
        for (int m = SartPoint; m <= EndPoint; m++)
        {
            if (i == 1) // price
            {
                int price_change = x[m, 1] - x[m - 1, 1];

                if (price_change >= 100)
                {
                    if (price_change > 300)
                    {
                        t.Points[m].MarkerColor = Color.Black;
                    }
                    else if (price_change > 200)
                        t.Points[m].MarkerColor = Color.Blue;
                    else if (price_change > 150)
                        t.Points[m].MarkerColor = Color.Green;
                    else
                        t.Points[m].MarkerColor = Color.Red;

                    t.Points[m].MarkerSize = 6;
                    t.Points[m].MarkerStyle = MarkerStyle.Cross;
                }

                if (x[m, 0] >= 90200) // 시간이 9:02 이후 배수 차이가 100 이상일 때
                {
                    int multiple_difference = x[m, 8] - x[m, 9]; // 배수 차이 
                    if (multiple_difference > 100)
                    {

                        t.Points[m].MarkerSize = 9;
                        t.Points[m].MarkerStyle = MarkerStyle.Circle;

                        if (multiple_difference > 500)
                            t.Points[m].MarkerColor = Color.Black;
                        else if (multiple_difference > 300)
                            t.Points[m].MarkerColor = Color.Blue;
                        else if (multiple_difference > 200)
                            t.Points[m].MarkerColor = Color.Green;
                        else
                            t.Points[m].MarkerColor = Color.Red;
                    }
                }
            }

            if (o.분거래천[0] > 10) // if o.분거래천[0] <= 10 including g.q = "h&s"
            {
                int price_change = x[m, 1] - x[m - 1, 1]; //?
                int mark_size = 0;
                Color color = Color.White;
                if (o.분거래천[0] < 50)
                {
                    color = Color.Red;
                    mark_size = 10;
                }
                else if (o.분거래천[0] < 100)
                {
                    color = Color.Red;
                    mark_size = 15;
                }
                else if (o.분거래천[0] < 200)
                {
                    color = Color.Green;
                    mark_size = 15;
                }
                else if (o.분거래천[0] < 300)
                {
                    color = Color.Green;
                    mark_size = 20;
                }
                else if (o.분거래천[0] < 500)
                {
                    color = Color.Blue;
                    mark_size = 20;
                }
                else if (o.분거래천[0] < 800)
                {
                    color = Color.Blue;
                    mark_size = 30;
                }
                else if (o.분거래천[0] < 1200)
                {
                    color = Color.Black;
                    mark_size = 30;
                }
                else if (o.분거래천[0] < 1700)
                {
                    color = Color.Black;
                    mark_size = 40;
                }
                else if (o.분거래천[0] >= 1700)
                {
                    color = Color.Black;
                    mark_size = 50;
                }
                t.Points[0].MarkerColor = color;
                t.Points[0].MarkerSize = mark_size;

                if (price_change >= 0)
                    t.Points[0].MarkerStyle = MarkerStyle.Circle;
                else
                    t.Points[0].MarkerStyle = MarkerStyle.Cross;
            }


            // magenta and cyan cross mark on the lines of amount and intensity
            if (i == 2 || i == 3)
            {
                if (x[m, i + 8] >= g.npts_for_magenta_cyan_mark) // the last price lowering magenta and cyan excluded
                                                                 //if (x[m, i + 8] >= g.npts_for_magenta_cyan_mark && x[m, 1] - x[m - 1, 1] >= 0) // the last price lowering magenta and cyan excluded
                {
                    if (i == 2)
                    {
                        t.Points[m].MarkerColor = Color.Magenta; // amount
                    }
                    else
                    {
                        t.Points[m].MarkerColor = Color.Cyan;  // intensity
                    }

                    t.Points[m].MarkerStyle = MarkerStyle.Cross;
                    t.Points[m].MarkerSize = 7;
                }
            }
        }
    }

    public static void LabelGeneral(Chart chart, string stock, int i, int EndPoint, int[,] x, Series t) // i (가격, 수급, 체강 순으로 Series))
    {
        // { 1, 2, 3, 4, 5, 6 }; price, amount, intensity, program, foreign, institute
        // 1, 4, 5 extened label
        // 2, 3 only 1 label
        // 6 no label
        int end_id = EndPoint - 1;
        int index = wk.return_index_of_ogldata(stock);

        if (index < 0)
            return;

        g.stock_data o = g.ogl_data[index];

        string string_to_add;
        double d = 0;

        switch (i)
        {
            case 1:
            case 4:
            case 5:
                // intitail data setting
                if (i == 1)
                    t.Points[EndPoint - 1].Label = "      " + x[end_id, i].ToString();
                else if (i == 4)
                    t.Points[EndPoint - 1].Label = o.프누천.ToString("F1");
                else if (i == 5)
                    t.Points[EndPoint - 1].Label = o.외누천.ToString("F1");

                // following data setting
                for (int k = 0; k < 4; k++)
                {
                    if (end_id - k - 1 < 0)
                        break;

                    if (i == 1)
                        d = x[end_id - k, i] - x[end_id - k - 1, i];
                    else if (i == 4)
                        d = o.분프로천[k] - o.분프로천[k + 1];
                    else if (i == 5)
                        d = o.분외인천[k] - o.분외인천[k + 1];

                    if (d > 0)
                        t.Points[EndPoint - 1].Label += "+" + d.ToString("F1");
                    else
                        t.Points[EndPoint - 1].Label += d.ToString("F1");
                }
                break;
            case 2:
            case 3:
                t.Points[EndPoint - 1].Label = o.x[end_id, i].ToString();
                break;
            case 6:
                return;
        }

        t.Color = colorGeneral[i];

        // working
        if (chart.Name == "chart1")
            t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))); // Calibri
        else
            t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
    }

    public static void LabelKodex(Chart chart, string stock, int start_time, int i, int EndPoint, int[,] x, Series t)
    {
        // { 1, 3, 4, 5, 6, 10, 11 }; price, program, foreign, institute, individua, Nasdaq, pension
        // all are extended label upto 4, or 5

        int index = wk.return_index_of_ogldata(stock);

        if (index < 0)
            return;

        g.stock_data o = g.ogl_data[index];


        int end_id = EndPoint - 1;
        int d = 0;

        switch (i)
        {
            case 1:
            case 4:
            case 5:
                // intitail data setting
                if (i == 1)
                    t.Points[EndPoint - 1].Label = "      " + x[end_id, i].ToString();
                else if (i == 4)
                    t.Points[EndPoint - 1].Label = o.프누천.ToString("F1");
                else if (i == 5)
                    t.Points[EndPoint - 1].Label = o.외누천.ToString("F1");

                // following data setting
                for (int k = 0; k < 4; k++)
                {
                    if (end_id - k - 1 < 0)
                        break;

                    if (i == 1)
                        d = x[end_id - k, i] - x[end_id - k - 1, i];
                    else if (i == 4)
                        d = (int)Math.Round(o.분프로천[k] - o.분프로천[k - 1]);
                    else if (i == 5)
                        d = (int)Math.Round(o.분외인천[k] - o.분외인천[k - 1]);

                    if (d >= 0 && k >= 1)
                        t.Points[EndPoint - 1].Label += "+" + d.ToString();
                    else
                        t.Points[EndPoint - 1].Label += d.ToString();
                }
                break;
            case 2:
            case 3:
                t.Points[EndPoint - 1].Label = o.x[end_id, i].ToString();
                break;
            case 6:
                return;
        }

        t.Color = colorKODEX[i];


        // working
        if (chart.Name == "chart1")
            t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))); // Calibri
        else
            t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
    }


    public static void ClearUnusedDataGridViews(Chart chart, List<string> stockswithbid)
    {
        if (chart == g.chart1)
        {
            stockswithbid.Add(fixedStocks[0]);
            stockswithbid.Add(fixedStocks[1]);
        }

        List<string> notInStocksWithBid = new List<string>();

        // Get the target form
        Form se = Application.OpenForms["se"];
        if (se == null)
        {
            MessageBox.Show("Form 'se' is not open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Iterate through all controls in the form
        foreach (Control control in se.Controls)
        {
            // Check if the control is a DataGridView
            if (control is DataGridView dataGridView)
            {
                // Check if the DataGridView name is not in stocksWithBid
                if (!stockswithbid.Contains(dataGridView.Name))
                {
                    notInStocksWithBid.Add(dataGridView.Name);
                }
            }
        }

        // Safely handle unsubscriptions and removal
        List<string> keysToRemove = new List<string>();
        foreach (var dgvName in notInStocksWithBid)
        {
            // Unsubscribe and dispose
            if (g.jpjds.TryGetValue(dgvName, out object a) && a is DSCBO1Lib.StockJpbid _stockjpbid)
            {
                try
                {
                    _stockjpbid.Unsubscribe();

                    // Find the DataGridView and dispose of it
                    Form form = fm.FindFormByName("se");
                    DataGridView dgv = fm.FindDataGridViewByName(form, dgvName);

                    if (dgv != null)
                    {
                        dgv.Dispose();
                    }

                    // Mark for removal
                    keysToRemove.Add(dgvName);
                }
                catch (Exception ex)
                {
                    // Log or handle errors gracefully
                    MessageBox.Show($"Error while processing {dgvName}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Remove from dictionary after iteration
        foreach (var key in keysToRemove)
        {
            g.jpjds.Remove(key);
        }

        stockswithbid.Remove(fixedStocks[0]);
        stockswithbid.Remove(fixedStocks[1]);
    }

    public static void ClearUnusedChartAreasAndAnnotations(Chart chart, List<string> displayedStockList)
    {
        foreach (var stockName in chart.ChartAreas
            .Cast<ChartArea>()
            .Where(ca => !displayedStockList.Contains(ca.Name))
            .Select(ca => ca.Name)
            .ToList())
        {
            ClearChartAreaAndAnnotations(chart, stockName);
        }
    }

    public static void ClearChartAreaAndAnnotations(Chart chart, string stockName)
    {
        // Remove associated Series
        var seriesToRemove = chart.Series
            .Cast<Series>()
            .Where(s => s.ChartArea == stockName) // Match Series to the ChartArea
            .ToList();

        foreach (var series in seriesToRemove)
        {
            chart.Series.Remove(series);
        }

        // Remove the ChartArea
        var chartArea = chart.ChartAreas
            .Cast<ChartArea>()
            .FirstOrDefault(ca => ca.Name == stockName);
        if (chartArea != null)
        {
            chart.ChartAreas.Remove(chartArea);
        }

        // Remove associated Annotations
        var annotationsToRemove = chart.Annotations
            .Cast<Annotation>()
            .Where(ann => ann.Name == stockName)
            .ToList();

        foreach (var annotation in annotationsToRemove)
        {
            chart.Annotations.Remove(annotation);
        }
    }

    // AnnotationGeneral contains AnnotationKODEX
    public static string AnnotationGeneral(Chart chart, g.stock_data o, int[,] x, int start_time, int end_time, int total_nrow)
    {
        string stock = o.stock;

        string stock_title = "";


        // 일반의 첫째 라인

        if (g.보유종목.Contains(stock))
        {
            stock_title = "$$" + stock_title;
        }
        else if (g.호가종목.Contains(stock))
        {
            stock_title = "@ " + stock_title;
        }

        else if (g.관심종목.Contains(stock))
        {
            stock_title = "@ " + stock_title;
        }

        if (stock == "KODEX 레버리지")
        {
            stock_title += "코스피" + "\n";
        }
        else if (stock == "KODEX 코스닥150레버리지")
        {
            stock_title += "코스닥" + "\n";
        }
        else
        {
            string first5Chars;
            if (stock.Length >= 5)
            {
                first5Chars = stock.Substring(0, 5);
            }
            else
            {
                first5Chars = stock; // or handle the case where the string is shorter than 6 characters
            }
            stock_title += first5Chars;
        }


        if (!g.KODEX4.Contains(stock))
        {
            if (o.oGL_sequence_id < 0) // 종목이 그룹 안에 없을 경우 종목 이름 뒤 한 칸 띄고 'x' 표시
                stock_title += "%";
            else
                stock_title += " ";
            stock_title += Math.Round(o.종거천 / 10.0) + "  " +

                               (o.프누천 / 10.0).ToString("F2") + "  " +
                               (o.외누천 / 10.0).ToString("F2") + "  " +
                               (o.기누천 / 10.0).ToString("F0");
        }
                (o.프누천 / 10.0).ToString("F2");





        stock_title += ("\n" + AnnotationGeneralMinute(o, x, start_time, end_time));

        // 일반 : 프돈 + 외돈 
        if (!(g.KODEX4.Contains(stock)))
        {
            stock_title += "\n";
            if (o.분외인천[0] >= 0)
                stock_title += o.분프로천[0].ToString("F0") + "+" + o.분외인천[0].ToString("F0") + "/" + o.분거래천[0].ToString("F0");
            else
                stock_title += o.분프로천[0].ToString("F0") + o.분외인천[0].ToString("F0") + "/" + o.분거래천[0].ToString("F0");

            for (int i = 1; i < 5; i++)
                stock_title += "   " + (o.분프로천[i] + o.분외인천[i]).ToString("F0") +
                    "/" + o.분거래천[i].ToString("F0");

            // 0825 DELETE
            stock_title += "\n";
            stock_title += (o.점수.푀분).ToString("F0"); // o.점수.거분).ToString("F0");
            if (o.점수.배차.ToString("F0") == "0" || o.점수.배차 >= 0) // 반올림값이 0 보다 큰 경우
                stock_title += "+";
            stock_title += (o.점수.배차).ToString("F0");
            if (o.점수.배합.ToString("F0") == "0" || o.점수.배합 >= 0) // 반올림값이 0 보다 큰 경우
                stock_title += "+";
            stock_title += o.점수.배합.ToString("F0");
            stock_title += "+" + o.점수.그순.ToString("F0");
            stock_title += " (" + o.x[end_time - 1, 1].ToString() + " / " + o.현재가.ToString("#,##0") + ")";
        }
        // KODEX4
        else
        {
            stock_title += "\n" + "(" + o.x[end_time - 1, 1].ToString() + "/" + o.현재가.ToString("#,##0") + ")";
        }

        //stock_title += " " + o.dev_avr; // 수급, 체강의 연속점수 삭제
        stock_title += " " + x[end_time - 1, 10] + "/" + x[end_time - 1, 11] + "  " + o.dev_avr; // 수급, 체강의 연속점수
        return stock_title;
    }

    public static string AnnotationGeneralMinute(g.stock_data o, int[,] x, int start_time, int end_time)
    {
        string tick_minute_string = "";
        string stock = o.stock;

        if (stock == "KODEX 레버리지")
        {
            for (int i = 0; i < 3; i++) 
            {
                tick_minute_string += ((int)(g.kospi_틱매수배[i])).ToString() + "/" + //ToString("0.#");
                        ((int)(g.kospi_틱매도배[i])).ToString() + "  ";
            }
            tick_minute_string += "\n";
        }
        else if (stock == "KODEX 200선물인버스2X")
        {
            for (int i = 0; i < 3; i++) 
            {
                tick_minute_string += ((int)(g.kospi_틱매도배[i])).ToString() + "/" + //ToString("0.#");
                        ((int)(g.kospi_틱매수배[i])).ToString() + "  ";
            }
            tick_minute_string += "\n";
        }

        else if (stock == "KODEX 코스닥150레버리지")
        {
            for (int i = 0; i < 3; i++) 
            {
                tick_minute_string += ((int)(g.kosdaq_틱매수배[i])).ToString() + "/" + //ToString("0.#");
                        ((int)(g.kosdaq_틱매도배[i])).ToString() + "  ";
            }
            tick_minute_string += "\n";
        }
        else if (stock == "KODEX 코스닥150선물인버스")
        {
            for (int i = 0; i < 3; i++) // 
            {
                tick_minute_string += ((int)(g.kosdaq_틱매도배[i])).ToString() + "/" + //ToString("0.#");
                    ((int)(g.kosdaq_틱매수배[i])).ToString() + "  ";
            }
            tick_minute_string += "\n";
        }
        else // 
        {
            // MDF 20230302
            //for (int i = 0; i < 4; i++) // 
            //{
            //    tick_minute_string += ((int)(o.틱매수배[i])).ToString() + "/" + //ToString("0.#");
            //            ((int)(o.틱매도배[i])).ToString() + "  ";
            //}
            //tick_minute_string += "\n";
        }


        // x[k, 8 & 9]
        for (int i = end_time - 1; i >= end_time - 5; i--)
        {
            if (i < 1)
            {
                break;
            }
            if (i == end_time - 1)
                tick_minute_string += x[i, 8].ToString() + "/" + //ToString("0.#");
                    x[i, 9].ToString() + "  ";
            else
                tick_minute_string += x[i, 8].ToString() + "/" + //ToString("0.#");
                    x[i, 9].ToString() + "  ";
        }
        return tick_minute_string;
    }


    private static void AddRectangleAnnotationWithText(
    Chart chart,
    string text,
    RectangleF rect,
    string chartAreaName,
    Color textColor,
    Color backgroundColor)
    {

        // Get the ChartArea's position and size
        ChartArea chartArea = chart.ChartAreas[chartAreaName];
        if (chartArea == null)
        {
            throw new ArgumentException($"ChartArea '{chartAreaName}' does not exist.");
        }

        // Calculate position relative to the chart area's dimensions
        double chartAreaTop = chartArea.Position.Y;
        double chartAreaLeft = chartArea.Position.X;
        double chartAreaWidth = chartArea.Position.Width;
        double chartAreaHeight = chartArea.Position.Height;

        // Adjust the annotation to appear at the top of the ChartArea
        double annotationX = chartAreaLeft + (rect.X * chartAreaWidth / 100); // Adjust rect.X relative to ChartArea width
        double annotationY = chartAreaTop; // Align annotation to the top of the ChartArea

        // Create the annotation
        RectangleAnnotation rectangleAnnotation = new RectangleAnnotation
        {
            Name = chartAreaName, // assign the same name as area name
            Text = text,
            Font = new Font("Arial", g.v.font),
            X = annotationX,
            Y = annotationY,
            Width = rect.Width, //! * chartAreaWidth / 100, // Adjust rect.Width relative to ChartArea width
            Height = rect.Height, //! * chartAreaHeight / 100, // Adjust rect.Height relative to ChartArea height
            LineColor = Color.Transparent,
            BackColor = backgroundColor,
            ForeColor = textColor,
            //ClipToChartArea = chartAreaName, // Must match the target ChartArea's name

            ClipToChartArea = "", // chartAreaName,
            AxisXName = chartAreaName + "\\X",
            AxisYName = chartAreaName + "\\Y",
            Alignment = ContentAlignment.TopLeft,
            ToolTip = "Rectangle Annotation Tooltip" // Optional: Add tooltip to verify it's being added
        }; ;

        // Add the annotation to the chart
        chart.Annotations.Add(rectangleAnnotation);
        // Ensure that the chart layout is recalculated
        chart.Invalidate();  // This forces the chart to refresh its layout

    }

    static void CalculateHeights(float fontSize, int numLines, int numRows, out double annotationHeight, out double chartAreaHeight)
    {
        // Get screen dimensions
        Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
        int screenHeight = workingRectangle.Height;

        // Conversion factor: 1 point ≈ 1.33 pixels
        double conversionFactor = 1.33;

        // Calculate the height of the annotation in pixels
        double labelHeightPixels = fontSize * numLines * conversionFactor;

        // Calculate the total available height in relative terms (percentage)
        double totalAvailableHeight = 100.0;

        // Calculate the height for one annotation and one chart area
        double totalRowHeightPercentage = totalAvailableHeight / numRows;

        // Calculate the height of the annotation in percentage based on screen height
        annotationHeight = (labelHeightPixels / screenHeight) * totalAvailableHeight;

        // Calculate the remaining height for the chart area
        chartAreaHeight = totalRowHeightPercentage - annotationHeight;
    }

    public static void UpdateChartSeriesKodex(Chart chart, string stockName)
    {
        // Find the index of the stock data in ogl_data
        int index = wk.return_index_of_ogldata(stockName);
        if (index < 0)
        {
            return; // Exit if the stock is not found
        }

        // Get the data for the stock
        g.stock_data o = g.ogl_data[index];
        int totalPoints = o.nrow;

        // Update or add points for each series in "KODEX" stock
        string[] seriesNames = { "1", "3", "4", "5", "6", "10", "11" };

        // Iterate over each series name and update
        foreach (string suffix in seriesNames)
        {
            double magnifier = 1.0;
            KodexMagnifier(o, Convert.ToInt32(suffix), ref magnifier);

            string seriesName = stockName + " " + suffix;
            if (chart.Series.IndexOf(seriesName) == -1)
                continue; // Skip if series not found

            Series series = chart.Series[seriesName];
            int seriesPoints = series.Points.Count;

            if (seriesPoints == totalPoints)
            {
                // If series has the same number of points, replace the last point
                series.Points[seriesPoints - 1].SetValueXY(((int)(o.x[seriesPoints - 1, 0] / g.HUNDRED)).ToString(),
                    o.x[totalPoints - 1, int.Parse(suffix)]);
            }
            //else if (seriesPoints < totalPoints)
            //{
            //    // If the series has fewer points, add missing points
            //    for (int i = seriesPoints; i < totalPoints; i++)
            //    {
            //        if (chart.InvokeRequired)
            //        {
            //            chart.Invoke(new Action(() =>
            //            {
            //                // Update the chart here
            //                series.Points.AddXY(((int)(o.x[i, 0] / g.HUNDRED)).ToString(), o.x[i, int.Parse(suffix)]);
            //            }));
            //        }
            //        else
            //        {
            //            // Update the chart here
            //            series.Points.AddXY(((int)(o.x[i, 0] / g.HUNDRED)).ToString(), o.x[i, int.Parse(suffix)]);
            //        }
            //    }
            //}
        }

        // Update or add points for "stockName 9" - the centerline
        string centerlineSeriesName = stockName + " 9";
        if (chart.Series.IndexOf(centerlineSeriesName) != -1)
        {
            Series centerlineSeries = chart.Series[centerlineSeriesName];

            if (centerlineSeries.Points.Count == 2)
            {
                // Replace the last point with the updated x value and y = 0
                centerlineSeries.Points[1].SetValueXY(((int)(o.x[totalPoints - 1, 0] / g.HUNDRED)).ToString(), 0);
            }
            else
            {
                // Clear and add two points if somehow not two points (should be consistent with two points)
                centerlineSeries.Points.Clear();
                centerlineSeries.Points.AddXY(((int)(o.x[0, 0] / g.HUNDRED)).ToString(), 0);
                centerlineSeries.Points.AddXY(((int)(o.x[totalPoints - 1, 0] / g.HUNDRED)).ToString(), 0);
            }
        }
    }

    public static void UpdateAnnotation(Chart chart, string stockName)
    {
        // Find the annotation associated with the stockName
        foreach (var annotation in chart.Annotations)
        {
            if (annotation is TextAnnotation textAnnotation && textAnnotation.Name == stockName)
            {
                // Update the annotation text, color, and background color based on the latest data
                int index = wk.return_index_of_ogldata(stockName);

                if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
                {
                    return;
                }

                g.stock_data o = g.ogl_data[index];

                string annotationText = AnnotationGeneral(chart, o, o.x, 0, o.nrow - 1, o.nrow);

                if (chart.InvokeRequired)
                {
                    chart.Invoke(new Action(() =>
                    {
                        // Update the chart here
                        textAnnotation.Text = annotationText;
                    }));
                }
                else
                {
                    // Update the chart here
                    textAnnotation.Text = annotationText;
                }

                // Update the position of the annotation to remain at the top-left corner of the chart area
                textAnnotation.X = 0;
                textAnnotation.Y = 0;
                textAnnotation.Alignment = ContentAlignment.TopLeft;
                textAnnotation.AxisXName = $"{stockName}\\X";
                textAnnotation.AxisYName = $"{stockName}\\Y";

                // Adjust annotation styling based on thresholds
                int[] scoreThresholds = { 90, 70, 50, 30, 10 };
                Color[] colors = g.Colors; // Assuming g.Colors array holds your colors
                for (int i = 0; i < scoreThresholds.Length; i++)
                {
                    if (o.점수.총점 > scoreThresholds[i])
                    {
                        textAnnotation.BackColor = colors[i];
                        break;
                    }
                }
                break; // Exit loop once the annotation is updated
            }
        }
    }
    public static void UpdateChartSeriesGeneral(Chart chart, string stockName)
    {
        // Find the index of the stock data in ogl_data
        int index = wk.return_index_of_ogldata(stockName);
        if (index < 0)
        {
            return; // Exit if the stock is not found
        }

        // Get the data for the stock
        g.stock_data o = g.ogl_data[index];
        int totalPoints = o.nrow;

        // Update or add points for each series in "General" stock
        string[] seriesNames = { "1", "2", "3", "4", "5", "6" };

        // Get the number of data points in the "stockName 1" series
        string baseSeriesName = stockName + " 1";
        if (chart.Series.IndexOf(baseSeriesName) == -1)
            return; // Exit if the base series does not exist

        Series baseSeries = chart.Series[baseSeriesName];
        int seriesPoints = baseSeries.Points.Count;

        // Update or add points for "stockName 1" and other series in seriesNames
        foreach (string suffix in seriesNames)
        {
            string seriesName = stockName + " " + suffix;
            if (chart.Series.IndexOf(seriesName) == -1)
                continue; // Skip if series not found

            Series series = chart.Series[seriesName];
            series.Points[seriesPoints - 1].Label = string.Empty;

            if (seriesPoints < totalPoints)
            {
                // Add new points with updated X and Y values
                for (int i = seriesPoints; i < totalPoints; i++)
                {
                    string xValue = ((int)(o.x[i, 0] / g.HUNDRED)).ToString();
                    int yValue = GeneralValue(o, i, int.Parse(suffix));  //o.x[i, int.Parse(suffix)]; // Y-axis (value)

                    series.Points[i - 1].Label = string.Empty;
                    if (chart.InvokeRequired)
                    {
                        chart.Invoke(new Action(() =>
                        {
                            series.Points.AddXY(xValue, yValue);
                            if (i == totalPoints - 1)
                                LabelGeneral(chart, stockName, int.Parse(suffix), totalPoints, o.x, series);
                            MarkGeneral(chart, stockName, series.Points.Count - 1,
                                int.Parse(suffix), series.Points.Count - 1, o.x, series);
                        }));
                    }
                    else
                    {
                        series.Points.AddXY(xValue, yValue);
                        if (i == totalPoints - 1)
                            LabelGeneral(chart, stockName, int.Parse(suffix), totalPoints, o.x, series);
                        MarkGeneral(chart, stockName, series.Points.Count - 1,
                            int.Parse(suffix), series.Points.Count - 1, o.x, series);
                    }
                }
            }
            else if (seriesPoints == totalPoints && totalPoints > 0)
            {
                // Replace the last point's Y value and X-axis (to ensure time progresses)
                string xValue = ((int)(o.x[seriesPoints - 1, 0] / g.HUNDRED)).ToString();
                int yValue = GeneralValue(o, seriesPoints - 1, int.Parse(suffix)); // o.x[totalPoints - 1, int.Parse(suffix)];

                if (chart.InvokeRequired)
                {
                    chart.Invoke(new Action(() =>
                    {
                        series.Points[seriesPoints - 1].SetValueXY(xValue, yValue);
                        LabelGeneral(chart, stockName, int.Parse(suffix), totalPoints, o.x, series);
                        MarkGeneral(chart, stockName, seriesPoints - 1,
                            int.Parse(suffix), series.Points.Count - 1, o.x, series);

                    }));
                }
                else
                {
                    series.Points[seriesPoints - 1].SetValueXY(xValue, yValue);
                    LabelGeneral(chart, stockName, int.Parse(suffix), totalPoints, o.x, series);
                    MarkGeneral(chart, stockName, series.Points.Count - 1,
                        int.Parse(suffix), series.Points.Count - 1, o.x, series);

                }
            }
        }

        // Update or add points for "stockName 9" - the centerline
        string centerlineSeriesName = stockName + " 9";
        if (chart.Series.IndexOf(centerlineSeriesName) != -1)
        {
            Series centerlineSeries = chart.Series[centerlineSeriesName];

            if (centerlineSeries.Points.Count == 2)
            {
                // Replace the last point with the updated x value and y = 0
                centerlineSeries.Points[1].SetValueXY(((int)(o.x[totalPoints - 1, 0] / g.HUNDRED)).ToString(), 0);
            }
            else
            {
                // Clear and add two points if somehow not two points (should be consistent with two points)
                centerlineSeries.Points.Clear();
                centerlineSeries.Points.AddXY(((int)(o.x[0, 0] / g.HUNDRED)).ToString(), 0);
                centerlineSeries.Points.AddXY(((int)(o.x[totalPoints - 1, 0] / g.HUNDRED)).ToString(), 0);
            }
        }

        chart.ChartAreas[stockName].AxisX.LabelStyle.Enabled = true;
        chart.ChartAreas[stockName].AxisX.MajorGrid.Enabled = false;
        chart.ChartAreas[stockName].AxisX.Interval = totalPoints - 1; // total number of point = 2, while mir 0820 -> 217
        chart.ChartAreas[stockName].AxisX.IntervalOffset = 1;

    }
    public static void UpdateChartSeries(Chart chart, string stockName, int nRow, int nCol)
    {

        if (stockName.Contains("KODEX"))
        {
            UpdateChartSeriesKodex(chart, stockName);
        }
        else
        {
            UpdateChartSeriesGeneral(chart, stockName); // 
            UpdateAnnotation(chart, stockName);

        }

    }



    static void AddSeriesToChart(string sid, Chart chart, string area, Color color, int borderWidth)
    {
        Series series = new Series(sid)
        {
            ChartArea = area,
            ChartType = SeriesChartType.Line,
            XValueType = ChartValueType.Date,
            IsVisibleInLegend = false,
            Color = color,
            BorderWidth = borderWidth
        };
        chart.Series.Add(series);
    }

    static int GeneralValue(g.stock_data o, int k, int id)
    {
        int value = 0;
        switch (id)
        {
            case 1:
                value = o.x[k, 1];
                break;
            case 2:
                value = (int)(Math.Sqrt(o.x[k, 2]) * 10);
                if (value > 500)
                    value = 500;
                break;
            case 3:
                value = (int)(Math.Sqrt(o.x[k, 3] / (double)g.HUNDRED) * 10);
                if (value > 500)
                    value = 500;
                break;
            case 4:
            case 5:
            case 6:
                double multiplier = 1;
                if (o.x[o.nrow - 1, 7] > g.EPS) // 누적거래량 ! = 0
                {
                    multiplier = 100.0 / o.x[o.nrow - 1, 7] * g.v.수급과장배수 * o.수급과장배수; // marketeye 
                }
                value = (int)(o.x[k, id] * multiplier);
                break;
        }
        return value;
    }

    static void RelocateChart1AreaAndDataGridView(Chart chart, string stockName, int row, int col)
    {
        // Set position and size of the chart area based on row and column
        RelocateChart1Area(chart, stockName, row, col);

        // Also set the position of the form next to it
        Form form = fm.FindFormByName("se");
        DataGridView dgv = fm.FindDataGridViewByName(form, stockName);
        if (dgv != null)
        {
            RelocateChart1DataGridView(dgv, row, col + 1); // Form is placed in the column next to the chart area
        }
    }

    private static void RelocateChart1Area(Chart chart, string stockName, int row, int col)
    {
        // Find the ChartArea with the given stockName
        ChartArea chartArea = chart.ChartAreas.FirstOrDefault(ca => ca.Name == stockName);

        // If ChartArea does not exist, return
        if (chartArea == null)
        {
            return;
        }

        // Define grid-based positioning
        int totalRows = g.nRow; // Total rows available
        int totalCols = g.nCol; // Total columns available
        float cellWidth = 100f / totalCols; // Width of each grid cell in %
        float cellHeight = 100f / totalRows; ; // Height of each grid cell in %
        //float yPosition = row * cellHeight + 5.221F + 2.0F;
        // Set ChartArea's position
        chartArea.Position = new ElementPosition(
            col * cellWidth,    // X position 20
            row * cellHeight + 5.155F,   // Y position
            cellWidth,          // Width
            28.175F //cellHeight       // Height
        );

        // Set InnerPlotPosition for plot area
        chartArea.InnerPlotPosition = new ElementPosition(
            5, // Leave a 10% margin on the left
                        10, // Leave a 10% margin at the top
                        75, // Use 80% of the ChartArea's width for the plot
                        80  // Use 80% of the ChartArea's height for the plot
        );

        // Adjust corresponding Annotation's position (independently)
        Annotation annotation = chart.Annotations.FirstOrDefault(a => a.Name == stockName);
        if (annotation is RectangleAnnotation rectangleAnnotation)
        {
            rectangleAnnotation.X = chartArea.Position.X; // Match the ChartArea's X
            rectangleAnnotation.Y = row * cellHeight; //chartArea.Position.Y; // Place annotation slightly above ChartArea
            rectangleAnnotation.Width = chartArea.Position.Width; // Match ChartArea's width
            rectangleAnnotation.Height = 5.155 + 2; // Annotation height (adjust as needed)
        }
    }

    static void RelocateChart1DataGridView(DataGridView dgv, int row, int col)
    {
        dgv.Left = col * (g.screenWidth / g.nCol) + 10; // Position based on screen width
        dgv.Top = row * (g.screenHeight / g.nRow) - 4 * row; // Position based on screen height

    }

    static int HandleUSIndex(g.stock_data o, int k, int end_time, int start_time, int index)
    {
        if (o.x[k, index] == 0 && k != 0)
        {
            int upperNonZero = FindNonZeroValue(o, k + 1, end_time, index, true);
            int lowerNonZero = FindNonZeroValue(o, k - 1, start_time, index, false);
            return (upperNonZero + lowerNonZero) / 2;
        }
        return o.x[k, index];
    }

    // Function to find the closest non-zero value in the specified direction
    static int FindNonZeroValue(g.stock_data o, int start, int end, int index, bool forward)
    {
        if (forward)
        {
            for (int j = start; j < end; j++)
            {
                if (o.x[j, index] != 0) return o.x[j, index];
            }
        }
        else
        {
            for (int j = start; j >= 0; j--)
            {
                if (o.x[j, index] != 0) return o.x[j, index];
            }
        }
        return 0;
    }

    public static bool ChartAreaExists(Chart chart, string stockName)
    {
        //foreach (var ca in chart.ChartAreas)
        //{
        //    Console.WriteLine($"ChartArea Name: {ca.Name}"); // Debug statement
        //}
        return chart.ChartAreas.Any(ca => ca.Name.Trim().Equals(stockName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    static int KodexMagnifier(g.stock_data o, int id, ref double magnifier)
    {
        int i = 0;
        int j = 0;

        switch (o.stock)
        {
            case "KODEX 레버리지":
                i = 0;
                break;
            case "KODEX 200선물인버스2X":
                i = 1;
                break;
            case "KODEX 코스닥150레버리지":
                i = 2;
                break;
            case "KODEX 코스닥150선물인버스":
                i = 3;
                break;
        }

        switch (id)
        {
            case 1: // price
                j = 0;
                break;
            case 3: // Program
                j = 1;
                break;
            case 4: // money
            case 5:
            case 6:
            case 11:
                j = 2;
                break;
            case 10: // US
                j = 3;
                break;

            default:
                return -1; // input mitake
        }

        magnifier = g.kodex_magnifier[i, j];

        return 0;
    }

    static int GetMagnifierIndex(int index)
    {
        switch (index)
        {
            case 1:
                return 0;
            case 3:
                return 1;
            case 4:
            case 5:
            case 6:
                return 2;
            case 10:
                return 3;
            case 11:
                return 2;
            default:
                return -1;
        }
    }

    // Helper function to get color based on index
    static Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 1: return colorKODEX[1]; // Price
            case 4: return colorKODEX[4]; // Program
            case 5: return colorKODEX[5]; // Foreign
            case 6: return colorKODEX[6]; // Institute
            case 10: return colorKODEX[10]; // Nasdaq
            case 11: return colorKODEX[11]; // Pension
            default: return colorKODEX[3];
        }
    }

    // Helper function to get border width based on index
    static int GetBorderWidthByIndex(int index)
    {
        switch (index)
        {
            case 1: return 2;    // Price
            case 4: return 2;    // Program
            case 5: return 2;    // Foreign
            case 6: return 2;    // Institute
            default: return 1;
        }
    }

    public void BatchUpdateChart(Chart chart, List<(string stockName, int xValue, int yValue)> updates)
    {
        chart.SuspendLayout(); // Suspend layout updates for better performance

        foreach (var (stockName, xValue, yValue) in updates)
        {
            // UpdateChartData(chart, stockName, xValue, yValue);
        }

        chart.ResumeLayout(); // Resume layout updates
        chart.Invalidate();   // Redraw chart
    }

 
        public static void MinuteAdvanceRetreat(int AdvanceLines)
    {
        if (AdvanceLines == 0)
        {
            g.time[1] = g.end_time_before_advance;
            g.end_time_before_advance = 0;
            g.end_time_extended = false;
        }
        else
        {
            g.end_time_before_advance = g.time[1];
            g.time[1] += AdvanceLines; // expedient
            if (g.time[1] > g.MAX_ROW)
                g.time[1] = g.MAX_ROW;

            g.end_time_extended = true;
        }
    }

}

