using New_Tradegy.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;

namespace New_Tradegy
{
    // by Chat Gpt 20250315
    public partial class Form_제어 : Form 
    {
        // Dictionary for DataGridView controls and their settings.
        private static readonly Dictionary<string, (int[], Action<int>)> controlConfigurations =
     new Dictionary<string, (int[], Action<int>)>()

{
    { "종거", (new int[] { 0, 50, 100, 200, 500, 1000, 1500, 2000 }, val => g.v.종가기준추정거래액이상_천만원 = val * 10) },
    { "분거", (new int[] { 0, 2, 5, 10, 20, 30, 50 }, val => g.v.분당거래액이상_천만원 = val) },
    { "호가", (new int[] { 0, 5, 10, 20, 30, 50 }, val => g.v.호가거래액_백만원 = val) },
    { "편차", (new int[] { 0, 1, 2, 3, 5, 7 }, val => g.v.편차이상 = val) },
    { "배차", (new int[] { 0, 10, 25, 50, 75, 100 }, val => g.v.배차이상 = val) },
    { "시총", (new int[] { 0, 10, 30, 50, 100, 200 }, val => g.v.시총이상 = val) },
    { "수과", (new int[] { 5, 10, 15, 20, 25, 30, 40, 50, 60, 80, 100 }, val => g.v.수급과장배수 = val) },
    //{ "배과", (new int[] { 1, 2, 3, 4, 5, 7, 10 }, val => g.v.배수과장배수 = 1.0 / val) },
    { "Eval", (new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 }, val => g.MarketeyeDividerForEvalStock = val) },
    { "Post", (new int[] { 10, 15, 20, 25, 30, 40, 50, 60, 70 }, val => g.postInterval = val) },
    { "Font", (new int[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, val => g.v.font = val / 2.0F) },
    { "푀플", (new int[] { 0, 1 }, val => g.v.푀플 = val)  },
    { "배플", (new int[] { 0, 1 }, val => g.v.배플 = val)  },
    //{ "보유", (new int[] { 10, 50, 100, 250, 500, 1000 }, val => g.v.보유종목점검최소액 = val) },
    { "선폭", (new int[] { 1, 2, 3 }, val => g.LineWidth = val) }
};


        //DataGridView g.제어.dgv;
        int Rows = 15;
        int Columns = 4;

        public Form_제어()
        {
            InitializeComponent();
        }

        private void Form_제어_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Size = new Size(g.screenWidth / g.nCol - 20, g.DgvCellHeight * 3);
            int a = g.screenHeight;

            this.Location = new Point(g.screenWidth / g.nCol + 10, g.screenHeight / 3 + 2); // + 30 remove from width



            g.제어.dtb = new DataTable();
            g.제어.dgv = new DataGridView();
            g.제어.dgv.DataError += (s, f) => wr.DataGridView_DataError(s, f, "제어 dgv");
            g.제어.dgv.DataSource = g.제어.dtb;
            this.Controls.Add(g.제어.dgv);

            g.제어.dgv.Visible = true;

            g.제어.dgv.Location = new Point(0, 0);
            g.제어.dgv.Size = this.Size;

            g.제어.dgv.ColumnHeadersVisible = false;
            g.제어.dgv.RowHeadersVisible = false;
            int fontsize = 8;

            g.제어.dgv.ReadOnly = true;
            g.제어.dgv.DefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            g.제어.dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            g.제어.dgv.RowTemplate.Height = g.DgvCellHeight;
            g.제어.dgv.ForeColor = Color.Black;
            g.제어.dgv.ScrollBars = ScrollBars.Vertical;
            g.제어.dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.제어.dgv.AllowUserToResizeColumns = false;
            g.제어.dgv.AllowUserToResizeRows = false;

            g.제어.dgv.AllowUserToAddRows = false;
            g.제어.dgv.AllowUserToDeleteRows = false;
            g.제어.dgv.Dock = System.Windows.Forms.DockStyle.Fill;

            g.제어.dgv.RowHeadersVisible = false;

            g.제어.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            g.제어.dgv.TabIndex = 1;
            g.제어.dgv.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(제어_CellFormatting);
            g.제어.dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(제어_CellMouseClick);
            this.TopMost = true;

            g.제어.dgv.KeyPress += 제어_KeyPress;


            // 제어 setting
            g.제어.dtb.Columns.Add("0");
            g.제어.dtb.Columns.Add("1");
            g.제어.dtb.Columns.Add("2");
            g.제어.dtb.Columns.Add("3");


