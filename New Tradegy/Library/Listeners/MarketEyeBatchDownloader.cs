
using New_Tradegy.Library.Core;
using New_Tradegy.Library.IO;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Listeners
{
    internal class MarketEyeBatchDownloader
    {
        private static CPSYSDIBLib.MarketEye _marketeye;
        //private static CPUTILLib.CpStockCode _cpstockcode;
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();

        private static IndexRangeTracker indexRangeTracker = new IndexRangeTracker();

        private static Dictionary<string, int> lastPrices = new Dictionary<string, int>();


        // 1 cycle of download takes 200ms 
        public static void RunDownloaderLoop()
        {
            if (!g.connected)
                return;

            int minuteSaveAll = -1;

            while (true)
            {

                

                DateTime date = DateTime.Now;
                int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));
                int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                // 시작시간 09:00
                // Trigger at 10:00, 11:00, 12:00, 13:00, 14:00, or 15:21 only once per minute
                if ((HHmm == 1000 || HHmm == 1100 || HHmm == 1200 || HHmm == 1300
                    || HHmm == 1400 || HHmm == 1520) && minuteSaveAll != HHmm)

                //시작시간 10:00
                //    if ((HHmm == 1100 || HHmm == 1200 || HHmm == 1300 ||
                // HHmm == 1400 || HHmm == 1500 || HHmm == 1620) &&
                // minuteSaveAll != HHmm)
                {
                    if (wk.isWorkingHour())
                    {
                        FileOut.SaveAllStocks();  // Use Task.Run for potentially long-running synchronous work
                        minuteSaveAll = HHmm;  // Mark this minute as saved
                    }
                }

                if (wk.isWorkingHour())
                {
                    SoundUtils.MarketTimeAlarmsAsync(HHmm);
                    try
                    {
                        DownloadBatch();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❗DownloadBatch failed: {ex.Message}");
                    }
                }
                
                // Wait 300 milliseconds (non-blocking) Block Request 60times/ 15 Secs
                Thread.Sleep(500);

                

            }
        }

        private static void DownloadBatch()
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
                _marketeye = new CPSYSDIBLib.MarketEye();
                _marketeye.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(HandleBatchData);
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
                indexList: g.StockManager.IndexList,
                holding: g.StockManager.HoldingList,
                interestedWithBid: g.StockManager.InterestedWithBidList,
                interestedOnly: g.StockManager.InterestedOnlyList,
                rankedStockList: g.StockManager.StockRankingList
            );

            for (int i = 0; i < codes.Length && i < selected.Count; i++)
            {
                var stock = g.StockManager.Repository.TryGetDataOrNull(selected[i]);
                if (stock == null) continue;
                codes[i] = stock.Code;
            }

            _marketeye.SetInputValue(0, fields);
            _marketeye.SetInputValue(1, codes);

            int result = _marketeye.BlockRequest();
            if (result != 0)
            {

            }
        }

        // CONTINUED REFACTORED _marketeye_received()
        private static void HandleBatchData()
        {
            DateTime date = DateTime.Now;
            int HHmm = Convert.ToInt32(date.ToString("HHmm"));
            int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));
            int HHmmssfff = Convert.ToInt32(date.ToString("HHmmssfff"));

            if (!wk.isWorkingHour()) return;

            //if (HHmm < 1000 || HHmm >= 1621) // 시작시간 10시인 경우
            //    return;

            int count = (int)_marketeye.GetHeaderValue(2); // number of stocks downloaded
            if (count != 200)
                return;

            var downloadList = new List<StockData>();
            List<StockData> localCopy;
            lock (g.lockObject)
            {
                for (int k = 0; k < count; k++)
                {
                    string code = _marketeye.GetDataValue(0, k);
                    var stock = _cpstockcode.CodeToName(code);

                    if (!g.StockRepository.Contains(stock))
                        continue;

                    var data = g.StockRepository.TryGetDataOrNull(stock);
                    if (data == null) 
                        continue;
                    var api = data.Api;

                    if (g.StockRepository.AllDatas.Any(x => x.Stock == data.Stock))
                    {
                        downloadList.Add(data);
                    }

                    api.시초가 = _marketeye.GetDataValue(5, k); // 5
                    api.전고가 = _marketeye.GetDataValue(6, k);  // 6
                    api.전저가 = _marketeye.GetDataValue(7, k);  // 7 
                    api.매도1호가 = _marketeye.GetDataValue(8, k); // 8
                    api.매수1호가 = _marketeye.GetDataValue(9, k); // 9

                    api.현재가 = api.매수1호가;


                    api.거래량 = _marketeye.GetDataValue(10, k);
                    api.총매도호가잔량 = _marketeye.GetDataValue(13, k);
                    api.총매수호가잔량 = _marketeye.GetDataValue(14, k);
                    api.최우선매도호가잔량 = (int)_marketeye.GetDataValue(15, k);
                    api.최우선매수호가잔량 = (int)_marketeye.GetDataValue(16, k);


                    api.전일종가 = _marketeye.GetDataValue(18, k);     // 23
                    api.체강 = (double)_marketeye.GetDataValue(19, k); // 24

                    if (api.전일종가 > 100)
                    {
                        api.가격 = (int)((api.현재가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                        api.시초 = (int)((api.시초가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                        api.전저 = (int)((api.전저가 - (int)api.전일종가) * 10000.0 / api.전일종가);
                    }
                    else
                        continue;

                    if (api.매도1호가 > 100 && api.시초가 > 100 && api.전저가 > 100)
                    {
                        double differ = (api.매도1호가 - api.현재가) * 10000.0 / api.전일종가;
                        double factor = 0.0;
                        if (api.최우선매도호가잔량 + api.최우선매수호가잔량 > 0)
                        {
                            factor = (double)api.최우선매수호가잔량 / (api.최우선매도호가잔량 + api.최우선매수호가잔량);
                        }
                        api.가격 += (int)(differ * factor);
                    }


                    if (!g.StockManager.IndexList.Contains(data.Stock))
                    {
                        api.당일프로그램순매수량 = _marketeye.GetDataValue(26, k); // 116
                        api.당일외인순매수량 = _marketeye.GetDataValue(27, k); // 118
                        api.당일기관순매수량 = _marketeye.GetDataValue(28, k); // 120
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

                    if (api.nrow <= 0 || api.nrow >= g.RealMaximumRow)
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

                    double totalMilliSeconds = Utils.TimeUtils.ElapsedMillisecondsDouble(HHmmssfff, api.x[comparison_row, 0] * 1000);
                    if (totalMilliSeconds <= 0)
                        continue;
                    double multiple_factor = 0.0;
                    if (MathUtils.IsSafeToDivide(totalMilliSeconds) && data.Statistics.일평균거래량 > 100)
                    {
                        multiple_factor = 60.0 / totalMilliSeconds * 380.0 * 1000 / data.Statistics.일평균거래량 * 10.0;
                        api.매수배 = (int)((현누적매수체결거래량 - 전누적매수체결거래량) * multiple_factor);
                        api.매도배 = (int)((현누적매도체결거래량 - 전누적매도체결거래량) * multiple_factor);
                    }

                    // ⏩ Next: build "t" array, append/replace api.x[nrow], shift tick arrays, update continuity


                    // 0, 시간, 1, 가격, 2 수급, 3 체강 * 100, 4 프로그램 매수액(억), 5 외인 매수액(억), 6 기관 매수액(억)
                    // 7 거래량, 8 매수배, 9 매도배, 10 수급연속횟수, 11 체강연속횟수

                    int currentPrice = api.가격;       // or your t[1] value
                    if (g.StockManager.IndexList.Contains(data.Stock))
                    {
                        if (lastPrices.ContainsKey(data.Stock))
                        {
                            int lastPrice = lastPrices[data.Stock];
                            int priceDelta = Math.Abs(currentPrice - lastPrice);

                            if (priceDelta > 50)      // spike threshold (adjust as needed)
                            {
                                // skip this record
                                Console.WriteLine($"[Spike] {data.Stock} {lastPrice} → {currentPrice}");
                                continue;
                            }
                        }

                        // Save this price for next check
                        lastPrices[data.Stock] = currentPrice;
                    }

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
                    else // General
                    {
                        t[4] = (int)api.당일프로그램순매수량;
                        t[5] = (int)api.당일외인순매수량;
                        t[6] = (int)api.당일기관순매수량;
                    }

                    int append_or_replace_row = append ? api.nrow : api.nrow - 1;
                    if (append_or_replace_row >= g.RealMaximumRow) return;

                    for (int i = 0; i < 12; i++)
                    {
                        api.x[append_or_replace_row, i] = t[i];
                    }

                    api.nrow = append_or_replace_row + 1;

                    if (!(g.StockManager.IndexList.Contains(data.Stock) && api.nrow >= 2))
                    {
                        // Continuity of amount ratio
                        if (api.x[api.nrow - 1, 7] == api.x[api.nrow - 2, 7])
                            api.x[api.nrow - 1, 10] = api.x[api.nrow - 2, 10];
                        else if (api.x[api.nrow - 1, 2] > api.x[api.nrow - 2, 2])
                            api.x[api.nrow - 1, 10] = api.x[api.nrow - 2, 10] + 1;
                        else // decrease of amount ratio
                            api.x[api.nrow - 1, 10] = 0;

                        // Continuity of intensity : the intensity was multiplied by g.HUNDRED ->
                        // too many cyan ? -> divided by g.HUNDRED again, let's see
                        if (api.x[api.nrow - 1, 7] == api.x[api.nrow - 2, 7])
                            api.x[api.nrow - 1, 11] = api.x[api.nrow - 2, 11];
                        else if (api.x[api.nrow - 1, 3] / g.HUNDRED > api.x[api.nrow - 2, 3] / g.HUNDRED)
                            api.x[api.nrow - 1, 11] = api.x[api.nrow - 2, 11] + 1;
                        else
                            api.x[api.nrow - 1, 11] = 0;
                    }


                    //?? zero and continue, 코스닥레버리지 and 코스피레버리지
                    totalMilliSeconds = Utils.TimeUtils.ElapsedMillisecondsDouble(HHmmssfff, api.틱의시간[0]); 
                    if (totalMilliSeconds <= 0)
                        continue;

                    double 틱매수체결배수 = 0.0;
                    double 틱매도체결배수 = 0.0;
                    if (MathUtils.IsSafeToDivide(totalMilliSeconds))
                    {
                        multiple_factor = 0.0;
                        if (data.Statistics.일평균거래량 > 100)
                            multiple_factor = 60.0 / totalMilliSeconds * 380.0 * 1000 / data.Statistics.일평균거래량 * 10.0;
                        틱매수체결배수 = (현누적매수체결거래량 - api.틱매수량[0]) * multiple_factor;
                        틱매도체결배수 = (현누적매도체결거래량 - api.틱매도량[0]) * multiple_factor;
                    }

                    // ⏩ Optional next: tick + minute shifting + post_real + index sync
                    api.AppendTick(
                        t,
                        HHmmssfff,
                        현누적매수체결거래량,
                        현누적매도체결거래량,
                        틱매수체결배수,
                        틱매도체결배수,
                        multiple_factor,
                        api.전일종가 / g.천만원 // used as money_factor
                    );
                    api.AppendMinuteIfNeeded(append); // if append == false, just return 

                    if (g.StockRepository.Contains("KODEX 레버리지"))
                    {
                        var kospi = g.StockRepository.TryGetDataOrNull("KODEX 레버리지");
                        int kospiIndex = kospi.Api.x[kospi.Api.nrow - 1, 1];
                        MajorIndex.Instance.KospiIndex = kospiIndex;
                        indexRangeTracker.CheckIndexAndSound(kospiIndex, "Kospi");
                    }

                    if (g.StockRepository.Contains("KODEX 코스닥150레버리지"))
                    {
                        var kosdaq = g.StockRepository.TryGetDataOrNull("KODEX 코스닥150레버리지");
                        int kosdaqIndex = kosdaq.Api.x[kosdaq.Api.nrow - 1, 1];
                        MajorIndex.Instance.KosdaqIndex = kosdaqIndex;
                        indexRangeTracker.CheckIndexAndSound(kosdaqIndex, "Kosdaq");
                    }
                }
                localCopy = downloadList.Select(x => x.Clone()).ToList();
            }

            Task.Run(() => PostProcessor.post_real(localCopy));

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

                var data1 = g.StockRepository.TryGetDataOrNull(items[0]).Api;

                double weight = Convert.ToDouble(items[1]);

                t[1] += data1.가격 * weight;
                t[2] += data1.수급 * weight;
                t[3] += data1.체강 * g.HUNDRED * weight;
                t[7] += data1.거래량 * weight;
                t[8] += data1.매수배 * weight;
                t[9] += data1.매도배 * weight;

                tick_매수배 += data1.틱매수배[0] * weight;
                tick_매도배 += data1.틱매도배[0] * weight;
            }

            t[4] = MajorIndex.Instance.ShanghaiIndex * g.HUNDRED;
            t[5] = MajorIndex.Instance.HangSengIndex * g.HUNDRED;
            t[6] = MajorIndex.Instance.NikkeiIndex * g.HUNDRED;
            t[10] = (int)(MajorIndex.Instance.Snp500Index * g.HUNDRED);
            t[11] = (int)(MajorIndex.Instance.NasdaqIndex * g.HUNDRED);

            if (!!g.StockRepository.Contains(mixed_stock))
                return;

            var data = g.StockRepository.TryGetDataOrNull(mixed_stock);

            int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            int time_bef_4int = data.Api.x[data.Api.nrow - 1, 0] / 100;
            t[0] = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            if (HHmm == time_bef_4int)
            {
                for (int j = 0; j < 12; j++)
                {
                    data.Api.x[data.Api.nrow - 1, j] = (int)t[j];
                }
            }
            else
            {
                if (data.Api.nrow < g.RealMaximumRow)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        data.Api.x[data.Api.nrow, j] = (int)t[j];
                    }
                    data.Api.nrow++;
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
