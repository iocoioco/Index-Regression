using CPTRADELib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static New_Tradegy.Library.g.stock_data;

namespace New_Tradegy.Library
{
    public class Order
    {
        public string Stock { get; set; }
        public int Price { get; set; }
        public bool WasUpper { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderTime { get; set; }
        public int UrgencyLevel { get; set; }
        public TimeSpan CancelThreshold { get; set; }

        public Order(string stock, int price, bool wasUpper, int quantity, DateTime orderTime, int urgencyLevel, TimeSpan cancelThreshold)
        {
            Stock = stock;
            Price = price;
            WasUpper = wasUpper;
            Quantity = quantity;
            OrderTime = orderTime;
            UrgencyLevel = urgencyLevel;
            this.CancelThreshold = cancelThreshold;
        }
    }

    public class StockExchange
    {
        private static CPTRADELib.CpTd0311 _cptd0311; //주문(현금 주문) 데이터를 요청
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        private static CPTRADELib.CpTdUtil _cptdutil;
        private static bool _checkedTradeInit = false;

        public static string accountNo;
        public static string accountGoodsStock;
        public static string[] arrAccountGoodsStock;

        public static List<Order> buyOrders = new List<Order>();
        public static List<Order> sellOrders = new List<Order>();

        public static void TradeInit()
        {
            if (_checkedTradeInit)
                return;

            _cptdutil = new CpTdUtil();

            int rv = _cptdutil.TradeInit(0);
            if (rv == 0)
            {
                _checkedTradeInit = true;

                string[] arrAccount = (string[])_cptdutil.AccountNumber;
                if (arrAccount.Length > 0)
                {
                    accountNo = arrAccount[0];

                    arrAccountGoodsStock = (string[])_cptdutil.get_GoodsList(accountNo, CPE_ACC_GOODS.CPC_STOCK_ACC);    // 주식
                    if (arrAccountGoodsStock.Length > 0)
                        accountGoodsStock = arrAccountGoodsStock[0];
                }
            }
            else if (rv == -1)
            {
                MessageBox.Show("계좌 비밀번호 오류 포함 에러.");
                _checkedTradeInit = false;
            }
            else if (rv == 1)
            {
                MessageBox.Show("OTP/보안카드키가 잘못되었습니다.");
                _checkedTradeInit = false;
            }
            else
            {
                MessageBox.Show("Error");
                _checkedTradeInit = false;
            }
        }

