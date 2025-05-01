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
using System.Linq;
using System.Windows.Forms;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.IO;

namespace New_Tradegy.Library.Listeners
{
    class BookBidGenerator
    {
        private CPUTILLib.CpCybos _cybos;
        private CPUTILLib.CpStockCode _stockCodeService = new CPUTILLib.CpStockCode();
        private DSCBO1Lib.StockJpbid _jpbidPrimary;
        private DSCBO1Lib.StockJpbid2 _jpbidSecondary;

        private DataTable _dataTable;
        private DataGridView _dataGridView;
        private int Rows = 5;
        private string _stockName;

        // Sensei 20250420
        public DataGridView GenerateBookBidView(string stockName)
        {
            if (g.test)
                return null;

            _stockName = stockName;

            int w0 = 61, w1 = 50, w2 = 61;
            int cellHeight = 27;

            _dataTable = new DataTable();
            _dataTable.Columns.Add("매도");
            _dataTable.Columns.Add("호가");
            _dataTable.Columns.Add("매수");

            for (int i = 0; i < 2 * Rows + 2; i++)
                _dataTable.Rows.Add("", "", "");

            _dataGridView = new DataGridView
            {
                DataSource = new BindingSource { DataSource = _dataTable },
                Name = _stockName,
                Location = new Point(120, 0), // temporary location
                Size = new Size(w0 + w1 + w2, cellHeight * 13),
                Dock = DockStyle.None,
                TabIndex = 1
            };

            // Setup grid using utility method
            GridUtils.SetupBasicGrid(
                _dataGridView,
                rowHeight: cellHeight,
                fontName: "Arial",
                fontSize: 9,
                bold: true,
                showColumnHeaders: false,
                showRowHeaders: false,
                columnWidths: new[] { w0, w1, w2 }
            );

            // Data error handling
            _dataGridView.DataError += (s, e) => StockFileExporter.DataGridView_DataError(s, e, "jpjd _dataGridView");
            _dataGridView.DataError += new DataGridViewDataErrorEventHandler(OnDataError);
            _dataGridView.CellMouseClick += new DataGridViewCellMouseEventHandler(OnCellMouseClick);

            // Subscription
            string stockcode = _stockCodeService.NameToCode(_stockName);
            _jpbidPrimary = new DSCBO1Lib.StockJpbid();
            _jpbidPrimary.SetInputValue(0, stockcode);
            _jpbidPrimary.Received += new DSCBO1Lib._IDibEvents_ReceivedEventHandler(OnBookBidReceived);
            _jpbidPrimary.Subscribe();

            if (g.BookBidInstances.ContainsKey(_stockName))
                return null;

            g.BookBidInstances.TryAdd(_stockName, _jpbidPrimary);

            g.MainForm.Controls.Add(_dataGridView);
            _dataGridView.BringToFront();

            RequestQuote();

            return _dataGridView;
        }


