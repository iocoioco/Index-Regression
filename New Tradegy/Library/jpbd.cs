using CPTRADELib;
using OpenQA.Selenium.BiDi.Modules.Input;
using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using static New_Tradegy.Library.g.stock_data;
using static OpenQA.Selenium.BiDi.Modules.Script.RealmInfo;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library
{

    class jp
    {
        private CPUTILLib.CpCybos _cpcybos;
        private CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        private DSCBO1Lib.StockJpbid _stockjpbid;
        private DSCBO1Lib.StockJpbid2 _stockjpbid2;

        private DataTable Dtb;
        private DataGridView Dgv;
        private int Rows = 5;
        private string Stock;





        public DataGridView Generate(string stock)
        {
            if (!g.connected)
            {
                return null;
            }

            Stock = stock;

            int w0 = 61;
            int w1 = 50;
            int w2 = 61;

            int CellHeight = 27;
            Dtb = new DataTable();

            Dtb.Columns.Add("매도");
            Dtb.Columns.Add("호가");
            Dtb.Columns.Add("매수");

            for (int i = 0; i < 2 * Rows + 2; i++)
            {
                Dtb.Rows.Add("", "", "");
            }

            Dgv = new DataGridView();

            Dgv.DataSource = Dtb;

            if (Dtb.Columns.Count == 0)
            {
                throw new InvalidOperationException("DataTable does not have any columns.");
            }

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = Dtb;
            Dgv.DataSource = bindingSource;

            string stockcode = _cpstockcode.NameToCode(stock);
            _stockjpbid = new DSCBO1Lib.StockJpbid();
            _stockjpbid.SetInputValue(0, stockcode);
            _stockjpbid.Received +=
                new DSCBO1Lib._IDibEvents_ReceivedEventHandler(stockjpbid_Received);

            _stockjpbid.Subscribe();

            if (!g.BookBidInstances.ContainsKey(stock))
            {
                g.BookBidInstances.TryAdd(stock, _stockjpbid);
            }
            else
            {
                return null;
            }

            Dgv.DataError += (s, e) => wr.DataGridView_DataError(s, e, "jpjd Dgv");
            Dgv.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);
            Dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            Dgv.Location = new Point(120, 0);
            Dgv.Size = new Size(w0 + w1 + w2, CellHeight * 13);

            Dgv.Name = stock;

            Dgv.ColumnHeadersVisible = false;
            Dgv.RowHeadersVisible = false;
            int fontsize = 9;


            Dgv.DefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            Dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
            Dgv.RowTemplate.Height = CellHeight;
            Dgv.ForeColor = Color.Black;
            Dgv.ScrollBars = ScrollBars.None;
            Dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            //Dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            Dgv.AllowUserToResizeColumns = false;
            Dgv.AllowUserToResizeRows = false;

            Dgv.AllowUserToAddRows = false;
            Dgv.AllowUserToDeleteRows = false;
            Dgv.Dock = System.Windows.Forms.DockStyle.None;

            Dgv.ReadOnly = true;
            Dgv.RowHeadersVisible = false;
            Dgv.ColumnHeadersVisible = false;
            Dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            Dgv.TabIndex = 1;
            Dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(Dgv_CellMouseClick);
            Dgv.KeyPress += OnDataGirdView1_KeyPress;

            Form form = fm.FindFormByName("Form1");
            form.Controls.Add(Dgv);

            Dgv.Columns[0].Width = w0;
            Dgv.Columns[1].Width = w1;
            Dgv.Columns[2].Width = w2;

            request_호가();

            Dgv.BringToFront();

            return Dgv;
        }

        private Form_매수_매도 GetOpenTradeForm(string stockName)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form_매수_매도 tradeForm && tradeForm._stock == stockName)
                {
                    return tradeForm; // Found an open form for the stock
                }
            }
            return null; // No open form found
        }

        public void OpenOrUpdateConfirmationForm(bool isSell, string stockName, int Amount, int price, int Urgency, string str)
        {
            Form_매수_매도 f = GetOpenTradeForm(stockName);
            if (f != null)
            {
                if (f._isSell == isSell)
                {
                    mc.Sound("", "not sold");
                    return;
                }
                else
                {
                    // Update existing form
                    f.UpdateForm(isSell, stockName, Amount, price, Urgency, str);
                }

            }
            else
            {
                // Create and show a new non-blocking (modeless) confirmation form
                Form_매수_매도 form = new Form_매수_매도(isSell, stockName, Amount, price, Urgency, str);
                form.Show(); // Modeless (non-blocking)
            }

        }
        private void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // 오른쪽 버튼 사용 또는 10호가창 리턴
                return;

            int Price = hg.HogaGetValue(Stock, e.RowIndex - Rows, 1);
            if (Price == 0)
            {
                return;
            }

            int index = wk.return_index_of_ogldata(Stock);
            if (index < 0)
            {
                return;
            }
            g.stock_data o = g.ogl_data[index];




            // 컬럼 1
            if (e.ColumnIndex == 1)
            {
                // Passing Price
                if (e.RowIndex < Rows)
                {
                    if (Price == o.pass.upperPassingPrice)
                    {
                        o.pass.upperPassingPrice = 0;
                    }
                    else
                    {
                        o.pass.upperPassingPrice = Price;
                    }
                }
                else if (e.RowIndex <= Rows)
                {
                    if (Price == o.pass.lowerPassingPrice)
                    {
                        o.pass.lowerPassingPrice = 0;
                    }
                    else
                    {
                        o.pass.lowerPassingPrice = Price;
                    }
                }
                // 5 & 10 hoga toggle
                else if (e.ColumnIndex <= Rows * 2)
                {
                    if (g.KODEX4.Contains(Stock))
                    {
                        return;
                    }
                    if (Rows == 5)
                    {
                        Rows = 10;
                    }
                    else
                    {
                        Rows = 5;
                    }
                }
                return;
            }








            // 컬럼 0, 2, 그 중 RowIndex >= Rows 리턴
            if (e.RowIndex >= Rows * 2)
            {
                return;
            }

            // 바로 매수/매도 결정

            bool isSell = false;
            isSell = (e.ColumnIndex == 0);
            bool isBuy = false;
            isBuy = (e.ColumnIndex == 2);

            if (isBuy)
            {

                var existingOrder = StockExchange.buyOrders.Find(order => order.Stock == Stock && order.Price == Price);

                // If such an order exists, remove it
                if (existingOrder != null)
                {
                    StockExchange.buyOrders.Remove(existingOrder);

                }



                int Amount = g.일회거래액 * 10000 / Price;
                if (Amount == 0)
                {
                    Amount = 1;
                }

                mc.Sound_돈(g.일회거래액);
                int Urgency = (g.optimumTrading) ? (int)(e.X / (double)Dgv.Columns[2].Width * 100) : 100;

                string str = "";
                //if (g.confirm_buy)
                //{
                    str += Stock + " : " + Price.ToString() + " X " + Amount.ToString() +
                               " = " + (Price * Amount / 10000).ToString() + "만원";

                    str += "\n" + sr.r3_display_매수_매도(o);

                    if (DealManager.CheckPreviousLoss(Stock))
                    {
                        return;
                    }

                    OpenOrUpdateConfirmationForm(isSell, Stock, Amount, Price, Urgency, str);
                //}
                //else
                //{
                //    DealManager.deal_exec("매수", Stock, Amount, Price, "01");
                //}
            }


            else
            {
                var existingOrder = StockExchange.sellOrders.Find(order => order.Stock == Stock && order.Price == Price);

                // If such an order exists, remove it
                if (existingOrder != null)
                {
                    StockExchange.sellOrders.Remove(existingOrder);

                }

                int Amount = g.일회거래액 * 10000 / Price;
                if (Amount == 0)
                    Amount = 1;

                if (o.보유량 < Amount)
                {
                    DealManager.DealCancelStock(Stock); // dgv_CellClick tr(2) 에러 메세지 20231122
                    DealManager.deal_hold(); // dgv_CellClick tr(1)
                    if (o.보유량 == 0)
                    {
                        return;
                    }
                }
                if (o.보유량 < Amount)
                {
                    Amount = o.보유량;
                }
                if (Amount == 0)
                {
                    Amount = 1;
                }

                mc.Sound_돈(g.일회거래액);
                int Urgency = (g.optimumTrading) ? (int)(e.X / (double)Dgv.Columns[0].Width * 100) : 100;


                string str = "";


                if (g.confirm_sell)  // "03" <- 시장가 매도 ... 비상매도
                {
                    str += Stock + " : " + Price.ToString() + " X " + Amount.ToString() +
                               " = " + (Price * Amount / 10000).ToString() + "만원";

                    str += "\n" + sr.r3_display_매수_매도(o);
                    OpenOrUpdateConfirmationForm(isSell, Stock, Amount, Price, Urgency, str);
                }
                else
                {
                    DealManager.deal_exec("매도", Stock, Amount, Price, "01");
                }
            }
        }


        // by Chat Gpt 20250315
        private void Unsubscribe()
        {
            // Step 1: Unsubscribe event
            if (_stockjpbid != null)
            {
                _stockjpbid.Received -= stockjpbid_Received; // Explicitly detach the event handler
                _stockjpbid.Unsubscribe(); // Unsubscribe from real-time data
            }

            // Step 2: Remove stock from dictionary safely
            if (g.BookBidInstances.TryRemove(Stock, out object removedValue))
            {
                // If the removed object implements IDisposable, dispose it
                if (removedValue is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Step 3: Dispose of DataGridView safely
            if (Dgv != null)
            {
                Dgv.Dispose();
                Dgv = null; // Ensure reference is cleared
            }

            // Step 4: Nullify _stockjpbid for garbage collection
            _stockjpbid = null;
        }


        // updated on 20241020, lock, BeginLoadData, EndLoadData added
        private void stockjpbid_Received()
        {
            lock (g.lockObject)
            {
                // Begin batch update to minimize UI refreshes
                Dgv.SuspendLayout();

                try
                {
                    if (Rows == 5)
                    {
                        Dtb.Rows[4][1] = _stockjpbid.GetHeaderValue(3).ToString();
                        Dtb.Rows[5][1] = _stockjpbid.GetHeaderValue(4).ToString();
                        Dtb.Rows[4][0] = _stockjpbid.GetHeaderValue(5).ToString();
                        Dtb.Rows[5][2] = _stockjpbid.GetHeaderValue(6).ToString();

                        Dtb.Rows[3][1] = _stockjpbid.GetHeaderValue(7).ToString();
                        Dtb.Rows[6][1] = _stockjpbid.GetHeaderValue(8).ToString();
                        Dtb.Rows[3][0] = _stockjpbid.GetHeaderValue(9).ToString();
                        Dtb.Rows[6][2] = _stockjpbid.GetHeaderValue(10).ToString();

                        Dtb.Rows[2][1] = _stockjpbid.GetHeaderValue(11).ToString();
                        Dtb.Rows[7][1] = _stockjpbid.GetHeaderValue(12).ToString();
                        Dtb.Rows[2][0] = _stockjpbid.GetHeaderValue(13).ToString();
                        Dtb.Rows[7][2] = _stockjpbid.GetHeaderValue(14).ToString();

                        Dtb.Rows[1][1] = _stockjpbid.GetHeaderValue(15).ToString();
                        Dtb.Rows[8][1] = _stockjpbid.GetHeaderValue(16).ToString();
                        Dtb.Rows[1][0] = _stockjpbid.GetHeaderValue(17).ToString();
                        Dtb.Rows[8][2] = _stockjpbid.GetHeaderValue(18).ToString();

                        Dtb.Rows[0][1] = _stockjpbid.GetHeaderValue(19).ToString();
                        Dtb.Rows[9][1] = _stockjpbid.GetHeaderValue(20).ToString();
                        Dtb.Rows[0][0] = _stockjpbid.GetHeaderValue(21).ToString();
                        Dtb.Rows[9][2] = _stockjpbid.GetHeaderValue(22).ToString();

                        Dtb.Rows[10][0] = _stockjpbid.GetHeaderValue(23).ToString();
                        Dtb.Rows[10][2] = _stockjpbid.GetHeaderValue(24).ToString();
                    }
                    else
                    {
                        Dtb.Rows[20][0] = Int32.Parse((_stockjpbid.GetHeaderValue(23).ToString())).ToString();
                        Dtb.Rows[20][2] = Int32.Parse((_stockjpbid.GetHeaderValue(24).ToString())).ToString();

                        Dtb.Rows[9][1] = _stockjpbid.GetHeaderValue(3).ToString();
                        Dtb.Rows[10][1] = _stockjpbid.GetHeaderValue(4).ToString();
                        Dtb.Rows[9][0] = _stockjpbid.GetHeaderValue(5).ToString();
                        Dtb.Rows[10][2] = _stockjpbid.GetHeaderValue(6).ToString();

                        Dtb.Rows[8][1] = _stockjpbid.GetHeaderValue(7).ToString();
                        Dtb.Rows[11][1] = _stockjpbid.GetHeaderValue(8).ToString();
                        Dtb.Rows[8][0] = _stockjpbid.GetHeaderValue(9).ToString();
                        Dtb.Rows[11][2] = _stockjpbid.GetHeaderValue(10).ToString();

                        Dtb.Rows[7][1] = _stockjpbid.GetHeaderValue(11).ToString();
                        Dtb.Rows[12][1] = _stockjpbid.GetHeaderValue(12).ToString();
                        Dtb.Rows[7][0] = _stockjpbid.GetHeaderValue(13).ToString();
                        Dtb.Rows[12][2] = _stockjpbid.GetHeaderValue(14).ToString();

                        Dtb.Rows[6][1] = _stockjpbid.GetHeaderValue(15).ToString();
                        Dtb.Rows[13][1] = _stockjpbid.GetHeaderValue(16).ToString();
                        Dtb.Rows[6][0] = _stockjpbid.GetHeaderValue(17).ToString();
                        Dtb.Rows[13][2] = _stockjpbid.GetHeaderValue(18).ToString();

                        Dtb.Rows[5][1] = _stockjpbid.GetHeaderValue(19).ToString();
                        Dtb.Rows[14][1] = _stockjpbid.GetHeaderValue(20).ToString();
                        Dtb.Rows[5][0] = _stockjpbid.GetHeaderValue(21).ToString();
                        Dtb.Rows[14][2] = _stockjpbid.GetHeaderValue(22).ToString();

                        Dtb.Rows[4][1] = _stockjpbid.GetHeaderValue(27).ToString();
                        Dtb.Rows[15][1] = _stockjpbid.GetHeaderValue(28).ToString();
                        Dtb.Rows[4][0] = _stockjpbid.GetHeaderValue(29).ToString();
                        Dtb.Rows[15][2] = _stockjpbid.GetHeaderValue(30).ToString();

                        Dtb.Rows[3][1] = _stockjpbid.GetHeaderValue(31).ToString();
                        Dtb.Rows[16][1] = _stockjpbid.GetHeaderValue(32).ToString();
                        Dtb.Rows[3][0] = _stockjpbid.GetHeaderValue(33).ToString();
                        Dtb.Rows[16][2] = _stockjpbid.GetHeaderValue(34).ToString();

                        Dtb.Rows[2][1] = _stockjpbid.GetHeaderValue(35).ToString();
                        Dtb.Rows[17][1] = _stockjpbid.GetHeaderValue(36).ToString();
                        Dtb.Rows[2][0] = _stockjpbid.GetHeaderValue(37).ToString();
                        Dtb.Rows[17][2] = _stockjpbid.GetHeaderValue(38).ToString();

                        Dtb.Rows[1][1] = _stockjpbid.GetHeaderValue(39).ToString();
                        Dtb.Rows[18][1] = _stockjpbid.GetHeaderValue(40).ToString();
                        Dtb.Rows[1][0] = _stockjpbid.GetHeaderValue(41).ToString();
                        Dtb.Rows[18][2] = _stockjpbid.GetHeaderValue(42).ToString();

                        Dtb.Rows[0][1] = _stockjpbid.GetHeaderValue(43).ToString();
                        Dtb.Rows[19][1] = _stockjpbid.GetHeaderValue(44).ToString();
                        Dtb.Rows[0][0] = _stockjpbid.GetHeaderValue(45).ToString();
                        Dtb.Rows[19][2] = _stockjpbid.GetHeaderValue(46).ToString();
                    }

                    // Additional logic for monitoring prices, for example
                    string code = _stockjpbid.GetHeaderValue(0).ToString();
                    string stock = _cpstockcode.CodeToName(code).ToString();

                    StockExchange.Instance.MonitorPrices(stock,
                        Convert.ToInt32(Dtb.Rows[Rows - 1][0]),
                        Convert.ToInt32(Dtb.Rows[Rows - 1][1]),
                        Convert.ToInt32(Dtb.Rows[Rows][2]),
                        Convert.ToInt32(Dtb.Rows[Rows][1]));

                    deal_호가_추가();
                }
                finally
                {
                    // End batch update
                    Dgv.ResumeLayout();
                }
            }
        }



        // updated on 20241020, lock, BeginLoadData, EndLoadData added
        private void stockjpbid_Received_new()
        {
            lock (g.lockObject)
            {
                // Begin batch update to minimize UI refreshes
                Dgv.SuspendLayout();

                try
                {

                    if (Rows == 5)
                    {
                        for (int i = 0; i < 5; i++) // Loop for 5 rows
                        {
                            int headerIndex = 3 + (i * 4); // Dynamically calculate header index
                            Dtb.Rows[4 - i][1] = _stockjpbid.GetHeaderValue(headerIndex).ToString();       // Middle column (prices for selling)
                            Dtb.Rows[5 + i][1] = _stockjpbid.GetHeaderValue(headerIndex + 1).ToString(); // Middle column (prices for buying)
                            Dtb.Rows[4 - i][0] = _stockjpbid.GetHeaderValue(headerIndex + 2).ToString(); // Left column (selling amounts)
                            Dtb.Rows[5 + i][2] = _stockjpbid.GetHeaderValue(headerIndex + 3).ToString(); // Right column (buying amounts)
                        }

                        // Handling the last row for the 10th price (if applicable)
                        Dtb.Rows[10][0] = _stockjpbid.GetHeaderValue(23).ToString(); // Last selling amount
                        Dtb.Rows[10][2] = _stockjpbid.GetHeaderValue(24).ToString(); // Last buying amount
                    }

                    else
                    {
                        for (int i = 0; i < 5; i++) // Selling Side
                        {
                            int headerIndex = 19 - (i * 2); // Adjusting the header index dynamically
                            int rowIndex = 9 - i; // Adjusting the DataTable row index for the top 5 rows

                            // Setting the selling price and divided amounts
                            Dtb.Rows[rowIndex][1] = _stockjpbid.GetHeaderValue(headerIndex).ToString();
                            Dtb.Rows[rowIndex][0] = (_stockjpbid.GetHeaderValue(headerIndex - 1).ToString()) + "/" +
                                                    (_stockjpbid.GetHeaderValue(headerIndex - 1).ToString());
                            Dtb.Rows[rowIndex][2] = ""; // Placeholder for extra data (can be updated with magenta "0")
                        }

                        for (int i = 0; i < 5; i++) // Buying Side
                        {
                            int headerIndex = 20 + (i * 2); // Adjusting the header index dynamically
                            int rowIndex = 9 + i; // Adjusting the DataTable row index for the bottom 5 rows

                            // Setting the buying price and divided amounts
                            Dtb.Rows[rowIndex][1] = _stockjpbid.GetHeaderValue(headerIndex).ToString();
                            Dtb.Rows[rowIndex][0] = ""; // Placeholder for selling amounts
                            Dtb.Rows[rowIndex][2] = (_stockjpbid.GetHeaderValue(headerIndex + 1).ToString()) + "/" +
                                                    (_stockjpbid.GetHeaderValue(headerIndex + 1).ToString());
                        }
                    }

                    // Additional logic for monitoring prices, for example
                    string code = _stockjpbid.GetHeaderValue(0).ToString();
                    string stock = _cpstockcode.CodeToName(code).ToString();

                    StockExchange.Instance.MonitorPrices(stock,
                        Convert.ToInt32(Dtb.Rows[Rows - 1][0]),
                        Convert.ToInt32(Dtb.Rows[Rows - 1][1]),
                        Convert.ToInt32(Dtb.Rows[Rows][2]),
                        Convert.ToInt32(Dtb.Rows[Rows][1]));

                    deal_호가_추가();
                }
                finally
                {
                    // End batch update
                    Dgv.ResumeLayout();
                }
            }
        }




        private void request_호가()
        {
            int index = wk.return_index_of_ogldata(Stock);
            if (index < 0)
            {
                return;
            }
            g.stock_data o = g.ogl_data[index];

            _stockjpbid2 = new DSCBO1Lib.StockJpbid2();
            if (_stockjpbid2.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            string stockcode = _cpstockcode.NameToCode(Stock);
            _stockjpbid2.SetInputValue(0, stockcode);

            int result = _stockjpbid2.BlockRequest();

            if (result == 0)
            {
                // Dtb 저장
                Dtb.Rows[2 * Rows][0] = Int32.Parse((_stockjpbid2.GetHeaderValue(4).ToString())); // 매도잔량][ could be 4
                Dtb.Rows[2 * Rows][2] = Int32.Parse((_stockjpbid2.GetHeaderValue(6).ToString())); // 매수잔량][ could be 6

                if (Rows == 5)
                {
                    int IndexSell = 4;
                    int IndexBuy = 5;

                    for (int i = 0; i < int.Parse(_stockjpbid2.GetHeaderValue(1).ToString()) / 2; i++)
                    {
                        Dtb.Rows[IndexSell][1] = _stockjpbid2.GetDataValue(0, i).ToString();
                        Dtb.Rows[IndexBuy][1] = _stockjpbid2.GetDataValue(1, i).ToString();

                        Dtb.Rows[IndexSell][0] = _stockjpbid2.GetDataValue(2, i).ToString();
                        Dtb.Rows[IndexBuy][2] = _stockjpbid2.GetDataValue(3, i).ToString();

                        IndexSell -= 1;
                        IndexBuy += 1;
                    }
                }
                else
                {
                    int IndexSell = 9;
                    int IndexBuy = 10;

                    for (int i = 0; i < Int32.Parse((_stockjpbid2.GetHeaderValue(1).ToString())); i++)
                    {
                        Dtb.Rows[IndexSell][1] = _stockjpbid2.GetDataValue(0, i).ToString();
                        Dtb.Rows[IndexBuy][1] = _stockjpbid2.GetDataValue(1, i).ToString();

                        Dtb.Rows[IndexSell][0] = _stockjpbid2.GetDataValue(2, i).ToString();
                        Dtb.Rows[IndexBuy][2] = _stockjpbid2.GetDataValue(3, i).ToString();

                        IndexSell -= 1;
                        IndexBuy += 1;
                    }
                }
                // 한 호가 차이 슬리피지 % 계산
                int valup = (int)_stockjpbid2.GetDataValue(0, 0);
                int valdn = (int)_stockjpbid2.GetDataValue(1, 0);

                // 일반 - 프누억
                if (g.KODEX4.Contains(Stock))
                {
                    Dtb.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##"); ;
                    Dtb.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##"); ;
                }
                else
                {
                    Dtb.Rows[Rows - 1][2] = (o.프누천 / 10.0).ToString("0.##");
                }

                if(MathUtils.IsSafeToDivide(valdn))
                {
                    Dtb.Rows[2 * Rows]["호가"] = ((valup - valdn) / (double)valdn * 100.0).ToString("0.##");
                }
                else
                {
                    Dtb.Rows[2 * Rows]["호가"] = "No Data";
                }
            }

            if (o.보유량 > 0)
            {
                if (o.매수1호가 > 0)
                    o.수익률 = (o.매수1호가 - o.장부가) / (double)o.매수1호가 * 100;
            }
            Dtb.Rows[2 * Rows + 1][0] = o.stock;
            Dtb.Rows[2 * Rows + 1][1] = o.일간변동평균편차;
            Dtb.Rows[2 * Rows + 1][2] = o.보유량.ToString() + "/" + o.수익률.ToString("F2");

            deal_호가_추가(); //\
        }

        private void deal_호가_추가()
        {
            int index = wk.return_index_of_ogldata(Stock);
            if (index < 0)
            {
                return;
            }
            g.stock_data o = g.ogl_data[index];

            int divider = 10;
            for (int i = 0; i < Rows; i++)
            {
                Dtb.Rows[i + Rows][0] = ((o.틱매수량[i] - o.틱매수량[i + 1]) / divider).ToString() +
                                                     "/" + ((o.틱매도량[i] - o.틱매도량[i + 1]) / divider).ToString();
            }
            for (int i = 0; i < Rows; i++) // ERR 20230228, 틱프돈백 ? 틱매수배 / 틱매도배 불필요 ?
            {
                Dtb.Rows[i][2] = o.틱프로천[i].ToString("F0") + "+" + o.틱외인천[i].ToString("F0") + "/" +
                                            o.틱거래천[i].ToString("F0");
            }

            o.틱의시간[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            o.매도1호가 = Convert.ToInt32(Dtb.Rows[Rows - 1][1].ToString());
            o.매수1호가 = Convert.ToInt32(Dtb.Rows[Rows][1].ToString());




            if (!int.TryParse(Dtb.Rows[Rows - 1][0].ToString(), out o.최우선매도호가잔량))
            {
                o.최우선매도호가잔량 = 0;
            }
            if (!int.TryParse(Dtb.Rows[Rows][2].ToString(), out o.최우선매수호가잔량))
            {
                o.최우선매수호가잔량 = 0;
            }


            if (o.전일종가 > 0)
                o.틱의가격[0] = (int)((o.매수1호가 - (int)o.전일종가) * 10000.0 / o.전일종가);

            double factor = 0.0;
            double differ = 0.0;
            if (o.전일종가 > 0)
                differ = (o.매도1호가 - o.매수1호가) * 10000.0 / o.전일종가;
            if (o.최우선매도호가잔량 + o.최우선매수호가잔량 > 0)
                factor = (double)o.최우선매수호가잔량 / (o.최우선매도호가잔량 + o.최우선매수호가잔량);
            o.틱의가격[0] += (int)(differ * factor);
            o.가격 = o.틱의가격[0];

            o.틱매도잔[0] = o.최우선매도호가잔량;
            o.틱매수잔[0] = o.최우선매수호가잔량;

            // 프누억
            if (Stock.Contains("KODEX"))
            {
                Dtb.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##"); ;
                Dtb.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##"); ;
            }
            else
            {
                Dtb.Rows[Rows - 1][2] = (o.프누천 / 10.0).ToString("0.##");
            }

            // 한 호가 차이 슬리피지 % 계산
            if (o.매수1호가 > 0)
            {
                Dtb.Rows[2 * Rows][1] = ((o.매도1호가 - o.매수1호가) / (double)o.매수1호가 * 100.0).ToString("0.##");
            }
            else
            {
                Dtb.Rows[2 * Rows][1] = "No Data";
            }



            if (o.보유량 > 0)
            {
                if (o.매수1호가 > 0)
                {
                    o.수익률 = (o.매수1호가 - o.장부가) / (double)o.매수1호가 * 100;
                }
            }
            Dtb.Rows[2 * Rows + 1][0] = o.stock;
            Dtb.Rows[2 * Rows + 1][1] = o.일간변동평균편차;
            Dtb.Rows[2 * Rows + 1][2] = o.보유량.ToString() + "/" + o.수익률.ToString("F2");


            for (int i = 0; i < 2 * Rows - 2; i++)
            {
                Dgv.Rows[i].Cells[1].Style.BackColor = Color.White; // red
            }


            // upperPassingPrice
            if (o.pass.upperPassingPrice > 0)
            {
                if (o.매도1호가 >= o.pass.upperPassingPrice)
                {
                    mc.Sound("일반", "passing upper"); // StopLoss[1]
                    o.pass.upperPassingPrice = 0;
                }
            }

            // lowerPassingPrice
            if (o.pass.lowerPassingPrice > 0)
            {
                if (o.매수1호가 <= o.pass.lowerPassingPrice)
                {
                    mc.Sound("일반", "passing lower"); // Stop Loss[0]
                    o.pass.lowerPassingPrice = 0;
                }
            }


            // MiddleCenter : need double loops, not by a single line
            foreach (DataGridViewRow row in Dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }

            Color[,] cellColors = new Color[2 * Rows + 2, 3];
            Color ColorLightRed = Color.FromArgb(255, 204, 204); // LightRed

            cellColors[Rows - 1, 2] = Color.LightGreen;
            if (g.KODEX4.Contains(Stock))
            {
                cellColors[Rows - 2, 2] = Color.LightGreen;
            }


            if (!g.confirm_sell)
            {
                // column 0
                for (int i = 0; i < Rows; i++)
                {
                    cellColors[i, 0] = Color.Cyan;
                }
            }

            if (g.optimumTrading)
            {
                // column 0
                for (int i = Rows; i < 2 * Rows; i++)
                {
                    cellColors[i, 0] = Color.Yellow;
                }
                // column 2
                int row = g.KODEX4.Contains(Stock) ? Rows - 2 : Rows - 1;
                for (int i = 0; i < row; i++)
                {
                    cellColors[i, 2] = Color.Yellow;
                }
            }


            //  Coloring upperPassingPrice in column 1
            if (o.pass.upperPassingPrice > 0 || o.pass.lowerPassingPrice > 0)
            {
                for (int i = 0; i < 2 * Rows; i++)
                {
                    if (int.TryParse(Dtb.Rows[i][1].ToString(), out int price))
                    {
                        if (price == o.pass.upperPassingPrice || price == o.pass.lowerPassingPrice)
                        {
                            cellColors[i, 1] = Color.Yellow;
                        }
                    }
                }
            }

            // Coloring buyOrder and sellOrder with Quantity
            var ordersWithStock = StockExchange.buyOrders
                .Where(order => order.Stock == Stock)
                .Concat(StockExchange.sellOrders.Where(order => order.Stock == Stock))
                .ToList();

            foreach (var order in ordersWithStock)
            {
                for (int i = 0; i < 2 * Rows; i++)
                {
                    if (int.TryParse(Dtb.Rows[i][1].ToString(), out int price) && price == order.Price)
                    {
                        if (StockExchange.buyOrders.Contains(order))
                        {
                            Dtb.Rows[i][2] = order.Quantity.ToString();
                            cellColors[i, 2] = Color.Red;
                        }
                        else if (StockExchange.sellOrders.Contains(order))
                        {
                            Dtb.Rows[i][0] = order.Quantity.ToString();
                            cellColors[i, 0] = Color.Red;
                        }
                    }
                }
            }

            // Finally apply the assigned colors to each cessl of Dgv
            for (int i = 0; i < 2 * Rows; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Dgv.Rows[i].Cells[j].Style.BackColor = cellColors[i, j];
                }
            }
        }

        private void OnDataGirdView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ky.chart_keypress(e);
        }

        private int RemainSB()
        {
            _cpcybos = new CPUTILLib.CpCybos();
            if (_cpcybos == null)
                return 400;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_SUBSCRIBE);               // 400건의 요청으로 제한   
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Log the error details to a file or logging system
            LogError(e.Exception);

            // Prevent the default error dialog from showing
            e.ThrowException = false;
        }

        // Method to log error details
        private void LogError(Exception ex)
        {
            // Example logging to a text file
            string filePath = @"C:\병신\LogFile.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"[{DateTime.Now}] An error occurred: {ex.Message}");
                writer.WriteLine(ex.StackTrace);
            }
        }
    }
}


