using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.Trackers;

namespace New_Tradegy.Library.Core
{

    public class RankLogic
    {
        private readonly FormWeights _formWeights;
        // usage : double score1 = _formWeights._푀분;
        public RankLogic(FormWeights weightsForm)
        {
            _formWeights = weightsForm;
        }

        public static List<StockData> RankByTotalScore(IEnumerable<StockData> stocks)
        {
            return stocks
                .OrderByDescending(s => s.Score.총점)
                .Take(150)
                .ToList();
        }

        public static List<StockData> RankByProgramAmount(IEnumerable<StockData> stocks)
        {
            return stocks
                .OrderByDescending(s => s.Api.틱프로천.Sum())
                .Take(150)
                .ToList();
        }

        // Add more strategies as needed
        public static void EvalStock_등합()
        {
            var repo = g.StockRepository;

            var list_푀분 = new List<(double value, string stock)>();
            var list_거분 = new List<(double value, string stock)>();
            var list_배차 = new List<(double value, string stock)>();
            var list_배합 = new List<(double value, string stock)>();
            var list_푀누 = new List<(double value, string stock)>();
            var list_종누 = new List<(double value, string stock)>();
            var list_피로 = new List<(double value, string stock)>();

            foreach (var data in repo.AllDatas)
            {
                list_푀분.Add((data.Score.푀분, data.Stock));
                list_거분.Add((data.Score.거분, data.Stock));
                list_배차.Add((data.Score.배차, data.Stock));
                list_배합.Add((data.Score.배합, data.Stock));
                list_푀누.Add((data.Score.푀누, data.Stock));
                list_종누.Add((data.Score.종누, data.Stock));
                list_피로.Add((data.Score.피로, data.Stock));
            }

            list_푀분 = list_푀분.OrderByDescending(x => x.value).ToList();
            list_거분 = list_거분.OrderByDescending(x => x.value).ToList();
            list_배차 = list_배차.OrderByDescending(x => x.value).ToList();
            list_배합 = list_배합.OrderByDescending(x => x.value).ToList();
            list_푀누 = list_푀누.OrderByDescending(x => x.value).ToList();
            list_종누 = list_종누.OrderByDescending(x => x.value).ToList();
            list_피로 = list_피로.OrderByDescending(x => x.value).ToList();

            foreach (var data in repo.AllDatas)
            {
                data.Score.푀분_등수 = list_푀분.FindIndex(x => x.stock == data.Stock);
                data.Score.거분_등수 = list_거분.FindIndex(x => x.stock == data.Stock);
                data.Score.배차_등수 = list_배차.FindIndex(x => x.stock == data.Stock);
                data.Score.배합_등수 = list_배합.FindIndex(x => x.stock == data.Stock);
                data.Score.푀누_등수 = list_푀누.FindIndex(x => x.stock == data.Stock);
                data.Score.종누_등수 = list_종누.FindIndex(x => x.stock == data.Stock);
                data.Score.피로_등수 = list_피로.FindIndex(x => x.stock == data.Stock);
            }

            // 등합 점수 계산 (가중치 반영 가능)
            foreach (var data in repo.AllDatas)
            {
                data.Score.등합 =
                    data.Score.푀분_등수 +
                    // data.Score.거분_등수 +
                    data.Score.배차_등수 +
                    data.Score.배합_등수;

                // 향후: 그룹별 가중치 적용 가능
                // data.Score.등합 += data.Score.그순 * g.s.그룹_wgt;
            }
        }