        // Sensei 20250420
        public void OpenOrUpdateConfirmationForm(bool isSell, string stockName, int Amount, int price, int Urgency, string str)
        {
            Form_매수_매도 f = GetOpenTradeForm(stockName);
            if (f != null)
            {
                if (f._isSell == isSell)
                {
                    Utils.SoundUtils.Sound("", "not sold");
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

        // Sensei 20250420
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

        // Sensei 20250420
        private void OnCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            g.ActiveBookBid = sender as DataGridView;

            if (e.Button == MouseButtons.Right)
                return;

            int price = GetClickedPrice(_dataGridView, e);
            if (price == 0)
                return; // Skip if no valid price

            if (!g.StockManager.Repository.TryGet(_stockName, out var stockData))
                return;

            // Column 1: Toggle passing price or toggle hoga depth
            if (e.ColumnIndex == 1)
            {
                if (e.RowIndex < Rows)
                {
                    stockData.Pass.upperPassingPrice = (price == stockData.Pass.upperPassingPrice) ? 0 : price;
                }
                else if (e.RowIndex <= Rows)
                {
                    stockData.Pass.lowerPassingPrice = (price == stockData.Pass.lowerPassingPrice) ? 0 : price;
                }
                else if (e.ColumnIndex <= Rows * 2 && !g.StockManager.IndexList.Contains(_stockName))
                {
                    Rows = (Rows == 5) ? 10 : 5;
                }
                return;
            }

            // Outside hoga range — ignore
            if (e.RowIndex >= Rows * 2)
                return;

            bool isSell = (e.ColumnIndex == 0);
            bool isBuy = (e.ColumnIndex == 2);

            if (isBuy)
            {
                var existingOrder = StockExchange.buyOrders
                    .Find(order => order.Stock == _stockName && order.Price == price);
                if (existingOrder != null)
                    StockExchange.buyOrders.Remove(existingOrder);

                int amount = g.일회거래액 * 10000 / price;
                if (amount == 0)
                    amount = 1;

                Utils.SoundUtils.Sound_돈(g.일회거래액);

                int urgency = (g.optimumTrading)
                    ? (int)(e.X / (double)_dataGridView.Columns[2].Width * 100)
                    : 100;

                string msg = $"{_stockName} : {price} X {amount} = {(price * amount / 10000)}만원\n";
                msg += StringUtils.r3_display_매수_매도(stockData);

                if (DealManager.CheckPreviousLoss(_stockName))
                    return;

                OpenOrUpdateConfirmationForm(isSell, _stockName, amount, price, urgency, msg);
            }
            else if (isSell)
            {
                var existingOrder = StockExchange.sellOrders
                    .Find(order => order.Stock == _stockName && order.Price == price);
                if (existingOrder != null)
                    StockExchange.sellOrders.Remove(existingOrder);

                int amount = g.일회거래액 * 10000 / price;
                if (amount == 0)
                    amount = 1;

                if (stockData.Deal.보유량 < amount)
                {
                    DealManager.DealCancelStock(_stockName);
                    DealManager.DealHold();
                    if (stockData.Deal.보유량 == 0)
                        return;
                }

                if (stockData.Deal.보유량 < amount)
                    amount = stockData.Deal.보유량;

                if (amount == 0)
                    amount = 1;

                Utils.SoundUtils.Sound_돈(g.일회거래액);

                int urgency = (g.optimumTrading)
                    ? (int)(e.X / (double)_dataGridView.Columns[0].Width * 100)
                    : 100;

                if (g.confirm_sell)
                {
                    string msg = $"{_stockName} : {price} X {amount} = {(price * amount / 10000)}만원\n";
                    msg += StringUtils.r3_display_매수_매도(stockData);
                    OpenOrUpdateConfirmationForm(isSell, _stockName, amount, price, urgency, msg);
                }
                else
                {
                    DealManager.DealExec("매도", _stockName, amount, price, "01");
                }
            }
        }

        // Sensei 20250420
        public void Unsubscribe()
        {
            // Step 1: Unsubscribe event
            if (_jpbidPrimary != null)
            {
                _jpbidPrimary.Received -= OnBookBidReceived; // Explicitly detach the event handler
                _jpbidPrimary.Unsubscribe(); // Unsubscribe from real-time data
            }

            // Step 2: Remove stock from dictionary safely
            if (g.BookBidInstances.TryRemove(_stockName, out object removedValue))
            {
                // If the removed object implements IDisposable, dispose it
                if (removedValue is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Step 3: Dispose of DataGridView safely
            if (_dataGridView != null)
            {
                _dataGridView.Dispose();
                _dataGridView = null; // Ensure reference is cleared
            }

            // Step 4: Nullify _jpbidPrimary for garbage collection
            _jpbidPrimary = null;
        }

        // Sensei 20250420
        private void OnBookBidReceived()
        {
            lock (g.lockObject)
            {
                // Begin batch update to minimize UI refreshes
                _dataGridView.SuspendLayout();

                try
                {
                    if (Rows == 5)
                    {
                        _dataTable.Rows[4][1] = _jpbidPrimary.GetHeaderValue(3).ToString();
                        _dataTable.Rows[5][1] = _jpbidPrimary.GetHeaderValue(4).ToString();
                        _dataTable.Rows[4][0] = _jpbidPrimary.GetHeaderValue(5).ToString();
                        _dataTable.Rows[5][2] = _jpbidPrimary.GetHeaderValue(6).ToString();

                        _dataTable.Rows[3][1] = _jpbidPrimary.GetHeaderValue(7).ToString();
                        _dataTable.Rows[6][1] = _jpbidPrimary.GetHeaderValue(8).ToString();
                        _dataTable.Rows[3][0] = _jpbidPrimary.GetHeaderValue(9).ToString();
                        _dataTable.Rows[6][2] = _jpbidPrimary.GetHeaderValue(10).ToString();

                        _dataTable.Rows[2][1] = _jpbidPrimary.GetHeaderValue(11).ToString();
                        _dataTable.Rows[7][1] = _jpbidPrimary.GetHeaderValue(12).ToString();
                        _dataTable.Rows[2][0] = _jpbidPrimary.GetHeaderValue(13).ToString();
                        _dataTable.Rows[7][2] = _jpbidPrimary.GetHeaderValue(14).ToString();

                        _dataTable.Rows[1][1] = _jpbidPrimary.GetHeaderValue(15).ToString();
                        _dataTable.Rows[8][1] = _jpbidPrimary.GetHeaderValue(16).ToString();
                        _dataTable.Rows[1][0] = _jpbidPrimary.GetHeaderValue(17).ToString();
                        _dataTable.Rows[8][2] = _jpbidPrimary.GetHeaderValue(18).ToString();

                        _dataTable.Rows[0][1] = _jpbidPrimary.GetHeaderValue(19).ToString();
                        _dataTable.Rows[9][1] = _jpbidPrimary.GetHeaderValue(20).ToString();
                        _dataTable.Rows[0][0] = _jpbidPrimary.GetHeaderValue(21).ToString();
                        _dataTable.Rows[9][2] = _jpbidPrimary.GetHeaderValue(22).ToString();

                        _dataTable.Rows[10][0] = _jpbidPrimary.GetHeaderValue(23).ToString();
                        _dataTable.Rows[10][2] = _jpbidPrimary.GetHeaderValue(24).ToString();
                    }
                    else
                    {
                        _dataTable.Rows[20][0] = Int32.Parse((_jpbidPrimary.GetHeaderValue(23).ToString())).ToString();
                        _dataTable.Rows[20][2] = Int32.Parse((_jpbidPrimary.GetHeaderValue(24).ToString())).ToString();

                        _dataTable.Rows[9][1] = _jpbidPrimary.GetHeaderValue(3).ToString();
                        _dataTable.Rows[10][1] = _jpbidPrimary.GetHeaderValue(4).ToString();
                        _dataTable.Rows[9][0] = _jpbidPrimary.GetHeaderValue(5).ToString();
                        _dataTable.Rows[10][2] = _jpbidPrimary.GetHeaderValue(6).ToString();

                        _dataTable.Rows[8][1] = _jpbidPrimary.GetHeaderValue(7).ToString();
                        _dataTable.Rows[11][1] = _jpbidPrimary.GetHeaderValue(8).ToString();
                        _dataTable.Rows[8][0] = _jpbidPrimary.GetHeaderValue(9).ToString();
                        _dataTable.Rows[11][2] = _jpbidPrimary.GetHeaderValue(10).ToString();

                        _dataTable.Rows[7][1] = _jpbidPrimary.GetHeaderValue(11).ToString();
                        _dataTable.Rows[12][1] = _jpbidPrimary.GetHeaderValue(12).ToString();
                        _dataTable.Rows[7][0] = _jpbidPrimary.GetHeaderValue(13).ToString();
                        _dataTable.Rows[12][2] = _jpbidPrimary.GetHeaderValue(14).ToString();

                        _dataTable.Rows[6][1] = _jpbidPrimary.GetHeaderValue(15).ToString();
                        _dataTable.Rows[13][1] = _jpbidPrimary.GetHeaderValue(16).ToString();
                        _dataTable.Rows[6][0] = _jpbidPrimary.GetHeaderValue(17).ToString();
                        _dataTable.Rows[13][2] = _jpbidPrimary.GetHeaderValue(18).ToString();

                        _dataTable.Rows[5][1] = _jpbidPrimary.GetHeaderValue(19).ToString();
                        _dataTable.Rows[14][1] = _jpbidPrimary.GetHeaderValue(20).ToString();
                        _dataTable.Rows[5][0] = _jpbidPrimary.GetHeaderValue(21).ToString();
                        _dataTable.Rows[14][2] = _jpbidPrimary.GetHeaderValue(22).ToString();

                        _dataTable.Rows[4][1] = _jpbidPrimary.GetHeaderValue(27).ToString();
                        _dataTable.Rows[15][1] = _jpbidPrimary.GetHeaderValue(28).ToString();
                        _dataTable.Rows[4][0] = _jpbidPrimary.GetHeaderValue(29).ToString();
                        _dataTable.Rows[15][2] = _jpbidPrimary.GetHeaderValue(30).ToString();

                        _dataTable.Rows[3][1] = _jpbidPrimary.GetHeaderValue(31).ToString();
                        _dataTable.Rows[16][1] = _jpbidPrimary.GetHeaderValue(32).ToString();
                        _dataTable.Rows[3][0] = _jpbidPrimary.GetHeaderValue(33).ToString();
                        _dataTable.Rows[16][2] = _jpbidPrimary.GetHeaderValue(34).ToString();

                        _dataTable.Rows[2][1] = _jpbidPrimary.GetHeaderValue(35).ToString();
                        _dataTable.Rows[17][1] = _jpbidPrimary.GetHeaderValue(36).ToString();
                        _dataTable.Rows[2][0] = _jpbidPrimary.GetHeaderValue(37).ToString();
                        _dataTable.Rows[17][2] = _jpbidPrimary.GetHeaderValue(38).ToString();

                        _dataTable.Rows[1][1] = _jpbidPrimary.GetHeaderValue(39).ToString();
                        _dataTable.Rows[18][1] = _jpbidPrimary.GetHeaderValue(40).ToString();
                        _dataTable.Rows[1][0] = _jpbidPrimary.GetHeaderValue(41).ToString();
                        _dataTable.Rows[18][2] = _jpbidPrimary.GetHeaderValue(42).ToString();

                        _dataTable.Rows[0][1] = _jpbidPrimary.GetHeaderValue(43).ToString();
                        _dataTable.Rows[19][1] = _jpbidPrimary.GetHeaderValue(44).ToString();
                        _dataTable.Rows[0][0] = _jpbidPrimary.GetHeaderValue(45).ToString();
                        _dataTable.Rows[19][2] = _jpbidPrimary.GetHeaderValue(46).ToString();
                    }

                    // Additional logic for monitoring prices, for example
                    string code = _jpbidPrimary.GetHeaderValue(0).ToString();
                    string stock = _stockCodeService.CodeToName(code).ToString();

                    StockExchange.Instance.MonitorPrices(stock,
                        Convert.ToInt32(_dataTable.Rows[Rows - 1][0]),
                        Convert.ToInt32(_dataTable.Rows[Rows - 1][1]),
                        Convert.ToInt32(_dataTable.Rows[Rows][2]),
                        Convert.ToInt32(_dataTable.Rows[Rows][1]));

                    RequestQuoteExtra();
                }
                finally
                {
                    // End batch update
                    _dataGridView.ResumeLayout();
                }
            }
        }

        // Sensei 20250420
        private void RequestQuote()
        {
            if (!g.StockRepository.TryGet(_stockName, out var stockData))
                return;

            _jpbidSecondary = new DSCBO1Lib.StockJpbid2();

            if (_jpbidSecondary.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            string stockcode = _stockCodeService.NameToCode(_stockName);
            _jpbidSecondary.SetInputValue(0, stockcode);

            int result = _jpbidSecondary.BlockRequest();
            if (result != 0) return;

            _dataTable.Rows[2 * Rows][0] = int.Parse(_jpbidSecondary.GetHeaderValue(4).ToString()); // 매도잔량
            _dataTable.Rows[2 * Rows][2] = int.Parse(_jpbidSecondary.GetHeaderValue(6).ToString()); // 매수잔량

            if (Rows == 5)
            {
                int indexSell = 4;
                int indexBuy = 5;

                for (int i = 0; i < int.Parse(_jpbidSecondary.GetHeaderValue(1).ToString()) / 2; i++)
                {
                    _dataTable.Rows[indexSell][1] = _jpbidSecondary.GetDataValue(0, i).ToString();
                    _dataTable.Rows[indexBuy][1] = _jpbidSecondary.GetDataValue(1, i).ToString();

                    _dataTable.Rows[indexSell][0] = _jpbidSecondary.GetDataValue(2, i).ToString();
                    _dataTable.Rows[indexBuy][2] = _jpbidSecondary.GetDataValue(3, i).ToString();

                    indexSell--;
                    indexBuy++;
                }
            }
            else
            {
                int indexSell = 9;
                int indexBuy = 10;

                for (int i = 0; i < int.Parse(_jpbidSecondary.GetHeaderValue(1).ToString()); i++)
                {
                    _dataTable.Rows[indexSell][1] = _jpbidSecondary.GetDataValue(0, i).ToString();
                    _dataTable.Rows[indexBuy][1] = _jpbidSecondary.GetDataValue(1, i).ToString();

                    _dataTable.Rows[indexSell][0] = _jpbidSecondary.GetDataValue(2, i).ToString();
                    _dataTable.Rows[indexBuy][2] = _jpbidSecondary.GetDataValue(3, i).ToString();

                    indexSell--;
                    indexBuy++;
                }
            }

            int valUp = (int)_jpbidSecondary.GetDataValue(0, 0);
            int valDn = (int)_jpbidSecondary.GetDataValue(1, 0);

            if (g.StockManager.IndexList.Contains(_stockName))
            {
                _dataTable.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##");
                _dataTable.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[Rows - 1][2] = (stockData.Post.프누천 / 10.0).ToString("0.##");
            }

            if (MathUtils.IsSafeToDivide(valDn))
            {
                _dataTable.Rows[2 * Rows]["호가"] = ((valUp - valDn) / (double)valDn * 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[2 * Rows]["호가"] = "No Data";
            }

            if (stockData.Deal.보유량 > 0 && stockData.Api.매수1호가 > 0)
            {
                stockData.Deal.수익률 = (stockData.Api.매수1호가 - stockData.Deal.장부가) / (double)stockData.Api.매수1호가 * 100;
            }

            _dataTable.Rows[2 * Rows + 1][0] = stockData.Stock;
            _dataTable.Rows[2 * Rows + 1][1] = stockData.Statistics.일간변동평균편차;
            _dataTable.Rows[2 * Rows + 1][2] = stockData.Deal.보유량 + "/" + stockData.Deal.수익률.ToString("F2");

            RequestQuoteExtra();
        }

        // Sensei 20250420
        private void RequestQuoteExtra()
        {
            if (!g.StockRepository.TryGet(_stockName, out var stockData))
                return;

            int divider = 10;

            for (int i = 0; i < Rows; i++)
            {
                _dataTable.Rows[i + Rows][0] = ((stockData.Api.틱매수량[i] - stockData.Api.틱매수량[i + 1]) / divider).ToString()
                                              + "/" +
                                              ((stockData.Api.틱매도량[i] - stockData.Api.틱매도량[i + 1]) / divider).ToString();
            }

            for (int i = 0; i < Rows; i++)
            {
                _dataTable.Rows[i][2] = stockData.Api.틱프로천[i].ToString("F0") + "+" +
                                        stockData.Api.틱외인천[i].ToString("F0") + "/" +
                                        stockData.Api.틱거래천[i].ToString("F0");
            }

            stockData.Api.틱의시간[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            stockData.Api.매도1호가 = Convert.ToInt32(_dataTable.Rows[Rows - 1][1]);
            stockData.Api.매수1호가 = Convert.ToInt32(_dataTable.Rows[Rows][1]);

            int.TryParse(_dataTable.Rows[Rows - 1][0]?.ToString(), out stockData.Api.최우선매도호가잔량);
            int.TryParse(_dataTable.Rows[Rows][2]?.ToString(), out stockData.Api.최우선매수호가잔량);

            if (stockData.Api.전일종가 > 0)
            {
                stockData.Api.틱의가격[0] = (int)((stockData.Api.매수1호가 - stockData.Api.전일종가) * 10000.0 / stockData.Api.전일종가);
            }

            // [remaining logic... convert similarly]






            double factor = 0.0;
            double differ = 0.0;
            if (o.전일종가 > 0)
                differ = (stockData.Api.매도1호가 - stockData.Api.매수1호가) * 10000.0 / o.전일종가;
            if (o.최우선매도호가잔량 + o.최우선매수호가잔량 > 0)
                factor = (double)o.최우선매수호가잔량 / (o.최우선매도호가잔량 + o.최우선매수호가잔량);
            o.틱의가격[0] += (int)(differ * factor);
            o.가격 = o.틱의가격[0];

            o.틱매도잔[0] = o.최우선매도호가잔량;
            o.틱매수잔[0] = o.최우선매수호가잔량;

            // 프누억
            if (_stockName.Contains("KODEX"))
            {
                _dataTable.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##"); ;
                _dataTable.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##"); ;
            }
            else
            {
                _dataTable.Rows[Rows - 1][2] = (stockData.Post.프누천 / 10.0).ToString("0.##");
            }

            // 한 호가 차이 슬리피지 % 계산
            if (stockData.Api.매수1호가 > 0)
            {
                _dataTable.Rows[2 * Rows][1] = ((stockData.Api.매도1호가 - stockData.Api.매수1호가) / (double)stockData.Api.매수1호가 * 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[2 * Rows][1] = "No Data";
            }



            if (stockData.Deal.보유량 > 0)
            {
                if (stockData.Api.매수1호가 > 0)
                {
                    stockData.Deal.수익률 = (stockData.Api.매수1호가 - stockData.Deal.장부가) / (double)stockData.Api.매수1호가 * 100;
                }
            }
            _dataTable.Rows[2 * Rows + 1][0] = stockData.Stock;
            _dataTable.Rows[2 * Rows + 1][1] = stockData.Statistics.일간변동평균편차;
            _dataTable.Rows[2 * Rows + 1][2] = stockData.Deal.보유량.ToString() + "/" + stockData.Deal.수익률.ToString("F2");


            for (int i = 0; i < 2 * Rows - 2; i++)
            {
                _dataGridView.Rows[i].Cells[1].Style.BackColor = Color.White; // red
            }


            // upperPassingPrice
            if (stockData.Pass.upperPassingPrice > 0)
            {
                if (stockData.Api.매도1호가 >= stockData.Pass.upperPassingPrice)
                {
                    Utils.SoundUtils.Sound("일반", "passing upper"); // StopLoss[1]
                    stockData.Pass.upperPassingPrice = 0;
                }
            }

            // lowerPassingPrice
            if (stockData.Pass.lowerPassingPrice > 0)
            {
                if (stockData.Api.매수1호가 <= stockData.Pass.lowerPassingPrice)
                {
                    Utils.SoundUtils.Sound("일반", "passing lower"); // Stop Loss[0]
                    stockData.Pass.lowerPassingPrice = 0;
                }
            }


            // MiddleCenter : need double loops, not by a single line
            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }

            Color[,] cellColors = new Color[2 * Rows + 2, 3];
            Color ColorLightRed = Color.FromArgb(255, 204, 204); // LightRed

            cellColors[Rows - 1, 2] = Color.LightGreen;
            if (g.StockManager.IndexList.Contains(_stockName))
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
                int row = g.StockManager.IndexList.Contains(_stockName) ? Rows - 2 : Rows - 1;
                for (int i = 0; i < row; i++)
                {
                    cellColors[i, 2] = Color.Yellow;
                }
            }


            //  Coloring upperPassingPrice in column 1
            if (stockData.Pass.upperPassingPrice > 0 || stockData.Pass.lowerPassingPrice > 0)
            {
                for (int i = 0; i < 2 * Rows; i++)
                {
                    if (int.TryParse(_dataTable.Rows[i][1].ToString(), out int price))
                    {
                        if (price == stockData.Pass.upperPassingPrice || price == stockData.Pass.lowerPassingPrice)
                        {
                            cellColors[i, 1] = Color.Yellow;
                        }
                    }
                }
            }

            // Coloring buyOrder and sellOrder with Quantity
            var ordersWithStock = StockExchange.buyOrders
                .Where(order => order.Stock == _stockName)
                .Concat(StockExchange.sellOrders.Where(order => order.Stock == _stockName))
                .ToList();

            foreach (var order in ordersWithStock)
            {
                for (int i = 0; i < 2 * Rows; i++)
                {
                    if (int.TryParse(_dataTable.Rows[i][1].ToString(), out int price) && price == order.Price)
                    {
                        if (StockExchange.buyOrders.Contains(order))
                        {
                            _dataTable.Rows[i][2] = order.Quantity.ToString();
                            cellColors[i, 2] = Color.Red;
                        }
                        else if (StockExchange.sellOrders.Contains(order))
                        {
                            _dataTable.Rows[i][0] = order.Quantity.ToString();
                            cellColors[i, 0] = Color.Red;
                        }
                    }
                }
            }

            // Finally apply the assigned colors to each cessl of _dataGridView
            for (int i = 0; i < 2 * Rows; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _dataGridView.Rows[i].Cells[j].Style.BackColor = cellColors[i, j];
                }
            }
        }

        // Sensei 20250420
        private void OnDataError(object sender, DataGridViewDataErrorEventArgs e)
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

        #region BookBid Utilities

        // Sensei 20250420
        public static int GetClickedPrice(DataGridView dgv, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != 0 && e.ColumnIndex != 2)
                return 0;

            if (e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count)
                return 0;

            var cellValue = dgv.Rows[e.RowIndex].Cells[1].Value?.ToString();

            if (int.TryParse(cellValue?.Replace(",", ""), out int price))
                return price;

            return StringUtils.ExtractIntFromString(cellValue);
        }

        // Sensei 20250420
        public static int GetTopBookPrice(bool isBuy)
        {
            var dgv = g.ActiveBookBid;
            if (dgv == null || dgv.Rows.Count < 6)
                return 0;

            int rowIndex = isBuy ? 5 : 0;

            var value = dgv.Rows[rowIndex].Cells[1].Value?.ToString();

            if (int.TryParse(value?.Replace(",", ""), out int price))
                return price;

            return StringUtils.ExtractIntFromString(value);
        }

        #endregion
    }
}


// updated on 20241020, lock, BeginLoadData, EndLoadData added
//private void OnBookBidReceived_new()
//{
//    lock (g.lockObject)
//    {
//        // Begin batch update to minimize UI refreshes
//        _dataGridView.SuspendLayout();

//        try
//        {

//            if (Rows == 5)
//            {
//                for (int i = 0; i < 5; i++) // Loop for 5 rows
//                {
//                    int headerIndex = 3 + (i * 4); // Dynamically calculate header index
//                    _dataTable.Rows[4 - i][1] = _jpbidPrimary.GetHeaderValue(headerIndex).ToString();       // Middle column (prices for selling)
//                    _dataTable.Rows[5 + i][1] = _jpbidPrimary.GetHeaderValue(headerIndex + 1).ToString(); // Middle column (prices for buying)
//                    _dataTable.Rows[4 - i][0] = _jpbidPrimary.GetHeaderValue(headerIndex + 2).ToString(); // Left column (selling amounts)
//                    _dataTable.Rows[5 + i][2] = _jpbidPrimary.GetHeaderValue(headerIndex + 3).ToString(); // Right column (buying amounts)
//                }

//                // Handling the last row for the 10th price (if applicable)
//                _dataTable.Rows[10][0] = _jpbidPrimary.GetHeaderValue(23).ToString(); // Last selling amount
//                _dataTable.Rows[10][2] = _jpbidPrimary.GetHeaderValue(24).ToString(); // Last buying amount
//            }

//            else
//            {
//                for (int i = 0; i < 5; i++) // Selling Side
//                {
//                    int headerIndex = 19 - (i * 2); // Adjusting the header index dynamically
//                    int rowIndex = 9 - i; // Adjusting the DataTable row index for the top 5 rows

//                    // Setting the selling price and divided amounts
//                    _dataTable.Rows[rowIndex][1] = _jpbidPrimary.GetHeaderValue(headerIndex).ToString();
//                    _dataTable.Rows[rowIndex][0] = (_jpbidPrimary.GetHeaderValue(headerIndex - 1).ToString()) + "/" +
//                                            (_jpbidPrimary.GetHeaderValue(headerIndex - 1).ToString());
//                    _dataTable.Rows[rowIndex][2] = ""; // Placeholder for extra data (can be updated with magenta "0")
//                }

//                for (int i = 0; i < 5; i++) // Buying Side
//                {
//                    int headerIndex = 20 + (i * 2); // Adjusting the header index dynamically
//                    int rowIndex = 9 + i; // Adjusting the DataTable row index for the bottom 5 rows

//                    // Setting the buying price and divided amounts
//                    _dataTable.Rows[rowIndex][1] = _jpbidPrimary.GetHeaderValue(headerIndex).ToString();
//                    _dataTable.Rows[rowIndex][0] = ""; // Placeholder for selling amounts
//                    _dataTable.Rows[rowIndex][2] = (_jpbidPrimary.GetHeaderValue(headerIndex + 1).ToString()) + "/" +
//                                            (_jpbidPrimary.GetHeaderValue(headerIndex + 1).ToString());
//                }
//            }

//            // Additional logic for monitoring prices, for example
//            string code = _jpbidPrimary.GetHeaderValue(0).ToString();
//            string stock = _stockCodeService.CodeToName(code).ToString();

//            StockExchange.Instance.MonitorPrices(stock,
//                Convert.ToInt32(_dataTable.Rows[Rows - 1][0]),
//                Convert.ToInt32(_dataTable.Rows[Rows - 1][1]),
//                Convert.ToInt32(_dataTable.Rows[Rows][2]),
//                Convert.ToInt32(_dataTable.Rows[Rows][1]));

//            RequestQuoteExtra();
//        }
//        finally
//        {
//            // End batch update
//            _dataGridView.ResumeLayout();
//        }
//    }
//}