            for (int i = 0; i < Rows; i++)
            {
                g.제어.dtb.Rows.Add("", "", "", "");
            }

            // Row 0
            int month = g.date % 10000 / 100;
            int day = g.date % 10000 % 100;
            g.일회거래액 = 0;
            g.제어.dtb.Rows[0][0] = month.ToString() + "/" + day.ToString();
            //g.제어.dtb.Rows[0][1] = g.v.KeyString;
            g.제어.dtb.Rows[0][2] = g.일회거래액;
            g.제어.dtb.Rows[0][3] = g.예치금;

            //if (g.connected)
            //{
            //    dl.UpdateDealProfit();
            //}

            // Row 1
            g.제어.dtb.Rows[1][0] = 0; // initial setting of UpdateDealProfit;
            g.제어.dtb.Rows[1][1] = ""; // (int)usd_krw; 
            g.제어.dtb.Rows[1][2] = MajorIndex.Instance.NasdaqIndex;
            g.제어.dtb.Rows[1][3] = MajorIndex.Instance.Snp500Index;

            // Row 2

            g.제어.dtb.Rows[2][0] = ""; //상해
            g.제어.dtb.Rows[2][1] = ""; // 홍콩
            g.제어.dtb.Rows[2][2] = MajorIndex.Instance.NikkeiIndex;
            g.제어.dtb.Rows[2][2] = ""; // 대만가권







            // Row 4
            g.제어.dtb.Rows[4][0] = "adv";
            g.제어.dtb.Rows[4][1] = g.v.q_advance_lines;


            // Row 5
            g.제어.dtb.Rows[5][0] = "종거";
            g.제어.dtb.Rows[5][1] = 100; g.v.종가기준추정거래액이상_천만원 = 100;
            g.제어.dtb.Rows[5][2] = "분거";
            g.제어.dtb.Rows[5][3] = 10; g.v.분당거래액이상_천만원 = 10;


            // Row 6
            g.제어.dtb.Rows[6][0] = "호가";
            g.제어.dtb.Rows[6][1] = 10; g.v.호가거래액_백만원 = 10; // not active for g.tesing
            g.제어.dtb.Rows[6][2] = "편차";
            g.제어.dtb.Rows[6][3] = 1; g.v.편차이상 = 1;

            // Row 7
            g.제어.dtb.Rows[7][0] = "배차";
            g.제어.dtb.Rows[7][1] = 0; g.v.배차이상 = 0; // defined, but not used
            g.제어.dtb.Rows[7][2] = "시총";
            g.제어.dtb.Rows[7][3] = 0; g.v.시총이상 = 0;

            // Row 8
            g.제어.dtb.Rows[8][0] = "수과";
            g.제어.dtb.Rows[8][1] = 50; g.v.수급과장배수 = 50;
            //g.제어.dtb.Rows[8][2] = "배과";
            //g.제어.dtb.Rows[8][3] = 1; g.v.배수과장배수 = 1;

            // Row 9
            g.제어.dtb.Rows[9][0] = "Eval";
            g.제어.dtb.Rows[9][1] = 10; g.MarketeyeDividerForEvalStock = 10;
            g.제어.dtb.Rows[9][2] = "Post";
            g.제어.dtb.Rows[9][3] = 30; g.postInterval = 30;

            // Row 10
            g.제어.dtb.Rows[10][0] = "Font";
            g.제어.dtb.Rows[10][1] = g.v.font = 17; g.v.font /= 2.0F;
            //g.제어.dtb.Rows[10][2] = "Form";
            //g.제어.dtb.Rows[10][3] = g.v.font = 15; g.v.font /= 2.0F;

            // Row 11
            //g.제어.dtb.Rows[11][0] = "mms";
            //g.제어.dtb.Rows[11][1] = 1000; g.marketeye_sleep_seconds = 1000;

            //g.제어.dtb.Rows[11][2] = "보유";
            //g.제어.dtb.Rows[11][3] = 10; g.v.보유종목점검최소액 = 10;

            // Row 12
            g.제어.dtb.Rows[12][0] = "푀플";
            g.제어.dtb.Rows[12][1] = "1"; g.v.푀플 = 1;
            g.제어.dtb.Rows[12][2] = "배플";
            g.제어.dtb.Rows[12][3] = "1"; g.v.배플 = 1;

