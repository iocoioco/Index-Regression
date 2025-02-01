
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

            g.m_mapOrder = new Hashtable(); // 초기
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

            var data = new g.OrderItem();
            //OrderItem data = null;
            switch (sConFlag)
            {
                case "1": // 체결

                    if (g.m_mapOrder.ContainsKey(nOrdKey) == false) // 주문번호로 체결요청이 없었다 그러므로 return
                        return;

                    data = (g.OrderItem)g.m_mapOrder[nOrdKey];

                    // 일부 체결 - 원주문에서 체결 수량만큼 뺀다.
                    if (data.m_nAmt - nContAmt > 0)
                    {
                        data.m_nAmt -= nContAmt; // 미체결수량은 기존 미체결수량에서 지금 체결된 수량 빼기
                        data.m_nModAmt = data.m_nAmt; // 정정취소 가능수량은 결과적으로 남은 미체결수량
                        data.m_nContAmt += nContAmt; // 체결수량(결과적으로 남은)
                    }
                    else
                    {
                        g.m_mapOrder.Remove(nOrdKey); // 미체결 잔량이 없어졌으므로(전부 체결) - 원주문 제거 
                    }
                    break;

                case "2": // 확인 - 정정 또는 취소 확인
                          // 원주문이 있는 지 체크 한다. (무조건 원주문이 있어야 하지만, ioc, fok 는 예외)
                    if (g.m_mapOrder.ContainsKey(nOrdOrgKey) == false)
                    {
                        // IOC/FOK 의 경우 취소 주문을 낸적이 없어도 자동으로 취소 확인이 들어 온다.
                        if ((string)mapConclution["정정취소"] == "3") // 취소
                            g.m_mapOrder.Remove(nOrdKey);
                        return;
                    }

                    // 원주문을 찾아 처리 
                    data = (g.OrderItem)g.m_mapOrder[nOrdOrgKey];

                    // 정정확인
                    if ((string)mapConclution["정정취소"] == "2") // 정정
                    {
                        // 일부 정정 : 기존 주문은 수량 조절,  새로운 미체결은 추가 
                        #region
                        //            public class OrderItem
                        //{
                        //    public string stock; // 종목
                        //    public string m_sCode; // 코드
                        //    public string buyorSell; // 매수, 매도, 보유

                        //    public int m_ordKey;
                        //    public int m_ordOrgKey;

                        //    // public string m_sText;
                        //    public int m_nAmt;
                        //    public int m_nContAmt;
                        //    public int m_nPrice;
                        //    public string m_sCredit;
                        //    public int m_nModAmt;

                        //    public string m_sHogaFlag;
                        //}
                        //public static Hashtable m_mapOrder;
                        #endregion
                        if (data.m_nAmt - nContAmt > 0)
                        {
                            // 기존 미체결 수량 조정
                            data.m_nAmt -= nContAmt; // 주문수량 정정 -> 수정
                            data.m_nModAmt = data.m_nAmt; // 정정취소 가능수량

                            // 새로운 정정 가격으로 추가 
                            g.OrderItem item1 = new g.OrderItem();

                            item1.m_ordKey = (int)nOrdKey; // 주문번호
                            item1.m_ordOrgKey = (int)nOrdOrgKey; // 원주문번호
                            item1.m_sCode = (string)mapConclution["종목코드"];
                            //item.m_sText = objRq.GetDataValue(5, i);        // 주문 내용

                            item1.m_nAmt = nContAmt; // 주문수량
                            item1.m_nPrice = (int)nPrice; // 주문단가
                            item1.m_nContAmt = 0; //# 체결수량
                            item1.m_nModAmt = nContAmt;  // 정정취소 가능수량
                            item1.buyorSell = data.buyorSell; // 매수/매도
                            item1.m_sHogaFlag = (string)mapConclution["주문호가구분"]; // 주문호가구분코드내용
                            g.m_mapOrder[item1.m_ordKey] = item1;

                        }
                        // 전체 정정 - 기존 주문은 제거하고, 새로운 정정 가격으로 추가 
                        else
                        {
                            g.m_mapOrder.Remove(nOrdOrgKey);

                            // 새로운 정정 가격으로 추가 
                            g.OrderItem item1 = new g.OrderItem();
                            item1.m_ordKey = (int)nOrdKey;
                            item1.m_ordOrgKey = (int)nOrdOrgKey;
                            item1.m_sCode = (string)mapConclution["종목코드"];
                            //item.m_sText = objRq.GetDataValue(5, i);        // 주문 내용

                            item1.m_nAmt = nContAmt; // 주문수량
                            item1.m_nPrice = (int)nPrice; // 주문단가
                            item1.m_nContAmt = 0; //# 체결수량
                            item1.m_nModAmt = nContAmt;  // 정정취소 가능수량
                            item1.buyorSell = data.buyorSell;
                            item1.m_sHogaFlag = (string)mapConclution["주문호가구분"]; // 주문호가구분코드내용
                            g.m_mapOrder[item1.m_ordKey] = item1;

                        }
                    }
                    // 취소확인 - 해당 원주문을 제거 
                    else if ((string)mapConclution["정정취소"] == "3")
                    {
                        if (g.m_mapOrder.ContainsKey(nOrdOrgKey) == true)
                        {
                            g.m_mapOrder.Remove(nOrdOrgKey);
                        }
                    }
                    break;

                case "3": // 거부
                    break;

                case "4": // 접수
                          // 신규 매수/매도 주문만 처리 
                    if ((string)mapConclution["정정취소"] != "1")
                        break;

                    // 신규 접수된 미체결 정보를 추가한다.
                    // g.Orderitem Class 내용 inside region
                    #region
                    //            public class OrderItem
                    //{
                    //    public string stock; // 종목
                    //    public string m_sCode; // 코드
                    //    public string buyorSell; // 매수, 매도, 보유

                    //    public int m_ordKey;
                    //    public int m_ordOrgKey;

                    //    // public string m_sText;
                    //    public int m_nAmt;
                    //    public int m_nContAmt;
                    //    public int m_nPrice;
                    //    public string m_sCredit;
                    //    public int m_nModAmt;

                    //    public string m_sHogaFlag;
                    //}
                    //public static Hashtable m_mapOrder;
                    #endregion 
                    g.OrderItem item = new g.OrderItem(); // Class Initialization
                    item.stock = stock;
                    item.m_ordKey = (int)nOrdKey;
                    item.m_ordOrgKey = (int)nOrdOrgKey;
                    item.m_sCode = (string)mapConclution["종목코드"];
                    //item.m_sText = objRq.GetDataValue(5, i);        // 주문 내용

                    item.m_nAmt = (int)nContAmt; // 주문수량
                    item.m_nPrice = (int)nPrice; // 주문단가
                    item.m_nContAmt = 0; //# 체결수량
                    item.m_nModAmt = item.m_nAmt;  // 접수시 주문수량 = 정정취소 가능수량

                    if ((string)mapConclution["매수매도"] == "1")
                        item.buyorSell = "매도";
                    else if ((string)mapConclution["매수매도"] == "2")
                        item.buyorSell = "매수";

                    item.m_sHogaFlag = (string)mapConclution["주문호가구분"]; // 주문호가구분코드내용

                    g.m_mapOrder[item.m_ordKey] = item;
                    break;
            }
            if (sConFlag == "1" || sConFlag == "2" || sConFlag == "4")
                dl.deal_hold();

            dgv2_update(); // CpConclusion_Received()
        }

        // called by marketeye_received(), CpConclusion_Received(), deal_hold_order(),
        // stockjpbids_Received()
        public static void dgv2_update()
        {
            lock (g.lockObject)
            {
                // Suspend DataGridView updates to avoid flickering
                g.매매.dgv.SuspendLayout();


                int rowCount = 0;

                // 매수 매도 진행 중 row
                if (g.m_mapOrder != null) // ADDED
                {
                    foreach (DictionaryEntry de in g.m_mapOrder)
                    {
                        g.OrderItem data = de.Value as g.OrderItem;

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


        //public static void dgv2_update_old()
        //{
        //    lock (g.lockObject)
        //    {
        //        //g.매매.BeginLoadData();

        //        int rowCount = 0;

        //        // 매수 매도 진행 중 row
        //        if (g.m_mapOrder != null) // ADDED
        //        {
        //            foreach (DictionaryEntry de in g.m_mapOrder)
        //            {
        //                g.OrderItem data = de.Value as g.OrderItem;

        //                g.매매.dtb.Rows[rowCount][0] = data.stock;
        //                g.매매.dtb.Rows[rowCount][1] = data.buyorSell;
        //                g.매매.dtb.Rows[rowCount][2] = data.m_nPrice;
        //                g.매매.dtb.Rows[rowCount][3] = data.m_nContAmt + "/" + data.m_nAmt;
        //                rowCount++;
        //            }
        //        }

        //        // empty line inbetween dealing and holding stocks
        //        g.매매.dtb.Rows[rowCount][0] = " ";
        //        g.매매.dtb.Rows[rowCount][1] = " ";
        //        g.매매.dtb.Rows[rowCount][2] = " ";
        //        g.매매.dtb.Rows[rowCount][3] = " ";
        //        rowCount++;

        //        // 보유종목 row
        //        int 보유종목_순서번호 = 0;
        //        foreach (var stock in g.보유종목.ToList())
        //        {
        //            int index = wk.return_index_of_ogldata(stock);
        //            if (index < 0)
        //                return;
        //            g.stock_data o = g.ogl_data[index];

        //            if (o.매수1호가 > 0)
        //                o.수익률 = (double)(o.매수1호가 - o.장부가) / o.매수1호가 * 100;

        //            g.매매.dtb.Rows[rowCount][0] = o.stock;
        //            g.매매.dtb.Rows[rowCount][1] = Math.Round((o.매수1호가 / 10000.0), 4);
        //            if (o.최우선매도호가잔량 >= 0)
        //                g.매매.dtb.Rows[rowCount][2] = Math.Round(((double)o.최우선매수호가잔량 / o.최우선매도호가잔량), 2);

        //            g.매매.dtb.Rows[rowCount][3] = o.보유량.ToString() + "/" + Math.Round(o.수익률, 2);

        //            // 상위 2개 종목 + g.v.보유종목점검최소액 > 9만원 (현재)
        //            //if (o.보유량 * o.매수1호가 / 10000.0 > g.v.보유종목점검최소액 && 보유종목_순서번호 < 2)
        //            //{
        //            dgv2_update_보유종목updownsoundandcolor(o, 보유종목_순서번호, rowCount);
        //            //}


        //            rowCount++;
        //            보유종목_순서번호++;

        //            if (rowCount == 10)
        //                break;
        //        }

        //        // empty row below holding stocks
        //        for (int i = rowCount; i < g.매매.dtb.Rows.Count; i++)
        //        {
        //            g.매매.dtb.Rows[i][0] = " ";
        //            g.매매.dtb.Rows[i][1] = " ";
        //            g.매매.dtb.Rows[i][2] = " ";
        //            g.매매.dtb.Rows[i][3] = " ";
        //        }


        //        foreach (var stock in g.보유종목) // if g.보유종목 change, no further action and return
        //        {
        //            int index = wk.return_index_of_ogldata(stock);
        //            if (index < 0)
        //                return;
        //            g.stock_data o = g.ogl_data[index];
        //            //if (op.dgv2_update_보유비매(o)) // false returning, the function does nothing
        //            //    return;
        //        }
        //    }
        //}


        public static void dgv2_update_보유종목updownsoundandcolor(g.stock_data o, int 보유종목_순서번호, int rowCount)
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
        // Chat Gpt Suggestions
        //        1. Handling Edge Cases and Error Checks
        //You can add more error-handling and validation in certain parts of your code, especially when working with data that might be incomplete or when iterating through collections like g.m_mapOrder and g.보유종목.Here are some specific suggestions:

        //Null Check in CpConclusion_Received(): When you're working with _CpConclusion.GetHeaderValue(), ensure that none of the header values being fetched are null or invalid. For example:

        //csharp
        //Copy code
        //string stockCode = _CpConclusion.GetHeaderValue(9)?.ToString();
        //if (string.IsNullOrEmpty(stockCode)) 
        //{
        //    // Handle error or log issue
        //    return;
        //}
        //DataGridView Row Count: It looks like you're updating g.매매.dtb.Rows[rowCount] without checking if rowCount is within bounds of the DataTable. Ensure that you're not attempting to write data to a row index that doesn't exist.

        //csharp
        //Copy code
        //if (g.매매.dtb.Rows.Count > rowCount) 
        //{
        //    // Safe to update row
        //    g.매매.dtb.Rows[rowCount][0] = data.stock;
        //    g.매매.dtb.Rows[rowCount][1] = data.buyorSell;
        //    // other updates...
        //}
        //2. Improved Status Tracking in CpConclusion_Received()
        //Currently, when orders are processed(particularly in case "1" for conclusion and case "2" for cancellation), you modify or remove orders from the g.m_mapOrder hashtable.However, if you'd like to track the order status history (conclusion, cancellation, etc.), you can consider introducing a status field in your OrderItem class that marks orders as Cancelled, Concluded, Partially Filled, etc., rather than just removing them:

        //csharp
        //    Copy code
        //public class OrderItem
        //    {
        //        public string Status { get; set; } // E.g. "Concluded", "Cancelled", etc.
        //                                           // Other existing fields
        //    }
        //    Then, instead of removing the item from g.m_mapOrder, update its status:

        //csharp
        //Copy code
        //data.Status = "Cancelled"; // or "Concluded" for partial/full fills
        //    g.m_mapOrder[nOrdKey] = data;
        //This will help you maintain a full history in the DataGridView and control how you display canceled or concluded orders.

        //3. Performance Enhancements
        //When you call dgv2_update(), you're iterating through both the g.m_mapOrder and g.보유종목 collections. If these collections are large, it could potentially slow down the UI update. To optimize this:

        //Batch Update the DataGridView: Instead of updating each row one-by-one, consider updating the DataTable in bulk by preparing the data in memory and then binding it to the DataGridView in one step.This reduces the number of UI redraws.

        //csharp
        //    Copy code
        //    g.매매.BeginLoadData();
        //    // Prepare data
        //    g.매매.EndLoadData();
        //4. Concurrency Considerations
        //If g.m_mapOrder or g.보유종목 are being updated from multiple threads, be cautious about race conditions.You're already using lock (g.lockObject) in dgv2_update(), which is good. Just ensure that all modifications to shared resources like g.m_mapOrder are protected by this lock.

        //5. UI Feedback for Long Operations
        //If updating the DataGridView takes significant time, consider providing user feedback via a progress bar or status indicator.You could also offload the data fetching and updating to a background thread, using Task.Run or similar, and then invoke the UI update on the main thread:

        //csharp
        //    Copy code
        //Task.Run(() =>
        //{
        //        // Fetch data and update logic
        //    }).ContinueWith(t => 
        //{
        //        // Update UI on the main thread
        //        dgv2_update();
        //    }, TaskScheduler.FromCurrentSynchronizationContext());
        //6. Simplify Color Logic
        //You have logic for coloring DataGridView rows based on performance(o.수익률). You could simplify this by storing predefined colors for certain ranges (e.g., positive or negative returns) and applying them directly, which makes the code easier to maintain:

        //csharp
        //Copy code
        //Color profitColor = o.수익률 > 0 ? Color.Green : Color.Red;
        //    g.매매.dgv.Rows [rowCount].DefaultCellStyle.BackColor = profitColor;
        //7. Keep Orders Sorted by Timestamp or Key
        //If the orders in g.m_mapOrder need to be displayed in a specific order (e.g., by time of order or by key), you could maintain an ordered structure (like SortedList or SortedDictionary) or apply sorting before displaying them in dgv2_update().

        //By incorporating these improvements, you should see more robust handling of various edge cases, cleaner data management, and potentially better performance in the UI.
        //        // Orderitem Usage Example
        #region
        //OrderItem xyz = new OrderItem();
        //xyz.m_ordKey = 12345;
        //xyz.m_ordOrgKey = 1;
        //g.m_mapOrder[xyz.m_ordKey] = xyz;

        //xyz = new OrderItem();
        //xyz.m_ordKey = 12346;
        //xyz.m_ordOrgKey = 0;
        //g.m_mapOrder[xyz.m_ordKey] = xyz;

        //xyz = new OrderItem();
        //xyz.m_ordKey = 23456;
        //g.m_mapOrder[xyz.m_ordKey] = xyz;

        //List<int> keys = g.m_mapOrder.Keys.Cast<int>().ToList();
        //OrderItem abc = null;
        //if (keys.Count > 0)
        //    abc = (OrderItem)g.m_mapOrder[keys[0]];

        //OrderItem bcd = null;
        //if (keys.Count > 0)
        //    bcd = (OrderItem)g.m_mapOrder[keys[1]];
        #endregion
    }
}
