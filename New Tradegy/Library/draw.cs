using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g;
using static New_Tradegy.Library.g.stock_data;
using static System.Net.Mime.MediaTypeNames;

namespace New_Tradegy.Library
{
    public class dr
    {
        // 0, 1(가격), 2(수급), 3(체강), 4(프로그램), 5(외인), 6(기관)
        public static Color[] colorGeneral = { Color.White, Color.Red, Color.DarkGray, Color.LightCoral, Color.DarkBlue, Color.Magenta, Color.Cyan };
        // 0, 1(가격), 2, 3(지수합), 4(기관), 5(외인), 6(개인), 7, 8, 9, 10(나스닥), 11(연기금)
        public static Color[] colorKODEX = { Color.White, Color.Red, Color.White, Color.Black, Color.Brown, Color.Magenta, Color.Green, Color.White, Color.White, Color.White, Color.Blue, Color.Brown };
        // short and long time extension or restoration 
        // and then, draw

        public static void draw_extended_time(int advance_lines)
        {
            if (advance_lines == 0)
            {
                g.time[1] = g.end_time_before_advance;
                g.end_time_before_advance = 0;
                g.end_time_extended = false;
            }
            else
            {
                g.end_time_before_advance = g.time[1];
                g.time[1] += advance_lines; // expedient
                if (g.time[1] > g.MAX_ROW)
                    g.time[1] = g.MAX_ROW;

                g.end_time_extended = true;
            }
        }

        