        // called by post_real, click, keys, history
        public static void EvalStock()
        {
            var repo = g.StockRepository;
            var resultList = new List<(double value, string code)>();
            double value = 0.0;


            if (g.test) // 실제 run 에서는 post() 에서 합산됨
            {
                foreach (var data in repo.AllDatas)
                {
                    string stock = data.Stock;
                    if (data.Api.nrow < 2)
                        continue;
                    PostProcessor.post(data);

                }
            }



            var specialGroupKeys = new HashSet<string> { "닥올", "피올", "편차", "평균" };

            foreach (var data in repo.AllDatas)
            {
                var holdings = g.StockManager.HoldingList;
                var interestedWithBid = g.StockManager.InterestedWithBidList;
                var interestedOnly = interestedWithBid.Except(holdings).ToList();
                var rankedStocks = g.StockManager.StockRankingList;

                //// 레버리지 외 지수관련 모두 제외;
                if ((data.Stock.Contains("KODEX") ||
                data.Stock.Contains("KOSEF") ||
                data.Stock.Contains("HANARO") ||
                data.Stock.Contains("TIGER") ||
                data.Stock.Contains("KBSTAR") ||
                data.Stock.Contains("혼합") ||
                g.StockManager.HoldingList.Contains(data.Stock) ||
                g.StockManager.InterestedWithBidList.Contains(data.Stock)))
                {
                    continue;
                }

                var stat = data.Statistics;
                string mode = g.v.MainChartDisplayMode;

                if (specialGroupKeys.Contains(mode))
                {
                    switch (mode)
                    {
                        case "피올":
                            if (stat.시장구분 == 'S')
                                resultList.Add((stat.시총, data.Stock));
                            break;

                        case "닥올":
                            if (stat.시장구분 == 'D')
                                resultList.Add((stat.시총, data.Stock));
                            break;

                        case "편차":
                            resultList.Add((stat.일간변동편차, data.Stock));
                            break;

                        case "평균":
                            resultList.Add((stat.일간변동평균, data.Stock));
                            break;
                    }
                }
                else
                {
                    PostProcessor.post(data); // ensure Post values are updated

                    int nrow = data.Api.nrow;
                    //?if (!EvalInclusion(data) || nrow < 2)
                    //?    continue;

                    int checkRow = g.test
                        ? Math.Min(g.Npts[1] - 1, nrow - 1)
                        : nrow - 1;

                    var score = data.Score;
                    var post = data.Post;
                    var api = data.Api;

                    switch (mode)
                    {
                        case "푀누":
                            value = post.푀누천 + post.외누천;
                            break;

                        case "종누":
                            value = post.종거천;
                            break;

                        case "푀분":
                            value = api.분프로천[0] + api.분외인천[0];
                            break;

                        case "총점":
                            value = score.총점;
                            break;

                        case "상순":
                            value = api.x[checkRow, 1];
                            break;

                        case "저순":
                            value = -api.x[checkRow, 1];
                            break;

                        case "배차":
                            value = api.분배수차[0];
                            break;

                        case "분거":
                            value = api.분거래천[0];
                            break;

                        case "가증":
                            value = api.x[checkRow, 1] - api.x[checkRow - 1, 1];
                            break;

                        default:
                            continue;
                    }

                    resultList.Add((value, data.Stock));
                }
            }

            resultList = resultList.OrderByDescending(x => x.value).ToList();

            lock (g.lockObject)
            {
                g.StockManager.StockRankingList.Clear();

                foreach (var (val, stock) in resultList)
                {
                    if (!g.StockManager.StockRankingList.Contains(stock))
                        g.StockManager.StockRankingList.Add(stock);
                }

                string newValue = $"{g.StockManager.StockRankingList.Count}/{repo.AllDatas.Count()}";

                if (g.controlPane.GetCellValue(1, 1) != newValue)
                    g.controlPane.SetCellValue(1, 1, newValue);
            }

            EvalGroup(); // re-evaluate groups after stock ranking
        }



