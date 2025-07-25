﻿using CPTRADELib;
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
using New_Tradegy.Library.Deals;
using New_Tradegy.Library.Core;

namespace New_Tradegy.Library.Listeners
{
    public class BookBidGenerator
    {
        private CPUTILLib.CpStockCode _stockCodeService = new CPUTILLib.CpStockCode();
        private DSCBO1Lib.StockJpbid _jpbidPrimary;
        private DSCBO1Lib.StockJpbid2 _jpbidSecondary;

        private DataTable _dataTable;
        private DataGridView _dataGridView;
        private int Rows = 5;
        private string _stock;

        // Sensei 20250420
        public DataGridView GenerateBookBidView(string stock)
        {
            if (!g.connected)
                return null;

            _stock = stock;

            int w0 = 61, w1 = 50, w2 = 61;
            

            // 1. DataTable 생성 및 초기화
            _dataTable = new DataTable();
            _dataTable.Columns.Add("매도");
            _dataTable.Columns.Add("호가");
            _dataTable.Columns.Add("매수");

            for (int i = 0; i < 2 * Rows + 2; i++)
                _dataTable.Rows.Add("", "", "");

            // 2. DataGridView 생성 및 설정 (단 한 번만)
            _dataGridView = new DataGridView
            {
                Name = _stock,
                Location = new Point(0, 0), // temporary location
                Size = new Size(w0 + w1 + w2, g.cellHeight * 12 - 10),
                Dock = DockStyle.None,
                TabIndex = 1,
                DataSource = _dataTable, // ✅ DataSource 포함
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ScrollBars = ScrollBars.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DefaultCellStyle = { Font = new Font("Arial", 9, FontStyle.Bold), ForeColor = Color.Black },
                RowTemplate = { Height = 27 },
                ColumnHeadersDefaultCellStyle = { Font = new Font("Arial", 9, FontStyle.Bold) },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.LightYellow
            };

            // 3. 이벤트 핸들러 등록
            _dataGridView.DataError += (s, e) => FileOut.DataGridView_DataError(s, e, $"jpjd {_dataGridView.Name}");
            _dataGridView.DataError += new DataGridViewDataErrorEventHandler(OnDataError);
            _dataGridView.CellMouseClick += new DataGridViewCellMouseEventHandler(OnCellMouseClick);

            // 4. 호가 데이터 구독 설정
            string stockcode = _stockCodeService.NameToCode(_stock);
            _jpbidPrimary = new DSCBO1Lib.StockJpbid();
            _jpbidPrimary.SetInputValue(0, stockcode);
            _jpbidPrimary.Received += new DSCBO1Lib._IDibEvents_ReceivedEventHandler(OnBookBidReceived);

            if (g.BookBidInstances.ContainsKey(_stock))
                return null;

            g.BookBidInstances.TryAdd(_stock, _jpbidPrimary);
            _jpbidPrimary.Subscribe();

            // 5. Form에 추가
            g.MainForm.Controls.Add(_dataGridView);
            _dataGridView.Visible = false;

            // 6. 데이터 요청 및 정렬
            RequestQuote();

            _dataGridView.BringToFront();
            g.ChartManager.Chart1.SendToBack();

            _dataGridView.Visible = true;

            _dataGridView.Columns[0].Width = w0;
            _dataGridView.Columns[1].Width = w1;
            _dataGridView.Columns[2].Width = w2;

            return _dataGridView;
        }