        public static void draw_chart() // duration : 0.008 ~ 0.2 seconds depends g.time[1]
        {
            int seq = 0;
            switch (g.q)
            {
                case "o&s":
                    // KODEX update or Generate ChartArea
                    // DisplayList : 보유, 호가, 관심 종목
                    // Check ChartArea for the stocks in DisplayList,
                    //  if not exist create ChartArea for missing stocks
                    // Remove ChartArea for not in DisplayList
                    // Locate ChartArea for the stocks in DisplayList
                    // Locate Forms with respect to the corresponding ChartArea



                    List<string> DisplayList = new List<string>();

                    int HogaCount = 0;
                    int TotalSpaceCount = g.nRow * (g.nCol - 2);
                    foreach (string stock in g.보유종목)
                    {
                        if (!DisplayList.Contains(stock) && !stock.Contains("KODEX"))
                        {
                            DisplayList.Add(stock);
                            HogaCount++;
                        }
                        if (HogaCount + DisplayList.Count >= TotalSpaceCount)
                            break;
                    }

                    foreach (string stock in g.호가종목)
                    {
                        if (!DisplayList.Contains(stock) && !stock.Contains("KODEX"))
                        {
                            DisplayList.Add(stock);
                            HogaCount++;
                        }
                        if (HogaCount + DisplayList.Count >= TotalSpaceCount)
                            break;
                    }

                    foreach (string stock in g.관심종목)
                    {
                        if (!DisplayList.Contains(stock) && !stock.Contains("KODEX"))
                        {
                            DisplayList.Add(stock);
                        }
                        if (HogaCount + DisplayList.Count >= TotalSpaceCount)
                            break;
                    }


                    foreach (string stock in g.sl)
                    {
                        if (!DisplayList.Contains(stock) && !stock.Contains("KODEX"))
                        {
                            DisplayList.Add(stock);
                        }
                        if (HogaCount + DisplayList.Count >= TotalSpaceCount)
                            break;
                    }

                    while (HogaCount + DisplayList.Count >= TotalSpaceCount)
                    {
                        DisplayList.RemoveAt(DisplayList.Count - 1);
                    }


                    for (seq = 0; seq < g.dl.Count; seq++)
                    {
                        if (g.dl[seq] == "")
                            continue;
                        switch (g.draw_selection)
                        {
                            case 1:
                                draw_stock(g.chart1, g.nRow, nCol, seq, g.dl[seq]); // "o&s":
                                break;
                            case 2:
                                draw_foreign_institute_price(seq, g.npts_fi_dwm, g.dl[seq]);
                                break;
                            case 3:
                                draw_stock_daily(seq, g.npts_fi_dwm, "일", g.dl[seq]);
                                break;
                        }
                    }

                    //stopwatch.Stop();

                    // Display the duration
                    // MessageBox.Show($"Time taken to plot the data: {stopwatch.Elapsed.TotalSeconds} seconds", "Duration");






                    break;

                case "s&s": // not used
                    #region
                    //if (g.Group_ranking_Gid < 0)
                    //{
                    //    g.Group_ranking_Gid = 0;
                    //}

                    //g.nCol = g.rqwey_nCol;
                    //g.nRow = 4; // 3 이하가 되면 그룹내 종목 갯수보다 작아져 draw_stock에서 에러 발생 

                    //g.DL.Clear();

                    //if (보유호가종목.Count > 0) // 
                    //{
                    //    List<string> s_list = new List<string>();


                    //    for (int j = 0; j < 보유호가종목.Count; j++)
                    //    {
                    //        s_list.Add(보유호가종목[j]);

                    //        if (s_list.Count == 4 || j == 1) // first column 2 KODEXs, and after on 4 stocks each column
                    //        {
                    //            g.DL.Add(s_list.ToList());
                    //            s_list.Clear();
                    //        }
                    //    }
                    //    if (s_list.Count > 0 && s_list.Count < 4) // 보유호가종목을 4개 컬럼에 차례로 채우고, 자투리가 있으면 하나의 컬럼으로 배치
                    //    {
                    //        g.DL.Add(s_list.ToList());
                    //    }
                    //}

                    //for (int i = g.Group_ranking_Gid; i < g.Group_ranking.Count; i++)
                    //{
                    //    List<string> c_list = new List<string>(g.Group_ranking[i].종목들);
                    //    if (c_list.Count > 0)
                    //    {
                    //        if (c_list[0].Contains("KODEX")) // 불필요한 데 ... 추가 검토 필요
                    //        {
                    //            continue;
                    //        }
                    //    }

                    //    for (int j = 0; j < 보유호가종목.Count; j++)
                    //    {
                    //        if (c_list.Contains(보유호가종목[j]))
                    //        {
                    //            c_list.Remove(보유호가종목[j]);
                    //        }
                    //    }

                    //    g.DL.Add(c_list);

                    //    // more than column count, break
                    //    if (g.DL.Count >= g.rqwey_nCol)
                    //    {
                    //        break;
                    //    }
                    //}

                    //for (int i = 0; i < g.DL.Count; i++)
                    //{
                    //    if (i >= g.rqwey_nCol)
                    //    {
                    //        break;
                    //    }
                    //    for (int id = 0; id < g.DL[i].Count; id++)
                    //    {
                    //        if (id > g.rqwey_nRow)
                    //            break;

                    //        if (i == 0)
                    //        {
                    //            seq = id;
                    //        }
                    //        else
                    //        {
                    //            seq = (i - 1) * g.nRow + id + 2;
                    //        }

                    //        switch (g.draw_selection)
                    //        {
                    //            case 1:
                    //                draw_stock(seq, g.DL[i][id]);
                    //                break;
                    //            case 2:
                    //                draw_foreign_institute_price(seq, g.npts_fi_dwm, g.DL[i][id]);
                    //                break;
                    //            case 3:
                    //                draw_stock_daily(seq, g.npts_fi_dwm, "일", g.DL[i][id]);
                    //                break;
                    //            default:
                    //                break;
                    //        }
                    //    }
                    //}
                    #endregion
                    break;

                case "a&s": // not used, 4개 종목 묶음으로 중첩 가격만 차트
                    #region
                    //    g.nCol = g.rqwey_nCol;
                    //    g.nRow = g.rqwey_nRow;

                    //    for (seq = g.Group_ranking_Gid; seq < g.Group_ranking_Gid + g.nCol * g.nRow; seq++)
                    //    {
                    //        if (seq >= g.Group_ranking.Count)
                    //        {
                    //            return;
                    //        }

                    //        draw_multiple(seq - g.Group_ranking_Gid, g.Group_ranking[seq].종목들);
                    //    }
                    #endregion
                    break;

                case "h&s": // 1개 종목 과거 일자별 차트  

                    #region
                    if (g.clickedStock == null)
                    {
                        return;
                    }

                    g.date = g.saved_date;
                    draw_stock(g.chart1, g.rqwey_nRow, g.rqwey_nCol, 0, g.clickedStock); // h&s

                    g.date = g.moving_reference_date;

                    g.date_list.Clear();
                    g.date_list.Add(g.saved_date);
                    for (int jndex = 1; jndex < (g.rqwey_nCol - 2) * g.rqwey_nRow; jndex++) // start from 3rd coulmn
                    {
                        int temp_date = wk.directory_분전후(g.date, -1); // 거래전일
                        if (temp_date == -1)
                        {
                            continue;
                        }
                        else
                        {
                            g.date = temp_date;
                            g.date_list.Add(temp_date);
                        }
                        draw_stock(g.chart1, g.rqwey_nRow, g.rqwey_nCol, jndex, g.clickedStock); // h&s, 두전째 날짜부터 계속
                    }
                    #endregion
                    break;

                case "o&g":
                case "s&g":
                case "a&g":
                case "h&g":
                case "kodex_leverage_single":
                case "kodex_inverse_single":
                case "kodex_kospi_group":
                case "kodex_kosdaq_group":
                    #region
                    if (g.clickedTitle == null &&
                        g.clickedStock == null && // 0723
                        g.q != "kodex_kospi_group" &&
                        g.q != "kodex_kosdaq_group" &&
                        g.q != "kodex_leverage_single" &&
                        g.q != "kodex_inverse_single")
                        return;

                    List<string> a_list = new List<string>();
                    if (g.q == "kodex_kospi_group") // key press 6
                    {
                        foreach (string s in g.kospi_mixed.stock) { a_list.Add(s); }
                    }
                    else if (g.q == "kodex_kosdaq_group") // key press 7
                    {
                        foreach (string s in g.kosdaq_mixed.stock) { a_list.Add(s); }
                    }
                    else if (g.q == "kodex_leverage_single" || g.q == "kodex_inverse_single")
                    {
                    }
                    else // 일반 상관 0723
                    {
                        a_list = g.상관절친종목.ToList();
                    }

                    if (a_list == null || a_list.Count <= 1 && !g.KODEX4.Contains(g.clickedStock))
                        return;

                    g.dl.Clear();

                    if (g.q == "kodex_leverage_single")
                    {
                        g.dl.Add("KODEX 레버리지");
                        g.dl.Add("KODEX 코스닥150레버리지");
                    }
                    else if (g.q == "kodex_inverse_single")
                    {
                        g.dl.Add("KODEX 200선물인버스2X");
                        g.dl.Add("KODEX 코스닥150선물인버스");
                    }
                    else if (g.q == "kodex_kospi_group")
                    {
                        g.dl.Add("KODEX 레버리지");
                        g.dl.Add("KODEX 200선물인버스2X");
                        //g.dl.Add("코스피혼합");
                        for (int i = 0; i < 10; i++)
                        {
                            g.dl.Add(g.kospi_mixed.stock[i]);
                        }
                    }
                    else if (g.q == "kodex_kosdaq_group")
                    {
                        g.dl.Add("KODEX 코스닥150레버리지");
                        g.dl.Add("KODEX 코스닥150선물인버스");
                        //g.dl.Add("코스닥혼합");
                        for (int i = 0; i < 10; i++)
                        {
                            g.dl.Add(g.kosdaq_mixed.stock[i]);
                        }
                    }
                    else
                    {
                        if (a_list == null || a_list.Count < 1)
                        {
                            break;
                        }

                        //foreach (var t in 보유호가종목)
                        //{
                        //    if (!g.dl.Contains(t))    // && !g.호가종목.Contains(t)) // 지수, 보유 포함, 관심 제외
                        //        g.dl.Add(t);
                        //}

                        foreach (string stock in a_list) // 0723
                        {
                            if (!g.dl.Contains(stock)) // KODEX, 보유주식 제외
                                g.dl.Add(stock);
                        }
                    }

                    if (g.dl.Count <= 2)
                    {
                        nCol = 2;
                        g.nRow = 1;
                    }
                    else if (g.dl.Count <= 4)
                    {
                        nCol = 4;
                        g.nRow = 1;
                    }
                    else if (g.dl.Count <= 8)
                    {
                        nCol = 4;
                        g.nRow = 2;
                    }
                    else if (g.dl.Count <= 12)
                    {
                        nCol = 6;
                        g.nRow = 2;
                    }
                    else if (g.dl.Count <= 18)
                    {
                        nCol = 6;
                        g.nRow = 3;
                    }
                    else
                    {
                        nCol = g.rqwey_nCol;
                        g.nRow = g.rqwey_nRow;
                    }

                    for (seq = 0; seq < g.dl.Count; seq++)
                    {
                        int index = wk.return_index_of_ogldata(g.dl[seq]);
                        if (index < 0)
                        {
                            continue;
                        }

                        if (seq >= nCol * g.nRow)
                        {
                            break;
                        }

                        switch (g.draw_selection)
                        {
                            case 1:
                                draw_stock(g.chart1, g.nRow, nCol, seq, g.dl[seq]); // o&g, kodex_leverage etc.
                                break;
                            case 2:
                                draw_foreign_institute_price(seq, g.npts_fi_dwm, g.dl[seq]);
                                break;
                            case 3:
                                draw_stock_daily(seq, g.npts_fi_dwm, "일", g.dl[seq]);
                                break;

                            case 4: // case 2의 draw_foreign_institute_price의 사용 데이터가 네이버와 차이 발생으로 시도 중인 API 데이터
                                    //g.npts_fi_dwm = 17;
                                    //draw_deal_history(seq, g.npts_fi_dwm, g.dl[seq]);
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion
                    break;


                default:
                    break;
            }




        }

        //    Parallel.For(0, g.dl.Count, i =>
        //                {
        //                    draw_stock(g.chart1, g.nRow, nCol, i, g.dl[i]); // o&g, kodex_leverage etc.
        //});

        public static void draw_stock_general(Chart chart, int nRow, int nCol, int seq, string stock)
        {
            int chart_id = mc.sender_to_chart_id(chart.Name);

            int index = wk.return_index_of_ogldata(stock);


            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }

            g.stock_data o = g.ogl_data[index];

            if (o.nrow < 2)
                return;


            float[] size = new float[2];
            float[] location = new float[2];
            int y_min = 100000;
            int y_max = -100000;



            string sid = "";

            if (g.test)
            {
                if (g.time[1] < 1) // g.time[0] = 0 means starting from 0859
                    return;
            }
            else
            {
                if (o.nrow <= 1) // no data yet, i.e. only 0859 
                    return;
            }



            // start time and end time
            int start_time = 0;
            int end_time = -1;

            // start time 
            if (o.shrink_draw == true)
            {
                if (g.test)
                {
                    start_time = g.time[1] - g.draw_shrink_time;
                    if (start_time < g.time[0])
                    {
                        start_time = g.time[0];
                    }
                }
                else
                {
                    start_time = o.nrow - g.draw_shrink_time;
                    if (start_time < g.time[0])
                    {
                        start_time = g.time[0];
                    }
                }
            }
            else
            {
                start_time = g.time[0];
            }


            // end time & maximum of a, i.e. amount
            if (g.test)
            {
                for (int k = start_time; k < g.time[1]; k++) // g.time[0] = 0 means starting from 0859
                {
                    if (o.x[k, 0] == 0 || o.x[k, 0] > 152100) // g.time[1]이 저장된 데이터 갯수보다 큰 경우 
                    {
                        end_time = k; //CHECK endtime = o.nrow. maybe no need of iteration
                        break;
                    }

                }
                if (end_time < 0)
                {
                    end_time = g.time[1]; // start of end_time equals -1, the time of whole data not equals 0 or >152100
                }

                // some stock with less data cannot be drawn at shrinked status
                if (end_time - start_time < 2)
                {
                    return;
                }
            }
            else
                end_time = o.nrow;


            // 단일가거래 종목은 차트 포함시키지 않음
            if (o.x[end_time - 1, 3] == 0)
                return;

            // The start of area and drawing of stock
            string area = seq.ToString();
            chart.ChartAreas.Add(area);

            // 1 가격, 2 수급, 3 체강, 8 배수, 9 중심
            int total_number_of_point = 0;


            for (int i = 1; i < 10; i++) // draw lines of price, amount, intensity
            {
                // i = 1 price, i = 2 amount, i = 3 intensity, 
                // 9 = center and tick multiple
                if (i == 4 || i == 5 || i == 6 || i == 7 || i == 8)
                {
                    continue;
                }

                sid = stock + " " + area + " " + i.ToString();

                Series t = new Series(sid);
                chart.Series.Add(t);

                total_number_of_point = 0;
                for (int k = start_time; k < end_time; k++)
                {
                    if (o.x[k, 0] == 0)  // time no data 
                    {
                        if (total_number_of_point < 2)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int value = 0;

                    if (i == 1) // price
                    {
                        value = o.x[k, 1];
                    }

                    if (i == 2) // amount
                    {
                        value = (int)(Math.Sqrt(o.x[k, 2]) * 10);
                        if (value > 500)
                            value = 500;
                    }

                    if (i == 3) // intensity
                    {
                        value = (int)(Math.Sqrt(o.x[k, 3] / (double)g.HUNDRED) * 10);
                        if (value > 500)
                            value = 500;
                        // value = (int)(Math.Pow(o.x[k, 3], 0.40));
                    }



                    if (i == 9) // tick multiple
                    {
                        value = 0;
                        if (k >= end_time - g.틱_array_size && !g.test) // tick multiple draw
                        {
                            int tick_index = (end_time - 1) - k;
                            if (tick_index < 0)
                            {
                                continue;
                            }
                            value = o.틱매수배[tick_index] - o.틱매도배[tick_index];
                            value = 0;
                        }
                    }

                    t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);

                    total_number_of_point++;
                    if (value < y_min)
                    {
                        y_min = value;
                    }
                    if (value > y_max)
                    {
                        y_max = value;
                    }
                }
                if (total_number_of_point < 2)
                    return;

                draw_stock_mark(chart, stock, start_time, i, total_number_of_point, o.x, t);

                // ChartArea, ChartType, BorderWidth
                t.ChartArea = area;

                t.ChartType = SeriesChartType.Line;
                t.XValueType = ChartValueType.Date;


                switch (i)
                {
                    case 1: // 가격
                        t.Color = colorGeneral[1];
                        t.BorderWidth = g.width.가격;
                        break;
                    case 2: // 수급
                        t.Color = colorGeneral[2];
                        t.BorderWidth = 1;
                        break;
                    case 3: // 체강
                        t.Color = colorGeneral[3];
                        t.BorderWidth = 1;
                        break;
                    case 8: // 매수 & 매도 배수 차이
                        //t.Color = Color.Black;
                        //t.BorderWidth = 1;
                        break;

                    case 9:  // 배수의 중심선
                        //t.Color = Color.Magenta;
                        //t.BorderWidth = 1;
                        break;
                    default:
                        break;
                }
                t.IsVisibleInLegend = false;
            }

            if (g.v.수급과장배수 != 0)
            {
                double percentile_multiplier = 1;
                if (o.x[end_time - 1, 7] > g.EPS) // 누적거래량 ! = 0
                {
                    percentile_multiplier = 100.0 / o.x[end_time - 1, 7]; // marketeye 
                }

                int[] id3 = { 4, 5, 6 }; // program, foreign, institute (amount dealt)
                for (int i = 0; i < id3.Length; i++)
                {
                    sid = stock + " " + area + " " + id3[i].ToString();
                    Series t = new Series(sid);
                    chart.Series.Add(t);

                    for (int k = start_time; k < end_time; k++)
                    {
                        if (o.x[k, 0] == 0) // if time = 0, end of data, break
                        {
                            break;
                        }
                        int value = (int)(o.x[k, id3[i]] * percentile_multiplier * g.v.수급과장배수 * o.수급과장배수); // 전체 & 종목 (수급과장배수)
                        t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                        if (value < y_min)
                        {
                            y_min = value;
                        }
                        if (value > y_max)
                        {
                            y_max = value;
                        }
                    }
                    draw_stock_mark(chart, stock, start_time, id3[i], total_number_of_point, o.x, t);
                    t.ChartArea = area;
                    t.ChartType = SeriesChartType.Line;
                    t.XValueType = ChartValueType.Date;
                    t.IsVisibleInLegend = false;
                    switch (id3[i])
                    {
                        case 4:
                            t.Color = colorGeneral[4]; // 당일프로그램순매수량
                            t.BorderWidth = g.width.프돈;

                            break;
                        case 5:
                            t.Color = colorGeneral[5]; // 당일외인순매수량
                            t.BorderWidth = g.width.외돈;
                            break;
                        case 6:
                            t.Color = colorGeneral[6]; // 당일기관순매수량
                            t.BorderWidth = g.width.기관;
                            break;
                        default:
                            break;
                    }
                }
            }

            string annotation = draw_stock_title(chart_id, o, seq, o.x, start_time, end_time, o.nrow); // g.test, o.nrow = g.MAX_ROW

            int numLines = 5;
            double annotationHeight;
            double chartAreaHeight;
            CalculateHeights(g.v.font, numLines, nRow, out annotationHeight, out chartAreaHeight);

            Color BackColor = Color.White;
            int[] scoreThresholds = { 90, 70, 50, 30, 10 };
            for (int i = 0; i < scoreThresholds.Length; i++)
            {
                if (o.점수.총점 > scoreThresholds[i])
                {
                    BackColor = g.Colors[i];
                    break;
                }
            }

            if (chart_id == 1)
                AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1],
                    100 / g.nCol, (int)annotationHeight + 2), area, Color.Black, BackColor);
            else
                AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1] + 1, size[0], (int)annotationHeight + 2), area, Color.Black, BackColor);

            // 1st x of left corner
            // 2nd y of left corner of 
            // 3rd x width from x
            // 4th y height from y
            chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(0, 10, 100, 80);



            int chartYMin = (int)Math.Floor(y_min / 100.0) * 100;
            int chartYMax = (int)Math.Ceiling(y_max / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Minimum = (double)chartYMin;
            chart.ChartAreas[area].AxisY.Maximum = (double)chartYMax;
            chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisX.Interval = total_number_of_point - 1;

            int yInterval = (int)Math.Ceiling((chartYMax - chartYMin) / 3 / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Interval = yInterval;





            chart.ChartAreas[area].AxisX.IntervalOffset = 1;
            chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;

            chart.ChartAreas[area].Position.X = location[0];
            chart.ChartAreas[area].Position.Y = location[1] + (float)annotationHeight;
            chart.ChartAreas[area].Position.Width = size[0];
            chart.ChartAreas[area].Position.Height = (float)chartAreaHeight;


            chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            chart.ChartAreas[area].AxisY.LabelStyle.Font
                = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));




            // 높은 등수 낮은 등수 : 낮은 등수로 eval_group에서 등록됨 : 수정 완료
            BackColor = Color.White;
            if (g.q == "o&s" && o.점수.그순 < 5)
            {
                chart.ChartAreas[area].BackColor = g.Colors[o.점수.그순]; //
            }


            if (o.분프로천[0] > 5 && o.분외인천[0] > 5 && o.분배수차[0] > 0)
                chart.ChartAreas[area].BackColor = g.Colors[5]; //




        }


        public static void draw_stock_general_history(Chart chart, int nRow, int nCol, int seq, string stock)
        {
            // Copy the array 10,000 times -> 1 ms
            //for (int k = 0; k < 10000; k++)
            //{
            //    Array.Copy(x, copy_x, x.Length);
            //}

            int chart_id = mc.sender_to_chart_id(chart.Name);

            int index = wk.return_index_of_ogldata(stock);

            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }
            g.stock_data d = g.ogl_data[index];

            g.stock_data o = d.DeepCopy(); // stock general for history : use and throw away

            if (seq != 0)
            {
                o.nrow = 0;
                o.nrow = rd.read_Stock_Minute(g.date, stock, o.x);
                ps.post(o); // general_history
            }


            if (o.nrow < 2)
                return;



            float[] size = new float[2];
            float[] location = new float[2];
            int y_min = 100000;
            int y_max = -100000;



            string sid = "";





            if (g.test)
            {
                if (g.time[1] < 1) // g.time[0] = 0 means starting from 0859
                    return;
            }
            else
            {
                if (o.nrow <= 1) // no data yet, i.e. only 0859 
                    return;
            }



            // start time and end time
            int start_time = 0;
            int end_time = g.MAX_ROW;

            #region
            // start time 
            //if (o.shrink_draw == true)
            //{
            //    if (g.test)
            //    {
            //        start_time = g.time[1] - g.draw_shrink_time;
            //        if (start_time < g.time[0])
            //        {
            //            start_time = g.time[0];
            //        }
            //    }
            //    else
            //    {
            //        start_time = o.nrow - g.draw_shrink_time;
            //        if (start_time < g.time[0])
            //        {
            //            start_time = g.time[0];
            //        }
            //    }
            //}
            //else
            //{
            //    start_time = g.time[0];
            //}


            // end time & maximum of a, i.e. amount
            //if (g.test)
            //{
            //    for (int k = start_time; k < g.time[1]; k++) // g.time[0] = 0 means starting from 0859
            //    {
            //        if (o.x[k, 0] == 0 || o.x[k, 0] > 152100) // g.time[1]이 저장된 데이터 갯수보다 큰 경우 
            //        {
            //            end_time = k; //CHECK endtime = o.nrow. maybe no need of iteration
            //            break;
            //        }

            //    }
            //    if (end_time < 0)
            //    {
            //        end_time = g.time[1]; // start of end_time equals -1, the time of whole data not equals 0 or >152100
            //    }

            //    // some stock with less data cannot be drawn at shrinked status
            //    if (end_time - start_time < 2)
            //    {
            //        return;
            //    }
            //}
            //else
            //    end_time = o.nrow;
            #endregion

            // 단일가거래 종목은 차트 포함시키지 않음
            //if (o.x[end_time - 1, 3] == 0)
            //    return;

            // The start of area and drawing of stock
            string area = seq.ToString();
            chart.ChartAreas.Add(area);

            // 1 가격, 2 수급, 3 체강, 8 배수, 9 중심
            int total_number_of_point = 0;


            for (int i = 1; i < 10; i++) // draw lines of price, amount, intensity
            {
                // i = 1 price, i = 2 amount, i = 3 intensity, 
                // i = 8 minute multiple, 9 = center and tick multiple
                if (i == 4 || i == 5 || i == 6 || i == 7)
                {
                    continue;
                }

                sid = stock + " " + area + " " + i.ToString();

                Series t = new Series(sid);
                chart.Series.Add(t);

                total_number_of_point = 0;
                for (int k = start_time; k < end_time; k++)
                {
                    if (o.x[k, 0] == 0)  // time no data 
                    {
                        if (total_number_of_point < 2)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int value = 0;

                    if (i == 1) // price
                    {
                        value = o.x[k, 1];
                    }

                    if (i == 2) // amount
                    {
                        value = (int)(Math.Sqrt(o.x[k, 2]) * 10);
                        if (value > 500)
                            value = 500;
                    }

                    if (i == 3) // intensity
                    {
                        value = (int)(Math.Sqrt(o.x[k, 3] / (double)g.HUNDRED) * 10);
                        if (value > 500)
                            value = 500;
                        // value = (int)(Math.Pow(o.x[k, 3], 0.40));
                    }

                    if (i == 8) // minute multiple
                    {
                        value = o.x[k, 8] - o.x[k, 9];
                        value = 0;
                    }

                    if (i == 9) // tick multiple
                    {
                        value = 0;
                        if (k >= end_time - g.틱_array_size && !g.test) // tick multiple draw
                        {
                            int tick_index = (end_time - 1) - k;
                            if (tick_index < 0)
                            {
                                continue;
                            }
                            value = o.틱매수배[tick_index] - o.틱매도배[tick_index];
                            value = 0;
                        }
                    }

                    t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);

                    total_number_of_point++;
                    if (value < y_min)
                    {
                        y_min = value;
                    }
                    if (value > y_max)
                    {
                        y_max = value;
                    }
                }
                if (total_number_of_point < 2)
                    return;

                draw_stock_mark(chart, stock, start_time, i, total_number_of_point, o.x, t);

                // ChartArea, ChartType, BorderWidth
                t.ChartArea = area;

                t.ChartType = SeriesChartType.Line;
                t.XValueType = ChartValueType.Date;


                switch (i)
                {
                    case 1: // 가격
                        t.Color = colorGeneral[1];
                        t.BorderWidth = g.width.가격;
                        break;
                    case 2: // 수급
                        t.Color = colorGeneral[2];
                        t.BorderWidth = 1;
                        break;
                    case 3: // 체강
                        t.Color = colorGeneral[3];
                        t.BorderWidth = 1;
                        break;
                    case 8: // 매수 & 매도 배수 차이
                        //t.Color = Color.Black;
                        //t.BorderWidth = 1;
                        break;

                    case 9:  // 배수의 중심선
                        //t.Color = Color.Magenta;
                        //t.BorderWidth = 1;
                        break;
                    default:
                        break;
                }
                t.IsVisibleInLegend = false;
            }

            if (g.v.수급과장배수 != 0)
            {
                double percentile_multiplier = 1;
                if (o.x[end_time - 1, 7] > g.EPS) // 누적거래량 ! = 0
                {
                    percentile_multiplier = 100.0 / o.x[end_time - 1, 7]; // marketeye 
                }

                int[] id3 = { 4, 5, 6 }; // program, foreign, institute (amount dealt)
                for (int i = 0; i < id3.Length; i++)
                {
                    sid = stock + area + " " + id3[i].ToString();
                    Series t = new Series(sid);
                    chart.Series.Add(t);

                    for (int k = start_time; k < end_time; k++)
                    {
                        if (o.x[k, 0] == 0) // if time = 0, end of data, break
                        {
                            break;
                        }
                        int value = (int)(o.x[k, id3[i]] * percentile_multiplier * g.v.수급과장배수 * o.수급과장배수); // 전체 & 종목 (수급과장배수)
                        t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                        if (value < y_min)
                        {
                            y_min = value;
                        }
                        if (value > y_max)
                        {
                            y_max = value;
                        }
                    }
                    draw_stock_mark(chart, stock, start_time, id3[i], total_number_of_point, o.x, t);
                    t.ChartArea = area;
                    t.ChartType = SeriesChartType.Line;
                    t.XValueType = ChartValueType.Date;
                    t.IsVisibleInLegend = false;
                    switch (id3[i])
                    {
                        case 4:
                            t.Color = colorGeneral[4]; // 당일프로그램순매수량
                            t.BorderWidth = g.width.프돈;

                            break;
                        case 5:
                            t.Color = colorGeneral[5]; // 당일외인순매수량
                            t.BorderWidth = g.width.외돈;
                            break;
                        case 6:
                            t.Color = colorGeneral[6]; // 당일기관순매수량
                            t.BorderWidth = g.width.기관;
                            break;
                        default:
                            break;
                    }
                }
            }

            string annotation = draw_stock_title(chart_id, o, seq, o.x, start_time, end_time, o.nrow); // g.test, o.nrow = g.MAX_ROW

            int numLines = 5;
            double annotationHeight;
            double chartAreaHeight;
            CalculateHeights(g.v.font, numLines, nRow, out annotationHeight, out chartAreaHeight);

            // Add a rectangle annotation with text
            Color BackColor = Color.White;
            if (o.점수.총점 > 50)
                BackColor = Color.FromArgb(255, 210, 255); // 빨강
            else if (o.점수.총점 > 40)
                BackColor = Color.FromArgb(255, 230, 255); // 빨강
            else if (o.점수.총점 > 30)
                BackColor = Color.FromArgb(210, 255, 255); // 사이안
            else if (o.점수.총점 > 20)
                BackColor = Color.FromArgb(230, 255, 255); // 사이안
            else if (o.점수.총점 > 10)
                BackColor = Color.FromArgb(255, 255, 190); // 노랑
            else
            {

            }

            if (chart_id == 1)
                AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1],
                    100 / g.nCol, (int)annotationHeight + 2), area, Color.Black, BackColor);
            else
                AddRectangleAnnotationWithText(chart, annotation, new RectangleF(location[0], location[1] + 1, size[0], (int)annotationHeight + 2), area, Color.Black, BackColor);

            // 1st x of left corner
            // 2nd y of left corner of 
            // 3rd x width from x
            // 4th y height from y
            chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(0, 10, 100, 80);



            int chartYMin = (int)Math.Floor(y_min / 100.0) * 100;
            int chartYMax = (int)Math.Ceiling(y_max / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Minimum = (double)chartYMin;
            chart.ChartAreas[area].AxisY.Maximum = (double)chartYMax;
            chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisX.Interval = total_number_of_point - 1;

            int yInterval = (int)Math.Ceiling((chartYMax - chartYMin) / 3 / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Interval = yInterval;





            chart.ChartAreas[area].AxisX.IntervalOffset = 1;
            chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;

            chart.ChartAreas[area].Position.X = location[0];
            chart.ChartAreas[area].Position.Y = location[1] + (float)annotationHeight;
            chart.ChartAreas[area].Position.Width = size[0];
            chart.ChartAreas[area].Position.Height = (float)chartAreaHeight;


            chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            chart.ChartAreas[area].AxisY.LabelStyle.Font
                = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));




            // 높은 등수 낮은 등수 : 낮은 등수로 eval_group에서 등록됨 : 수정 완료
            if (o.분프로천[0] > 5 && o.분외인천[0] > 5 && o.분배수차[0] > 0)
                chart.ChartAreas[area].BackColor = Color.FromArgb(230, 230, 255); //
            if (o.점수.그순 == 0 && g.q == "o&s")
                chart.ChartAreas[area].BackColor = Color.FromArgb(255, 230, 255); // 빨강
            if (o.점수.그순 == 1 && g.q == "o&s")
                chart.ChartAreas[area].BackColor = Color.FromArgb(230, 255, 255); // 사이안
            if (o.점수.그순 == 2 && g.q == "o&s")
                chart.ChartAreas[area].BackColor = Color.FromArgb(255, 255, 190); // 노랑



        }




        static void CalculateHeights(float fontSize, int numLines, int numRows, out double annotationHeight, out double chartAreaHeight)
        {
            // Get screen dimensions
            Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
            int screenHeight = workingRectangle.Height;

            // Conversion factor: 1 point ≈ 1.33 pixels
            double conversionFactor = 1.33;

            // Calculate the height of the annotation in pixels
            double labelHeightPixels = fontSize * numLines * conversionFactor;

            // Calculate the total available height in relative terms (percentage)
            double totalAvailableHeight = 100.0;

            // Calculate the height for one annotation and one chart area
            double totalRowHeightPercentage = totalAvailableHeight / numRows;

            // Calculate the height of the annotation in percentage based on screen height
            annotationHeight = (labelHeightPixels / screenHeight) * totalAvailableHeight;

            // Calculate the remaining height for the chart area
            chartAreaHeight = totalRowHeightPercentage - annotationHeight;
        }

        private static void AddRectangleAnnotationWithText(
    Chart chart,
    string text,
    RectangleF rect,
    string chartAreaName,
    Color textColor,
    Color backgroundColor)
        {
            // Get the chart area
            ChartArea chartArea = chart.ChartAreas[chartAreaName];
            if (chartArea == null)
            {
                throw new ArgumentException($"ChartArea '{chartAreaName}' does not exist.");
            }

            // Calculate position relative to the chart area's dimensions
            double chartAreaTop = chartArea.Position.Y;
            double chartAreaLeft = chartArea.Position.X;
            double chartAreaWidth = chartArea.Position.Width;
            double chartAreaHeight = chartArea.Position.Height;

            // Adjust the annotation to appear at the top of the ChartArea
            double annotationX = chartAreaLeft + (rect.X * chartAreaWidth / 100); // Adjust rect.X relative to ChartArea width
            double annotationY = chartAreaTop; // Align annotation to the top of the ChartArea

            RectangleAnnotation rectangleAnnotation = new RectangleAnnotation
            {
                Text = text,
                Font = new Font("Arial", g.v.font),
                X = annotationX,
                Y = annotationY,
                Width = rect.Width * chartAreaWidth / 100, // Adjust rect.Width relative to ChartArea width
                Height = rect.Height * chartAreaHeight / 100, // Adjust rect.Height relative to ChartArea height
                LineColor = Color.Transparent,
                BackColor = backgroundColor,
                ForeColor = textColor,
                ClipToChartArea = chartAreaName,
                AxisXName = chartAreaName + "\\X",
                AxisYName = chartAreaName + "\\Y",
                Alignment = ContentAlignment.TopLeft,
                ToolTip = "Rectangle Annotation Tooltip" // Optional: Add tooltip to verify it's being added
            };

            // Add the annotation to the chart
            chart.Annotations.Add(rectangleAnnotation);
        }


        private static void AddRectangleAnnotationWithText_old(Chart chart, string text, RectangleF rect, string chartAreaName, Color textColor, Color backgroundColor)
        {
            // X and Y Coordinates: Relative to the chart's axes units.
            // Width and Height: Also in chart's axes units, not pixels.

            // Arial Narrow
            // Roboto Condensed
            // Helvetica Condensed
            if (chart.Name == "chart2")
            {
                rect.X += 3;

            }
            RectangleAnnotation rectangleAnnotation = new RectangleAnnotation
            {
                Text = text,
                Font = new Font("Arial", g.v.font),
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height,
                LineColor = Color.Transparent,
                BackColor = backgroundColor,
                ForeColor = textColor,
                ClipToChartArea = "",
                AxisXName = chartAreaName + "\\X",
                AxisYName = chartAreaName + "\\Y",
                Alignment = ContentAlignment.TopLeft,
                ToolTip = "Rectangle Annotation Tooltip" // Optional: Add tooltip to verify it's being added
            };
            chart.Annotations.Add(rectangleAnnotation);
        }

        public static void draw_stock_KODEX(Chart chart, int nRow, int nCol, int seq, string stock)
        {
            int chart_id = mc.sender_to_chart_id(chart.Name);

            int index = wk.return_index_of_ogldata(stock);
            if (index < 0)
                return;


            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }

            g.stock_data o = g.ogl_data[index];

            int magnifier_id = -1;
            for (int i = 0; i < g.KODEX4.Count; i++)
            {
                if (o.stock == g.KODEX4[i])
                {
                    magnifier_id = i;
                    break;
                }
            }



            float[] size = new float[2];
            float[] location = new float[2];
            int y_min = 100000;
            int y_max = -100000;



            string sid = "";


            if (g.test)
            {
                if (g.time[1] < 1) // g.time[0] = 0 means starting from 0859
                    return;
            }
            else
            {
                if (o.nrow <= 1) // no data yet, i.e. only 0859 
                    return;
            }








            // g.draw_shrink_time is controlled by 'o' and 'O'
            int start_time = 0;
            int end_time = -1;
            if (o.shrink_draw == true)
            {
                if (g.test)
                {
                    start_time = g.time[1] - g.draw_shrink_time;
                    if (start_time < g.time[0])
                    {
                        start_time = g.time[0];
                    }
                }
                else
                {
                    start_time = o.nrow - g.draw_shrink_time;
                    if (start_time < g.time[0])
                    {
                        start_time = g.time[0];
                    }
                }
            }
            else
            {
                start_time = g.time[0];
            }







            // end time
            if (g.test)
            {
                for (int k = start_time; k < g.time[1]; k++) // g.time[0] = 0 means starting from 0859
                {
                    if (o.x[k, 0] == 0 || o.x[k, 0] > 152100) // g.time[1]이 저장된 데이터 갯수보다 큰 경우 
                    {
                        end_time = k; //CHECK endtime = o.nrow. maybe no need of iteration
                        break;
                    }

                }
                if (end_time < 0)
                {
                    end_time = g.time[1]; // start of end_time equals -1, the time of whole data not equals 0 or >152100
                }

                // some stock with less data cannot be drawn at shrinked status
                if (end_time - start_time < 2)
                {
                    return;
                }
            }
            else
                end_time = o.nrow;


            // The start of area and drawing of stock
            string area = seq.ToString();
            chart.ChartAreas.Add(area); //  error 0인 요소가 있습니다.

            // 1 가격, 2 수급, 3 체강, 8 배수, 9 중심
            int total_number_of_point = 0;

            for (int i = 1; i < 10; i++) // draw lines of price, amount, intensity
            {
                // i = 1 price, i = 2 amount, i = 3 intensity, 
                // i = 8 minute multiple, 9 = center and tick multiple
                //if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
                //{
                //    continue;
                //}


                if (i != 1)
                {
                    continue;
                }

                sid = stock + " " + area + " " + i.ToString();

                Series t = new Series(sid);
                chart.Series.Add(t);

                total_number_of_point = 0;
                //int expected_number_interval = end_time - start_time;

                for (int k = start_time; k < end_time; k++)
                {
                    if (o.x[k, 0] == 0)  // time no data 
                    {
                        if (total_number_of_point < 2)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int value = 0;


                    if (i == 1) // price & 
                    {
                        if (g.KODEX4.Contains(stock))
                        {
                            double magnifier = 1.0;
                            // double shifter = 0.0;
                            if (KodexMagnifier(o, i, ref magnifier) == -1) //, ref shifter) == -1)
                                continue;

                            value = (int)(o.x[k, 1] * g.kodex_magnifier[magnifier_id, 0]);
                            //if (stock.Contains("인버스"))
                            //{
                            //    value = (int)(value - shifter);
                            //}
                            //else
                            //{
                            //    value = (int)(value + shifter);
                            //}
                        }
                        else
                        {
                            value = (int)(o.x[k, 1]);
                        }
                    }

                    if (i == 2) // amount
                    {
                        value = (int)(Math.Sqrt(o.x[k, 2]) * 10);
                        if (value > 500)
                            value = 500;
                    }

                    if (i == 3) // intensity
                    {
                        value = (int)(Math.Sqrt(o.x[k, 3] / (double)g.HUNDRED) * 10);
                        if (value > 500)
                            value = 500;
                        // value = (int)(Math.Pow(o.x[k, 3], 0.40));
                    }

                    if (i == 8) // minute multiple
                    {
                        value = (int)((o.x[k, 8] - o.x[k, 9]) * g.v.배수과장배수); // minute multiple draw, if inverse, swapped in Read_Stock
                        value = 0;
                    }

                    if (i == 9) // skipped, center line and tick multiple
                    {
                        value = 0;
                        if (k >= end_time - g.틱_array_size && !g.test) // tick multiple draw
                        {
                            int multiple_factor = (int)(g.v.배수과장배수 * 10); // five times of min. for tick

                            int tick_index = (end_time - 1) - k;
                            if (tick_index < 0)
                            {
                                continue;
                            }

                            value = (int)(o.틱매수배[tick_index] -
                                        o.틱매도배[tick_index]) * multiple_factor;
                            value = 0;
                        }
                    }

                    t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);

                    total_number_of_point++;
                    if (value < y_min)
                    {
                        y_min = value;
                    }
                    if (value > y_max)
                    {
                        y_max = value;
                    }
                }
                if (total_number_of_point < 2)
                    return;

                draw_stock_mark(chart, stock, start_time, i, total_number_of_point, o.x, t);

                // ChartArea, ChartType, BorderWidth
                t.ChartArea = area;

                t.ChartType = SeriesChartType.Line;
                t.XValueType = ChartValueType.Date;


                // Line Color 
                switch (i)
                {
                    case 1: // 가격
                        t.Color = colorGeneral[1];
                        t.BorderWidth = g.width.가격;
                        break;
                    case 2: // 수급
                        t.Color = colorGeneral[2];
                        t.BorderWidth = 1;

                        break;
                    case 3: // 체강
                        t.Color = colorGeneral[3];
                        t.BorderWidth = 1;
                        break;
                    case 8: // 매수 & 매도 배수 차이
                        //t.Color = Color.Red;
                        //t.BorderWidth = 1;
                        break;

                    case 9:  // skipped, 배수의 중심선
                        //t.Color = Color.Black;
                        //t.BorderWidth = 1;
                        break;
                    default:
                        break;
                }
                t.IsVisibleInLegend = false;
            }

            if (g.v.수급과장배수 != 0)
            {
                // KODEX 

                double magnifier = 1.0;
                //double shifter = 0.0;

                //magnifier *= g.v.수급과장배수 / 300.0;
                // 0, 시간, 1, 가격, 2 수급, 3 체강 * 100,
                // 4 기관순매수액(억), 5 프로그램순매수액(억), 6 개인순매수액(억)
                // 7 거래량, 8 매수배, 9 매도배, 10 Nasdaq_지수, 11 연기금순매수액(억)
                int[] id_index = { 3, 4, 5, 6, 10, 11 }; // 지수내의 지수내합(3), 기관(4), 외인(5), 개인(6), 나스닥지수(10), 연기(11)

                for (int i = 0; i < id_index.Length; i++)
                {
                    KodexMagnifier(o, id_index[i], ref magnifier); //, ref shifter); // shift for KODEX
                    sid = stock + area + " " + id_index[i].ToString();
                    Series t = new Series(sid);
                    chart.Series.Add(t);


                    int magnifier_jd = -1;
                    if (magnifier_id >= 0)
                    {
                        switch (id_index[i])
                        {
                            case 3:
                                magnifier_jd = 1;
                                break;
                            case 4:
                                magnifier_jd = 2;
                                break;
                            case 5:
                                magnifier_jd = 2;
                                break;
                            case 6:
                                magnifier_jd = 2;
                                break;
                            case 10:
                                magnifier_jd = 3;
                                break;
                            case 11:
                                magnifier_jd = 2;
                                break;
                        }
                    }

                    if (magnifier_jd < 0)
                        return;

                    for (int k = start_time; k < end_time; k++)
                    {
                        if (o.x[k, 0] == 0) // if time = 0, break
                        {
                            break;
                        }

                        int value;
                        if (id_index[i] == 10) // us_index
                        {
                            if (o.x[k, id_index[i]] == 0 && k != 0)
                            {
                                int upper_non_zero = 0;
                                for (int j = k + 1; j < end_time; j++)
                                {
                                    if (o.x[j, id_index[i]] != 0)
                                    {
                                        upper_non_zero = o.x[j, id_index[i]];
                                        break;
                                    }
                                }
                                int lower_non_zero = 0;
                                for (int j = k - 1; j >= start_time; j--)
                                {
                                    if (o.x[j, id_index[i]] != 0)
                                    {
                                        lower_non_zero = o.x[j, id_index[i]];
                                        break;
                                    }
                                }
                                value = (upper_non_zero + lower_non_zero) / 2;
                            }
                            else
                                value = o.x[k, id_index[i]];

                            value = (int)(value * g.kodex_magnifier[magnifier_id, magnifier_jd]);
                            //value += (int)shifter;
                            if (o.stock.Contains("인버스"))
                            {
                                value *= -1;
                            }
                        }
                        else
                        {
                            value = (int)(o.x[k, id_index[i]] * g.kodex_magnifier[magnifier_id, magnifier_jd]);
                            if (id_index[i] == 11) // pension
                                value *= 3; // 다섯 배 확대하여 본다
                            //value += (int)shifter;
                        }


                        t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                        if (value < y_min)
                        {
                            y_min = value;
                        }
                        if (value > y_max)
                        {
                            y_max = value;
                        }
                    }
                    draw_stock_mark(chart, stock, start_time, id_index[i], total_number_of_point, o.x, t); // DO I NEED THIS

                    t.ChartArea = area;
                    t.ChartType = SeriesChartType.Line;
                    t.XValueType = ChartValueType.Date;
                    t.IsVisibleInLegend = false;
                    // Line Color 
                    switch (id_index[i])
                    {

                        case 3: // 지수 구성 종목 프로 + 외인 합계
                            t.Color = colorKODEX[3]; // 지수 내의 프외 합
                            //t.BorderWidth = 2;
                            t.BorderWidth = g.width.프돈;
                            break;
                        case 4:
                            t.Color = colorKODEX[4]; // 기관
                            t.BorderWidth = 1;
                            break;
                        case 5:
                            t.Color = colorKODEX[5]; // 외인
                            t.BorderWidth = 1;
                            break;
                        case 6:
                            t.Color = colorKODEX[6]; // 개인
                            t.BorderWidth = 2;
                            break;
                        case 10:
                            t.Color = colorKODEX[10];// 나스닥지수
                            t.BorderWidth = 1;
                            break;
                        case 11:
                            t.Color = colorKODEX[11]; // 연기금
                            t.BorderWidth = 1;
                            break;
                        default:
                            break;
                    }
                    //if (g.q == "kodex_leverage_single" || g.q == "kodex_inverse_single")
                    //{
                    //    switch (id_index[i])
                    //    {
                    //        case 5:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        case 6:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        case 10:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        default:
                    //            t.BorderWidth = 1;
                    //            break;
                    //    }
                    //}
                }
            }

            // string annotation = draw_stock_title(chart_id, o, seq, index, o.x, start_time, end_time, o.nrow); // g.test, o.nrow = g.MAX_ROW

            // Add an annotation for the label
            //AddTextAnnotation(chart, annotation, new PointF(location[0], location[1]), g.v.font, area, Color.Blue, Color.Red); // text, back


            chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(20, 5, 55, 90);


            chart.ChartAreas[area].AxisY.Minimum = Math.Floor(y_min / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Maximum = Math.Ceiling(y_max / 100.0) * 100;
            chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisX.Interval = total_number_of_point - 1;








            chart.ChartAreas[area].AxisX.IntervalOffset = 1;
            chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;

            chart.ChartAreas[area].Position.X = location[0];
            chart.ChartAreas[area].Position.Y = location[1];
            chart.ChartAreas[area].Position.Width = size[0];
            chart.ChartAreas[area].Position.Height = size[1];


            chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            chart.ChartAreas[area].AxisY.LabelStyle.Font
                = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));





            int b = 255;
            int r = 255;
            if (o.분당가격차 > 35)
            {
                r = 150;
            }
            else if (o.분당가격차 > 28)
            {
                r = 170;
            }
            else if (o.분당가격차 > 21)
            {
                r = 190;
            }
            else if (o.분당가격차 > 14)
            {
                r = 210;
            }
            else if (o.분당가격차 > 7)
            {
                r = 230;
            }
            else if (o.분당가격차 <= 7 && o.분당가격차 >= -7)
            {

            }
            else if (o.분당가격차 > -14)
            {
                b = 230;
            }
            else if (o.분당가격차 > -21)
            {
                b = 210;
            }
            else if (o.분당가격차 > -28)
            {
                b = 190;
            }
            else if (o.분당가격차 > -35)
            {
                b = 170;
            }
            else
            {
                b = 150;
            }
            chart.ChartAreas[area].BackColor = Color.FromArgb(b, r, 255); // 사이안, 빨강
            // chart.Legends[label_title].BackColor = Color.FromArgb(b, r, 255); // 사이안, 빨강
        }

        public static void draw_stock_KODEX_history(Chart chart, int nRow, int nCol, int seq, string stock)
        {
            int chart_id = mc.sender_to_chart_id(chart.Name);

            int index = wk.return_index_of_ogldata(stock);

            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }

            g.stock_data d = g.ogl_data[index];



            g.stock_data o = d.DeepCopy(); // stock KODEX for history : use and throw away



            if (seq != 0)
            {
                o.nrow = 0;
                o.nrow = rd.read_Stock_Minute(g.date, stock, o.x);
                ps.post(o); // KODEX_history
            }


            if (o.nrow < 2)
                return;



            int magnifier_id = -1;
            for (int i = 0; i < g.KODEX4.Count; i++)
            {
                if (o.stock == g.KODEX4[i])
                {
                    magnifier_id = i;
                    break;
                }
            }



            float[] size = new float[2];
            float[] location = new float[2];
            int y_min = 100000;
            int y_max = -100000;



            string sid = "";



            // g.draw_shrink_time is controlled by 'o' and 'O'
            int start_time = 0;
            int end_time = g.MAX_ROW;




            // The start of area and drawing of stock
            string area = seq.ToString();
            chart.ChartAreas.Add(area);

            // 1 가격, 2 수급, 3 체강, 8 배수, 9 중심
            int total_number_of_point = 0;

            for (int i = 1; i < 10; i++) // draw lines of price, amount, intensity
            {
                // i = 1 price, i = 2 amount, i = 3 intensity, 
                // i = 8 minute multiple, 9 = center and tick multiple
                //if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
                //{
                //    continue;
                //}


                if (i != 1)
                {
                    continue;
                }

                sid = stock + " " + area + " " + i.ToString();

                Series t = new Series(sid);
                chart.Series.Add(t);

                total_number_of_point = 0;
                //int expected_number_interval = end_time - start_time;

                for (int k = start_time; k < end_time; k++)
                {
                    if (o.x[k, 0] == 0)  // time no data 
                    {
                        if (total_number_of_point < 2)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int value = 0;


                    if (i == 1) // price & 
                    {
                        if (g.KODEX4.Contains(stock))
                        {
                            double magnifier = 1.0;
                            // double shifter = 0.0;
                            if (KodexMagnifier(o, i, ref magnifier) == -1) //, ref shifter) == -1)
                                continue;

                            value = (int)(o.x[k, 1] * g.kodex_magnifier[magnifier_id, 0]);
                            //if (stock.Contains("인버스"))
                            //{
                            //    value = (int)(value - shifter);
                            //}
                            //else
                            //{
                            //    value = (int)(value + shifter);
                            //}
                        }
                        else
                        {
                            value = (int)(o.x[k, 1]);
                        }
                    }

                    if (i == 2) // amount
                    {
                        value = (int)(Math.Sqrt(o.x[k, 2]) * 10);
                        if (value > 500)
                            value = 500;
                    }

                    if (i == 3) // intensity
                    {
                        value = (int)(Math.Sqrt(o.x[k, 3] / (double)g.HUNDRED) * 10);
                        if (value > 500)
                            value = 500;
                        // value = (int)(Math.Pow(o.x[k, 3], 0.40));
                    }

                    if (i == 8) // minute multiple
                    {
                        value = (int)((o.x[k, 8] - o.x[k, 9]) * g.v.배수과장배수); // minute multiple draw, if inverse, swapped in Read_Stock
                        value = 0;
                    }

                    if (i == 9) // skipped, center line and tick multiple
                    {
                        value = 0;
                        if (k >= end_time - g.틱_array_size && !g.test) // tick multiple draw
                        {
                            int multiple_factor = (int)(g.v.배수과장배수 * 10); // five times of min. for tick

                            int tick_index = (end_time - 1) - k;
                            if (tick_index < 0)
                            {
                                continue;
                            }

                            value = (int)(o.틱매수배[tick_index] -
                                        o.틱매도배[tick_index]) * multiple_factor;
                            value = 0;
                        }
                    }

                    t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);

                    total_number_of_point++;
                    if (value < y_min)
                    {
                        y_min = value;
                    }
                    if (value > y_max)
                    {
                        y_max = value;
                    }
                }
                if (total_number_of_point < 2)
                    return;

                draw_stock_mark(chart, stock, start_time, i, total_number_of_point, o.x, t);

                // ChartArea, ChartType, BorderWidth
                t.ChartArea = area;

                t.ChartType = SeriesChartType.Line;
                t.XValueType = ChartValueType.Date;


                // Line Color 
                switch (i)
                {
                    case 1: // 가격
                        t.Color = colorGeneral[1];
                        t.BorderWidth = g.width.가격;
                        break;
                    case 2: // 수급
                        t.Color = colorGeneral[2];
                        t.BorderWidth = 1;

                        break;
                    case 3: // 체강
                        t.Color = colorGeneral[3];
                        t.BorderWidth = 1;
                        break;
                    case 8: // 매수 & 매도 배수 차이
                        //t.Color = Color.Red;
                        //t.BorderWidth = 1;
                        break;

                    case 9:  // skipped, 배수의 중심선
                        //t.Color = Color.Black;
                        //t.BorderWidth = 1;
                        break;
                    default:
                        break;
                }
                t.IsVisibleInLegend = false;
            }

            if (g.v.수급과장배수 != 0)
            {
                // KODEX 

                double magnifier = 1.0;
                //double shifter = 0.0;

                //magnifier *= g.v.수급과장배수 / 300.0;
                // 0, 시간, 1, 가격, 2 수급, 3 체강 * 100,
                // 4 기관순매수액(억), 5 프로그램순매수액(억), 6 개인순매수액(억)
                // 7 거래량, 8 매수배, 9 매도배, 10 Nasdaq_지수, 11 연기금순매수액(억)
                int[] id_index = { 3, 4, 5, 6, 10, 11 }; // 지수내의 지수내합(3), 기관(4), 외인(5), 개인(6), 나스닥지수(10), 연기(11)

                for (int i = 0; i < id_index.Length; i++)
                {
                    KodexMagnifier(o, id_index[i], ref magnifier); //, ref shifter); // shift for KODEX
                    sid = stock + area + " " + id_index[i].ToString();
                    Series t = new Series(sid);
                    chart.Series.Add(t);


                    int magnifier_jd = -1;
                    if (magnifier_id >= 0)
                    {
                        switch (id_index[i])
                        {
                            case 3:
                                magnifier_jd = 1;
                                break;
                            case 4:
                                magnifier_jd = 2;
                                break;
                            case 5:
                                magnifier_jd = 2;
                                break;
                            case 6:
                                magnifier_jd = 2;
                                break;
                            case 10:
                                magnifier_jd = 3;
                                break;
                            case 11:
                                magnifier_jd = 2;
                                break;
                        }
                    }

                    if (magnifier_jd < 0)
                        return;

                    for (int k = start_time; k < end_time; k++)
                    {
                        if (o.x[k, 0] == 0) // if time = 0, break
                        {
                            break;
                        }

                        int value;
                        if (id_index[i] == 10) // us_index
                        {
                            if (o.x[k, id_index[i]] == 0 && k != 0)
                            {
                                int upper_non_zero = 0;
                                for (int j = k + 1; j < end_time; j++)
                                {
                                    if (o.x[j, id_index[i]] != 0)
                                    {
                                        upper_non_zero = o.x[j, id_index[i]];
                                        break;
                                    }
                                }
                                int lower_non_zero = 0;
                                for (int j = k - 1; j >= start_time; j--)
                                {
                                    if (o.x[j, id_index[i]] != 0)
                                    {
                                        lower_non_zero = o.x[j, id_index[i]];
                                        break;
                                    }
                                }
                                value = (upper_non_zero + lower_non_zero) / 2;
                            }
                            else
                                value = o.x[k, id_index[i]];

                            value = (int)(value * g.kodex_magnifier[magnifier_id, magnifier_jd]);
                            //value += (int)shifter;
                            if (o.stock.Contains("인버스"))
                            {
                                value *= -1;
                            }
                        }
                        else
                        {
                            value = (int)(o.x[k, id_index[i]] * g.kodex_magnifier[magnifier_id, magnifier_jd]);
                            if (id_index[i] == 11) // pension
                                value *= 3; // 다섯 배 확대하여 본다
                            //value += (int)shifter;
                        }


                        t.Points.AddXY(((int)(o.x[k, 0] / g.HUNDRED)).ToString(), value);
                        if (value < y_min)
                        {
                            y_min = value;
                        }
                        if (value > y_max)
                        {
                            y_max = value;
                        }
                    }
                    draw_stock_mark(chart, stock, start_time, id_index[i], total_number_of_point, o.x, t); // DO I NEED THIS

                    t.ChartArea = area;
                    t.ChartType = SeriesChartType.Line;
                    t.XValueType = ChartValueType.Date;
                    t.IsVisibleInLegend = false;
                    // Line Color 
                    switch (id_index[i])
                    {

                        case 3: // 지수 구성 종목 프로 + 외인 합계
                            t.Color = colorKODEX[3]; // 지수 내의 프외 합
                            //t.BorderWidth = 2;
                            t.BorderWidth = g.width.프돈;
                            break;
                        case 4:
                            t.Color = colorKODEX[4]; // 기관
                            t.BorderWidth = 1;
                            break;
                        case 5:
                            t.Color = colorKODEX[5]; // 외인
                            t.BorderWidth = 1;
                            break;
                        case 6:
                            t.Color = colorKODEX[6]; // 개인
                            t.BorderWidth = 2;
                            break;
                        case 10:
                            t.Color = colorKODEX[10];// 나스닥지수
                            t.BorderWidth = 1;
                            break;
                        case 11:
                            t.Color = colorKODEX[11]; // 연기금
                            t.BorderWidth = 1;
                            break;
                        default:
                            break;
                    }
                    //if (g.q == "kodex_leverage_single" || g.q == "kodex_inverse_single")
                    //{
                    //    switch (id_index[i])
                    //    {
                    //        case 5:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        case 6:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        case 10:
                    //            t.BorderWidth = 2;
                    //            break;
                    //        default:
                    //            t.BorderWidth = 1;
                    //            break;
                    //    }
                    //}
                }
            }

            // string annotation = draw_stock_title(chart_id, o, seq, index, o.x, start_time, end_time, o.nrow); // g.test, o.nrow = g.MAX_ROW

            // Add an annotation for the label
            //AddTextAnnotation(chart, annotation, new PointF(location[0], location[1]), g.v.font, area, Color.Blue, Color.Red); // text, back


            chart.ChartAreas[area].InnerPlotPosition = new ElementPosition(20, 5, 55, 90);


            chart.ChartAreas[area].AxisY.Minimum = Math.Floor(y_min / 100.0) * 100;
            chart.ChartAreas[area].AxisY.Maximum = Math.Ceiling(y_max / 100.0) * 100;
            chart.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
            chart.ChartAreas[area].AxisX.Interval = total_number_of_point - 1;








            chart.ChartAreas[area].AxisX.IntervalOffset = 1;
            chart.ChartAreas[area].AxisX.LabelStyle.Enabled = true;

            chart.ChartAreas[area].Position.X = location[0];
            chart.ChartAreas[area].Position.Y = location[1];
            chart.ChartAreas[area].Position.Width = size[0];
            chart.ChartAreas[area].Position.Height = size[1];


            chart.ChartAreas[area].AxisX.LabelStyle.Font
            = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            chart.ChartAreas[area].AxisY.LabelStyle.Font
                = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));





            int b = 255;
            int r = 255;
            if (o.분당가격차 > 35)
            {
                r = 150;
            }
            else if (o.분당가격차 > 28)
            {
                r = 170;
            }
            else if (o.분당가격차 > 21)
            {
                r = 190;
            }
            else if (o.분당가격차 > 14)
            {
                r = 210;
            }
            else if (o.분당가격차 > 7)
            {
                r = 230;
            }
            else if (o.분당가격차 <= 7 && o.분당가격차 >= -7)
            {

            }
            else if (o.분당가격차 > -14)
            {
                b = 230;
            }
            else if (o.분당가격차 > -21)
            {
                b = 210;
            }
            else if (o.분당가격차 > -28)
            {
                b = 190;
            }
            else if (o.분당가격차 > -35)
            {
                b = 170;
            }
            else
            {
                b = 150;
            }
            chart.ChartAreas[area].BackColor = Color.FromArgb(b, r, 255); // 사이안, 빨강
            // chart.Legends[label_title].BackColor = Color.FromArgb(b, r, 255); // 사이안, 빨강
        }
        public static void draw_stock(Chart chart, int nRow, int nCol, int seq, string stock)
        {
            if (stock.Contains("KODEX"))
            {
                if (g.q != "h&s")
                    draw_stock_KODEX(chart, nRow, nCol, seq, stock);
                else
                    draw_stock_KODEX_history(chart, nRow, nCol, seq, stock);
            }
            else
            {
                if (g.q != "h&s" || chart.Name == "chart2")
                    draw_stock_general(chart, nRow, nCol, seq, stock);
                else if (!stock.Contains("KODEX"))
                    draw_stock_general_history(chart, nRow, nCol, seq, stock);
            }
        }


        public static int KodexMagnifier(g.stock_data o, int id,
                                                                   ref double magnifier) //, ref double shifter)
        {
            int i = 0;
            int j = 0;

            switch (o.stock)
            {
                case "KODEX 레버리지":
                    i = 0;
                    break;
                case "KODEX 200선물인버스2X":
                    i = 1;
                    break;
                case "KODEX 코스닥150레버리지":
                    i = 2;
                    break;
                case "KODEX 코스닥150선물인버스":
                    i = 3;
                    break;
            }

            switch (id)
            {
                case 1: // price
                    j = 0;
                    break;
                case 4: // money
                case 5:
                case 6:
                case 11:
                    j = 1;
                    break;
                case 10: // US
                    j = 2;
                    break;

                default:
                    return -1; // input mitake
            }

            magnifier = g.kodex_magnifier[i, j];
            //shifter = g.k.shifter[i, j];

            return 0;
        }

        public static void draw_size_location(Chart chart, int nRow, int nCol, int seq, float[] location, float[] size)
        {
            int chart_id = mc.sender_to_chart_id(chart.Name); // 1, 2, 
            int row;
            int col;

            if (chart_id == 1)
            {
                if (nCol != 0)
                    size[0] = 100.0F / nCol; // nCol is always even number
                if (nRow != 0)
                    size[1] = 100F / nRow;


                switch (g.q)
                {
                    case "o&s":
                    case "s&s":
                    case "o&g":
                    case "s&g":
                    case "a&g":
                    case "h&g":
                        if (seq == 0)
                        {
                            size[1] = 50.0F;
                            location[0] = 0.0F;
                            location[1] = 0.0F;
                        }
                        else if (seq == 1)
                        {
                            size[1] = 50.0F;
                            location[0] = 0.0F;
                            location[1] = 50.0F;
                        }
                        else
                        {
                            row = (seq - 2) % nRow;
                            col = (seq - 2) / nRow + 1;

                            //if (col >= nCol / 2)
                            //{
                            //    location[0] = size[0] * col + 20.0F;
                            //}
                            //else
                            //{
                            location[0] = size[0] * col;
                            //}
                            location[1] = size[1] * row;
                        }
                        //if (seq == 0)
                        //{
                        //    location[0] = 0.0F;
                        //    location[1] = 0.0F;
                        //}
                        //else if (seq == 1)
                        //{
                        //    location[0] = 0.0F;
                        //    location[1] = 50.0F;
                        //}
                        //else
                        //{
                        //    row = (seq - 2) % nRow;
                        //    col = (seq - 2) / nRow + 1;

                        //    if (col >= nCol / 2)
                        //    {
                        //        location[0] = size[0] * col + 20.0F;
                        //    }
                        //    else
                        //    {
                        //        location[0] = size[0] * col;
                        //    }
                        //    location[1] = size[1] * row;
                        //}
                        break;
                    case "h&s":
                    case "a&s":
                    case "kodex_leverage_single":
                    case "kodex_inverse_single":
                    case "kodex_kospi_group":
                    case "kodex_kosdaq_group":
                        row = seq % nRow;
                        col = seq / nRow + 2; // start from 3rd column
                        location[0] = size[0] * col;
                        location[1] = size[1] * row;
                        break;
                }
            }
            else
            {
                size[0] = 100.0F / nCol;
                size[1] = 97.0F / nRow;



                row = seq % nRow;
                col = seq / nRow;

                location[0] = size[0] * col;
                location[1] = size[1] * row + 3.0F;
            }
        }

        public static string draw_stock_title(int chart_id, g.stock_data o, int seq, int[,] x, int start_time, int end_time, int total_nrow)
        {
            string stock = o.stock;

            string stock_title = "";
            //stock_title += " " + o.점수.급상.ToString() + " "; // 수정요

            if (chart_id == 2)
            {
                int indey = wk.return_index_of_ogldata(o.stock);
                if (indey < 0) { }
                foreach (var line in g.ogl_data[indey].절친)
                {
                    string[] words = line.Split('\t');


                    if (words[1] == o.stock)
                    {
                        stock_title += Convert.ToDouble(words[0]).ToString("F2") + " ";
                        break;
                    }
                }
            }

            if (g.q == "h&s" && o != null)
            {
                if (seq == 0)
                {
                    stock_title = stock; // 종목
                }
                else
                {
                    stock_title = g.date.ToString(); // h&s의 경우 날짜
                }


                stock_title += " " + seq.ToString() +                        //sid.ToString() + // 날짜 중복 문제로 생각없이 추가하였는 데 필요 ?
                                  " " + (o.종거천 / 10).ToString("F0") +
                                  " " + (o.프누천 / 10).ToString("F0") +
                                  " " + (o.외누천 / 10).ToString("F0") +
                                  " " + (o.기누천 / 10).ToString("F0");
            }

            else if (stock.Contains("혼합"))
            {
                if ((stock == "코스피혼합") && g.dl[0].Contains("인버스"))  // kodex&s 
                    stock_title = stock + " : " + "인버스";
                if ((stock == "코스피혼합") && g.dl[0].Contains("레버리지")) // kodex&s and kodex_kospi_group
                    stock_title = stock;
                if ((stock == "코스닥혼합") && g.dl[2].Contains("인버스")) // kodex&s
                    stock_title = stock + " : " + "인버스";
                if ((stock == "코스닥혼합") && (g.dl[0].Contains("코스닥") || g.dl[2].Contains("레버리지"))) // kodex&s and kodex_kosdaq_group
                    stock_title = stock;
            }
            else
            {
                // 일반의 첫째 라인
                if (g.q != "h&s")
                {
                    if (g.보유종목.Contains(stock))
                    {
                        stock_title = "$$" + stock_title;
                    }
                    else if (g.호가종목.Contains(stock))
                    {
                        stock_title = "@ " + stock_title;
                    }

                    else if (g.관심종목.Contains(stock))
                    {
                        stock_title = "@ " + stock_title;
                    }

                    if (stock == "KODEX 레버리지")
                    {
                        stock_title += "코스피" + "\n";
                    }
                    else if (stock == "KODEX 코스닥150레버리지")
                    {
                        stock_title += "코스닥" + "\n";
                    }
                    else
                    {
                        string first5Chars;
                        if (stock.Length >= 5)
                        {
                            first5Chars = stock.Substring(0, 5);
                        }
                        else
                        {
                            first5Chars = stock; // or handle the case where the string is shorter than 6 characters
                        }
                        stock_title += first5Chars;
                    }


                    if (!g.KODEX4.Contains(stock))
                    {
                        if (o.oGL_sequence_id < 0) // 종목이 그룹 안에 없을 경우 종목 이름 뒤 한 칸 띄고 'x' 표시
                            stock_title += "%";
                        else
                            stock_title += " ";
                        stock_title += Math.Round(o.종거천 / 10.0) + "  " +

                                           (o.프누천 / 10.0).ToString("F2") + "  " +
                                           (o.외누천 / 10.0).ToString("F2") + "  " +
                                           (o.기누천 / 10.0).ToString("F0");
                    }
                    (o.프누천 / 10.0).ToString("F2");


                }
            }

            if (g.test && o.selected_for_group == false)
            {
                stock_title = "$$$ " + stock_title;
            }


            stock_title += ("\n" + draw_stock_title_tick_minute(o, x, start_time, end_time));

            // 일반 : 프돈 + 외돈 
            if (!(g.KODEX4.Contains(stock) || stock.Contains("혼합")))
            {
                stock_title += "\n";
                if (o.분외인천[0] >= 0)
                    stock_title += o.분프로천[0].ToString("F0") + "+" + o.분외인천[0].ToString("F0") + "/" + o.분거래천[0].ToString("F0");
                else
                    stock_title += o.분프로천[0].ToString("F0") + o.분외인천[0].ToString("F0") + "/" + o.분거래천[0].ToString("F0");

                for (int i = 1; i < 5; i++)
                    stock_title += "   " + (o.분프로천[i] + o.분외인천[i]).ToString("F0") +
                        "/" + o.분거래천[i].ToString("F0");

                // 0825 DELETE
                stock_title += "\n";
                stock_title += (o.점수.푀분).ToString("F0"); // o.점수.거분).ToString("F0");
                if (o.점수.배차.ToString("F0") == "0" || o.점수.배차 >= 0) // 반올림값이 0 보다 큰 경우
                    stock_title += "+";
                stock_title += (o.점수.배차).ToString("F0");
                if (o.점수.배합.ToString("F0") == "0" || o.점수.배합 >= 0) // 반올림값이 0 보다 큰 경우
                    stock_title += "+";
                stock_title += o.점수.배합.ToString("F0");
                stock_title += "+" + o.점수.그순.ToString("F0");
                stock_title += " (" + o.x[end_time - 1, 1].ToString() + " / " + o.현재가.ToString("#,##0") + ")";
            }
            // KODEX4
            else
            {
                stock_title += "\n" + "(" + o.x[end_time - 1, 1].ToString() + "/" + o.현재가.ToString("#,##0") + ")";
            }

            //stock_title += " " + o.dev_avr; // 수급, 체강의 연속점수 삭제
            stock_title += " " + x[end_time - 1, 10] + "/" + x[end_time - 1, 11] + "  " + o.dev_avr; // 수급, 체강의 연속점수
            return stock_title;
        }

        public static void draw_stock_mark(Chart chart, string stock, int start_time, int i, int total_number_of_point, int[,] x, Series t) // i (가격, 수급, 체강 순으로 Series)
        {
            // draw mark for price, amount, intensity
            // npts : total number of o.x[,], end_xid : end x id of o.x[,]
            int index = wk.return_index_of_ogldata(stock);

            if (index < 0)
                return;

            bool KODEX = false;
            if (g.KODEX4.Contains(stock))
                KODEX = true;


            g.stock_data o = g.ogl_data[index];


            if (total_number_of_point == 0) { return; }
            string[] difference = new string[7];
            int k;


            int end_id = start_time + total_number_of_point - 1;
            // if price increases more than the specified value, than circle mark
            for (int m = start_time + 1; m <= end_id; m++)
            {
                if (i == 1) // price
                {
                    int p_id = m - start_time; // g.time[0] != 0, shifting needed
                    int price_change = x[m, 1] - x[m - 1, 1];

                    if (KODEX)
                    {
                        price_change = x[m, 1] - x[m - 1, 1];
                        if (price_change >= 20) // KODEX 가격 변화가 20 이상 상승하는 경우 Circle로 표시
                        {
                            if (price_change > 40)
                            {
                                t.Points[p_id].MarkerColor = Color.Blue; // KODEX
                            }
                            else if (price_change > 30)
                            {
                                t.Points[p_id].MarkerColor = Color.Green; // KODEX
                            }
                            else
                            {
                                t.Points[p_id].MarkerColor = Color.Red; // KODEX
                            }
                            t.Points[p_id].MarkerSize = 7;
                            t.Points[p_id].MarkerStyle = MarkerStyle.Circle;
                        }
                    }
                    else
                    {
                        if (price_change >= 100)
                        {
                            if (price_change > 300)
                            {
                                t.Points[p_id].MarkerColor = Color.Black;
                            }

                            else if (price_change > 200)
                                t.Points[p_id].MarkerColor = Color.Blue;
                            else if (price_change > 150)
                                t.Points[p_id].MarkerColor = Color.Green;
                            else
                                t.Points[p_id].MarkerColor = Color.Red;

                            t.Points[p_id].MarkerSize = 6;
                            t.Points[p_id].MarkerStyle = MarkerStyle.Cross;
                        }

                        if (x[m, 0] >= 90200) // 시간이 9:02 이후 배수 차이가 100 이상일 때
                        {
                            int multiple_difference = x[m, 8] - x[m, 9]; // 배수 차이 
                            if (multiple_difference > 100)
                            {

                                t.Points[p_id].MarkerSize = 9;
                                t.Points[p_id].MarkerStyle = MarkerStyle.Circle;

                                if (multiple_difference > 500)
                                    t.Points[p_id].MarkerColor = Color.Black;
                                else if (multiple_difference > 300)
                                    t.Points[p_id].MarkerColor = Color.Blue;
                                else if (multiple_difference > 200)
                                    t.Points[p_id].MarkerColor = Color.Green;
                                else
                                    t.Points[p_id].MarkerColor = Color.Red;
                            }
                        }
                    }

                    if (g.q != "h&s")
                    {
                        int mark_size = 0;

                        if (o.분거래천[0] < 10)
                        {
                        }
                        else if (o.분거래천[0] < 50)
                        {
                            t.Points[0].MarkerColor = Color.Red;
                            mark_size = 10;
                        }
                        else if (o.분거래천[0] < 100)
                        {
                            t.Points[0].MarkerColor = Color.Red;
                            mark_size = 15;
                        }
                        else if (o.분거래천[0] < 200)
                        {
                            t.Points[0].MarkerColor = Color.Green;
                            mark_size = 15;
                        }
                        else if (o.분거래천[0] < 300)
                        {
                            t.Points[0].MarkerColor = Color.Green;
                            mark_size = 20;
                        }
                        else if (o.분거래천[0] < 500)
                        {
                            t.Points[0].MarkerColor = Color.Blue;
                            mark_size = 20;
                        }
                        else if (o.분거래천[0] < 800)
                        {
                            t.Points[0].MarkerColor = Color.Blue;
                            mark_size = 30;
                        }
                        else if (o.분거래천[0] < 1200)
                        {
                            t.Points[0].MarkerColor = Color.Black;
                            mark_size = 30;
                        }
                        else if (o.분거래천[0] < 1700)
                        {
                            t.Points[0].MarkerColor = Color.Black;
                            mark_size = 40;
                        }
                        else
                        {
                            t.Points[0].MarkerColor = Color.Black;
                            mark_size = 50;
                        }

                        if (g.q == "kodex_leverage_single" || g.q == "kodex_inverse_single")
                            t.Points[0].MarkerSize = mark_size * 5;
                        else
                            t.Points[0].MarkerSize = mark_size;

                        if (price_change >= 0)
                            t.Points[0].MarkerStyle = MarkerStyle.Circle;
                        else
                            t.Points[0].MarkerStyle = MarkerStyle.Cross;
                    }
                }

                // magenta and cyan cross mark on the lines of amount and intensity
                if ((i == 2 || i == 3) && !KODEX)
                {
                    if (KODEX)
                    {
                        //int positive_count = 0; // count of positive intensity
                        //int sequence_id = m - start_time;

                        //if (sequence_id - g.npts_for_magenta_cyan_mark < 0)
                        //{
                        //    continue;
                        //}

                        //for (int n = 0; n < g.npts_for_magenta_cyan_mark; n++)
                        //{
                        //    if (m - 1 - n < 0)
                        //    {
                        //        break;
                        //    }

                        //    int diff = x[m - n, i] - x[m - 1 - n, i];


                        //    if (diff > 0) // from ">=" to ">"
                        //    {
                        //        positive_count++;
                        //    }
                        //}
                        //if (positive_count >= g.npts_for_magenta_cyan_mark) // magenta and cyan
                        //                                                    // if (positive_count >= g.npts_for_magenta_cyan_mark && x[m, 1] - x[m - 1, 1] >= 0) // the last price lowering magenta and cyan excluded
                        //{
                        //    if (i == 2)
                        //    {
                        //        t.Points[sequence_id].MarkerColor = Color.Magenta; // amount
                        //    }
                        //    else
                        //    {
                        //        t.Points[sequence_id].MarkerColor = Color.Cyan;  // intensity
                        //    }

                        //    t.Points[sequence_id].MarkerStyle = MarkerStyle.Cross;
                        //    t.Points[sequence_id].MarkerSize = 11;
                        //}
                    }


                    else // 일반
                    {
                        if (x[m, i + 8] >= g.npts_for_magenta_cyan_mark) // the last price lowering magenta and cyan excluded
                                                                         //if (x[m, i + 8] >= g.npts_for_magenta_cyan_mark && x[m, 1] - x[m - 1, 1] >= 0) // the last price lowering magenta and cyan excluded
                        {
                            int sequence_id = m - start_time;
                            if (i == 2)
                            {

                                t.Points[sequence_id].MarkerColor = Color.Magenta; // amount
                            }
                            else
                            {
                                t.Points[sequence_id].MarkerColor = Color.Cyan;  // intensity
                            }

                            t.Points[sequence_id].MarkerStyle = MarkerStyle.Cross;
                            t.Points[sequence_id].MarkerSize = 7;
                        }
                    }
                }
            }

            //Label of price, amount and intensity at the end point
            if (i == 1 || i == 2 || i == 3 ||
                (i == 4) ||
                (i == 5) || // && stock.Contains("KODEX")) || 20220723
                (i == 6 && KODEX) ||
                (i == 10 && KODEX) ||
                (i == 11 && KODEX))
            {

                int plus_count = 0;
                int sum = 0;

                if (i == 3)
                {
                    if (KODEX)
                        t.Points[total_number_of_point - 1].Label = "      " + ((int)(x[end_id, i])).ToString();
                    else
                        t.Points[total_number_of_point - 1].Label = "      " + ((int)(x[end_id, i] / g.HUNDRED)).ToString();
                }
                else if (i == 4 && !KODEX)
                    t.Points[total_number_of_point - 1].Label = o.프누천.ToString("F1");
                else if (i == 5 && !KODEX)
                    t.Points[total_number_of_point - 1].Label = o.외누천.ToString("F1");
                else
                    t.Points[total_number_of_point - 1].Label = "      " + x[end_id, i].ToString();

                // Curve End Label
                for (k = 0; k < 4; k++)
                {
                    if (end_id - k - 1 < 0)
                        break;

                    int d = 0;
                    if (i == 3) // 2020/0801/1013. due to intensity = 0 display problem
                    {
                        if (KODEX)
                            d = (int)(x[end_id - k, i]) - (int)(x[end_id - k - 1, i]);
                        else
                            d = (int)(x[end_id - k, i] / g.HUNDRED) - (int)(x[end_id - k - 1, i] / g.HUNDRED);
                    }

                    else if (i == 4 && !KODEX)
                        d = (int)Math.Round(o.분프로천[k]); // 20220723
                    else if (i == 5 && !KODEX)
                        d = (int)Math.Round(o.분외인천[k]); // 20220723
                    else
                        d = x[end_id - k, i] - x[end_id - k - 1, i]; // difference

                    if (k < 5)
                    {
                        if (d > 0)
                            plus_count++;

                        if (d < 0)
                            plus_count--;

                        sum += d;
                    }

                    if (d >= 0)
                        difference[k] = "+" + d.ToString();
                    else
                        difference[k] = d.ToString();

                    if (difference[k] != null)
                    {
                        // t.MarkerSize = 10;
                        if ((i != 2 && i != 3) || KODEX)
                            t.Points[total_number_of_point - 1].Label += difference[k];

                        if (i == 2)
                            t.LabelForeColor = colorGeneral[2];    // label of amount

                        if (i == 3)
                            if (KODEX)
                                t.LabelForeColor = colorKODEX[3];
                            else
                                t.LabelForeColor = colorGeneral[3];   // label of intensity

                        if (i == 1 || i == 4 || i == 5 || i == 6)
                        {
                            if (KODEX)
                                t.LabelForeColor = colorKODEX[i];
                            else
                            {
                                t.LabelForeColor = colorGeneral[i];

                            }

                        }

                        if (i == 10 || i == 11)
                            t.LabelForeColor = colorKODEX[i];

                    }

                }
            }

            //if (i == 1)
            //{
            //    if (o.pass.passed_high_id > 0)
            //    {
            //        if (o.pass.passed_high_id >= start_time &&
            //            o.pass.passed_high_id - start_time < total_number_of_point)
            //        {
            //            t.Points[o.pass.passed_high_id - start_time].MarkerColor = Color.Black;
            //            t.Points[o.pass.passed_high_id - start_time].MarkerStyle = MarkerStyle.Square;
            //            t.Points[o.pass.passed_high_id - start_time].MarkerSize = 11;
            //        }
            //    }
            //}

            // working
            if (chart.Name == "chart1")
                t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))); // Calibri
            else
                t.Font = new Font("Arial", g.v.font, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))); // Calibri
        }

        public static string draw_stock_title_tick_minute(g.stock_data o, int[,] x, int start_time, int end_time)
        {
            string tick_minute_string = "";
            string stock = o.stock;

            if (stock == "KODEX 레버리지")
            {
                for (int i = 0; i < 3; i++) // draw_stock_title_tick_minute_string
                {
                    tick_minute_string += ((int)(g.kospi_틱매수배[i])).ToString() + "/" + //ToString("0.#");
                            ((int)(g.kospi_틱매도배[i])).ToString() + "  ";
                }
                tick_minute_string += "\n";
            }
            else if (stock == "KODEX 200선물인버스2X")
            {
                for (int i = 0; i < 3; i++) // draw_stock_title_tick_minute_string
                {
                    tick_minute_string += ((int)(g.kospi_틱매도배[i])).ToString() + "/" + //ToString("0.#");
                            ((int)(g.kospi_틱매수배[i])).ToString() + "  ";
                }
                tick_minute_string += "\n";
            }

            else if (stock == "KODEX 코스닥150레버리지")
            {
                for (int i = 0; i < 3; i++) // draw_stock_title_tick_minute_string
                {
                    tick_minute_string += ((int)(g.kosdaq_틱매수배[i])).ToString() + "/" + //ToString("0.#");
                            ((int)(g.kosdaq_틱매도배[i])).ToString() + "  ";
                }
                tick_minute_string += "\n";
            }
            else if (stock == "KODEX 코스닥150선물인버스")
            {
                for (int i = 0; i < 3; i++) // draw_stock_title_tick_minute_string
                {
                    tick_minute_string += ((int)(g.kosdaq_틱매도배[i])).ToString() + "/" + //ToString("0.#");
                        ((int)(g.kosdaq_틱매수배[i])).ToString() + "  ";
                }
                tick_minute_string += "\n";
            }
            else // 
            {
                // MDF 20230302
                //for (int i = 0; i < 4; i++) // draw_stock_title_tick_minute_string
                //{
                //    tick_minute_string += ((int)(o.틱매수배[i])).ToString() + "/" + //ToString("0.#");
                //            ((int)(o.틱매도배[i])).ToString() + "  ";
                //}
                //tick_minute_string += "\n";
            }


            // x[k, 8 & 9]
            for (int i = end_time - 1; i >= end_time - 5; i--)
            {
                if (i < 1)
                {
                    break;
                }
                if (i == end_time - 1)
                    tick_minute_string += x[i, 8].ToString() + "/" + //ToString("0.#");
                        x[i, 9].ToString() + "  ";
                else
                    tick_minute_string += x[i, 8].ToString() + "/" + //ToString("0.#");
                        x[i, 9].ToString() + "  ";
            }
            return tick_minute_string;
        }

        public static void draw_stock_daily(int seq, int npts_toplot, string 일주월, string stock)
        {

            int npts_toread = npts_toplot + 120; // need 120 ?
            double maxy = -10000000;
            double miny = 10000000;


            string path = @"C:\병신\data\" + 일주월 + "\\" + stock + ".txt";

            if (!File.Exists(path))
            {
                return;
            }

            int lineCount = File.ReadAllLines(path).Length;
            if (lineCount < npts_toread)
            {
                return;
            }
            List<string> lines = File.ReadLines(path).Reverse().ToList();

            // Read Data File
            double[,] x = new double[npts_toread, 10];

            int n = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                if (g.date >= Convert.ToInt32(words[0]))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        x[n, i] = Convert.ToDouble(words[i]);
                        if (i >= 1 && i <= 4)
                        {
                            if (maxy < x[n, i])
                            {
                                maxy = x[n, i];
                            }
                        }
                    }
                    n++;
                }
                if (n >= npts_toread)
                {
                    break;
                }
            }


            // Max x[,] = 100 Rearrangement of Data
            for (int i = 0; i < npts_toread; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    x[i, j] /= (maxy / 100); // divide by max and mupltiply by 100 to make 100%
                }
            }

            maxy = -10000000;
            miny = 10000000;


            // Calculate Moving Average (5, 20, 60)
            for (int k = 0; k < npts_toplot + 60; k++) // why add 60 ? 
            {
                // 5  days moving average
                x[k, 5] = 0;
                for (int i = 0; i < 5; i++)
                {
                    x[k, 5] += x[k + i, 4];
                }

                x[k, 5] = x[k, 5] / 5.0;

                // 20 days moving average
                x[k, 6] = 0;
                for (int i = 0; i < 20; i++)
                {
                    x[k, 6] += x[k + i, 4];
                }

                x[k, 6] = x[k, 6] / 20.0;

                // 60 days moving average
                x[k, 7] = 0;
                for (int i = 0; i < 60; i++)
                {
                    x[k, 7] += x[k + i, 4];
                }

                x[k, 7] = x[k, 7] / 60.0;
            }

            // Calculate Bollinger Sigma
            for (int k = 0; k < npts_toplot; k++)
            {
                // Bollinger 1 Sigma 
                double sum = 0;
                for (int i = 0; i < 20; i++)
                {
                    sum += (x[k + i, 6] - x[k + i, 4]) * (x[k + i, 6] - x[k + i, 4]);
                }

                sum /= 20.0;
                x[k, 9] = Math.Sqrt(sum); // 1 Standard Deviation
            }

            string area = seq.ToString();
            g.chart1.ChartAreas.Add(area);


            // candle stickstring id = mov.ToString();
            string sid = stock + seq.ToString() + "candle";
            Series t = new Series(sid);
            g.chart1.Series.Add(t);


            // Set series chart type
            t.ChartType = SeriesChartType.Candlestick;
            //g.chart1.Series["price"].ChartType = SeriesChartType.Candlestick;
            //g.chart1.Series["price"].ChartType = SeriesChartType.Candlestick;

            // Set the style of the open-close marks
            t["OpenCloseStyle"] = "Triangle";
            //g.chart1.Series["price"]["OpenCloseStyle"] = "Triangle";

            // Show both open and close marks
            t["ShowOpenClose"] = "Both";

            // Set point width
            t["PointWidth"] = "0.5";

            // Set colors bars
            t["PriceUpColor"] = "Red"; // <<== use text indexer for series
            t["PriceDownColor"] = "Blue"; // <<== use text indexer for series

            t.IsVisibleInLegend = false;
            t.ChartArea = area;


            for (int k = npts_toplot - 1; k >= 0; k--)
            {
                string s = "";
                if (일주월 == "일")
                {
                    s = x[k, 0].ToString();
                    s = s.Substring(s.Length - 4); // 천보 데이터 부족시 Error Occured, Because S.Lenght - 4 < 0 : Debug Later
                }

                // adding date and high
                t.Points.AddXY(s, x[k, 2]);
                // adding low
                t.Points[npts_toplot - k - 1].YValues[1] = x[k, 3];
                //adding open
                t.Points[npts_toplot - k - 1].YValues[2] = x[k, 1];
                // adding close
                t.Points[npts_toplot - k - 1].YValues[3] = x[k, 4];
            }

            // Moving Average Draw
            for (int mov = 0; mov < 3; mov++) //
            {
                string mid = "moving" + seq.ToString() + mov.ToString();
                Series m = new Series(mid); // <<== make sure to name the series "price"
                g.chart1.Series.Add(m);

                // Set series chart type
                m.ChartType = SeriesChartType.Line;

                for (int k = npts_toplot - 1; k >= 0; k--)
                {
                    string s;
                    if (일주월 == "일")
                    {
                        s = x[k, 0].ToString();
                        s = s.Substring(s.Length - 4);
                    }
                    g.chart1.Series[mid].Points.AddXY(x[k, 0].ToString(), x[k, mov + 5]);

                    if (x[k, mov + 5] > maxy)
                    {
                        maxy = x[k, mov + 5];
                    }

                    if (x[k, mov + 5] < miny)
                    {
                        miny = x[k, mov + 5];
                    }
                }

                m.IsVisibleInLegend = false;
                m.ChartArea = area;
                m.BorderWidth = 1;

                if (mov == 0)
                {
                    m.Color = Color.Red;
                }

                if (mov == 1)
                {
                    m.Color = Color.Green;
                }

                if (mov == 2)
                {
                    m.Color = Color.Blue;
                }
            }

            // Bollinger Band Draw

            for (int bol = 0; bol < 2; bol++)
            {
                if (일주월 != "일")
                {
                    break;
                }

                string bid = "bollinger" + seq.ToString() + bol.ToString();
                Series b = new Series(bid); // <<== make sure to name the series "price"
                g.chart1.Series.Add(b);

                // Set series chart type
                b.ChartType = SeriesChartType.Line;

                for (int k = npts_toplot - 1; k >= 0; k--)
                {
                    string s;
                    if (일주월 == "일")
                    {
                        s = x[k, 0].ToString();
                        s = s.Substring(s.Length - 4);
                    }

                    if (bol == 0) // lower bound curve
                    {
                        b.Points.AddXY(x[k, 0].ToString(), x[k, 6] - x[k, 9] * 2);
                        if (x[k, 6] - x[k, 9] * 2 < miny)
                        {
                            miny = x[k, 6] - x[k, 9] * 2;
                        }
                    }
                    else    // upper boudn curve
                    {
                        b.Points.AddXY(x[k, 0].ToString(), x[k, 6] + x[k, 9] * 2);
                        if (x[k, 6] + x[k, 9] * 2 > maxy)
                        {
                            maxy = x[k, 6] + x[k, 9] * 2;
                        }
                    }
                }
                b.IsVisibleInLegend = false;
                b.ChartArea = area;
                b.BorderWidth = 2; // Bollinger
                b.Color = Color.Black;
            }

            float[] location = new float[2];
            float[] size = new float[2];
            // 크기, 위치 결정
            draw_size_location(g.chart1, nRow, nCol, seq, location, size);

            g.chart1.Size = Screen.PrimaryScreen.WorkingArea.Size;
            g.chart1.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            g.chart1.ChartAreas[area].AxisY.MajorGrid.Enabled = false;

            g.chart1.ChartAreas[area].AxisX.Interval = npts_toplot - 1;
            g.chart1.ChartAreas[area].AxisX.IntervalOffset = 1;
            g.chart1.ChartAreas[area].AxisY.Interval = npts_toplot + 1000;
            g.chart1.ChartAreas[area].AxisY.IntervalOffset = -1;

            g.chart1.ChartAreas[area].AxisY.Maximum = maxy * 1.05;
            g.chart1.ChartAreas[area].AxisY.Minimum = miny * 0.95;

            g.chart1.ChartAreas[area].Position.X = location[0];
            g.chart1.ChartAreas[area].Position.Y = location[1];
            g.chart1.ChartAreas[area].Position.Width = size[0]; // width
            g.chart1.ChartAreas[area].Position.Height = size[1]; // height


            // Legend

            int index = wk.return_index_of_ogldata(stock);
            if (index < 0)
            {
                return;
            }

            string label_title = draw_foreign_institute_price_daily_stock_title(stock, index, x, 0);
            label_title = stock;

            g.chart1.Series.Add(label_title); // empty series assigned for legend
            g.chart1.Legends.Add(new Legend(label_title));
            g.chart1.Legends[label_title].Position.X = location[0];
            g.chart1.Legends[label_title].Position.Y = location[1];
            g.chart1.Legends[label_title].Position.Width = 12.0F;
            g.chart1.Legends[label_title].Position.Height = 1.5F; // was 1.5
            g.chart1.Series[label_title].Legend = label_title;
            g.chart1.Series[label_title].BorderWidth = 1;
            g.chart1.Series[label_title].IsVisibleInLegend = true;
            g.chart1.Series[label_title].MarkerBorderWidth = 0;
        }

        public static void draw_multiple(int seq, List<string> 종목들)
        {
            if (종목들.Count == 0 || 종목들 == null)
                return;

            List<string> stocklist = new List<string>();   // changing single list

            for (int i = 0; i < 종목들.Count; i++)
            {
                stocklist.Add(종목들[i]);
                if (stocklist.Count == 4)
                    break;
            }

            float[] size = new float[2];
            float[] location = new float[2];

            // 크기, 위치 결정
            draw_size_location(g.chart1, nRow, nCol, seq, location, size);

            string sid = "";
            int[,] x = new int[383, 5];

            string area = seq.ToString();
            g.chart1.ChartAreas.Add(area);

            int max_time = 0;
            int[] npts = new int[4];

            if (stocklist.Count >= 2)
            {
                for (int i = 0; i < stocklist.Count; i++)
                {
                    g.stock_data o = null;

                    int index = wk.return_index_of_ogldata(stocklist[i]);

                    if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
                    {
                        return;
                    }
                    else
                    {
                        o = g.ogl_data[index];
                        npts[i] = o.nrow;
                        if (max_time < npts[i])
                            max_time = npts[i];
                    }
                }
            }

            for (int i = 0; i < stocklist.Count; i++)
            {
                g.stock_data o = null;

                int index = wk.return_index_of_ogldata(stocklist[i]);

                if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
                {
                    return;
                }
                else
                {
                    o = g.ogl_data[index];
                }
                for (int j = 0; j < o.nrow; j++)
                {
                    if (i == 0)
                    {
                        x[j, 0] = o.x[j, 0]; // time copy once, if i = 0
                    }
                    x[j, i + 1] = o.x[j, 1]; // price of each stock only copy
                }
            }

            for (int i = 0; i < stocklist.Count; i++)
            {
                sid = stocklist[i] + area + " " + i.ToString();

                Series t = new Series(sid);
                g.chart1.Series.Add(t);

                int npoint = 0;
                for (int k = g.time[0]; k < g.time[1]; k++)
                {
                    if (x[k, 0] <= 0)
                    {
                        break;
                    }

                    if (x[0, 0] < 90000)
                    {
                        for (int m = 2; m <= 3; m++) // 수급, 강도 너무 큰 값 가질 수 있음
                        {
                            x[0, m] = x[1, m];
                        }
                    }
                    t.Points.AddXY(((int)(x[k, 0] / g.HUNDRED)).ToString(), x[k, i + 1]);
                    npoint++;
                }
                if (npoint < 1) // in case the file is empty 
                {
                    continue;
                }

                g.stock_data o = new g.stock_data();
                int index = wk.return_index_of_ogldata(stocklist[i]);
                if (index >= 0)
                    o = g.ogl_data[index];
                else
                    o = null;
                if (o.분거래천[0] < 1)
                {
                }
                else if (o.분거래천[0] < 5)
                {
                    t.Points[0].MarkerColor = Color.Red;
                    t.Points[0].MarkerSize = 10;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 10)
                {
                    t.Points[0].MarkerColor = Color.Red;
                    t.Points[0].MarkerSize = 15;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 20)
                {
                    t.Points[0].MarkerColor = Color.Green;
                    t.Points[0].MarkerSize = 15;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 30)
                {
                    t.Points[0].MarkerColor = Color.Green;
                    t.Points[0].MarkerSize = 20;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 50)
                {
                    t.Points[0].MarkerColor = Color.Blue;
                    t.Points[0].MarkerSize = 20;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 100)
                {
                    t.Points[0].MarkerColor = Color.Blue;
                    t.Points[0].MarkerSize = 30;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 150)
                {
                    t.Points[0].MarkerColor = Color.Blue;
                    t.Points[0].MarkerSize = 40;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else if (o.분거래천[0] < 200)
                {
                    t.Points[0].MarkerColor = Color.Blue;
                    t.Points[0].MarkerSize = 50;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }
                else
                {
                    t.Points[0].MarkerColor = Color.Blue;
                    t.Points[0].MarkerSize = 60;
                    //t.Points[0].MarkerStyle = MarkerStyle.Circle;
                }

                if (g.time[1] > 1)
                {
                    int price_change = o.x[g.time[1] - 1, 1] - o.x[g.time[1] - 2, 1];
                    if (price_change >= 0)
                        t.Points[0].MarkerStyle = MarkerStyle.Circle;
                    else
                        t.Points[0].MarkerStyle = MarkerStyle.Cross;
                }
                else
                    t.Points[0].MarkerStyle = MarkerStyle.Circle;


                g.chart1.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
                g.chart1.ChartAreas[area].AxisX.Interval = npoint - 1;
                g.chart1.ChartAreas[area].AxisX.IntervalOffset = 1;
                g.chart1.ChartAreas[area].AxisX.LabelStyle.Enabled = true;

                t.Points[npoint - 1].Label = "    " + stocklist[i];

                //t.Points[npoint - 1].Label = "    " + (i + 1).ToString();

                t.BorderWidth = 2; // multiple

                // ChartArea, ChartType, BorderWidth
                t.ChartArea = area;
                t.ChartType = SeriesChartType.Line;
                t.XValueType = ChartValueType.Date;

                // Line Color 
                switch (i)
                {
                    case 0:
                        t.Color = Color.Red; // multiple
                        break;
                    case 1:
                        t.Color = Color.Green; // multiple
                        break;
                    case 2:
                        t.Color = Color.Blue; // multiple
                        break;
                    case 3:
                        t.Color = Color.Purple; // multiple
                        break;
                    case 4:
                        t.Color = Color.Magenta; // multiple
                        break;
                    default:
                        break;
                }
                t.IsVisibleInLegend = false;
            }

            // Area Setting
            g.chart1.ChartAreas[seq.ToString()].Position.X = location[0];
            g.chart1.ChartAreas[seq.ToString()].Position.Y = location[1];
            g.chart1.ChartAreas[seq.ToString()].Position.Width = size[0];
            g.chart1.ChartAreas[seq.ToString()].Position.Height = size[1];
        }

        public static void draw_foreign_institute_price(int seq, int npts_toplot, string stock)
        {
            //stock = "삼성전자";

            string path = @"C:\병신\data\일\\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return;
            }

            List<string> lines = File.ReadLines(path).Reverse().Take(npts_toplot).ToList();

            // read data file
            double[,] x = new double[npts_toplot, 5];
            int n = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                x[n, 0] = Convert.ToDouble(words[0]); // 일자
                x[n, 1] = Convert.ToDouble(words[4]); // 종가
                x[n, 2] = Convert.ToDouble(words[8]); // 외인
                if (stock == "삼성전자")
                {
                    x[n, 2] *= 1;
                }

                x[n, 3] = Convert.ToDouble(words[9]); // 기관
                x[n, 4] = (x[n, 2] + x[n, 3]) * -1.0; // 개인

                n++;
            }

            if (n < npts_toplot)
            {
                npts_toplot = n;
            }

            double pmax = 0.0;        // 종가 절대값 최대치
            double nmax = 0.0;
            for (int k = 0; k < n; k++)
            {
                for (int i = 1; i <= 4; i++)
                {

                    if (i == 1)
                    {
                        x[k, i] = (x[k, i] - x[n - 1, i]) / x[n - 1, i] * 100;
                        if (pmax < Math.Abs(x[k, i]))  // 가격 최대
                        {
                            pmax = Math.Abs(x[k, i]);
                        }
                    }
                    else
                    {
                        x[k, i] -= x[n - 1, i];
                        if (nmax < Math.Abs(x[k, i])) // 외인 기관 개인 최대수량
                        {
                            nmax = Math.Abs(x[k, i]);
                        }
                    }
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 2; i <= 4; i++)
                {
                    if (pmax != 0)
                        x[k, i] /= (nmax / pmax); // Zero Divider ? 
                }
            }

            string area = seq.ToString();
            g.chart1.ChartAreas.Add(area);
            // Draw Lines
            for (int i = 1; i <= 4; i++)
            {
                string sid = stock + i.ToString(); //종가, 외인, 기관, 개인 순서
                Series t = new Series(sid);
                g.chart1.Series.Add(t);

                if (i == 1)
                {
                    t.Color = Color.Red; // foreign
                    t.BorderWidth = 1; // foreign
                }

                if (i == 2)
                {
                    t.Color = Color.Magenta; // foreign
                    t.BorderWidth = 2; // foreign
                }

                if (i == 3)
                {
                    t.Color = Color.Cyan; // foreign
                    t.BorderWidth = 2; // foreign
                }

                if (i == 4)
                {
                    t.Color = Color.Black; // foreign
                    t.BorderWidth = 2; // foreign
                }

                for (int k = npts_toplot - 1; k >= 0; k--)
                {
                    string s = "";
                    {
                        s = x[k, 0].ToString();
                        if (s == "0")
                        {
                            break;
                        }

                        s = s.Substring(s.Length - 4);
                    }
                    t.Points.AddXY(s, x[k, i]);
                }
                t.ChartType = SeriesChartType.Line;
                t.IsVisibleInLegend = false;


                /*
                if(i==4)
                    t.BorderWidth = 2;
                    */

                t.ChartArea = area;


            }

            float[] location = new float[2];
            float[] size = new float[2];
            // 크기, 위치 결정
            draw_size_location(g.chart1, nRow, nCol, seq, location, size);

            g.chart1.Size = Screen.PrimaryScreen.WorkingArea.Size;
            g.chart1.ChartAreas[area].AxisX.MajorGrid.Enabled = false;
            g.chart1.ChartAreas[area].AxisY.MajorGrid.Enabled = false;
            g.chart1.ChartAreas[area].Position.X = location[0];
            g.chart1.ChartAreas[area].Position.Y = location[1];
            g.chart1.ChartAreas[area].Position.Width = size[0]; // width
            g.chart1.ChartAreas[area].Position.Height = size[1];// height
            g.chart1.ChartAreas[area].AxisX.Interval = npts_toplot - 1;
            g.chart1.ChartAreas[area].AxisX.IntervalOffset = 1;


            // Legend
            int index = wk.return_index_of_ogldata(stock);
            if (index < 0)
            {
                return;
            }
            string label_title = draw_foreign_institute_price_daily_stock_title(stock, index, x, 0);
            if (index >= 0)
            {
                if (g.test && g.ogl_data[index].selected_for_group == false)
                {
                    label_title = "$$$ " + label_title;
                }
            }


            g.chart1.Series.Add(label_title); // empty series assigned for legend
            g.chart1.Legends.Add(new Legend(label_title));
            g.chart1.Legends[label_title].Position.X = location[0];
            g.chart1.Legends[label_title].Position.Y = location[1];
            g.chart1.Legends[label_title].Position.Width = 12.0F;
            g.chart1.Legends[label_title].Position.Height = 1.50F;
            g.chart1.Series[label_title].Legend = label_title;
            g.chart1.Series[label_title].BorderWidth = 1;
            g.chart1.Series[label_title].IsVisibleInLegend = true;
            g.chart1.Series[label_title].MarkerBorderWidth = 0;
        }

        public static string draw_foreign_institute_price_daily_stock_title(string stock, int index, double[,] x, int end_time)
        {
            int 당외량백분율 = 0;
            int 당기량백분율 = 0;

            int 시총 = (int)g.ogl_data[index].시총; // this is double type, so converted to integer
            int 종가기준추정누적거래액_천만원 = 0;


            string label_title = "";
            if (g.q != "h&s")
            {
                if (g.보유종목.Contains(stock))
                {
                    label_title += "&&" + " ";
                }
                if (g.호가종목.Contains(stock))
                {
                    label_title += "^^" + " ";
                }

                if (g.관심종목.Contains(stock))
                {
                    label_title += "**" + " ";
                }

                label_title += stock + " " + 종가기준추정누적거래액_천만원 + " " + 시총 + " " +
                    당외량백분율.ToString() + " " + 당기량백분율.ToString();
            }
            return label_title;
        }
    }
}

