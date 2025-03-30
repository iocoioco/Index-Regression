using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g;


namespace New_Tradegy
{
    public partial class Form_호가 : Form
    {
        // private static CPUTILLib.CpCybos _cpcybos;
        // Chart Chart;

        //string _stock;

        public Form_호가(string stock)  
        {
         
            InitializeComponent();

        //    _stock = stock;

        
        }


        private void Form_호가_Load(object sender, EventArgs e)
        {
            
            //this.FormBorderStyle = FormBorderStyle.None;//윈도우테두리제거방법

            //this.Text = _stock;
            //this.Name = _stock;

            //this.MaximizeBox = false;
            //this.ControlBox = false;

            //var a = new jp();

            //a.Generate(_stock);
        }
    }
}
