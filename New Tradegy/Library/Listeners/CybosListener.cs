
using CPUTILLib;
using DSCBO1Lib;
using NLog.Layouts;
using System;
using System.Collections;

using System.Drawing;
using System.Linq;


using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Core;
namespace New_Tradegy.Library.Listeners
{
    internal class CybosListener
    {
        private static DSCBO1Lib.CpConclusion _CpConclusion;
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        // 주문 항목 정보 

        private static CPTRADELib.CpTd0311 _cptd0311; //주문(현금 주문) 데이터를 요청

        private static bool _isSubscribed = false;

        public static void Init_CpConclusion()
        {
            if (_isSubscribed) return;

            _CpConclusion = new DSCBO1Lib.CpConclusion();
            _CpConclusion.Received += new DSCBO1Lib._IDibEvents_ReceivedEventHandler(CpConclusion_Received);
            _CpConclusion.Subscribe();

            _isSubscribed = true;
        }

        private static void CpConclusion_Received()
        {
            try
            {
                Hashtable mapConclution = new Hashtable();
                int nContAmt = _CpConclusion.GetHeaderValue(3);
                int nPrice = _CpConclusion.GetHeaderValue(4);
                int nOrdKey = _CpConclusion.GetHeaderValue(5);
                int nOrdOrgKey = _CpConclusion.GetHeaderValue(6);
                string sConFlag = _CpConclusion.GetHeaderValue(14);

                mapConclution["종목코드"] = _CpConclusion.GetHeaderValue(9);
                string stock = _cpstockcode.CodeToName(mapConclution["종목코드"].ToString());
                mapConclution["매수매도"] = _CpConclusion.GetHeaderValue(12);
                mapConclution["정정취소"] = _CpConclusion.GetHeaderValue(16);
                mapConclution["주문호가구분"] = _CpConclusion.GetHeaderValue(18);
                mapConclution["주문조건구분"] = _CpConclusion.GetHeaderValue(19);
                mapConclution["장부가"] = _CpConclusion.GetHeaderValue(21);
                mapConclution["매도가능"] = _CpConclusion.GetHeaderValue(22);
                mapConclution["체결기준잔고"] = _CpConclusion.GetHeaderValue(23);

                lock (OrderTracker.orderLock)
                {
                    switch (sConFlag)
                    {
                        case "1": // 체결
                            OrderTracker.Update(nOrdKey, x =>
                            {
                                if (x.m_nAmt - nContAmt > 0)
                                {
                                    x.m_nAmt -= nContAmt;
                                    x.m_nModAmt = x.m_nAmt;
                                    x.m_nContAmt += nContAmt;
                                }
                                else
                                {
                                    OrderTracker.Remove(nOrdKey);
                                }

                                TradeLogger.LogTrade(new OrderItem
                                {
                                    stock = x.stock,
                                    buyorSell = x.buyorSell,
                                    m_nPrice = x.m_nPrice,
                                    m_nContAmt = nContAmt
                                });
                            });
                            break;

                        case "2": // 확인
                            if (!OrderTracker.Exists(nOrdOrgKey))
                            {
                                if ((string)mapConclution["정정취소"] == "3")
                                    OrderTracker.Remove(nOrdKey);
                                break;
                            }

                            var original = OrderTracker.Get(nOrdOrgKey);
                            if (original == null) break;

                            if ((string)mapConclution["정정취소"] == "2")
                            {
                                if (original.m_nAmt - nContAmt > 0)
                                {
                                    original.m_nAmt -= nContAmt;
                                    original.m_nModAmt = original.m_nAmt;

                                    var updated = new OrderItem
                                    {
                                        m_ordKey = nOrdKey,
                                        m_ordOrgKey = nOrdOrgKey,
                                        m_sCode = (string)mapConclution["종목코드"],
                                        m_nAmt = nContAmt,
                                        m_nPrice = nPrice,
                                        m_nContAmt = 0,
                                        m_nModAmt = nContAmt,
                                        buyorSell = original.buyorSell,
                                        m_sHogaFlag = (string)mapConclution["주문호가구분"]
                                    };
                                    OrderTracker.Add(updated);
                                }
                                else
                                {
                                    OrderTracker.Remove(nOrdOrgKey);

                                    var updated = new OrderItem
                                    {
                                        m_ordKey = nOrdKey,
                                        m_ordOrgKey = nOrdOrgKey,
                                        m_sCode = (string)mapConclution["종목코드"],
                                        m_nAmt = nContAmt,
                                        m_nPrice = nPrice,
                                        m_nContAmt = 0,
                                        m_nModAmt = nContAmt,
                                        buyorSell = original.buyorSell,
                                        m_sHogaFlag = (string)mapConclution["주문호가구분"]
                                    };
                                    OrderTracker.Add(updated);
                                }
                            }
                            else if ((string)mapConclution["정정취소"] == "3")
                            {
                                OrderTracker.Remove(nOrdOrgKey);
                            }
                            break;

                        case "3": // 거부
                            mc.Sound("Keys", "거부됨");
                            break;

                        case "4": // 접수
                            if ((string)mapConclution["정정취소"] != "1") break;

                            var newItem = new OrderItem
                            {
                                stock = stock,
                                m_ordKey = nOrdKey,
                                m_ordOrgKey = nOrdOrgKey,
                                m_sCode = (string)mapConclution["종목코드"],
                                m_nAmt = nContAmt,
                                m_nPrice = nPrice,
                                m_nContAmt = 0,
                                m_nModAmt = nContAmt,
                                buyorSell = (string)mapConclution["매수매도"] == "1" ? "매도" : "매수",
                                m_sHogaFlag = (string)mapConclution["주문호가구분"]
                            };
                            OrderTracker.Add(newItem);
                            break;
                    }
                }

                if (sConFlag == "1" || sConFlag == "2" || sConFlag == "4")
                {
                    DealManager.deal_hold();
                    g.Chart1Manager.UpdateLayoutIfChanged();
                }

                UpdateDgv2();
            }
            catch (Exception ex)
            {
                mc.Sound("Keys", "error");
                System.Diagnostics.Debug.WriteLine("CpConclusion_Received ERROR: " + ex.Message);
            }
        }

