
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using System.Drawing;

using New_Tradegy.Library;
using System.Windows.Forms.DataVisualization.Charting;
using static New_Tradegy.Library.g.stock_data;
using System.Collections;

namespace New_Tradegy.Library
{
    internal class ky
    {
        private static TextBox searchTextBox;
        public static void char1_previewkeydown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                #region
                // 매도(ActiveForm)
                case Keys.F9:
                    
                    break;
                    //case Keys.F2: //  지수 과장배수 확대
                    //    switch (g.clickedStock)
                    //    {
                    //        case "KODEX 레버리지":
                    //            g.k.magnifier[0, g.k.index_magnifier_shifter] *= 1.5;
                    //            break;
                    //        case "KODEX 200선물인버스2X":
                    //            g.k.magnifier[1, g.k.index_magnifier_shifter] *= 1.5;
                    //            break;
                    //        case "KODEX 코스닥150레버리지":
                    //            g.k.magnifier[2, g.k.index_magnifier_shifter] *= 1.5;
                    //            break;
                    //        case "KODEX 코스닥150선물인버스":
                    //            g.k.magnifier[3, g.k.index_magnifier_shifter] *= 1.5;
                    //            break;
                    //        default:
                    //            int index = wk.return_index_of_ogldata(g.clickedStock);
                    //            if (index < 0)
                    //                return;
                    //            g.stock_data o = g.ogl_data[index];
                    //            o.수급과장배수 *= 1.5;
                    //            break;
                    //    }

                    //        break;

                    //    case Keys.F4: //  호가종목 추가
                    //        Form se = (Form)Application.OpenForms["se"];
                    //        TextBox tb = (TextBox)se.Controls["textBox1"];
                    //        tb.Text = "";
                    //        tb.Show();
                    //        tb.Size = new Size(150, 20);
                    //        tb.Location = new Point(95, 500);