        public void MonitorPrices(string stock, int sellHogaVolume, int sellHogaPrice, int buyHogaVolume, int buyHogaPrice)
        {
            // buyOrders
            foreach (var order in new List<Order>(buyOrders))
            {
                if (order.Stock == stock && order.Price == sellHogaPrice)
                {
                    string str = "";
                    str += order.Stock + " : " + order.Price.ToString() + " X " + order.Quantity.ToString() +
                                       " = " + (order.Price * order.Quantity / 10000).ToString() + "만원";
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                    {
                        return;
                    }
                    str += "\n" + sr.r3_display_매수_매도(g.ogl_data[index]);

                    RemoveOrder(buyOrders, order);

                    using (var form = new Form_매수_매도(stock, "매수 ?", order.UrgencyLevel, g.cancelThreshhold, str))
                    {
                        DialogResult result = form.ShowDialog();
                        if (result == DialogResult.Yes) // 시장가
                        {
                            dl.deal_exec("매수", order.Stock, order.Quantity, order.Price, "03");
                        }
                        if (result == DialogResult.OK) // 지정가
                        {

                            dl.deal_exec("매수", order.Stock, order.Quantity, order.Price, "01");
                        }
                    }
                }
            }

            // sellOrders
            foreach (var order in new List<Order>(sellOrders))
            {
                if (order.Stock == stock && order.Price == buyHogaPrice)
                {
                    string str = "";
                    str += order.Stock + " : " + order.Price.ToString() + " X " + order.Quantity.ToString() +
                                       " = " + (order.Price * order.Quantity / 10000).ToString() + "만원";
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                    {
                        return;
                    }
                    str += "\n" + sr.r3_display_매수_매도(g.ogl_data[index]);

                    RemoveOrder(sellOrders, order);

                    using (var form = new Form_매수_매도(stock, "매도 ?", order.UrgencyLevel, g.cancelThreshhold, str))
                    {
                        DialogResult result = form.ShowDialog();
                        if (result == DialogResult.Yes) // 시장가
                        {
                            dl.deal_exec("매도", order.Stock, order.Quantity, order.Price, "03");
                        }
                        if (result == DialogResult.OK) // 지정가
                        {

                            dl.deal_exec("매도", order.Stock, order.Quantity, order.Price, "01");
                        }
                    }
                }
            }



            #region

            //DateTime now = DateTime.Now;

            //    // 매수 실행
            //    foreach (var order in new List<Order>(buyOrders))
            //    {
            //        double volumeRatio = (double)sellHogaVolume / (buyHogaVolume + sellHogaVolume);
            //        double buyThreshold = order.UrgencyLevel / 100.0;

            //        if ((now - order.OrderTime) >= order.CancelThreshold)
            //        {
            //            RemoveOrder(buyOrders, order);
            //        }
            //        // order.Price higher than sellHogaPrice, Sudden drop of sellHogaPrice below order.Price
            //        // cancell the order
            //        else if (order.Stock == stock && sellHogaPrice < order.Price)
            //        {
            //            RemoveOrder(buyOrders, order);
            //        }
            //        else if (order.Stock == stock && sellHogaPrice == order.Price)
            //        {
            //            if (IsHogaGapAcceptable(sellHogaPrice, buyHogaPrice))
            //            {
            //                if (volumeRatio <= buyThreshold)
            //                {
            //                    ExecuteOrder("Buy", order);
            //                    RemoveOrder(buyOrders, order);
            //                }
            //            }
            //        }
            //    }

            //    // 매도 실행
            //    foreach (var order in new List<Order>(sellOrders))
            //    {
            //        double volumeRatio = (double)buyHogaVolume / (buyHogaVolume + sellHogaVolume);
            //        double sellThreshold = order.UrgencyLevel / 100.0;

            //        if (IsSuddenDrop(sellHogaPrice, buyHogaPrice)) // 매수호가  & 매도호가 Gap Occurs
            //        {
            //            order.UrgencyLevel = 100;
            //        }

            //        if ((now - order.OrderTime) >= order.CancelThreshold)
            //        {
            //            RemoveOrder(sellOrders, order);
            //        }
            //        else if (order.Stock == stock && buyHogaPrice <= order.Price) // 주문가격 <= 매수호가
            //        {
            //            if (order.UrgencyLevel == 100 || volumeRatio <= sellThreshold)
            //            {
            //                ExecuteOrder("Sell", order);
            //                RemoveOrder(sellOrders, order);
            //            }
            //        }
            //    }
            #endregion
        }

        public void AddBuyOrder(string stock, int price, bool wasUpper, int quantity, DateTime orderTime, int urgencyLevel, TimeSpan cancelThreshold)
            {
                // Find the existing order with the same stock and price
                var existingOrder = buyOrders.Find(order => order.Stock == stock && order.Price == price);

                // If such an order exists, remove it
                if (existingOrder != null)
                {
                    buyOrders.Remove(existingOrder);
                }

                buyOrders.Add(new Order(stock, price, wasUpper, quantity, orderTime, urgencyLevel, cancelThreshold));
            }

            public void AddSellOrder(string stock, int price, bool wasUpper, int quantity, DateTime orderTime, int urgencyLevel, TimeSpan cancelThreshold)
            {
                // Find the existing order with the same stock and price
                var existingOrder = sellOrders.Find(order => order.Stock == stock && order.Price == price);

                // If such an order exists, remove it
                if (existingOrder != null)
                {
                    sellOrders.Remove(existingOrder);
                }

                sellOrders.Add(new Order(stock, price, wasUpper, quantity, orderTime, urgencyLevel, cancelThreshold));
            }

            private bool IsSuddenDrop(int sellHogaPrice, int buyHogaPrice)
            {
                return (sellHogaPrice - buyHogaPrice) > (int)TickDifference(sellHogaPrice);
            }

