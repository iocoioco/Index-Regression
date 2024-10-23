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
        private static CPUTILLib.CpCybos _cpcybos;
        Chart Chart;

        string _stock;
        Point _location;
        Size _size;
        int _rows;
        int _rowId;
        int _colId;
        //New_Tradegy.Library.jp _jp;

        //public static Color[] colorKODEX = { Color.White, Color.Red, Color.White, Color.Black, Color.Cyan, Color.Magenta, Color.DarkGreen, Color.White, Color.White, Color.White, Color.Blue, Color.Brown };
        // short and long time extension or restoration 
        // and then, dr.draw_chart()

        public Form_호가(string stock, Size formSize, Point formLocation, int rows, int rowId, int colId)  
        {
            InitializeComponent();

            _stock = stock;
            _location = formLocation;
            _size = formSize;
            if (_rows == 10)
            {
                _size.Height = g.formSize.ch * 22;
            }
            _rows = rows;
            _rowId = rowId;
            _colId = colId;
        }


        private void Form_호가_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;//윈도우테두리제거방법

            this.Size = _size;
            this.Location = _location;

            this.Text = _stock;
            this.Name = _stock;

            this.MaximizeBox = false;
            this.ControlBox = false;

            var a = new jp();

            a.Generate(_stock, this, _rows, _rowId, _colId);
        }
    }
}
