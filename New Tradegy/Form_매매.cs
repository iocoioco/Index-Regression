using New_Tradegy.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy
{
    public partial class Form_매매 : Form
    {
        int Rows = 11;
        int Columns = 4;
        int fontSize = 8;
        public Form_매매()
        {
            InitializeComponent();
        }

        private void Form_매매_Load(object sender, EventArgs e)
        {
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
            this.FormBorderStyle = FormBorderStyle.None;//윈도우테두리제거방법
            this.Size = new Size(g.screenWidth / g.rqwey_nCol - scrollbarWidth, g.formSize.ch * 9 + 14);
            this.Location = new Point(g.screenWidth / g.rqwey_nCol + 20, g.formSize.ch * 15); // + 30 deleted from width


            g.매매.dgv = new DataGridView();
            g.매매.dgv.DataError += (s, f) => wr.DataGridView_DataError(s, f, "매매 dgv");
            g.매매.dgv.Location = new Point(0, 0);
            g.매매.dgv.Size = this.Size;

            g.매매.dgv.DataSource = g.제어.dtb;
            g.매매.dgv.ColumnHeadersVisible = false;
            g.매매.dgv.RowHeadersVisible = false;


            g.매매.dgv.ReadOnly = true;
            g.매매.dgv.DefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            g.매매.dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);
            g.매매.dgv.RowTemplate.Height = g.formSize.ch - 1;
            g.매매.dgv.ForeColor = Color.Black;
            g.매매.dgv.ScrollBars = ScrollBars.None;
            g.매매.dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.매매.dgv.AllowUserToResizeColumns = false;
            g.매매.dgv.AllowUserToResizeRows = false;

            g.매매.dgv.AllowUserToAddRows = false;
            g.매매.dgv.AllowUserToDeleteRows = false;
            g.매매.dgv.Dock = System.Windows.Forms.DockStyle.Fill;

            g.매매.dgv.RowHeadersVisible = false;

            g.매매.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            g.매매.dgv.TabIndex = 1;
            g.매매.dgv.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(매매_CellFormatting);

            g.매매.dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(매매_CellMouseClick);
            this.TopMost = true;

            this.KeyPreview = true; // Ensure the form captures key events
            this.KeyDown += new KeyEventHandler(Form_매매_KeyDown);



            // work
            g.매매.dtb = new DataTable();
            g.매매.dtb.Columns.Add("0");
            g.매매.dtb.Columns.Add("1");
            g.매매.dtb.Columns.Add("2");
            g.매매.dtb.Columns.Add("3");

            for (int j = 0; j < Rows; j++)
            {
                g.매매.dtb.Rows.Add("", "", "", "");
            }
            g.매매.dgv.DataSource = g.매매.dtb;
            this.Controls.Add(g.매매.dgv);

            g.매매.dgv.Visible = true;



            // stock, 매수/매도, 가격, 거래진행(실행/잔량)
            g.매매.dgv.Columns[0].Width = (int)(g.매매.dgv.Width * 0.20); // 0.25
            g.매매.dgv.Columns[1].Width = (int)(g.매매.dgv.Width * 0.20);    // 0.2
            g.매매.dgv.Columns[2].Width = (int)(g.매매.dgv.Width * 0.25);    // 0.2
            g.매매.dgv.Columns[3].Width = (int)(g.매매.dgv.Width * 0.35);   // 0.35
        }

        private void Form_매매_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true; // Indicate that the key has been handled
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }



        //private void 매매_KeyDown(object sender, KeyEventArgs e) // dgv_CellClick tr(4)
        //{
        //    // if key is ESC, then close the form   
        //    if (e.KeyCode == Keys.Escape)
        //    {
        //        this.Close();
        //    }
        //}

        public static void 매매_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) // dgv_CellClick tr(4)
        {
            if (e.Button == MouseButtons.Right) // doing nothing and return;
                return;

            string stock = g.매매.dgv.Rows[e.RowIndex].Cells[0].Value.ToString();

            switch (e.ColumnIndex)
            {
                case 1: // 체결종목 취소, 취소가 안 되는 이유는 뭘까 //?
                    if (g.test)
                        return;

                    if (e.RowIndex < g.m_mapOrder.Count)
                    {
                        dl.DealCancelRowIndex(e.RowIndex); // work_CellMouseClick
                        ms.Sound("Keys", "cancel");
                    }
                    break;

                case 2: // 매도 Blocked
                    if (!g.test)
                    {
                        string buySell = "매도";
                        int 거래가격 = hg.HogaGetValue(stock, 0, 1); // 0 : 매수1호가 라인, 1 : column

                        int Urgency = 100;
                        if (g.optimumTrading)
                        {
                            Urgency = (int)(e.X / (double)g.매매.dgv.Columns[2].Width * 100);
                        }

                        // dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;

                case 3: // 매수 Blocked 
                    if (!g.test)
                    {
                        string buySell = "매수";
                        int 거래가격 = hg.HogaGetValue(stock, -1, 1); // 0 : 매수1호가 라인, 1 : column

                        int Urgency = 100;
                        if (g.optimumTrading)
                        {
                            Urgency = (int)(e.X / (double)g.매매.dgv.Columns[2].Width * 100);
                        }

                        // dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;
            }
        }

        private void 매매_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }
        private void 매매_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            g.매매.dgv.Rows[0].DefaultCellStyle.Font = new Font("Arial", fontSize, FontStyle.Bold);

            if (e.RowIndex == 0)
            {

            }
            if (e.RowIndex == 1)
            {

            }
        }

    }
}