            private void ExecuteOrder(string buyorsell, Order order)
            {
                string 주문조건구분 = "0"; // 없음
                                     // 주문호가구분 = "01"; // 지정가, "03" 시장가

                TradeInit();
                if (_checkedTradeInit == false)
                    return;

                if (order.Quantity == 0)
                    return;

                string stockcode = _cpstockcode.NameToCode(order.Stock);
                _cptd0311 = new CPTRADELib.CpTd0311();

                if (_cptd0311.GetDibStatus() == 1)
                    Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");

                if (buyorsell == "Buy")
                {
                    _cptd0311.SetInputValue(0, "2");    // 2:매수
                }
                else
                {
                    _cptd0311.SetInputValue(0, "1");    // 1:매도
                }

                _cptd0311.SetInputValue(1, g.Account);  // 계좌번호
                _cptd0311.SetInputValue(2, "01");     // 상품구분코드
                _cptd0311.SetInputValue(3, stockcode);  // 종목코드
                _cptd0311.SetInputValue(4, order.Quantity);  // 주문수량 
                _cptd0311.SetInputValue(5, order.Price);    // 매수단가
                _cptd0311.SetInputValue(7, 주문조건구분);  // 주문조건구분("0":없음,"1":IOC, "2":FOK)
                _cptd0311.SetInputValue(8, "01");    // 주문호가구분("01":보통,"02":임의,"03":시장가,"05":조건부지정가 etc)

                //string sRQName = order.Stock + " " + buyorsell + " " + order.Price + " " + order.Quantity+ " " + order.UrgencyLevel + " " + order.CancelThreshold;
                //MessageBox.Show(sRQName);
                int check = _cptd0311.BlockRequest();
            }

            private int TickDifference(double x)
            {
                if (x < 2000)
                {
                    return 1;
                }
                else if (x < 5000)
                {
                    return 5;
                }
                else if (x < 20000)
                {
                    return 10;
                }
                else if (x < 50000)
                {
                    return 50;
                }
                else if (x < 200000)
                {
                    return 100;
                }
                else if (x < 500000)
                {
                    return 500;
                }
                else
                {
                    return 1000;
                }
            }

            private bool IsHogaGapAcceptable(int sellHogaPrice, int buyHogaPrice)
            {
                double priceDifference = Math.Abs(sellHogaPrice - buyHogaPrice);
                int allowedTickDifference = TickDifference(Math.Min(sellHogaPrice, buyHogaPrice));
                return priceDifference <= allowedTickDifference;
            }

            private void RemoveOrder(List<Order> orderList, Order order)
            {
                orderList.Remove(order);
            }
        }

        class jp
        {
            private CPUTILLib.CpCybos _cpcybos;
            private CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
            private DSCBO1Lib.StockJpbid _stockjpbid;
            private DSCBO1Lib.StockJpbid2 _stockjpbid2;

            //public DataTable Dtb { get; set; }
            private DataTable Dtb = new DataTable();
            private DataGridView Dgv = new DataGridView();
            private int Rows;
            private string Stock;
            private Form F;
            private int RowID, ColId;

            StockExchange stockExchange = new StockExchange();

            public void Generate(string stock, Form f, int rows, int rowId, int colId)
            {
                if (!g.connected)
                {
                    return;
                }

                Stock = stock;
                Rows = rows;
                RowID = rowId;
                ColId = colId;

                F = f;

                int w0 = 61;
                int w1 = 50;
                int w2 = 61;
                int ch = 28;

                Dtb.Columns.Add("매도");
                Dtb.Columns.Add("호가");
                Dtb.Columns.Add("매수");

                for (int i = 0; i < 2 * Rows + 2; i++)
                {
                    Dtb.Rows.Add("", "", "");
                }


                string stockcode = _cpstockcode.NameToCode(stock);
                _stockjpbid = new DSCBO1Lib.StockJpbid();
                _stockjpbid.SetInputValue(0, stockcode);
                _stockjpbid.Received +=
                    new DSCBO1Lib._IDibEvents_ReceivedEventHandler(stockjpbid_Received);

                _stockjpbid.Subscribe();


                if (!g.jpjds.ContainsKey(stock))
                {
                    g.jpjds.Add(stock, _stockjpbid);
                }
                else
                {
                    return;
                }

                Dgv.DataError += (s, e) => wr.DataGridView_DataError(s, e, "jpjd Dgv");
                Dgv.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);
                Dgv.Location = new Point(0, 0);
                Dgv.Size = new Size(w0 + w1 + w2, F.Height);

