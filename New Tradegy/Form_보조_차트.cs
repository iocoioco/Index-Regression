
using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace New_Tradegy
{
    public partial class Form_보조_차트 : Form
    {
        private Point mousePosition;

        public int nRow;
        public int nCol;

        public string keyString = "그순";
        private int Cell_Height = 30;
        // 상관, 보유, 그순, 관심, 코닥, 코피, 푀손

        public static List<string> displayList = new List<string>();

        DataTable dtb;// = new DataTable();







        private static Form_보조_차트 _instance;

        public static Form_보조_차트 Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new Form_보조_차트();
                }
                return _instance;
            }
        }









        public Form_보조_차트()
        {
            InitializeComponent();
        }







        public void InvalidateChart2()
        {
            chart2.Invalidate();
        }










        private void Form_보조_차트_Load(object sender, EventArgs e)
        {
            g.chart2 = chart2;

            dataGridView1.DataError += (s, f) => wr.DataGridView_DataError(s, f, "보조차트 dgv");
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold); // Roman
            dataGridView1.RowTemplate.Height = Cell_Height;
            dataGridView1.ForeColor = Color.Black;
            dataGridView1.ScrollBars = ScrollBars.None;

            this.Text = keyString;

            // when small monitor
            //this.Size = new System.Drawing.Size(559, 925); // 559 vs 553
            //this.Location = new System.Drawing.Point(-553, 72); // 72
            //chart2.Size = new Size(this.Width, 925 - Cell_Height);
            //chart2.Location = new Point(0, 0);


            if (g.MachineName == "HP")
            {
                this.Size = new Size(964, 1039);
                this.Location = new Point(-1920 / 2, 0);
                chart2.Size = new Size(this.Width, this.Height - Cell_Height);
                chart2.Location = new Point(0, 0);
            }
            else
            {
                this.Size = new Size(964, 1039);
                this.Location = new Point(1910, 0);
                chart2.Size = new Size(1920 / 2, 995);
                chart2.Location = new Point(0, 0);
            }


            dataGridView1.Size = new Size(this.Width, Cell_Height);
            dataGridView1.Location = new System.Drawing.Point(0, 0);

            dtb = new DataTable();
            dtb.Columns.Add("0");
            dtb.Columns.Add("1");
            dtb.Columns.Add("2");
            dtb.Columns.Add("3");
            dtb.Columns.Add("4");
            dtb.Columns.Add("5");
            dtb.Columns.Add("6");
            dtb.Columns.Add("7");

            dtb.Rows.Add("", "", "", "", "", "", "", "");

            dtb.Rows[0][0] = "상관"; //ok
            dtb.Rows[0][1] = "보유"; //ok
            dtb.Rows[0][2] = "그순";
            dtb.Rows[0][3] = "관심"; //ok
            dtb.Rows[0][4] = "코닥"; //ok
            dtb.Rows[0][5] = "코피"; //ok
            dtb.Rows[0][6] = "절친"; //ok
            dtb.Rows[0][7] = "푀손"; //ok

            dataGridView1.DataSource = dtb;
            for (int i = 0; i < 8; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[i].Width = this.Width / 8;
            }

            Form_보조_차트_DRAW();

            //Task task_푀손 = new Task(THREAD_Form_보조_차트);
            //task_푀손.Start();



            // Mouse with Moving String
            //chart2.MouseMove += Chart_MouseMove;
            //chart2.Paint += Chart_Paint;

        }







        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            ChartState.ActiveChart = "chart2";
            ChartState.Chart2MousePosition = new Point(e.X + 100, e.Y + 10);
            ChartState.Chart1MousePosition = null;

            chart2.Invalidate(); // Trigger the Paint event to redraw chart2
            se.Instance?.InvalidateChart1();
        }
        // NR
        private void Chart_Paint(object sender, PaintEventArgs e)
        {
            if (ChartState.ActiveChart == "chart2")
            {
                ChartHelper.DrawNumbersAtMousePosition(e.Graphics, ChartState.Chart2MousePosition);
            }

            //if (ChartState.ActiveChart == "chart2" && ChartState.Chart2MousePosition.HasValue)
            //{
            //    // Draw the numbers at the mouse position
            //    using (Font font = new Font("Arial", 10, FontStyle.Bold))
            //    using (Brush brush = new SolidBrush(Color.Black))
            //    {
            //        e.Graphics.DrawString("123\n456", font, brush, ChartState.Chart2MousePosition.Value);
            //    }
            //}
        }







        // NF
        public void THREAD_Form_보조_차트()
        {
            //string title ="";
            //while (true)
            //{
            //    if(keyString == "푀손")
            //    {
            //        wk.보조_차트_StocksGivenKeyString(keyString, ref displayList, ref title);

            //        for (int seq = 0; seq < displayList.Count; seq++)
            //        {
            //            if (seq < nRow * nCol)
            //                dr.draw_stock(chart2, nRow, nCol, seq, displayList[seq]);
            //        }
            //    }
            //    Thread.Sleep(1000 * 10);
            //}
        }

        public void Form_보조_차트_DRAW() // duration 0.08 ~ 0.12 seconds
        {

            if (g.add_interest)
                return;

            if (keyString != "상관")
            {
                if (!g.end_time_extended)
                {
                    displayList.Clear();
                    wk.보조_차트_StocksGivenKeyString(keyString, displayList, g.clickedStock, g.clickedTitle);
                }
            }
            else
            {
                //if (g.clickedTitle != g.saved_clickedTitle)
                //{
                    displayList.Clear();
                    wk.보조_차트_StocksGivenKeyString(keyString, displayList, g.clickedStock, g.clickedTitle);
                //    g.saved_clickedTitle = g.clickedTitle;
                //}
            }

            if (keyString == "상관")
                this.Text = keyString + "(" + g.clickedTitle + ")";
            else if (keyString == "절친")
                this.Text = keyString + "(" + g.clickedStock + ")";
            else if (keyString == "그순")
                this.Text = keyString + "(" + g.oGl_data_selection + ")";
            else
                this.Text = keyString;

            if (displayList.Count <= 2)
            {
                nCol = 2;
                nRow = 1;
            }
            else if (displayList.Count <= 4)
            {
                nCol = 2;
                nRow = 2;
            }
            else if (displayList.Count <= 6)
            {
                nCol = 3;
                nRow = 2;
            }
            else if (displayList.Count <= 9)
            {
                nCol = 3;
                nRow = 3;
            }
            else if (displayList.Count <= 12)
            {
                nCol = 4;
                nRow = 3;
            }
            else
            {
                nCol = 5;
                nRow = 3;
            }

            ChartClear();
            for (int seq = 0; seq < displayList.Count; seq++)
            {
                if (seq < nRow * nCol)
                    dr.draw_stock(chart2, nRow, nCol, seq, displayList[seq]);
            }
            dataGridView1.Refresh();
            //this.BringToFront();
        }


        public void Form_보조_차트_DRAW(string stock)
        {
            if (displayList.Count == nRow * nCol)
            {
                ChartClear();
                displayList.Clear();
            }

            if (displayList.Contains(stock)) { return; }

            nCol = 4;
            nRow = 3;

           
            displayList.Add(stock);

            dr.draw_stock(chart2, nRow, nCol, displayList.Count - 1, stock);
        }

        public static void ChartClear()
        {
            g.chart2.Series.Clear();
            g.chart2.ChartAreas.Clear();
            g.chart2.Annotations.Clear();
            
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


            SetFocusAndReturn();
        }

        private void Form_보조_차트_ResizeEnd(object sender, EventArgs e)
        {
            chart2.Size = new Size((int)(this.Width), (int)(this.Height - Cell_Height));
            chart2.Location = new Point(0, 0);




            dataGridView1.Size = new Size(this.Width, Cell_Height);
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

        private void chart2_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
            SetFocusAndReturn();
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
            SetFocusAndReturn();
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



        

        // Import the necessary functions from user32.dll
        [DllImport("user32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);


        private void SetFocusAndReturn()
        {
            IntPtr handle = this.Handle;

            // Delay for a short time to allow the browser to open
            System.Threading.Thread.Sleep(100);

            // Check if the form is minimized
            if (IsIconic(handle))
            {
                // Restore the window if it's minimized
                ShowWindow(handle, SW_RESTORE);
            }

            // Set the focus back to the form
            SetForegroundWindow(handle);
            SetFocus(handle);
            // Keep focus on the TextBox
            chart2.Focus();
        }
        // Additional necessary API calls
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;
    }
}




