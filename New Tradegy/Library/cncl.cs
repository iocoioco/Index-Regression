
using System;
using System.Collections;
using System.Drawing;
using System.Linq;
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


            // Orderitem Usage Example
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
                //g.매매.BeginLoadData();

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
                //g.매매.AcceptChanges();
                //g.매매.EndLoadData();

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
                ms.Sound("가", sound);
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

        // 일천, 오천, 일만, 십만, 오십만 이상 1/ / 1000 호가 단위, 코스닥은 오만원 이상 100원 단위
        //public static void dgv2_update_old()
        //{
        //    lock (g.lockObject)
        //    {
        //        int count = 0;


        //        foreach (DictionaryEntry de in g.m_mapOrder)
        //        {
        //            g.OrderItem data = de.Value as g.OrderItem;

        //            g.매매.dtb.Rows[count][0] = data.stock;
        //            g.매매.dtb.Rows[count][1] = data.buyorSell;
        //            g.매매.dtb.Rows[count][2] = data.m_nPrice;
        //            g.매매.dtb.Rows[count][3] = data.m_nAmt + "/" + data.m_nContAmt;
        //            count++;
        //        }

        //        // empty line
        //        g.매매.dtb.Rows[count][0] = " ";
        //        g.매매.dtb.Rows[count][1] = " ";
        //        g.매매.dtb.Rows[count][2] = " ";
        //        g.매매.dtb.Rows[count][3] = " ";
        //        count++;

        //        // 보유종목 row
        //        foreach (var stock in g.보유종목)
        //        {
        //            int index = wk.return_index_of_ogldata(stock);
        //            if (index < 0)
        //                continue;

        //            g.stock_data o = g.ogl_data[index];

        //            if (o.매수1호가 > 0)
        //                o.수익률 = (o.매수1호가 - o.장부가) / (double)o.매수1호가 * 100;

        //            double 매수1호가량_매도1호가량_비례 = 0.0;
        //            if (o.최우선매도호가잔량 > 0)
        //                매수1호가량_매도1호가량_비례 = (double)o.최우선매수호가잔량 / (double)o.최우선매도호가잔량;

        //            g.매매.dtb.Rows[count][0] = o.stock;
        //            g.매매.dtb.Rows[count][1] = o.보유량.ToString() + "/" + (o.매수1호가 / 10000.0).ToString("F3");
        //            g.매매.dtb.Rows[count][2] = o.최우선매수호가잔량.ToString() + "/" + 매수1호가량_매도1호가량_비례.ToString("F1");
        //            g.매매.dtb.Rows[count][3] = o.수익률.ToString("F2");
        //            count++;
        //        }

        //        // empty row
        //        for (int i = count; i < g.매매.dtb.Rows.Count; i++)
        //        {
        //            g.매매.dtb.Rows[i][0] = " ";
        //            g.매매.dtb.Rows[i][1] = " ";
        //            g.매매.dtb.Rows[i][2] = " ";
        //            g.매매.dtb.Rows[i][3] = " ";
        //        }

        //    }
        //}
    }
}
