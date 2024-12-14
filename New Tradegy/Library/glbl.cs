using MathNet.Numerics.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace New_Tradegy.Library
{
    public class g
    {

        public static int cancelThreshhold = 5;
        public static int postInterval = 30;
        public static Dictionary<string, object> jpjds = 
            new Dictionary<string, object>();

        public static bool workingday = false;

        public static char PeoridNews = 'w';
            public static string Account;
        public static string MachineName;
        public static bool shortform;
        public static int 전일종가이상 = 1000;
        

        public static bool _checkedTradeInit = false;

        public static bool connected = false;

        public static bool optimumTrading = false;

        public static int screenHeight;
        public static int screenWidth;

        public static int date;
        public static DateTime savedTime;

        public static bool test; // use given date

        public static bool 시초 = true; // at the begining of market, try all stocks first and then use the order of evaluation
        //public static bool 돌파적용 = false;

        public static bool draw_history_forwards = false;

        //public static bool timer_첵_first = true;
        //public static bool draw_kodex_inverse = false;
        public static bool click_trade = false;
        public static bool confirm_buy = true;
        public static bool add_interest = false;
        public static bool confirm_sell = false;
        public static bool draw_stock_shrink_or_not = false;
        public static bool settingBuy = false;
        public static bool settingSell = false;

        public static int deal_profit = 0;
        public static int deal_total_profit = 0;
        public static int deal_finish_time = 0;
        public static int deal_maximum_loss = 0;

        public static string str;

        //public static double box_percentage = 45;
        //public static double non_box_half_percentage; 

        public class OrderItem
        {
            public string stock; // 종목
            public string m_sCode; // 종목코드
            public string buyorSell; // 매수, 매도, 보유

            public int m_ordKey; // 주문번호
            public int m_ordOrgKey; // 원주문번호

            // public string m_sText;
            public int m_nAmt; // 주문수량
            public int m_nContAmt; // 체결수량
            public int m_nPrice; //주문단가
           // public string m_sCredit;
            public int m_nModAmt; // 접수시 주문수량 = 정정취소 가능수량

            public string m_sHogaFlag; // 주문호가구분코드내용
        }
        public static Hashtable m_mapOrder;

        public static Color[] Colors = new Color[]
            {
                Color.FromArgb(255, 200, 255),    // 보통 빨강
                Color.FromArgb(255, 200, 200),   // 옅은 빨강
                Color.FromArgb(200, 255, 255),   // 보통 주황
                Color.FromArgb(200, 255, 200),   // 옅은 주황
                Color.FromArgb(255, 255, 200),   // 보통 노랑
                Color.FromArgb(200, 200, 255), // 옅은 노랑
               
            };






        public static int 일회거래액 = 0;
        public static readonly object lockObject = new object(); // Make sure it's initialized
        public static int check_time = 0;
        public string start_time = "00:00:00";


        public static int MAX_ROW = 382; // XX 382 -> 500
        public static int saved_nrow;

        public class w
        {
            public int 가격, 프돈, 외돈, 기관;
        }
        public static w width = new w();

        public class form_size
        {
            public int ch = 28;
        

            public static int h = 28;
            
     
            public int 상단_y = 0;
            public int 호가_ht = h  * 12;
    
            public int 제어_y = h * 12;
            public int 제어_ht = h * 3;
   
            public int 매매_y = h * 15;
            public int 매매_ht = h * 7;
     
            public int 그룹_y = h * 22;
            public int 그룹_ht = h * 3;
    
            public int 하단_y = h * 25;
            public int 호가폭감소 = 10;
            
        }
        public static form_size formSize = new form_size();

        public class dgvClass
        {
            public string stock;
            public string code;
            public int index;
            public DataTable dtb;
            public DataGridView dgv;
            public int[,] itb = new int[21, 3];
        }
        public static List<dgvClass> 호가 = new List<dgvClass>();
        public static dgvClass 매매 = new dgvClass();
        public static dgvClass 그룹 = new dgvClass();
        public static dgvClass 제어 = new dgvClass();
        //public static dgvClass test = new dgvClass();



        // list of variables``
        public static int revolving_number_for_kospi = 0; // used in misc : revoling_naver(int kospi_or_kosdaq) - not used
        public static int revolving_number_for_kosdaq = 0; // used in misc : revoling_naver(int kospi_or_kosdaq) - not used

        //public static double 일중거래액환산율;
        public static double 천만원 = 10000000.0;
        public static double 억원 = 100000000.0;
        public static Chart chart1;
        public static Chart chart2;

        public static char device = 'S'; // S Samsung, L Lg, C B2, c small notebookH

        //public static string key_string = "총점";
        // public static string saved_key_string = "";
        public static char current_key_char;
        public static char previousKeyChar;


        public static string clickedStock = "";
        public static string savedClickedStock = "";
        //public static int numberSameClickedStock = 0;
        public static string clickedTitle = "";
        public static string saved_clickedTitle = "";


        public static string q;
        public static string saved_q;
        //public static string saved_check_q;
        //public static string saved_working_q;



       


        //public static List<string> ogl = new List<string>();   // total set of single stock list
        public static List<g.stock_data> ogl_data = new List<g.stock_data>();
        public static List<string> sl = new List<string>();   // selected single stock list from g.sl cal.
        public static List<string> dl = new List<string>();   // selected stocks for display



        public static List<List<string>> oGL = new List<List<string>>(); // large group set list
        //public static List<string> oGL_title = new List<string>();


       




        public static List<List<string>> DL = new List<List<string>>(); // temporary working space for group list
        public static string oGl_data_selection = "총점"; // initial value

        public static List<string> 보유종목 = new List<string>();
        public static string 급락종목 = "";
        public static List<string> 호가종목 = new List<string>();
        public static List<string> 관심종목 = new List<string>();
        public static List<string> 파일관심종목 = new List<string>();
        public static List<string> 상관절친종목 = new List<string>();

        public class group_data
        {
            public string title;
            public List<string> stocks = new List<string>();

            public double 총점, 수평, 강평;
            public double 프누, 종누, 거분, 배합, 푀분, 배차, 가증, 분거, 상순, 저순;
            public double average_price;
        }
        public static List<g.group_data> oGL_data = new List<group_data>();

        public static Dictionary<string, int> 관심삭제 = new Dictionary<string, int>();
        public static List<string> KODEX4 = new List<string>();   // 클릭된 종목, Toggle로 선택 & 취소

        // public static List<string> 평불종목 = new List<string>();   // 매수된 종목

       
        public static List<string> 지수종목 = new List<string>();
        public static List<string> 코스피History = new List<string>();   // 매수된 종목
        public static List<string> 코스닥History = new List<string>();   // 클릭된 종목, Toggle로 선택 & 취소

        public class kospi_kosdaq_mixed
        {
            public List<string> stock = new List<string>();
            public List<double> weight = new List<double>();
        }
        public static kospi_kosdaq_mixed kospi_mixed = new kospi_kosdaq_mixed();
        public static kospi_kosdaq_mixed kosdaq_mixed = new kospi_kosdaq_mixed();

        public static List<List<string>> aV = new List<List<string>>(); // 평균거래량








        public static int 틱_array_size = 60; //
        public static int 분_array_size = 15; // 
        public static int stocks_per_marketeye = 200;
        //public static int marketeye_sleep_seconds = 1000;

        // public static int setting_text_count = 0;


        public static int kospi_value = -3000;
        public static int kosdq_value = -3000;
        public static int kospi_was1호가 = 0;
        public static int kosdaq_was1호가 = 0;
        public static int timeKospiSouned = 0;
        public static int priceKospiSounded = 0;
        public static int timeKosdaqSouned = 0;
        public static int priceKosdaqSounded = 0;



        public static int ogl_data_next = 0;
        public static int 예치금 = 0;

        public static long 코스피매수배 = 0;
        public static long 코스피매도배 = 0;
        public static long 코스피프외순매수 = 0;
        public static long 코스피개인순매수 = 0;
        public static long 코스피외인순매수 = 0;
        public static long 코스피기관순매수 = 0;
        public static long 코스피금투순매수 = 0;
        public static long 코스피연기순매수 = 0;

        public static long 코스닥매수배 = 0;
        public static long 코스닥매도배 = 0;
        public static long 코스닥프외순매수 = 0;
        public static long 코스닥개인순매수 = 0;
        public static long 코스닥외인순매수 = 0;
        public static long 코스닥기관순매수 = 0;
        public static long 코스닥금투순매수 = 0;
        public static long 코스닥연기순매수 = 0;

        public static int 코스피지수 = 0;
        public static int 코스닥지수 = 0;
        public static float 상해종합지수 = 0;
        public static float 항생지수 = 0;
        public static float 니케이지수 = 0;

        public static float SP_지수 = 0;
        public static float Nasdaq_지수 = 0;




        public static int window_x_size;
        public static int window_y_size;

        public static int rqwey_nCol = 10;
        public static int rqwey_nRow = 3;



        public static int marketeye_count = 0;
        public static int marketeye_count_draw_tick = 0;
        public static int minuteSaveAll = 0;
        public static int alamed_hhmm = 0;




        public static int moving_reference_date = 0;
        public static int saved_date = 0;
        public static int saved_hs_date = 0;
        public static List<int> date_list = new List<int>();
        public static int[,] time_list = new int[100, 2];
        public static int[] time = new int[2];
        //public static int saved_time = 0;
        public static int[] saved_time = new int[2];

        public static string saved_stock;
        public static int[,] eval_score = new int[10, 12];

        public static int end_time_before_advance;
        public static bool end_time_extended;

        public static int nCol = 10;
        public static int nRow = 3;

        public static int gid;
        public static int Gid;
        public static int Group_ranking_Gid;
        public static int saved_Gid;
        public static int draw_selection = 1;
        public static int npts_fi_dwm = 40;
        public static int draw_shrink_time = 30;
        public static int npts_for_magenta_cyan_mark = 4;

        //public static int money_shift = 2;


        public static double EPS = 0.0000001;
        public static double HUNDRED = 100.0;
        public static double THOUSAND = 1000.0;
        public static double TEN = 10.0;
        //public static double 일중거래액환산율;

        public static double[] 누적 = new double[60 * 7];

        public static double[] kospi_틱매수배 = new double[틱_array_size]; // 
        public static double[] kospi_틱매도배 = new double[틱_array_size]; // 
        public static double[] kosdaq_틱매수배 = new double[틱_array_size]; // 
        public static double[] kosdaq_틱매도배 = new double[틱_array_size]; // 

        public static double[] screenFactor = new double[2];


        public static string saved_clickedStock;
        public static double saved_col_percentage;



        public class variable
        {
            // public int 점수선택 = 0; // 0 : 푀분 > 0 , 배차 > 0, 1 : 푀분 > 0, 2 : 배차 > 0, 3 : 푀분 + 배차 > 0, 4: no 선택

            public string key_string = "총점";
            public string SpfKeyString = "그순";
            public string old_key_string = "푀분";

            public double old_분당거래액이상_천만원;// 분거
            public double old_호가거래액_백만원; // not active for g.tesing 호가
            public double old_편차이상; // 편차
            public double old_배차이상; // defined, but not used // 배차
            public double old_종가기준추정거래액이상_천만원; // 종거
            public double old_시총이상; // 시총
            public int old_배플;
            public int old_푀플;

            public double 분당거래액이상_천만원; // in setting 10
            public double 호가거래액_백만원; // in setting 10
            public double 편차이상;  // in setting 1
            public double 배차이상; // in setting 0, not used
            public double 종가기준추정거래액이상_천만원; // insetting 0
            public double 시총이상; // in setting 0

            public int 푀플 = 1;
            public int 배플 = 1;



            public int 보유종목점검최소액 = 9; // 만원으로 확인하도록 if(o.보유량 * o.매수1호가 / 10000.0 > g.v.보유종목점검최소액)
            public int 비상매도시손실율 = -2;

            public double 수급과장배수 = 20; // 수급 과장하여 표시하는 배수

            public double 배수과장배수 = 1.0; // 수급 과장하여 표시하는 배수, p & P key control the value

            //public string[] files_to_open_by_clicking_edge = new string[8];   // selected stocks for display

            public int q_advance_lines = 15;
            public int Q_advance_lines = 150;
            public int r3_display_lines = 20;

            // old version EvalKODEX()
            public int[] kospi_difference_for_sound = new int[3];
            public int[] kosdq_difference_for_sound = new int[3];

            // new version eval_index()
            public int[] index_difference_sound = new int[3];


            //public double 틱프돈천이상 = 10;
            //public double 분프로천푀퍼티지이상 = 50;

            public int columnsofoGl_data;

            public int eval_per_marketeyes;

            public int neglectable_price_differ = 0; // 가격 상승 피로도 계산시 무시하는 가격 차이

            public float font = 16.0F;


            public int Screens;
        }
        public static variable v = new variable();


        //public class kodex_magnifier_shifter
        //{
        //    //public int[,] shifter = new int[4, 3];
        //    // price, money, US
        //    public double[,] magnifier = new double[4, 3];
        //    // price, money, U
        //    public double[,] max_min = new double[4, 2];
        //    // i = 0 KODEX 레버리지, i = 1 KODEX 200선물인버스2X
        //    // i = 2 KODEX 코스닥150레버리지, i = 3 KODEX 코스닥150레버리지

        //    public double saved_row_percentage = 0.0;
        //    public DateTime savedTime;
        //    public int index_magnifier_shifter;
        //}
        //public static kodex_magnifier_shifter k = new kodex_magnifier_shifter();

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


 
        public class stock_data
        {
            public class t
            {
                public int array_count = 0;

                public double 틱프로잠정합_천 = 0.0;
                public double 틱외인잠정합_천 = 0.0;

                public double[] 틱프로천 = new double[틱_array_size];
                public double[] 틱외인천 = new double[틱_array_size];
                public int[] 틱의가격 = new int[틱_array_size];
                public int[] 틱의시간 = new int[틱_array_size];
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
            }
            public ux pass = new ux();

            public class d
            {
                public int upperPassingPrice = 0;
                public int lowerPassingPrice = 0;
            }
            public d deal = new d();

            public string stock;
    

            public bool shrink_draw = false;

            //public bool in_group_or_not = false;

            public List<string> 절친 = new List<string>();

            public bool included = false;
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

            public long 매도1호가; // 8 long
            public long 매수1호가; // 9 long

            public ulong 거래량; // 10 ulong
            //public ulong 거래액_원; // 11 ulong
            public double 전일거래액_천만원; // marketeye not provide, calculated from "일"

            //public char 장구분; // 12 char '0' 장전 '1' 동시호가 '2' 장중
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

            public string dev_avr;
            public double avr;
            public double dev;

            public int max_dealt; // in testing, these are diplayed in textBox 10, 11,, 12 etc
            public int min_dealt;
            public int avr_dealt;
            public ulong 일평균거래량;
            public int 일평균거래액;
            public int 일평균푀돈액;

            public char 시장구분; // P 코스피, D 코스닥

            public int nrow = 0;
            public int[,] x = new int[MAX_ROW, 12];

            // 틱 데이터
            public int[] 틱의시간 = new int[틱_array_size]; // 틱의시간    // 호가창 tT
            public int[] 틱의가격 = new int[틱_array_size]; // 틱의가격    // 호가창
            public int[] 틱의수급 = new int[틱_array_size]; // 틱의수급
            public int[] 틱의체강 = new int[틱_array_size]; // 틱의체강

            public int[] 틱매수량 = new int[틱_array_size]; // 틱매수량
            public int[] 틱매도량 = new int[틱_array_size]; // 틱매도량
            public int[] 틱매수배 = new int[틱_array_size]; // 틱매수배
            public int[] 틱매도배 = new int[틱_array_size]; // 틱매도배

            public int[] 틱배수차 = new int[틱_array_size];  // 틱배수차
            public int[] 틱배수합 = new int[틱_array_size];  // 틱배수합
            public int[] 틱프외퍼 = new int[틱_array_size];  // 틱프외퍼

            public int[] 틱프로량 = new int[틱_array_size]; // 틱프로량  
            public double[] 틱프로천 = new double[틱_array_size]; // 틱프돈천

            public int[] 틱외인량 = new int[틱_array_size]; // 틱외인량
            public double[] 틱외인천 = new double[틱_array_size]; // 틱외돈천                  //

            public double[] 틱거래천 = new double[틱_array_size]; // 틱거돈천

            public int[] 틱매도잔 = new int[틱_array_size]; // 최우선매도호가잔량
            public int[] 틱매수잔 = new int[틱_array_size]; // 최우선매수호가잔량


            // 분 데이터
            public double[] 분프로천 = new double[분_array_size]; // 분프로천
            public double[] 분외인천 = new double[분_array_size]; // 분외인천
            public double[] 분거래천 = new double[분_array_size]; // 분거래천
            public int[] 분매수배 = new int[분_array_size]; // 분매수배
            public int[] 분매도배 = new int[분_array_size]; // 분매도배
            public int[] 분배수차 = new int[분_array_size];  // 분배수차
            public int[] 분배수합 = new int[분_array_size];  // 분배수차

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

            public stock_data ShallowCopy()
            {
                return (stock_data)this.MemberwiseClone();
            }


            public stock_data DeepCopy()
            {
                stock_data clone = (stock_data)this.MemberwiseClone();

                // Deep copy nested classes
                clone.변곡 = new t
                {
                    array_count = this.변곡.array_count,
                    틱프로잠정합_천 = this.변곡.틱프로잠정합_천,
                    틱외인잠정합_천 = this.변곡.틱외인잠정합_천,
                    틱프로천 = (double[])this.변곡.틱프로천.Clone(),
                    틱외인천 = (double[])this.변곡.틱외인천.Clone(),
                    틱의가격 = (int[])this.변곡.틱의가격.Clone(),
                    틱의시간 = (int[])this.변곡.틱의시간.Clone()
                };

                // Manually deep copy pass object
                clone.pass = new ux
                {
                    previousPriceHigh = this.pass.previousPriceHigh,
                    previousPriceLow = this.pass.previousPriceLow,
                    priceStatus = this.pass.priceStatus,
                    previousProgramHigh = this.pass.previousProgramHigh,
                    previousProgramLow = this.pass.previousProgramLow,
                    programStatus = this.pass.programStatus,
                    monthStatus = this.pass.monthStatus,
                    quarterStatus = this.pass.quarterStatus,
                    halfStatus = this.pass.halfStatus,
                    yearStatus = this.pass.yearStatus,
                    month = this.pass.month,
                    quarter = this.pass.quarter,
                    half = this.pass.half,
                    year = this.pass.year
                };

                clone.deal = new d
                {
                    upperPassingPrice = this.deal.upperPassingPrice,
                    lowerPassingPrice = this.deal.lowerPassingPrice
                };

                clone.점수 = new score
                {
                    dev = this.점수.dev,
                    mkc = this.점수.mkc,
                    avr = this.점수.avr,
                    돌파 = this.점수.돌파,
                    눌림 = this.점수.눌림,
                    가연 = this.점수.가연,
                    가분 = this.점수.가분,
                    가틱 = this.점수.가틱,
                    가반 = this.점수.가반,
                    가지 = this.점수.가지,
                    가위 = this.점수.가위,
                    수연 = this.점수.수연,
                    수지 = this.점수.수지,
                    수위 = this.점수.수위,
                    강연 = this.점수.강연,
                    강지 = this.점수.강지,
                    강위 = this.점수.강위,
                    푀분 = this.점수.푀분,
                    프틱 = this.점수.프틱,
                    프지 = this.점수.프지,
                    프퍼 = this.점수.프퍼,
                    프누 = this.점수.프누,
                    거분 = this.점수.거분,
                    거틱 = this.점수.거틱,
                    거일 = this.점수.거일,
                    배차 = this.점수.배차,
                    배반 = this.점수.배반,
                    배합 = this.점수.배합,
                    급락 = this.점수.급락,
                    급상 = this.점수.급상,
                    스물 = this.점수.스물,
                    그룹 = this.점수.그룹,
                    동시 = this.점수.동시,
                    총점 = this.점수.총점,
                    그순 = this.점수.그순
                };

                clone.통계 = new statics
                {
                    프분_count = this.통계.프분_count,
                    프분_avr = this.통계.프분_avr,
                    프분_dev = this.통계.프분_dev,
                    거분_avr = this.통계.거분_avr,
                    거분_dev = this.통계.거분_dev,
                    배차_avr = this.통계.배차_avr,
                    배차_dev = this.통계.배차_dev,
                    배합_avr = this.통계.배합_avr,
                    배합_dev = this.통계.배합_dev
                };

                clone.정도 = new level
                {
                    돌파 = this.정도.돌파,
                    눌림 = this.정도.눌림,
                    가반 = this.정도.가반,
                    가지 = this.정도.가지,
                    강지 = this.정도.강지,
                    배반 = this.정도.배반,
                    프퍼 = this.정도.프퍼,
                    푀퍼 = this.정도.푀퍼,
                    프지 = this.정도.프지,
                    프가 = this.정도.프가,
                    급락 = this.정도.급락,
                    잔잔 = this.정도.잔잔
                };

                // Deep copy arrays
                clone.틱의시간 = (int[])this.틱의시간.Clone();
                clone.틱의가격 = (int[])this.틱의가격.Clone();
                clone.틱의수급 = (int[])this.틱의수급.Clone();
                clone.틱의체강 = (int[])this.틱의체강.Clone();

                clone.틱매수량 = (int[])this.틱매수량.Clone();
                clone.틱매도량 = (int[])this.틱매도량.Clone();
                clone.틱매수배 = (int[])this.틱매수배.Clone();
                clone.틱매도배 = (int[])this.틱매도배.Clone();

                clone.틱배수차 = (int[])this.틱배수차.Clone();
                clone.틱배수합 = (int[])this.틱배수합.Clone();
                clone.틱프외퍼 = (int[])this.틱프외퍼.Clone();

                clone.틱프로량 = (int[])this.틱프로량.Clone();
                clone.틱프로천 = (double[])this.틱프로천.Clone();

                clone.틱외인량 = (int[])this.틱외인량.Clone();
                clone.틱외인천 = (double[])this.틱외인천.Clone();

                clone.틱거래천 = (double[])this.틱거래천.Clone();

                clone.틱매도잔 = (int[])this.틱매도잔.Clone();
                clone.틱매수잔 = (int[])this.틱매수잔.Clone();

                clone.분프로천 = (double[])this.분프로천.Clone();
                clone.분외인천 = (double[])this.분외인천.Clone();
                clone.분거래천 = (double[])this.분거래천.Clone();
                clone.분매수배 = (int[])this.분매수배.Clone();
                clone.분매도배 = (int[])this.분매도배.Clone();
                clone.분배수차 = (int[])this.분배수차.Clone();
                clone.분배수합 = (int[])this.분배수합.Clone();

                // Deep copy multi-dimensional array
                clone.x = new int[MAX_ROW, 12];
                for (int i = 0; i < MAX_ROW; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        clone.x[i, j] = this.x[i, j];
                    }
                }

                // Deep copy list
                clone.절친 = new List<string>(this.절친);

                return clone;
            }
        }
    }
}