        public static void UpdateDgv2()
        {
            lock (OrderTracker.orderLock)
            {
                g.매매.dgv.SuspendLayout();

                int rowCount = 0;

                if (OrderTracker.OrderMap != null)
                {
                    foreach (var kvp in OrderTracker.OrderMap)
                    {
                        var data = kvp.Value;

                        g.매매.dtb.Rows[rowCount][0] = data.stock;
                        g.매매.dtb.Rows[rowCount][1] = data.buyorSell;
                        g.매매.dtb.Rows[rowCount][2] = data.m_nPrice;
                        g.매매.dtb.Rows[rowCount][3] = data.m_nContAmt + "/" + data.m_nAmt;
                        rowCount++;
                    }
                }

                g.매매.dtb.Rows[rowCount][0] = " ";
                g.매매.dtb.Rows[rowCount][1] = " ";
                g.매매.dtb.Rows[rowCount][2] = " ";
                g.매매.dtb.Rows[rowCount][3] = " ";
                rowCount++;

                int 보유종목_순서번호 = 0;
                foreach (var stock in g.StockManager.HoldingList.ToList())
                {
                    var data = StockRepository.Instance.GetOrThrow(stock);
                    if (data == null)
                        return;

                    if (data.Api.매수1호가 > 0)
                        data.Deal.수익률 = (double)(data.Api.매수1호가 - data.Deal.장부가) / data.Api.매수1호가 * 100;

                    g.매매.dtb.Rows[rowCount][0] = data.Stock;
                    g.매매.dtb.Rows[rowCount][1] = Math.Round((data.Api.매수1호가 / 10000.0), 4);
                    if (data.Api.최우선매도호가잔량 >= 0)
                        g.매매.dtb.Rows[rowCount][2] = Math.Round(((double)data.Api.최우선매수호가잔량 / data.Api.최우선매도호가잔량), 2);

                    g.매매.dtb.Rows[rowCount][3] = data.Deal.보유량.ToString() + "/" + Math.Round(data.Deal.수익률, 2);

                    Dgv2_Update_보유종목UpDownSoundAndColor(data, 보유종목_순서번호, rowCount);

                    rowCount++;
                    보유종목_순서번호++;

                    if (rowCount == 10)
                        break;
                }

                for (int i = rowCount; i < g.매매.dtb.Rows.Count; i++)
                {
                    g.매매.dtb.Rows[i][0] = " ";
                    g.매매.dtb.Rows[i][1] = " ";
                    g.매매.dtb.Rows[i][2] = " ";
                    g.매매.dtb.Rows[i][3] = " ";
                }

                g.매매.dgv.ResumeLayout();

                foreach (var stock in g.StockManager.HoldingList)
                {
                    var data = StockRepository.Instance.GetOrThrow(stock);
                    if (data == null)
                        return;

                    // if (op.dgv2_update_보유비매(data)) return;
                }
            }
        }

