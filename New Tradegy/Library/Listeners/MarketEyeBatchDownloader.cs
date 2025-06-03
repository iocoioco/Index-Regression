
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.IO;
using New_Tradegy.Library.PostProcessing;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace New_Tradegy.Library.Listeners
{
    internal class MarketEyeBatchDownloader
    {
        private static CPSYSDIBLib.MarketEye _marketeye;
        private static CPUTILLib.CpStockCode _cpstockcode;

        private static IndexRangeTracker indexRangeTracker = new IndexRangeTracker();

        public static async Task StartDownloaderAsync()
        {
            while (true)
            {
                DateTime date = DateTime.Now;
                int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));
                int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                // 시작시간 09:00
                // Trigger at 10:00, 11:00, 12:00, 13:00, 14:00, or 15:21 only once per minute
                if ((HHmm == 1000 || HHmm == 1100 || HHmm == 1200 || HHmm == 1300
                    || HHmm == 1400 || HHmm == 1521) &&
                g.minuteSaveAll != HHmm)
                {
                    if (wk.isWorkingHour())
                    {
                        // Save all stocks once at the mentioned times
                        await FileOut.SaveAllStocks();  // Use Task.Run for potentially long-running synchronous work
                        g.minuteSaveAll = HHmm;  // Mark this minute as saved
                    }
                }


                if (wk.isWorkingHour())
                {
                    // Trigger the marketeye alarm task
                    await SoundUtils.MarketTimeAlarmsAsync(HHmm);

                    // Call marketeye logic
                    await DownloadBatchAsync();
                }

                // Wait 250 milliseconds (non-blocking) Block Request 60times/ 15 Secs
                await Task.Delay(300);
            }
        }

        private static async Task DownloadBatchAsync()
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


            if (_marketeye == null)
            {
                _cpstockcode = new CPUTILLib.CpStockCode();
                _marketeye = new CPSYSDIBLib.MarketEye();
                _marketeye.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(HandleBatchDataAsync);
            }

            object[] fields = new object[]
            {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        10, 11, 12, 13, 14, 15, 16, 22, 23, 24,
        28, 31, 36, 37, 38, 45, 116, 118, 120, 121,
        122, 127
            };

            string[] codes = new string[g.stocks_per_marketeye];

            // ✅ Fill the codes using your BatchSelector
            var selected = MarketEyeBatchSelector.Select200Batch(
                leverage: g.StockManager.LeverageList,
                holding: g.StockManager.HoldingList,
                interestedWithBid: g.StockManager.InterestedWithBidList,
                interestedOnly: g.StockManager.InterestedOnlyList
            );

            for (int i = 0; i < codes.Length && i < selected.Count; i++)
            {
                var stock = g.StockManager.Repository.TryGetStockOrNull(selected[i]);
                if (stock == null) continue;
                codes[i] = stock.Code;
            }
                

            _marketeye.SetInputValue(0, fields);
            _marketeye.SetInputValue(1, codes);

            int result = _marketeye.BlockRequest();
            if (result != 0)
            {
                
            }

            await Task.CompletedTask; // for consistency with async signature
        }

        // CONTINUED REFACTORED _marketeye_received()
        private static async void HandleBatchDataAsync()
        {
            DateTime date = DateTime.Now;
            int HHmm = Convert.ToInt32(date.ToString("HHmm"));
            int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));

            if (HHmm < 800 || HHmm >= 1521) return;

            //if (HHmm < 1000 || HHmm >= 1621) // 시작시간 10시인 경우
            //    return;

            int count = (int)_marketeye.GetHeaderValue(2); // number of stocks downloaded
            var downloadList = new List<StockData>();

            for (int k = 0; k < count; k++)
            {
                string stock = _marketeye.GetDataValue(0, k);
                if (!g.StockRepository.Contains(stock)) 
                    continue;

                var data = g.StockRepository.TryGetStockOrNull(stock);
                if(data == null) continue;
                var api = data.Api;
                downloadList.Add(data);


                api.매도1호가 = _marketeye.GetDataValue(8, k);
                api.매수1호가 = _marketeye.GetDataValue(9, k);

                api.현재가 = api.매수1호가;
                api.시초가 = _marketeye.GetDataValue(5, k);
                api.전고가 = _marketeye.GetDataValue(6, k);
                api.전저가 = _marketeye.GetDataValue(7, k);

                api.거래량 = _marketeye.GetDataValue(10, k);
                api.총매도호가잔량 = _marketeye.GetDataValue(13, k);
                api.총매수호가잔량 = _marketeye.GetDataValue(14, k);
                api.최우선매도호가잔량 = (int)_marketeye.GetDataValue(15, k);
                api.최우선매수호가잔량 = (int)_marketeye.GetDataValue(16, k);
                api.전일종가 = _marketeye.GetDataValue(18, k);
                api.체강 = (double)_marketeye.GetDataValue(19, k);

                if (api.전일종가 > 100)
                {
                    api.가격 = (int)((api.매수1호가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                    double differ = (api.매도1호가 - api.매수1호가) * 10000.0 / api.전일종가;
                    double factor = 0.0;
                    if (api.최우선매도호가잔량 + api.최우선매수호가잔량 > 0)
                    {
                        factor = (double)api.최우선매도호가잔량 / (api.최우선매도호가잔량 + api.최우선매수호가잔량);
                    }
                    api.가격 += (int)(differ * factor);
                    api.시초 = (int)((api.시초가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                    api.전저 = (int)((api.전저가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                }

                if (!g.StockManager.IndexList.Contains(data.Stock))
                {
                    api.당일프로그램순매수량 = _marketeye.GetDataValue(26, k);
                    api.당일기관순매수량 = _marketeye.GetDataValue(28, k);
                }

                api.공매도수량 = _marketeye.GetDataValue(31, k);

                double 누적거래액환산율 = wk.누적거래액환산율(HHmmss);

                if (data.Statistics.일평균거래량 > 100)
                    api.수급 = (int)((double)api.거래량 / data.Statistics.일평균거래량 * 100.0 * 누적거래액환산율);

                double 현누적매수체결거래량 = 0.0;
                double 현누적매도체결거래량 = 0.0;
                if (api.체강 > 0)
                {
                    현누적매수체결거래량 = api.거래량 * (api.체강 / (100.0 + api.체강));
                    현누적매도체결거래량 = api.거래량 * 100.0 / (100.0 + api.체강);
                }

                if (api.nrow <= 0 || api.nrow >= g.MAX_ROW)
                    continue;

                int time_befr_4int = api.x[api.nrow - 1, 0] / 100;
                bool append = HHmm != time_befr_4int || time_befr_4int == 859;
                int comparison_row = append ? api.nrow - 1 : api.nrow - 2;

                if (time_befr_4int == 859)
                    api.x[0, 1] = api.가격;

                int 전거래량 = Convert.ToInt32(api.x[comparison_row, 7]);
                double 전체강 = Convert.ToInt32(api.x[comparison_row, 3]) / g.HUNDRED;
                double 전누적매수체결거래량 = 0.0;
                double 전누적매도체결거래량 = 0.0;
                if (전체강 > 0)
                {
                    전누적매수체결거래량 = 전거래량 * (전체강 / (100.0 + 전체강));
                    전누적매도체결거래량 = 전거래량 * 100.0 / (100.0 + 전체강);
                }

                double totalSeconds = Utils.TimeUtils.total_Seconds(api.x[comparison_row, 0], HHmmss);
                double multiple_factor = 0.0;
                if (MathUtils.IsSafeToDivide(totalSeconds) && data.Statistics.일평균거래량 > 100)
                {
                    multiple_factor = 60.0 / totalSeconds * 380.0 / data.Statistics.일평균거래량 * 10.0;
                    api.매수배 = (int)((현누적매수체결거래량 - 전누적매수체결거래량) * multiple_factor);
                    api.매도배 = (int)((현누적매도체결거래량 - 전누적매도체결거래량) * multiple_factor);
                }

                // ⏩ Next: build "t" array, append/replace api.x[nrow], shift tick arrays, update continuity



                int[] t = new int[12];
                t[0] = HHmmss;
                t[1] = api.가격;
                t[2] = api.수급;
                t[3] = (int)(api.체강 * g.HUNDRED);
                t[7] = (int)api.거래량;
                t[8] = api.매수배;
                t[9] = api.매도배;

                if (data.Stock.Contains("KODEX 레버리지") || data.Stock.Contains("KODEX 200선물인버스2X"))
                {
                    t[3] = (int)(MajorIndex.Instance.KospiProgramNetBuy + MajorIndex.Instance.KospiForeignNetBuy);
                    t[4] = (int)MajorIndex.Instance.KospiInstitutionNetBuy;
                    t[5] = (int)MajorIndex.Instance.KospiForeignNetBuy;
                    t[6] = (int)MajorIndex.Instance.KospiRetailNetBuy;
                    t[10] = (int)(MajorIndex.Instance.NasdaqIndex * g.THOUSAND);
                    t[11] = (int)MajorIndex.Instance.KospiPensionNetBuy;
                }
                else if (data.Stock.Contains("KODEX 코스닥150레버리지") || data.Stock.Contains("KODEX 코스닥150선물인버스"))
                {
                    t[3] = (int)(MajorIndex.Instance.KosdaqProgramNetBuy + MajorIndex.Instance.KosdaqForeignNetBuy);
                    t[4] = (int)MajorIndex.Instance.KosdaqInstitutionNetBuy;
                    t[5] = (int)MajorIndex.Instance.KosdaqForeignNetBuy;
                    t[6] = (int)MajorIndex.Instance.KosdaqRetailNetBuy;
                    t[10] = (int)(MajorIndex.Instance.NasdaqIndex * g.THOUSAND);
                    t[11] = (int)MajorIndex.Instance.KosdaqPensionNetBuy;
                }
                else
                {
                    t[4] = (int)api.당일프로그램순매수량;
                    t[5] = (int)api.당일외인순매수량;
                    t[6] = (int)api.당일기관순매수량;
                }

                int append_or_replace_row = append ? api.nrow : api.nrow - 1;
                if (append_or_replace_row >= g.MAX_ROW) return;

                for (int i = 0; i < 12; i++)
                {
                    api.x[append_or_replace_row, i] = t[i];
                }
                api.nrow = append_or_replace_row + 1;

                if (!(g.StockManager.IndexList.Contains(data.Stock) && api.nrow >= 2))
                {
                    if (api.x[api.nrow - 1, 7] == api.x[api.nrow - 2, 7])
                        api.x[api.nrow - 1, 10] = api.x[api.nrow - 2, 10];
                    else if (api.x[api.nrow - 1, 2] > api.x[api.nrow - 2, 2])
                        api.x[api.nrow - 1, 10] = api.x[api.nrow - 2, 10] + 1;
                    else
                        api.x[api.nrow - 1, 10] = 0;

                    if (api.x[api.nrow - 1, 7] == api.x[api.nrow - 2, 7])
                        api.x[api.nrow - 1, 11] = api.x[api.nrow - 2, 11];
                    else if (api.x[api.nrow - 1, 3] / g.HUNDRED > api.x[api.nrow - 2, 3] / g.HUNDRED)
                        api.x[api.nrow - 1, 11] = api.x[api.nrow - 2, 11] + 1;
                    else
                        api.x[api.nrow - 1, 11] = 0;
                }

                // ⏩ Optional next: tick + minute shifting + post_real + index sync
                api.AppendTick(
                    t,
                    HHmmss,
                    현누적매수체결거래량,
                    현누적매도체결거래량,
                    api.틱매수량[0],
                    api.틱매도량[0],
                    multiple_factor,
                    api.전일종가 / g.천만원
                );
                api.AppendMinuteIfNeeded(append);
            }

  
            PostProcessor.post_real(downloadList);

            if (g.StockRepository.Contains("KODEX 레버리지"))
            {
                var kospi = g.StockRepository.TryGetStockOrNull("KODEX 레버리지");
                int kospiIndex = kospi.Api.x[kospi.Api.nrow - 1, 1];
                MajorIndex.Instance.KospiIndex = kospiIndex;
                indexRangeTracker.CheckIndexAndSound(kospiIndex, "Kospi");
            }

            if (g.StockRepository.Contains("KODEX 코스닥150레버리지"))
            {
                var kosdaq = g.StockRepository.TryGetStockOrNull("KODEX 코스닥150레버리지");
                int kosdaqIndex = kosdaq.Api.x[kosdaq.Api.nrow - 1, 1];
                MajorIndex.Instance.KosdaqIndex = kosdaqIndex;
                indexRangeTracker.CheckIndexAndSound(kosdaqIndex, "Kosdaq");
            }

            PostProcessor.post_코스닥_코스피_프외_순매수_배차_합산();
            g.MarketeyeCount++;
        }


        // no reference
        public static void marketeye_received_혼합종목(string mixed_stock, List<string> list)
        {
            double[] t = new double[12];
            double tick_매수배 = 0.0;
            double tick_매도배 = 0.0;

            foreach (string line in list)
            {
                string[] items = line.Split('\t');

                var data = g.StockRepository.TryGetStockOrNull(items[0]).Api;

                double weight = Convert.ToDouble(items[1]);

                t[1] += data.가격 * weight;
                t[2] += data.수급 * weight;
                t[3] += data.체강 * g.HUNDRED * weight;
                t[7] += data.거래량 * weight;
                t[8] += data.매수배 * weight;
                t[9] += data.매도배 * weight;

                tick_매수배 += data.틱매수배[0] * weight;
                tick_매도배 += data.틱매도배[0] * weight;
            }

            t[4] = MajorIndex.Instance.ShanghaiIndex * g.HUNDRED;
            t[5] = MajorIndex.Instance.HangSengIndex * g.HUNDRED;
            t[6] = MajorIndex.Instance.NikkeiIndex * g.HUNDRED;
            t[10] = (int)(MajorIndex.Instance.Snp500Index * g.HUNDRED);
            t[11] = (int)(MajorIndex.Instance.NasdaqIndex * g.HUNDRED);

            if (!!g.StockRepository.Contains(mixed_stock))
                return;

            var v = g.StockRepository.TryGetStockOrNull(mixed_stock);

            int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            int time_bef_4int = v.Api.x[v.Api.nrow - 1, 0] / 100;
            t[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            if (HHmm == time_bef_4int)
            {
                for (int j = 0; j < 12; j++)
                {
                    v.Api.x[v.Api.nrow - 1, j] = (int)t[j];
                }
            }
            else
            {
                if (v.Api.nrow < g.MAX_ROW)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        v.Api.x[v.Api.nrow, j] = (int)t[j];
                    }
                    v.Api.nrow++;
                }
            }

            for (int i = MajorIndex.TickArraySize - 1; i >= 1; i--)
            {
                if (mixed_stock.Contains("코스피혼합"))
                {
                    MajorIndex.Instance.KospiTickBuyPower[i] = MajorIndex.Instance.KospiTickBuyPower[i - 1];
                    MajorIndex.Instance.KospiTickSellPower[i] = MajorIndex.Instance.KospiTickSellPower[i - 1];
                }
                if (mixed_stock.Contains("코스닥혼합"))
                {
                    MajorIndex.Instance.KosdaqTickBuyPower[i] = MajorIndex.Instance.KosdaqTickBuyPower[i - 1];
                    MajorIndex.Instance.KosdaqTickSellPower[i] = MajorIndex.Instance.KosdaqTickSellPower[i - 1];
                }
            }

            if (mixed_stock.Contains("코스피혼합"))
            {
                MajorIndex.Instance.KospiTickBuyPower[0] = tick_매수배;
                MajorIndex.Instance.KospiTickSellPower[0] = tick_매도배;
            }
            if (mixed_stock.Contains("코스닥혼합"))
            {
                MajorIndex.Instance.KosdaqTickBuyPower[0] = tick_매수배;
                MajorIndex.Instance.KosdaqTickSellPower[0] = tick_매도배;
            }
        }
    }
}
