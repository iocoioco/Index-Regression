using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library;
using static New_Tradegy.Library.g;
using static New_Tradegy.Library.g.stock_data;


#region
//Stopwatch stopwatch = new Stopwatch(); 
//stopwatch.Stop();
//elapsedTime += stopwatch.ElapsedMilliseconds;
#endregion

//draw__stock      0.33 sec

//KeyPress(chart)	'q'(example)	0.35 sec	
//	g.sl calculation		0.02 sec


//MouseClick	r1		0.04 sec
//		r2		0.04sec
//		r3		0.06 sec
//		r4		0.04 sec
//		r5		0.03 sec
//		r6		0.40 sec
//		r7
//		r8		0.06 sec
//		r9		0.34 sec (g.sl cal. & draw_stock)

//dataGridView2Update 		0.1sec

namespace New_Tradegy.Library
{
    public class ev
    {
        // called by timer_draw_Tick, g.제어.dgv_CellClick, chart1_click, keys, TimerCheckTick

        public static void eval_stock_등합() // 12 MilliSeconds
        {
            var a_tuple_푀분 = new List<Tuple<double, string>> { };
            var a_tuple_거분 = new List<Tuple<double, string>> { };
            var a_tuple_배차 = new List<Tuple<double, string>> { };
            var a_tuple_배합 = new List<Tuple<double, string>> { };
            foreach (var o in g.ogl_data)
            {
                a_tuple_푀분.Add(Tuple.Create(o.점수.푀분, o.stock));
                a_tuple_거분.Add(Tuple.Create(o.점수.거분, o.stock));
                a_tuple_배차.Add(Tuple.Create(o.점수.배차, o.stock));
                a_tuple_배합.Add(Tuple.Create(o.점수.배합, o.stock));
            }
            if (!g.q.Contains("&g")) // in group no evaluation 
            {
                a_tuple_푀분 = a_tuple_푀분.OrderByDescending(t => t.Item1).ToList();
                a_tuple_거분 = a_tuple_거분.OrderByDescending(t => t.Item1).ToList();
                a_tuple_배차 = a_tuple_배차.OrderByDescending(t => t.Item1).ToList();
                a_tuple_배합 = a_tuple_배합.OrderByDescending(t => t.Item1).ToList();
            }
            foreach (var o in g.ogl_data)
            {
                o.점수.푀분_등수 = a_tuple_푀분.FindIndex(s => s.Item2 == o.stock);
                o.점수.거분_등수 = a_tuple_거분.FindIndex(s => s.Item2 == o.stock);
                o.점수.배차_등수 = a_tuple_배차.FindIndex(s => s.Item2 == o.stock);
                o.점수.배합_등수 = a_tuple_배합.FindIndex(s => s.Item2 == o.stock);
            }
            // define weights to each evaluation item, 0.5, 0.3, 0.1, 0.1 find all stocks and sort them by the sum of the weights ascending order  
            var a_tuple_등합 = new List<Tuple<double, string>> { };
            foreach (var o in g.ogl_data)
            {
                double value = 0.0;
                o.점수.등합 = o.점수.푀분_등수 +
                             //o.점수.거분_등수 +
                             o.점수.배차_등수 +
                             o.점수.배합_등수; // +
                                         //o.점수.그순 * g.s.그룹_wgt; //\
            }
        }

