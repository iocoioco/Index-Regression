
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Deals;
using New_Tradegy.Library.Listeners;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.PostProcessing;
using New_Tradegy.Library.Trackers.Charting;
using New_Tradegy.Library.UI.KeyBindings;
using Newtonsoft.Json.Linq;
using System.Linq;


namespace New_Tradegy.Library.UI.ChartClickHandlers
{
    internal class ChartClickHandler
    {
        private static CPUTILLib.CpCybos _cpcybos;



        private static int GetAskPriceFromGivenStock(string stockName)
        {
            var dgv = Utils.FormUtils.FindDataGridViewByName(Form1.Instance, stockName);

            var cellValue = dgv.Rows[4].Cells[1].Value?.ToString();

            if (int.TryParse(cellValue?.Replace(",", ""), out int price))
                return price;
            else
                return 1;
        }

        public static void HandleControlClick(Chart chart, string selection, int row, int col)
        {
            switch (selection)
            {
                case "l1": // Control click : buy at market price
                    if (!g.StockManager.HoldingList.Contains(g.clickedStock) && !g.StockManager.InterestedWithBidList.Contains(g.clickedStock))
                    {
                        g.StockManager.InterestedWithBidList.Add(g.clickedStock);
                    }
                    g.ChartMain.RefreshMainChart(); // index already has bookbid

                    int price = GetAskPriceFromGivenStock(g.clickedStock);
                    if (price < 0) return;

                    int quantity = g.일회거래액 * 10000 / price;
                    if (quantity == 0) quantity = 1;

                    Utils.SoundUtils.Sound_돈(g.일회거래액);

                    string info = g.clickedStock + " : " + price + " X " + quantity +
                                  " = " + (price * quantity / 10000) + "만원";

                    var stockData = g.StockRepository.TryGetDataOrNull(g.clickedStock);
                    if (stockData == null) return;

                    info += "\n" + Utils.StringUtils.r3_display_매수_매도(stockData);

                    if (DealManager.CheckPreviousLoss(g.clickedStock)) return;

                    var confirmationDialog = new BookBidGenerator();
                    bool isSell = false;
                    confirmationDialog.OpenOrUpdateConfirmationForm(isSell, g.clickedStock, quantity, price, 100, info);

                    DealManager.DealExec("매수", g.clickedStock, quantity, price, "03");
                    break;
            }
        }


        public static void HandleClick(Chart chart, string selection, int row_id, int col_id)
        {
            var data = g.StockRepository.TryGetDataOrNull(g.clickedStock);
            if (data == null) return;

            switch (selection)
            {
                case "l1": // Shrink Toggle
                    {
                        data.Misc.ShrinkDraw = !data.Misc.ShrinkDraw;
                        ActionCode.New(true, post: false, eval: false, draw: 'B').Run();
                    }
                    break;

                case "l2": // 수급과장배수, Leverage magnifier * 1.5
                    {
                        if (g.StockRepository.AllGeneralDatas.Any(x => x.Stock == data.Stock))
                        {
                            data.Misc.수급과장배수 *= 1.5;
                        }
                        else
                        {
                            if (g.StockManager.LeverageList[0] == data.Stock)
                            {
                                g.kodex_magnifier[0, 0] *= 1.5;
                            }
                            else
                                g.kodex_magnifier[1, 0] *= 1.5;
                        }
                        ActionCode.New(true, post: false, eval: false, draw: 'B').Run();
                    }
                    break;

                case "l3": // Stock Memo
                    {
                        string dirPath = @"C:\병신\data work\Stock Memo";
                        string filePath = Path.Combine(dirPath, g.clickedStock + ".txt");
                        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                        if (!File.Exists(filePath))
                            File.Create(filePath).Dispose();
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    break;

                case "l4": 
                    if (g.test)
                    {
                        ActionHandlers.TimeShortMoveKey?.Invoke();
                        //ActionCode.New(true, post: false, eval: false, draw: 'B').Run();
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
                        ActionCode.New(false, post: false, eval: false, draw: 'm').Run();
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
                            else
                                g.StockManager.InterestedOnlyList.Add(g.clickedStock);
                        }
                    }
                    ActionCode.New(false, false, eval: true, draw: 'm').Run();

                    break;

                case "l7":
                    wk.CallNaverChart(g.clickedStock, "main");
                    break;

                case "l8":
                    if (g.StockRepository.AllGeneralDatas.Any(x => x.Stock == data.Stock))
                    {
                        data.Misc.수급과장배수 *= 0.66;
                    }
                    else
                    {
                        if (g.StockManager.LeverageList[0] == data.Stock)
                        {
                            g.kodex_magnifier[0, 0] *= 0.66;
                        }
                        else
                            g.kodex_magnifier[1, 0] *= 0.66;
                    }

                    
                    ActionCode.New(true, false, eval: true, draw: 'B').Run();
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
                        ActionCode.New(false, false, eval: true, draw: 'B').Run();
                    }
                    break;

                case "r1":
                    string keyword = $"{g.clickedStock} 뉴스 주식";
                    string encodedQuery = Uri.EscapeDataString(keyword);
                    string url = $"https://www.google.com/search?q={encodedQuery}&tbs=qdr:{g.PeoridNews}";
                    Process.Start("chrome.exe", $"--new-tab {url}");
                    break;

                case "r2":
                    wk.CallNaverChart(g.clickedStock, "fchart");
                    break;

                case "r3":
                    string query = g.clickedStock + " 기업정보";
                    encodedQuery = Uri.EscapeDataString(query);
                    url = $"https://www.google.com/search?q={encodedQuery}";
                    Process.Start("chrome.exe", $"--new-tab {url}");
                    break;

                case "r4":
                    if (g.test)
                    {
                        g.Npts[1]++;
                        if (g.Npts[1] > g.TestMaximumRow)
                        {
                            g.Npts[0] = 0;
                            g.Npts[1] = 2;
                        }
                        ActionCode.New(true, false, eval: true, draw: 'B').Run();
                    }
                    break;

                case "r5":
                    break;

                case "r6":
                    // this is not implemented correctly, thus return for the time being
                    
                    if (g.q == "h&s")
                    {
                        g.Gid = g.saved_Gid;
                        g.Npts[0] = g.test ? g.SavedNpts[0] : 0;
                        g.Npts[1] = g.test ? g.SavedNpts[1] : g.RealMaximumRow;

                        int month = g.date % 10000 / 100;
                        int day = g.date % 100;
                        string newValue = month + "/" + day;


                        if (g.controlPane.GetCellValue(0, 0) != newValue)
                            g.controlPane.SetCellValue(0, 0, newValue);


                    }
                    else
                    {
                        g.q = "h&s";
                        g.saved_Gid = g.Gid;
                        g.moving_reference_date = g.date;
                        g.Npts[0] = g.test ? g.SavedNpts[0] : 0;
                        g.Npts[1] = g.test ? g.SavedNpts[1] : g.RealMaximumRow;
                    }
                    break;

                case "r7":
                    Utils.StringUtils.r3_display_lines(chart, g.clickedStock, row_id, col_id);
                    break;

                case "r8":
                    wk.CallNaverChart(g.clickedStock, "frgn");
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
                        g.v.SubChartDisplayMode = "상관";
                        ActionCode.New(false, false, eval: true, draw: 's').Run();
                    }
                    break;
            }
        }
    }
}
