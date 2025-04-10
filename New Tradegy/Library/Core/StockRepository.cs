using System.Collections.Generic;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Core
{
    public class StockRepository
    {
        private static StockRepository _instance = null;
        private static readonly object _lock = new object();

        private StockRepository() { }
        // have to use instance var r = StockRepository.Instance; // ✅ Safe and controlled

        public static StockRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new StockRepository();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, StockData> _datas = new Dictionary<string, StockData>();


        public void Initialize(IEnumerable<string> stocks)
        {
            foreach (var stock in stocks)
            {
                _datas[stock] = new StockData { Stock = stock };
            }
        }

        public void AddOrUpdate(string stock, StockData data)
        {
            _datas[stock] = data;
        }

        public StockData GetOrThrow(string stock)
        {
            if (_datas.TryGetValue(stock, out var data))
                return data;

            throw new KeyNotFoundException($"Stock code '{stock}' not found.");
        }

        public IEnumerable<StockData> AllDatas => _datas.Values;

        public bool Contains(string stock)
        {
            return _datas.ContainsKey(stock);
        }

    }
}
