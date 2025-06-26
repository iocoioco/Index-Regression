
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Trackers.Charting;

namespace New_Tradegy.Library
{
    public class g
    {
        public static Form1 MainForm { get; set; }
        public static Form1 SubForm { get; set; }



        //public static DataGridView ActiveBookBid;

        //public static ChartIndex ChartIndex;

        public static BookBidManager BookBidManager;
        public static ChartMain ChartMain;

        public static ChartManager ChartManager;

        public static StockManager StockManager;
        public static StockRepository StockRepository;


        public static GroupManager GroupManager;
        //public static Dictionary<string, StockData> Ogldata = new Dictionary<string, StockData>();

        public static ControlPane controlPane;
        public static GroupPane groupPane;
        public static TradePane tradePane;

        // AppState
        public static bool test; // test or real
        public static int date; // current day for display

        // ScreenConfig



        public static int screenWidth = 1920; // window height
        public static int screenHeight = 1032; // window width
        public static int cellHeight = 28;

        // TradingSettings

        public static bool optimumTrading = false; // fire buy or sell along with book bid/ask ratio 
        public static bool add_interest = false; // add stock or not to the 관심종목 when stocks active
        public static bool confirm_sell = false; // sell by click or modal display to confirm sell condition
        public static int 일회거래액 = 0; // amount of deal money in 10 thousands Won
        public static int 예치금 = 0;

        public static int UpdateDealProfit = 0; // profit for the day

        // Constants
        public static string Account; // DaiShin Account number
        public static double 천만원 = 10000000.0; // unit for division
        public static double 억원 = 100000000.0; // unit for division
        public static double HUNDRED = 100.0;
        public static double THOUSAND = 1000.0;
        public static int CellHeight = 28;

        // InputControl
        public static bool shortform; // shortform includes less number of stocks for test purpose 


        // ChartManager
        public static Chart chart1; // main chart
        public static Chart chart2; // sub chart
        public static bool draw_history_forwards = false; // move date forwards or backwards for history analysis

        // DgvControl

        //public class dgvClass
        //{
        //    public DataTable dtb;
        //    public DataGridView dgv;
        //    public TradePane TradeRenderer;
        //    public GroupPane GroupRenderer;
        //    public ControlPane ControlRenderer;
        //}
        //public static dgvClass 매매 = new dgvClass();
        //public static dgvClass 그룹 = new dgvClass();
        //public static dgvClass 제어 = new dgvClass();

        // APIControl
        public static bool connected = false; // Daishin API connected or not

        // PostControl
        public static int postInterval = 30; // seconds to be interpolated to 1 minutes

        // MiscControl
        public static char PeoridNews = 'd'; // google search 'd' past day, 'w' past month

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

        // BookBid(was jpjs
        public static ConcurrentDictionary<string, object> BookBidInstances = // bookbid instances dictionary
           new ConcurrentDictionary<string, object>();

        public static readonly object lockObject = new object(); // Make sure it's initialized

        public static int MAX_ROW = 382; // maximum rows of minute data for each stock from 0859 to 1520

        public static string clickedStock = ""; // target stock to be used
        public static string clickedTitle = ""; // target group title to be used

        public static string q; // mode of display (h : history etc.)


        
         

        public static string oGl_data_selection = "총점"; // group ranking calculation ciriterion










        public class kospi_kosdaq_mixed // 
        {
            public List<string> stock = new List<string>();
            public List<double> weight = new List<double>();
        }
        public static kospi_kosdaq_mixed kospi_mixed = new kospi_kosdaq_mixed(); // stocks for kospi index 
        public static kospi_kosdaq_mixed kosdaq_mixed = new kospi_kosdaq_mixed(); // stocks for kosdaq index 

        public static int stocks_per_marketeye = 200; // max. number of stocks to be downloaded at a time thru. API 



        public static int MarketeyeCount = 0; // total count of API download from start of market
        public static int MarketeyeCountDivicer = 10; // evaluation of stocks after how many MarketeyeCount increases
        public static int minuteSaveAll = 0; // to block repeated saving of minute data of all stocks (every hour saved)
        public static int AlarmedHHmm = 0; // to bock alarm repeated like "Taiwan market open"

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

        public class Score
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
            public List<List<double>> 푀누 = new List<List<double>>();
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
        public static Score s = new Score();

        // variables : chart1 control, stock selection, font etc
        public class Variable
        {
            public string MainChartDisplayMode = "푀분";
            public string SubChartDisplayMode = "피올";
            public string PreviousMainChartDisplayMode = "푀분";

            public int 분당거래액이상_천만원; // in setting 10
            public int 호가거래액_백만원; // in setting 10
            public int 편차이상;  // in setting 1
            public int 배차이상; // in setting 0, not used
            public int 종가기준추정거래액이상_천만원; // insetting 0
            public int 시총이상; // in setting 0

            public int 푀플 = 1;
            public int 배플 = 1;
            public int 수급과장배수 = 20; // 수급 과장하여 표시하는 배수
            public int 배수과장배수 = 20;

            public int q_advance_lines = 15;
            public int Q_advance_lines = 150;
            public int r3_display_lines = 20;

            public float font = 16.0F;
        }
        public static Variable v = new Variable();

        // kodex (price, program buy, others buy, retail buys)
        // for kospi leverage inverse and kosdaq leverage and inverse
        public static double[,] kodex_magnifier = new double[4, 4];
    }
}




