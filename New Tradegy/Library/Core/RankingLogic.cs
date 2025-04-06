using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Core
{
    public static class RankingLogic
    {
        public static List<StockData> RankByTotalScore(IEnumerable<StockData> stocks)
        {
            return stocks
                .OrderByDescending(s => s.Score.총점)
                .Take(50)
                .ToList();
        }

        public static List<StockData> RankByProgramAmount(IEnumerable<StockData> stocks)
        {
            return stocks
                .OrderByDescending(s => s.TickData.틱프로천.Sum())
                .Take(50)
                .ToList();
        }

        // Add more strategies as needed
    }
