using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy
{

        public static class StockManager
        {
            public static List<string> TotalStockList = new List<string>();

            public static void AddIfMissing(IEnumerable<string> stocks)
            {
                foreach (string stock in stocks)
                {
                    if (!TotalStockList.Contains(stock))
                        TotalStockList.Add(stock);
                }
            }
        }
    
}