                Dgv.Name = stock;


                Dgv.DataSource = Dtb;
                Dgv.ColumnHeadersVisible = false;
                Dgv.RowHeadersVisible = false;
                int fontsize = 9;

                int CellHeight = ch;
                Dgv.DefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
                Dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", fontsize, FontStyle.Bold);
                Dgv.RowTemplate.Height = CellHeight;
                Dgv.ForeColor = Color.Black;
                Dgv.ScrollBars = ScrollBars.None;
                Dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                Dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                Dgv.AllowUserToResizeColumns = false;
                Dgv.AllowUserToResizeRows = false;

                Dgv.AllowUserToAddRows = false;
                Dgv.AllowUserToDeleteRows = false;
                Dgv.Dock = System.Windows.Forms.DockStyle.Fill;

                Dgv.Name = stock;
                Dgv.ReadOnly = true;
                Dgv.RowHeadersVisible = false;
                Dgv.ColumnHeadersVisible = false;
                Dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                Dgv.TabIndex = 1;

                Dgv.CellMouseClick += new DataGridViewCellMouseEventHandler(Dgv_CellMouseClick);

                Dgv.PreviewKeyDown += new PreviewKeyDownEventHandler(Dgv_PreviewKeyDown);

                Dgv.KeyPress += OnDataGirdView1_KeyPress;

                f.Controls.Add(Dgv);

                Dgv.DataSource = Dtb;

                f.TopMost = true;

                Dgv.Columns[0].Width = w0;
                Dgv.Columns[1].Width = w1;
                Dgv.Columns[2].Width = w2;

                request_호가();
            }

            private void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right) // 실수로 오른쪽 눌리는 경우 발생하여 추가
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

