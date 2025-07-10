using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static New_Tradegy.Library.g;

namespace New_Tradegy.Library.Models
{
    public class StockData
    {
        public string Stock { get; set; }
        public string Code { get; set; }

        public PricePassData Pass { get; set; } = new PricePassData();
        public ScoreData Score { get; set; } = new ScoreData();
        public StatisticsData Statistics { get; set; } = new StatisticsData();
        public LevelData Level { get; set; } = new LevelData();
        public ApiData Api { get; set; } = new ApiData();
        public PostData Post { get; set; } = new PostData();
        public DealStatus Deal { get; set; } = new DealStatus();
        public MiscData Misc { get; set; } = new MiscData();

        public StockData Clone()
        {
            return new StockData
            {
                Stock = this.Stock,
                Code = this.Code,
                Pass = this.Pass?.Clone(),
                Score = this.Score?.Clone(),
                Statistics = this.Statistics?.Clone(),
                Level = this.Level?.Clone(),
                Api = this.Api?.Clone(),
                Post = this.Post?.Clone(),
                Deal = this.Deal?.Clone(),
                Misc = this.Misc?.Clone()
            };
        }
    }

    

    // public PricePassData Pass { get; set; } = new PricePassData();
    public class PricePassData
    {
        public int upperPassingPrice = 0;
        public int lowerPassingPrice = 0;

        public int PreviousPriceHigh = int.MinValue;
        public int? PreviousPriceLow = null;
        public int PriceStatus = 0;

        public int PreviousProgramHigh = int.MinValue;
        public int? PreviousProgramLow = null;
        public int ProgramStatus = 0;

        public int MonthStatus, QuarterStatus, HalfStatus, YearStatus;
        public int Month, Quarter, Half, Year;

        public PricePassData Clone()
        {
            return new PricePassData
            {
                upperPassingPrice = this.upperPassingPrice,
                lowerPassingPrice = this.lowerPassingPrice,

                PreviousPriceHigh = this.PreviousPriceHigh,
                PreviousPriceLow = this.PreviousPriceLow,
                PriceStatus = this.PriceStatus,

                PreviousProgramHigh = this.PreviousProgramHigh,
                PreviousProgramLow = this.PreviousProgramLow,
                ProgramStatus = this.ProgramStatus,

                MonthStatus = this.MonthStatus,
                QuarterStatus = this.QuarterStatus,
                HalfStatus = this.HalfStatus,
                YearStatus = this.YearStatus,

                Month = this.Month,
                Quarter = this.Quarter,
                Half = this.Half,
                Year = this.Year
            };
        }
    }



    // public ScoreData Score { get; set; } = new ScoreData();
    public class ScoreData
    {
        public int 피올_등수;
        public int 닥올_등수;

        public int 피누_등수; public double 피누;
        public int 종누_등수; public double 종누;

        public int 푀분_등수; public double 푀분;
        public int 등합_등수; public double 등합;

        public int 배차_등수; public double 배차;
        public int 배합_등수; public double 배합;

        public int 상순_등수; public double 상순;
        public int 저순_등수; public double 저순;

        public int 편차_등수; public double 편차;
        public int 평균_등수; public double 평균;

        public int 피로_등수; public double 피로;

        public int 그룹_등수;

        public double 급락;
        public double 급상;

        public ScoreData Clone()
        {
            return new ScoreData
            {
                피올_등수 = this.피올_등수,
                닥올_등수 = this.닥올_등수,

                피누_등수 = this.피누_등수,
                피누 = this.피누,
                종누_등수 = this.종누_등수,
                종누 = this.종누,

                푀분_등수 = this.푀분_등수,
                푀분 = this.푀분,
                등합_등수 = this.등합_등수,
                등합 = this.등합,

                배차_등수 = this.배차_등수,
                배차 = this.배차,
                배합_등수 = this.배합_등수,
                배합 = this.배합,

                상순_등수 = this.상순_등수,
                상순 = this.상순,
                저순_등수 = this.저순_등수,
                저순 = this.저순,

                편차_등수 = this.편차_등수,
                편차 = this.편차,
                평균_등수 = this.평균_등수,
                평균 = this.평균,

                피로_등수 = this.피로_등수,
                피로 = this.피로,

                그룹_등수 = this.그룹_등수,

                급락 = this.급락,
                급상 = this.급상
            };
        }
    }



