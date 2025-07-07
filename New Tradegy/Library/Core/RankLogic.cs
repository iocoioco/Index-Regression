using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.Trackers;

namespace New_Tradegy.Library.Core
{

    public class RankLogic
    {
        public static List<StockData> RankBy등합(IEnumerable<StockData> datas)
        {
            return datas
                .OrderByDescending(s => s.Score.등합)
                .Take(150)
                .ToList();
        }

        public static void RankByMode()
        {
            // Stopwatch sw = Stopwatch.StartNew();
            
            if (g.test) // 실제 run 에서는 post() 에서 합산됨
            {
                foreach (var data in g.StockRepository.AllDatas)
                {
                    string stock = data.Stock;
                    if (data.Api.nrow < 2)
                        continue;
                    PostProcessor.post(data);
                }
            }

            RankBy등합();
            RankGroup(); // re-evaluate groups after stock ranking

            RankByModes();

            //sw.Stop();
            //double sec = sw.Elapsed.TotalSeconds;
        }

        // 푀분 : chart는 푀분 r3_display는 프분 only
        // 푀분, 배차 양이라도 가격 하락의 경우는 하락이 많음 : 이유 ?
        // 지수 영향
        // 피로
        public static void RankBy등합()
        {
            var repo = g.StockRepository;

            var list_푀분 = new List<(double value, string stock)>();
            var list_거분 = new List<(double value, string stock)>();
            var list_배차 = new List<(double value, string stock)>();
            var list_배합 = new List<(double value, string stock)>();
            var list_푀누 = new List<(double value, string stock)>();
            var list_종누 = new List<(double value, string stock)>();
            var list_피로 = new List<(double value, string stock)>();
            var list_등합 = new List<(double value, string stock)>();

            var selectedDatas = new List<StockData>();

            foreach (var data in repo.AllGeneralDatas)
            {
                if (!EvalInclusion(data))
                    continue;
                selectedDatas.Add(data);
                list_푀분.Add((data.Score.푀분, data.Stock));
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

            foreach (var data in selectedDatas)
            {
                data.Score.푀분_등수 = list_푀분.FindIndex(x => x.stock == data.Stock);

                data.Score.배차_등수 = list_배차.FindIndex(x => x.stock == data.Stock);
                data.Score.배합_등수 = list_배합.FindIndex(x => x.stock == data.Stock);
                data.Score.푀누_등수 = list_푀누.FindIndex(x => x.stock == data.Stock);
                data.Score.종누_등수 = list_종누.FindIndex(x => x.stock == data.Stock);
                data.Score.피로_등수 = list_피로.FindIndex(x => x.stock == data.Stock);
            }

            // 등합 점수 계산 (가중치 반영 가능)
            foreach (var data in selectedDatas)
            {
                data.Score.등합 =
                    data.Score.푀분_등수 * WeightManager.Weights["푀분"] +
                    data.Score.배차_등수 * WeightManager.Weights["배차"] +
                    data.Score.배합_등수 * WeightManager.Weights["배합"];

                list_등합.Add((data.Score.등합, data.Stock));
            }

            list_등합 = list_등합.OrderBy(x => x.value).ToList(); // Ascending

            lock (g.lockObject)
            {
                g.StockManager.StockRankingList.Clear();

                int rank = 0;
                foreach (var (val, stock) in list_등합)
                {
                    if (!g.StockManager.StockRankingList.Contains(stock))
                        g.StockManager.StockRankingList.Add(stock);
                    var data = g.StockRepository.TryGetDataOrNull(stock);
                    data.Score.등합_등수 = rank++;
                }

                string newValue = $"{g.StockManager.StockRankingList.Count}/{repo.AllGeneralDatas.Count()}";

                if (g.controlPane.GetCellValue(1, 1) != newValue)
                    g.controlPane.SetCellValue(1, 1, newValue);
            }
        }


        // called by post_real, click, keys, history
        public static void RankByModes()
        {
            var repo = g.StockRepository;
            var resultList = new List<(double value, string code)>();
            double value = 0.0;

            var specialGroupKeys = new HashSet<string> { "닥올", "피올", "편차", "평균" };

            foreach (var data in repo.AllGeneralDatas)
            {
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
                    //PostProcessor.post(data); // ensure Post values are updated

                    int checkRow = g.test
                        ? Math.Min(g.Npts[1] - 1, data.Api.nrow - 1)
                        : data.Api.nrow - 1;
                    if (!EvalInclusion(data) || checkRow < 1)
                        continue;

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
                g.StockManager.StockRankingByModesList.Clear();

                foreach (var (val, stock) in resultList)
                {
                    if (!g.StockManager.StockRankingByModesList.Contains(stock))
                        g.StockManager.StockRankingByModesList.Add(stock);
                }

                string newValue = $"{g.StockManager.StockRankingByModesList.Count}/{repo.AllGeneralDatas.Count()}";

                if (g.controlPane.GetCellValue(1, 1) != newValue)
                    g.controlPane.SetCellValue(1, 1, newValue);
            }
        }



        public static bool EvalInclusion(StockData data)
        {
            string stock = data.Stock;
            var score = data.Score;
            var stat = data.Statistics;
            var post = data.Post;
            var api = data.Api;

            // ❌ Exclude ETFs and already-handled stocks
            if (g.StockManager.IndexList.Contains(stock))
            {
                return false;
            }

            // ✅ Always include 누적 계열 regardless of filters
            if (g.v.MainChartDisplayMode == "푀누" ||
                g.v.MainChartDisplayMode == "종누")
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
                if (!g.test)
                {
                    if (post.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
                    post.매수호가거래액_백만원 < g.v.호가거래액_백만원)
                        return false;

                    if (g.v.분당거래액이상_천만원 > api.분거래천[0])
                        return false;
                }
            }

            // ❌ Exclude if 편차 below limit
            if (g.v.편차이상 >= stat.일간변동편차)
                return false;

            // ❌ Exclude by 시총 filtering (supports negative logic)
            if (g.v.시총이상 >= 0)
            {
                if (stat.시총  < g.v.시총이상 - 0.01) // 시총 1조는 stat.시총 100으로 preprocessed
                    return false;
            }
            

            return true;
        }











