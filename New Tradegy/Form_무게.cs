using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using New_Tradegy.Library;

namespace New_Tradegy
{
    public partial class Form_무게 : Form
    {
        private string filePath = @"C:\병신\data\무게.txt"; // Path to the file containing the doubles

        public Form_무게()
        {
            InitializeComponent();
            LoadValuesFromFile();
            AttachTextChangedEvents();
        }

        private void LoadValuesFromFile()
        {
            if (File.Exists(filePath))
            {
                string[] values = File.ReadAllLines(filePath);
                if (values.Length >= 7)
                {
                    textBox1.Text = values[0];
                    textBox2.Text = values[1];
                    textBox3.Text = values[2];
                    textBox4.Text = values[3];
                    textBox5.Text = values[4];
                    textBox6.Text = values[5]; // New text box
                    textBox7.Text = values[6]; // New text box
                }
            }
        }

        private void AttachTextChangedEvents()
        {
            textBox1.TextChanged += TextBox_TextChanged;
            textBox2.TextChanged += TextBox_TextChanged;
            textBox3.TextChanged += TextBox_TextChanged;
            textBox4.TextChanged += TextBox_TextChanged;
            textBox5.TextChanged += TextBox_TextChanged;
            textBox6.TextChanged += TextBox_TextChanged; // New text box
            textBox7.TextChanged += TextBox_TextChanged; // New text box
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            SaveValuesToFile();
        }

        private void SaveValuesToFile()
        {
           string[] values = new string[7];
            values[0] = textBox1.Text;
            values[1] = textBox2.Text;
            values[2] = textBox3.Text;
            values[3] = textBox4.Text;
            values[4] = textBox5.Text;
            values[5] = textBox6.Text; // New text box
            values[6] = textBox7.Text; // New text box



            // convert values to doubles with name a, b, c, d, e, f, g

            if (double.TryParse(values[0], out g.s.푀분_wgt) &&
                double.TryParse(values[1], out g.s.거분_wgt) &&
                double.TryParse(values[2], out g.s.배차_wgt) &&
                double.TryParse(values[3], out g.s.배합_wgt) &&
                double.TryParse(values[4], out g.s.그룹_wgt) &&
                double.TryParse(values[5], out g.s.피로_wgt) &&
                double.TryParse(values[6], out g.s.기타_wgt))
            {
                // Successfully parsed all values
                File.WriteAllLines(filePath, values);// Save the values to the file
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            string[] values = new string[7];
            values[0] = textBox1.Text;
            values[1] = textBox2.Text;
            values[2] = textBox3.Text;
            values[3] = textBox4.Text;
            values[4] = textBox5.Text;
            values[5] = textBox6.Text; // New text box
            values[6] = textBox7.Text; // New text box

            if (double.TryParse(values[0], out g.s.푀분_wgt) &&
                double.TryParse(values[1], out g.s.거분_wgt) &&
                double.TryParse(values[2], out g.s.배차_wgt) &&
                double.TryParse(values[3], out g.s.배합_wgt) &&
                double.TryParse(values[4], out g.s.그룹_wgt) &&
                double.TryParse(values[5], out g.s.피로_wgt) &&
                double.TryParse(values[6], out g.s.기타_wgt))
            {
                // Successfully parsed all values
                File.WriteAllLines(filePath, values);// Save the values to the file
                                                     // close form
                this.Close();
            }
            else
            {
                // Handle the error, e.g., show a message to the user
                MessageBox.Show("Please enter valid numeric values in all text boxes.");
            }
        }
    }
}