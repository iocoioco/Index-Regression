using DSCBO1Lib;
using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static New_Tradegy.Library.g;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace New_Tradegy
{
    // 1st  x 809 y 271 w 359
    // 2nd x 809 y 509 w 359
    // 3rd x 809 y 750 w 359 h 405

    // 종목
    // (1st, 2nd, 3rd)
    // 매수 또는 매도
    
    public partial class Form_매수_매도 : Form
    {
        public static string str;
        string stock;
        int _urgency;
        int _cancelThreshhold;

        //private System.Windows.Forms.Button Yes;

        public Form_매수_매도(string ReceivedStock, string text, int Urgency, int CancelThreshhold, string ReceivedString)
        {
            InitializeComponent();
            this.Text = text; // 매수 또는 매도

            stock = ReceivedStock;
            str = ReceivedString;

            int blockStart = 1; //arbitrary numbers to test
            int blockLength = 15;
            richTextBox1.SelectionStart = blockStart;
            richTextBox1.SelectionLength = blockLength;
            richTextBox1.SelectionBackColor = Color.FromArgb(235, 255, 255);

            Form se = (Form)Application.OpenForms["se"];
            DataGridView dgv = fm.FindDataGridViewByName(se, stock);
            this.Size = new Size(g.screenWidth / 5, 405);
            this.Location = new Point(dgv.Location.X + g.screenWidth / g.nCol, dgv.Location.Y);

            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Size = this.Size;

            _urgency = Urgency;
            _cancelThreshhold = CancelThreshhold;
        }

        private void Form_매수_매도_Load(object sender, EventArgs e)
        {
            appendMemoRichTextBox(str, Color.Black, true);

            // Initialize Yes button
            //Yes = new System.Windows.Forms.Button();


            //Yes.Text = "Yes";
            //Yes.DialogResult = DialogResult.Yes;
            //Yes.Click += Yes_Click;
            //this.Controls.Add(Yes);

            Ok.TabIndex = 0;
            Yes.TabIndex = 1;
            Cancel.TabIndex = 2;


            // Adjust Ok and Cancel button positions
            Yes.Size = new Size(80, 25);
            Ok.Size = new Size(80, 25);
            Cancel.Size = new Size(80, 25);

            Yes.Location = new Point(this.Width / 8, (int)(this.Height * 0.80));
            Ok.Location = new Point(this.Width / 8 * 3, (int)(this.Height * 0.80));
            Cancel.Location = new Point(this.Width / 8 * 5, (int)(this.Height * 0.80));

            // Bring buttons to front
            Yes.BringToFront();
            Ok.BringToFront();
            Cancel.BringToFront();

           
            // Urgency
            HScrollBar hUrgencyScrollBar = new HScrollBar();
            hUrgencyScrollBar.Minimum = 0;
            hUrgencyScrollBar.Maximum = 110;
            hUrgencyScrollBar.Value = (int)(_urgency * 1.1);
            hUrgencyScrollBar.Name = "";
            hUrgencyScrollBar.Text = "";
            hUrgencyScrollBar.Location = new Point(this.Width / 7, (int)(this.Height * 0.65));
            int w = Cancel.Location.X - Ok.Location.X + Cancel.Size.Width;
            hUrgencyScrollBar.Size = new System.Drawing.Size(w, (int)(Ok.Size.Height * 1.0));
            hUrgencyScrollBar.Scroll += HUrgencyscrollBar_Scroll;
            hUrgencyScrollBar.BringToFront();




            // CancelThreshhold
            HScrollBar hCancelThreshholdScrollBar = new HScrollBar();
            hCancelThreshholdScrollBar.Minimum = 0;
            hCancelThreshholdScrollBar.Maximum = 110;
            hCancelThreshholdScrollBar.Value = (_cancelThreshhold - 1) * 25;
            hCancelThreshholdScrollBar.Name = "";
            hCancelThreshholdScrollBar.Text = "";
            hCancelThreshholdScrollBar.Location = new Point(this.Width / 7, (int)(this.Height * 0.65) + 30);
            w = Cancel.Location.X - Ok.Location.X + Cancel.Size.Width;
            hCancelThreshholdScrollBar.Size = new System.Drawing.Size(w, (int)(Ok.Size.Height * 1.0));
            hCancelThreshholdScrollBar.Scroll += HCancelThreshholdscrollBar_Scroll;
            hCancelThreshholdScrollBar.Width = w;
            hCancelThreshholdScrollBar.BringToFront();

            richTextBox1.Controls.Add(hUrgencyScrollBar);
            richTextBox1.Controls.Add(hCancelThreshholdScrollBar);
            this.TopMost = true;

            int index = wk.return_index_of_ogldata(stock);
            if (index < 0)
                return;
            g.stock_data o = g.ogl_data[index];

            if (index >= 0)
            {
                if (o.매수1호가 > 0)
                {
                    o.수익률 = (double)(o.매수1호가 - o.장부가) / o.매수1호가 * 100;
                }

                if (o.수익률 < 0)
                {
                    appendMemoRichTextBox("\n               No Watering" + ": " + o.수익률.ToString("0.##"), Color.Red, true);
                }
            }
            this.ActiveControl = Ok;
            this.Ok.DialogResult = DialogResult.OK;
            this.AcceptButton = Ok;

        }

        

        public int The_urgency
        {
            get { return this._urgency; }
        }

        public int The_cancelthreshhold
        {
            get { return this._cancelThreshhold; }
        }

        private void HUrgencyscrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                _urgency = (int)e.NewValue;
                if (_urgency < 0)
                {
                    _urgency = 10;
                }
                if (_urgency > 100)
                {
                    _urgency = 100;
                }
            }
        }

        private void HCancelThreshholdscrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                _cancelThreshhold = e.NewValue/ 25 + 1; // the interval was set 20 - 110, now reset effect 1 - 5
                g.cancelThreshhold = _cancelThreshhold ;
            }
        }

        // addNewLine : if true add new line at the end, else ...
        public void appendMemoRichTextBox(string text, Color color, bool addNewLine = false)
        {
           
            richTextBox1.SuspendLayout();
            richTextBox1.SelectionColor = color;
            if(color ==  Color.Black)
            {
                richTextBox1.SelectionFont = new Font("Arial", 9, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            }
            else
            {
                richTextBox1.SelectionFont = new Font("Arial", 15, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            }
            //richTextBox1.SelectionFont = new Font("Arial", 15, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
           // richTextBox1.Font = new Font("Arial", 15, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            richTextBox1.AppendText(addNewLine
                ? $"{text}{Environment.NewLine}"
                : text);
            richTextBox1.ScrollToCaret();
            richTextBox1.ResumeLayout();


            //int startIndex = richTextBox1.SelectionStart;
            //richTextBox1.Text = richTextBox1.Text.Insert(richTextBox1.SelectionStart, text);
        }
        private void Yes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (!Validate())
            {
                this.DialogResult = DialogResult.None;
                this.Close();
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            if (!Validate())
            {
                this.DialogResult = DialogResult.None;
                this.Close();
            }
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Form_매수_매도_ReiszeEnd(object sender, EventArgs e)
        {
            Location = this.Location;
            Size = this.Size;
        }

        //private void Form_매수_매도_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    ky.chart_keypress(e);
        //}

        //private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if(e.KeyChar == (char)Keys.Escape)
        //    {
        //        this.Close();
        //        return;
        //    }

        //}

        private void Ok_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Escape)
            {
                this.Close();
                return;
            }
            ky.chart_keypress(e);
        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (ActiveControl == richTextBox1)
        //    {
        //        Ok.Focus(); // Move to the first button (Yes) when richTextBox has focus
        //        return true;
        //    }

        //    // Check if the left or right arrow keys are pressed
        //    if (keyData == Keys.Left)
        //    {
        //        // Handle left arrow key behavior: Move between Yes and Ok
        //        if (ActiveControl == Ok)
        //        {
        //            Yes.Focus();
        //            return true;  // Stops further processing
        //        }
        //        else if (ActiveControl == Yes)
        //        {
        //            Cancel.Focus();
        //            return true;  // Stops further processing
        //        }
        //        else
        //        {
        //            Ok.Focus();
        //            return true;  // Stops further processing
        //        }
        //    }

        //    if (keyData == Keys.Right)
        //    {
        //        // Handle right arrow key behavior: Move between Ok and Cancel
        //        if (ActiveControl == Ok)
        //        {
        //            Cancel.Focus();
        //            return true;  // Stops further processing
        //        }
        //        // Move from Cancel back to Ok if the right arrow is pressed
        //        if (ActiveControl == Cancel)
        //        {
        //            Yes.Focus();
        //            return true;  // Stops further processing
        //        }
        //        else
        //        {
        //            Ok.Focus();
        //            return true;  // Stops further processing
        //        }
        //    }

        //    // Default case: No special key processing, pass control to the base method
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}


    }
}
