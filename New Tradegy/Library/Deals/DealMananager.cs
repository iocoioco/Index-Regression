using CPTRADELib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;

namespace New_Tradegy.Library.Deals
{
    public class DealManager
    {
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        private static readonly CPUTILLib.CpCodeMgr _cpcodemgr = new CPUTILLib.CpCodeMgr();
        public static CPUTILLib.CpCybos _cpcybos;
        private static CPTRADELib.CpTdUtil _cptdutil;
        private CPTRADELib.CpTd0303 _cptd0303; // 주식 주문관리 (매매 유형 정정 주문) 데이터를 요청
        private static CPTRADELib.CpTd0311 _cptd0311; // 주문(현금 주문) 데이터를 요청
        private readonly CPTRADELib.CpTd0312 _cptd0312; // 신용 주문 데이터를 요청
        private readonly CPTRADELib.CpTd0313 _cptd0313; // 주문관리 (가격 정정 주문) 데이터를 요청
        private readonly CPTRADELib.CpTd0732 _cptd0732; // 주식 결제예정 예수금 가계산 데이터를 요청
        private static CPTRADELib.CpTd0314 _cptd0314; // 주문관리 (취소 주문) 데이터를 요청
        private static CPTRADELib.CpTdNew5331A _depositQuery; // 계좌별 매수주문 가능 금액/수량 데이터를 요청
        private readonly CPTRADELib.CpTdNew5331B _cptdnew5331b; // 계좌별 매도주문 가능 수량 데이터를 요청
        private static CPTRADELib.CpTd5339 _cptd5339; // 계좌별 미체가장 잔량 데이터를 요청
        private readonly CPTRADELib.CpTd5341 _cptd5341; // 금일 계좌별 주문/체가 내역 조회 데이터를 요청폗
        private static CPTRADELib.CpTd6033 _cptd6033; // 계좌별 잔고 및 주문체가 평가 현황 데이터를 요청
        private static CPTRADELib.CpTd5342 _profitQuery;
        private static bool _checkedTradeInit = false;

        public static string accountNo;
        public static string accountGoodsStock;
        public static string[] arrAccountGoodsStock;

