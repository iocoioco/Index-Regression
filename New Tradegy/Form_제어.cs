using New_Tradegy.Library;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace New_Tradegy
{
    public partial class Form_제어 : Form
    {

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
            this.Size = new Size(g.screenWidth / 11, g.formSize.ch * 3);
            int a = g.screenHeight;

            this.Location = new Point(g.screenWidth / g.rqwey_nCol + 15, g.formSize.ch * 12); // + 30 remove from width



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
            g.제어.dgv.RowTemplate.Height = g.formSize.ch;
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
            //g.제어.dtb.Rows[0][1] = g.v.key_string;
            g.제어.dtb.Rows[0][2] = g.일회거래액;
            g.제어.dtb.Rows[0][3] = g.예치금;

            if (!g.test)
            {
                dl.deal_profit();
            }

            // Row 1
            g.제어.dtb.Rows[1][0] = g.deal_profit;
            g.제어.dtb.Rows[1][1] = ""; // (int)usd_krw; 
            g.제어.dtb.Rows[1][2] = g.Nasdaq_지수;
            g.제어.dtb.Rows[1][3] = g.SP_지수;

            // Row 2

            g.제어.dtb.Rows[2][0] = ""; //상해
            g.제어.dtb.Rows[2][1] = ""; // 홍콩
            g.제어.dtb.Rows[2][2] = g.니케이지수;
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
            g.제어.dtb.Rows[8][1] = 20; g.v.수급과장배수 = 20;
            g.제어.dtb.Rows[8][2] = "배과";
            g.제어.dtb.Rows[8][3] = 1; g.v.배수과장배수 = 1;

            // Row 9
            g.제어.dtb.Rows[9][0] = "평가";
            g.제어.dtb.Rows[9][1] = 20; g.v.eval_per_marketeyes = 20;
            g.제어.dtb.Rows[9][2] = "초간";
            g.제어.dtb.Rows[9][3] = 30; g.postInterval = 30;

            // Row 10
            g.제어.dtb.Rows[10][0] = "Main";
            g.제어.dtb.Rows[10][1] = g.v.font = 16; g.v.font /= 2.0F;
            //g.제어.dtb.Rows[10][2] = "Form";
            //g.제어.dtb.Rows[10][3] = g.v.font = 15; g.v.font /= 2.0F;

            // Row 11
            //g.제어.dtb.Rows[11][0] = "mms";
            //g.제어.dtb.Rows[11][1] = 1000; g.marketeye_sleep_seconds = 1000;

            g.제어.dtb.Rows[11][2] = "보유";
            g.제어.dtb.Rows[11][3] = 10; g.v.보유종목점검최소액 = 10;

            // Row 12
            g.제어.dtb.Rows[12][0] = "푀플";
            g.제어.dtb.Rows[12][1] = "1"; g.v.푀플 = 1;
            g.제어.dtb.Rows[12][2] = "배플";
            g.제어.dtb.Rows[12][3] = "1"; g.v.배플 = 1;

            // Row 13
            g.제어.dtb.Rows[13][0] = "가격";
            g.제어.dtb.Rows[13][1] = 2; g.width.가격 = 2;
            g.제어.dtb.Rows[13][2] = "프돈";
            g.제어.dtb.Rows[13][3] = 2; g.width.프돈 = 2;

            // Roiw 14
            g.제어.dtb.Rows[14][0] = "외돈";
            g.제어.dtb.Rows[14][1] = 2; g.width.외돈 = 2;
            g.제어.dtb.Rows[14][2] = "기관";
            g.제어.dtb.Rows[14][3] = 1; g.width.기관 = 2;

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
                                dl.deal_deposit();
                                break;
                        }
                        break;

                    case 1:
                        switch (e.ColumnIndex)
                        {
                            case 0:
                                dl.deal_profit();
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

                switch (clickedVariable)
                {
                    case "종거":
                        int[] array = new int[] { 0, 50, 100, 200, 500, 1000, 1500, 2000 }; // 종거
                        int newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.종가기준추정거래액이상_천만원 = newValue * 10; // 억원을 천만원으로
                        g.제어.dtb.Rows[5][1] = newValue;
                        break;

                    case "분거":
                        array = new int[] { 0, 2, 5, 10, 20, 30, 50 }; // 분거
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.분당거래액이상_천만원 = newValue;
                        g.제어.dtb.Rows[5][3] = newValue;
                        break;

                    case "호가":
                        array = new int[] { 0, 5, 10, 20, 30, 50 }; // 호가
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.호가거래액_백만원 = newValue;
                        g.제어.dtb.Rows[6][1] = newValue;
                        break;

                    case "편차":
                        array = new int[] { 0, 1, 2, 3, 5, 7 }; // 편차
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.편차이상 = newValue;
                        g.제어.dtb.Rows[6][3] = newValue;
                        break;

                    case "배차":
                        array = new int[] { 0, 10, 25, 50, 75, 100 }; // 배차
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.배차이상 = newValue;
                        g.제어.dtb.Rows[7][1] = newValue;
                        break;

                    case "시총":
                        array = new int[] { 0, 10, 30, 50, 100, 200 }; // 시총 (백억 단위)
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.시총이상 = newValue;
                        g.제어.dtb.Rows[7][3] = newValue;
                        break;

                    case "수과":
                        array = new int[] { 5, 10, 15, 20, 25, 30, 40, 60, 100 }; // 수과
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.수급과장배수 = newValue;
                        g.제어.dtb.Rows[8][1] = newValue;
                        break;

                    case "배과":
                        array = new int[] { 1, 2, 3, 4, 5, 7, 10 }; // 배과
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.배수과장배수 = 1.0 / newValue;
                        g.제어.dtb.Rows[8][3] = newValue;
                        break;

                    case "평가": // not used
                        array = new int[] { 2, 5, 7, 10, 15, 20, 25, 40, 60 }; // 평가
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.eval_per_marketeyes = newValue;
                        g.제어.dtb.Rows[9][1] = newValue;
                        break;

                    case "초간":
                        array = new int[] { 10, 15, 20, 25, 30, 40, 50, 60, 70 }; // 초간
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.eval_per_marketeyes = newValue;
                        g.제어.dtb.Rows[9][3] = newValue;
                        break;

                    // font and pixel used
                    // 7   9.31
                    // 8   10.64
                    // 9   11.97
                    // 10  13.30
                    // 11  14.63
                    // 12  15.96
                    // 13  17.29
                    case "Main":
                        array = new int[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.v.font = (float)(newValue / 2.0F);
                        g.제어.dtb.Rows[10][1] = newValue;
                        break;

                    case "보유":
                        array = new int[] { 10, 50, 100, 250, 500, 1000 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }

                        g.v.보유종목점검최소액 = newValue;
                        g.제어.dtb.Rows[11][3] = newValue;
                        break;

                    case "푀플":
                        if (g.v.푀플 == 0)
                            g.v.푀플 = 1;
                        else
                            g.v.푀플 = 0;
                        if (g.v.푀플 == 1)
                            g.제어.dtb.Rows[12][1] = "1";
                        else
                            g.제어.dtb.Rows[12][1] = "0";
                        break;

                    case "배플":
                        if (g.v.배플 == 0)
                            g.v.배플 = 1;
                        else
                            g.v.배플 = 0;
                        if (g.v.배플 == 1)
                            g.제어.dtb.Rows[12][3] = "1";
                        else
                            g.제어.dtb.Rows[12][3] = "0";
                        break;

                    case "가격":
                        array = new int[] { 1, 2, 3 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.width.가격 = newValue;
                        g.제어.dtb.Rows[13][1] = newValue;
                        break;

                    case "프돈":
                        array = new int[] { 1, 2, 3 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.width.프돈 = newValue;
                        g.제어.dtb.Rows[13][3] = newValue;
                        break;

                    case "외돈":
                        array = new int[] { 1, 2, 3 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.width.외돈 = newValue;
                        g.제어.dtb.Rows[14][0] = newValue;
                        break;

                    case "기관":
                        array = new int[] { 1, 2, 3 };
                        newValue = FindNewValueFromArray(array, clickedValue, upper);
                        if (newValue < 0)
                        {
                            return;
                        }
                        g.width.기관 = newValue;
                        g.제어.dtb.Rows[14][3] = newValue;
                        break;
                }
            }

            g.제어.dgv.Refresh();
            
        }

        private int FindNewValueFromArray(int[] array, int clickedValue, bool upper)
        {
            int position = -1;
            for (int i = 0; i < array.Length; i++)
            {
                {
                    if (array[i] == clickedValue)
                    {

                        if (upper)
                        {
                            position = i + 1;
                            if (position >= array.Length)
                            {
                                position = 0;
                            }
                        }
                        else
                        {
                            position = i - 1;
                            if (position < 0)
                            {
                                position = array.Length - 1;
                            }
                        }
                    }
                }
            }
            if (position < 0)
            {
                return -1;
            }
            else
            {
                return array[position];
            }
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