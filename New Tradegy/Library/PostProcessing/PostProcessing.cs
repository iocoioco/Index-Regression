using System;
using System.Collections.Generic;
using System.Windows.Forms;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Trackers.Charting;


/* 배점 : 대형주 배차 조건 충족시, 수일 과다 움직임 높게(dev 큰 종목 가능성 높음) + 배차
 *  1차 상승시 접근, 2차 이상 자제
 * 그룹 전체도 동일조건 적용
 * 
 * 순간 엄청난 거분 + 프분
 * 누적 프돈, 거돈
 * 
 * 추가조건 : 간단접근 -> 세밀접근
 * 
 * 프돈의 손실종목
 * 
 * 프 매수 가 하락 또는 횡보 -> 가 하락 가능
 * 프 매도 가 상승 -> 프 매수 전환 가능
 * 
 * 기본 : 검둥이 급상 후 매도
 * 분거래액이 일정이상이어야 검토
 * 체강 낮은 종목 제외
 * 돌파 + 배수차 + 분거래액
 * 분거래액이 과대
 * 
 * 돌파 정의 : 강한 돌파 동시 진입 전저조건 -50
 * 
 * 매수 종목 손절 잡고 진입(횟수 줄이고 베팅액 증가)
 * 
 * 자동 매도
 * 
 * 양쪽 지수 프돈 지속 증가 또는 큰 양전
*/
namespace New_Tradegy.Library.PostProcessing
{
    public class PostProcessor
    {
        public static void ManageChart1Invoke()
        {
            Form se = (Form)Application.OpenForms["Form1"];
            if (se.InvokeRequired)
            {
                se.Invoke(new Action(() => g.ChartMainRenderer.RefreshMainChart()));
            }
            else
            {
                g.ChartMainRenderer.RefreshMainChart();
            }
        }
        public static void ManageChart2Invoke()
        {
        
            Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
            if (Form_보조_차트.InvokeRequired)
            {
                Form_보조_차트.Invoke(new Action(() => Form_보조_차트.Form_보조_차트_DRAW()));
            }
            else
            {
                Form_보조_차트.Form_보조_차트_DRAW();
            }
        }

        // done by Sensei
        public static void post_real(List<StockData> downloadedList)
        {
            foreach (var data in downloadedList)
            {
                post(data);
            }

            if (g.MarketeyeCount % g.MarketeyeCountDivicer == 1)
            {
                RankLogic.EvalStock();
            }

            foreach (var data in downloadedList)
            {
                if (g.StockManager.HoldingList.Contains(data.Stock)) // Stock is the name
                {
                    g.매매.TradeRenderer?.Update(); // update DGV for holding stock
                    marketeye_received_보유종목_푀분의매수매도_소리내기(data); // renamed to match new structure
                }
            }

            ManageChart1Invoke();
            ManageChart2Invoke();
        }

        // done by Sensei
        public static void post_test()
        {
            foreach (var stock in g.StockRepository.AllDatas)
            {
                post(stock);
            }
        }

        // done by Sensei
        public static void marketeye_received_보유종목_푀분의매수매도_소리내기(StockData data)
        {
            var api = data.Api;
            var stat = data.Statistics;

            if (data.Deal.보유량 * api.전일종가 < 200000)
                return;

            double sound_indicator = 0;

            if (Math.Abs(api.틱프로천[0]) > 0.1 || Math.Abs(api.틱외인천[0]) > 0.1)
            {
                string sound = "";
                int orderIndex = -1;

                for (int i = 0; i < 3; i++)
                {
                    if (g.StockManager.HoldingList[i] == data.Stock)
                    {
                        orderIndex = i;
                        sound = i == 0 ? "one " : i == 1 ? "two " : "three ";
                        break;
                    }
                }
                if (orderIndex < 0)
                    return;

                if (stat.프분_dev > 0)
                {
                    sound_indicator = (api.틱프로천[0] + api.틱외인천[0]) / stat.프분_dev;

                    if (sound_indicator > 2) sound += "buyest";
                    else if (sound_indicator > 1) sound += "buyer";
                    else if (sound_indicator > 0) sound += "buy";
                    else if (sound_indicator > -1) sound += "sell";
                    else if (sound_indicator > -2) sound += "seller";
                    else sound += "sellest";
                }

                Utils.SoundUtils.Sound("가", sound);
            }

            // Future: 비상매도 condition logic
        }

