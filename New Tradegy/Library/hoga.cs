using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{

    internal class hg
    {
        private static CPUTILLib.CpCybos _cpcybos;

        public static bool HogaInsert(string stock, int rows, int rowId, int colId)
        {
            bool inserted = false;

            Size formSize = new Size();
            Point formLocation = new Point();
            HogaFormSizeLocation(stock, rowId, colId, rows, ref formSize, ref formLocation);

            if (stock == g.KODEX4[0])
            {
                HogaRemove(g.KODEX4[1]);
            }
            else if (stock == g.KODEX4[1])
            {
                HogaRemove(g.KODEX4[0]);
            }
            else if (stock == g.KODEX4[2])
            {
                HogaRemove(g.KODEX4[3]);
            }
            else if (stock == g.KODEX4[3])
            {
                HogaRemove(g.KODEX4[2]);
            }
            // if a form with stock name exists, locate a new location and return
            Form f = HogaFormNameGivenStock(stock);
            if (f != null)
            {
                f.Location = formLocation;
                return false;
            }

            Form_호가 form_호가 = new Form_호가(stock);
            form_호가.Show();

            return inserted;
        }
        public static bool HogaInsert(string stock)
        {
            if (HogaFormNameGivenStock(stock) != null)
                return false;

            bool inserted = false;

            if (stock == g.KODEX4[0])
            {
                HogaRemove(g.KODEX4[1]);
            }
            else if (stock == g.KODEX4[1])
            {
                HogaRemove(g.KODEX4[0]);
               
            }
            else if (stock == g.KODEX4[2])
            {
                HogaRemove(g.KODEX4[3]);
            }
            else if (stock == g.KODEX4[3])
            {
                HogaRemove(g.KODEX4[2]);
               
            }
            // if a form with stock name exists, locate a new location and return
            

            Form_호가 form_호가 = new Form_호가(stock);
           
            
            form_호가.Show();

            return inserted;
        }

        // called by HogaInsert
        // rowID 1 ~ 3, colID if 일반, not couting 0 and 1, start from 0, rows 5 or 10, return formSize, formLocation
        public static void HogaFormSizeLocation(string stock, int rowId, int colId, int rows, ref Size formSize, ref Point formLocation)
        {
            // 크기
            formSize.Width = g.screenWidth / 11;      // main form column => 11 ea 1920 / 11 = 174
            if (rows == 5)
            {
                formSize.Height = g.formSize.ch * 12; // height of a row * 12 rows for 5 Hoga
            }
            else
            {
                formSize.Height = g.formSize.ch * 22; // height of a row * 22 rows for 10 Hoga
            }

            // Column
            if (colId == g.nCol - 1)
            {
                colId--; // last column
            }
            else
            {
                colId++;
            }
            // Row
            if(rows == 10 && rowId == 2)
            {
                rowId--;
            }

            if (stock == g.KODEX4[2] || stock == g.KODEX4[3]) // kosdaq leverage & inverse
            {
                rowId++;
            }
           
            
            int xShift = 0;
            if (g.nCol == 10)
                xShift += 15;
            if (g.nCol == 9)
                xShift += 30;

            int yShift = 0;
            if (rows == 10 && rowId == 2)
            {
                yShift = g.formSize.ch * 4;
            }

            formLocation = new Point(g.screenWidth / g.rqwey_nCol * colId + xShift, g.screenHeight / g.rqwey_nRow * rowId + yShift);
        }


        // by Chat Gpt 20250315
        public static bool HogaRemove(string stock)
        {
            if (g.jpjds.TryGetValue(stock, out object a) && a is DSCBO1Lib.StockJpbid _stockjpbid)
            {
                // Unsubscribe from the stock data feed
                _stockjpbid.Unsubscribe();

                // Find the DataGridView by stock name and dispose of it safely
                Form f = fm.FindFormByName("Form1");
                DataGridView dgv = f?.Controls.Find(stock, true).FirstOrDefault() as DataGridView;

                if (dgv != null)
                {
                    dgv.Dispose();
                }

                // Remove the stock from the dictionary safely
                if (g.jpjds.TryRemove(stock, out object removedValue))
                {
                    // If the removed object implements IDisposable, dispose of it
                    if (removedValue is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                return true;
            }

            return false;
        }


        // from HogaForm find value : rowShft 
        public static int HogaGetValue(string stock, int rowShift, int column)
        {
            int return_value = -1;

            Form se = fm.FindFormByName("Form1");
            DataGridView dgv = fm.FindDataGridViewByName(se, stock);
            if (dgv == null)
            {
                return -1;
            }

            if (!IsBidChartComplete(dgv))
            {
                // Handle the case where the bid chart is not complete
                return -1;
            }


            int a = (dgv.RowCount - 2) / 2 + rowShift; // 매수1호가 -> rowShift = 0
            if (a >= dgv.RowCount - 2) // 호가 하단 3개 Row 호가 없음, 잘못 클릭으로 처리
            {
                return -1;
            }
            else
            {
                return return_value = wk.return_integer_from_mixed_string(dgv.Rows[a].Cells[column].Value.ToString());
            }
        }

        // Method to check if the bid chart is complete
        private static bool IsBidChartComplete(DataGridView dgv)
        {
            if (dgv == null)
            {
                return false;
            }

            if (dgv.RowCount != 12 && dgv.RowCount != 22)
            {
                return false;
            }
            return true;
        }

        // return form name if exist as a given stock name
        public static Form HogaFormNameGivenStock(string stock)
        {
            Form formFound = null;
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (form.Name == stock)
                    formFound = form;
            }
            return formFound;
        }

        // this is used to delete duplicate Form_지수_조정
        public static List<Form> FormNameContainGivenString(string str)
        {
            List<Form> list = new List<Form>();
            Form formFound = null;
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (form.Name.Contains(str))
                    list.Add(form);
            }
            return list;
        }

        public static Form FormContainGivenString(string str)
        {

            Form formFound = null;
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (form.Name.Contains(str))
                    return form;
            }
            return formFound;
        }

        // all stock forms name to a list and return it
        public static List<string> HogaAllStockFormNames()
        {
            List<string> stockFormNames = new List<string>();
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (wk.isStock(form.Name))
                {
                    stockFormNames.Add(form.Name);
                }
            }
            return stockFormNames;
        }

        // all forms name to a list and return it
        public static List<Form> FormListGeneralStock()
        {
            List<Form> FormList = new List<Form>();
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (wk.isStock(form.Name) && !g.KODEX4.Contains(form.Name))
                {
                    FormList.Add(form);
                }
            }
            return FormList;
        }

        public static int RemainSB()
        {
            if (_cpcybos == null)
                return 400;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_SUBSCRIBE);               // 400건의 요청으로 제한   
        }
    }
}
