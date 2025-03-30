using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{
    internal class wn
    {
        public static void Memo_TopMost()
        {
            //if (Application.OpenForms["Memo"] == null)
            //{
            //    Form Memo = new Memo();
            //    Memo.Size = new Size(359, 405);
            //    Memo.Location = new Point(809, 509);
            //    Memo.Show();
            //}

            //Memo Memo_1 = (Memo)Application.OpenForms["Memo"];
            //if (Memo_1 != null)
            //{
            //    Memo_1.TopMost = true;
            //}

            Form memo = new Memo();
            memo.Size = new Size(359, 405);
            memo.Location = new Point(809, 509);
            memo.Show();
            //Memo.TopMost = true;    
        }
    }
}
