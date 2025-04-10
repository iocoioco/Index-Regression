using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
namespace New_Tradegy.Library
{
    public class g
    {
        public static StockRepository Repository;
        public static ChartManager ChartManager;
        public static StockManager StockManager;


        public static GroupManager GroupManager;
        //public static Dictionary<string, StockData> Ogldata = new Dictionary<string, StockData>();

        // AppState
        public static bool test; // test or real
        public static int date; // current day for display

        // ScreenConfig
       
        public static int screenHeight; // window height
        public static int screenWidth; // window width

        // TradingSettings
        public static bool optimumTrading = false; // fire buy or sell along with book bid/ask ratio 
        public static bool add_interest = false; // add stock or not to the 관심종목 when stocks active
        public static bool confirm_sell = false; // sell by click or modal display to confirm sell condition
        public static int 일회거래액 = 0; // amount of deal money in 10 thousands Won
        

        public static int deal_profit = 0; // profit for the day

        // Constants
        public static string Account; // DaiShin Account number
        public static double 천만원 = 10000000.0; // unit for division
        public static double 억원 = 100000000.0; // unit for division
        public static double HUNDRED = 100.0;
        public static double THOUSAND = 1000.0;
        public static int DgvCellHeight = 28;

        // InputControl
        public static bool shortform; // shortform includes less number of stocks for test purpose 


        // ChartManager
        public static Chart chart1; // main chart
        public static Chart chart2; // sub chart
        public static bool draw_history_forwards = false; // move date forwards or backwards for history analysis

        // DgvControl
        
        public class dgvClass
        {
            //public string stock;
            //public string code;
            //public int index;
            public DataTable dtb;
            public DataGridView dgv;
            //public int[,] itb = new int[21, 3];
        }
        public static dgvClass 매매 = new dgvClass();
        public static dgvClass 그룹 = new dgvClass();
        public static dgvClass 제어 = new dgvClass();

        // StockGroup

        // Index

        // Score

        // Sectors

        // OrderItem

        // APIControl
        public static bool connected = false; // Daishin API connected or not

        // PostControl
        public static int postInterval = 30; // seconds to be interpolated to 1 minutes

        // MiscControl
        public static char PeoridNews = 'w'; // google search 'd' past day, 'w' past month
   
        // FontAndThemeAndColor
        public static Color[] Colors = new Color[] // five color set for annotation and chartarea based on activity
        {
            Color.FromArgb(255, 200, 255),    // 보통 빨강
            Color.FromArgb(255, 200, 200),   // 옅은 빨강
            Color.FromArgb(200, 255, 255),   // 보통 주황
            Color.FromArgb(200, 255, 200),   // 옅은 주황
            Color.FromArgb(255, 255, 200),   // 보통 노랑
            Color.FromArgb(200, 200, 255), // 옅은 노랑
               
        };
        public static int LineWidth = 2; // curve width for default 


        // InputTracking

        // BookBid(was jpjs
        public static ConcurrentDictionary<string, object> BookBidInstances = // bookbid instances dictionary
           new ConcurrentDictionary<string, object>(); 







        
       

        
        
        

       

        
        

       
        
        public static readonly object lockObject = new object(); // Make sure it's initialized

        public static int MAX_ROW = 382; // maximum rows of minute data for each stock from 0859 to 1520

        
        
   

     

       


        public static string clickedStock = ""; // target stock to be used
        public static string clickedTitle = ""; // target group title to be used

        public static string q; // mode of display (h : history etc.)

        public static List<g.stock_data> ogl_data = new List<g.stock_data>(); // stock data holder
        public static List<string> sl = new List<string>();   // selected single stock based on ranking
        public static List<string> dl = new List<string>();   // selected stocks for display on chart1 (main chart)

        public static string oGl_data_selection = "총점"; // group ranking calculation ciriterion

        public static List<string> 보유종목 = new List<string>(); // stock holding

