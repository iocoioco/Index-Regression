using System.Collections.Generic;
using System.Linq;
using New_Tradegy.Library.Core;
using New_Tradegy;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Listeners
{
    public static class MarketEyeBatchSelector
    {
        private static int repositoryOffset = 0;

        public static List<string> Select200Batch(
            List<string> leverage,             // 2 items
            List<string> holding,              // 보유종목 (with bids)
            List<string> interestedWithBid,    // 관심 종목 (with bids)
            List<string> interestedOnly,       // 관심 종목 (no bids)
            int fixedCount = 100,
            int batchSize = 200
        )
        {
            HashSet<string> selected = new HashSet<string>();

            // ✅ 1. Add leverage first
            foreach (var s in leverage)
                selected.Add(s);

            // ✅ 2. Fill fixed part (up to 100 stocks)
            foreach (var s in holding)
            {
                if (selected.Count >= fixedCount) break;
                selected.Add(s);
            }

            foreach (var s in interestedWithBid)
            {
                if (selected.Count >= fixedCount) break;
                selected.Add(s);
            }

            foreach (var s in interestedOnly)
            {
                if (selected.Count >= fixedCount) break;
                selected.Add(s);
            }

            // ✅ 3. Fill the rest from repository sequentially
            var repoList = g.StockRepository.AllDatas
                .Select(x => x.Stock)
                .ToList();

            int remaining = batchSize - selected.Count;
            for (int i = 0; i < remaining && repoList.Count > 0; i++)
            {
                string stock = repoList[(repositoryOffset + i) % repoList.Count];
                selected.Add(stock);
            }

            repositoryOffset = (repositoryOffset + remaining) % repoList.Count;

            return selected.Take(batchSize).ToList();
        }
    }
}


// usage
//var selectedStocks = BatchSelector.Select200Batch(
//    leverage: stockManager.LeverageList,
//holding: stockManager.HoldingList,
//    interestedWithBid: stockManager.InterestedWithBidList,
//    interestedOnly: stockManager.InterestedOnlyList
//);