        /// <summary>
        /// Open the Form_매수_매도 to confirm a deal
        /// </summary>
        /// <param name="isSell">True if this is a sell operation; false for buy</param>
        /// <param name="stockName">Name of the stock</param>
        /// <param name="Amount">Number of shares to trade</param>
        /// <param name="price">Price per share</param>
        /// <param name="Urgency">Urgency level (e.g., 0 = normal, 1 = urgent)</param>
        /// <param name="str">Optional tag or label for the transaction</param>
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
                if (form is Form_매수_매도 tradeForm &&
                    !string.IsNullOrEmpty(tradeForm._stock) &&
                    tradeForm._stock.Trim() == stockName.Trim())
                {
                    return tradeForm;
                }
            }
            return null; // No open form found
        }

        // Sensei 20250420
        private void OnCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // g.ActiveBookBid = sender as DataGridView;

            if (e.Button == MouseButtons.Right)
                return;

            int price = GetClickedPrice(_dataGridView, e);
            if (price == 0)
                return; // Skip if no valid price

            if (!g.StockManager.Repository.TryGet(_stock, out var data))
                return;

            // Column 1: Toggle passing price or toggle hoga depth
            if (e.ColumnIndex == 1)
            {
                if (e.RowIndex < Rows)
                {
                    data.Pass.upperPassingPrice = (price == data.Pass.upperPassingPrice) ? 0 : price;
                }
                else if (e.RowIndex <= Rows)
                {
                    data.Pass.lowerPassingPrice = (price == data.Pass.lowerPassingPrice) ? 0 : price;
                }
                else if (e.ColumnIndex <= Rows * 2 && !g.StockManager.IndexList.Contains(_stock))
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
                    .Find(order => order.Stock == _stock && order.Price == price);
                if (existingOrder != null)
                    StockExchange.buyOrders.Remove(existingOrder);

                int amount = g.일회거래액 * 10000 / price;
                if (amount == 0)
                    amount = 1;

                Utils.SoundUtils.Sound_돈(g.일회거래액);

                int urgency = (g.optimumTrading)
                    ? (int)(e.X / (double)_dataGridView.Columns[2].Width * 100)
                    : 100;

                string msg = $"{_stock} : {price} X {amount} = {(price * amount / 10000)}만원\n";
                msg += StringUtils.r3_display_매수_매도(data);

                if (DealManager.CheckPreviousLoss(_stock))
                    return;

                OpenOrUpdateConfirmationForm(isSell, _stock, amount, price, urgency, msg);
            }
            else if (isSell)
            {
                var existingOrder = StockExchange.sellOrders
                    .Find(order => order.Stock == _stock && order.Price == price);
                if (existingOrder != null)
                    StockExchange.sellOrders.Remove(existingOrder);

                int amount = g.일회거래액 * 10000 / price;
                if (amount == 0)
                    amount = 1;

                if (data.Deal.보유량 < amount)
                {
                    DealManager.DealCancelStock(_stock);
                    DealManager.DealHold();
                    if (data.Deal.보유량 == 0)
                        return;
                }

                if (data.Deal.보유량 < amount)
                    amount = data.Deal.보유량;

                if (amount == 0)
                    amount = 1;

                Utils.SoundUtils.Sound_돈(g.일회거래액);

                int urgency = (g.optimumTrading)
                    ? (int)(e.X / (double)_dataGridView.Columns[0].Width * 100)
                    : 100;

                if (g.confirm_sell)
                {
                    string msg = $"{_stock} : {price} X {amount} = {(price * amount / 10000)}만원\n";
                    msg += StringUtils.r3_display_매수_매도(data);
                    OpenOrUpdateConfirmationForm(isSell, _stock, amount, price, urgency, msg);
                }
                else
                {
                    DealManager.DealExec("매도", _stock, amount, price, "01");
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
            if (g.BookBidInstances.TryRemove(_stock, out object removedValue))
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
                var parent = _dataGridView.Parent;
                if (parent != null)
                    parent.Controls.Remove(_dataGridView); // ✅ 폼에서 제거

                _dataGridView.Dispose();
                _dataGridView = null;
            }

            // Step 4: Nullify _jpbidPrimary for garbage collection
            _jpbidPrimary = null;
        }

        // Sensei 20250420
        private void OnBookBidReceived()
        {
            lock (g.lockObject)
            {
                //                (int row, int col, int header)[] mapping = new[]
                //{
                //    (4, 1, 3), (5, 1, 4), (4, 0, 5), (5, 2, 6),
                //    (3, 1, 7), (6, 1, 8), (3, 0, 9), (6, 2, 10),
                //    ...
                //};

                //                foreach (var (row, col, header) in mapping)
                //                    _dataTable.Rows[row][col] = _jpbidPrimary.GetHeaderValue(header).ToString();

                //string GetValue(int index)
                //{
                //    try { return _jpbidPrimary.GetHeaderValue(index).ToString(); }
                //    catch { return ""; } // 또는 기본값 "0"
                //}


                //int SafeParse(object obj)
                //{
                //    return int.TryParse(obj?.ToString(), out int val) ? val : 0;
                //}



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

            //int TryParseSafe(object obj)
            //{
            //    return int.TryParse(obj?.ToString(), out int val) ? val : 0;
            //}

            //if (Rows == 5)
            //    FillFiveRowBookBid();
            //else
            //    FillTenRowBookBid();





            if (!g.StockRepository.TryGet(_stock, out var data))
                return;

            _jpbidSecondary = new DSCBO1Lib.StockJpbid2();

            if (_jpbidSecondary.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            string stockcode = _stockCodeService.NameToCode(_stock);
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

            if (g.StockManager.IndexList.Contains(_stock))
            {
                _dataTable.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##");
                _dataTable.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[Rows - 1][2] = (data.Post.푀누천 / 10.0).ToString("0.##");
            }

            if (MathUtils.IsSafeToDivide(valDn))
            {
                _dataTable.Rows[2 * Rows]["호가"] = ((valUp - valDn) / (double)valDn * 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[2 * Rows]["호가"] = "No Data";
            }

            if (data.Deal.보유량 > 0 && data.Api.매수1호가 > 0)
            {
                data.Deal.수익률 = (data.Api.매수1호가 - data.Deal.장부가) / (double)data.Api.매수1호가 * 100;
            }

            _dataTable.Rows[2 * Rows + 1][0] = data.Stock;
            _dataTable.Rows[2 * Rows + 1][1] = data.Statistics.일간변동평균편차;
            _dataTable.Rows[2 * Rows + 1][2] = data.Deal.보유량 + "/" + data.Deal.수익률.ToString("F2");

            RequestQuoteExtra();
        }

        // Sensei 20250420
        private void RequestQuoteExtra()
        {
            // Convert.ToInt32(...)는 예외 발생 위험
            //int.TryParse(_dataTable.Rows[Rows - 1][1]?.ToString(), out data.Api.매도1호가);
            //int.TryParse(_dataTable.Rows[Rows][1]?.ToString(), out data.Api.매수1호가);



            if (!g.StockRepository.TryGet(_stock, out var data))
                return;

            int divider = 10;

            for (int i = 0; i < Rows; i++)
            {
                _dataTable.Rows[i + Rows][0] = ((data.Api.틱매수량[i] - data.Api.틱매수량[i + 1]) / divider).ToString()
                                              + "/" +
                                              ((data.Api.틱매도량[i] - data.Api.틱매도량[i + 1]) / divider).ToString();
            }

            for (int i = 0; i < Rows; i++)
            {
                _dataTable.Rows[i][2] = data.Api.틱프로천[i].ToString("F0") + "+" +
                                        data.Api.틱외인천[i].ToString("F0") + "/" +
                                        data.Api.틱거래천[i].ToString("F0");
            }

            data.Api.틱의시간[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmssfff"));

            data.Api.매도1호가 = Convert.ToInt32(_dataTable.Rows[Rows - 1][1]);
            data.Api.매수1호가 = Convert.ToInt32(_dataTable.Rows[Rows][1]);

            int.TryParse(_dataTable.Rows[Rows - 1][0]?.ToString(), out data.Api.최우선매도호가잔량);
            int.TryParse(_dataTable.Rows[Rows][2]?.ToString(), out data.Api.최우선매수호가잔량);

            if (data.Api.전일종가 > 0)
            {
                data.Api.틱의가격[0] = (int)((data.Api.매수1호가 - data.Api.전일종가) * 10000.0 / data.Api.전일종가);
            }


            //??
            double factor = 0.0;
            double differ = 0.0;
            if (data.Api.전일종가 > 0)
                differ = (data.Api.매도1호가 - data.Api.매수1호가) * 10000.0 / data.Api.전일종가;
            if (data.Api.최우선매도호가잔량 + data.Api.최우선매수호가잔량 > 0)
                factor = (double)data.Api.최우선매수호가잔량 / (data.Api.최우선매도호가잔량 + data.Api.최우선매수호가잔량);
            data.Api.틱의가격[0] += (int)(differ * factor);
            data.Api.가격 = data.Api.틱의가격[0];

            data.Api.틱매도잔[0] = data.Api.최우선매도호가잔량;
            data.Api.틱매수잔[0] = data.Api.최우선매수호가잔량; //??

            // 푀누억
            if (_stock.Contains("KODEX"))
            {
                _dataTable.Rows[Rows - 2][2] = (MajorIndex.Instance.KospiIndex / 100.0).ToString("0.##"); ;
                _dataTable.Rows[Rows - 1][2] = (MajorIndex.Instance.KosdaqIndex / 100.0).ToString("0.##"); ;
            }
            else
            {
                _dataTable.Rows[Rows - 1][2] = (data.Post.푀누천 / 10.0).ToString("0.##");
            }

            // 한 호가 차이 슬리피지 % 계산
            if (data.Api.매수1호가 > 0)
            {
                _dataTable.Rows[2 * Rows][1] = ((data.Api.매도1호가 - data.Api.매수1호가) / (double)data.Api.매수1호가 * 100.0).ToString("0.##");
            }
            else
            {
                _dataTable.Rows[2 * Rows][1] = "No Data";
            }



            if (data.Deal.보유량 > 0)
            {
                if (data.Api.매수1호가 > 0)
                {
                    data.Deal.수익률 = (data.Api.매수1호가 - data.Deal.장부가) / (double)data.Api.매수1호가 * 100;
                }
            }
            _dataTable.Rows[2 * Rows + 1][0] = data.Stock;
            _dataTable.Rows[2 * Rows + 1][1] = data.Statistics.일간변동평균편차;
            _dataTable.Rows[2 * Rows + 1][2] = data.Deal.보유량.ToString() + "/" + data.Deal.수익률.ToString("F2");





            //for (int i = 0; i < _dataGridView.Rows.Count; i++)
            //{
            //    for (int j = 0; j < _dataGridView.Columns.Count; j++)
            //    {
            //        _dataGridView.Rows[i].Cells[j].Style.BackColor = Color.White;
            //    }
            //}

            for (int i = 0; i < 2 * Rows - 2; i++)
            {
                if (_dataGridView.Rows.Count > 0 && _dataGridView.Rows[0].Cells.Count > 1)
                {
                    _dataGridView.Rows[0].Cells[1].Style.BackColor = Color.White;
                }
            }


            // upperPassingPrice
            if (data.Pass.upperPassingPrice > 0)
            {
                if (data.Api.매도1호가 >= data.Pass.upperPassingPrice)
                {
                    Utils.SoundUtils.Sound("일반", "passing upper"); // StopLoss[1]
                    data.Pass.upperPassingPrice = 0;
                }
            }

            // lowerPassingPrice
            if (data.Pass.lowerPassingPrice > 0)
            {
                if (data.Api.매수1호가 <= data.Pass.lowerPassingPrice)
                {
                    Utils.SoundUtils.Sound("일반", "passing lower"); // Stop Loss[0]
                    data.Pass.lowerPassingPrice = 0;
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
            if (g.StockManager.IndexList.Contains(_stock))
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
                int row = g.StockManager.IndexList.Contains(_stock) ? Rows - 2 : Rows - 1;
                for (int i = 0; i < row; i++)
                {
                    cellColors[i, 2] = Color.Yellow;
                }
            }


            //  Coloring upperPassingPrice in column 1
            if (data.Pass.upperPassingPrice > 0 || data.Pass.lowerPassingPrice > 0)
            {
                for (int i = 0; i < 2 * Rows; i++)
                {
                    if (int.TryParse(_dataTable.Rows[i][1].ToString(), out int price))
                    {
                        if (price == data.Pass.upperPassingPrice || price == data.Pass.lowerPassingPrice)
                        {
                            cellColors[i, 1] = Color.Yellow;
                        }
                    }
                }
            }

            // Coloring buyOrder and sellOrder with Quantity
            var ordersWithStock = StockExchange.buyOrders
                .Where(order => order.Stock == _stock)
                .Concat(StockExchange.sellOrders.Where(order => order.Stock == _stock))
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
            if (_dataGridView.Rows.Count == 0)
                return;

            for (int i = 0; i < Math.Min(2 * Rows, _dataGridView.Rows.Count); i++)
            {
                for (int j = 0; j < Math.Min(3, _dataGridView.Columns.Count); j++)
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

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now}] An error occurred: {ex.Message}");
                    writer.WriteLine(ex.StackTrace);
                }
            }
            catch
            {
                // 로그 실패 시 아무 것도 하지 않음 (또는 Debug.WriteLine 정도만)
            }
        }

        #region BookBid Utilities

        // Sensei 20250420
        public static int GetClickedPrice(DataGridView dgv, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != 0 && e.ColumnIndex != 2) // click sell or buy side
                return 0;

            if (e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count) // in the range of bookbid row
                return 0;

            var cellValue = dgv.Rows[e.RowIndex].Cells[1].Value?.ToString();

            if (int.TryParse(cellValue?.Replace(",", ""), out int price))
                return price;

            return StringUtils.ExtractIntFromString(cellValue);
        }



        #endregion
    }
}
