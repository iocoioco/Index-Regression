using CPTRADELib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Models;
namespace New_Tradegy.Library
{
    public class DealManager
    {
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        private static readonly CPUTILLib.CpCodeMgr _cpcodemgr = new CPUTILLib.CpCodeMgr();
        public static CPUTILLib.CpCybos _cpcybos;
        private static CPTRADELib.CpTdUtil _cptdutil;
        private CPTRADELib.CpTd0303 _cptd0303; //주식 주문관리 (매매 유형 정정 주문) 데이터를 요청
        private static CPTRADELib.CpTd0311 _cptd0311; //주문(현금 주문) 데이터를 요청
        private readonly CPTRADELib.CpTd0312 _cptd0312; //신용 주문 데이터를 요청
        private readonly CPTRADELib.CpTd0313 _cptd0313; //주문관리 (가격 정정 주문) 데이터를 요청
        private readonly CPTRADELib.CpTd0732 _cptd0732; //주식 결제예정 예수금 가계산 데이터를 요청
        private static CPTRADELib.CpTd0314 _cptd0314; // 주문관리 (취소 주문) 데이터를 요청
        private static CPTRADELib.CpTdNew5331A _cptdnew5331a; //계좌별 매수주문 가능 금액/수량 데이터를 요청
        private readonly CPTRADELib.CpTdNew5331B _cptdnew5331b; //계좌별 매도주문 가능 수량 데이터를 요청
        private static CPTRADELib.CpTd5339 _cptd5339; //계좌별 미체결 잔량 데이터를 요청
        private readonly CPTRADELib.CpTd5341 _cptd5341; //금일 계좌별 주문/체결 내역 조회 데이터를 요청ㅍ
        private static CPTRADELib.CpTd6033 _cptd6033; //계좌별 잔고 및 주문체결 평가 현황 데이터를 요청
        private static CPTRADELib.CpTd5342 _CpTd5342;
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

        public static int deal_profit()
        {

            TradeInit();
            if (_checkedTradeInit == false)
                return 0;

            _CpTd5342 = new CPTRADELib.CpTd5342();
            if (_CpTd5342.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return 0;
            }


            _CpTd5342.SetInputValue(0, g.Account);
            _CpTd5342.SetInputValue(1, "01"); // 상품관리구분코드
            _CpTd5342.SetInputValue(2, 20); // 요청개수 - 최대 20개


            _CpTd5342.SetInputValue(3, "1"); // "1" : 금일, "2" : 전일


            _CpTd5342.SetInputValue(4, ""); // 요청종목코드[default:"" - 모든종목]



            int result = _CpTd5342.BlockRequest();


            if (_CpTd5342.Continue == 1)
            {
                // "연속 데이터 있음";
            }
            else
            {
                // "연속 데이터 없음";
            }

            if (result == 0)
            {
                ulong a = _CpTd5342.GetHeaderValue(9);
                ulong b = _CpTd5342.GetHeaderValue(10);
                g.deal_profit = ((int)(a - b) / 10000);
                if (g.MarketeyeCount == 0)
                {
                    g.deal_profit = 0;
                }
                if (g.제어.dtb.Rows[1][0].ToString() != g.deal_profit.ToString())
                {
                    g.제어.dtb.Rows[1][0] = g.deal_profit.ToString();
                }
                
                return g.deal_profit;
            }
            return g.deal_profit;
        }


        public static void deal_deposit() // tr(1)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            _cptdnew5331a = new CpTdNew5331A();

            int result = 1;

            if (_cptdnew5331a.GetDibStatus() == 1)
            {
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                return;
            }

            _cptdnew5331a.SetInputValue(0, g.Account);// 계좌번호
            _cptdnew5331a.SetInputValue(1, "01"); // 상품관리구분코드 : 거래소
                                                  //_cptdnew5331a.SetInputValue(2, ""); // 종목코드
                                                  //_cptdnew5331a.SetInputValue(3, "01"); // 주문호가구분코드, "01" : 보통 "02" 임의, "03" 시장가
                                                  //_cptdnew5331a.SetInputValue(4, 0); // 주문단가, default : 0(long)
            _cptdnew5331a.SetInputValue(5, "Y");  // N 계좌별deposit, Y deposit 100
            _cptdnew5331a.SetInputValue(6, '1'); // '1' 금액조회, '2' 수량조회