        public static void eval_stock() // 12 MilliSeconds
        {
            var a_tuple = new List<Tuple<double, string>> { };
            double value = 0.0;

            var group = new HashSet<string> { "닥올", "피올", "편차", "평균" };

            if (group.Contains(g.v.KeyString))
            {

                foreach (var o in g.ogl_data)
                {
                    switch (g.v.KeyString)
                    {
                        case "피올":
                            if (o.시장구분 == 'S')
                            {
                                value = (double)o.시총;
                                a_tuple.Add(Tuple.Create(value, o.stock));
                            }
                                
                            break;
                        case "닥올":
                            if (o.시장구분 == 'D')
                            {
                                value = (double)o.시총;
                                a_tuple.Add(Tuple.Create(value, o.stock));
                            }
                                
                            break;
                        case "편차":
                            value = o.dev;
                            a_tuple.Add(Tuple.Create(value, o.stock));
                            break;
                        case "평균":
                            value = o.avr;
                            a_tuple.Add(Tuple.Create(value, o.stock));
                            break;
                    }
                    
                }
            }
            else
            {

                foreach (var o in g.ogl_data)
                {
                    ps.post(o);
                    // ps.PostPassing(o, o.nrow - 1, true); // marketeye_received() real
                    int check_row = 0;
                    if (g.test)
                    {
                        check_row = g.Npts[1] - 1;
                        if (check_row > o.nrow - 1)
                            check_row = o.nrow - 1;
                    }
                    else
                        check_row = o.nrow - 1;

                    if (!eval_inclusion(o) || o.nrow < 2) continue;
                    switch (g.v.KeyString)
                    {
                        case "총점":
                            value = o.점수.총점;
                            break;
                        case "프누":
                            value = o.프누천 + o.외누천;
                            break;
                        case "종누":
                            value = o.종거천;
                            break;
                        case "프편":
                            value = (o.프누천 + o.외누천) / o.통계.프분_dev;
                            break;
                        case "종편":
                            value = o.종거천 / o.통계.프분_dev;
                            break;
                        case "푀분":// 분당프로그램(천만원)
                            value = o.분프로천[0] + o.분외인천[0];// 20220720;
                            break;
                        case "배차": // 배수차
                            value = o.분배수차[0];
                            break;
                        case "가증": // price jump
                            value = o.x[check_row, 1] - o.x[check_row - 1, 1];
                            break;
                        case "분거": // 분거천 순서
                            value = o.분거래천[0];
                            break;

                        case "상순": // higher price order
                            value = o.x[check_row, 1];
                            break;
                        case "저순": // lower price order
                            value = o.x[check_row, 1] * -1.0;
                            break;
                    }
                    a_tuple.Add(Tuple.Create(value, o.stock));
                }
            }

            a_tuple = a_tuple.OrderByDescending(t => t.Item1).ToList();

            lock (g.lockObject)
            {
                g.sl.Clear();

                foreach (var item in a_tuple)
                {
                    if (!g.sl.Contains(item.Item2))
                    {
                        g.sl.Add(item.Item2);
                    }
                }
                string newValue = g.sl.Count.ToString() + "/" + g.ogl_data.Count.ToString();
                if (g.제어.dtb.Rows[1][1].ToString() != newValue)
                {
                    g.제어.dtb.Rows[1][1] = newValue;
                }
            }
            ev.eval_group();
        }

        public static bool eval_inclusion(g.stock_data o)
        {
            // 레버리지 외 지수관련 및 보유종목, 호가종목, 관심종목  continue;
            if ((g.KODEX4.Contains(o.stock) && !o.stock.Contains("레버리지")) ||
                o.stock.Contains("KODEX") ||
                o.stock.Contains("KOSEF") ||
                o.stock.Contains("HANARO") ||
                o.stock.Contains("TIGER") ||
                o.stock.Contains("KBSTAR") ||
                o.stock.Contains("혼합") ||
                g.보유종목.Contains(o.stock) ||
                g.호가종목.Contains(o.stock) ||
                g.관심종목.Contains(o.stock))
            {
                return false;
            }

            if (g.v.KeyString == "프누" ||
                g.v.KeyString == "종누" ||
                g.v.KeyString == "프편" ||
                g.v.KeyString == "종편" 
            )
                return true;
            
            if (g.v.푀플 == 1 && o.점수.푀분 < 0)
                return false;
            if (g.v.배플 == 1 && o.점수.배차 < 0)
                return false;

            // 누적거래액 이상
            if (g.v.종가기준추정거래액이상_천만원 > (int)o.종거천)
                return false;


            if (wk.isWorkingHour())
            {
                // 호가거래액이상
                if (o.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
                    o.매수호가거래액_백만원 < g.v.호가거래액_백만원)
                    return false;
                // 분당 추정거래대금
                if (g.v.분당거래액이상_천만원 > o.분거래천[0]) // && o.x[o.nrow -1, 0] > 90100)
                    return false;
            }

            // 편차 일정 이하 및 배수차 일정 이하 -> 제외
            if (g.v.편차이상 >= o.dev) // && g.v.배차이상 > o.분배수차[0])
                return false;

            // 시총 이하 제외 또는 시총 이상 제외
            if (g.v.시총이상 >= 0)
            {
                if (o.시총 < g.v.시총이상 - 0.01)
                    return false;
            }
            else
            {
                if (o.시총 > (g.v.시총이상 - 0.01) * -1.0)
                    return false;
            }
            return true;
        }

