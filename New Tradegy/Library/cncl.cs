
using CPUTILLib;
using DSCBO1Lib;
using NLog.Layouts;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static New_Tradegy.Library.g.stock_data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using New_Tradegy;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
namespace New_Tradegy.Library
{
    internal class cn
    {
        private static DSCBO1Lib.CpConclusion _CpConclusion;
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        // 주문 항목 정보 

        private static CPTRADELib.CpTd0311 _cptd0311; //주문(현금 주문) 데이터를 요청

        public static void Init_CpConclusion()
        {


            _CpConclusion = new DSCBO1Lib.CpConclusion();
            _CpConclusion.Received +=
                new DSCBO1Lib._IDibEvents_ReceivedEventHandler(CpConclusion_Received);

            _CpConclusion.Subscribe();

            //m_mapOrder = new Hashtable(); // 초기
        }

        private static void CpConclusion_Received()
        {
            // 거래명칭  = _CpConclusion.GetHeaderValue(0);    
            //  계좌명 = _CpConclusion.GetHeaderValue(1);    
            // 종목명 = _CpConclusion.GetHeaderValue(2);    
            // 계좌번호  = _CpConclusion.GetHeaderValue(7);    
            // "상품관리 구분 코드: " & tobj.GetHeaderValue(8))
            // "종목 코드: " & tobj.GetHeaderValue(9))
            // "매매구분 코드: " & tobj.GetHeaderValue(12))
            // "체결구분 코드: " & tobj.GetHeaderValue(14))
            // "신용대출 구분 코드: " & tobj.GetHeaderValue(15))
            // "정정취소 구분 코드: " & tobj.GetHeaderValue(16))

            Hashtable mapConclution = new Hashtable();
            int nContAmt = _CpConclusion.GetHeaderValue(3);        // 주문, 체결 수량
            int nPrice = _CpConclusion.GetHeaderValue(4);  // 가격
            int nOrdKey = _CpConclusion.GetHeaderValue(5);  //주문번호
            int nOrdOrgKey = _CpConclusion.GetHeaderValue(6);  //원주문번호
            string sConFlag = _CpConclusion.GetHeaderValue(14);  //체결 플래그 1 체결 2 확인...

            // 기타 값들은 맵에 저장..
            mapConclution["종목코드"] = _CpConclusion.GetHeaderValue(9);  //종목코드
            string stock = _cpstockcode.CodeToName(mapConclution["종목코드"].ToString());
            mapConclution["매수매도"] = _CpConclusion.GetHeaderValue(12);  //매수/매도 구분 1 매도 2매수
            mapConclution["정정취소"] = _CpConclusion.GetHeaderValue(16);  //정정/취소 구분코드 (1 정상, 2 정정 3 취소)
            mapConclution["주문호가구분"] = _CpConclusion.GetHeaderValue(18);  //주문호가구분코드
            mapConclution["주문조건구분"] = _CpConclusion.GetHeaderValue(19);  //주문조건구분코드
            mapConclution["장부가"] = _CpConclusion.GetHeaderValue(21);  //장부가
            mapConclution["매도가능"] = _CpConclusion.GetHeaderValue(22);  //매도가능수량
            mapConclution["체결기준잔고"] = _CpConclusion.GetHeaderValue(23);  //체결기준잔고수량

            // 현재 종목만 처리 
            //if (m_sCode != (string)mapConclution["종목코드"])
            //    return;

            //var data = new OrderItem();
            lock (OrderTracker.orderLock) // 🔒 wrap the whole switch for safety
            {
                OrderItem data = null;
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

                            // ✅ Log the contract (partial or full)
                            TradeLogger.LogTrade(new OrderItem
                            {
                                stock = x.stock,
                                buyorSell = x.buyorSell,
                                m_nPrice = x.m_nPrice,
                                m_nContAmt = nContAmt
                            });
                        });
                        break;


                    case "2": // 확인 - 정정 또는 취소 확인
                        {
                            // IOC/FOK의 경우, 원주문이 없어도 취소확인만 처리
                            if (!OrderTracker.Exists(nOrdOrgKey))
                            {
                                if ((string)mapConclution["정정취소"] == "3") // 취소
                                    OrderTracker.Remove(nOrdKey);
                                break;
                            }

                            data = OrderTracker.Get(nOrdOrgKey); // 원주문 가져오기

                            if ((string)mapConclution["정정취소"] == "2") // 정정확인
                            {
                                if (data.m_nAmt - nContAmt > 0) // 일부 정정
                                {
                                    // 기존 원주문 수량 조절
                                    data.m_nAmt -= nContAmt;
                                    data.m_nModAmt = data.m_nAmt;

                                    // 정정된 신규 주문 추가
                                    var item1 = new OrderItem
                                    {
                                        m_ordKey = nOrdKey,
                                        m_ordOrgKey = nOrdOrgKey,
                                        m_sCode = (string)mapConclution["종목코드"],
                                        m_nAmt = nContAmt,
                                        m_nPrice = nPrice,
                                        m_nContAmt = 0,
                                        m_nModAmt = nContAmt,
                                        buyorSell = data.buyorSell,
                                        m_sHogaFlag = (string)mapConclution["주문호가구분"]
                                    };
                                    OrderTracker.Add(item1);
                                }
                                else // 전체 정정
                                {
                                    OrderTracker.Remove(nOrdOrgKey);

                                    var item1 = new OrderItem
                                    {
                                        m_ordKey = nOrdKey,
                                        m_ordOrgKey = nOrdOrgKey,
                                        m_sCode = (string)mapConclution["종목코드"],
                                        m_nAmt = nContAmt,
                                        m_nPrice = nPrice,
                                        m_nContAmt = 0,
                                        m_nModAmt = nContAmt,
                                        buyorSell = data.buyorSell,
                                        m_sHogaFlag = (string)mapConclution["주문호가구분"]
                                    };
                                    OrderTracker.Add(item1);
                                }
                            }
                            else if ((string)mapConclution["정정취소"] == "3") // 취소확인
                            {
                                OrderTracker.Remove(nOrdOrgKey);
                            }

                            break;
                        }