        public static List<string> 호가종목 = new List<string>(); // stocks with bookbid on chart1 (main chart)
        public static List<string> 관심종목 = new List<string>(); // stocks without bookbid which has higher ranking
        public static List<string> 파일관심종목 = new List<string>(); // stocks saved in file name of "관심.txt"

        public class group_data // group of stocks (sector wise)
        {
            public string title; // group title
            public List<string> stocks = new List<string>(); // stocks in group

            public double 총점, 수평, 강평;
            public double 프누, 종누, 거분, 배합, 푀분, 배차, 가증, 분거, 상순, 저순;

        }
        public static List<g.group_data> oGL_data = new List<group_data>(); // group data list

        public static List<string> KODEX4 = new List<string>();   // Kospi leverage inverse and Kosdaq leverage inverse : 4 stocks

        public class kospi_kosdaq_mixed // 
        {
            public List<string> stock = new List<string>();
            public List<double> weight = new List<double>();
        }
        public static kospi_kosdaq_mixed kospi_mixed = new kospi_kosdaq_mixed(); // stocks for kospi index 
        public static kospi_kosdaq_mixed kosdaq_mixed = new kospi_kosdaq_mixed(); // stocks for kosdaq index 

        public static int stocks_per_marketeye = 200; // max. number of stocks to be downloaded at a time thru. API 

        public static int ogl_data_next = 0; // sequential index among total stocks to be downloaded next time thru. API anong
        public static int 예치금 = 0; // deposit money in 10 thousands Won

        
        public static int rqwey_nCol = 10; // number of chart area columns in main chart
        public static int rqwey_nRow = 3; // number of chart area rows in main chart

        public static int MarketeyeCount = 0; // total count of API download from start of market
        public static int MarketeyeDividerForEvalStock = 10; // evaluation of stocks after how many MarketeyeCount increases
        public static int minuteSaveAll = 0; // to block repeated saving of minute data of all stocks (every hour saved)
        public static int alamed_hhmm = 0; // to bock alarm repeated like "Taiwan market open"

        public static int moving_reference_date = 0; // to check history data

        public static int[] Npts = new int[2]; // total number of point from start Npts[0] to current Npts[1] used for test

        public static int[] SavedNpts = new int[2]; // when check history data and get back to the current data 

        public static int EndNptsBeforeExtend; // shrink draw i.e to check recent 30 points data not whole data
        public static bool EndNptsExtendedOrNot; // shrink draw i.e to check recent 30 points data not whole data
        public static int NptsForShrinkDraw = 30; // how many points backwards used from the last minute

        public static int nCol = 10;  // number of chart area columns in main chart
        public static int nRow = 3; // number of chart area rows in main chart

        public static int gid; // start index of stock to display in the rank
        public static int Gid; // start index of group to display in the rank
        public static int saved_Gid; // saved Gid for return from history display
        public static int draw_selection = 1; // 1, normal, 2, days 3, Bollinder
        public static int npts_fi_dwm = 40; // draw_selection = 2, how many days to display

        public static int npts_for_magenta_cyan_mark = 4; // magenta and cyan marking on price curve repetition criterion


        // variables : chart1 control, stock selection, font etc
        public class variable
        {
            public string KeyString = "총점";
            public string SubKeyStr = "그순";
            public string old_key_string = "푀분";

            public double 분당거래액이상_천만원; // in setting 10
            public double 호가거래액_백만원; // in setting 10
            public double 편차이상;  // in setting 1
            public double 배차이상; // in setting 0, not used
            public double 종가기준추정거래액이상_천만원; // insetting 0
            public double 시총이상; // in setting 0

            public int 푀플 = 1;
            public int 배플 = 1;
            public double 수급과장배수 = 20; // 수급 과장하여 표시하는 배수

            public int q_advance_lines = 15;
            public int Q_advance_lines = 150;
            public int r3_display_lines = 20;

            public float font = 16.0F;
        }
        public static variable v = new variable();

        // kodex (price, program buy, others buy, retail buys)
        // for kospi leverage inverse and kosdaq leverage and inverse
        public static double[,] kodex_magnifier = new double[4, 4];