        // done by Sensei
        public static double post_score(StockData data, int check_row)
        {
            var stat = data.Statistics;
            var score = data.Score;
            var api = data.Api;

            double sending_value;

            if (MathUtils.IsSafeToDivide(stat.프분_dev))
            {
                sending_value = (api.분프로천[0] + api.분외인천[0]) / stat.프분_dev;
                score.푀분 = sending_value;
                if (api.분프로천[0] > 5 && api.분외인천[0] > 5)
                {
                    score.푀분 *= 1.5;
                }
            }
            else
            {
                score.푀분 = 0.0;
            }
            score.푀분 *= g.s.푀분_wgt;

            if (MathUtils.IsSafeToDivide(stat.배차_dev))
            {
                sending_value = (api.분배수차[0] - stat.배차_avr) / stat.배차_dev;
                score.배차 = sending_value;
            }
            else
            {
                score.배차 = 0.0;
            }
            score.배차 *= g.s.배차_wgt;

            if (MathUtils.IsSafeToDivide(stat.배합_dev))
            {
                sending_value = (api.분배수합[0] - stat.배합_avr) / stat.배합_dev;
                score.배합 = sending_value;
            }
            else
            {
                score.배합 = 0.0;
            }
            score.배합 *= g.s.배합_wgt;

            return score.푀분 + score.배차 + score.배합;
        }

        // done by Sensei
        public static void post_코스닥_코스피_프외_순매수_배차_합산_382()
        {
            int index;

            var repo = g.StockRepository;
            var kospi_leverage = repo.TryGetStockOrNull(g.StockManager.IndexList[0]);
            var kosdaq_leverage = repo.TryGetStockOrNull(g.StockManager.IndexList[1]);
            var kospi_inverse = repo.TryGetStockOrNull(g.StockManager.IndexList[2]);
            var kosdaq_inverse = repo.TryGetStockOrNull(g.StockManager.IndexList[3]);

            for (int i = 0; i < 382; i++)
            {
                double sum = 0.0;
                foreach (var stock in g.kospi_mixed.stock)
                {
                    if (!repo.Contains(stock)) continue;
                    var data = repo.TryGetStockOrNull(stock);
                    if (data != null) continue;
                    double money_factor = data.Api.전일종가 / g.억원;
                    sum += (int)((data.Api.x[i, 4] + data.Api.x[i, 5]) * money_factor);
                }
                kospi_leverage.Api.x[i, 3] = (int)sum;
                kospi_inverse.Api.x[i, 3] = (int)sum;

                sum = 0.0;
                foreach (var stock in g.kosdaq_mixed.stock)
                {
                    if (!repo.Contains(stock)) continue;
                    var data = repo.TryGetStockOrNull(stock);
                    double money_factor = data.Api.전일종가 / g.억원;
                    sum += (int)((data.Api.x[i, 4] + data.Api.x[i, 5]) * money_factor);
                }
                kosdaq_leverage.Api.x[i, 3] = (int)sum;
                kosdaq_inverse.Api.x[i, 3] = (int)sum;
            }
        }

