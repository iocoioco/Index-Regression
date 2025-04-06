using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Core
{
    public class StockRepository
    {
        private Dictionary<string, StockData> _data = new Dictionary<string, StockData>();

        public void AddOrUpdate(string code, StockData data)
        {
            _data[code] = data;
        }

        public StockData Get(string code)
        {
            return _data.TryGetValue(code, out var result) ? result : null;
        }

        public bool Contains(string code)
        {
            return _data.ContainsKey(code);
        }

        // ✅ Add this method:
        public IEnumerable<StockData> GetAll()
        {
            return _data.Values;
        }
    }


}
