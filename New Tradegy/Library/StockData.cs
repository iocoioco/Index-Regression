using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library
{
    public class StockData
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public TickReflection TickTrend { get; set; } = new TickReflection();
        public PricePassData Pass { get; set; } = new PricePassData();
        public DealData Deal { get; set; } = new DealData();
        public ScoreData Score { get; set; } = new ScoreData();
        public StatData Stat { get; set; } = new StatData();
        public LevelData Level { get; set; } = new LevelData();

        public List<string> Friends { get; set; } = new List<string>();

        public MarketSnapshot Market { get; set; } = new MarketSnapshot();
        public TickData TickData { get; set; } = new TickData();
        public MinuteData Minute { get; set; } = new MinuteData();
        public DailyData Daily { get; set; } = new DailyData();

        public MiscData Misc { get; set; } = new MiscData();

        public int[,] XMatrix { get; set; } = new int[382, 12]; // use MAX_ROW
    }

    // Models/TickSnapshot.cs
    public class TickReflection
    {
        public int ArrayCount { get; set; } = 0;
        public double ProvisionalProgramSumK { get; set; } = 0.0;
        public double ProvisionalForeignSumK { get; set; } = 0.0;

        public double[] ProgramK = new double[MarketData.TickArraySize];
        public double[] ForeignK = new double[MarketData.TickArraySize];
        public int[] Prices = new int[MarketData.TickArraySize];
        public int[] Times = new int[MarketData.TickArraySize];
    }

    // Models/PricePassData.cs
    public class PricePassData
    {
        //public int PreviousPriceHigh = int.MinValue;
        //public int? PreviousPriceLow = null;
        //public int PriceStatus = 0;

        //public int PreviousProgramHigh = int.MinValue;
        //public int? PreviousProgramLow = null;
        //public int ProgramStatus = 0;

        //public int MonthStatus, QuarterStatus, HalfStatus, YearStatus;
        //public int Month, Quarter, Half, Year;
    }

    // Models/DealData.cs
    public class DealData
    {
        //public int UpperPassingPrice = 0;
        //public int LowerPassingPrice = 0;
    }

    // Models/ScoreData.cs
    public class ScoreData
    {
        //public double 푀분;
        //public double 거분;
        //public double 배차, 배합;
        //public double 급락, 급상;
        public double 총점;
        //public int 푀분_등수, 거분_등수;
        //public int 배차_등수, 배합_등수;
        //public int 그순;
        //public double 등합;
    }

    // Models/StatData.cs
    public class StatData
    {
        //public int 프분_count;
        //public double 프분_avr, 프분_dev;
        //public double 거분_avr, 거분_dev;
        //public double 배차_avr, 배차_dev;
        //public double 배합_avr, 배합_dev;
    }

    // Models/LevelData.cs
    public class LevelData
    {
        //public double 프퍼, 푀퍼, 급락, 잔잔;
    }

    // Models/MarketSnapshot.cs
    public class MarketSnapshot
    {
        // Add basic OHLC, volume, etc. here later
    }

    // Models/TickData.cs
    public class TickData
    {
        public double[] 틱프로천 = new double[MarketData.TickArraySize]; // 틱프돈천
    }

    // Models/MinuteData.cs
    public class MinuteData
    {
        // Add Minute-level arrays here later
    }

    // Models/DailyData.cs
    public class DailyData
    {
        // Add Daily summary data here later
    }

    // Models/MiscData.cs
    public class MiscData
    {
        // Add 수익률, 장부가, 시장구분, etc. here later
    }

}