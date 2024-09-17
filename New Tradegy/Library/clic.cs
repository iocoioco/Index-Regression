
using CPSYSDIBLib;
using MathNet.Numerics.Random;
using New_Tradegy.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace New_Tradegy.Library
{
    internal class cl
    {

        private static CPUTILLib.CpCybos _cpcybos;

        public static string CoordinateMapping(Chart chart, int nRow, int nCol, List<string> displayList, MouseEventArgs e, ref string selection, ref int cellX, ref int cellY)
        {
            double x_min = 0.0;
            double y_min = 0.0;
            double x_max = chart.Bounds.Width;
            double y_max = chart.Bounds.Height;
            string clickedStock = null;
            if (chart.Name == "chart2")
            {
                y_min = 31;
            }
            // Normalize coordinates to a (100, 100) system
            double norm_x = (e.X) / (x_max - x_min) * 100.0;
            double norm_y = (e.Y - y_min) / (y_max - y_min) * 100.0;

            // Clamp normalized coordinates to the range [0, 100]
            norm_x = Math.Max(0, Math.Min(100, norm_x));
            norm_y = Math.Max(0, Math.Min(100, norm_y));


            // Find cell size in the normalized system
            double cellWidth = 100.0 / nCol;


            // Determine the cell address
            cellX = (int)(norm_x / cellWidth);

            double cellHeight = 100.0 / nRow; // if chart2
            if (chart.Name == "chart1")
            {
                if (cellX == 0)
                    cellHeight = 100.0 / 2.0;

                else
                    cellHeight = 100.0 / nRow;
            }

            cellY = (int)(norm_y / cellHeight);

            // Ensure cell indices are within grid bounds
            cellX = Math.Min(cellX, nCol - 1);
            cellY = Math.Min(cellY, nRow - 1);

            // Calculate percentage within the cell from the left-bottom corner
            double percentageXInCell = (norm_x % cellWidth) / cellWidth * 100.0;
            double percentageYInCell = (norm_y % cellHeight) / cellHeight * 100.0;

            // Determine final address based on percentage conditions
            if (e.Button == MouseButtons.Left)
            {
                selection = "l";
            }
            else
            {
                selection = "r";
            }

            int finalAddress = 9;
            if (percentageXInCell > 66.66)
            {
                if (percentageYInCell < 33.33)
                    finalAddress = 1;
                else if (percentageYInCell < 66.66)
                    finalAddress = 4;
                else
                    finalAddress = 7;
            }
            else if (percentageXInCell > 33.33)
            {
                if (percentageYInCell < 33.33)
                    finalAddress = 2;
                else if (percentageYInCell < 66.66)
                    finalAddress = 5;
                else
                    finalAddress = 8;
            }
            else
            {
                if (percentageYInCell < 33.33)
                    finalAddress = 3;
                else if (percentageYInCell < 66.66)
                    finalAddress = 6;
            }
            selection += finalAddress.ToString();

            // Find clicked stock
            if (chart.Name == "chart1")
            {
                if (g.q == "h&s")
                    clickedStock = g.clickedStock;
                else
                    if (cellX == 0)
                    clickedStock = displayList[cellY];
                else
                    clickedStock = displayList[nRow * cellX + cellY - 1]; // first col -1, second col -3
            }
            else
            {
                if (g.q == "h&s")
                    clickedStock = g.clickedStock;
                else // including "h&g"
                {
                    if (nRow * cellX + cellY < displayList.Count)
                        clickedStock = displayList[nRow * cellX + cellY];
                }
            }
            return clickedStock;
        }





        public static Form GetActiveForm()
        {
            // Returns null for an MDI app
            Form activeForm = Form.ActiveForm;
            if (activeForm == null)
            {

                FormCollection openForms = Application.OpenForms;
                for (int i = 0; i < openForms.Count && activeForm == null; ++i)
                {
                    Form openForm = openForms[i];
                    string t = openForm.Name;
                    if (openForm.IsMdiContainer)
                    {
                        activeForm = openForm.ActiveMdiChild;
                    }
                }
            }
            return activeForm;
        }
        private static int TryGetPrice(string stock, int maxAttempts, int delayMilliseconds)
        {
            int price = 0;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                price = hg.HogaGetValue(stock, -1, 1); // -1 : 매도1호가 라인, 1 : column
                if (price > 0)
                {
                    return price;
                }

                attempts++;
                System.Threading.Thread.Sleep(delayMilliseconds); // Wait before retrying
            }
            return -1;
            // Handle the case where a valid price was not obtained
            // throw new Exception("Failed to get a valid price after multiple attempts.");
        }

        public static void CntlLeftRightAction(Chart chart, string selection, int row_id, int col_id)
        {
            switch (selection)
            {
                case "l1":
                    // int Price = hg.HogaGetValue(g.clickedStock, -1, 2); // -1 : 매도1호가 라인, 1 : column



                    int Price = TryGetPrice(g.clickedStock, 5, 100); // Try up to 5 times with 100 millisecond delay
                    if (Price < 0) return;

                    int Amount = g.일회거래액 * 10000 / Price;
                    if (Amount == 0)
                    {
                        Amount = 1;
                    }

                    ms.Sound_돈(g.일회거래액);


                    str = "";
                    if (g.confirm_buy)
                    {
                        str += g.clickedStock + " : " + Price.ToString() + " X " + Amount.ToString() +
                                   " = " + (Price * Amount / 10000).ToString() + "만원";

                        int index = wk.return_index_of_ogldata(g.clickedStock);
                        if (index < 0) return;
                        g.stock_data o = g.ogl_data[index];

                        str += "\n" + sr.r3_display_매수_매도(o);


                        using (var form = new Form_매수_매도(g.clickedStock, "매수 ?", 100, g.cancelThreshhold, str))
                        {
                            DialogResult result = form.ShowDialog();

                            if (result != DialogResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                g.cancelThreshhold = form.The_cancelthreshhold;
                            }
                        }
                    }

                    dl.deal_exec("매수", g.clickedStock, Amount, Price, "01"); // Cntl + l1 


                    break;

                case "l2":

                    break;

                case "l3":

                    break;

                case "l4":

                    break;

                case "l5":

                    break;

                case "l6":

                    break;

                case "l7": // g.time[1]++

                    break;


                case "l8":

                    break;

                case "l9": // g.time[1]++

                    break;

                case "r1": // News

                    break;

                case "r2":// naver chart only

                    break;

                case "r3": // 정보

                    break; ;

                case "r4": // Naver main

                    break;

                case "r5": // 관심 추가 제거

                    break;

                // 종목의 컬럼상 중앙, 로우상 2/3 이상 을 클릭한 경우, 외인 수급 파일 열기
                case "r6":

                    break;

                case "r7":

                    break;

                case "r8": // naver 외,기 또는 매동정보

                    break;

                case "r9": // 20201112

                    break;
            }
        }

        public static void LeftRightAction(Chart chart, string selection, int row_id, int col_id)
        {
            // int chart_id = ms.sender_to_chart_id(chart.Name);

            int index = wk.return_index_of_ogldata(g.clickedStock);
            if (index < 0) return;
            g.stock_data o = g.ogl_data[index];


            switch (selection)
            {
                case "l1":
                    if (o.shrink_draw)
                    {
                        o.shrink_draw = false;
                    }
                    else
                    {
                        o.shrink_draw = true;
                    }
                    break;

                case "l2":


                    if (g.KODEX4.Contains(g.clickedStock))
                    {
                        // delete all existing forms with name of Form_지수_조정
                        var list = hg.FormNameContainGivenString("Form_지수_조정");
                        foreach (var form in list)
                        {
                            form.Dispose();
                        }

                        int w = g.chart1.Bounds.Width;
                        int h = g.chart1.Bounds.Height;

                        Form Form_지수_조정_ = new Form_지수_조정(g.clickedStock);

                        Form_지수_조정_.WindowState = FormWindowState.Normal;
                        Form_지수_조정_.StartPosition = FormStartPosition.Manual;
                        Form_지수_조정_.Location = new Point((int)(w / 6), h / 2 - h / 4 / 2);
                        Form_지수_조정_.Size = new Size(w / 4, h / 4);
                        Form_지수_조정_.Show();
                        //Form_지수_조정_.TopMost = true;
                        return;
                    }
                    else
                    {
                        o.수급과장배수 *= 1.5;
                    }
                    break;

                case "l3":
                    //if (g.clickedStock.Contains("KODEX 레버리지"))
                    //    wn.Memo_TopMost();
                    //else 

                    if (g.test)
                    {
                        g.time[0] = 0;
                        g.time[1] = g.MAX_ROW;
                    }
                    break;

                case "l4":
                    if (g.test) // 짧은 시간 앞으로 in draw(테스트)
                    {
                        if (g.end_time_before_advance == 0) // time extension
                            dr.draw_extended_time(g.v.q_advance_lines);
                        else
                            dr.draw_extended_time(0);
                        // wk.BringToFront();
                    }
                    break;

                case "l5":
                    if (!g.connected) // 시간 뒤로 (테스트)
                    {
                        if (g.end_time_before_advance == 0) // time extension
                            dr.draw_extended_time(g.v.Q_advance_lines);
                        else
                            dr.draw_extended_time(0);
                    }
                    else
                    {

                        // KODEX 레버리지 호가창


                        if (g.clickedStock == g.KODEX4[0] || g.clickedStock == g.KODEX4[2]) // not leverage
                        {
                            if (!hg.HogaInsert(g.clickedStock, 5, row_id, col_id))
                            {
                                return; // not inserted
                            }
                        }
                        else
                        {
                            if (!g.보유종목.Contains(g.clickedStock))
                            {
                                if (g.호가종목.Contains(g.clickedStock))
                                {
                                    g.호가종목.Remove(g.clickedStock);

                                    // Remove all buy orders with the given stock name
                                    StockExchange.buyOrders.RemoveAll(order => order.Stock == g.clickedStock);

                                    // Remove all sell orders with the given stock name
                                    StockExchange.sellOrders.RemoveAll(order => order.Stock == g.clickedStock);

                                    dr.draw_chart();
                                }
                                else
                                {
                                    g.호가종목.Add(g.clickedStock);
                                    hg.HogaInsert(g.clickedStock, 5, row_id, col_id);
                                    if (g.관심종목.Contains(g.clickedStock))
                                    {
                                        g.관심종목.Remove(g.clickedStock);
                                    }

                                    dr.draw_chart();
                                }
                            }
                            return;
                        }
                    }
                    break;

                case "l6":
                    if (!g.보유종목.Contains(g.clickedStock))
                    {
                        if (g.호가종목.Contains(g.clickedStock))
                        {
                            g.호가종목.Remove(g.clickedStock);
                            g.관심종목.Add(g.clickedStock);
                        }
                        else
                        {
                            if (g.관심종목.Contains(g.clickedStock))
                            {
                                g.관심종목.Remove(g.clickedStock);
                            }
                            else
                            {
                                g.관심종목.Add(g.clickedStock);
                            }
                        }
                        dr.draw_chart();
                    }
                    break;

                case "l7": // g.time[1]++
                    if (g.test)
                    {
                        g.time[1]++;
                        if (g.time[1] > g.MAX_ROW)
                        {
                            g.time[0] = 0;
                            g.time[1] = 2;
                        }
                        // wk.BringToFront();
                    }
                    else
                    {

                    }
                    break;


                case "l8":
                    o.수급과장배수 *= 0.66;
                    break;

                case "l9": // g.time[1]++

                    //if (g.clickedStock.Contains("KODEX 레버리지"))
                    //    wn.Memo_TopMost();
                    //else 

                    if (g.test)
                    {
                        g.time[1]--;
                        if (g.time[1] < 2)
                        {
                            g.time[0] = 0;
                            g.time[1] = 2;
                        }
                        // wk.BringToFront();
                    }
                    else
                    {
                        if (g.clickedStock == g.KODEX4[0])
                        {
                            if (!hg.HogaInsert(g.KODEX4[1], 5, row_id, col_id))
                            {
                                return; // not inserted
                            }
                        }
                        if (g.clickedStock == g.KODEX4[2])
                        {
                            if (!hg.HogaInsert(g.KODEX4[3], 5, row_id, col_id))
                            {
                                return; // not inserted
                            }
                        }
                    }
                    break;

                case "r1": // News
                    string t = "http://google.com/search?q=" + g.clickedStock + " 뉴스 " + "주식" + "&tbs=qdr:" + g.PeoridNews; // qdr:w, m, d, h
                    Process.Start(t);


                    // the followings are suggested by ChatGpt to exact
                    //string encodedQuery = Uri.EscapeDataString(g.clickedStock + " 뉴스 " + "주식");
                    //string t = $"http://google.com/search?q={encodedQuery}&tbs=qdr:{g.PeoridNews}";
                    //Process.Start("chrome.exe", t);

                    break;

                case "r2":// naver chart only
                    wk.call_네이버(g.clickedStock, "fchart");

                    break;

                case "r3": // 정보
                    string query = g.clickedStock + " 잡코리아 " + "기업정보";
                    string encodedQuery = Uri.EscapeDataString(query);
                    t = "http://google.com/search?q=" + encodedQuery;
                    Process.Start("chrome.exe", t);

                    break; ;

                case "r4": // Naver main
                    wk.call_네이버(g.clickedStock, "main");
                    break;

                case "r5": // 관심 추가 제거
                           // URLs you want to open
                           //string url1 = "https://finance.naver.com/item/main.nhn?code=005930";
                           //string url2 = "https://finance.naver.com/item/main.nhn?code=005930";
                           //string url3 = "https://finance.naver.com/item/main.nhn?code=005930";

                    //// Open the URLs in the default browser
                    //Process.Start(new ProcessStartInfo("cmd", $"/c start {url1}") { CreateNoWindow = true });
                    //Process.Start(new ProcessStartInfo("cmd", $"/c start {url2}") { CreateNoWindow = true });
                    //Process.Start(new ProcessStartInfo("cmd", $"/c start {url3}") { CreateNoWindow = true });
                    break;

                // 종목의 컬럼상 중앙, 로우상 2/3 이상 을 클릭한 경우, 외인 수급 파일 열기
                case "r6":
                    // h&s 진출
                    if (g.q == "h&s")
                    {
                        g.q = g.saved_q;

                        g.Gid = g.saved_Gid;
                        g.date = g.saved_date;

                        if (g.test)
                        {
                            g.time[0] = g.saved_time[0];
                            g.time[1] = g.saved_time[1];
                        }
                        else
                        {
                            g.time[0] = 0;
                            g.time[1] = g.MAX_ROW;
                        }

                        int month = g.date % 10000 / 100;
                        int day = g.date % 10000 % 100;
                        g.제어.dtb.Rows[0][0] = month.ToString() + "/" + day.ToString();
                    }
                    // h&s 진입
                    else
                    {
                        g.saved_q = g.q;
                        g.q = "h&s";

                        g.saved_Gid = g.Gid;
                        g.saved_date = g.date;
                        g.moving_reference_date = g.date;

                        if (g.test)
                        {
                            g.saved_time[0] = g.time[0];
                            g.saved_time[1] = g.time[1];
                        }
                        else
                        {
                            g.time[0] = 0;
                            g.time[1] = g.MAX_ROW;
                        }

                    }
                    break;

                case "r7":
                    //Form f = (Form)(hg.FormNameGivenStock(g.clickedStock));
                    //f.Location = new Point(f.Location.X, f.Location.Y + 100);
                    if (g.v.Screens >= 2)
                        sr.r3_display_lines(chart, g.clickedStock, row_id, col_id);
                    //wk.BringToFront();

                    break;

                case "r8": // naver 외,기 또는 매동정보
                    wk.call_네이버(g.clickedStock, "frgn");
                    break;

                case "r9": // 20201112

                    if (g.clickedStock == "KODEX 레버리지")

                    {
                        //dl.deal_호가("KODEX 200선물인버스2X");
                    }
                    else if (g.clickedStock == "KODEX 코스닥150레버리지")
                    {
                        // dl.deal_호가("KODEX 코스닥150선물인버스");
                    }
                    else
                    {
                        bool found = false;
                        for (int j = 0; j < g.oGL_data.Count; j++)
                        {
                            if (g.oGL_data[j].stocks.Contains(g.clickedStock))
                            {
                                g.clickedTitle = g.oGL_data[j].title;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            return;
                        //}
                        dr.draw_보조_차트("상관");
                    }
                    break;
            }

            if (
                selection == "l4" ||
                 selection == "l5")
            {
                ps.post_all(); // clic l4, l5
            }

            if (
                selection == "l7" ||
                selection == "l9")
            {
                ev.eval_stock();
            }

            if (selection == "l1" ||
               selection == "l2" ||
                selection == "l3" ||
               selection == "l4" ||
               selection == "l5" ||
                selection == "l7" ||
                selection == "l8" ||
                selection == "l9" ||
                selection == "r5" ||
                selection == "r6" ||
                selection == "r9")
            {
                dr.draw_chart();
            }

            if (selection == "l1" ||
             selection == "l2" ||
             selection == "l3" ||
             selection == "l4" ||
             selection == "l5" ||
              selection == "l7" ||
              selection == "l8" ||
              selection == "l9")
            { dr.draw_보조_차트(); }

            //wk.BringToFront();
            Form f = null;
            f = hg.FormContainGivenString("se");
            if (f != null)
            {
                f.Activate();
            }
        }

        public static int RemainSB()
        {
            _cpcybos = new CPUTILLib.CpCybos();
            if (_cpcybos == null)
                return 400;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_SUBSCRIBE);               // 400건의 요청으로 제한   
        }
    }


}
