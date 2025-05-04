using System.Collections.Generic;
using System.Windows.Forms;
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

        private Dictionary<string, StockData> _stockMap = new Dictionary<string, StockData>();


        public void Initialize(IEnumerable<string> stocks)
        {
            foreach (var stock in stocks)
            {
                _stockMap[stock] = new StockData { Stock = stock };
            }
        }

        public void AddOrUpdate(string stock, StockData data)
        {
            _stockMap[stock] = data;
        }

        public bool TryGet(string stockName, out StockData stock)
        {
            return _stockMap.TryGetValue(stockName, out stock);
        }

        public StockData TryGetStockOrNull(string stock)
        {
            if (_stockMap.TryGetValue(stock, out var data))
                return data;

            MessageBox.Show("Stock name not found: " + stock);
            return null;
        }

        public IEnumerable<StockData> AllDatas => _stockMap.Values;

        public bool Contains(string stock)
        {
            return _stockMap.ContainsKey(stock);
        }

        public void Add(string stockName, StockData data)
        {
            _stockMap[stockName] = data;
        }
    }
}
