using System.Collections.Generic;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Core
{
    public class StockRepository
    {
        private Dictionary<string, StockData> _data = new Dictionary<string, StockData>();

        public void AddOrUpdate(string stock, StockData data)
        {
            _data[stock] = data;
        }

        public StockData Get(string stock)
        {
            return _data.TryGetValue(stock, out var result) ? result : null;
        }

        public bool Contains(string stock)
        {
            return _data.ContainsKey(stock);
        }

        public IEnumerable<StockData> GetAll()
        {
            return _data.Values;
        }
    }
}