                    //        break;
                    //}
                    //if (draw)
                    //    dr.draw_chart();
                    #endregion
            }
         }


        public static void chart_keypress(KeyPressEventArgs e)
        {
            if ((short)e.KeyChar > 127)
            {
                mc.Sound("Keys", "Korean");
                return;
            }

            

            switch (e.KeyChar)
            {
                // Functionw
                #region
                case '\u001B': // Escape
                    if (g.test)
                    {
                        if (g.clickedStock == null)
                            return;
                        else // 제외 파일에 클릭된 종목 추가
                        { 
                            // g.clickeStock를 제외.txt에 추가 : 실수 발생 가능 -> block
                            //string file_name = @"C:\병신\data\제외.txt";

                            //List<string> list_1 = rd.read_제외(); 
                            //if(!list_1.Contains(g.clickedStock))
                            //{
                            //    StreamWriter sw = File.AppendText(file_name);
                            //    sw.WriteLine("{0}", g.clickedStock);
                            //    sw.Close();
                            //}
                        }
                    }
                    else
                    {
                        mc.Sound("Keys", "cancel");
                        List<int> keyColl = g.m_mapOrder.Keys.Cast<int>().ToList();

                        for (int i = g.m_mapOrder.Count - 1; i >= 0; i--)
                        {
                            dl.DealCancelRowIndex(i); // Escape Key
                        }
                    }
                    break;
                #endregion

                // Number
                #region
                case '`':
                    {
                        if (g.일회거래액 > 500)
                            g.일회거래액 /= 2;
                        else if (g.일회거래액 > 100)
                            g.일회거래액 = 100;
                        else
                            g.일회거래액 = 0;
                        mc.Sound_돈(g.일회거래액);

                        if(g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
                        {
                            g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
                        }
                    }
                    break;

                case '1':
                    if (g.test) // 긴 시간 앞으로 in draw
                    {
                        if (g.end_time_before_advance == 0) // time extension
                            mm.MinuteAdvanceRetreat(g.v.Q_advance_lines);
                        else
                            mm.MinuteAdvanceRetreat(0);
                    }
                    else
                    {

                        if (g.일회거래액 < 100)
                            g.일회거래액 = 100;
                        else if (g.일회거래액 < 500)
                            g.일회거래액 = 500;
                        else
                            g.일회거래액 *= 2;
                        mc.Sound_돈(g.일회거래액);
                        if (g.일회거래액 > 4000)
                        {
                            string caption1 = "일회거래액";
                            string message1 = "                     더블 ?";

                            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                            DialogResult result1;

                            result1 = MessageBox.Show(new Form { TopMost = true },
                                    message1, caption1, buttons, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                            if (result1 == System.Windows.Forms.DialogResult.No)
                            {
                                g.일회거래액 /= 2;
                            }
                        }
                        if (g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
                        {
                            g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
                        }
                    }
                    break;

                case '2': // 매도
                    if (g.test) // 시간 뒤로 (테스트)
                    {
                        g.time[1]--;
                        if (g.time[1] < 2)
                        {
                            g.time[0] = 0;
                            g.time[1] = 2;
                        }
                    }
                    else
                    {
                        Form f = cl.GetActiveForm();
                        if (f == null)
                        {
                            return;
                        }

                        string stock = f.Name;
                        string buySell = "매도";
                        int 거래가격 = hg.HogaGetValue(f.Name, 0, 1); // 0 : 매수1호가 라인, 1 : 호가 column
                        int Urgency = 100;
                        // dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;

                case '3': // 매수
                    if (!g.test)
                    {
                        Form f = cl.GetActiveForm();
                        if (f == null)
                        {
                            return;
                        }
                        string stock = f.Name;
                        string buySell = "매수";
                        if(!wk.isStock(stock))
                        {
                            mc.Sound("돈", "not hoga");
                            return;
                        }
                        int 거래가격 = hg.HogaGetValue(stock, -1, 1); // 0 : 매도1호가 라인, 1 : 호가 column
                        int Urgency = 100;
                        // dl.deal_sett(stock, buySell, 거래가격, Urgency);
                    }
                    break;

                case '4':

                        g.confirm_sell = !g.confirm_sell;

                        if (g.confirm_sell)
                            mc.Sound("Keys", "confirm sell");
                        else
                            mc.Sound("Keys", "no confirm sell");
                    
                    break;

                case '5':
                    // g.q = "kodex_inverse_single";
                    //g.clickedStock = "KODEX 코스닥150선물인버스";
                    //dl.deal_호가(g.clickedStock); // chart1KeyPress in '5' KODEX 코스닥150선물인버스 tr(1)
                    //g.clickedStock = "KODEX 200선물인버스2X";
                    //dl.deal_호가(g.clickedStock); // chart1KeyPress in '5' KODEX 200선물인버스2X tr(1)
                    break;

                case '6':
                    break;

                case '7':
                    break;

                case '0':
                    break;
                #endregion

                // TOP
                #region
                case 'q':
                    if (g.test) // 짧은 시간 앞으로 in draw(테스트)
                    {
                        if (g.end_time_before_advance == 0) // time extension
                            mm.MinuteAdvanceRetreat(g.v.q_advance_lines);
                        else
                            mm.MinuteAdvanceRetreat(0);
                    }
                    //else // 매도
                    //    dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "매도", g.일회거래액); // chart1KeyPress in tr(3)

                    break;

                case 'Q':
                    //dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "대기매도", g.일회거래액); // chart1KeyPress in 'Q'  tr(3)
                    break;

                case '\u0011': // Cntl + q, 비상매도
                    if (g.test)
                        return;
                    //dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "시장가", g.일회거래액); // chart1KeyPress in 'Cntl + q'  tr(3)
                    break;


                case 'w':
                    if (g.test) // 시간 앞으로 (테스트)
                    {
                        g.time[1]++;
                        if (g.time[1] > g.MAX_ROW)
                        {
                            g.time[0] = 0;
                            g.time[1] = 2;
                        }
                    }
                    // 매수
                        //dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "매수", g.일회거래액); // 
                    
                    break;

                case 'W':
                    if (g.test) // 시초
                    {
                            g.time[0] = 0;
                            g.time[1] = 1;
                    }
                     // 대기매수
                        // dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "대기매수", g.일회거래액);

                    break;

                case '\u0017': // cntl + w
                    Form Form_무게 = new Form_무게(); // grup
                    Form_무게.Show();
                    // form to the top
                    Form_무게.TopMost = true;

                    // dl.deal_trade_by_key(g.호가[0].dgv, g.매매.dgv, "시장가매수", g.일회거래액);
                    break;

                case 'e':
                    Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
                    if (Form_보조_차트 != null)
                    {
                        List<string> list_6= new List<string> { "그순", "관심" };
                        Form_보조_차트.keyString = mc.cycleStrings(Form_보조_차트.keyString, list_6);
                        Form_보조_차트.Form_보조_차트_DRAW();
                    }
                    break;

                case 'E':
                    
                    break;

                case 'r':
                    g.draw_shrink_time -= 10;
                    if (g.draw_shrink_time <= 10)
                    {
                        g.draw_shrink_time = 10;
                    }
                    break;

                case 'R':
                    g.draw_shrink_time += 10;
                    break;

                case '\u0012': // Cntl + R
                    
                    break;

                case 't':
                    if (g.test)
                    {
                        if (g.draw_selection == 1)
                        {
                            g.time[1] += 10;
                            if (g.time[1] > g.MAX_ROW)
                            {
                                g.time[0] = 0;
                                g.time[1] = 10;
                            }
                        }
                        else
                            g.npts_fi_dwm += 10;
                    }
                    break;

                case 'T':
                    if (g.test)
                    {
                        if (g.draw_selection == 1)
                        {
                            g.time[1] -= 10;
                        }
                        else
                            g.npts_fi_dwm -= 10;
                    }
                    break;

                case 'y':
                    if (g.test)
                    {
                        g.time[1] += 30;

                        if (g.time[1] > g.MAX_ROW)
                        {
                            g.time[0] = 0;
                            g.time[1] = 30;
                        }
                    }
                    break;

                case 'Y': //
                    if (g.test)
                    {
                        g.time[1] -= 30;

                        if (g.time[1] < 2)
                        {
                            g.time[0] = 0;
                            g.time[1] = 2;
                        }
                    }
                    break;

                case 'u':

                    break;

                case 'U':

                    break;

                case 'i':

                    break;

                case 'I':

                    break;

                case 'o':
                    
                    break;
                case 'O':

                    break;

                case 'p':
                    if (g.PeoridNews == 'd')
                    {
                        g.PeoridNews = 'w';
                        mc.Sound("일반", "news week");
                    }
                        
                    else if (g.PeoridNews == 'w')
                    {
                        g.PeoridNews = 'm';
                        mc.Sound("일반", "news month");
                    }
                    else
                    {
                        g.PeoridNews = 'd';
                        mc.Sound("일반", "news day");
                    }
                    break;

                case 'P':
                    break;

                case '[':   // Bollinger
                    g.draw_selection = 3;
                    break;

                case ']':   // draw 외인, 기관, 가격
                    g.draw_selection = 2;
                    break;

                case '\\':   // draw 종목
                    g.draw_selection = 1;
                    break;

                
                #endregion

                // Middle
                #region
                case 'a':
                    List<string> list = new List<string> {  "피올", "닥올" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list);

                    g.v.columnsofoGl_data = 0; // number of column for group = 0
                    g.q = "o&s";
                    g.gid = 0;
                    Form se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case 'A':
                    if (g.test)
                        return;

                    g.add_interest = !g.add_interest;
                    if (g.add_interest)
                        mc.Sound("일반", "add interest");
                    else
                        mc.Sound("일반", "no add interest");
                    break;

                case '\u0001': // Cntl + A
                    wn.Memo_TopMost();
                    break;

                case 's':
                    List<string> list_2 = new List<string> { "프누", "종누" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list_2);
                    g.q = "o&s";
                    g.gid = 0;
                    se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case 'S':
                    List<string> list_3 = new List<string> { "프편", "종편" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list_3);
                    g.q = "o&s";
                    g.gid = 0;
                    se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case '\u0013': // Ctrl + S
                    if (!g.test)
                    {
                        string caption = "Save all stocks ?";
                        string message = "모든 파일 현재 시간 기준 저장";
                        string default_option = "No";
                        string result = mc.message(caption, message, default_option);

                        if (result == "Yes")
                            wr.SaveAllStocks();
                    }
                    break;





                case 'd':
                    List<string> list_7 = new List<string> { "푀분", "총점" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list_7);
                    g.q = "o&s"; 
                    g.gid = 0;
                    se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case 'D':
                    g.v.key_string = "배차";
                    break;

                case 'f':
                    Form_보조_차트 fa = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
                    list_3 = new List<string> { "코피", "코닥", "관심" };
                    g.v.SpfKeyString = mc.cycleStrings(g.v.SpfKeyString, list_3);
                    mm.ManageChart2(g.v.SpfKeyString); // 'f' "코피", "코닥", "관심"
                    break;
    
                case 'F':
                    break;
                case '\u0006': // Ctrl + F
                               // Add TextBox
           
                    break;
               



            
                case 'g':
                    List<string> list_4 = new List<string> { "상순", "저순" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list_4);
                    g.q = "o&s";
                    g.gid = 0;
                    se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case 'G':
                    break;

                case 'h':
                    List<string> list_5 = new List<string> { "편차", "평균" };
                    g.v.key_string = mc.cycleStrings(g.v.key_string, list_5);
                    g.q = "o&s";
                    g.gid = 0;
                    se = (Form)Application.OpenForms["se"];
                    se.Text = g.v.key_string;
                    break;

                case 'H':
                    break;

                case 'J':
                    g.nCol += 1;
                    break;

                case 'j':
                    if (g.nCol > 3)
                    {
                        g.nCol -= 1;
                    }
                    break;

                case 'K':
                 
                    break;

                case 'k':
                    
                    break;

                case 'l':
                    if (g.test)
                        g.draw_history_forwards = !g.draw_history_forwards;
                    break;

                case ';':
                    string 바탕화면 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filename = 바탕화면 + @"\제어.txt";
                    Process.Start(filename);
                    break;
                case ':':
                    filename = @"C:\병신\data\상관.txt";
                    Process.Start(filename);
                    break;

                case '\'':
                    filename = @"C:\병신\감\메모.txt";
                    if(!File.Exists(filename))
                    {
                        File.Create(filename).Close();
                    }
                    Process.Start(filename);
                    break;
                case '"':
                    filename = @"C:\병신\data\상관.txt";
                    Process.Start(filename);
                    break;
                #endregion

                // Bottom
                #region
                case 'z':
                    g.optimumTrading = !g.optimumTrading;
                    if (g.optimumTrading)
                        mc.Sound("돈", "optimum");
                    else
                        mc.Sound("돈", "non optimum");
                    break;

                case 'Z':
                         
                    break;


                case 'x':
                    if (!g.test)
                    {
                        g.click_trade = !g.click_trade;
                        if (g.click_trade)
                        {
                            mc.Sound("Keys", "click trade");
                            g.제어.dgv.Rows[0].DefaultCellStyle.BackColor = Color.Red;
                        }

                        else
                        {
                            mc.Sound("Keys", "no click trade");
                            g.제어.dgv.Rows[0].DefaultCellStyle.BackColor = Color.White;
                        }
                    }
                    break;

                case 'X':

                    break;

                case 'c':
                    Process[] AllBrowsers = Process.GetProcesses();
                    foreach (var process in AllBrowsers)
                    {
                        if (process.MainWindowTitle != "")
                        {
                            string s = process.ProcessName.ToLower();
                            if (s == "iexplore" || s == "iexplorer" || s == "chrome" || s == "firefox" ||
                                s == "notepad" || s == "microsoftedgecp" || s == "microsoftedge" || s.Contains("microsoft edge"))
                                process.Kill();
                        }
                    }
                    Form_매수_매도 Form_매수_매도 = (Form_매수_매도)Application.OpenForms["Form_매수_매도"];
                    if (Form_매수_매도 != null)
                    {
                        Form_매수_매도.Close();
                    }
                    Form_지수_조정 Form_지수_조정 = (Form_지수_조정)Application.OpenForms["Form_지수_조정"];
                    if (Form_지수_조정 != null)
                    {
                        Form_지수_조정.Close();
                    }
                    break;
                     
                case 'm':
                    for (int i = g.호가종목.Count - 1; i >= 0; i--)
                    {
                       // rd.read_관심제거추가(g.호가종목[i]); // this does nothing
                        g.호가종목.Remove(g.호가종목[i]);
                    }
                    break;

                case 'M':
                    for (int i = g.관심종목.Count - 1; i >= 0; i--)
                    {
                        // rd.read_관심제거추가(g.관심종목[i]); // this does nothing
                        g.관심종목.Remove(g.관심종목[i]);
                    }
                    break;

  

                // 화면 전 후 이동
                case '.':
                    if (g.test)
                    {
                        wk.date_backwards_forwards("forwards");
                    }
                    break;

                case ',':
                    if (g.test)
                    {
                        wk.date_backwards_forwards("backwards");
                    }
                    break;

                case ' ':
                    switch (g.q)
                    {
                        case "o&s":
                            int count = g.보유종목.Count + g.호가종목.Count;
                            if (g.gid + ((g.nCol - 2) * g.nRow - count) < g.sl.Count)
                            {
                                g.gid += (g.nCol - 2) * g.nRow - count;
                            }
                            else
                            {
                                g.gid = 0;
                            }
                            break;

                        case "h&s":
                            for (int jndex = 1; jndex < (g.nCol - 2) * g.nRow; jndex++)
                            {
                                int return_date;
                                if (g.draw_history_forwards)
                                    return_date = wk.directory_분전후(g.moving_reference_date, +1); // 거래익일
                                else
                                    return_date = wk.directory_분전후(g.moving_reference_date, -1); // 거래전일
                                if (return_date == -1)
                                {
                                    return;
                                }
                                else
                                {
                                    g.moving_reference_date = return_date;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                case 'n':
                    switch (g.q)
                    {
                        case "o&s":
                            //case "e&s":
                            int count = g.보유종목.Count + g.호가종목.Count; // 지수종목
                            if (g.gid - ((g.nCol - 2) * g.nRow - count) >= 0)
                            {
                                g.gid -= (g.nCol - 2) * g.nRow - count;
                            }
                            else
                            {
                                g.gid = 0;
                            }
                            break;

                        case "h&s":
                            for (int jndex = 1; jndex < (g.nCol - 2) * g.nRow; jndex++)
                            {
                                int return_date = wk.directory_분전후(g.moving_reference_date, 1); // 거래익일
                                if (return_date == -1)
                                {
                                    return;
                                }
                                else
                                {
                                    g.moving_reference_date = return_date;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    return;
                    #endregion
            }

            // time correction 
            if (g.time[0] < 0) { g.time[0] = 0; }
            if (g.time[1] <= g.time[0]) { g.time[1] = g.time[0] + 10; }
            if (g.time[1] < 2) { g.time[1] = 2; }
            if (g.time[1] > g.MAX_ROW){g.time[1] = g.MAX_ROW;}
            if (g.npts_fi_dwm < 5) { g.npts_fi_dwm = 5; } 
            if (g.gid < 0) { g.gid = 0; }
            if (g.Gid < 0) { g.Gid = 0; }

            // Form_보조_차트 was called in the each keys
            // Main Chart Processing for post, eval, draw
            string testpost = "1q";
            string testeval = "qwWrRtTyYasSdDghjJmM., n";
            string testdraw = "qwWrRtTyYasSdDghjJlmM., n";
            

            string realkeys = "rRasSdDghjJmM n"; // r & R : shrink time changer

            if(g.test) // test
            {
                if (testpost.Contains(e.KeyChar))
                {
                    ps.post_test();
                }
                if (testeval.Contains(e.KeyChar))
                {
                    ev.eval_stock();
                }
                if (testdraw.Contains(e.KeyChar))
                {
                    mm.ManageChart1(); // key multi for test
                }
            }
            else // real
            {
                if (realkeys.Contains(e.KeyChar))
                {
                    ev.eval_stock();
                    mm.ManageChart1(); // key multi for real
                }
            }
        }
    }
}
