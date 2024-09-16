namespace New_Tradegy
{
    partial class Form_매수_매도
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.Yes = new System.Windows.Forms.Button(); // Declare the Yes button
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(145, 179);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(915, 703);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            //this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(713, 660);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(200, 100);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "취소";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            //this.Cancel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Cancel_KeyPress);
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(281, 660);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(200, 100);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "지정가";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            this.Ok.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Ok_KeyPress);
            // 
            // Yes
            // 
            this.Yes.Location = new System.Drawing.Point(497, 660); // Set the location for the Yes button
            this.Yes.Name = "Yes";
            this.Yes.Size = new System.Drawing.Size(200, 100);
            this.Yes.TabIndex = 3;
            this.Yes.Text = "시장가";
            this.Yes.UseVisualStyleBackColor = true;
            this.Yes.Click += new System.EventHandler(this.Yes_Click);
            // 
            // Form_매수_매도
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1238, 1040);
            this.Controls.Add(this.Yes); // Add the Yes button to the form
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.richTextBox1);
            this.Name = "Form_매수_매도";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form_매수_매도";
            this.Load += new System.EventHandler(this.Form_매수_매도_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_매수_매도_ReiszeEnd);
            //this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form_매수_매도_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Yes; // Declare the Yes button
    }
}