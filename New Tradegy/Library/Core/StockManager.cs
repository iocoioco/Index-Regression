using New_Tradegy.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using New_Tradegy.Library.Core;

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

        public void UpdateTick(string code, double[] newTickPro)
        {
            var data = _repository.Get(code);
            if (data == null) return;

            Array.Copy(newTickPro, data.Tick.ProK, Math.Min(newTickPro.Length, data.Tick.TickProK.Length));
        }

        public double GetTotalScore(string code)
        {
            var data = _repository.Get(code);
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
            var all = _repository.GetAll();
            RankingList = RankingLogic
                .RankByTotalScore(all)
                .Select(s => s.Code)
                .ToList();
        }
    }

}