            result = _cptdnew5331a.BlockRequest();

            string textBox = _cptdnew5331a.GetDibMsg1();

            g.예치금 = (int)(Convert.ToInt64(_cptdnew5331a.GetHeaderValue(10)) / 10000); // 현금주문가능금액 

            if(g.제어.dtb.Rows[0][3].ToString() != g.예치금.ToString())
            {
                g.제어.dtb.Rows[0][3] = g.예치금.ToString();
            }
            
        }

        public static void deal_processing() // tr(1)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            _cptd5339 = new CPTRADELib.CpTd5339();

            string textBox = "";
            int result = 1;

            //if (_cptdnew5331a.GetDibStatus() == 1)
            //{
            //	Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
            //	return;
            //}

            _cptd5339.SetInputValue(0, g.Account); // 계좌번호
            _cptd5339.SetInputValue(1, "01");    // 상품관리구분코드 : 거래소주식
                                                 //_cptd5339.SetInputValue(3, "A006800"); // 생략하면 전체 조회
            _cptd5339.SetInputValue(4, "0"); // 전체
            _cptd5339.SetInputValue(5, "0"); // 순
                                             // 차 
            _cptd5339.SetInputValue(7, 20); // 최대

            result = _cptd5339.BlockRequest();

            textBox = _cptd5339.GetDibMsg1();

            // 미체결 (매수, 매도 포함)
            string 계좌번호 = (string)_cptd5339.GetHeaderValue(0); // 계좌번호 안 나오네 iklee217에서는 나오는 데 
            string 계좌명 = (string)_cptd5339.GetHeaderValue(4); // 계좌명
            int count = (int)_cptd5339.GetHeaderValue(5); // 획득 데이터 숫자 



