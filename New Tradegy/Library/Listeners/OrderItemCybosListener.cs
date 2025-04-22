
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
    internal class OrderItemCybosListener
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

                lock (OrderItemTracker.orderLock)
                {
                    switch (sConFlag)
                    {
                        case "1": // 체결
                            OrderItemTracker.Update(nOrdKey, x =>
                            {
                                if (x.m_nAmt - nContAmt > 0)
                                {
                                    x.m_nAmt -= nContAmt;
                                    x.m_nModAmt = x.m_nAmt;
                                    x.m_nContAmt += nContAmt;
                                }
                                else
                                {
                                    OrderItemTracker.Remove(nOrdKey);
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
                            if (!OrderItemTracker.Exists(nOrdOrgKey))
                            {
                                if ((string)mapConclution["정정취소"] == "3")
                                    OrderItemTracker.Remove(nOrdKey);
                                break;
                            }

                            var original = OrderItemTracker.Get(nOrdOrgKey);
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
                                    OrderItemTracker.Add(updated);
                                }
                                else
                                {
                                    OrderItemTracker.Remove(nOrdOrgKey);

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
                                    OrderItemTracker.Add(updated);
                                }
                            }
                            else if ((string)mapConclution["정정취소"] == "3")
                            {
                                OrderItemTracker.Remove(nOrdOrgKey);
                            }
                            break;

                        case "3": // 거부
                            Utils.SoundUtils.Sound("Keys", "거부됨");
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
                            OrderItemTracker.Add(newItem);
                            break;
                    }
                }

                if (sConFlag == "1" || sConFlag == "2" || sConFlag == "4")
                {
                    DealManager.DealHold();
                    g.ChartGeneral.UpdateLayoutIfChanged();
                }

                g.매매.TradeRenderer?.Update(); // ? -> if g.매매.Renderer assigned
            }
            catch (Exception ex)
            {
                Utils.SoundUtils.Sound("Keys", "error");
                System.Diagnostics.Debug.WriteLine("CpConclusion_Received ERROR: " + ex.Message);
            }
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