        public static void EvalKODEX(g.stock_data o)//, int[,] x) 20240130
        {
            if (o.stock == "KODEX 레버리지")
            {
                string sound = "";
                int priceDifference = o.x[o.nrow - 1, 1] - g.priceKospiSounded;

                if (Math.Abs(o.매수1호가 - g.kospi_was1호가) > 0)
                {
                    int HHmmssfff = Convert.ToInt32(DateTime.Now.ToString("HHmmssfff")); // run_marketeye() 
                    string str = HHmmssfff.ToString() + " ";
                    for (int k = 1; k < 12; k++) // bound exist not the size
                    {
                        str += "\t" + o.x[o.nrow - 1, k];
                    }
                    g.코스피History.Add(str);
                    g.kospi_was1호가 = (int)o.매수1호가;
                }

                // from (-5, 5) to (-10, 10) 20240831
                if (priceDifference > -10 && priceDifference < 10)
                    return;
                if (priceDifference <= -15)
                    sound = "p d d d";
                else if (priceDifference <= -10)
                    sound = "p d d";
                else if (priceDifference <= -5)
                    sound = "p d";
                else if (priceDifference < 10)
                    sound = "p u";
                else if (priceDifference < 15)
                    sound = "p u u";
                else
                    sound = "p u u u";

                g.priceKospiSounded = o.x[o.nrow - 1, 1];

                mc.Sound("코스피 코스닥", sound);
            }
            if (o.stock == "KODEX 코스닥150레버리지")
            {
                string sound = "";
                int priceDifference = o.x[o.nrow - 1, 1] - g.priceKosdaqSounded;

                if (Math.Abs(o.매수1호가 - g.kosdaq_was1호가) > 0)
                {
                    int HHmmssfff = Convert.ToInt32(DateTime.Now.ToString("HHmmssfff"));
                    string str = HHmmssfff.ToString() + " ";
                    for (int k = 1; k < 12; k++) // bound exist not the size
                    {
                        str += "\t" + o.x[o.nrow - 1, k];
                    }
                    g.코스닥History.Add(str);
                    g.kosdaq_was1호가 = (int)o.매수1호가;
                }

                if (priceDifference > -10 && priceDifference < 10)
                    return;
                if (priceDifference <= -15)
                    sound = "q d d d";
                else if (priceDifference <= -10)
                    sound = "q d d";
                else if (priceDifference <= -5)
                    sound = "q d";
                else if (priceDifference < 10)
                    sound = "q u";
                else if (priceDifference < 15)
                    sound = "q u u";
                else
                    sound = "q u u u";

                g.priceKosdaqSounded = o.x[o.nrow - 1, 1];

                mc.Sound("코스피 코스닥", sound);
            }
        }