                switch (e.ColumnIndex)
                {
                    case 0: // 매도
                        if (e.RowIndex >= 2 * Rows)
                        {
                            return;
                        }
                        var existingOrder = StockExchange.sellOrders.Find(order => order.Stock == Stock && order.Price == Price);

                        // If such an order exists, remove it
                        if (existingOrder != null)
                        {
                            StockExchange.sellOrders.Remove(existingOrder);
                            return;
                        }

                        bool WasUpper = false;
                        if (e.RowIndex < Rows)
                            WasUpper = true;

                        int Amount = g.일회거래액 * 10000 / Price;
                        if (Amount == 0)
                            Amount = 1;

                        if (o.보유량 < Amount)
                        {
                            dl.DealCancelStock(Stock); // dgv_CellClick tr(2) 에러 메세지 20231122
                            dl.deal_hold(); // dgv_CellClick tr(1)
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

                        ms.Sound_돈(g.일회거래액);
                        int Urgency = (g.optimumTrading) ? (int)(e.X / (double)Dgv.Columns[0].Width * 100) : 100;


                        string str = "";
                        if (g.confirm_sell)  // "03" <- 시장가 매도 ... 비상매도
                        {
                            str += Stock + " : " + Price.ToString() + " X " + Amount.ToString() +
                                       " = " + (Price * Amount / 10000).ToString() + "만원";

                            // dr.draw_보조_차트("상관");
                            using (var form = new Form_매수_매도(Stock, "매도 ?", Urgency, g.cancelThreshhold, str))
                            {
                                DialogResult result = form.ShowDialog();
                                if (result == DialogResult.Yes) // 시장가
                                {
                                    dl.deal_exec("매도", Stock, Amount, Price, "03");
                                }
                                else if (result == DialogResult.OK) // 지정가
                                {
                                    if (e.RowIndex == Rows - 1 || e.RowIndex == Rows)
                                    {
                                        dl.deal_exec("매도", Stock, Amount, Price, "01");
                                    }
                                    else
                                    {
                                        Urgency = form.The_urgency;
                                        g.cancelThreshhold = form.The_cancelthreshhold;
                                        stockExchange.AddSellOrder(Stock, Price, WasUpper, Amount, DateTime.Now, Urgency, TimeSpan.FromMinutes(382));
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (e.RowIndex == Rows - 1 || e.RowIndex == Rows)
                            {
                                dl.deal_exec("매도", Stock, Amount, Price, "01");
                            }
                            else
                            {
                                // Urgency and TimeThresh NF
                                stockExchange.AddSellOrder(Stock, Price, WasUpper, Amount, DateTime.Now, Urgency, TimeSpan.FromMinutes(382));
                            }
                        }
                        break;

                    case 1:
                        // Passing Price
                        if (e.RowIndex < Rows)
                        {
                            if (Price == o.deal.upperPassingPrice)
                            {
                                o.deal.upperPassingPrice = 0;
                            }
                            else
                            {
                                o.deal.upperPassingPrice = Price;
                            }
                        }
                        else if (e.RowIndex < 2 * Rows)
                        {
                            if (Price == o.deal.lowerPassingPrice)
                            {
                                o.deal.lowerPassingPrice = 0;
                            }
                            else
                            {
                                o.deal.lowerPassingPrice = Price;
                            }
                        }
                        // 5 & 10 hoga toggle
                        else
                        {
                            if (g.KODEX4.Contains(Stock))
                            {
                                return;
                            }
                            if (Rows == 5)
                            {
                                Unsubscribe();
                                hg.HogaInsert(Stock, 10, RowID, ColId);
                            }
                            else
                            {
                                Unsubscribe();
                                hg.HogaInsert(Stock, 5, RowID, ColId);
                            }
                        }
                        dr.draw_chart();
                        break;

                    case 2: // 매수
                        if (e.RowIndex >= 2 * Rows)
                        {
                            return;
                        }
                        existingOrder = StockExchange.buyOrders.Find(order => order.Stock == Stock && order.Price == Price);

                        // If such an order exists, remove it
                        if (existingOrder != null)
                        {
                            StockExchange.buyOrders.Remove(existingOrder);
                            return;
                        }


                        WasUpper = false;
                        if (e.RowIndex < Rows)
                            WasUpper = true;

                        Amount = g.일회거래액 * 10000 / Price;
                        if (Amount == 0)
                        {
                            Amount = 1;
                        }

                        ms.Sound_돈(g.일회거래액);
                        Urgency = (g.optimumTrading) ? (int)(e.X / (double)Dgv.Columns[0].Width * 100) : 100;

                        str = "";
                        if (g.confirm_buy)
                        {
                            str += Stock + " : " + Price.ToString() + " X " + Amount.ToString() +
                                       " = " + (Price * Amount / 10000).ToString() + "만원";

                            str += "\n" + sr.r3_display_매수_매도(o);


                            using (var form = new Form_매수_매도(Stock, "매수 ?", Urgency, g.cancelThreshhold, str))
                            {
                                DialogResult result = form.ShowDialog();
                                if (result == DialogResult.Yes) // 시장가
                                {
                                    dl.deal_exec("매수", Stock, Amount, Price, "03");
                                }
                                else if (result == DialogResult.OK) // 지정가
                                {
                                    if (e.RowIndex == Rows - 1 || e.RowIndex == Rows)
                                    {
                                        dl.deal_exec("매수", Stock, Amount, Price, "01");
                                    }
                                    else
                                    {
                                        Urgency = form.The_urgency;
                                        g.cancelThreshhold = form.The_cancelthreshhold;
                                        stockExchange.AddBuyOrder(Stock, Price, WasUpper, Amount, DateTime.Now, Urgency, TimeSpan.FromMinutes(382));
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        break;
                }

            }

            private void Unsubscribe()
            {
                //string stockcode = _cpstockcode.NameToCode(Stock);
                //_stockjpbid.SetInputValue(0, stockcode);
                Dgv.Dispose();
                F.Close();
                _stockjpbid.Unsubscribe();
                g.jpjds.Remove(Stock);
            }

            private void stockjpbid_Received()
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
                string code = _stockjpbid.GetHeaderValue(0).ToString();
                string stock = _cpstockcode.CodeToName(code).ToString();

                stockExchange.MonitorPrices(Stock, Convert.ToInt32(Dtb.Rows[Rows - 1][0]), Convert.ToInt32(Dtb.Rows[Rows - 1][1]),
                                                   Convert.ToInt32(Dtb.Rows[Rows][2]), Convert.ToInt32(Dtb.Rows[Rows][1]));

                deal_호가_추가();
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
                        Dtb.Rows[Rows - 2][2] = (g.코스피지수 / 100.0).ToString("0.##"); ;
                        Dtb.Rows[Rows - 1][2] = (g.코스닥지수 / 100.0).ToString("0.##"); ;
                    }
                    else
                    {
                        Dtb.Rows[Rows - 1][2] = (o.프누천 / 10.0).ToString("0.##");
                    }


                    if (valdn > g.EPS)
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
                Dtb.Rows[2 * Rows + 1][1] = o.dev_avr;
                Dtb.Rows[2 * Rows + 1][2] = o.보유량.ToString() + "/" + o.수익률.ToString("F2");

                deal_호가_추가(); //?
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
                    Dtb.Rows[Rows - 2][2] = (g.코스피지수 / 100.0).ToString("0.##"); ;
                    Dtb.Rows[Rows - 1][2] = (g.코스닥지수 / 100.0).ToString("0.##"); ;
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
                Dtb.Rows[2 * Rows + 1][1] = o.dev_avr;
                Dtb.Rows[2 * Rows + 1][2] = o.보유량.ToString() + "/" + o.수익률.ToString("F2");


                for (int i = 0; i < 2 * Rows - 2; i++)
                {
                    Dgv.Rows[i].Cells[1].Style.BackColor = Color.White; // red
                }


                // upperPassingPrice
                if (o.deal.upperPassingPrice > 0)
                {
                    if (o.매도1호가 >= o.deal.upperPassingPrice)
                    {
                        ms.Sound("일반", "passing upper"); // StopLoss[1]
                        o.deal.upperPassingPrice = 0;
                    }
                }

                // lowerPassingPrice
                if (o.deal.lowerPassingPrice > 0)
                {
                    if (o.매수1호가 <= o.deal.lowerPassingPrice)
                    {
                        ms.Sound("일반", "passing lower"); // Stop Loss[0]
                        o.deal.lowerPassingPrice = 0;
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
                if (o.deal.upperPassingPrice > 0 || o.deal.lowerPassingPrice > 0)
                {
                    for (int i = 0; i < 2 * Rows; i++)
                    {
                        if (int.TryParse(Dtb.Rows[i][1].ToString(), out int price))
                        {
                            if (price == o.deal.upperPassingPrice || price == o.deal.lowerPassingPrice)
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

            private void Dgv_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                //ky.char1_previewkeydown(sender, e);
                return;
                switch (e.KeyCode)
                {
                    #region
                    // 매도(ActiveForm)
                    case Keys.F5:
                        //Form f = cl.GetActiveForm();
                        //if (f == null)
                        //{
                        //    return;
                        //}

                        int Price = hg.HogaGetValue(Stock, 0, 1); // 0 : 매수1호가 라인, 1 : 호가 column
                        if (Price <= 0)
                        {
                            return;
                        }

                        //string Stock = f.Name;
                        int index = wk.return_index_of_ogldata(Stock);
                        if (index < 0)
                        {
                            return;
                        }
                        g.stock_data o = g.ogl_data[index];

                        int Amount = g.일회거래액 * 10000 / Price;
                        ms.Sound_돈(g.일회거래액);

                        if (o.보유량 < Amount)
                        {
                            dl.DealCancelStock(Stock); // dgv_CellClick tr(2) 에러 메세지 20231122
                            dl.deal_hold(); // dgv_CellClick tr(1)
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

                        dl.deal_exec("매도", Stock, Amount, Price, "03"); // dgv_PreviewKeyDow F1(F5) "01"; 지정가, "03" 시장가
                        break;

                    case Keys.F9:
                        Form Form_무게 = new Form_그룹(); // grup
                        Form_무게.Show();
                        break;

                        #endregion
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