    // public StatData Stat { get; set; } = new StatData();
    public class StatisticsData
    {
        public int 피분_count;

        public double 푀분_avr, 푀분_dev;
        public double 거분_avr, 거분_dev;
        public double 배차_avr, 배차_dev;
        public double 배합_avr, 배합_dev;
        public double 피누_avr, 피누_dev;
        public double 종누_avr, 종누_dev;

        public char 시장구분; // P 코스피, D 코스닥
        public double 시총;

        public string 일간변동평균편차;
        public double 일간변동평균;
        public double 일간변동편차;

        public int 일최대거래액;
        public int 일최저거래액;
        public int 일평균거래액;
        public ulong 일평균거래량;

        public StatisticsData Clone()
        {
            return new StatisticsData
            {
                피분_count = this.피분_count,

                푀분_avr = this.푀분_avr,
                푀분_dev = this.푀분_dev,
                거분_avr = this.거분_avr,
                거분_dev = this.거분_dev,
                배차_avr = this.배차_avr,
                배차_dev = this.배차_dev,
                배합_avr = this.배합_avr,
                배합_dev = this.배합_dev,
                피누_avr = this.피누_avr,
                피누_dev = this.피누_dev,
                종누_avr = this.종누_avr,
                종누_dev = this.종누_dev,

                시장구분 = this.시장구분,
                시총 = this.시총,

                일간변동평균편차 = this.일간변동평균편차,
                일간변동평균 = this.일간변동평균,
                일간변동편차 = this.일간변동편차,

                일최대거래액 = this.일최대거래액,
                일최저거래액 = this.일최저거래액,
                일평균거래액 = this.일평균거래액,
                일평균거래량 = this.일평균거래량
            };
        }
    }


    // public LevelData Level { get; set; } = new LevelData();
    public class LevelData
    {
        public double 프퍼, 푀퍼, 급락, 잔잔;

        public LevelData Clone()
        {
            return new LevelData
            {
                프퍼 = this.프퍼,
                푀퍼 = this.푀퍼,
                급락 = this.급락,
                잔잔 = this.잔잔
            };
        }
    }



    // public ApiData Api { get; set; } = new ApiData();
    public class ApiData
    {
        public long 현재가; //4 long
        public long 시초가; //5 long  
        public int 시초;
        public long 전고가; //6 long

        public long 전저가; //7 long
        public int 전저;

        public long 매수1호가;
        public long 매도1호가; // 8 long public long 매수1호가; // 9 long

        public ulong 거래량; // 10 ulong

        public double 전일거래액_천만원; // marketeye not provide, calculated from "일"

        public char 장구분; // 12 char '0' 장전 '1' 동시호가 '2' 장중
        public ulong 총매도호가잔량; //13 ulong
        public ulong 총매수호가잔량; //14 ulong
        public int 최우선매도호가잔량; //15 (ulong) converted to int
        public int 최우선매수호가잔량; //16 (ulong) converted to int

        public long 전일종가; //23 long

        public long 예상체결가; //28 long // not down
        public ulong 예상체결수량; //31 ulong // not down

        public char 시간외단일대비부호; //36 char +, - // not down
        public long 시간외단일전일대비; //37 long, 36 필히 하여야 함 // not down
        public long 시간외단일현재가; //38 long // not down
        public ulong 시간외단일거래대금; //45 ulonglong // not down

        public long 당일프로그램순매수량; // 116 long
        public long 당일외인순매수량; //118 long

        public long 당일기관순매수량; //120 long

        public ulong 공매도수량; //127 ulong

        public int 가격;
        public int 수급;
        public double 체강;

