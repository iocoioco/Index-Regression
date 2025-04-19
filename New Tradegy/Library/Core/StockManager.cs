using System;
using System.Collections.Generic;
using System.Linq;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;

namespace New_Tradegy
{
    public class StockManager
    {
        private readonly StockRepository _repository;
        public StockRepository Repository => _repository;

        public List<string> TotalStockList { get; } = new List<string>();
        public List<string> RankingList { get; private set; } = new List<string>();
        public List<string> HoldingList { get; private set; } = new List<string>();
        public List<string> LeverageList { get; private set; } = new List<string>();
        public List<string> InverseList { get; private set; } = new List<string>();
        public List<string> IndexList { get; private set; }

        public List<string> InterestedWithBidList { get; private set; } = new List<string>();   // Right-click zone 5
        public List<string> InterestedOnlyList { get; private set; } = new List<string>();      // Right-click zone 4

        public Dictionary<string, List<string>> Groups { get; private set; } = new Dictionary<string, List<string>>();

        public StockManager(StockRepository repository)
        {
            _repository = repository;

            // Set leverage indices once (can be updated if needed)
            LeverageList = new List<string> { "KODEX 레버리지", "KODEX 코스닥150레버리지" };
            InverseList = new List<string> { "KODEX 200선물인버스2X", "KODEX 코스닥150선물인버스" };
            IndexList = LeverageList.Concat(InverseList).ToList();
        }

        public void UpdateTick(string stock, double[] newTickPro)
        {
            var data = _repository.GetOrThrow(stock);
            if (data == null) return;

            Array.Copy(newTickPro, data.Reflection.ProgramK, Math.Min(newTickPro.Length, data.Reflection.ProgramK.Length));
        }

        public double GetTotalScore(string stock)
        {
            var data = _repository.GetOrThrow(stock);
            return data?.Score.총점 ?? 0.0;
        }

        public void AddIfMissing(IEnumerable<string> stocks)
        {
            foreach (string stock in stocks)
            {
                if (!_repository.Contains(stock))
                    TotalStockList.Add(stock);
            }
        }

        public void UpdateRankingByTotalScore()
        {
            var all = _repository.AllDatas;
            RankingList = RankLogic
                .RankByTotalScore(all)
                .Select(s => s.Stock)
                .ToList();
        }

        // ✅ Right-click zone 4 toggle
        public void ToggleInterestedOnly(string stock)
        {
            if (InterestedOnlyList.Contains(stock))
                InterestedOnlyList.Remove(stock);
            else
            {
                InterestedOnlyList.Add(stock);
                InterestedWithBidList.Remove(stock); // mutually exclusive
            }
        }

        // ✅ Right-click zone 5 toggle
        public void ToggleInterestedWithBid(string stock)
        {
            if (InterestedWithBidList.Contains(stock))
                InterestedWithBidList.Remove(stock);
            else
            {
                InterestedWithBidList.Add(stock);
                InterestedOnlyList.Remove(stock); // mutually exclusive
            }
        }

        public void ClearInterest()
        {
            InterestedOnlyList.Clear();
            InterestedWithBidList.Clear();
        }
    }
}
