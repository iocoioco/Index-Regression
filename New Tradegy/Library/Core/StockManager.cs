using System;
using System.Collections.Generic;
using System.Linq;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Core;
using New_Tradegy.Library;

namespace New_Tradegy
{
    public class StockManager
    {
        private readonly StockRepository _repository;

        public List<string> TotalStockList { get; } = new List<string>();
        public List<string> RankingList { get; private set; } = new List<string>();
        public List<string> HoldingList { get; private set; } = new List<string>();
        public List<string> InterestedList { get; private set; } = new List<string>();

        public Dictionary<string, List<string>> Groups { get; private set; } = new Dictionary<string, List<string>>();

        public StockManager(StockRepository repository)
        {
            _repository = repository;
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
    }
}
