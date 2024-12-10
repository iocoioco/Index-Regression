using New_Tradegy.Library;
using System;
using System.Windows.Forms;

namespace New_Tradegy
{
    public partial class Form_지수_조정 : Form
    {
        double[] t = new double[6];
        // 지수내의 지수내합(3), 기관(4), 외인(5), 개인(6), 나스닥지수(10), 연기(11)
        // "기관", "외인", "개인", "연기" 를 하나의 변수로 처리
        // "지수"
        // "나스닥" 
        // "가격"
        string _stock = "";
        string[] s = new string[4] { "가격", "지수", "기타", "나스닥" };
        public Form_지수_조정(string kospiorKosdaq)
        {
            InitializeComponent();

            _stock = kospiorKosdaq;
        }

        private void Form_지수_조정_Load(object sender, EventArgs e)
        {
            this.Text = "Form_지수_조정" + " (" + _stock + ")";


            int w = g.chart1.Bounds.Width / 4;
            int h = g.chart1.Bounds.Height / 4;

            int id = 0;
            for (int i = 0; i < g.KODEX4.Count; i++)
            {
                if (g.clickedStock == g.KODEX4[i])
                {
                    id = i;
                    break;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                System.Windows.Forms.TextBox t = new System.Windows.Forms.TextBox();
                t.Text = s[i];
                t.Location = new System.Drawing.Point(0, 10 + i * h / 5);
                t.Size = new System.Drawing.Size(40, h / 10);
                t.Enabled = false;
                this.Controls.Add(t);

                HScrollBar hScrollBar = new HScrollBar();
                hScrollBar.Minimum = 10;
                hScrollBar.Maximum = 200;
                hScrollBar.Value = 100;

                hScrollBar.Name = "Horizontal";
                hScrollBar.Text = "Horizontal" + " " + i.ToString();
                hScrollBar.Location = new System.Drawing.Point(50, i * h / 5);
                hScrollBar.Size = new System.Drawing.Size(w - 80, h / 10);
                hScrollBar.Scroll += HScrollBar_Scroll;
                //int a = hScrollBar.InnerScrollBar.Width;
                this.Controls.Add(hScrollBar);
                //this.KeyDown += new KeyEventHandler(Form_지수_조정_Keydown);
                this.KeyPress += new KeyPressEventHandler(지수_조정_KeyPress);
            }
            this.TopMost = true;
        }

        private void Form_지수_조정_Keydown(object sender, KeyEventArgs e)
        {

        }

        private void 지수_조정_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.Close();
                return;
            }
            ky.chart_keypress(e);
        }


        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                double value = e.NewValue;

                HScrollBar c = (HScrollBar)sender;
                string Name = c.Text;

                int id = 0;
                for (int i = 0; i < g.KODEX4.Count; i++)
                {
                    if (g.clickedStock == g.KODEX4[i])
                    {
                        id = i;
                        break;
                    }
                }

                int jd = 0;
                for (int j = 0; j < 4; j++)
                {
                    if (Name.Contains(j.ToString()))
                    {
                        jd = j;
                        break;
                    }
                }
                g.kodex_magnifier[id, jd] *= (value / 100.0);

                int index = wk.return_index_of_ogldata(g.clickedStock);
                g.stock_data o = g.ogl_data[index];

                wk.deleteChartAreaAnnotation(g.chart1, g.clickedStock, true, false); // KODEX
                wk.deleteChartAreaAnnotation(g.chart2, g.clickedStock, true, false); // KODEX
                md.mdm(); // single KODEX, HScrollBar_Scroll
                dr.mds(); // signle KODEX, HScrollBar_Scroll

            }
        }

        private void Form_지수_조정_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }

        private void Form_지수_조정_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form_지수_조정_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form_지수_조정_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


        //SQUID size_location() 참조할 것
        //지수 지수 = (지수)Application.OpenForms["지수"];
        //Form_KODEX_.Location = new Point(0, 0);
        //Form_KODEX_.Size = new Size(1920 / 2, height);

        //this.components = new System.ComponentModel.Container();
        //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        //this.ClientSize = new System.Drawing.Size(1300, 450);
    }
}