        public int nrow = 0;
        public int[,] x = new int[382, 12];

        // 틱 데이터
        public long[] 틱의시간 = new long[MajorIndex.TickArraySize]; // 틱의시간    // 호가창 tT
        public int[] 틱의가격 = new int[MajorIndex.TickArraySize]; // 틱의가격    // 호가창
        public int[] 틱의수급 = new int[MajorIndex.TickArraySize]; // 틱의수급
        public int[] 틱의체강 = new int[MajorIndex.TickArraySize]; // 틱의체강

        public int[] 틱매수량 = new int[MajorIndex.TickArraySize]; // 틱매수량
        public int[] 틱매도량 = new int[MajorIndex.TickArraySize]; // 틱매도량
        public int[] 틱매수배 = new int[MajorIndex.TickArraySize]; // 틱매수배
        public int[] 틱매도배 = new int[MajorIndex.TickArraySize]; // 틱매도배

        public int[] 틱배수차 = new int[MajorIndex.TickArraySize];  // 틱배수차
        public int[] 틱배수합 = new int[MajorIndex.TickArraySize];  // 틱배수합
        public int[] 틱프외퍼 = new int[MajorIndex.TickArraySize];  // 틱프외퍼

        public int[] 틱프로량 = new int[MajorIndex.TickArraySize]; // 틱프로량  
        public double[] 틱프로천 = new double[MajorIndex.TickArraySize]; // 틱프돈천

        public int[] 틱외인량 = new int[MajorIndex.TickArraySize]; // 틱외인량
        public double[] 틱외인천 = new double[MajorIndex.TickArraySize]; // 틱외돈천                  //

        public double[] 틱거래천 = new double[MajorIndex.TickArraySize]; // 틱거돈천

        public int[] 틱매도잔 = new int[MajorIndex.TickArraySize]; // 최우선매도호가잔량
        public int[] 틱매수잔 = new int[MajorIndex.TickArraySize]; // 최우선매수호가잔량

        // 분 데이터
        public double[] 분프로천 = new double[MajorIndex.MinuteArraySize]; // 분프로천
        public double[] 분외인천 = new double[MajorIndex.MinuteArraySize]; // 분외인천
        public double[] 분거래천 = new double[MajorIndex.MinuteArraySize]; // 분거래천
        public int[] 분매수배 = new int[MajorIndex.MinuteArraySize]; // 분매수배
        public int[] 분매도배 = new int[MajorIndex.MinuteArraySize]; // 분매도배
        public int[] 분배수차 = new int[MajorIndex.MinuteArraySize];  // 분배수차
        public int[] 분배수합 = new int[MajorIndex.MinuteArraySize];  // 분배수차

        public int 매수배;
        public int 매도배;


