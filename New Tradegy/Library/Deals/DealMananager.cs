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

namespace New_Tradegy.Library
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

        public static int UpdateDealProfit()
        {
            TradeInit();
            if (!_checkedTradeInit)
                return 0;

            _profitQuery = new CpTd5342();
            if (_profitQuery.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return 0;
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
                g.DealProfit = ((int)(gross - cost) / 10000);

                if (g.MarketeyeCount == 0)
                {
                    g.DealProfit = 0;
                }

                if (g.제어.dtb.Rows[1][0].ToString() != g.DealProfit.ToString())
                {
                    g.제어.dtb.Rows[1][0] = g.DealProfit.ToString();
                }

                return g.DealProfit;
            }

            return g.DealProfit;
        }


        public static void UpdateAvailableDeposit()
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
            _depositQuery.SetInputValue(6, '1');         // '1': 금액조회

            int result = _depositQuery.BlockRequest();

            string message = _depositQuery.GetDibMsg1();
            long cashAvailable = Convert.ToInt64(_depositQuery.GetHeaderValue(10));
            g.예치금 = (int)(cashAvailable / 10000); // 현금주문가능금액 단위변환

            if (g.제어.dtb.Rows[0][3].ToString() != g.예치금.ToString())
            {
                g.제어.dtb.Rows[0][3] = g.예치금.ToString();
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

                OrderTracker.Add(item);
            }
        }


        public static bool CheckPreviousLoss(string stockSymbol)
        {
            var stock = StockRepository.Instance.GetOrThrow(stockSymbol);
            if (stock == null) return false; // Stock not found

            // Ensure valid purchase price exists and at least 1 share is held
            if (stock.Api.매수1호가 > 0 && stock.Deal.보유량 >= 1)
            {
                double 수익률 = (double)(stock.Api.매수1호가 - stock.Deal.장부가) / stock.Api.매수1호가 * 100;

                if (수익률 < -0.45 && stock.Deal.평가금액 > 4_500_000)
                {
                    mc.Sound("alarm", "lost already");
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

            g.보유종목.Clear();

            // Reset all stocks' 보유량
            foreach (var stock in StockRepository.Instance.AllDatas)
            {
                stock.Deal.보유량 = 0;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                string code = (string)_cptd6033.GetDataValue(0, i);

                if (!StockRepository.Instance.Contains(code))
                    continue;

                var stock = StockRepository.Instance.GetOrThrow(code);
                var api = stock.Api;
                var deal = stock.Deal;

                deal.보유량 = (int)_cptd6033.GetDataValue(15, i);
                deal.장부가 = (double)_cptd6033.GetDataValue(17, i);
                deal.평가금액 = (long)_cptd6033.GetDataValue(9, i);
                deal.손익단가 = (long)_cptd6033.GetDataValue(18, i);

                if (api.매수1호가 > 0)
                {
                    deal.수익률 = (api.매수1호가 - deal.장부가) / api.매수1호가 * 100;
                }

                g.보유종목.Add(code);
            }

            // Remove 보유종목 from 호가종목
            foreach (string code in g.보유종목)
            {
                g.호가종목.Remove(code);
            }

            deal_hold_order();
        }

        public static void deal_hold_order()
        {
            var stocks = new List<Tuple<long, string>>();

            foreach (var code in g.보유종목)
            {
                if (!StockRepository.Instance.Contains(code))
                    continue;

                var stock = StockRepository.Instance.GetOrThrow(code);
                long holdingValue = stock.Deal.보유량 * stock.Api.현재가;

                stocks.Add(Tuple.Create(holdingValue, code));
            }

            // Sort descending by holding value
            var sorted = stocks.OrderByDescending(t => t.Item1).ToList();

            g.보유종목.Clear();
            foreach (var item in sorted)
            {
                g.보유종목.Add(item.Item2);
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
            var data = OrderTracker.GetOrderByRowIndex(rowindex);
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

            var orders = OrderTracker.OrderMap.Values
                .Where(o => o.stock == stock)
                .ToList();

            if (orders.Count == 0)
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

        public static void 설정매수수량_설정매도수량(string stock, DataGridView dgv2, ref int 설정매수수량, ref int 설정매도수량)
        {
            설정매수수량 = 0;
            설정매도수량 = 0;

            for (int i = 0; i < dgv2.Rows.Count; i++)
            {
                var row = dgv2.Rows[i];

                // Skip if already partially or fully executed (e.g., "20/100")
                if (row.Cells[2].Value?.ToString().Contains("/") == true)
                    continue;

                // Check if this row corresponds to the target stock
                if (row.Cells[0].Value?.ToString() == stock)
                {
                    if (!int.TryParse(row.Cells[2].Value?.ToString(), out 설정매수수량))
                        설정매수수량 = 0;

                    if (!int.TryParse(row.Cells[3].Value?.ToString(), out 설정매도수량))
                        설정매도수량 = 0;

                    break; // Found and filled — exit loop early
                }
            }
        }


        public static void EvalStock()
        {
            var repo = StockRepository.Instance;
            var result = new List<(double value, string code)>();
            double value = 0.0;

            var specialGroups = new HashSet<string> { "닥올", "피올", "편차", "평균" };

            if (specialGroups.Contains(g.v.KeyString))
            {
                foreach (var stock in repo.AllDatas)
                {
                    switch (g.v.KeyString)
                    {
                        case "피올":
                            if (stock.Statistics.시장구분 == 'S')
                                result.Add((stock.Statistics.시총, stock.Stock));
                            break;

                        case "닥올":
                            if (stock.Statistics.시장구분 == 'D')
                                result.Add((stock.Statistics.시총, stock.Stock));
                            break;

                        case "편차":
                            result.Add((stock.Statistics.일간변동편차, stock.Stock));
                            break;

                        case "평균":
                            result.Add((stock.Statistics.일간변동평균, stock.Stock));
                            break;
                    }
                }
            }
            else
            {
                foreach (var stock in repo.AllDatas)
                {
                    ps.post(stock); // Calculate 프누천, 외누천, 종거천, etc.

                    int checkRow = g.test ? Math.Min(g.Npts[1] - 1, stock.Api.nrow - 1) : stock.Api.nrow - 1;
                    if (!eval_inclusion(stock) || stock.Api.nrow < 2)
                        continue;

                    switch (g.v.KeyString)
                    {
                        case "총점":
                            value = stock.Score.총점;
                            break;

                        case "프누":
                            value = stock.Post.프누천 + stock.Post.외누천;
                            break;

                        case "종누":
                            value = stock.Post.종거천;
                            break;

                        case "프편":
                            value = (stock.Post.프누천 + stock.Post.외누천) / stock.Statistics.프분_dev;
                            break;

                        case "종편":
                            value = stock.Post.종거천 / stock.Statistics.프분_dev;
                            break;

                        case "푀분":
                            value = stock.Api.분프로천[0] + stock.Api.분외인천[0];
                            break;

                        case "배차":
                            value = stock.Api.분배수차[0];
                            break;

                        case "가증":
                            value = stock.Api.x[checkRow, 1] - stock.Api.x[checkRow - 1, 1];
                            break;

                        case "분거":
                            value = stock.Api.분거래천[0];
                            break;

                        case "상순":
                            value = stock.Api.x[checkRow, 1];
                            break;

                        case "저순":
                            value = -stock.Api.x[checkRow, 1];
                            break;
                    }

                    result.Add((value, stock.Stock));
                }
            }

            // Sorting by descending value
            result = result.OrderByDescending(r => r.value).ToList();

            // Update global ranking list (g.sl)
            lock (g.lockObject)
            {
                g.sl.Clear();
                foreach (var item in result)
                {
                    if (!g.sl.Contains(item.code))
                        g.sl.Add(item.code);
                }

                string newValue = $"{g.sl.Count}/{repo.AllDatas.Count()}";
                if (g.제어.dtb.Rows[1][1].ToString() != newValue)
                {
                    g.제어.dtb.Rows[1][1] = newValue;
                }
            }

            ev.eval_group(); // Optional post-group processing
        }


        public static bool eval_inclusion(StockData data)
        {
            string stock = data.Stock;
            var score = data.Score;
            var stat = data.Statistics;
            var post = data.Post;
            var api = data.Api;

            // Exclude ETFs and specific groups
            if ((g.KODEX4.Contains(stock) && !stock.Contains("레버리지")) ||
                stock.Contains("KODEX") ||
                stock.Contains("KOSEF") ||
                stock.Contains("HANARO") ||
                stock.Contains("TIGER") ||
                stock.Contains("KBSTAR") ||
                stock.Contains("혼합") ||
                g.보유종목.Contains(stock) ||
                g.호가종목.Contains(stock) ||
                g.관심종목.Contains(stock))
            {
                return false;
            }

            // Always include for 누적 types
            if (g.v.KeyString is "프누" or "종누" or "프편" or "종편")
                return true;

            // 필터링 조건 - 음수 배제
            if (g.v.푀플 == 1 && score.푀분 < 0)
                return false;

            if (g.v.배플 == 1 && score.배차 < 0)
                return false;

            // 추정거래액 조건
            if (g.v.종가기준추정거래액이상_천만원 > (int)post.종거천)
                return false;

            // 장중일 경우 실시간 호가 거래액 체크
            if (wk.isWorkingHour())
            {
                if (post.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
                    post.매수호가거래액_백만원 < g.v.호가거래액_백만원)
                    return false;

                if (g.v.분당거래액이상_천만원 > api.분거래천[0])
                    return false;
            }

            // 변동성 필터
            if (g.v.편차이상 >= stat.일간변동편차)
                return false;

            // 시가총액 필터 (양/음에 따라 다르게 처리)
            if (g.v.시총이상 >= 0)
            {
                if (stat.시총 < g.v.시총이상 - 0.01)
                    return false;
            }
            else
            {
                if (stat.시총 > (g.v.시총이상 - 0.01) * -1.0)
                    return false;
            }

            return true;
        }

    }
}
