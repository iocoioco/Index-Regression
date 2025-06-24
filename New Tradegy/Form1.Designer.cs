using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
namespace New_Tradegy
{
  partial class Form1
  {
    // <summary>
    // Required designer variable.
    // </summary>
    private System.ComponentModel.IContainer components = null;

    // <summary>
    // Clean up any resources being used.
    // </summary>
    // <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
    if (disposing && (components != null))
    {
      components.Dispose();
    }
    base.Dispose(disposing);
    }

    // <summary>
    // </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.timer_eval_draw = new System.Windows.Forms.Timer(this.components);
            this.timer_코스피_코스닥 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();

            this.timer_코스피_코스닥.Tick += new System.EventHandler(this.timer_코스피_코스닥_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // chart1
            // 
            this.chart1.Location = new System.Drawing.Point(72, 141);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(300, 300);
            this.chart1.TabIndex = 1;
            this.chart1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 1061);
            this.Controls.Add(this.chart1);
            this.Font = new System.Drawing.Font("Gulim", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "20";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

    }
        private System.Windows.Forms.Timer timer_eval_draw;
        private System.Windows.Forms.Timer timer_코스피_코스닥;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}

