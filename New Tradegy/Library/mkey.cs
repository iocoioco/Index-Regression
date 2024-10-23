using CPSYSDIBLib;
using New_Tradegy.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace New_Tradegy
{
    internal class mk
    {
        private static CPSYSDIBLib.MarketEye _marketeye;
        private static CPUTILLib.CpStockCode _cpstockcode;

        public static async Task task_marketeye()
        {
            while (true)
            {
                DateTime date = DateTime.Now;
                int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));
                int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                // Trigger at 10:00, 11:00, 12:00, 13:00, 14:00, or 15:21 only once per minute
                if ((HHmm == 1000 || HHmm == 1100 || HHmm == 1200 || HHmm == 1300 || HHmm == 1400 || HHmm == 1521) &&
                    g.minuteSaveAll != HHmm)
                {
                    if (wk.isWorkingHour())
                    {
                        // Save all stocks once at the mentioned times
                        await Task.Run(() => wr.SaveAllStocks());  // Use Task.Run for potentially long-running synchronous work
                        g.minuteSaveAll = HHmm;  // Mark this minute as saved
                    }
                }

                // Trigger the marketeye alarm task
                await Task.Run(() => ms.task_marketeye_alarm(HHmm));

                // Call marketeye logic
                await Task.Run(() => marketeye());

                // Wait 250 milliseconds (non-blocking)
                await Task.Delay(250);
            }
        }


        private static void marketeye()
        {
            /*
			0 code string
			1 시간ulong hhmm
			2.전일대비부호 char
			3 전일대비 long
			4 현재가 long
			5 시가 long
			6 전고 long
			7 전저long
			8 매도호가long
			9 매수호가long

			10 거래량ulong
			11 거래대금원 ulong
			12 장구분char '0' 장전 '1' 동시호가 '2' 장중
			13 총매도호가잔량ulong
			14 총매수호가잔량ulong
			15:최우선매도호가잔량(ulong)
			16:최우선매수호가잔량(ulong)
			22 전일거래량ulong
			23 전일종가long
			24 체결강도float

			28 예상체결가 long
			31 예상체결수량 ulong
			36 시간외단일대비부호char +, -
			37 시간외단일전일대비long, 36 필히 하여야 함
			38 시간외단일현재가long
			45 시간외단일거래대금ulonglong
			116 당일프로그램순매수량long
			118 당일외인순매수량long
			120 당일기관순매수량long
			121 전일외국인순매수long

			122 전일기관순매수long
			127 공매도수량ulong

            not used
            63:52주최고가(long or float)
            64:52주최저가(long or float)
			*/

            _cpstockcode = new CPUTILLib.CpStockCode();
            _marketeye = new CPSYSDIBLib.MarketEye();
            _marketeye.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_marketeye_received);
            object[] fields = new object[32] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15, 16, 22, 23, 24,
                28, 31, 36, 37, 38, 45,116,118,120,121,
                122,127};

            string[] codes = new string[g.stocks_per_marketeye];
            marketeye_종목(ref codes);

            _marketeye.SetInputValue(0, fields);
            _marketeye.SetInputValue(1, codes);

            int result = _marketeye.BlockRequest();
            if (result == 0)
            {
            }
        }


        public static void marketeye_종목(ref string[] codes)
        {
            int accumulated_count = 0;
            string code;
            List<string> 지수보유호가관심종목 = new List<string>();

        지수보유호가관심종목.Clear();

            지수보유호가관심종목.Add("KODEX 레버리지");
            지수보유호가관심종목.Add("KODEX 코스닥150레버리지");

            foreach (var stock in g.보유종목)
            {
                if (!지수보유호가관심종목.Contains(stock))
                {
                    지수보유호가관심종목.Add(stock);
                }
            }
            foreach (var stock in g.호가종목)
            {
                if (!지수보유호가관심종목.Contains(stock))
                {
                    지수보유호가관심종목.Add(stock);
                }
            }
            foreach (var stock in g.관심종목)
            {
                if (!지수보유호가관심종목.Contains(stock))
                {
                    지수보유호가관심종목.Add(stock);
                }
            }

            // KODEX 레버리지, 코스닥 레버리지 + 보유종목 + 호가관심종목 : 항상 포함
            foreach (var stock in 지수보유호가관심종목) // 지수보유호가관심종목 추가  
            {
                if (accumulated_count == g.stocks_per_marketeye)  // g.stocks_per_marketeye : max. number of stocks per one cycle
                    break;

                int index = g.ogl_data.FindIndex(x => x.stock == stock);
                if (index < 0)
                    continue;

                if (!codes.Contains(g.ogl_data[index].code) && accumulated_count < g.stocks_per_marketeye)
                {
                    codes[accumulated_count++] = g.ogl_data[index].code;
                }
            }

            //if (g.시초)
            //{
            //    for (int i = g.ogl_data_next; i < g.ogl_data.Count; i++)
            //    {
            //        if (accumulated_count == g.stocks_per_marketeye) //0504
            //            break;

            //        if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
            //        {
            //            if (codes.Contains(g.ogl_data[i].code))
            //                continue;

            //            codes[accumulated_count++] = g.ogl_data[i].code;
            //            g.ogl_data_next = i + 1;
            //            if (g.ogl_data_next == g.ogl_data.Count)
            //            {
            //                i = 0;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            // stock_eval에서 최우선 순위의 g.stocks_per_marketeye 개 선택
            // 다운 전 초기에는 시총이 큰 종목 우선 선택
            // 위의 지수보유호가종목을 넣고 g.stocks_per_marketeye 개까지 codes에 순차적으로 입력

            lock (g.lockObject) // g.sl  추가, 특정조건 하에서 선택된 활동성 높은 종목들
            {
                foreach (var stock in g.sl) //BLOCK g.sl may be changed 컬렉션이 수정되었습니다 ... 메세지
                {
                    if (accumulated_count >= g.stocks_per_marketeye / 2 ||
                        accumulated_count == g.stocks_per_marketeye)  // g.stocks_per_marketeye : max. number of stocks per one cycle
                        break;

                    code = _cpstockcode.NameToCode(stock); // 코스피혼합, 코스닥혼합 code.Length = 0 제외될 것임
                    if (codes.Contains(code))
                        continue;

                    if (code != null && code.Length == 7)
                    {
                        codes[accumulated_count++] = code;
                    }
                }
            }

            // ogl_data 추가
            for (int i = g.ogl_data_next; i < g.ogl_data.Count; i++)
            {
                if (accumulated_count == g.stocks_per_marketeye) //0504
                    break;

                if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
                {
                    if (codes.Contains(g.ogl_data[i].code))
                        continue;

                    codes[accumulated_count++] = g.ogl_data[i].code;
                    g.ogl_data_next = i + 1;
                    if (g.ogl_data_next == g.ogl_data.Count)
                    {
                        g.ogl_data_next = 0;
                    }
                }
            }

            // ogl_data 끝까지 추가하여도 g.stocks_per_marketeye 보다 작으면 ogl_data 처음부터 추가
            if (accumulated_count != g.stocks_per_marketeye)
            {
                for (int i = 0; i < g.ogl_data.Count; i++)
                {
                    if (accumulated_count == g.stocks_per_marketeye) //0504
                        break;

                    if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
                    {
                        if (codes.Contains(g.ogl_data[i].code))
                            continue;

                        codes[accumulated_count++] = g.ogl_data[i].code;
                        g.ogl_data_next = i + 1;
                        if (g.ogl_data_next == g.ogl_data.Count)
                        {
                            g.ogl_data_next = 0;
                        }
                    }
                }
            }
        }


        private static void _marketeye_received() // 100 MilliSecond for array_size 30 & for array_size 60
        {
            DateTime date = DateTime.Now;
            int HHmm = Convert.ToInt32(date.ToString("HHmm")); // run_task_read_eval_score()
            int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));

            if (HHmm < 900 || HHmm >= 1521) // 시작시간 9시인 경우
                return;

            //if (HHmm < 1000 || HHmm >= 1621) // 시작시간 10시인 경우
            //    return;

            int countfield = (int)_marketeye.GetHeaderValue(0); // number of data 
            dynamic fieldname = _marketeye.GetHeaderValue(1); // name of data ... 프로그램매수 등
            int count = (int)_marketeye.GetHeaderValue(2); // number of stock downloaded


            for (int k = 0; k < count; k++)
            {
                //int k = 10;
                string code = _marketeye.GetDataValue(0, k); // code is returned instead of stock name

                int indey = g.ogl_data.FindIndex(x => x.code == code);
                if (indey < 0)
                    continue;

                g.stock_data o = g.ogl_data[indey];

                //string stock = o.stock;

                //ulong 시간 = (char)_marketeye.GetDataValue(1, k); //1
                //char 전일대비부호 = (char)_marketeye.GetDataValue(2, k); //1 : 상한, 2 : 상승, 3 : 보합, 4 : 하한, 5 : 하락
                //long 전일대비 = _marketeye.GetDataValue(3, k); // 주의) 반드시 대비부호(2)와 같이 요청을 하여야 함


                o.매도1호가 = _marketeye.GetDataValue(8, k); ///
                o.매수1호가 = _marketeye.GetDataValue(9, k); ///

                o.현재가 = o.매수1호가; // _marketeye.GetDataValue(4, k); // changed on 20220925
                o.시초가 = _marketeye.GetDataValue(5, k); //5
                o.전고가 = _marketeye.GetDataValue(6, k); //6
                o.전저가 = _marketeye.GetDataValue(7, k); //

                o.거래량 = _marketeye.GetDataValue(10, k); //10
                // o.거래액_원 = _marketeye.GetDataValue(11, k); //11, 거래대금(원) ulong 

                o.총매도호가잔량 = _marketeye.GetDataValue(13, k); //6
                o.총매수호가잔량 = _marketeye.GetDataValue(14, k); //7
                o.최우선매도호가잔량 = (int)_marketeye.GetDataValue(15, k); //9
                o.최우선매수호가잔량 = (int)_marketeye.GetDataValue(16, k); //10

                o.전일종가 = _marketeye.GetDataValue(18, k); //8
                o.체강 = (double)_marketeye.GetDataValue(19, k); //9

                if (o.전일종가 > 100) // zero divider protection
                {
                    o.가격 = (int)((o.매수1호가 - (int)o.전일종가) * 10000.0 / o.전일종가);
                    double differ = (o.매도1호가 - o.매수1호가) * 10000.0 / o.전일종가;
                    double factor = 0.0;
                    if (o.최우선매도호가잔량 + o.최우선매수호가잔량 > 0)
                    {
                        factor = (double)o.최우선매도호가잔량 / (o.최우선매도호가잔량 + o.최우선매수호가잔량);
                    }
                    o.가격 += (int)(differ * factor); // the above two lines added for continuous price change
                    o.시초 = (int)((o.시초가 - (int)o.전일종가) * 10000.0 / o.전일종가);
                    o.전저 = (int)((o.전저가 - (int)o.전일종가) * 10000.0 / o.전일종가);
                    // o.전고.일 = (int)((o.전고가 - (int)o.전일종가) * 10000.0 / o.전일종가);
                }



                if (!g.KODEX4.Contains(o.stock)) // KODEX : 데이터 내용 상이로 다운로드하지 않음  unnecessary variables, remove later
                {
                    o.당일프로그램순매수량 = _marketeye.GetDataValue(26, k); //7
                    // o.당일외인순매수량 = _marketeye.GetDataValue(27, k); //7
                    o.당일기관순매수량 = _marketeye.GetDataValue(28, k); //8
                }

                o.공매도수량 = _marketeye.GetDataValue(31, k); //9

                double 누적거래액환산율 = wk.누적거래액환산율(HHmmss);

                if (o.일평균거래량 > 100)
                    o.수급 = (int)((double)o.거래량 / o.일평균거래량 * 100.0 * 누적거래액환산율);

                double 현누적매수체결거래량 = 0.0;
                double 현누적매도체결거래량 = 0.0;
                if (o.체강 > 0)
                {
                    현누적매수체결거래량 = o.거래량 * (o.체강 / (100.0 + o.체강)); // 체강 -> double
                    현누적매도체결거래량 = o.거래량 * 100.0 / (100.0 + o.체강);
                }

                if (o.nrow <= 0 || o.nrow >= g.MAX_ROW)
                    continue;

                int time_befr_4int = o.x[o.nrow - 1, 0] / 100; // change 6 digit time to 4 or 3 digit time
                bool append;


                // minute time check using 4 digit, decide append or not
                int comparison_row;
                if (HHmm != time_befr_4int || time_befr_4int == 859) // times differ or time is 859, append 
                {
                    comparison_row = o.nrow - 1;
                    append = true;
                    if (time_befr_4int == 859) // 시초가 -> 859에 저장
                        o.x[0, 1] = o.가격;
                }
                else
                {
                    comparison_row = o.nrow - 2; // times equal, replace
                    append = false;
                }


                int 전거래량 = Convert.ToInt32(o.x[comparison_row, 7]);
                double 전체강 = Convert.ToInt32(o.x[comparison_row, 3]) / g.HUNDRED; // 전체강 -> double
                double 전누적매수체결거래량 = 0.0;
                double 전누적매도체결거래량 = 0.0;
                if (전체강 > 0)
                {
                    전누적매수체결거래량 = 전거래량 * (전체강 / (100.0 + 전체강));
                    전누적매도체결거래량 = 전거래량 * 100.0 / (100.0 + 전체강);
                }

                // BLOCKED
                int before_time_6int = o.x[comparison_row, 0];
                double totalSeconds = ms.total_Seconds(before_time_6int, HHmmss);
                double multiple_factor;
                if (totalSeconds > g.EPS && o.일평균거래량 > 100)
                {
                    multiple_factor = 60.0 / totalSeconds * 380.0 / o.일평균거래량 * 10.0;

                    o.매수배 = (int)((현누적매수체결거래량 - 전누적매수체결거래량) * multiple_factor);
                    o.매도배 = (int)((현누적매도체결거래량 - 전누적매도체결거래량) * multiple_factor);
                }

                int[] t = new int[12];
                // 0, 시간, 1, 가격, 2 수급, 3 체강 * 100, 4 프로그램 매수액(억), 5 외인 매수액(억), 6 기관 매수액(억)
                // 7 거래량, 8 매수배, 9 매도배, 10 수급연속횟수, 11 체강연속횟수
                t[0] = HHmmss;
                t[1] = o.가격; // 매수1호가기준              
                t[2] = o.수급;
                t[3] = (int)(o.체강 * g.HUNDRED);
                t[7] = (int)o.거래량;
                t[8] = o.매수배;
                t[9] = o.매도배;


                // 0, 시간, 1, 가격, 2 수급, 3 체강 * 100, 4 기관순매수액(억), 5 프고르램순매수액(억), 6 개인순매수액(억)
                // 7 거래량, 8 매수배, 9 매도배, 10 Nasdaq_지수, 11 연기금순매수액(억)
                if (o.stock.Contains("KODEX 레버리지") || o.stock.Contains("KODEX 200선물인버스2X")) // full
                {
                    t[3] = (int)g.코스피프외순매수; //지수 종목 프로 + 외인 매수합, post_지수_프외()에서 시간대별 계산
                    t[4] = (int)g.코스피기관순매수;
                    t[5] = (int)g.코스피외인순매수;
                    t[6] = (int)g.코스피개인순매수;
                    t[10] = (int)(g.Nasdaq_지수 * g.THOUSAND);
                    t[11] = (int)g.코스피연기순매수;
                }
                else if (o.stock.Contains("KODEX 코스닥150레버리지") || o.stock.Contains("KODEX 코스닥150선물인버스")) // full
                {
                    t[3] = (int)g.코스닥프외순매수;// t[3] : 지수 종목 프로 + 외인 매수합, post_지수_프외()에서 시간대별 계산
                    t[4] = (int)g.코스닥기관순매수;
                    t[5] = (int)g.코스닥외인순매수;
                    t[6] = (int)g.코스닥개인순매수;
                    t[10] = (int)(g.Nasdaq_지수 * g.THOUSAND);  // was g.코스닥금투순매수;
                    t[11] = (int)g.코스닥연기순매수;
                }
                else // 일반, 10 수급연속, 11 체강연속 below
                {
                    t[4] = (int)o.당일프로그램순매수량; // from marketeye
                    t[5] = (int)o.당일외인순매수량; // from _cpsvr8091s
                    t[6] = (int)o.당일기관순매수량; // from marketeye
                }

                int append_or_replace_row;
                if (append)
                    append_or_replace_row = o.nrow;
                else
                    append_or_replace_row = o.nrow - 1;

                if (append_or_replace_row >= g.MAX_ROW)
                    return;

                for (int i = 0; i < 12; i++)
                {
                    o.x[append_or_replace_row, i] = t[i];
                }

                o.nrow = append_or_replace_row + 1; // total number of row after opration either append or replace

                // Continuity : Genral Only, KODEX & Mixed excluded
                if ((!(g.KODEX4.Contains(o.stock) || o.stock.Contains("혼합"))) && o.nrow >= 2)
                {
                    // Continuity of amount ratio
                    if (o.x[o.nrow - 1, 7] == o.x[o.nrow - 2, 7]) // no trade or VI
                        o.x[o.nrow - 1, 10] = o.x[o.nrow - 2, 10];

                    else if (o.x[o.nrow - 1, 2] > o.x[o.nrow - 2, 2]) // increase of amount ratio
                        o.x[o.nrow - 1, 10] = o.x[o.nrow - 2, 10] + 1;

                    else // decrease of amount ratio
                        o.x[o.nrow - 1, 10] = 0;

                    // Continuity of intensity : the intensity was multiplied by g.HUNDRED -> too many cyan ? -> divided by g.HUNDRED again, let's see
                    if (o.x[o.nrow - 1, 7] == o.x[o.nrow - 2, 7]) // no trade or VI
                        o.x[o.nrow - 1, 11] = o.x[o.nrow - 2, 11];

                    else if (o.x[o.nrow - 1, 3] / g.HUNDRED > o.x[o.nrow - 2, 3] / g.HUNDRED) // increase of intensity ratio
                        o.x[o.nrow - 1, 11] = o.x[o.nrow - 2, 11] + 1;

                    else // decrease of intensity ratio
                        o.x[o.nrow - 1, 11] = 0;
                }






                // MDF 20230302
                // tick data setting
                // BLOCKED
                totalSeconds = ms.total_Seconds(o.틱의시간[0], HHmmss); // second can be zero, oops
                double 틱매수체결배수 = 0.0;
                double 틱매도체결배수 = 0.0;
                if (totalSeconds > g.EPS)
                {
                    multiple_factor = 0.0;
                    if (o.일평균거래량 > 100)
                        multiple_factor = 60.0 / totalSeconds * 380.0 / o.일평균거래량 * 10.0; // MDF on 20230301
                    틱매수체결배수 = (현누적매수체결거래량 - o.틱매수량[0]) * multiple_factor; // o.틱매수량[0] : not current, but previous i.e. not updated
                    틱매도체결배수 = (현누적매도체결거래량 - o.틱매도량[0]) * multiple_factor; // o.틱매도량[0] : not current, but previous i.e. not updated
                }







                double money_factor = o.전일종가 / g.천만원;
                // shift tick data[i]
                for (int i = g.틱_array_size - 1; i >= 1; i--) // marketeye_receive
                {
                    o.틱의시간[i] = o.틱의시간[i - 1];
                    o.틱의가격[i] = o.틱의가격[i - 1];
                    o.틱의수급[i] = o.틱의수급[i - 1];
                    o.틱의체강[i] = o.틱의체강[i - 1];
                    o.틱매수량[i] = o.틱매수량[i - 1];
                    o.틱매도량[i] = o.틱매도량[i - 1];
                    o.틱매수배[i] = o.틱매수배[i - 1];
                    o.틱매도배[i] = o.틱매도배[i - 1];

                    // MDF 20230302
                    // BLOCKED
                    o.틱배수차[i] = o.틱배수차[i - 1];
                    o.틱배수합[i] = o.틱배수합[i - 1];

                    o.틱프로량[i] = o.틱프로량[i - 1];
                    o.틱프로천[i] = o.틱프로천[i - 1];
                    o.틱외인량[i] = o.틱외인량[i - 1]; // 20220720
                    o.틱외인천[i] = o.틱외인천[i - 1]; // 20220720
                    o.틱거래천[i] = o.틱거래천[i - 1];
                    o.틱프외퍼[i] = o.틱프외퍼[i - 1];

                    o.틱매도잔[i] = o.틱매도잔[i - 1];
                    o.틱매수잔[i] = o.틱매수잔[i - 1];
                }

                // replace tick data[0]
                o.틱의시간[0] = HHmmss;
                o.틱의가격[0] = o.가격;
                o.틱의수급[0] = o.수급;
                o.틱의체강[0] = (int)(o.체강 * g.HUNDRED);
                o.틱매수량[0] = (int)현누적매수체결거래량;
                o.틱매도량[0] = (int)현누적매도체결거래량;


                // MDF on 20230302
                // BLOCKED 
                o.틱매수배[0] = (int)(틱매수체결배수);
                o.틱매도배[0] = (int)(틱매도체결배수);
                o.틱배수차[0] = o.틱매수배[0] - o.틱매도배[1];
                o.틱배수합[0] = o.틱매수배[0] + o.틱매도배[1];


                o.틱프로량[0] = t[4];
                o.틱프로천[0] = (int)((o.틱프로량[0] - o.틱프로량[1]) * money_factor);
                o.틱외인량[0] = t[5];
                o.틱외인천[0] = (int)((o.틱외인량[0] - o.틱외인량[1]) * money_factor);
                o.틱거래천[0] = (int)((o.틱매수량[0] - o.틱매수량[1] + o.틱매도량[0] - o.틱매도량[1]) * money_factor);

                if (o.틱거래천[0] > 0)
                    o.틱프외퍼[0] = (int)((o.틱프로천[0] + o.틱외인천[0]) / o.틱거래천[0] * 100);

                o.틱매도잔[0] = o.최우선매도호가잔량;
                o.틱매수잔[0] = o.최우선매수호가잔량;

                // replace minute data[i]
                // minute data[0] calculated in post_minute for both testing and real
                if (append)
                {
                    for (int i = g.분_array_size - 1; i >= 2; i--) // marketeye_receive
                    {
                        o.분프로천[i] = o.분프로천[i - 1];
                        o.분외인천[i] = o.분외인천[i - 1];
                        o.분거래천[i] = o.분거래천[i - 1];

                        o.분매수배[i] = o.분매수배[i - 1];
                        o.분매도배[i] = o.분매도배[i - 1];

                        o.분배수차[i] = o.분배수차[i - 1];
                        o.분배수합[i] = o.분배수합[i - 1];
                    }

                    if (o.nrow - 3 >= 0)
                    {
                        o.분프로천[1] = (int)((o.x[o.nrow - 2, 4] - o.x[o.nrow - 3, 4]) * money_factor);
                        o.분외인천[1] = (int)((o.x[o.nrow - 2, 5] - o.x[o.nrow - 3, 5]) * money_factor);
                        o.분거래천[1] = (int)((o.x[o.nrow - 2, 7] - o.x[o.nrow - 3, 7]) * money_factor);
                    }
                    if (o.nrow - 2 >= 0)
                    {
                        o.분매수배[1] = o.x[o.nrow - 2, 8];
                        o.분매도배[1] = o.x[o.nrow - 2, 9];
                    }
                    o.분배수차[1] = o.분매수배[1] - o.분매도배[1];
                    o.분배수합[1] = o.분매수배[1] + o.분매도배[1];
                }

                ps.post(o); // marketeye_received()
                ps.PostPassing(o, o.nrow - 1, true); // marketeye_received() real

                if (g.보유종목.Contains(o.stock)) // 보유종목 중 Form_호가 사용하지 않고 있는 경우 대비
                {
                    cn.dgv2_update(); // marketeye_received()
                    marketeye_received_보유종목_푀분의매수매도_소리내기(o); // this is from mip
                }

                // ps.marketeye_received_틱프로천_ebb_tide(o); // not defined clearly, so return doing nothing
                // do not know what is going on in the routine,
                // temporarily blocked on 20240605
                // ms.marketeye_record_변곡(o);

            }

            int index = wk.return_index_of_ogldata("KODEX 레버리지");
            g.stock_data q = g.ogl_data[index];
            g.코스피지수 = q.x[q.nrow - 1, 1];
           
            index = wk.return_index_of_ogldata("KODEX 코스닥150레버리지");
            q = g.ogl_data[index];
            g.코스닥지수 = q.x[q.nrow - 1, 1];
            
            ps.post_지수_프외_배차_합산();
            
            g.marketeye_count++;



            // marketeye_received 다운로드 200 종목, 코스피혼합의 10개 종목, 코스닥혼합의 10개 종목은
            // 항상 200개 종목 중에 포함되고 매 5초당 다운로드한다는 과정에서 코스피혼합과 코스닥혼합 계산
            //marketeye_received_혼합종목("코스피혼합", g.코스피합성);
            //marketeye_received_혼합종목("코스닥혼합", g.코스닥합성);



            // real의 경우 g.지수종목 및 .g.KODEX4는 매 marketeye()에 포함되므로 모든
            // 종목의 라인 수는 동일하므로 특정 라인 번호를 보내어 마직막 라인에서 
            // 프외 돈의 크기를 계산한다. 


            //int index = wk.return_index_of_ogldata(g.KODEX4[0]); // 코스피 레버리지를 기준 종목으로 
            //if (index >= 0)
            //{
            //    g.stock_data o = g.ogl_data[index];
      
            //}

            //wr.wt(" ");


            // Thread.Sleep : array_size 30 & 60 not much different in duration  
            //                                                                                      o.틱의시간[0] - o.틱의시간[29];     o.틱의시간[0] - o.틱의시간[59]; 
            // 1000 : interval marketeye_received 1100 ~ 1700 MilliSeconds      삼성전자 37 secs, 희림 6:30 secs
            //  500 : interval marketeye_received  1000 MilliSeconds                삼성전자 28 secs, 희림 4:30 secs
            //  100 : interval marketeye_received  500 MilliSeconds                 삼성전자 14 secs, 희림 2:26 secs
            //     0 : interval marketeye_received  300 MilliSeconds                 삼성전자 10 secs, 희림 2:00 secs 삼성전자 19 secs, 희림 3:12 secs
        }

        // no reference
        public static void marketeye_received_혼합종목(string mixed_stock, List<string> list)
        {
            // 분
            double[] t = new double[12];
            double tick_매수배 = 0.0;
            double tick_매도배 = 0.0;

            foreach (string line in list)
            {
                string[] items = line.Split('\t');

                // index 가 탐색되지 않으면 -1 리턴
                int index = wk.return_index_of_ogldata(items[0]);
                if (index < 0)
                { continue; }
                g.stock_data o = g.ogl_data[index];

                double weight = Convert.ToDouble(items[1]);

                t[1] += o.가격 * weight;
                t[2] += o.수급 * weight;
                t[3] += o.체강 * g.HUNDRED * weight;

                t[7] += o.거래량 * weight; // no meaning

                t[8] += o.매수배 * weight;
                t[9] += o.매도배 * weight;

                tick_매수배 += o.틱매수배[0] * weight;
                tick_매도배 += o.틱매도배[0] * weight;
            }
            // 0, 1, 2, 3, 7, 8, 9 used upside
            // 4, 5, 6, 10, 11 used down below
            t[4] = g.상해종합지수 * g.HUNDRED;
            t[5] = g.항생지수 * g.HUNDRED;
            t[6] = g.니케이지수 * g.HUNDRED;
            //t[8] = 매수배; // no need to multiply g.TEN, each item already multiplied
            //t[9] = 매도배; // no need to multiply g.TEN, each item already multiplied
            t[10] = (int)(g.SP_지수 * g.HUNDRED);
            t[11] = (int)(g.Nasdaq_지수 * g.HUNDRED);

            int mixed_index = wk.return_index_of_ogldata(mixed_stock);
            if (mixed_index < 0)
            {
                return;
            }
            g.stock_data v = g.ogl_data[mixed_index];

            int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            int time_bef_4int = v.x[v.nrow - 1, 0] / 100;

            t[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
            if (HHmm == time_bef_4int)
            {
                for (int j = 0; j < 12; j++) // replace
                {
                    v.x[v.nrow - 1, j] = (int)t[j];
                }
            }
            else // append as a new line
            {
                if (v.nrow < g.MAX_ROW)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        v.x[v.nrow, j] = (int)t[j];
                    }
                    v.nrow++;
                }
            }

            // 틱  배수만 우선 정리
            for (int i = g.틱_array_size - 1; i >= 1; i--) // marketeye_receive
            {
                if (mixed_stock.Contains("코스피혼합"))
                {
                    g.kospi_틱매수배[i] = g.kospi_틱매수배[i - 1];
                    g.kospi_틱매도배[i] = g.kospi_틱매도배[i - 1];
                }
                if (mixed_stock.Contains("코스닥혼합"))
                {
                    g.kosdaq_틱매수배[i] = g.kosdaq_틱매수배[i - 1];
                    g.kosdaq_틱매도배[i] = g.kosdaq_틱매도배[i - 1];
                }
            }

            // 틱의 값이 매번 바뀌므로 분의 값과 달리 오른쪽 변수는 따로 저장 한 후 
            // 현재의 틱 값에 다시 저장하여야 하므로 주의
            if (mixed_stock.Contains("코스피혼합"))
            {
                g.kospi_틱매수배[0] = tick_매수배;
                g.kospi_틱매도배[0] = tick_매도배;
            }
            if (mixed_stock.Contains("코스닥혼합"))
            {
                g.kosdaq_틱매수배[0] = tick_매수배;
                g.kosdaq_틱매도배[0] = tick_매도배;
            }
        }

        public static void marketeye_received_보유종목_푀분의매수매도_소리내기(g.stock_data o)
        {
            if (o.보유량 * o.전일종가 < 200000) // 보유액 20,000 이하이면 무시하고 return
                return;

            double sound_indicater = 0;

            // 소리
            #region
            if (Math.Abs(o.틱프로천[0]) > 0.1 || Math.Abs(o.틱외인천[0]) > 0.1)
            {
                string sound = "";
                int 보유종목_순서번호 = -1;
                for (int i = 0; i < 3; i++)
                {
                    if (g.보유종목[i] == o.stock)
                    {
                        보유종목_순서번호 = i;
                        if (i == 0)
                            sound = "one ";
                        else if (i == 1)
                            sound = "two ";
                        else
                            sound = "three ";
                        break;
                    }
                }
                if (보유종목_순서번호 < 0)
                    return;

                if (o.통계.프분_dev > 0)
                {
                    sound_indicater = (o.틱프로천[0] + o.틱외인천[0]) / o.통계.프분_dev;
                    if (sound_indicater > 2) sound += "buyest";
                    else if (sound_indicater > 1) sound += "buyer";
                    else if (sound_indicater > 0) sound += "buy";
                    else if (sound_indicater > -1) sound += "sell";
                    else if (sound_indicater > -2) sound += "seller";
                    else sound += "sellest";

                }
                ms.Sound("가", sound);
            }
            #endregion

            // 비상매도
            // 푀틱 매도 푀분 매도 가 하락
            // 푀틱 매수 푀분 매수 배차 음, 가 하락
            // 배차 음 가 하락 (누군가 던지고 있다)
            // 피로도
        }

    }
}
