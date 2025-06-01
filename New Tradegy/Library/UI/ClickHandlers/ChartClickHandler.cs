
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Deals;
using New_Tradegy.Library.Listeners;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.UI.KeyBindings;


namespace New_Tradegy.Library.UI.ChartClickHandlers
{
    internal class ChartClickHandler
    {
        private static CPUTILLib.CpCybos _cpcybos;

        private static void RunAction(bool clear, bool post, bool eval, char draw)
        {
            ActionCode.New(clear: clear, post: post, eval: eval, draw: draw).Run();
        }

        private static int GetPriceFromGivenStock(string stockName)
        {
            var dgv = Utils.FormUtils.FindDataGridViewByName(Form1.Instance, stockName);

            var cellValue = dgv.Rows[5].Cells[1].Value?.ToString();

            if (int.TryParse(cellValue?.Replace(",", ""), out int price))
                return price;
            else
                return 1;
        }

        public static void HandleControlClick(Chart chart, string selection, int row, int col)
        {
            switch ()
            {
                case "l1": // buy at market price
                    if (!g.StockManager.HoldingList.Contains(g.clickedStock) && !g.StockManager.InterestedWithBidList.Contains(g.clickedStock))
                    {
                        g.StockManager.InterestedWithBidList.Add(g.clickedStock);
                    }
                    g.ChartGeneral1.UpdateLayoutIfChanged(); // index already has bookbid

                    int price = GetPriceFromGivenStock(g.clickedStock);
                    if (price < 0) return;

                    int quantity = g.일회거래액 * 10000 / price;
                    if (quantity == 0) quantity = 1;

                    Utils.SoundUtils.Sound_돈(g.일회거래액);

                    string info = g.clickedStock + " : " + price + " X " + quantity +
                                  " = " + (price * quantity / 10000) + "만원";

                    var stockData = g.StockRepository.TryGetStockOrNull(g.clickedStock);
                    if (stockData == null) return;

                    info += "\n" + Utils.StringUtils.r3_display_매수_매도(stockData);

                    if (DealManager.CheckPreviousLoss(g.clickedStock)) return;

                    var confirmationDialog = new BookBidGenerator();
                    bool isSell = false;
                    confirmationDialog.OpenOrUpdateConfirmationForm(isSell, g.clickedStock, quantity, price, 100, info);

                    DealManager.DealExec("매수", g.clickedStock, quantity, price, "03");
                    break;

                case "l2": // price magnifier * 1.333
                    int magnifierIdx = g.StockManager.IndexList.FindIndex(code => code == g.clickedStock);
                    if (magnifierIdx >= 0)
                    {
                        g.kodex_magnifier[magnifierIdx, 0] *= 1.333;
                        RunAction(true, false, true, 'B');
                       
                    }
                    break;

                case "l8": // price magnifier * 0.666
                    magnifierIdx = g.StockManager.IndexList.FindIndex(code => code == g.clickedStock);
                    if (magnifierIdx >= 0)
                    {
                        g.kodex_magnifier[magnifierIdx, 0] *= 0.666;
                        RunAction(true, false, true, 'B');
                    }
                    break;
            }
        }


        public static void HandleClick(Chart chart, string selection, int row_id, int col_id)
        {
            var stockData = g.StockRepository.TryGetStockOrNull(g.clickedStock);
            if (stockData == null) return;

            switch (selection)
            {
                case "l1":
                {
                    stockData.Misc.ShrinkDraw = !stockData.Misc.ShrinkDraw;
                        RunAction(true, false, true, 'B');

                        break;
                }

                case "l2":
                    {
                        stockData.Misc.수급과장배수 *= 1.5;
                        RunAction(true, false, true, 'B');
                    }
                    break;

                case "l3":
                    {
                        string dirPath = @"C:\병신\data\Stock Memo";
                        string filePath = Path.Combine(dirPath, g.clickedStock + ".txt");
                        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                        if (!File.Exists(filePath)) File.Create(filePath).Dispose();
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    break;

                case "l4":
                    if (g.test)
                    {
                        ActionHandlers.TimeShortMoveKey?.Invoke();
                        RunAction(true, false, true, 'B');

                    }
                    break;

                case "l5":
                    {
                        if (!g.StockManager.HoldingList.Contains(g.clickedStock))
                        {
                            if (g.StockManager.InterestedWithBidList.Contains(g.clickedStock))
                            {
                                g.StockManager.InterestedWithBidList.Remove(g.clickedStock);
                            }
                            else
                            {
                                g.StockManager.InterestedWithBidList.Add(g.clickedStock);
                                if (g.StockManager.InterestedOnlyList.Contains(g.clickedStock))
                                    g.StockManager.InterestedOnlyList.Remove(g.clickedStock);
                            }
                        }
                        RunAction(true, false, true, 'B');
                    }
                    break;

                case "l6":
                    if (!g.StockManager.HoldingList.Contains(g.clickedStock))
                    {
                        if (g.StockManager.InterestedWithBidList.Contains(g.clickedStock))
                        {
                            g.StockManager.InterestedWithBidList.Remove(g.clickedStock);
                            g.StockManager.InterestedOnlyList.Add(g.clickedStock);
                        }
                        else
                        {
                            if (g.StockManager.InterestedOnlyList.Contains(g.clickedStock))
                                g.StockManager.InterestedOnlyList.Remove(g.clickedStock);
                            else g.StockManager.InterestedOnlyList.Add(g.clickedStock);
                        }
                    }
                    RunAction(true, false, true, 'B');

                    break;

                case "l7":
                    wk.call_네이버(g.clickedStock, "main");
                    break;

                case "l8":
                    stockData.Misc.수급과장배수 *= 0.66;
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
                    Utils.StringUtils.r3_display_lines(chart, g.clickedStock, row_id, col_id);
                    break;

                case "r8":
                    wk.call_네이버(g.clickedStock, "frgn");
                    break;

                case "r9":
                    if (g.clickedStock == "KODEX 레버리지" || g.clickedStock == "KODEX 코스닥150레버리지")
                    {
                        return;
                    }
                    else
                    {
                        var group = g.GroupManager.FindGroupByStock(g.clickedStock);
                        if (group != null)
                        {
                            g.clickedTitle = group.Title;
                        }

                        //If g.v.SubChartDisplayMode is "상관" or "절친", then:
                        //Assign g.v.SubChartDisplayMode to key.
                        //❌ Otherwise:
                        //Assign "상관" to key(as a fallback default).
                        string key = (g.v.SubChartDisplayMode == "상관" || g.v.SubChartDisplayMode == "절친") ? g.v.SubChartDisplayMode : "상관";
                        mm.ManageChart2(key);
                    }
                    break;
            }
            // action[0] m main, s sub, B both Delete ?
            // action[1] p post_test
            // action[2] e evalstock()
            // action[3] m main, s sub, B both Redraw ?
            if (action[0] != ' ')
            {
                if (action[0] == 'm' || action[0] == 'B')
                    mm.ClearChartAreaAndAnnotations(g.ChartManager.Chart1, g.clickedStock);
                if (action[0] == 's' || action[0] == 'B')
                    mm.ManageChart2();
            }

            if (action[1] != ' ') PostProcessor.post_test();
            if (action[2] != ' ') RankLogic.EvalStock();
            if (action[3] != ' ')
            {
                if (action[3] == 'm' || action[3] == 'B') mm.ManageChart1();
                if (action[3] == 's' || action[3] == 'B') mm.ManageChart2();
            }
        }
    }
}
