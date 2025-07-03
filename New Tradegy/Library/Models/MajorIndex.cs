using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Models
{
    public sealed class MajorIndex
    {
        private static readonly Lazy<MajorIndex> _instance = new Lazy<MajorIndex>(() => new MajorIndex());
        public static MajorIndex Instance => _instance.Value;

        private MajorIndex() { }

        public const int TickArraySize = 60; // const static implicitly
        public const int MinuteArraySize = 15; // const static implicitly

        // KOSPI
        public double KospiBuyPower { get; set; }
        public double KospiSellPower { get; set; }
        public long KospiProgramNetBuy { get; set; }
        public long KospiRetailNetBuy { get; set; }
        public long KospiForeignNetBuy { get; set; }
        public long KospiInstitutionNetBuy { get; set; }
        public long KospiInvestmentNetBuy { get; set; } // 금투
        public long KospiPensionNetBuy { get; set; }
        public double[] KospiTickBuyPower { get; } = new double[TickArraySize];
        public double[] KospiTickSellPower { get; } = new double[TickArraySize];

        // KOSDAQ
        public double KosdaqBuyPower { get; set; }
        public double KosdaqSellPower { get; set; }
        public long KosdaqProgramNetBuy { get; set; }
        public long KosdaqRetailNetBuy { get; set; }
        public long KosdaqForeignNetBuy { get; set; }
        public long KosdaqInstitutionNetBuy { get; set; }
        public long KosdaqInvestmentNetBuy { get; set; } // 금투
        public long KosdaqPensionNetBuy { get; set; }
        public double[] KosdaqTickBuyPower { get; } = new double[TickArraySize];
        public double[] KosdaqTickSellPower { get; } = new double[TickArraySize];

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
