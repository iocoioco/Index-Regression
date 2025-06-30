using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.Core
{
    public class StockRepository
    {
        private static StockRepository _instance = null;
        private static readonly object _lock = new object();

        private StockRepository() { }
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
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

        public void AddOrUpdate(string stock, StockData data)
        {
            _stockMap[stock] = data;
            if (data != null && !string.IsNullOrEmpty(data.Stock))
                data.Code = _cpstockcode.NameToCode(data.Stock);
        }

        public bool TryGet(string stock, out StockData data)
        {
            return _stockMap.TryGetValue(stock, out data);
        }

        public StockData TryGetStockOrNull(string stock)
        {
            if (stock == null)
                return null;
            if (_stockMap.TryGetValue(stock, out var data))
                return data;

            // MessageBox.Show("Stock name not found: " + stock);

            return null;
        }

        // public IEnumerable<string> AllStockNames => _stockMap.Keys;
        // public IEnumerable<(string Name, StockData Data)> AllPairs => stockMap.Select(kvp => (kvp.Key, kvp.Value));
        public IEnumerable<StockData> AllDatas => _stockMap.Values; // datas in sequence

        public IEnumerable<StockData> AllGeneralDatas =>
            g.StockRepository.AllDatas
            .Where(data => !g.StockManager.IndexList.Contains(data.Stock));

        public bool Contains(string stock)
        {
            return _stockMap.ContainsKey(stock);
        }
    }
}