        public class score
        {
            public List<List<double>> dev = new List<List<double>>();
            public List<List<double>> mkc = new List<List<double>>();
            public List<List<double>> avr = new List<List<double>>();
            public List<List<double>> 돌파 = new List<List<double>>();
            public List<List<double>> 눌림 = new List<List<double>>();
            public List<List<double>> 가연 = new List<List<double>>();
            public List<List<double>> 가분 = new List<List<double>>();
            public List<List<double>> 가틱 = new List<List<double>>();
            public List<List<double>> 가반 = new List<List<double>>();
            public List<List<double>> 가지 = new List<List<double>>();
            public List<List<double>> 가위 = new List<List<double>>();
            public List<List<double>> 수연 = new List<List<double>>();
            public List<List<double>> 수지 = new List<List<double>>();
            public List<List<double>> 수위 = new List<List<double>>();
            public List<List<double>> 강연 = new List<List<double>>();
            public List<List<double>> 강지 = new List<List<double>>();
            public List<List<double>> 강위 = new List<List<double>>();
            public List<List<double>> 푀분 = new List<List<double>>();
            public List<List<double>> 프틱 = new List<List<double>>();
            public List<List<double>> 프지 = new List<List<double>>();
            public List<List<double>> 프퍼 = new List<List<double>>();
            public List<List<double>> 프누 = new List<List<double>>();
            public List<List<double>> 거분 = new List<List<double>>();
            public List<List<double>> 거틱 = new List<List<double>>();
            public List<List<double>> 거일 = new List<List<double>>();
            public List<List<double>> 배차 = new List<List<double>>();
            public List<List<double>> 배반 = new List<List<double>>();
            public List<List<double>> 배합 = new List<List<double>>();
            public List<List<double>> 급락 = new List<List<double>>();
            public List<List<double>> 잔잔 = new List<List<double>>();
            public List<List<double>> 그룹 = new List<List<double>>();

            public double 푀분_wgt;
            public double 거분_wgt;
            public double 배차_wgt;
            public double 배합_wgt;
            public double 그룹_wgt;
            public double 피로_wgt;
            public double 기타_wgt;
        }
        public static score s = new score();






        //public class stock_data
        //{
        //    public string stock; // Name
        //    public string code; // Code


        //    //  public TickReflection TickReflection { get; set; } = new TickReflection();
        //    public class t // TickReflection
        //    {
        //        public int array_count = 0; // ArrayCount

        //        public double 틱프로잠정합_천 = 0.0; // ProvisionalProgramSumK
        //        public double 틱외인잠정합_천 = 0.0; // ProvisionalForeignSumK

        //        public double[] 틱프로천 = new double[MarketData.TickArraySize]; // ProgramK
        //        public double[] 틱외인천 = new double[MarketData.TickArraySize]; // ForeignK
        //        public int[] 틱의가격 = new int[MarketData.TickArraySize]; // Prices
        //        public int[] 틱의시간 = new int[MarketData.TickArraySize]; // Times
        //    }
        //    public t 변곡 = new t();




        //    // public PricePassData Pass { get; set; } = new PricePassData();
        //    public class ux
        //    {
        //        public int previousPriceHigh = int.MinValue; // PreviousPriceHigh
        //        public int? previousPriceLow = null; // PreviousPriceLow
        //        public int priceStatus = 0; // PriceStatus

        //        public int previousProgramHigh = int.MinValue; // PreviousProgramHigh
        //        public int? previousProgramLow = null; // PreviousProgramLow
        //        public int programStatus = 0; // ProgramStatus

        //        public int monthStatus = 0; // MonthStatus, QuarterStatus, HalfStatus, YearStatus;
        //        public int quarterStatus = 0;
        //        public int halfStatus = 0;
        //        public int yearStatus = 0;

        //        public int month; // Month, Quarter, Half, Year
        //        public int quarter;
        //        public int half;
        //        public int year;

        //        public int upperPassingPrice = 0;
        //        public int lowerPassingPrice = 0;
        //    }
        //    public ux pass = new ux();










        //    public List<string> 절친 = new List<string>();