#region
//public static void draw_stock_general_factors(g.stock_data o, int start_time, int end_time, double[] factors)
//{
//    double[] abs_max = new double[9];
//    double abs_value = 0;

//    for (int j = 1; j < 9; j++)
//    {
//        if (j < 7)
//        {
//            for (int i = start_time; i < end_time; i++)
//            {
//                switch (j)
//                {
//                    case 1:
//                    case 2:
//                    case 3:
//                        switch (j)
//                        {
//                            case 1:
//                                abs_value = Math.Abs(o.x[i, j]); // price, supply, strength
//                                break;
//                            case 2:
//                                abs_value = Math.Log(o.x[i, j]); // price, supply, strength
//                                break;
//                            case 3:
//                                abs_value = Math.Log(o.x[i, j] / 100); // price, supply, strength
//                                break;
//                        }
//                        if (abs_max[j] < abs_value)
//                            abs_max[j] = abs_value;
//                        break;
//                    case 4:
//                    case 5:
//                    case 6:
//                        if (o.x[end_time - 1, 7] > 0)
//                            abs_value = Math.Abs((double)o.x[i, j] / o.x[end_time - 1, 7]); // money
//                        if (abs_max[4] < abs_value)
//                            abs_max[4] = abs_value;
//                        break;
//                }
//            }
//        }

//        if (j == 8)
//        {
//            for (int i = start_time; i < end_time; i++)
//            {
//                abs_value = Math.Abs(o.x[i, 8] - o.x[i, 9]); // 배수차
//                if (abs_max[8] < abs_value)
//                    abs_max[8] = abs_value;
//            }
//        }
//    }

//    for (int j = 2; j < 9; j++)
//    {
//        if (abs_max[j] > g.EPS)
//            factors[j] = (double)abs_max[1] / abs_max[j];
//        else
//            factors[j] = 1.0;
//    }
//    factors[1] = 1.0;
//    factors[2] = factors[2] * 0.5;
//    factors[3] = factors[3] * 0.5;
//    factors[4] = 0.0;
//    factors[4] = factors[4] * 0.7;
//    factors[5] = factors[4];
//    factors[6] = factors[4];
//    factors[8] = factors[8] * 0.5;
//}
#endregion