        public void AppendTick(int[] t, int HHmmssfff, double 현누적매수체결거래량, double 현누적매도체결거래량,
            double 틱매수체결배수, double 틱매도체결배수, double multipleFactor, double moneyFactor)
        {
            for (int i = MajorIndex.TickArraySize - 1; i >= 1; i--)
            {
                틱의시간[i] = 틱의시간[i - 1];
                틱의가격[i] = 틱의가격[i - 1];
                틱의수급[i] = 틱의수급[i - 1];
                틱의체강[i] = 틱의체강[i - 1];
                틱매수량[i] = 틱매수량[i - 1];
                틱매도량[i] = 틱매도량[i - 1];
                틱매수배[i] = 틱매수배[i - 1];
                틱매도배[i] = 틱매도배[i - 1];

                틱배수차[i] = 틱배수차[i - 1];
                틱배수합[i] = 틱배수합[i - 1];

                틱프로량[i] = 틱프로량[i - 1];
                틱프로천[i] = 틱프로천[i - 1];
                틱외인량[i] = 틱외인량[i - 1];
                틱외인천[i] = 틱외인천[i - 1];
                틱거래천[i] = 틱거래천[i - 1];
                틱프외퍼[i] = 틱프외퍼[i - 1];

                틱매도잔[i] = 틱매도잔[i - 1];
                틱매수잔[i] = 틱매수잔[i - 1];
            }

            틱의시간[0] = HHmmssfff;
            틱의가격[0] = 가격;
            틱의수급[0] = 수급;
            틱의체강[0] = (int)(체강 * g.HUNDRED);
            틱매수량[0] = (int)현누적매수체결거래량;
            틱매도량[0] = (int)현누적매도체결거래량;

            틱매수배[0] = (int)틱매수체결배수; //??
            틱매도배[0] = (int)틱매도체결배수;
            틱배수차[0] = 틱매수배[0] - 틱매도배[1];
            틱배수합[0] = 틱매수배[0] + 틱매도배[1];

            틱프로량[0] = t[4];
            틱프로천[0] = (int)((틱프로량[0] - 틱프로량[1]) * moneyFactor);
            틱외인량[0] = t[5];
            틱외인천[0] = (int)((틱외인량[0] - 틱외인량[1]) * moneyFactor);
            틱거래천[0] = (int)((틱매수량[0] - 틱매수량[1] + 틱매도량[0] - 틱매도량[1]) * moneyFactor);

            if (틱거래천[0] > 0)
                틱프외퍼[0] = (int)((틱프로천[0] + 틱외인천[0]) / 틱거래천[0] * 100);

            틱매도잔[0] = 최우선매도호가잔량;
            틱매수잔[0] = 최우선매수호가잔량;
        }

        public void AppendMinuteIfNeeded(bool append)
        {
            if (!append) return;

            for (int i = MajorIndex.MinuteArraySize - 1; i >= 2; i--)
            {
                분프로천[i] = 분프로천[i - 1];
                분외인천[i] = 분외인천[i - 1];
                분거래천[i] = 분거래천[i - 1];

                분매수배[i] = 분매수배[i - 1];
                분매도배[i] = 분매도배[i - 1];

                분배수차[i] = 분배수차[i - 1];
                분배수합[i] = 분배수합[i - 1];
            }

            if (nrow - 3 >= 0)
            {
                분프로천[1] = (int)((x[nrow - 2, 4] - x[nrow - 3, 4]) * 전일종가 / g.천만원);
                분외인천[1] = (int)((x[nrow - 2, 5] - x[nrow - 3, 5]) * 전일종가 / g.천만원);
                분거래천[1] = (int)((x[nrow - 2, 7] - x[nrow - 3, 7]) * 전일종가 / g.천만원);
            }
            if (nrow - 2 >= 0)
            {
                분매수배[1] = x[nrow - 2, 8];
                분매도배[1] = x[nrow - 2, 9];
            }
            분배수차[1] = 분매수배[1] - 분매도배[1];
            분배수합[1] = 분매수배[1] + 분매도배[1];
        }



