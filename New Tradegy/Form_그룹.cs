using New_Tradegy.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy
{
    public partial class Form_그룹 : Form
    {
        public Form_그룹()
        {
            InitializeComponent();
          
            // if(g.connected)
                this.FormClosed += new FormClosedEventHandler(Form_그룹_FormClosed);
        }

        private void Form_그룹_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Unsubscribe from events
            g.그룹.dgv.DataError -= (s, f) => wr.DataGridView_DataError(s, f, "그룹 dgv");
            g.그룹.dgv.CellFormatting -= 그룹_CellFormatting;
            g.그룹.dgv.CellMouseClick -= 그룹_CellMouseClick;
            g.그룹.dgv.KeyPress -= 그룹_KeyPress;

            // Dispose of the DataGridView
            g.그룹.dgv.Dispose();

            // Dispose of the form
            this.Dispose();
        }
        private void Form_그룹_Load(object sender, EventArgs e)
        {
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
           
            //this.FormBorderStyle = FormBorderStyle.None;//윈도우테두리제거방법
            this.Size = new Size(g.screenWidth / g.rqwey_nCol, g.formSize.ch * 10 + 12);
            this.Location = new Point(g.screenWidth / g.rqwey_nCol * (g.rqwey_nCol - 1), g.screenHeight / g.rqwey_nRow * 2 + 37);

            this.Name = "Form_그룹";

            g.그룹.dtb = new DataTable();
            g.그룹.dgv = new DataGridView();
            g.그룹.dgv.DataError += (s, f) => wr.DataGridView_DataError(s, f, "그룹 dgv");
            g.그룹.dgv.DataSource = g.그룹.dtb;
            this.Controls.Add(g.그룹.dgv);

            g.그룹.dgv.Visible = true;
            g.그룹.dgv.Location = new Point(0, 0);
            g.그룹.dgv.Size = this.Size;
            g.그룹.dgv.ColumnHeadersVisible = false;
            g.그룹.dgv.RowHeadersVisible = false;
            
            int fontsize = 10;

            g.그룹.dgv.ReadOnly = true;
            g.그룹.dgv.DefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            g.그룹.dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            g.그룹.dgv.RowTemplate.Height = g.formSize.ch;
            g.그룹.dgv.ForeColor = Color.Black;

            if(!g.test)
            {
                g.그룹.dgv.ScrollBars = ScrollBars.None;
                
            }
            else
            {
                g.그룹.dgv.ScrollBars = ScrollBars.Vertical;
            }
            
            g.그룹.dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.그룹.dgv.AllowUserToAddRows = false;
            g.그룹.dgv.AllowUserToDeleteRows = false;
            g.그룹.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            g.그룹.dgv.RowHeadersVisible = false;
            g.그룹.dgv.AllowUserToResizeColumns = false;
            g.그룹.dgv.AllowUserToResizeRows = false;

            //g.그룹.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            g.그룹.dgv.TabIndex = 1;
            g.그룹.dgv.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(그룹_CellFormatting);
            g.그룹.dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(그룹_CellMouseClick);
            //g.그룹.dgv.Scroll += new ScrollEventHandler(grup_VerticalScroller_Scroll);
            //g.그룹.dgv.ScrollBars = ScrollBars.Vertical;
            //g.그룹.dgv.Controls.Add(g.그룹.dgv.Scroll);
            this.TopMost = true;

            g.그룹.dgv.KeyPress += 그룹_KeyPress;



            // grup
            g.그룹.dtb.Columns.Add("0");
            g.그룹.dtb.Columns.Add("1");
            g.그룹.dtb.Columns.Add("2");
           
            int rows = g.oGL_data.Count;
            if (!g.test)
            {
                rows = 9;
            }
            
            for (int j = 0; j < rows; j++)
            {
                g.그룹.dtb.Rows.Add("", "", "");
            }

            for (int i = 0; i < 3; i++)
            {
                if(g.connected)
                {
                    g.그룹.dgv.Columns[i].Width = this.Width / 3 - 6;
                }
                else
                {
                    g.그룹.dgv.Columns[i].Width = this.Width / 3 - 10;
                }
            }
        }

        //private void grup_VerticalScroller_Scroll(object sender, ScrollEventArgs e)
        //{
        //    g.그룹.dgv.FirstDisplayedScrollingRowIndex = e.NewValue;
        //}
        public static void 그룹_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) // dgv_CellClick tr(4)
        {
            g.clickedTitle = g.그룹.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            bool found = false;
            foreach(var item in g.oGL_data)
            {
                if(item.title == g.clickedTitle)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                if (e.Button == MouseButtons.Left)
                {
                    int index = g.oGL_data.FindIndex(x => x.title == g.clickedTitle);
                    if (index < 0) return;

                    g.clickedStock = g.oGL_data[index].stocks[0];

          
                    mm.ManageChart2("상관"); // 그룹_CellMouseClick

                }

                else
                {
                    string t = "http://google.com/search?q=" + g.clickedTitle
                        + " 주식" + " 뉴스 " + "&tbs=qdr:d"; // qdr:w, m, d, h
                    Process.Start(t);
                    return;
                }
            }
        }


        private void 그룹_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }


        private void 그룹_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int coloring_column = 0;
            switch (g.oGl_data_selection)
            {
                case "총점":
                    coloring_column = 0;
                    break;
                case "푀분":
                    coloring_column = 1;
                    break;
                case "가증":
                    coloring_column = 2;
                    break;
            }
            for (int i = 0; i < 3; i++)
            {
                if (i == coloring_column)
                {
                    g.그룹.dgv.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255); // cyan
                }
                else
                {
                    g.그룹.dgv.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                }

            }

            if (e.RowIndex == 0)
            {
                if (e.ColumnIndex == 0)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // red
                if (e.ColumnIndex == 1)
                    e.CellStyle.BackColor = Color.FromArgb(255, 255, 255); // red
                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // red
                if (e.ColumnIndex == 3)
                    e.CellStyle.BackColor = Color.FromArgb(255, 255, 255); // red
            }
        }
    }
}
