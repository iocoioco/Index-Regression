using System;
using System.Collections.Generic;
using System.Linq;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.PostProcessing;

namespace New_Tradegy.Library.Core
{
    public static class RankLogic
    {
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

            var list_푀분 = new List<(double value, string code)>();
            var list_거분 = new List<(double value, string code)>();
            var list_배차 = new List<(double value, string code)>();
            var list_배합 = new List<(double value, string code)>();

            foreach (var stock in repo.AllDatas)
            {
                list_푀분.Add((stock.Score.푀분, stock.Stock));
                list_거분.Add((stock.Score.거분, stock.Stock));
                list_배차.Add((stock.Score.배차, stock.Stock));
                list_배합.Add((stock.Score.배합, stock.Stock));
            }

            // 그룹 상태일 경우 등수 계산 생략
            if (!g.q.Contains("&g"))
            {
                list_푀분 = list_푀분.OrderByDescending(x => x.value).ToList();
                list_거분 = list_거분.OrderByDescending(x => x.value).ToList();
                list_배차 = list_배차.OrderByDescending(x => x.value).ToList();
                list_배합 = list_배합.OrderByDescending(x => x.value).ToList();
            }

            foreach (var stock in repo.AllDatas)
            {
                stock.Score.푀분_등수 = list_푀분.FindIndex(x => x.code == stock.Stock);
                stock.Score.거분_등수 = list_거분.FindIndex(x => x.code == stock.Stock);
                stock.Score.배차_등수 = list_배차.FindIndex(x => x.code == stock.Stock);
                stock.Score.배합_등수 = list_배합.FindIndex(x => x.code == stock.Stock);
            }

            // 등합 점수 계산 (가중치 반영 가능)
            foreach (var stock in repo.AllDatas)
            {
                stock.Score.등합 =
                    stock.Score.푀분_등수 +
                    // stock.Score.거분_등수 +
                    stock.Score.배차_등수 +
                    stock.Score.배합_등수;

                // 향후: 그룹별 가중치 적용 가능
                // stock.Score.등합 += stock.Score.그순 * g.s.그룹_wgt;
            }
        }



        public static void EvalStock()
        {
            var repo = g.StockRepository;
            var resultList = new List<(double value, string code)>();
            double value = 0.0;

            var specialGroupKeys = new HashSet<string> { "닥올", "피올", "편차", "평균" };

            if (specialGroupKeys.Contains(g.v.KeyString))
            {
                foreach (var stock in repo.AllDatas)
                {
                    var stat = stock.Statistics;

                    switch (g.v.KeyString)
                    {
                        case "피올":
                            if (stat.시장구분 == 'S')
                                resultList.Add((stat.시총, stock.Stock));
                            break;

                        case "닥올":
                            if (stat.시장구분 == 'D')
                                resultList.Add((stat.시총, stock.Stock));
                            break;

                        case "편차":
                            resultList.Add((stat.일간변동편차, stock.Stock));
                            break;

                        case "평균":
                            resultList.Add((stat.일간변동평균, stock.Stock));
                            break;
                    }
                }
            }
            else
            {
                foreach (var stock in repo.AllDatas)
                {
                    PostProcessor.post(stock); // ensure Post values are updated

                    int nrow = stock.Api.nrow;
                    if (!EvalInclusion(stock) || nrow < 2)
                        continue;

                    int checkRow = g.test
                        ? Math.Min(g.Npts[1] - 1, nrow - 1)
                        : nrow - 1;

                    var score = stock.Score;
                    var stat = stock.Statistics;
                    var post = stock.Post;
                    var api = stock.Api;

                    switch (g.v.KeyString)
                    {
                        case "총점":
                            value = score.총점;
                            break;

                        case "프누":
                            value = post.프누천 + post.외누천;
                            break;

                        case "종누":
                            value = post.종거천;
                            break;

                        case "프편":
                            value = (post.프누천 + post.외누천) / stat.프분_dev;
                            break;

                        case "종편":
                            value = post.종거천 / stat.프분_dev;
                            break;

                        case "푀분":
                            value = api.분프로천[0] + api.분외인천[0];
                            break;

                        case "배차":
                            value = api.분배수차[0];
                            break;

                        case "가증":
                            value = api.x[checkRow, 1] - api.x[checkRow - 1, 1];
                            break;

                        case "분거":
                            value = api.분거래천[0];
                            break;

                        case "상순":
                            value = api.x[checkRow, 1];
                            break;

                        case "저순":
                            value = -api.x[checkRow, 1];
                            break;
                    }

                    resultList.Add((value, stock.Stock));
                }
            }

            resultList = resultList.OrderByDescending(x => x.value).ToList();

            lock (g.lockObject)
            {
                g.sl.Clear();

                foreach (var (val, code) in resultList)
                {
                    if (!g.sl.Contains(code))
                        g.sl.Add(code);
                }

                string newValue = $"{g.sl.Count}/{repo.AllDatas.Count()}";
                if (g.제어.dtb.Rows[1][1].ToString() != newValue)
                {
                    g.제어.dtb.Rows[1][1] = newValue;
                }
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
            if (g.v.KeyString == "프누" ||
                g.v.KeyString == "종누" ||
                g.v.KeyString == "프편" ||
                g.v.KeyString == "종편")
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
                foreach (var stockName in group.Stocks)
                {
                    if (count == 3) break;

                    var stock = repo.TryGetStockOrNull(stockName);

                    if (stock == null || stock.Api.nrow < 2 || stock.Api.nrow >= 382)
                        continue;

                    int row = g.test
                        ? Math.Min(g.Npts[1] - 1, stock.Api.nrow - 1)
                        : stock.Api.nrow - 1;

                    if (stock.Api.x[row, 1] < -3000 || stock.Api.x[row, 1] > 3000)
                        continue;

                    group.TotalScore += stock.Score.총점;
                    group.거분 += stock.Score.거분;
                    group.푀분 += stock.Score.푀분;
                    group.수평 += stock.Api.x[row, 2];
                    group.강평 += stock.Api.x[row, 3] / 100.0;
                    group.가증 += stock.Api.x[row, 1] - stock.Api.x[row - 1, 1];

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
                foreach (var stockName in rankedGroups[i].Stocks)
                {
                    var stock = repo.TryGetStockOrNull(stockName);
                    if (stock != null)
                        stock.Score.그순 = i;
                }
            }

            g.그룹.GroupRenderer?.Update(g.GroupManager.GroupRankingList);
        }

    }
}