        public ApiData Clone()
        {
            return new ApiData
            {
                현재가 = this.현재가,
                시초가 = this.시초가,
                시초 = this.시초,
                전고가 = this.전고가,
                전저가 = this.전저가,
                전저 = this.전저,

                매수1호가 = this.매수1호가,
                매도1호가 = this.매도1호가,
                거래량 = this.거래량,

                전일거래액_천만원 = this.전일거래액_천만원,
                장구분 = this.장구분,
                총매도호가잔량 = this.총매도호가잔량,
                총매수호가잔량 = this.총매수호가잔량,
                최우선매도호가잔량 = this.최우선매도호가잔량,
                최우선매수호가잔량 = this.최우선매수호가잔량,

                전일종가 = this.전일종가,
                예상체결가 = this.예상체결가,
                예상체결수량 = this.예상체결수량,

                시간외단일대비부호 = this.시간외단일대비부호,
                시간외단일전일대비 = this.시간외단일전일대비,
                시간외단일현재가 = this.시간외단일현재가,
                시간외단일거래대금 = this.시간외단일거래대금,

                당일프로그램순매수량 = this.당일프로그램순매수량,
                당일외인순매수량 = this.당일외인순매수량,
                당일기관순매수량 = this.당일기관순매수량,
                공매도수량 = this.공매도수량,

                가격 = this.가격,
                수급 = this.수급,
                체강 = this.체강,

                nrow = this.nrow,
                x = (int[,])this.x.Clone(),  // 2D array clone

                틱의시간 = (long[])this.틱의시간.Clone(),
                틱의가격 = (int[])this.틱의가격.Clone(),
                틱의수급 = (int[])this.틱의수급.Clone(),
                틱의체강 = (int[])this.틱의체강.Clone(),

                틱매수량 = (int[])this.틱매수량.Clone(),
                틱매도량 = (int[])this.틱매도량.Clone(),
                틱매수배 = (int[])this.틱매수배.Clone(),
                틱매도배 = (int[])this.틱매도배.Clone(),

                틱배수차 = (int[])this.틱배수차.Clone(),
                틱배수합 = (int[])this.틱배수합.Clone(),
                틱프외퍼 = (int[])this.틱프외퍼.Clone(),

                틱프로량 = (int[])this.틱프로량.Clone(),
                틱프로천 = (double[])this.틱프로천.Clone(),
                틱외인량 = (int[])this.틱외인량.Clone(),
                틱외인천 = (double[])this.틱외인천.Clone(),
                틱거래천 = (double[])this.틱거래천.Clone(),

                틱매도잔 = (int[])this.틱매도잔.Clone(),
                틱매수잔 = (int[])this.틱매수잔.Clone(),

                분프로천 = (double[])this.분프로천.Clone(),
                분외인천 = (double[])this.분외인천.Clone(),
                분거래천 = (double[])this.분거래천.Clone(),

                분매수배 = (int[])this.분매수배.Clone(),
                분매도배 = (int[])this.분매도배.Clone(),
                분배수차 = (int[])this.분배수차.Clone(),
                분배수합 = (int[])this.분배수합.Clone(),

                매수배 = this.매수배,
                매도배 = this.매도배
            };
        }

    }

    // public DealStatus Deal { get; set; } = new DealStatus();
    public class DealStatus
    {
        public int 보유량;
        public double 수익률;
        public double 전수익률;
        public double 장부가;

        public long 평가금액; // Error if changed to int
        public long 손익단가; // Error if changed to int

        public DealStatus Clone()
        {
            return new DealStatus
            {
                보유량 = this.보유량,
                수익률 = this.수익률,
                전수익률 = this.전수익률,
                장부가 = this.장부가,
                평가금액 = this.평가금액,
                손익단가 = this.손익단가
            };
        }
    }


    // public PostData Post { get; set; } = new PostData();
    public class PostData
    {
        public int 매수호가거래액_백만원;  // ok
        public int 매도호가거래액_백만원;  // ok
        public int 분당가격차;             // ok

        public double 푀누천;
        public double 외누천;
        public double 기누천;
        public double 거누천;
        public double 종거천;

        public PostData Clone()
        {
            return new PostData
            {
                매수호가거래액_백만원 = this.매수호가거래액_백만원,
                매도호가거래액_백만원 = this.매도호가거래액_백만원,
                분당가격차 = this.분당가격차,

                푀누천 = this.푀누천,
                외누천 = this.외누천,
                기누천 = this.기누천,
                거누천 = this.거누천,
                종거천 = this.종거천
            };
        }
    }


    // public MiscData Misc { get; set; } = new MiscData();
    public class MiscData
    {
        public double 수급과장배수 = 1;
        public int oGL_sequence_id;
        public bool ShrinkDraw = false;

        public List<string> Friends { get; set; } = new List<string>();

        public MiscData Clone()
        {
            return new MiscData
            {
                수급과장배수 = this.수급과장배수,
                oGL_sequence_id = this.oGL_sequence_id,
                ShrinkDraw = this.ShrinkDraw,
                Friends = new List<string>(this.Friends)  // Deep copy of list
            };
        }
    }

}