                    case "3": // 거부
                        break;

                    case "4": // 접수
                        {
                            // 신규 매수/매도 주문만 처리
                            if ((string)mapConclution["정정취소"] != "1")
                                break;

                            var item = new OrderItem
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

                            OrderTracker.Add(item);
                            break;
                        }
                }
            }
            if (sConFlag == "1" || sConFlag == "2" || sConFlag == "4")
                DealManager.deal_hold();

            dgv2_update(); // CpConclusion_Received()
        }

        // called by marketeye_received(), CpConclusion_Received(), deal_hold_order(),
        // stockjpbids_Received()
        public static void dgv2_update()
        {
            lock (OrderTracker.orderLock)
            {
                // Suspend DataGridView updates to avoid flickering
                g.매매.dgv.SuspendLayout();


                int rowCount = 0;

                // 매수 매도 진행 중 row
                if (OrderTracker.OrderMap != null) // ADDED
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

                // empty line inbetween dealing and holding stocks
                g.매매.dtb.Rows[rowCount][0] = " ";
                g.매매.dtb.Rows[rowCount][1] = " ";
                g.매매.dtb.Rows[rowCount][2] = " ";
                g.매매.dtb.Rows[rowCount][3] = " ";
                rowCount++;

                // 보유종목 row
                int 보유종목_순서번호 = 0;
                foreach (var stock in g.보유종목.ToList())
                {
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                        return;
                    g.stock_data o = g.ogl_data[index];

                    if (o.매수1호가 > 0)
                        o.수익률 = (double)(o.매수1호가 - o.장부가) / o.매수1호가 * 100;

                    g.매매.dtb.Rows[rowCount][0] = o.stock;
                    g.매매.dtb.Rows[rowCount][1] = Math.Round((o.매수1호가 / 10000.0), 4);
                    if (o.최우선매도호가잔량 >= 0)
                        g.매매.dtb.Rows[rowCount][2] = Math.Round(((double)o.최우선매수호가잔량 / o.최우선매도호가잔량), 2);

                    g.매매.dtb.Rows[rowCount][3] = o.보유량.ToString() + "/" + Math.Round(o.수익률, 2);

                    // 상위 2개 종목 + g.v.보유종목점검최소액 > 9만원 (현재)
                    //if (o.보유량 * o.매수1호가 / 10000.0 > g.v.보유종목점검최소액 && 보유종목_순서번호 < 2)
                    //{
                    dgv2_update_보유종목updownsoundandcolor(o, 보유종목_순서번호, rowCount);
                    //}


                    rowCount++;
                    보유종목_순서번호++;

                    if (rowCount == 10)
                        break;
                }

                // empty row below holding stocks
                for (int i = rowCount; i < g.매매.dtb.Rows.Count; i++)
                {
                    g.매매.dtb.Rows[i][0] = " ";
                    g.매매.dtb.Rows[i][1] = " ";
                    g.매매.dtb.Rows[i][2] = " ";
                    g.매매.dtb.Rows[i][3] = " ";
                }

                // Resume DataGridView updates to render all changes at once
                g.매매.dgv.ResumeLayout();

                foreach (var stock in g.보유종목) // if g.보유종목 change, no further action and return
                {
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                        return;
                    g.stock_data o = g.ogl_data[index];
                    //if (op.dgv2_update_보유비매(o)) // false returning, the function does nothing
                    //    return;
                }
            }
        }

        private static void dgv2_update_보유종목updownsoundandcolor(g.stock_data o, int 보유종목_순서번호, int rowCount)
        {
            string sound = "";
            switch (보유종목_순서번호)
            {
                case 0:
                    sound = "one";
                    break;
                case 1:
                    sound = "two";
                    break;
                case 2:
                    sound = "three";
                    break;
            }

            if (o.보유량 * o.현재가 > 500000) // 50만원 이상일 경우 up and down 소리 발생
            {
                string posfix_up_down;

                if (o.전수익률 == o.수익률)
                    posfix_up_down = "";
                else if (o.전수익률 < o.수익률)
                    posfix_up_down = " up";
                else
                    posfix_up_down = " down";

                sound += posfix_up_down;
                mc.Sound("가", sound);
            }


            // Color
            int red = 255; // if decrease, getting dark cyan
            int green = 255; // if decrease, getting dark red
            int blue = 255;
            if (o.수익률 > 0) // 수익
            {
                red = (int)(255.0 - 255.0 * o.수익률 / 10.0); // 수익 증가하면 getting dark cyan
                if (red < 0)
                    red = 0;
            }

            if (o.수익률 < 0) // 손실 증가하면 getting dark red 
            {
                green = (int)(255.0 + 255.0 * o.수익률 / 10.0);
                if (green < 0)
                    green = 0;
            }
            g.매매.dgv.Rows[rowCount].DefaultCellStyle.BackColor = Color.FromArgb(red, green, blue);

            o.전수익률 = o.수익률;
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