        // done by Sensei
        public static void post_코스닥_코스피_프외_순매수_배차_합산()
        {
            var repo = g.StockRepository;

            MajorIndex.Instance.KospiSellPower = 0;
            MajorIndex.Instance.KospiBuyPower = 0;
            MajorIndex.Instance.KospiProgramNetBuy = 0;
            MajorIndex.Instance.KospiForeignNetBuy = 0;

            for (int i = 0; i < g.kospi_mixed.stock.Count; i++)
            {
                string stock = g.kospi_mixed.stock[i];
                if (!repo.Contains(stock)) continue;

                var data = repo.TryGetStockOrNull(stock);
                var api = data.Api;
                double money_factor = api.전일종가 / g.억원;

                int row = api.nrow - 1;
                MajorIndex.Instance.KospiProgramNetBuy += (int)(api.x[row, 4] * money_factor);
                MajorIndex.Instance.KospiForeignNetBuy += (int)(api.x[row, 5] * money_factor);
                MajorIndex.Instance.KospiBuyPower = (int)(api.x[row, 8] * g.kospi_mixed.weight[i]);
                MajorIndex.Instance.KospiSellPower = (int)(api.x[row, 9] * g.kospi_mixed.weight[i]);
            }

            MajorIndex.Instance.KosdaqBuyPower = 0;
            MajorIndex.Instance.KosdaqSellPower = 0;
            MajorIndex.Instance.KosdaqProgramNetBuy = 0;
            MajorIndex.Instance.KosdaqForeignNetBuy = 0;

            for (int i = 0; i < g.kosdaq_mixed.stock.Count; i++)
            {
                string stock = g.kosdaq_mixed.stock[i];
                if (!repo.Contains(stock)) continue;

                var data = repo.TryGetStockOrNull(stock);
                var api = data.Api;
                double money_factor = api.전일종가 / g.억원;

                int row = api.nrow - 1;
                MajorIndex.Instance.KosdaqProgramNetBuy += (int)(api.x[row, 4] * money_factor);
                MajorIndex.Instance.KosdaqForeignNetBuy += (int)(api.x[row, 5] * money_factor);
                MajorIndex.Instance.KosdaqBuyPower = (int)(api.x[row, 8] * g.kosdaq_mixed.weight[i]);
                MajorIndex.Instance.KosdaqSellPower = (int)(api.x[row, 9] * g.kosdaq_mixed.weight[i]);
            }
        }

        // done by Sensei
        public static void post(StockData data)
        {
            string stock = data.Stock;

            if ((stock.Contains("KODEX") && !stock.Contains("레버리지")) ||
                stock.Contains("KODSEF") ||
                stock.Contains("TIGER") ||
                stock.Contains("KBSTAR") ||
                stock.Contains("HANARO"))
            {
                return;
            }

            int check_row = g.test
                ? Math.Min(g.Npts[1] - 1, data.Api.nrow - 1)
                : data.Api.nrow - 1;

            post_minute(data, check_row);

            data.Score.총점 = post_score(data, check_row);

            PostPassing(data, check_row, true);
        }

        // done by Sensei
        public static void PostPassing(StockData data, int checkRow, bool add)
        {
            var pass = data.Pass;
            var api = data.Api;
            bool previousPriceReset = false;

            int price = api.x[checkRow, 1];

            if (price < pass.PreviousPriceHigh)
            {
                pass.PriceStatus = 0;
            }
            else if (add && price > pass.PreviousPriceHigh && pass.PreviousPriceLow.HasValue)
            {
                if (price > pass.PreviousPriceLow.Value)
                {
                    pass.PreviousPriceHigh = price;
                    previousPriceReset = true;
                    pass.PreviousPriceLow = null;
                    pass.PriceStatus = 1;

                    if (!g.StockManager.InterestedOnlyList.Contains(data.Stock) &&
                        !data.Stock.Contains("KODEX") &&
                        api.분거래천[0] > 50 &&
                        api.분배수차[0] > 100 &&
                        api.분프로천[0] >= 0 &&
                        g.add_interest)
                    {
                        if (g.StockManager.InterestedOnlyList.Count > 2)
                        {
                            g.StockManager.InterestedOnlyList.RemoveAt(0);
                        }
                        g.StockManager.InterestedOnlyList.Add(data.Stock);
                    }
                }
                else
                {
                    pass.PriceStatus = 2;
                }
            }
            else if (price > pass.PreviousPriceHigh && !pass.PreviousPriceLow.HasValue)
            {
                pass.PreviousPriceHigh = price;
                pass.PriceStatus = 2;
            }

            if (pass.PreviousPriceHigh - 50 > price && !previousPriceReset)
            {
                pass.PreviousPriceLow = price;
            }

            if (pass.PreviousPriceHigh > pass.Month)
                pass.MonthStatus = 1;
            if (pass.PreviousPriceHigh > pass.Quarter)
                pass.QuarterStatus = 1;
            if (pass.PreviousPriceHigh > pass.Half)
                pass.HalfStatus = 1;
            if (pass.PreviousPriceHigh > pass.Year)
                pass.YearStatus = 1;
        }