        //    public class score
        //    {
        //        public double 푀분;
        //        public double 거분;
        //        public double 배차, 배합;
        //        public double 급락, 급상;
        //        public double 총점;
        //        public int 푀분_등수, 거분_등수;
        //        public int 배차_등수, 배합_등수;
        //        public int 그순; // 그룹_등수
        //        public double 등합;
        //    }
        //    public score 점수 = new score();







        //    public class statics
        //    {
        //        public int 프분_count;
        //        public double 프분_avr, 프분_dev;
        //        public double 거분_avr, 거분_dev;
        //        public double 배차_avr, 배차_dev;
        //        public double 배합_avr, 배합_dev;
        //    }
        //    public statics 통계 = new statics();






        //    public class level
        //    {
        //        public double 프퍼, 푀퍼, 급락, 잔잔;
        //    }
        //    public level 정도 = new level();







        //    //1 시간ulong hhmm

        //    public long 현재가; //4 long
        //    public long 시초가; //5 long  
        //    public int 시초;
        //    public long 전고가; //6 long

        //    public long 전저가; //7 long
        //    public int 전저;

        //    public long 매수1호가;
        //    public long 매도1호가; // 8 long public long 매수1호가; // 9 long

        //    public ulong 거래량; // 10 ulong

        //    public double 전일거래액_천만원; // marketeye not provide, calculated from "일"

        //    public char 장구분; // 12 char '0' 장전 '1' 동시호가 '2' 장중
        //    public ulong 총매도호가잔량; //13 ulong
        //    public ulong 총매수호가잔량; //14 ulong
        //    public int 최우선매도호가잔량; //15 (ulong) converted to int
        //    public int 최우선매수호가잔량; //16 (ulong) converted to int

        //    public long 전일종가; //23 long


        //    public long 예상체결가; //28 long // not down
        //    public ulong 예상체결수량; //31 ulong // not down

        //    public char 시간외단일대비부호; //36 char +, - // not down
        //    public long 시간외단일전일대비; //37 long, 36 필히 하여야 함 // not down
        //    public long 시간외단일현재가; //38 long // not down
        //    public ulong 시간외단일거래대금; //45 ulonglong // not down

        //    public long 당일프로그램순매수량; // 116 long
        //    public long 당일외인순매수량; //118 long

        //    public long 당일기관순매수량; //120 long

        //    public ulong 공매도수량; //127 ulong

        //    public int 가격;
        //    public int 수급;
        //    public double 체강;

        //    public int nrow = 0;
        //    public int[,] x = new int[MAX_ROW, 12];

        //    // 틱 데이터
        //    public int[] 틱의시간 = new int[MarketData.TickArraySize]; // 틱의시간    // 호가창 tT
        //    public int[] 틱의가격 = new int[MarketData.TickArraySize]; // 틱의가격    // 호가창
        //    public int[] 틱의수급 = new int[MarketData.TickArraySize]; // 틱의수급
        //    public int[] 틱의체강 = new int[MarketData.TickArraySize]; // 틱의체강

        //    public int[] 틱매수량 = new int[MarketData.TickArraySize]; // 틱매수량
        //    public int[] 틱매도량 = new int[MarketData.TickArraySize]; // 틱매도량
        //    public int[] 틱매수배 = new int[MarketData.TickArraySize]; // 틱매수배
        //    public int[] 틱매도배 = new int[MarketData.TickArraySize]; // 틱매도배

        //    public int[] 틱배수차 = new int[MarketData.TickArraySize];  // 틱배수차
        //    public int[] 틱배수합 = new int[MarketData.TickArraySize];  // 틱배수합
        //    public int[] 틱프외퍼 = new int[MarketData.TickArraySize];  // 틱프외퍼

        //    public int[] 틱프로량 = new int[MarketData.TickArraySize]; // 틱프로량  
        //    public double[] 틱프로천 = new double[MarketData.TickArraySize]; // 틱프돈천

        //    public int[] 틱외인량 = new int[MarketData.TickArraySize]; // 틱외인량
        //    public double[] 틱외인천 = new double[MarketData.TickArraySize]; // 틱외돈천                  //