            if (result == 0)
            {
                //if (g.m_mapOrder != null)
                //    g.m_mapOrder.Clear(); // ERR

                for (int i = 0; i < int.Parse(_cptd5339.GetHeaderValue(5).ToString()); i++)
                {
                    OrderItem item = new OrderItem();
                    item.stock = (string)_cptd5339.GetDataValue(4, i); // 종목명
                    item.m_ordKey = (int)_cptd5339.GetDataValue(1, i); // 주문번호
                    item.m_ordOrgKey = (int)_cptd5339.GetDataValue(2, i);
                    item.m_sCode = _cpstockcode.NameToCode(item.stock);
                    //item.m_sText = objRq.GetDataValue(5, i);        // 주문 내용

                    item.m_nAmt = (int)_cptd5339.GetDataValue(6, i); // 주문수량
                    item.m_nPrice = (int)_cptd5339.GetDataValue(7, i); // 주문단가
                    item.m_nContAmt = (int)_cptd5339.GetDataValue(8, i); // 체결수량
                    item.m_nModAmt = (int)_cptd5339.GetDataValue(11, i); // 정정취소가능수량

                    if ((string)_cptd5339.GetDataValue(13, i) == "1")
                        item.buyorSell = "매도";
                    else if ((string)_cptd5339.GetDataValue(13, i) == "2")
                        item.buyorSell = "매수";

                    item.m_sHogaFlag = _cptd5339.GetDataValue(21, i); // 주문호가구분코드내용

                    OrderTracker.Add(item);


                }
            }
        }

        // Function to check if the stock is in loss
        public static bool CheckPreviousLoss(string stockSymbol)
        {
            int index = wk.return_index_of_ogldata(stockSymbol);
            if (index < 0) return false; // Stock not found in holdings

            g.stock_data o = g.ogl_data[index];

            //? no watering
            if (o.매수1호가 > 0 && o.보유량 >= 1) // Ensure valid purchase price exists
            {
                double 수익률 = (double)(o.매수1호가 - o.장부가) / o.매수1호가 * 100;

                if (수익률 < -0.45 && o.평가금액 > 4500000) // If loss detected and eval amount is over 1M
                {
                    mc.Sound("alarm", "lost already");
                    return true; // Block trade
                }
            }
            return false;
        }

        public static void deal_hold() // tr(1)
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

            int result = 1;

            _cptd6033.SetInputValue(0, g.Account); // 계좌번호
            _cptd6033.SetInputValue(1, "01");    // 상품관리구분코드
            _cptd6033.SetInputValue(2, 50);    // 요청건수(최대: 50[default : 14]) // blocked on 20210110 due to trouble

            result = _cptd6033.BlockRequest(); // 축 개체 에러 발생 20200716

            if (result == 0)
            {
                int count = (int)_cptd6033.GetHeaderValue(7); // 보유종목 수, 최대 50개 가능
                if (count > 51) // 51개 이상이면 아상 발생
                    return;

                g.보유종목.Clear();
                foreach (g.stock_data t in g.ogl_data)
                {
                    t.보유량 = 0;
                }

                for (int i = count - 1; i >= 0; i--)
                {
                    string 종목 = (string)_cptd6033.GetDataValue(0, i);

                    int index = wk.return_index_of_ogldata(종목);
                    if (index < 0)
                        continue;
                    g.stock_data o = g.ogl_data[index];

                    if (index >= 0)
                    {
                        o.보유량 = (int)_cptd6033.GetDataValue(15, i);
                        //o.수익률 = (double)_cptd6033.GetDataValue(11, i); 전체 통일위해 다음 라인 사용
                        if (o.매수1호가 > 0)
                            o.수익률 = (o.매수1호가 - o.장부가) / (double)o.매수1호가 * 100;
                        o.장부가 = (double)_cptd6033.GetDataValue(17, i);
                        o.평가금액 = (long)_cptd6033.GetDataValue(9, i);
                        o.손익단가 = (long)_cptd6033.GetDataValue(18, i);

                        g.보유종목.Add(o.stock); // 20201104
                    }
                }

                // 보유종목에 있고 호가종목에 있으면 호가종목에서 제외
                foreach (string stock in g.보유종목)
                {
                    if (g.호가종목.Contains(stock))
                        g.호가종목.Remove(stock);
                }
                deal_hold_order();
            }
        }

        public static void deal_hold_order()
        {
            var stocks = new List<Tuple<int, string>> { };

            foreach (var stock in g.보유종목)
            {
                int index = wk.return_index_of_ogldata(stock);
                if (index < 0)
                    return;
                g.stock_data o = g.ogl_data[index];

                int total_amount = o.보유량 * (int)o.현재가;
                stocks.Add(Tuple.Create(total_amount, stock));
            }

            stocks = stocks.OrderByDescending(t => t.Item1).ToList();
            g.보유종목.Clear();

            foreach (var item in stocks)
                g.보유종목.Add(item.Item2);

            
        }

        public static void deal_exec(string buyorsell, string stock,
        long numberofstock, long price, string 주문호가구분) // tr(1)
        {
            string 주문조건구분 = "0"; // 없음
                                 // 주문호가구분 = "01"; // 지정가, "03" 시장가

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            if (numberofstock == 0)
                return;

            string stockcode = _cpstockcode.NameToCode(stock);
            _cptd0311 = new CPTRADELib.CpTd0311();

            if (_cptd0311.GetDibStatus() == 1)
                Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");

            if (buyorsell == "매수")
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
            _cptd0311.SetInputValue(4, numberofstock);  // 주문수량 
            _cptd0311.SetInputValue(5, price);    // 매수단가
            _cptd0311.SetInputValue(7, 주문조건구분);  // 주문조건구분("0":없음,"1":IOC, "2":FOK)
            _cptd0311.SetInputValue(8, 주문호가구분);    // 주문호가구분("01":보통,"02":임의,"03":시장가,"05":조건부지정가 etc)

            int check = _cptd0311.BlockRequest();
        }

        //주문관리(취소 주문)
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


        // called when the amount of stock is less than that of selling
        public static void DealCancelStock(string stock) // tr(1)
        {
            if (!g.connected)
                return;

            TradeInit();
            if (_checkedTradeInit == false)
                return;

            var keyList = OrderTracker.OrderMap.Keys.ToList();

            foreach (var kvp in OrderTracker.OrderMap)
            {
                var data = kvp.Value;

                if (data.stock == stock)
                {
                    int 원주문번호 = data.m_ordOrgKey; // CpTd0314 manual 상 원주문번호로 되어있음
                    string stockcode = _cpstockcode.NameToCode(data.stock);
                    _cptd0314 = new CPTRADELib.CpTd0314();

                    // 미체결 주문번호입력에 따른 선택적 취소
                    _cptd0314.SetInputValue(1, 원주문번호);// 원주문번호 
                    _cptd0314.SetInputValue(2, g.Account);// 계좌번호 
                    _cptd0314.SetInputValue(3, "01"); // 상품관리구분코드
                    _cptd0314.SetInputValue(4, stockcode);// 종목코드
                    _cptd0314.SetInputValue(5, 0);// 0 : 전체 취소, 숫자입력 : 입력숫자만큼 취소

                    int result = _cptd0314.BlockRequest();
                }
            }
        }

     
        // not used
        private string deal_correct(long 원주문번호, string stock, long 주문수량, long 주문단가) // tr(1)
        {
            if (!g.connected)
                return "";

            TradeInit();
            if (_checkedTradeInit == false)
                return "";

            string stockcode = _cpstockcode.NameToCode(stock);
            _cptd0303 = new CPTRADELib.CpTd0303();

            // 정정 : 주문번호가 아닌 원주문번호 입력 에러 발생
            _cptd0303.SetInputValue(1, 원주문번호); // 원주문번호
            _cptd0303.SetInputValue(2, g.Account); // 계좌번호
            _cptd0303.SetInputValue(3, "1");   // 상품관리구분코드
            _cptd0303.SetInputValue(4, stockcode);   // 종목코드
            _cptd0303.SetInputValue(5, 주문수량);   // 주문수량
            _cptd0303.SetInputValue(6, 주문단가);   // 주문단가

            int result = _cptd0303.BlockRequest();

            return _cptd0303.GetDibMsg1();
        }


        // not used
        //public static string deal_trade_매수점검(g.stock_data o) // tr(1)
        //{
        //    // warning message
        //    string message = "";

        //    // price, amount, intensity
        //    #region
        //    // price
        //    int total_differ = 0;
        //    int differ = 0;
        //    if (o.nrow >= 2)
        //        total_differ = o.x[o.nrow - 1, 1] - o.x[o.nrow - 2, 1];

        //    for (int i = o.nrow - 2; i >= 0; i--)
        //    {
        //        if (i <= 0) break;
        //        differ = o.x[i, 1] - o.x[i - 1, 1];
        //        if (total_differ * differ < 0 && Math.Abs(differ) > g.v.neglectable_price_differ)
        //            break;
        //        else total_differ += differ;
        //    }
        //    message = total_differ.ToString();
        //    for (int i = 1; i < 5; i++)
        //    {
        //        if (o.nrow - i - 1 < 0)
        //            break;
        //        differ = o.x[o.nrow - i, 1] - o.x[o.nrow - i - 1, 1];
        //        if (differ >= 0)
        //            message += "\t" + "+" + differ.ToString();
        //        else
        //            message += "\t" + differ.ToString();
        //    }
        //    message += "\t" + "  // price " + "\n";

        //    // amount
        //    message += o.x[o.nrow - 1, 2].ToString();
        //    for (int i = 1; i < 5; i++)
        //    {
        //        if (o.nrow - i - 1 < 0)
        //            break;
        //        differ = o.x[o.nrow - i, 2] - o.x[o.nrow - i - 1, 2];
        //        if (differ >= 0)
        //            message += "\t" + "+" + differ.ToString();
        //        else
        //            message += "\t" + differ.ToString();
        //    }
        //    message += "\t" + "  // amount " + "\n";

        //    // intensity
        //    message += (o.x[o.nrow - 1, 3] / 100).ToString();
        //    for (int i = 1; i < 5; i++)
        //    {
        //        if (o.nrow - i - 1 < 0)
        //            break;
        //        differ = (o.x[o.nrow - i, 3] - o.x[o.nrow - i - 1, 3]) / 100;
        //        if (differ >= 0)
        //            message += "\t" + "+" + differ.ToString();
        //        else
        //            message += "\t" + differ.ToString();
        //    }
        //    message += "\t" + "  // intensity " + "\n\n";
        //    #endregion

        //    // tick multiple & minute multiple
        //    #region
        //    for (int i = 0; i < 5; i++)
        //        message += o.틱매수배[i].ToString() + "/" + o.틱매수배[i].ToString() + " \t";
        //    message += "  // tick multiple " + "\n";

        //    for (int i = 0; i < 5; i++)
        //        message += o.분매수배[i].ToString() + "/" + o.분매수배[i].ToString() + " \t";
        //    message += "  // minute multiple " + "\n\n";
        //    #endregion

        //    // 틱프로천, 틱외인천 & 분프로천, 분외인천
        //    #region
        //    double money_factor = o.현재가 / g.천만원; // 천만원

        //    double program_money = 0.0;
        //    double foreign_money = 0.0;
        //    double dealt_money = 0.0;
        //    // str_add += String.Format("{0,12}", o.x[j, k] + " / " + o.x[j, k+1]); right justify
        //    // str_add += String.Format("{0,-12}", o.x[j, k] + " / " + o.x[j, k+1]); left justify

        //    // 틱프로천, 틱외인천
        //    for (int i = 0; i < 4; i++)
        //    {
        //        program_money = (o.틱프로량[i] - o.틱프로량[i + 1]) * money_factor;
        //        foreign_money = (o.틱외인량[i] - o.틱외인량[i + 1]) * money_factor;
        //        dealt_money = (o.틱매수량[i] - o.틱매수량[i + 1] +
        //                                             o.틱매도량[i] - o.틱매도량[i + 1]) * money_factor;

        //        if (foreign_money >= 0)
        //            message += String.Format("{0,-22}",
        //                             program_money.ToString("F1") + "+" +
        //                             foreign_money.ToString("F1") + "/" +
        //                             dealt_money.ToString("F1"));
        //        else
        //            message += String.Format("{0,-22}",
        //                             program_money.ToString("F1") +
        //                             foreign_money.ToString("F1") + "/" +
        //                             dealt_money.ToString("F1"));
        //    }

        //    // 분프로천, 분외인천
        //    for (int i = 0; i < 4; i++)
        //    {
        //        program_money = (o.x[o.nrow - i - 1, 4] - o.x[o.nrow - i - 2, 4]) * money_factor;
        //        foreign_money = (o.x[o.nrow - i - 1, 5] - o.x[o.nrow - i - 2, 5]) * money_factor;
        //        dealt_money = (o.x[o.nrow - i - 1, 7] - o.x[o.nrow - i - 2, 7]) * money_factor;

        //        if (foreign_money >= 0)
        //            message += String.Format("{0,-22}",
        //                             program_money.ToString("F1") + "+" +
        //                             foreign_money.ToString("F1") + "/" +
        //                             dealt_money.ToString("F1"));
        //        else
        //            message += String.Format("{0,-22}",
        //                             program_money.ToString("F1") +
        //                             foreign_money.ToString("F1") + "/" +
        //                             dealt_money.ToString("F1"));
        //    }
        //    message += "\n";

        //    message +=
        //               Math.Round(o.프누천 / 10.0) + "  " +
        //               Math.Round(o.외누천 / 10.0) + "  " +
        //               Math.Round(o.기누천 / 10.0) + "\t\t" +
        //               o.x[o.nrow - 1, 10] + "/" + o.x[o.nrow - 1, 11] + "\t\t" + o.일간변동평균편차;
        //    message += "\n";
        //    #endregion

        //    // 호가차
        //    #region
        //    long 호가차 = 0;
        //    if (o.현재가 < 2000) 호가차 = 1;
        //    else if (o.현재가 < 5000) 호가차 = 5;
        //    else if (o.현재가 < 20000) 호가차 = 10;
        //    else if (o.현재가 < 50000) 호가차 = 50;
        //    else if (o.현재가 < 200000) 호가차 = 100;
        //    else if (o.현재가 < 500000) 호가차 = 500;
        //    else 호가차 = 1000;

        //    if (호가차 != 0)
        //    {
        //        if ((o.매도1호가 - o.매수1호가) / 호가차 != 1)
        //            message += "1호가차 이상";
        //    }


        //    // 0 - 2000 1
        //    // 2,000 - 5,000 5
        //    // 5,000 - 20,000 10
        //    // 20,000 - 50,000 50
        //    // 50,000 - 200,000 100
        //    // 200,000 - 500,000 500
        //    // 500,000 - 1,000
        //    #endregion

        //    message += "\n";

        //    //+                      "틱거 미약\n" +


        //    //                            "호가차 갭 점검\n" +

        //    //                            "거분, 프외, 프퍼 한호가 차이 갭 있는 여부    거분, 프외, 프퍼" +
        //    //                            "거분, 프외, 프퍼 한호가 차이 갭 있는 여부    거분, 프외, 프퍼 \n";

        //    return message;
        //}

        // not implemented,
        // the buy or sell data saved in g.매매대기.Add(t) and
        //executed in  dgv2_updated_trade_exec()
        //public static void deal_trade_최적거래(string buyorsell, string stock,
        //long numberofstock, long price, string 주문조건구분, string 주문호가구분) // tr(1)
        //{
        //    if (!g.connected)
        //        return;

        //    if (numberofstock == 0)
        //        return;

        //    mc.Sound_돈(g.일회거래액);

        //    //g.trade_waiting t = new g.trade_waiting();

        //    if (buyorsell == "매수")
        //    {
        //        t.buy_or_sell = "매수";

        //        int index = wk.return_index_of_ogldata(stock);
        //        if (index < 0)
        //            return;
        //        g.stock_data o = g.ogl_data[index];

        //        if (g.confirm_buy)
        //        {
        //            string caption = stock + " : " + price.ToString() + " X " + numberofstock.ToString() +
        //               " = " + (numberofstock * price / 10000).ToString() + "만원";
        //            string message = "                     매수 ? \n" +
        //                "test \n" +
        //                "done";

        //            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        //            DialogResult result;
        //            // Displays the MessageBox
        //            result = MessageBox.Show(message, caption, buttons);

        //            if (result == System.Windows.Forms.DialogResult.No)
        //            {
        //                return;
        //            }
        //        }
        //    }

        //    else // sell
        //    {
        //        int index = wk.return_index_of_ogldata(stock);
        //        if (index < 0)
        //            return;
        //        g.stock_data o = g.ogl_data[index];

        //        if (g.confirm_sell) // && 주문호가구분 != "03") // dgv2를 클릭하여 매도할 경우, 우발적 클릭을 방지하기 위해 확인 후 매도
        //                            // "03" <- 시장가 매도 ... 비상매도
        //        {
        //            string caption = stock + " : " + price.ToString() + " X " + numberofstock.ToString() +
        //               " = " + (numberofstock * price / 10000).ToString() + "만원";
        //            string message = "                  매도 ?";

        //            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        //            DialogResult result;
        //            // Displays the MessageBox
        //            result = MessageBox.Show(message, caption, buttons);

        //            if (result == System.Windows.Forms.DialogResult.No)
        //            {
        //                return;
        //            }
        //        }
        //        t.buy_or_sell = "매도";
        //    }

        //    t.number = numberofstock;
        //    t.price = price;

        //    string stockcode = _cpstockcode.NameToCode(stock);
        //    t.code = stockcode;

        //    g.매매대기.Add(t);
        //}


        // not used
        public static void 설정매수수량_설정매도수량(string stock, DataGridView dgv2, ref int 설정매수수량, ref int 설정매도수량)
        {
            for (int i = 0; i < dgv2.Rows.Count; i++)
            {
                if (!dgv2.Rows[i].Cells[2].Value.ToString().Contains("/")) // 체결종목이 아니면  
                {
                    if (stock == dgv2.Rows[i].Cells[0].Value.ToString()) // 보유 관심 종목이면
                    {
                        bool success = int.TryParse((string)dgv2.Rows[i].Cells[2].Value, out 설정매수수량);
                        if (!success)
                        {
                            설정매수수량 = 0;
                        }

                        success = int.TryParse((string)dgv2.Rows[i].Cells[3].Value, out 설정매도수량);
                        if (!success)
                        {
                            설정매도수량 = 0;
                        }
                    }
                }
            }
        }
    }
}