        // done by Sensei
        // 통계 적용
        public static void post_score_급락(StockData data, int check_row)
        {
            if (g.StockManager.IndexList.Contains(data.Stock))
                return;

            var api = data.Api;
            var score = data.Score;

            int drop_count = 0;
            for (int i = check_row - 1; i >= 1; i--)
            {
                if (api.x[i, 1] - api.x[i - 1, 1] < 0)
                    drop_count++;
                else
                    break;
            }

            double drop_value = api.x[check_row - 1, 1] - api.x[check_row - 1 - drop_count, 1];
            score.급락 = drop_value;
        }

        // done by Sensei
        public static void post_score_급상(StockData data, int check_row)
        {
            if (g.StockManager.IndexList.Contains(data.Stock))
                return;

            var api = data.Api;
            var score = data.Score;

            int rise_count = 0;
            for (int i = check_row; i >= 1; i--)
            {
                if (i >= MajorIndex.MinuteArraySize)
                    break;

                if (api.x[i, 1] - api.x[i - 1, 1] > 0)
                    rise_count++;
                else
                    break;
            }

            double rise_value = api.x[check_row, 1] - api.x[check_row - rise_count, 1];
            score.급상 = rise_value;
        }

        // done by Sensei
        public static void post_score_interpolation(List<List<double>> s,
            double 성적, ref double 획득점수)
        {
            획득점수 = 0; // 초기화

            if (s.Count < 2 || s[0][0] == 0) // less than 3 ... can not interpolate
                return;

            double 배점 = s[0][0];

            double u_ratio = 0;
            double l_ratio = 0;

            int position = s.Count; // the number of array does exceed this number
            for (int i = 1; i < s.Count; i++)
            {
                if (성적 < s[i][0])
                {
                    position = i;
                    break;
                }
            }

            if (position == 1)
                획득점수 = 배점 * s[1][1];
            else if (position == s.Count)
                획득점수 = 배점 * s[s.Count - 1][1];
            else
            {
                double divider = s[position][0] - s[position - 1][0];

                if (MathUtils.IsSafeToDivide(divider))
                {
                    l_ratio = (s[position][0] - 성적) / divider;
                    u_ratio = (성적 - s[position - 1][0]) / divider;

                    획득점수 = 배점 * (s[position - 1][1] * l_ratio + s[position][1] * u_ratio);
                }
                else
                {
                    획득점수 = 0;
                }
            }
        }