        //    public double[] 틱거래천 = new double[MarketData.TickArraySize]; // 틱거돈천

        //    public int[] 틱매도잔 = new int[MarketData.TickArraySize]; // 최우선매도호가잔량
        //    public int[] 틱매수잔 = new int[MarketData.TickArraySize]; // 최우선매수호가잔량

        //    // 분 데이터
        //    public double[] 분프로천 = new double[MarketData.MinuteArraySize]; // 분프로천
        //    public double[] 분외인천 = new double[MarketData.MinuteArraySize]; // 분외인천
        //    public double[] 분거래천 = new double[MarketData.MinuteArraySize]; // 분거래천
        //    public int[] 분매수배 = new int[MarketData.MinuteArraySize]; // 분매수배
        //    public int[] 분매도배 = new int[MarketData.MinuteArraySize]; // 분매도배
        //    public int[] 분배수차 = new int[MarketData.MinuteArraySize];  // 분배수차
        //    public int[] 분배수합 = new int[MarketData.MinuteArraySize];  // 분배수차

        //    public int 매수배;
        //    public int 매도배;





        //    // Deal Manager
        //    public int 보유량;
        //    public double 수익률;
        //    public double 전수익률;
        //    public double 장부가;

        //    public long 평가금액; // Error, if use int instead of long
        //    public long 손익단가; // Error, if use int instead of long





        //    // post
        //    public int 매수호가거래액_백만원; // ok
        //    public int 매도호가거래액_백만원; // ok
        //    public int 분당가격차; // ok

        //    public double 프누천;
        //    public double 외누천;
        //    public double 기누천;
        //    public double 거누천;
        //    public double 종거천;


        //    // Misc
        //    public double 수급과장배수 = 1;
        //    public int oGL_sequence_id;
        //    public char 시장구분; // P 코스피, D 코스닥
        //    public double 시총;
        //    public string 일간변동평균편차;
        //    public double 일간변동평균;
        //    public double 일간변동편차;
        //    public int 일최대거래액; // in testing, these are diplayed in textBox 10, 11,, 12 etc
        //    public int 일최저거래액;
        //    public int 일평균거래액;
        //    public ulong 일평균거래량;



        //    public bool ShrinkDraw = false;

        //}



        public class stock_data
        {
            public class t
            {
                public int array_count = 0;

                public double 틱프로잠정합_천 = 0.0;
                public double 틱외인잠정합_천 = 0.0;

                public double[] 틱프로천 = new double[MajorIndex.TickArraySize];
                public double[] 틱외인천 = new double[MajorIndex.TickArraySize];
                public int[] 틱의가격 = new int[MajorIndex.TickArraySize];
                public int[] 틱의시간 = new int[MajorIndex.TickArraySize];
            }
            public t 변곡 = new t();

            public class ux
            {
                public int previousPriceHigh = int.MinValue;
                public int? previousPriceLow = null;
                public int priceStatus = 0;

                public int previousProgramHigh = int.MinValue;
                public int? previousProgramLow = null;
                public int programStatus = 0;

                public int monthStatus = 0;
                public int quarterStatus = 0;
                public int halfStatus = 0;
                public int yearStatus = 0;

                public int month;
                public int quarter;
                public int half;
                public int year;

                public int upperPassingPrice = 0;
                public int lowerPassingPrice = 0;
            }
            public ux pass = new ux();

            //public class d
            //{
            //    public int upperPassingPrice = 0;
            //    public int lowerPassingPrice = 0;
            //}
            //public d deal = new d();

            public string stock;


            public bool ShrinkDraw = false;

            //public bool in_group_or_not = false;

            public List<string> 절친 = new List<string>();


            public bool downloaded = false;

            public string code; //0

            public class score
            {
                public double dev, mkc, avr;
                public double 돌파, 눌림;
                public double 가연, 가분, 가틱, 가반, 가지, 가위;
                public double 수연, 수지, 수위;
                public double 강연, 강지, 강위;
                public double 푀분, 프틱, 프지, 프퍼, 프누;
                public double 거분, 거틱, 거일;
                public double 배차, 배반, 배합;