        public static void eval_group() // duration 0.009 seconds
        {
            // 그룹 프돈 유입 또는 그룹 전체 가증, 후사면일 경우 또는 그룹 약할 때 프돈 지속 안되는 경우
            // 단독 한항우 시초 프외 유입 및 사이안 & 마젠타
            // 배수가 움직이는 종목 공략
            // 개인은 프돈을 보지 못 해서 프돈이 팔아도 일단 히트업된 종목은 조금 더 가므로 매도 서둘 필요 없음
            // 상순, 가증, 프외, 거일, 거분 ... 거분이 없으면 상승 곤란, 호가창 빈약도 의미없음

            // List<string> used_stocks = new List<string>();
            for (int i = 0; i < g.oGL_data.Count; i++) // 각 그룹 종목 총점 순서 후 상위 3개 종목으로 그룹 순위 결정
            {
                g.oGL_data[i].총점 = 0.0;
                g.oGL_data[i].총점 = 0.0;
                g.oGL_data[i].푀분 = 0.0;
                g.oGL_data[i].종누 = 0.0;

                g.oGL_data[i].수평 = 0.0;
                g.oGL_data[i].강평 = 0.0;
                g.oGL_data[i].가증 = 0.0;


                wk.거분순서(g.oGL_data[i].stocks);

                int maximum_count = 0;
                foreach (var stock in g.oGL_data[i].stocks)
                {
                    if (maximum_count == 3)
                        break;

                    // if (stock == "삼성전자" || stock == "SK하이닉스") continue; // 20240728 blocked
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                        continue;

                    g.stock_data o = g.ogl_data[index];

                    int CheckNpts = 0;
                    if (g.test)
                        CheckNpts = g.Npts[1] - 1;
                    else
                        CheckNpts = o.nrow - 1;

                    if (CheckNpts < 1)
                        return;
                    if (CheckNpts >= 382)
                        CheckNpts = 381;

                    if (o.x[CheckNpts, 1] < -3000 || o.x[CheckNpts, 1] > 3000)
                        continue;

                    g.oGL_data[i].총점 += o.점수.총점;
                    g.oGL_data[i].거분 += o.점수.거분;
                    g.oGL_data[i].푀분 += o.점수.푀분;
                    g.oGL_data[i].수평 += o.x[CheckNpts, 2];
                    g.oGL_data[i].강평 += o.x[CheckNpts, 3] / 100;
                    g.oGL_data[i].가증 += o.x[CheckNpts, 1] - o.x[CheckNpts - 1, 1];

                    maximum_count++;
                }
                g.oGL_data[i].총점 /= maximum_count;
                g.oGL_data[i].거분 /= maximum_count;
                g.oGL_data[i].푀분 /= maximum_count;
                g.oGL_data[i].수평 /= maximum_count;
                g.oGL_data[i].강평 /= maximum_count;
                g.oGL_data[i].가증 /= maximum_count;
            }

            // var newList = list.OrderBy(x => x.Product.Name).ToList(); // ascending
            // var newList = list.OrderBy(x => x.Product.Name).Reverse()

            // 총점, 푀분, 종누(그룹 순위)
            if (g.oGl_data_selection == "총점") // 1st column click
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.총점).ToList();
            else if (g.oGl_data_selection == "푀분") // 2nd column click
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.푀분).ToList();
            else if (g.oGl_data_selection == "가증") // 3rd column click
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.가증).ToList();

            // 각 종목에 그룹 순위 부여 -> draw_stock에서 chart coloring 사용
            for (int i = g.oGL_data.Count - 1; i >= 0; i--)
            {
                foreach (var stock in g.oGL_data[i].stocks)
                {
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0) continue;
                    g.ogl_data[index].점수.그순 = i;
                }
            }

            // 표에 입력
            int display_count = g.oGL_data.Count; 
            if (!g.test)
            {
                if (g.oGL_data.Count > 9)
                {
                    display_count = 9; // g.connected row list 9
                }
            }
            #region

            if (hg.HogaFormNameGivenStock("Form_그룹") != null)
            {
                // Suspend layout to reduce flickering and improve performance
                g.그룹.dgv.SuspendLayout();

                try
                {
                    for (int i = 0; i < display_count; i++)
                    {
                        // Check if the data has actually changed before updating the row
                        bool hasChanged = g.그룹.dtb.Rows[i][0].ToString() != g.oGL_data[i].title ||
                                          g.그룹.dtb.Rows[i][1].ToString() != ((int)g.oGL_data[i].푀분).ToString() ||
                                          g.그룹.dtb.Rows[i][2].ToString() != ((int)g.oGL_data[i].총점).ToString();

                        if (hasChanged)
                        {
                            // Only update if the data has changed
                            g.그룹.dtb.Rows[i][0] = g.oGL_data[i].title;
                            g.그룹.dtb.Rows[i][1] = ((int)g.oGL_data[i].푀분).ToString();
                            g.그룹.dtb.Rows[i][2] = ((int)g.oGL_data[i].총점).ToString();
                        }
                    }
                }
                finally
                {
                    // Resume layout to apply the changes and refresh the display
                    g.그룹.dgv.ResumeLayout();
                }
            }

            //if (hg.HogaFormNameGivenStock("Form_그룹") != null)
            //{
            //    for (int i = 0; i < display_count; i++)
            //    {
            //        g.그룹.dtb.Rows[i][0] = g.oGL_data[i].title;
            //        g.그룹.dtb.Rows[i][1] = ((int)g.oGL_data[i].푀분).ToString();
            //        g.그룹.dtb.Rows[i][2] = ((int)g.oGL_data[i].총점).ToString();
            //    }

            //    // two groups in a row : not used
            //    #region
            //    //g.그룹.dtb.Rows[0][0] = g.ogl_data.Count;
            //    //g.그룹.dtb.Rows[0][1] = g.sl.Count;

            //    // 1st
            //    //g.그룹.dtb.Rows[1][0] = g.oGL_data[0].title;
            //    //g.그룹.dtb.Rows[1][1] = ((int)g.oGL_data[0].총점).ToString() + "/" +
            //    //                               ((int)g.oGL_data[0].푀분).ToString();
            //    // 1nd
            //    //g.그룹.dtb.Rows[2][0] = g.oGL_data[1].title;
            //    //g.그룹.dtb.Rows[2][1] = ((int)g.oGL_data[1].총점).ToString() + "/" +
            //    //                               ((int)g.oGL_data[1].푀분).ToString();
            //    // 3rd
            //    //g.그룹.dtb.Rows[0][2] = g.oGL_data[2].title;
            //    //g.그룹.dtb.Rows[0][3] = ((int)g.oGL_data[2].총점).ToString() + "/" +
            //    //                               ((int)g.oGL_data[2].푀분).ToString();
            //    // 4th
            //    //g.그룹.dtb.Rows[1][2] = g.oGL_data[3].title;
            //    //g.그룹.dtb.Rows[1][3] = ((int)g.oGL_data[3].총점).ToString() + "/" +
            //    //                               ((int)g.oGL_data[3].푀분).ToString();
            //    // 5th
            //    //g.그룹.dtb.Rows[2][2] = g.oGL_data[4].title;
            //    //g.그룹.dtb.Rows[2][3] = ((int)g.oGL_data[4].총점).ToString() + "/" +
            //    //                               ((int)g.oGL_data[4].푀분).ToString();
            //    #endregion
            //}


            #endregion









            #region
            //public static void EvalKODEX_not_using1(g.stock_data o)//, int![,] x) 20240130
            //{
            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKospiSouned == 0)
            //        {
            //            g.timeKospiSouned = o.틱의시간[0];
            //            g.priceKospiSounded = o.x[o.nrow - 1, 5];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKospiSouned, o.틱의시간[0]);

            //        if (elapsed_seconds < 33)
            //        {
            //            return;
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 5] - g.priceKospiSounded;
            //        g.timeKospiSouned = o.틱의시간[0];
            //        g.priceKospiSounded = o.x[o.nrow - 1, 5];

            //        if (priceDifference < -150)
            //            sound = "qm 150";
            //        else if (priceDifference < -130)
            //            sound = "qm 130";
            //        else if (priceDifference < -110)
            //            sound = "qm 110";
            //        else if (priceDifference < -90)
            //            sound = "qm 90";
            //        else if (priceDifference < -70)
            //            sound = "qm 70";
            //        else if (priceDifference < -50)
            //            sound = "qm 50";
            //        else if (priceDifference < -30)
            //            sound = "qm 30";
            //        else if (priceDifference < -10)
            //            sound = "qm 10";
            //        else if (priceDifference < 10)
            //            sound = "qp 10";
            //        else if (priceDifference < 30)
            //            sound = "qp 30";
            //        else if (priceDifference < 50)
            //            sound = "qp 50";
            //        else if (priceDifference < 70)
            //            sound = "qp 70";
            //        else if (priceDifference < 90)
            //            sound = "qp 90";
            //        else if (priceDifference < 110)
            //            sound = "qp 110";
            //        else if (priceDifference < 130)
            //            sound = "qp 130";
            //        else
            //            sound = "qp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKosdaqSouned == 0)
            //        {
            //            g.timeKosdaqSouned = o.틱의시간[0];
            //            g.priceKosdaqSounded = o.x[o.nrow - 1, 1];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKosdaqSouned, o.틱의시간[0]);

            //        if (elapsed_seconds < 31)
            //        {
            //            return;
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 1] - g.priceKosdaqSounded;
            //        g.timeKosdaqSouned = o.틱의시간[0];
            //        g.priceKosdaqSounded = o.x[o.nrow - 1, 1];

            //        if (priceDifference < -150)
            //            sound = "pm 150";
            //        else if (priceDifference < -130)
            //            sound = "pm 130";
            //        else if (priceDifference < -110)
            //            sound = "pm 110";
            //        else if (priceDifference < -90)
            //            sound = "pm 90";
            //        else if (priceDifference < -70)
            //            sound = "pm 70";
            //        else if (priceDifference < -50)
            //            sound = "pm 50";
            //        else if (priceDifference < -30)
            //            sound = "pm 30";
            //        else if (priceDifference < -10)
            //            sound = "pm 10";
            //        else if (priceDifference < 10)
            //            sound = "pp 10";
            //        else if (priceDifference < 30)
            //            sound = "pp 30";
            //        else if (priceDifference < 50)
            //            sound = "pp 50";
            //        else if (priceDifference < 70)
            //            sound = "pp 70";
            //        else if (priceDifference < 90)
            //            sound = "pp 90";
            //        else if (priceDifference < 110)
            //            sound = "pp 110";
            //        else if (priceDifference < 130)
            //            sound = "pp 130";
            //        else
            //            sound = "pp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //}

            //public static bool eval_inclusion_not_using0(g.stock_data o)
            //{
            //    bool true_or_false = false;


            //    // 누적거래액 이상
            //    if (g.v.종가기준추정거래액이상_천만원 > (int)o.종거천)
            //        true_or_false = false;

            //    // real only 
            //    if (g.connected)
            //    {
            //        // 호가거래액이상
            //        if (o.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
            //            o.매수호가거래액_백만원 < g.v.호가거래액_백만원)
            //            true_or_false = false;
            //    }

            //    // 분당 추정거래대금 일정 이하 제외
            //    if (g.v.분당거래액이상_천만원 > o.분거래천[0]) // && o.x[o.nrow -1, 0] > 90100)
            //        true_or_false = false;


            //    // 편차 일정 이하 및 배수차 일정 이하 -> 제외
            //    if (g.v.편차이상 >= o.dev) // && g.v.배차이상 > o.분배수차[0])
            //        true_or_false = false;

            //    // 시총 이하 제외 또는 시총 이상 제외
            //    if (g.v.시총이상 >= 0)
            //    {
            //        if (o.시총 < g.v.시총이상 - 0.01)
            //            true_or_false = false;
            //    }
            //    else
            //    {
            //        if (o.시총 > (g.v.시총이상 - 0.01) * -1.0)
            //            true_or_false = false;
            //    }

            //    if (g.v.KeyString == "피올" ||
            //        g.v.KeyString == "닥올" ||
            //        g.v.KeyString == "프누" ||
            //        g.v.KeyString == "종누" ||
            //        g.v.KeyString == "분거" ||
            //        g.v.KeyString == "가증" ||
            //        g.v.KeyString == "상순" ||
            //        g.v.KeyString == "저순" ||
            //        g.v.KeyString == "편차" ||
            //        g.v.KeyString == "평균")
            //    { // 시장구분
            //        true_or_false |= true;
            //        if (g.v.KeyString == "피올" && o.시장구분 != 'S') // 코스피만 선택 : 코스닥종목 제외
            //        {
            //            true_or_false = false;
            //        }
            //        if (g.v.KeyString == "닥올" && o.시장구분 != 'D') // 코스닥만 선택 : 코스피종목 제외
            //        {
            //            true_or_false = false;
            //        }
            //    }

            //    return true_or_false;
            //}

            //public static void EvalKODEX_not_using0(g.stock_data o)//, int[,] x) 20230818
            //{
            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKospiSouned == 0)
            //        {
            //            g.timeKospiSouned = o.틱의시간[0];
            //            g.priceKospiSounded = o.x[o.nrow - 1, 5];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKospiSouned, o.틱의시간[0]);

            //        if (elapsed_seconds < 33)
            //        {
            //            return;
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 5] - g.priceKospiSounded;
            //        g.timeKospiSouned = o.틱의시간[0];
            //        g.priceKospiSounded = o.x[o.nrow - 1, 5];

            //        if (priceDifference < -150)
            //            sound = "qm 150";
            //        else if (priceDifference < -130)
            //            sound = "qm 130";
            //        else if (priceDifference < -110)
            //            sound = "qm 110";
            //        else if (priceDifference < -90)
            //            sound = "qm 90";
            //        else if (priceDifference < -70)
            //            sound = "qm 70";
            //        else if (priceDifference < -50)
            //            sound = "qm 50";
            //        else if (priceDifference < -30)
            //            sound = "qm 30";
            //        else if (priceDifference < -10)
            //            sound = "qm 10";
            //        else if (priceDifference < 10)
            //            sound = "qp 10";
            //        else if (priceDifference < 30)
            //            sound = "qp 30";
            //        else if (priceDifference < 50)
            //            sound = "qp 50";
            //        else if (priceDifference < 70)
            //            sound = "qp 70";
            //        else if (priceDifference < 90)
            //            sound = "qp 90";
            //        else if (priceDifference < 110)
            //            sound = "qp 110";
            //        else if (priceDifference < 130)
            //            sound = "qp 130";
            //        else
            //            sound = "qp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKosdaqSouned == 0)
            //        {
            //            g.timeKosdaqSouned = o.틱의시간[0];
            //            g.priceKosdaqSounded = o.x[o.nrow - 1, 1];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKosdaqSouned, o.틱의시간[0]);

            //        if (elapsed_seconds < 31)
            //        {
            //            return;
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 1] - g.priceKosdaqSounded;
            //        g.timeKosdaqSouned = o.틱의시간[0];
            //        g.priceKosdaqSounded = o.x[o.nrow - 1, 1];

            //        if (priceDifference < -150)
            //            sound = "pm 150";
            //        else if (priceDifference < -130)
            //            sound = "pm 130";
            //        else if (priceDifference < -110)
            //            sound = "pm 110";
            //        else if (priceDifference < -90)
            //            sound = "pm 90";
            //        else if (priceDifference < -70)
            //            sound = "pm 70";
            //        else if (priceDifference < -50)
            //            sound = "pm 50";
            //        else if (priceDifference < -30)
            //            sound = "pm 30";
            //        else if (priceDifference < -10)
            //            sound = "pm 10";
            //        else if (priceDifference < 10)
            //            sound = "pp 10";
            //        else if (priceDifference < 30)
            //            sound = "pp 30";
            //        else if (priceDifference < 50)
            //            sound = "pp 50";
            //        else if (priceDifference < 70)
            //            sound = "pp 70";
            //        else if (priceDifference < 90)
            //            sound = "pp 90";
            //        else if (priceDifference < 110)
            //            sound = "pp 110";
            //        else if (priceDifference < 130)
            //            sound = "pp 130";
            //        else
            //            sound = "pp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //}

            // old version : not using currently
            //public static void EvalKODEX(g.stock_data o)//, int[,] x) 20230818
            //{
            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKospiSouned == 0)
            //        {
            //            g.timeKospiSouned = o.틱의시간[0];
            //            g.priceKospiSounded = o.x[o.nrow - 1, 5];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKospiSouned, o.틱의시간[0]);

            //        if (elapsed_seconds < 33)
            //        {
            //            return;
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 5] - g.priceKospiSounded;
            //        g.timeKospiSouned = o.틱의시간[0];
            //        g.priceKospiSounded = o.x[o.nrow - 1, 5];

            //        if (priceDifference < -150)
            //            sound = "qm 150";
            //        else if (priceDifference < -130)
            //            sound = "qm 130";
            //        else if (priceDifference < -110)
            //            sound = "qm 110";
            //        else if (priceDifference < -90)
            //            sound = "qm 90";
            //        else if (priceDifference < -70)
            //            sound = "qm 70";
            //        else if (priceDifference < -50)
            //            sound = "qm 50";
            //        else if (priceDifference < -30)
            //            sound = "qm 30";
            //        else if (priceDifference < -10)
            //            sound = "qm 10";
            //        else if (priceDifference < 10)
            //            sound = "qp 10";
            //        else if (priceDifference < 30)
            //            sound = "qp 30";
            //        else if (priceDifference < 50)
            //            sound = "qp 50";
            //        else if (priceDifference < 70)
            //            sound = "qp 70";
            //        else if (priceDifference < 90)
            //            sound = "qp 90";
            //        else if (priceDifference < 110)
            //            sound = "qp 110";
            //        else if (priceDifference < 130)
            //            sound = "qp 130";
            //        else
            //            sound = "qp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //        if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        if (g.timeKosdaqSouned == 0)
            //        {
            //            g.timeKosdaqSouned = o.틱의시간[0];
            //            g.priceKosdaqSounded = o.x[o.nrow - 1, 1];
            //            return;
            //        }

            //        double elapsed_seconds = ms.total_Seconds(g.timeKosdaqSouned, o.틱의시간[0]);

            //        if(elapsed_seconds < 31)
            //        {
            //            return; 
            //        }

            //        string sound = "";
            //        int priceDifference = o.x[o.nrow - 1, 1] - g.priceKosdaqSounded;
            //        g.timeKosdaqSouned = o.틱의시간[0];
            //        g.priceKosdaqSounded = o.x[o.nrow - 1, 1];

            //        if (priceDifference < -150)
            //            sound = "qm 150";
            //        else if (priceDifference < -130)
            //            sound = "qm 130";
            //        else if (priceDifference < -110)
            //            sound = "qm 110";
            //        else if (priceDifference < -90)
            //            sound = "qm 90";
            //        else if (priceDifference < -70)
            //            sound = "qm 70";
            //        else if (priceDifference < -50)
            //            sound = "qm 50";
            //        else if (priceDifference < -30)
            //            sound = "qm 30";
            //        else if (priceDifference < -10)
            //            sound = "qm 10";
            //        else if (priceDifference < 10)
            //            sound = "qp 10";
            //        else if (priceDifference < 30)
            //            sound = "qp 30";
            //        else if (priceDifference < 50)
            //            sound = "qp 50";
            //        else if (priceDifference < 70)
            //            sound = "qp 70";
            //        else if (priceDifference < 90)
            //            sound = "qp 90";
            //        else if (priceDifference < 110)
            //            sound = "qp 110";
            //        else if (priceDifference < 130)
            //            sound = "qp 130";
            //        else
            //            sound = "qp 150";

            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //}


            // old version : not using
            //public static void EvalKODEX_not_using4(g.stock_data o)//, int[,] x)
            //{
            //    if (!g.connected)
            //        return;


            //    if (o.stock == "KODEX 레버리지")
            //    {
            //        string sound = "";
            //        int difference = o.가격 - g.kospi_value;

            //        if (difference > g.v.kospi_difference_for_sound[2])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p u u u";
            //        }
            //        else if (difference > g.v.kospi_difference_for_sound[1])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p u u";
            //        }
            //        else if (difference > g.v.kospi_difference_for_sound[0])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p u";
            //        }

            //        else if (difference < -1.0 * g.v.kospi_difference_for_sound[2])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p d d d";
            //        }
            //        else if (difference < -1.0 * g.v.kospi_difference_for_sound[1])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p d d";
            //        }
            //        else if (difference < -1.0 * g.v.kospi_difference_for_sound[0])
            //        {
            //            g.kospi_value = o.가격;
            //            sound = "p d";
            //        }
            //        else
            //        {
            //            sound = "";
            //        }
            //        mc.Sound("코스피 코스닥", sound);
            //    }

            //    if (o.stock == "KODEX 코스닥150레버리지")
            //    {
            //        string sound = "";
            //        int difference = o.가격 - g.kosdq_value;
            //        if (difference > g.v.kosdq_difference_for_sound[2])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x u u u";
            //        }
            //        else if (difference > g.v.kosdq_difference_for_sound[1])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x u u";
            //        }
            //        else if (difference > g.v.kosdq_difference_for_sound[0])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x u";
            //        }

            //        else if (difference < -1.0 * g.v.kosdq_difference_for_sound[2])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x d d d";
            //        }
            //        else if (difference < -1.0 * g.v.kosdq_difference_for_sound[1])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x d d";
            //        }
            //        else if (difference < -1.0 * g.v.kosdq_difference_for_sound[0])
            //        {
            //            g.kosdq_value = o.가격;
            //            sound = "x d";
            //        }
            //        else
            //        {
            //            sound = "";
            //        }
            //        mc.Sound("코스피 코스닥", sound);
            //    }
            //}

            // new verwsion : not used, sound rarely
            //public static void EvalKODEX_not_using2(g.stock_data o)//, int[,] x)
            //{
            //    // !!! 2023 1/9 3:00, naming of index_difference_sound and
            //    // checking every 20 seconds
            //    if (o.nrow < 2)
            //        return;

            //    // checking every 20 seconds
            //    int CheckNpts = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
            //    double elaped_seconds = ms.total_Seconds(g.check_time, CheckNpts); // from time to time
            //    if (elaped_seconds < 20)
            //        return;

            //    int index = wk.return_index_of_ogldata("KODEX 레버리지");
            //    if (index < 0) return;

            //    g.stock_data p = g.ogl_data[index];
            //    index = wk.return_index_of_ogldata("KODEX 코스닥150레버리지");
            //    if (index < 0) return;
            //    g.stock_data q = g.ogl_data[index];

            //    double 분당가격차;
            //    if (Math.Abs(p.분당가격차) > Math.Abs(q.분당가격차))
            //        분당가격차 = p.분당가격차;
            //    else
            //        분당가격차 = q.분당가격차;

            //    string sound = "";

            //    if (분당가격차 > g.v.index_difference_sound[2])
            //        sound = "UP UP UP";
            //    else if (분당가격차 > g.v.index_difference_sound[1])
            //        sound = "UP UP";
            //    else if (분당가격차 > g.v.index_difference_sound[0])
            //        sound = "UP";
            //    else if (분당가격차 < -g.v.index_difference_sound[2])
            //        sound = "DOWN DOWN DOWN";
            //    else if (분당가격차 < -g.v.index_difference_sound[1])
            //        sound = "DOWN DOWN";
            //    else if (분당가격차 < -g.v.index_difference_sound[0])
            //        sound = "DOWN";

            //    mc.Sound("코스피 코스닥", sound);

            //    g.check_time = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
            //}
            #endregion
        }
    }
}












