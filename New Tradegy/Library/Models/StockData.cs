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

        public TickReflection Reflection { get; set; } = new TickReflection();
        public PricePassData Pass { get; set; } = new PricePassData();
        public ScoreData Score { get; set; } = new ScoreData();
        public StatisticsData Statistics { get; set; } = new StatisticsData();
        public LevelData Level { get; set; } = new LevelData();
        public ApiData Api { get; set; } = new ApiData();
        public PostData Post { get; set; } = new PostData();
        public DealStatus Deal { get; set; } = new DealStatus();
        public MiscData Misc { get; set; } = new MiscData();

       
    }


    public class TickReflection
    {
        public int ArrayCount { get; set; } = 0;
        public double ProvisionalProgramSumK { get; set; } = 0.0;
        public double ProvisionalForeignSumK { get; set; } = 0.0;

        public double[] ProgramK = new double[MajorIndex.TickArraySize];
        public double[] ForeignK = new double[MajorIndex.TickArraySize];
        public int[] Prices = new int[MajorIndex.TickArraySize];
        public int[] Times = new int[MajorIndex.TickArraySize];
    }

    // public PricePassData Pass { get; set; } = new PricePassData();
    public class PricePassData
    {
        public int PreviousPriceHigh = int.MinValue;
        public int? PreviousPriceLow = null;
        public int PriceStatus = 0;

        public int PreviousProgramHigh = int.MinValue;
        public int? PreviousProgramLow = null;
        public int ProgramStatus = 0;

        public int MonthStatus, QuarterStatus, HalfStatus, YearStatus;
        public int Month, Quarter, Half, Year;
    }



    // public ScoreData Score { get; set; } = new ScoreData();
    public class ScoreData
    {
        public double 푀분;
        public double 거분;
        public double 배차, 배합;
        public double 급락, 급상;
        public double 총점;
        public int 푀분_등수, 거분_등수;
        public int 배차_등수, 배합_등수;
        public int 그순;
        public double 등합;
    }



    // public StatData Stat { get; set; } = new StatData();
    public class StatisticsData
    {
        public int 프분_count;
        public double 프분_avr, 프분_dev;
        public double 거분_avr, 거분_dev;
        public double 배차_avr, 배차_dev;
        public double 배합_avr, 배합_dev;

        public char 시장구분; // P 코스피, D 코스닥
        public double 시총;

        public string 일간변동평균편차;
        public double 일간변동평균;
        public double 일간변동편차;
        public int 일최대거래액; // in testing, these are diplayed in textBox 10, 11,, 12 etc
        public int 일최저거래액;
        public int 일평균거래액;
        public ulong 일평균거래량;
    }

    // public LevelData Level { get; set; } = new LevelData();
    public class LevelData
    {
        public double 프퍼, 푀퍼, 급락, 잔잔;
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
        public int[] 틱의시간 = new int[MajorIndex.TickArraySize]; // 틱의시간    // 호가창 tT
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



        public void AppendTick(int[] t, int HHmmss, double currBuyVol, double currSellVol, long prevBuyVol, long prevSellVol, double multipleFactor, double moneyFactor)
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

            틱의시간[0] = HHmmss;
            틱의가격[0] = 가격;
            틱의수급[0] = 수급;
            틱의체강[0] = (int)(체강 * g.HUNDRED);
            틱매수량[0] = (int)currBuyVol;
            틱매도량[0] = (int)currSellVol;

            틱매수배[0] = (int)((currBuyVol - prevBuyVol) * multipleFactor);
            틱매도배[0] = (int)((currSellVol - prevSellVol) * multipleFactor);
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

    }





    // public DealStatus Deal { get; set; } = new DealStatus();
    public class DealStatus
    {
        public int 보유량;
        public double 수익률;
        public double 전수익률;
        public double 장부가;

        public long 평가금액; // Error, if use int instead of long
        public long 손익단가; // Error, if use int instead of long

    }


    // public PostData Post { get; set; } = new PostData();
    public class PostData
    {
        public int 매수호가거래액_백만원; // ok
        public int 매도호가거래액_백만원; // ok
        public int 분당가격차; // ok

        public double 프누천;
        public double 외누천;
        public double 기누천;
        public double 거누천;
        public double 종거천;
    }




    // public MiscData Misc { get; set; } = new MiscData();
    public class MiscData
    {
        public double 수급과장배수 = 1;
        public int oGL_sequence_id;




        public bool ShrinkDraw = false;
        public List<string> Friends { get; set; } = new List<string>();
    }










     //public static StockData ConvertFromOldStockData(stock_data old)
     //   {
     //       var s = new StockData();

     //       s.Stock = old.stock;

     //       // ⏹ TickReflection
     //       s.Reflection.ArrayCount = old.변곡.array_count;
     //       s.Reflection.ProvisionalProgramSumK = old.변곡.틱프로잠정합_천;
     //       s.Reflection.ProvisionalForeignSumK = old.변곡.틱외인잠정합_천;
     //       s.Reflection.ProgramK = old.변곡.틱프로천;
     //       s.Reflection.ForeignK = old.변곡.틱외인천;
     //       s.Reflection.Prices = old.변곡.틱의가격;
     //       s.Reflection.Times = old.변곡.틱의시간;

     //       // ⏹ Pass
     //       s.Pass.PreviousPriceHigh = old.pass.previousPriceHigh;
     //       s.Pass.PreviousPriceLow = old.pass.previousPriceLow;
     //       s.Pass.PriceStatus = old.pass.priceStatus;
     //       s.Pass.PreviousProgramHigh = old.pass.previousProgramHigh;
     //       s.Pass.PreviousProgramLow = old.pass.previousProgramLow;
     //       s.Pass.ProgramStatus = old.pass.programStatus;
     //       s.Pass.MonthStatus = old.pass.monthStatus;
     //       s.Pass.QuarterStatus = old.pass.quarterStatus;
     //       s.Pass.HalfStatus = old.pass.halfStatus;
     //       s.Pass.YearStatus = old.pass.yearStatus;
     //       s.Pass.Month = old.pass.month;
     //       s.Pass.Quarter = old.pass.quarter;
     //       s.Pass.Half = old.pass.half;
     //       s.Pass.Year = old.pass.year;

     //       // ⏹ Score
     //       s.Score.푀분 = old.점수.푀분;
     //       s.Score.거분 = old.점수.거분;
     //       s.Score.배차 = old.점수.배차;
     //       s.Score.배합 = old.점수.배합;
     //       s.Score.급락 = old.점수.급락;
     //       s.Score.급상 = old.점수.급상;
     //       s.Score.총점 = old.점수.총점;
     //       s.Score.푀분_등수 = old.점수.푀분_등수;
     //       s.Score.거분_등수 = old.점수.거분_등수;
     //       s.Score.배차_등수 = old.점수.배차_등수;
     //       s.Score.배합_등수 = old.점수.배합_등수;
     //       s.Score.그순 = old.점수.그순;
     //       s.Score.등합 = old.점수.등합;

     //       // ⏹ Statistics
     //       s.Statistics.프분_count = old.통계.프분_count;
     //       s.Statistics.프분_avr = old.통계.프분_avr;
     //       s.Statistics.프분_dev = old.통계.프분_dev;
     //       s.Statistics.거분_avr = old.통계.거분_avr;
     //       s.Statistics.거분_dev = old.통계.거분_dev;
     //       s.Statistics.배차_avr = old.통계.배차_avr;
     //       s.Statistics.배차_dev = old.통계.배차_dev;
     //       s.Statistics.배합_avr = old.통계.배합_avr;
     //       s.Statistics.배합_dev = old.통계.배합_dev;
     //       s.Statistics.일간변동평균편차 = old.일간변동평균편차;
     //       s.Statistics.일간변동평균 = old.일간변동평균;
     //       s.Statistics.일간변동편차 = old.일간변동편차;
     //       s.Statistics.일최대거래액 = old.일최대거래액;
     //       s.Statistics.일최저거래액 = old.일최저거래액;
     //       s.Statistics.일평균거래액 = old.일평균거래액;
     //       s.Statistics.일평균거래량 = old.일평균거래량;
     //       s.Statistics.시장구분 = old.시장구분;
     //       s.Statistics.시총 = old.시총;

     //       // ⏹ Level
     //       s.Level.프퍼 = old.정도.프퍼;
     //       s.Level.푀퍼 = old.정도.푀퍼;
     //       s.Level.급락 = old.정도.급락;
     //       s.Level.잔잔 = old.정도.잔잔;

     //       // ⏹ Api
     //       s.Api.현재가 = old.현재가;
     //       s.Api.시초가 = old.시초가;
     //       s.Api.시초 = old.시초;
     //       s.Api.전고가 = old.전고가;
     //       s.Api.전저가 = old.전저가;
     //       s.Api.전저 = old.전저;
     //       s.Api.매수1호가 = old.매수1호가;
     //       s.Api.매도1호가 = old.매도1호가;
     //       s.Api.거래량 = old.거래량;
     //       s.Api.전일거래액_천만원 = old.전일거래액_천만원;
     //       s.Api.장구분 = old.장구분;
     //       s.Api.총매도호가잔량 = old.총매도호가잔량;
     //       s.Api.총매수호가잔량 = old.총매수호가잔량;
     //       s.Api.최우선매도호가잔량 = old.최우선매도호가잔량;
     //       s.Api.최우선매수호가잔량 = old.최우선매수호가잔량;
     //       s.Api.전일종가 = old.전일종가;
     //       s.Api.예상체결가 = old.예상체결가;
     //       s.Api.예상체결수량 = old.예상체결수량;
     //       s.Api.시간외단일대비부호 = old.시간외단일대비부호;
     //       s.Api.시간외단일전일대비 = old.시간외단일전일대비;
     //       s.Api.시간외단일현재가 = old.시간외단일현재가;
     //       s.Api.시간외단일거래대금 = old.시간외단일거래대금;
     //       s.Api.당일프로그램순매수량 = old.당일프로그램순매수량;
     //       s.Api.당일외인순매수량 = old.당일외인순매수량;
     //       s.Api.당일기관순매수량 = old.당일기관순매수량;
     //       s.Api.공매도수량 = old.공매도수량;
     //       s.Api.가격 = old.가격;
     //       s.Api.수급 = old.수급;
     //       s.Api.체강 = old.체강;
     //       s.Api.nrow = old.nrow;
     //       s.Api.x = old.x;
     //       s.Api.틱의시간 = old.틱의시간;
     //       s.Api.틱의가격 = old.틱의가격;
     //       s.Api.틱의수급 = old.틱의수급;
     //       s.Api.틱의체강 = old.틱의체강;
     //       s.Api.틱매수량 = old.틱매수량;
     //       s.Api.틱매도량 = old.틱매도량;
     //       s.Api.틱매수배 = old.틱매수배;
     //       s.Api.틱매도배 = old.틱매도배;
     //       s.Api.틱배수차 = old.틱배수차;
     //       s.Api.틱배수합 = old.틱배수합;
     //       s.Api.틱프외퍼 = old.틱프외퍼;
     //       s.Api.틱프로량 = old.틱프로량;
     //       s.Api.틱프로천 = old.틱프로천;
     //       s.Api.틱외인량 = old.틱외인량;
     //       s.Api.틱외인천 = old.틱외인천;
     //       s.Api.틱거래천 = old.틱거래천;
     //       s.Api.틱매도잔 = old.틱매도잔;
     //       s.Api.틱매수잔 = old.틱매수잔;
     //       s.Api.분프로천 = old.분프로천;
     //       s.Api.분외인천 = old.분외인천;
     //       s.Api.분거래천 = old.분거래천;
     //       s.Api.분매수배 = old.분매수배;
     //       s.Api.분매도배 = old.분매도배;
     //       s.Api.분배수차 = old.분배수차;
     //       s.Api.분배수합 = old.분배수합;
     //       s.Api.매수배 = old.매수배;
     //       s.Api.매도배 = old.매도배;

     //       // ⏹ Deal
     //       s.Deal.보유량 = old.보유량;
     //       s.Deal.수익률 = old.수익률;
     //       s.Deal.전수익률 = old.전수익률;
     //       s.Deal.장부가 = old.장부가;
     //       s.Deal.평가금액 = old.평가금액;
     //       s.Deal.손익단가 = old.손익단가;

     //       // ⏹ Post
     //       s.Post.매수호가거래액_백만원 = old.매수호가거래액_백만원;
     //       s.Post.매도호가거래액_백만원 = old.매도호가거래액_백만원;
     //       s.Post.분당가격차 = old.분당가격차;
     //       s.Post.프누천 = old.프누천;
     //       s.Post.외누천 = old.외누천;
     //       s.Post.기누천 = old.기누천;
     //       s.Post.거누천 = old.거누천;
     //       s.Post.종거천 = old.종거천;

     //       // ⏹ Misc
     //       s.Misc.수급과장배수 = old.수급과장배수;
     //       s.Misc.oGL_sequence_id = old.oGL_sequence_id;
     //       s.Misc.ShrinkDraw = old.ShrinkDraw;
     //       s.Misc.Friends = old.절친;

     //       return s;
     //   }


    }