                public double 급락, 급상;
                public double 스물;
                public double 그룹;

                public double 동시;

                public double 총점;

                public int 푀분_등수, 거분_등수;
                public int 배차_등수, 배합_등수;
                public int 그순; // 그룹_등수

                public double 등합;
            }
            public score 점수 = new score();
            public class statics
            {
                public int 프분_count;
                public double 프분_avr, 프분_dev;
                public double 거분_avr, 거분_dev;
                public double 배차_avr, 배차_dev;
                public double 배합_avr, 배합_dev;
                public double 그룹_avr, 그룹_dev;
                public double 피로_avr, 피로_dev;
                public double 기타_avr, 기타_dev;

            }
            public statics 통계 = new statics();

            public class level
            {
                public double 돌파, 눌림, 가반, 가지, 강지, 배반, 프퍼, 푀퍼, 프지, 프가, 급락, 잔잔;
            }
            public level 정도 = new level();

            //1 시간ulong hhmm
            public char 전일대비부호; // 2 char
            public long 전일대비; // 3 long
            public long 현재가; //4 long
            public long 시초가; //5 long  
            public int 시초;
            public long 전고가; //6 long

            public long 전저가; //7 long
            public int 전저;

            public long 매수1호가;
            public long 매도1호가; // 8 long public long 매수1호가; // 9 long

            public ulong 거래량; // 10 ulong
            //public ulong 거래액_원; // 11 ulong
            public double 전일거래액_천만원; // marketeye not provide, calculated from "일"

            public char 장구분; // 12 char '0' 장전 '1' 동시호가 '2' 장중
            public ulong 총매도호가잔량; //13 ulong
            public ulong 총매수호가잔량; //14 ulong
            public int 최우선매도호가잔량; //15 (ulong) converted to int
            public int 최우선매수호가잔량; //16 (ulong) converted to int
            //public ulong 전일거래량; //22 ulong
            public long 전일종가; //23 long
            //public double 체결강도; //24 float

            public long 예상체결가; //28 long
            public ulong 예상체결수량; //31 ulong

            public char 시간외단일대비부호; //36 char +, -
            public long 시간외단일전일대비; //37 long, 36 필히 하여야 함
            public long 시간외단일현재가; //38 long
            public ulong 시간외단일거래대금; //45 ulonglong

            public double 수급과장배수 = 1;
            public long 당일프로그램순매수량; // 116 long
            public long 당일외인순매수량; //118 long

            public long 당일기관순매수량; //120 long
            //public long 전일외국인순매수; //121 long

            //public long 전일기관순매수; //122 long
            public ulong 공매도수량; //127 ulong

            public double 시총;

            public int 시간;
            public int 가격;
            public int 수급;
            public double 체강;

            public int 보유량;
            public double 수익률;
            public double 전수익률;
            public double 장부가;

            public long 평가금액; // Error, if use int instead of long
            public long 손익단가; // Error, if use int instead of long

            public string 일간변동평균편차;
            public double 일간변동평균;
            public double 일간변동편차;

            public int 일최대거래액; // in testing, these are diplayed in textBox 10, 11,, 12 etc
            public int 일최저거래액;
            public int 일평균거래액;
            public ulong 일평균거래량;

            public int 일평균푀돈액;

            public char 시장구분; // P 코스피, D 코스닥

            public int nrow = 0;
            public int[,] x = new int[MAX_ROW, 12];

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

            // 일 데이터
            public double 프누천;
            public double 외누천;
            public double 기누천;
            public double 거누천;
            public double 종거천;

            public int 매수호가거래액_백만원; // ok
            public int 매도호가거래액_백만원; // ok
            public int 분당가격차; // ok

            public int 매수배;
            public int 매도배;

            public bool selected_for_group = true; // used, but the value is fixed in glbl

            public int oGL_sequence_id; // assigned in the program, but not used 

        }
    }
}