            // Row 13
            g.제어.dtb.Rows[13][0] = "선폭";
            g.제어.dtb.Rows[13][1] = 2; g.LineWidth = 2;


            for (int i = 0; i < Columns; i++)
            {
                int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
                g.제어.dgv.Columns[i].Width = (g.screenWidth / (11) - scrollbarWidth) / 4;
            }
        }

        private void 제어_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 5)
            {
                switch (e.RowIndex)
                {
                    case 0:
                        switch (e.ColumnIndex)
                        {
                            case 3:
                                //dl.UpdateAvailableDeposit();
                                break;
                        }
                        break;

                    case 1:
                        switch (e.ColumnIndex)
                        {
                            case 0:
                                DealManager.UpdateDealProfit();
                                break;
                            case 1:
                                Process.Start("chrome.exe", "https://kr.investing.com/currencies/usd-krw");
                                break;
                            case 2:
                                Process.Start("https://www.investing.com/indices/nq-100-futures?cid=1175151");
                                break;
                            case 3:
                                Process.Start("chrome.exe", "https://finviz.com/map.ashx?t=sec");
                                break;
                        }
                        break;

                    case 2:
                        Process.Start("chrome.exe", "https://www.investing.com/indices/major-indices");
                        break;

                    case 3:
                        switch (e.ColumnIndex)
                        {
                            case 0:
                            case 1:
                                Process.Start("chrome.exe", "https://www.investing.com/crypto/bitcoin/chart");
                                break;
                        }
                        break;

                    case 4:
                        if (e.ColumnIndex == 0)
                        {
                            g.v.q_advance_lines -= 5;
                        }
                        if (e.ColumnIndex == 1)
                        {
                            g.v.q_advance_lines += 5;
                        }
                        g.제어.dtb.Rows[4][1] = g.v.q_advance_lines;
                        break;
                }
                return;
            }
            else
            {
                string clickedVariable = "";
                int clickedValue = 0;
                bool upper = true;
                if (e.ColumnIndex % 2 == 0)
                {
                    clickedVariable = g.제어.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    clickedValue = Convert.ToInt32(g.제어.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value.ToString());
                    upper = false;
                }
                else
                {
                    clickedVariable = g.제어.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value.ToString();
                    clickedValue = Convert.ToInt32(g.제어.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                    upper = true;
                }
                if (clickedValue < 0) // Empty Click
                {
                    return;
                }

                var (array, updateAction) = controlConfigurations[clickedVariable];

                int newValue = FindNewValueFromArray(array, clickedValue, upper);
                if (newValue < 0 || !controlConfigurations.ContainsKey(clickedVariable)) return;

                updateAction(newValue);
                g.제어.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex % 2 == 0 ? e.ColumnIndex + 1 : e.ColumnIndex].Value = newValue;

                RefreshCharts();

            }

            g.제어.dgv.Refresh();

        }

        private void RefreshCharts()
        {
            g.ChartManager.ClearAll();

            ev.eval_stock();
            mm.ManageChart1();
            mm.ManageChart2();
        }

        private int FindNewValueFromArray(int[] array, int clickedValue, bool upper)
        {
            int index = Array.IndexOf(array, clickedValue);
            if (index == -1) return -1; // Not found

            return upper
                ? (index < array.Length - 1 ? array[index + 1] : clickedValue)
                : (index > 0 ? array[index - 1] : clickedValue);
        }


        private void 제어_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }

        private void 제어_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == 0)
            {
                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // 베팅액
            }
            if (e.RowIndex == 1)
            {
                if (e.ColumnIndex == 0)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // 손익

                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.Yellow; // Nasdaq
                if (e.ColumnIndex == 3)
                    e.CellStyle.BackColor = Color.Yellow; // S&P
            }
            if (e.RowIndex == 2)
            {
                if (e.ColumnIndex == 0)
                    e.CellStyle.BackColor = Color.Yellow;
                if (e.ColumnIndex == 1)
                    e.CellStyle.BackColor = Color.Yellow;
                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.Yellow;
                if (e.ColumnIndex == 3)
                    e.CellStyle.BackColor = Color.Yellow;
            }
            if (e.RowIndex == 5)
            {
                if (e.ColumnIndex == 2)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // light red
                if (e.ColumnIndex == 3)
                    e.CellStyle.BackColor = Color.FromArgb(255, 175, 255); // light red
            }
        }
    }
}