        public static bool EvalInclusion(StockData data)
        {
            string stock = data.Stock;
            var score = data.Score;
            var stat = data.Statistics;
            var post = data.Post;
            var api = data.Api;

            // ❌ Exclude ETFs and already-handled stocks
            if ((g.StockManager.IndexList.Contains(stock) && !stock.Contains("레버리지")) ||
                stock.Contains("KODEX") ||
                stock.Contains("KOSEF") ||
                stock.Contains("HANARO") ||
                stock.Contains("TIGER") ||
                stock.Contains("KBSTAR") ||
                stock.Contains("혼합") ||
                g.StockManager.HoldingList.Contains(stock) ||
                g.StockManager.InterestedWithBidList.Contains(stock) ||
                g.StockManager.InterestedOnlyList.Contains(stock))
            {
                return false;
            }

            // ✅ Always include 누적 계열 regardless of filters
            if (g.v.MainChartDisplayMode == "푀누" ||
                g.v.MainChartDisplayMode == "종누" ||
                g.v.MainChartDisplayMode == "프편" ||
                g.v.MainChartDisplayMode == "종편")
            {
                return true;
            }

            // ❌ Exclude if 푀분 or 배차 filtering is enabled and negative
            if (g.v.푀플 == 1 && score.푀분 < 0)
                return false;

            if (g.v.배플 == 1 && score.배차 < 0)
                return false;

            // ❌ Exclude if 종가 기준 추정 거래액 is below threshold
            if (g.v.종가기준추정거래액이상_천만원 > (int)post.종거천)
                return false;

            // ❌ Real-time check during market hours
            if (wk.isWorkingHour())
            {
                if (post.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
                    post.매수호가거래액_백만원 < g.v.호가거래액_백만원)
                    return false;

                if (g.v.분당거래액이상_천만원 > api.분거래천[0])
                    return false;
            }

            // ❌ Exclude if 편차 below limit
            if (g.v.편차이상 >= stat.일간변동편차)
                return false;

            // ❌ Exclude by 시총 filtering (supports negative logic)
            if (g.v.시총이상 >= 0)
            {
                if (stat.시총 < g.v.시총이상 - 0.01)
                    return false;
            }
            else
            {
                if (stat.시총 > (-1.0) * (g.v.시총이상 - 0.01))
                    return false;
            }

            return true;
        }











        public static void EvalGroup()
        {
            var repo = g.StockRepository;

            var groupManager = g.GroupManager;
            var allGroups = groupManager.GetAll();

            foreach (var group in allGroups)
            {
                // Reset group-level metrics
                group.TotalScore = group.푀분 = group.종누 = group.수평 = group.강평 = group.가증 = 0.0;

                // Sort group stock list if needed
                wk.거분순서(group.Stocks);

                int count = 0;
                foreach (var stock in group.Stocks)
                {
                    if (count == 3) break;

                    var data = repo.TryGetStockOrNull(stock);

                    if (data == null || data.Api.nrow < 2 || data.Api.nrow >= 382)
                        continue;

                    int row = g.test
                        ? Math.Min(g.Npts[1] - 1, data.Api.nrow - 1)
                        : data.Api.nrow - 1;

                    if (data.Api.x[row, 1] < -3000 || data.Api.x[row, 1] > 3000)
                        continue;

                    group.TotalScore += data.Score.총점;
                    group.거분 += data.Score.거분;
                    group.푀분 += data.Score.푀분;
                    group.수평 += data.Api.x[row, 2];
                    group.강평 += data.Api.x[row, 3] / 100.0;
                    group.가증 += data.Api.x[row, 1] - data.Api.x[row - 1, 1];

                    count++;
                }

                if (count > 0)
                {
                    group.TotalScore /= count;
                    group.거분 /= count;
                    group.푀분 /= count;
                    group.수평 /= count;
                    group.강평 /= count;
                    group.가증 /= count;
                }
            }

            switch (g.oGl_data_selection)
            {
                case "총점":
                    groupManager.SortBy(g => g.TotalScore);
                    break;
                case "푀분":
                    groupManager.SortBy(g => g.푀분);
                    break;
                case "가증":
                    groupManager.SortBy(g => g.가증);
                    break;
                default:
                    groupManager.SortBy(g => g.TotalScore);
                    break;
            }

            var rankedGroups = groupManager.GroupRankingList;

            // Store group rank index into each stock
            for (int i = rankedGroups.Count - 1; i >= 0; i--)
            {
                foreach (var stock in rankedGroups[i].Stocks)
                {
                    var data = repo.TryGetStockOrNull(stock);
                    if (data != null)
                        data.Score.그순 = i;
                }
            }

            g.groupPane?.Update(g.GroupManager.GroupRankingList);
        }

    }
}