        // done by Sensei
        public static void post_minute(StockData data, int check_row)
        {
            var api = data.Api;
            var post = data.Post;

            if (api.nrow < 2) return;

            double money_factor = api.전일종가 / g.천만원;

            post.푀누천 = api.x[check_row, 4] * money_factor;
            post.외누천 = api.x[check_row, 5] * money_factor;
            post.기누천 = api.x[check_row, 6] * money_factor;
            post.거누천 = api.x[check_row, 7] * money_factor;
            post.종거천 = api.x[check_row, 7] * money_factor * wk.누적거래액환산율(api.x[check_row, 0]);
            post.매도호가거래액_백만원 = (int)(api.최우선매도호가잔량 * money_factor * 10);
            post.매수호가거래액_백만원 = (int)(api.최우선매수호가잔량 * money_factor * 10);

            if (g.test)
            {
                double multiple_factor = data.Statistics.일평균거래량 > 0 ? 380.0 / data.Statistics.일평균거래량 : 0;
                if (multiple_factor == 0) return;

                for (int i = 0; i < MajorIndex.MinuteArraySize - 1; i++)
                {
                    if (check_row - i - 1 < 0) break;

                    api.분프로천[i] = (int)((api.x[check_row - i, 4] - api.x[check_row - i - 1, 4]) * money_factor);
                    api.분외인천[i] = (int)((api.x[check_row - i, 5] - api.x[check_row - i - 1, 5]) * money_factor);
                    api.분거래천[i] = (int)((api.x[check_row - i, 7] - api.x[check_row - i - 1, 7]) * money_factor);
                    api.분매수배[i] = (int)((api.x[check_row, 8] + api.x[check_row - 1, 8]) * multiple_factor);
                    api.분매도배[i] = (int)((api.x[check_row, 9] + api.x[check_row - 1, 9]) * multiple_factor);
                    api.분배수차[i] = api.x[check_row, 8] - api.x[check_row, 9];
                    api.분배수합[i] = api.x[check_row, 8] + api.x[check_row, 9];
                }

                if (check_row > 1)
                    post.분당가격차 = api.x[check_row, 1] - api.x[check_row - 1, 1];
            }
            else
            {
                if (api.틱의시간[1] == 0 || data.Statistics.일평균거래량 == 0 || api.전일종가 == 0) return;

                int selected = MajorIndex.TickArraySize - 1;
                for (int i = 1; i < MajorIndex.TickArraySize; i++)
                {
                    if (api.틱의시간[i] == 0)
                    {
                        selected = i - 1;
                        break;
                    }
                    double elapsed = Utils.TimeUtils.total_Seconds(api.틱의시간[i], api.틱의시간[0]);
                    if (elapsed > g.postInterval)
                    {
                        selected = i;
                        break;
                    }
                }

                if (selected == 0)
                    selected = MajorIndex.TickArraySize - 1;

                if (selected > 0)
                {
                    double totalSeconds = Utils.TimeUtils.total_Seconds(api.틱의시간[selected], api.틱의시간[0]);
                    if (totalSeconds == 0) return;

                    int amount = api.틱매수량[0] - api.틱매수량[selected] + api.틱매도량[0] - api.틱매도량[selected];
                    int progAmount = api.틱프로량[0] - api.틱프로량[selected];
                    int foreignAmount = api.틱외인량[0] - api.틱외인량[selected];

                    double tmFactor = api.전일종가 / g.천만원 / totalSeconds * 60;
                    double multiFactor = 60.0 / totalSeconds * 380.0 / data.Statistics.일평균거래량;

                    api.분프로천[0] = (int)(progAmount * tmFactor);
                    api.분외인천[0] = (int)(foreignAmount * tmFactor);
                    api.분거래천[0] = (int)(amount * tmFactor);
                    api.분매수배[0] = (int)((api.틱매수량[0] - api.틱매수량[selected]) * multiFactor);
                    api.분매도배[0] = (int)((api.틱매도량[0] - api.틱매도량[selected]) * multiFactor);
                    api.분배수차[0] = api.분매수배[0] - api.분매도배[0];
                    api.분배수합[0] = api.분매수배[0] + api.분매도배[0];

                    post.분당가격차 = (int)((api.틱의가격[0] - api.틱의가격[selected]) / totalSeconds * 60);
                }
            }

            if (api.분거래천[0] > 0)
            {
                data.Level.프퍼 = 100.0 * api.분프로천[0] / api.분거래천[0];
                data.Level.푀퍼 = 100.0 * (api.분프로천[0] + api.분외인천[0]) / api.분거래천[0];
            }
            else
            {
                data.Level.프퍼 = 0.0;
                data.Level.푀퍼 = 0.0;
            }
        }

        // done by Sensei
        public static void post_minute_급락(StockData data)
        {
            data.Level.급락 = 0;
            var api = data.Api;

            double price_down = 0;
            for (int i = api.nrow - 2; i > 0; i--)
            {
                int price_diff = api.x[api.nrow - 1, 1] - api.x[i, 1];
                if (price_diff < price_down)
                    price_down = price_diff;
                else
                    break;
            }
        }

        // done by Sensei
        public static void post_minute_잔잔(StockData data)
        {
            data.Level.잔잔 = 0;
            var api = data.Api;

            int range = 5;
            int total = 0;
            for (int i = 1; i <= range; i++)
            {
                total += api.분배수차[i];
            }
            if (range != 0)
                data.Level.잔잔 = total / (double)range;
        }

        // done by Sensei
        public static int post_minute_직선(StockData data)
        {
            var api = data.Api;
            if (api.nrow < 5)
                return -1;

            int count = 5;
            if (api.nrow < count + 1)
                return 0;

            double[] xVals = new double[count];
            double[] yVals = new double[count];

            for (int i = 0; i < count; i++)
            {
                xVals[i] = i;
                yVals[i] = api.x[api.nrow - 1 - count + i, 1];
            }

            Utils.MathUtils.LinearRegression(xVals, yVals, out double rSquared, out double yIntercept, out double slope);

            return 1;
        }
    }
}
