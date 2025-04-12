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
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
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
                            value = o.일간변동편차;
                            a_tuple.Add(Tuple.Create(value, o.stock));
                            break;
                        case "평균":
                            value = o.일간변동평균;
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

        public static bool eval_inclusion(StockData data)
        {
            string stock = data.Stock;
            var score = data.Score;
            var stat = data.Statistics;
            var post = data.Post;
            var api = data.Api;

            if ((g.KODEX4.Contains(stock) && !stock.Contains("레버리지")) ||
                stock.Contains("KODEX") ||
                stock.Contains("KOSEF") ||
                stock.Contains("HANARO") ||
                stock.Contains("TIGER") ||
                stock.Contains("KBSTAR") ||
                stock.Contains("혼합") ||
                g.보유종목.Contains(stock) ||
                g.호가종목.Contains(stock) ||
                g.관심종목.Contains(stock))
            {
                return false;
            }

            if (g.v.KeyString == "프누" ||
                g.v.KeyString == "종누" ||
                g.v.KeyString == "프편" ||
                g.v.KeyString == "종편")
                return true;

            if (g.v.푀플 == 1 && score.푀분 < 0)
                return false;
            if (g.v.배플 == 1 && score.배차 < 0)
                return false;

            if (g.v.종가기준추정거래액이상_천만원 > (int)post.종거천)
                return false;

            if (wk.isWorkingHour())
            {
                if (post.매도호가거래액_백만원 < g.v.호가거래액_백만원 &&
                    post.매수호가거래액_백만원 < g.v.호가거래액_백만원)
                    return false;

                if (g.v.분당거래액이상_천만원 > api.분거래천[0])
                    return false;
            }

            if (g.v.편차이상 >= stat.일간변동편차)
                return false;

            if (g.v.시총이상 >= 0)
            {
                if (stat.시총 < g.v.시총이상 - 0.01)
                    return false;
            }
            else
            {
                if (stat.시총 > (g.v.시총이상 - 0.01) * -1.0)
                    return false;
            }

            return true;
        }

        public static void eval_group()
        {
            for (int i = 0; i < g.oGL_data.Count; i++)
            {
                var group = g.oGL_data[i];
                group.총점 = 0.0;
                group.푀분 = 0.0;
                group.종누 = 0.0;
                group.수평 = 0.0;
                group.강평 = 0.0;
                group.가증 = 0.0;

                wk.거분순서(group.stocks);

                int count = 0;
                foreach (var stockName in group.stocks)
                {
                    if (count == 3) break;
                    var stock = StockRepository.Instance.Get(stockName);
                    if (stock == null || stock.Api.nrow < 2 || stock.Api.nrow >= 382) continue;

                    int row = g.test ? Math.Min(g.Npts[1] - 1, stock.Api.nrow - 1) : stock.Api.nrow - 1;
                    if (stock.Api.x[row, 1] < -3000 || stock.Api.x[row, 1] > 3000) continue;

                    group.총점 += stock.Score.총점;
                    group.거분 += stock.Score.거분;
                    group.푀분 += stock.Score.푀분;
                    group.수평 += stock.Api.x[row, 2];
                    group.강평 += stock.Api.x[row, 3] / 100.0;
                    group.가증 += stock.Api.x[row, 1] - stock.Api.x[row - 1, 1];
                    count++;
                }

                if (count > 0)
                {
                    group.총점 /= count;
                    group.거분 /= count;
                    group.푀분 /= count;
                    group.수평 /= count;
                    group.강평 /= count;
                    group.가증 /= count;
                }
            }

            if (g.oGl_data_selection == "총점")
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.총점).ToList();
            else if (g.oGl_data_selection == "푀분")
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.푀분).ToList();
            else if (g.oGl_data_selection == "가증")
                g.oGL_data = g.oGL_data.OrderByDescending(x => x.가증).ToList();

            for (int i = g.oGL_data.Count - 1; i >= 0; i--)
            {
                foreach (var stock in g.oGL_data[i].stocks)
                {
                    var s = StockRepository.Instance.Get(stock);
                    if (s != null)
                        s.Score.그순 = i;
                }
            }

            int displayCount = !g.test && g.oGL_data.Count > 9 ? 9 : g.oGL_data.Count;

            if (hg.HogaFormNameGivenStock("Form_그룹") != null)
            {
                g.그룹.dgv.SuspendLayout();
                try
                {
                    for (int i = 0; i < displayCount; i++)
                    {
                        var group = g.oGL_data[i];
                        bool changed = g.그룹.dtb.Rows[i][0].ToString() != group.title ||
                                       g.그룹.dtb.Rows[i][1].ToString() != ((int)group.푀분).ToString() ||
                                       g.그룹.dtb.Rows[i][2].ToString() != ((int)group.총점).ToString();

                        if (changed)
                        {
                            g.그룹.dtb.Rows[i][0] = group.title;
                            g.그룹.dtb.Rows[i][1] = ((int)group.푀분).ToString();
                            g.그룹.dtb.Rows[i][2] = ((int)group.총점).ToString();
                        }
                    }
                }
                finally
                {
                    g.그룹.dgv.ResumeLayout();
                }
            }
        }

    }
}