        private static void Dgv2_Update_보유종목UpDownSoundAndColor(StockData o, int 보유종목_순서번호, int rowCount)
        {
            string sound = "";
            if (보유종목_순서번호 == 0)
                sound = "one";
            else if (보유종목_순서번호 == 1)
                sound = "two";
            else if (보유종목_순서번호 == 2)
                sound = "three";

            if (o.Deal.보유량 * o.Api.현재가 > 500000)
            {
                string postfix = "";
                if (o.Deal.전수익률 != o.Deal.수익률)
                    postfix = o.Deal.전수익률 < o.Deal.수익률 ? " up" : " down";

                mc.Sound("가", sound + postfix);
            }

            int red = 255, green = 255, blue = 255;
            if (o.Deal.수익률 > 0)
            {
                red = (int)(255.0 - 255.0 * o.Deal.수익률 / 10.0);
                if (red < 0) red = 0;
            }
            else if (o.Deal.수익률 < 0)
            {
                green = (int)(255.0 + 255.0 * o.Deal.수익률 / 10.0);
                if (green < 0) green = 0;
            }

            g.매매.dgv.Rows[rowCount].DefaultCellStyle.BackColor = Color.FromArgb(red, green, blue);
            o.Deal.전수익률 = o.Deal.수익률;
        }
    }
}


//public partial class Form1 : Form
//{
//    public static Form1 Instance;

//    public Form1()
//    {
//        InitializeComponent();
//        Instance = this; // Save reference for global access
//    }

//    public void UpdateGridFromOutside(OrderItem item)
//    {
//        if (this.InvokeRequired)
//        {
//            this.Invoke(new Action(() => UpdateGridFromOutside(item)));
//            return;
//        }

//        // Example: find the row by code or add new one
//        bool found = false;
//        foreach (DataGridViewRow row in dataGridView1.Rows)
//        {
//            if (row.Cells[0].Value?.ToString() == item.m_sCode)
//            {
//                row.Cells[1].Value = item.HoldingQty;
//                row.Cells[2].Value = item.BuyPrice;
//                row.Cells[3].Value = item.CurrentPrice;
//                row.Cells[4].Value = item.Profit;
//                row.Cells[5].Value = $"{item.ProfitRate:F2}%";
//                 Apply color
//                 row.DefaultCellStyle.BackColor = item.RowColor;

//                found = true;
//                break;
//            }
//        }

//        if (!found)
//        {
//            dataGridView1.Rows.Add(item.m_sCode, item.HoldingQty, item.BuyPrice,
//                                   item.CurrentPrice, item.Profit, $"{item.ProfitRate:F2}%");
//        }
//    }
//}


// calling from outside
//if (Form1.Instance != null)
//{
//    Form1.Instance.UpdateGridFromOutside(orderItem);
//}


// Bonus: If you want to send a specific cell value
//public void UpdateSpecificCell(string code, int columnIndex, object value)
//{
//    if (this.InvokeRequired)
//    {
//        this.Invoke(new Action(() => UpdateSpecificCell(code, columnIndex, value)));
//        return;
//    }

//    foreach (DataGridViewRow row in dataGridView1.Rows)
//    {
//        if (row.Cells[0].Value?.ToString() == code)
//        {
//            row.Cells[columnIndex].Value = value;
//            break;
//        }
//    }
//}

//Then call from outside:

//csharp
//Copy
//Edit
//Form1.Instance.UpdateSpecificCell("KODEX", 3, 16000); // update CurrentPrice


//public class OrderItem
//{
//    public string stock;
//    public string m_sCode;
//    public string buyorSell;
//    public int m_ordKey;
//    public int HoldingQty;
//    public int BuyPrice;
//    public int CurrentPrice;

//    public Color RowColor { get; set; } = Color.White; // Default white

//    public int Profit => (CurrentPrice * HoldingQty) - (BuyPrice * HoldingQty);
//    public double ProfitRate => BuyPrice > 0 ? (double)Profit / (BuyPrice * HoldingQty) * 100.0 : 0;
//}