        public static void RankGroup()
        {
            var repo = g.StockRepository;

            var groupManager = g.GroupManager;
            var allGroups = groupManager.GetAll();

            foreach (var group in allGroups)
            {
                // Reset group-level metrics
                group.TotalScore = group.푀분 = group.등합_등수 = group.배차 = group.수평 = group.강평 = group.가증 = 0.0;

                // Sort group stock list if needed
                wk.거분순서(group.Stocks);

                int count = 0;
                foreach (var stock in group.Stocks)
                {
                    if (!g.StockManager.StockRankingList.Contains(stock))
                        continue;

                    if (count == 3) break;

                    var data = repo.TryGetDataOrNull(stock);

                    if (data == null || data.Api.nrow < 2 || data.Api.nrow > 382)
                        continue;

                    int row = g.test
                        ? Math.Min(g.Npts[1] - 1, data.Api.nrow - 1)
                        : data.Api.nrow - 1;

                    if (data.Api.x[row, 1] < -3000 || data.Api.x[row, 1] > 3000)
                        continue;


                    group.등합_등수 += data.Score.등합_등수;
                    group.푀분 += data.Score.푀분;
                    group.배차 += data.Score.배차;
                    count++;
                }

                if (count > 0)
                {
                    group.등합_등수 /= count;
                    group.푀분 /= count;
                    group.배차 /= count;
                }
                else
                {
                    group.등합_등수 = 10000.0;
                }
            }

            groupManager.OrderBy(g => g.등합_등수);
            var rankedGroups = groupManager.GroupRankingList;

            // Store group rank index into each stock
            for (int i = rankedGroups.Count - 1; i >= 0; i--)
            {
                foreach (var stock in rankedGroups[i].Stocks)
                {
                    var data = repo.TryGetDataOrNull(stock);
                    if (data != null)
                        data.Score.그룹_등수 = i;
                }
            }

            g.groupPane?.Update(g.GroupManager.GroupRankingList);
        }

    }
}
