using New_Tradegy.Library.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library
{
    internal class ChartClickHandler
    {
        private static CPUTILLib.CpCybos _cpcybos;

        private static int TryGetPrice(string stockCode, int maxAttempts, int delayMs)
        {
            int price = 0;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                price = hg.HogaGetValue(stockCode, -1, 1); // -1: 매도1호가, 1: column
                if (price > 0) return price;
                attempts++;
                System.Threading.Thread.Sleep(delayMs);
            }
            return -1;
        }

       

        public static void HandleControlClick(Chart chart, string regionKey, int row, int col)
        {
            string postEvalFlags = "    ";
            switch (regionKey)
            {
                case "l1":
                    if (!g.보유종목.Contains(g.clickedStock) && !g.호가종목.Contains(g.clickedStock))
                    {
                        g.호가종목.Add(g.clickedStock);
                    }
                    mm.ManageChart1();

                    int price = TryGetPrice(g.clickedStock, 5, 100);
                    if (price < 0) return;

                    int quantity = g.일회거래액 * 10000 / price;
                    if (quantity == 0) quantity = 1;

                    mc.Sound_돈(g.일회거래액);

                    string info = g.clickedStock + " : " + price + " X " + quantity +
                                  " = " + (price * quantity / 10000) + "만원";

                    var stockData = StockRepository.Instance.GetOrThrow(g.clickedStock);
                    if (stockData == null) return;

                    info += "\n" + sr.r3_display_매수_매도(stockData);

                    if (DealManager.CheckPreviousLoss(g.clickedStock)) return;

                    var confirmationDialog = new jp();
                    confirmationDialog.OpenOrUpdateConfirmationForm(false, g.clickedStock, quantity, price, 100, info);

                    DealManager.deal_exec("매수", g.clickedStock, quantity, price, "03");
                    break;

                case "l2":
                    int magnifierIdx = g.KODEX4.FindIndex(code => code == g.clickedStock);
                    if (magnifierIdx >= 0)
                    {
                        g.kodex_magnifier[magnifierIdx, 0] *= 1.333;
                        postEvalFlags = "d  B";
                    }
                    break;

                case "l8":
                    magnifierIdx = g.KODEX4.FindIndex(code => code == g.clickedStock);
                    if (magnifierIdx >= 0)
                    {
                        g.kodex_magnifier[magnifierIdx, 0] *= 0.666;
                        postEvalFlags = "d  B";
                    }
                    break;

                case "l4":
                    if (g.test)
                    {
                        mm.MinuteAdvanceRetreat(g.EndNptsBeforeExtend == 0 ? g.v.Q_advance_lines : 0);
                        postEvalFlags = " p B";
                    }
                    break;

                case "r4":
                    if (g.test)
                    {
                        g.Npts[1]--;
                        if (g.Npts[1] < 2)
                        {
                            g.Npts[0] = 0;
                            g.Npts[1] = 2;
                        }
                        postEvalFlags = " peB";
                    }
                    break;

                case "r9":
                    // Placeholder for additional logic if needed
                    break;
            }

            if (postEvalFlags[0] != ' ')
            {
                if (postEvalFlags[0] == 'm' || postEvalFlags[0] == 'B')
                    mm.ClearChartAreaAndAnnotations(g.ChartManager.Chart1, g.clickedStock);
                if (postEvalFlags[0] == 's' || postEvalFlags[0] == 'B')
                    mm.ManageChart2();
            }
            if (postEvalFlags[1] != ' ')
                ps.post_test();
            if (postEvalFlags[2] != ' ')
                ev.eval_stock();
            if (postEvalFlags[3] != ' ')
            {
                if (postEvalFlags[3] == 'm' || postEvalFlags[3] == 'B')
                    mm.ManageChart1();
                if (postEvalFlags[3] == 's' || postEvalFlags[3] == 'B')
                    mm.ManageChart2();
            }
        }


        public static void HandlerClick(Chart chart, string selection, int row_id, int col_id)
        {
            var stockData = StockRepository.Instance.GetOrThrow(g.clickedStock);
            if (stockData == null) return;

            string action = "    ";
            switch (selection)
            {
                case "l1":
                    stockData.ShrinkDraw = !stockData.ShrinkDraw;
                    action = "B  B";
                    break;

                case "l2":
                    if (g.KODEX4.Contains(g.clickedStock))
                    {
                        var list = hg.FormNameContainGivenString("Form_지수_조정");
                        foreach (var form in list) form.Dispose();

                        int w = g.ChartManager.Chart1.Bounds.Width;
                        int h = g.ChartManager.Chart1.Bounds.Height;

                        var formAdjust = new Form_지수_조정(g.clickedStock)
                        {
                            WindowState = FormWindowState.Normal,
                            StartPosition = FormStartPosition.Manual,
                            Location = new Point((int)(w / 6), h / 2 - h / 4 / 2),
                            Size = new Size(w / 4, h / 4)
                        };
                        formAdjust.Show();
                        return;
                    }
                    else
                    {
                        stockData.수급과장배수 *= 1.5;
                        action = "B  B";
                    }
                    break;

                case "l3":
                    string dirPath = @"C:\병신\data\Stock Memo";
                    string filePath = Path.Combine(dirPath, g.clickedStock + ".txt");
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    if (!File.Exists(filePath)) File.Create(filePath).Dispose();
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    break;

                case "l4":
                    if (g.test)
                    {
                        mm.MinuteAdvanceRetreat(g.EndNptsBeforeExtend == 0 ? g.v.q_advance_lines : 0);
                        action = " p B";
                    }
                    break;

                case "l5":
                    if (g.clickedStock == g.KODEX4[0] || g.clickedStock == g.KODEX4[2])
                    {
                        wk.deleteChartAreaAnnotation(g.ChartManager.Chart1, g.clickedStock);
                        if (g.clickedStock == g.KODEX4[0]) mm.fixedStocks[0] = g.KODEX4[1];
                        if (g.clickedStock == g.KODEX4[1]) mm.fixedStocks[0] = g.KODEX4[0];
                        if (g.clickedStock == g.KODEX4[2]) mm.fixedStocks[1] = g.KODEX4[3];
                        if (g.clickedStock == g.KODEX4[3]) mm.fixedStocks[1] = g.KODEX4[2];
                        action = "   B";
                    }
                    else
                    {
                        if (!g.보유종목.Contains(g.clickedStock))
                        {
                            if (g.호가종목.Contains(g.clickedStock))
                            {
                                g.호가종목.Remove(g.clickedStock);
                            }
                            else
                            {
                                g.호가종목.Add(g.clickedStock);
                                if (g.관심종목.Contains(g.clickedStock)) g.관심종목.Remove(g.clickedStock);
                            }
                        }
                        action = " peB";
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
                            if (g.관심종목.Contains(g.clickedStock)) g.관심종목.Remove(g.clickedStock);
                            else g.관심종목.Add(g.clickedStock);
                        }
                    }
                    action = " peB";
                    break;

                case "l7":
                    wk.call_네이버(g.clickedStock, "main");
                    break;

                case "l8":
                    stockData.수급과장배수 *= 0.66;
                    action = "B  B";
                    break;

                case "l9":
                    if (g.test)
                    {
                        g.Npts[1]--;
                        if (g.Npts[1] < 2)
                        {
                            g.Npts[0] = 0;
                            g.Npts[1] = 2;
                        }
                        action = " peB";
                    }
                    break;

                case "r1":
                    string t = "http://google.com/search?q=" + g.clickedStock + " 뉴스 주식&tbs=qdr:" + g.PeoridNews;
                    Process.Start(t);
                    break;

                case "r2":
                    wk.call_네이버(g.clickedStock, "fchart");
                    break;

                case "r3":
                    string query = g.clickedStock + " 기업정보";
                    string encodedQuery = Uri.EscapeDataString(query);
                    t = "http://google.com/search?q=" + encodedQuery;
                    Process.Start("chrome.exe", t);
                    break;

                case "r4":
                    if (g.test)
                    {
                        g.Npts[1]++;
                        if (g.Npts[1] > g.MAX_ROW)
                        {
                            g.Npts[0] = 0;
                            g.Npts[1] = 2;
                        }
                        action = " peB";
                    }
                    break;

                case "r5":
                    break;

                case "r6":
                    if (g.q == "h&s")
                    {
                        g.Gid = g.saved_Gid;
                        g.Npts[0] = g.test ? g.SavedNpts[0] : 0;
                        g.Npts[1] = g.test ? g.SavedNpts[1] : g.MAX_ROW;

                        int month = g.date % 10000 / 100;
                        int day = g.date % 100;
                        string newValue = month + "/" + day;
                        if (g.제어.dtb.Rows[0][0].ToString() != newValue)
                        {
                            g.제어.dtb.Rows[0][0] = newValue;
                        }
                    }
                    else
                    {
                        g.q = "h&s";
                        g.saved_Gid = g.Gid;
                        g.moving_reference_date = g.date;
                        g.Npts[0] = g.test ? g.SavedNpts[0] : 0;
                        g.Npts[1] = g.test ? g.SavedNpts[1] : g.MAX_ROW;
                    }
                    break;

                case "r7":
                    sr.r3_display_lines(chart, g.clickedStock, row_id, col_id);
                    break;

                case "r8":
                    wk.call_네이버(g.clickedStock, "frgn");
                    break;

                case "r9":
                    if (g.clickedStock == "KODEX 레버리지" || g.clickedStock == "KODEX 코스닥150레버리지")
                    {
                        // Placeholder for future logic
                    }
                    else
                    {
                        foreach (var group in g.oGL_data)
                        {
                            if (group.stocks.Contains(g.clickedStock))
                            {
                                g.clickedTitle = group.title;
                                break;
                            }
                        }

                        var form = Application.OpenForms["Form_보조_차트"] as Form_보조_차트;
                        string key = g.v.SubKeyStr == "상관" || g.v.SubKeyStr == "절친" ? g.v.SubKeyStr : "상관";
                        mm.ManageChart2(key);
                    }
                    break;
            }

            if (action[0] != ' ')
            {
                if (action[0] == 'm' || action[0] == 'B')
                    mm.ClearChartAreaAndAnnotations(g.ChartManager.Chart1, g.clickedStock);
                if (action[0] == 's' || action[0] == 'B')
                    mm.ManageChart2();
            }

            if (action[1] != ' ') ps.post_test();
            if (action[2] != ' ') ev.eval_stock();
            if (action[3] != ' ')
            {
                if (action[3] == 'm' || action[3] == 'B') mm.ManageChart1();
                if (action[3] == 's' || action[3] == 'B') mm.ManageChart2();
            }
        }



    }
}
