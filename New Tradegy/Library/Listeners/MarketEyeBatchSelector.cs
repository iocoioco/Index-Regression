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
            List<string> indexList,               // 1. Index stocks (always included)
            List<string> holding,                 // 2. Holding stocks
            List<string> interestedWithBid,       // 3. 관심 종목 (with bids)
            List<string> interestedOnly,          // 4. 관심 종목 (no bids)
            List<string> rankedStockList,         // 5. Ranked list to fill up to 100
            int batchSize = 200)
        {
            HashSet<string> selected = new HashSet<string>();

            // 🔽 Define IsValid inside so it can access 'selected'
            bool IsValid(string s) => s != "" && !selected.Contains(s);

            // ✅ 1. Add index stocks (no duplicates)
            foreach (var s in indexList)
            {
                if (IsValid(s)) selected.Add(s);
            }

            // ✅ 2. Add from holding up to 100
            foreach (var s in holding)
            {
                if (selected.Count >= 100) break;
                if (IsValid(s)) selected.Add(s);
            }

            // ✅ 3. Add from interestedWithBid up to 100
            foreach (var s in interestedWithBid)
            {
                if (selected.Count >= 100) break;
                if (IsValid(s)) selected.Add(s);
            }

            // ✅ 4. Add from interestedOnly up to 100
            foreach (var s in interestedOnly)
            {
                if (selected.Count >= 100) break;
                if (IsValid(s)) selected.Add(s);
            }

            // ✅ 5. Fill up to 100 with ranked stocks
            foreach (var s in rankedStockList)
            {
                if (selected.Count >= 100) break;
                if (IsValid(s)) selected.Add(s);
            }

            // ✅ 6. Fill remaining from AllGeneralDatas using rotating offset
            var repoAllGeneralStockList = g.StockRepository.AllGeneralDatas.Select(x => x.Stock).ToList();

            int remaining = batchSize - selected.Count;
            int i = 0;
            while (selected.Count < batchSize && repoAllGeneralStockList.Count > 0 && i < repoAllGeneralStockList.Count)
            {
                string s = repoAllGeneralStockList[(repositoryOffset + i) % repoAllGeneralStockList.Count];
                if (IsValid(s)) selected.Add(s);
                i++;
            }

            repositoryOffset = (repositoryOffset + remaining) % repoAllGeneralStockList.Count;

            return selected.Take(batchSize).ToList();  // Trim in case it's slightly over 200 due to race condition
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