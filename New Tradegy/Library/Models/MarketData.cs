using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Models
{
    public sealed class MarketData
    {
        private static readonly Lazy<MarketData> _instance = new Lazy<MarketData>(() => new MarketData());
        public static MarketData Instance => _instance.Value;

        private MarketData() { }

        public const int TickArraySize = 60;
        public const int MinuteArraySize = 15;

        // KOSPI
        public long KospiBuyTotal { get; set; }
        public long KospiSellTotal { get; set; }
        public long KospiProgramNetBuy { get; set; }
        public long KospiRetailNetBuy { get; set; }
        public long KospiForeignNetBuy { get; set; }
        public long KospiInstitutionNetBuy { get; set; }
        public long KospiInvestmentNetBuy { get; set; }
        public long KospiPensionNetBuy { get; set; }
        public double[] KospiTickBuyRatio { get; } = new double[TickArraySize];
        public double[] KospiTickSellRatio { get; } = new double[TickArraySize];

        // KOSDAQ
        public long KosdaqBuyTotal { get; set; }
        public long KosdaqSellTotal { get; set; }
        public long KosdaqProgramNetBuy { get; set; }
        public long KosdaqRetailNetBuy { get; set; }
        public long KosdaqForeignNetBuy { get; set; }
        public long KosdaqInstitutionNetBuy { get; set; }
        public long KosdaqInvestmentNetBuy { get; set; }
        public long KosdaqPensionNetBuy { get; set; }
        public double[] KosdaqTickBuyRatio { get; } = new double[TickArraySize];
        public double[] KosdaqTickSellRatio { get; } = new double[TickArraySize];

        // Indexes
        public int KospiIndex { get; set; }
        public int KosdaqIndex { get; set; }
        public float ShanghaiIndex { get; set; }
        public float HangSengIndex { get; set; }
        public float NikkeiIndex { get; set; }
        public float Snp500Index { get; set; }
        public float NasdaqIndex { get; set; }
    }
}