        public static void TradeInit()
        {
            if (_checkedTradeInit)
                return;

            if (_cptdutil == null)
            {
                _cptdutil = new CpTdUtil();
            }

            int rv = _cptdutil.TradeInit(0);
            if (rv == 0)
            {
                _checkedTradeInit = true;

                string[] arrAccount = (string[])_cptdutil.AccountNumber;
                if (arrAccount.Length > 0)
                {
                    accountNo = arrAccount[0];

                    arrAccountGoodsStock = (string[])_cptdutil.get_GoodsList(accountNo, CPE_ACC_GOODS.CPC_STOCK_ACC); // 주식
                    if (arrAccountGoodsStock.Length > 0)
                        accountGoodsStock = arrAccountGoodsStock[0];
                }
            }
            else if (rv == -1)
            {
                MessageBox.Show("계좌 비밀로 오류 포함 에러.");
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

        public static void DealProfit()
        {
            TradeInit();
            if (!_checkedTradeInit)
                return;

            _profitQuery = new CpTd5342();
            if (_profitQuery.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            _profitQuery.SetInputValue(0, g.Account);
            _profitQuery.SetInputValue(1, "01"); // 상품관리구분코드
            _profitQuery.SetInputValue(2, 20);    // 요청개수 - 최대 20개
            _profitQuery.SetInputValue(3, "1");  // "1" : 금일, "2" : 전일
            _profitQuery.SetInputValue(4, "");   // 요청종목코드[default:"" - 모든종목]

            int result = _profitQuery.BlockRequest();

            if (_profitQuery.Continue == 1)
            {
                // 연속 데이터 있음
            }
            else
            {
                // 연속 데이터 없음
            }

            if (result == 0)
            {
                ulong gross = _profitQuery.GetHeaderValue(9);
                ulong cost = _profitQuery.GetHeaderValue(10);
                int dealProfit = ((int)(gross - cost) / 10000);

                if (g.MarketeyeCount == 0)
                {
                    dealProfit = 0;
                }

                if (g.제어.dtb.Rows[1][0].ToString() != dealProfit.ToString())
                {
                    g.제어.dtb.Rows[1][0] = dealProfit.ToString();
                }
            }
        }

        // Sensei 20250426
        public static void DealDeposit()
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            _depositQuery = new CpTdNew5331A();

            if (_depositQuery.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            _depositQuery.SetInputValue(0, g.Account);  // 계좌번호
            _depositQuery.SetInputValue(1, "01");       // 상품관리구분코드 : 거래소
            _depositQuery.SetInputValue(5, "Y");        // Y: Deposit 100 기준
            _depositQuery.SetInputValue(6, "1");         // '1': 금액조회

            int result = _depositQuery.BlockRequest();

            string message = _depositQuery.GetDibMsg1();
            long cashAvailable = 0;
            try
            {
                cashAvailable = Convert.ToInt64(_depositQuery.GetHeaderValue(10));
            }
            catch
            {
                Trace.TraceWarning("GetHeaderValue(10) parsing failed.");
                return;
            }

            int dealDeposit = (int)(cashAvailable / 10000); // 현금주문가능금액 단위변환

            if (g.제어.dtb.Rows.Count > 0)  // safer check
            {
                if (g.제어.dtb.Rows[0][3].ToString() != dealDeposit.ToString())
                {
                    g.제어.dtb.Rows[0][3] = dealDeposit.ToString();
                }
            }
        }


        public static void DealProcessing() // tr(1)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (!_checkedTradeInit)
                return;

            _cptd5339 = new CPTRADELib.CpTd5339();

            // Setup request
            _cptd5339.SetInputValue(0, g.Account); // 계좌번호
            _cptd5339.SetInputValue(1, "01");    // 상품관리구분코드: 거래소주식
            _cptd5339.SetInputValue(4, "0");     // 전체
            _cptd5339.SetInputValue(5, "0");     // 순수
            _cptd5339.SetInputValue(7, 20);       // 최대 20개 요청

            int result = _cptd5339.BlockRequest();
            string textBox = _cptd5339.GetDibMsg1();

            if (result != 0)
                return;

            // Retrieve header data
            string accountNumber = (string)_cptd5339.GetHeaderValue(0);
            string accountName = (string)_cptd5339.GetHeaderValue(4);
            int recordCount = (int)_cptd5339.GetHeaderValue(5);

            // Parse each record
            for (int i = 0; i < recordCount; i++)
            {
                var item = new OrderItem
                {
                    stock = (string)_cptd5339.GetDataValue(4, i),
                    m_ordKey = (int)_cptd5339.GetDataValue(1, i),
                    m_ordOrgKey = (int)_cptd5339.GetDataValue(2, i),
                    m_sCode = _cpstockcode.NameToCode((string)_cptd5339.GetDataValue(4, i)),
                    m_nAmt = (int)_cptd5339.GetDataValue(6, i),
                    m_nPrice = (int)_cptd5339.GetDataValue(7, i),
                    m_nContAmt = (int)_cptd5339.GetDataValue(8, i),
                    m_nModAmt = (int)_cptd5339.GetDataValue(11, i),
                    m_sHogaFlag = _cptd5339.GetDataValue(21, i)
                };

                string bsCode = (string)_cptd5339.GetDataValue(13, i);
                item.buyorSell = bsCode == "1" ? "매도" : bsCode == "2" ? "매수" : "";

                OrderItemTracker.Add(item);
            }
        }


        public static bool CheckPreviousLoss(string stockSymbol)
        {
            var stock = g.StockRepository.TryGetStockOrNull(stockSymbol);
            if (stock == null) return false; // Stock not found

            // Ensure valid purchase price exists and at least 1 share is held
            if (stock.Api.매수1호가 > 0 && stock.Deal.보유량 >= 1)
            {
                double 수익률 = (double)(stock.Api.매수1호가 - stock.Deal.장부가) / stock.Api.매수1호가 * 100;

                if (수익률 < -0.45 && stock.Deal.평가금액 > 4_500_000)
                {
                    Utils.SoundUtils.Sound("alarm", "lost already");
                    return true;
                }
            }
            return false;
        }

        public static void DealHold()
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            _cptd6033 = new CPTRADELib.CpTd6033();

            if (_cptd6033.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            _cptd6033.SetInputValue(0, g.Account);  // 계좌번호
            _cptd6033.SetInputValue(1, "01");       // 상품관리구분코드
            _cptd6033.SetInputValue(2, 50);         // 요청건수 (최대 50)

            int result = _cptd6033.BlockRequest();

            if (result != 0)
                return;

            int count = (int)_cptd6033.GetHeaderValue(7); // 보유종목 수
            if (count > 51) return;

            g.StockManager.HoldingList.Clear();

            // Reset all stocks' 보유량
            //var r = g.StockRepository;
            foreach (var stock in g.StockRepository.AllDatas)
            {
                stock.Deal.보유량 = 0;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                string code = (string)_cptd6033.GetDataValue(0, i);

                if (!g.StockRepository.Contains(code))
                    continue;

                var data = g.StockRepository.TryGetStockOrNull(code);
                if (data == null) continue;
                var api = data.Api;
                var deal = data.Deal;

                deal.보유량 = (int)_cptd6033.GetDataValue(15, i);
                deal.장부가 = (double)_cptd6033.GetDataValue(17, i);
                deal.평가금액 = (long)_cptd6033.GetDataValue(9, i);
                deal.손익단가 = (long)_cptd6033.GetDataValue(18, i);

                if (api.매수1호가 > 0)
                {
                    deal.수익률 = (api.매수1호가 - deal.장부가) / api.매수1호가 * 100;
                }

                g.StockManager.HoldingList.Add(code);
            }

            // Remove 보유종목 from 호가종목
            foreach (string code in g.StockManager.HoldingList)
            {
                g.StockManager.InterestedWithBidList.Remove(code);
            }

            DealHold_order();
        }

        public static void DealHold_order()
        {
            var stocksTuple = new List<Tuple<long, string>>();

            var repo = g.StockRepository;
            foreach (var code in g.StockManager.HoldingList)
            {
                if (!repo.Contains(code))
                    continue;

                var stock = repo.TryGetStockOrNull(code);
                if (stock == null) continue;
                long holdingValue = stock.Deal.보유량 * stock.Api.현재가;

                stocksTuple.Add(Tuple.Create(holdingValue, code));
            }

            // Sort descending by holding value
            var sorted = stocksTuple.OrderByDescending(t => t.Item1).ToList();

            g.StockManager.HoldingList.Clear();
            foreach (var item in sorted)
            {
                g.StockManager.HoldingList.Add(item.Item2);
            }
        }

        public static void DealExec(string buyOrSell, string stockName, long quantity, long price, string orderType) // tr(1)
        {
            const string orderCondition = "0"; // 주문조건구분: "0" 없음, "1" IOC, "2" FOK

            TradeInit();
            if (!_checkedTradeInit || quantity == 0)
                return;

            string stockCode = _cpstockcode.NameToCode(stockName); // 종목 이름 → 코드 변환
            if (string.IsNullOrEmpty(stockCode))
                return;

            _cptd0311 = new CPTRADELib.CpTd0311();

            if (_cptd0311.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            // 매수: 2, 매도: 1
            string buyOrSellCode = (buyOrSell == "매수") ? "2" : "1";

            _cptd0311.SetInputValue(0, buyOrSellCode);    // 주문유형
            _cptd0311.SetInputValue(1, g.Account);        // 계좌번호
            _cptd0311.SetInputValue(2, "01");             // 상품구분코드
            _cptd0311.SetInputValue(3, stockCode);        // 종목코드
            _cptd0311.SetInputValue(4, quantity);         // 주문수량
            _cptd0311.SetInputValue(5, price);            // 주문단가
            _cptd0311.SetInputValue(7, orderCondition);   // 주문조건
            _cptd0311.SetInputValue(8, orderType);        // 주문호가구분

            int result = _cptd0311.BlockRequest();

            // Optional logging
            if (result != 0)
            {
                Trace.TraceError($"[Order Failed] {buyOrSell} {stockName} x{quantity} @ {price}, result code: {result}");
            }
        }


        public static void DealCancelRowIndex(int rowindex)
        {
            var data = OrderItemTracker.GetOrderByRowIndex(rowindex);
            if (data == null)
                return;

            DealCancelOrder(data);
        }


        public static void DealCancelOrder(OrderItem data)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
            {
                MessageBox.Show("Trade initialization failed.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            int 원주문번호 = data.m_ordKey;
            string stockcode = _cpstockcode.NameToCode(data.stock);

            if (string.IsNullOrEmpty(stockcode))
            {
                MessageBox.Show("Invalid stock code.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _cptd0314 = new CPTRADELib.CpTd0314();

            _cptd0314.SetInputValue(1, 원주문번호);     // 원주문번호
            _cptd0314.SetInputValue(2, g.Account);     // 계좌번호
            _cptd0314.SetInputValue(3, "01");          // 상품관리구분코드
            _cptd0314.SetInputValue(4, stockcode);     // 종목코드
            _cptd0314.SetInputValue(5, 0);             // 0: 전체 취소

            int result = _cptd0314.BlockRequest();
            if (result != 0)
            {
                MessageBox.Show($"Order cancellation failed with result code: {result}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public static void DealCancelStock(string stock) // tr(1)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            string stockCode = _cpstockcode.NameToCode(stock);
            if (string.IsNullOrEmpty(stockCode))
                return;

            var orders = OrderItemTracker.OrderMap.Values
                .Where(o => o.stock == stock)
                .ToList(); // ← this makes 'orders' a List<T>

            if (orders.Count() == 0)
                return;

            foreach (var data in orders)
            {
                int 원주문번호 = data.m_ordOrgKey;

                _cptd0314 = new CPTRADELib.CpTd0314();
                _cptd0314.SetInputValue(1, 원주문번호);     // 원주문번호
                _cptd0314.SetInputValue(2, g.Account);      // 계좌번호
                _cptd0314.SetInputValue(3, "01");           // 상품관리구분코드
                _cptd0314.SetInputValue(4, stockCode);      // 종목코드
                _cptd0314.SetInputValue(5, 0);              // 0: 전체 취소

                int result = _cptd0314.BlockRequest();
                if (result != 0)
                {
                    Trace.TraceWarning($"[Cancel Failed] {stock}, OrderKey: {원주문번호}, result: {result}");
                }
            }
        }

        private string DealCorrect(long 원주문번호, string stock, long 주문수량, long 주문단가) // tr(1)
        {
            if (!g.connected)
                return "";

            TradeInit();
            if (!_checkedTradeInit)
                return "";

            string stockCode = _cpstockcode.NameToCode(stock);
            if (string.IsNullOrEmpty(stockCode))
                return "Invalid stock code.";

            _cptd0303 = new CPTRADELib.CpTd0303();

            // 정정주문 입력
            _cptd0303.SetInputValue(1, 원주문번호);       // 원주문번호
            _cptd0303.SetInputValue(2, g.Account);        // 계좌번호
            _cptd0303.SetInputValue(3, "1");              // 상품관리구분코드
            _cptd0303.SetInputValue(4, stockCode);        // 종목코드
            _cptd0303.SetInputValue(5, 주문수량);         // 정정 수량
            _cptd0303.SetInputValue(6, 주문단가);         // 정정 단가

            int result = _cptd0303.BlockRequest();

            if (result != 0)
            {
                Trace.TraceWarning($"[정정실패] {stock} / 수량: {주문수량}, 단가: {주문단가}, 원주문번호: {원주문번호}");
            }

            return _cptd0303.GetDibMsg1();
        }

    }
}
