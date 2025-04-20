using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using New_Tradegy.Library.UI.KeyBindings;
using New_Tradegy.Library;
using New_Tradegy.Library.Trackers;
using System.Drawing;
using New_Tradegy.Library.Core;

namespace New_Tradegy.KeyBindings
{
    public static class StockKeyBindings
    {
        public static void RegisterAll()
        {
            KeyBindingManager.Register(Keys.Escape, false, false, false, DealCancel);   // Press 'f'
            KeyBindingManager.Register('F', false, false, 코피_코닥_관심);   // Press Shift + F
            KeyBindingManager.Register('f', true, false, 코피_코닥_관심);    // Press Ctrl + f
            KeyBindingManager.Register(Keys.Escape, true, false, false, 상순_저순);

            KeyBindingManager.Register('F', true, true, 코피_코닥_관심);    // Press Ctrl + Shift + F
            KeyBindingManager.Register(Keys.Escape, false, false, false, 상순_저순);
            KeyBindingManager.Register(Keys.F, true, false, false, 코피_코닥_관심);
            KeyBindingManager.Register('h', true, true, 코피_코닥_관심);    // Press

            KeyBindingManager.Register('j', false, false, 편차_평균);
            KeyBindingManager.Register('d', false, false, false, 푀분_총점);
            KeyBindingManager.Register('a', false, false, false, 피올_닥올);
            KeyBindingManager.Register('s', false, false, false, 프누_종누);
            KeyBindingManager.Register('S', false, false, false, 프편_종편);
            KeyBindingManager.Register('D', false, false, false, 배차);
            KeyBindingManager.Register('F', false, false, false, 분거);

            KeyBindingManager.Register('z', false, false, false, 토글_옵티멈);
            KeyBindingManager.Register(' ', false, false, false, 화면_다음);
            KeyBindingManager.Register('n', false, false, false, 화면_이전);

            KeyBindingManager.Register(';', false, false, false, 열기_제어);
            KeyBindingManager.Register(':', false, false, false, 열기_상관);
            KeyBindingManager.Register('\'', false, false, false, 열기_메모);
        }
        public static readonly Action<Form> 거래모두취소 = form =>
        {
            mc.Sound("Keys", "cancel");

            for (int i = OrderTracker.OrderMap.Count - 1; i >= 0; i--)
            {
                var data = OrderTracker.GetOrderByRowIndex(i);
                if (data != null)
                    DealManager.DealCancelOrder(data);
            }
        };

        public static readonly Action<Form> 거래액절반 = form =>
        {
            if (g.일회거래액 > 500)
                g.일회거래액 /= 2;
            else if (g.일회거래액 > 100)
                g.일회거래액 = 100;
            else
                g.일회거래액 = 0;
            mc.Sound_돈(g.일회거래액);

            if (g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
            {
                g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
            }
        };

        public static readonly Action<Form> 거래액두배 = form =>
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
        };

        public static readonly Action<Form> 긴시간앞뒤로 = form =>
        {
            if (!g.test) return;
            if (g.EndNptsBeforeExtend == 0) // time extension
                mm.MinuteAdvanceRetreat(g.v.Q_advance_lines);
            else
                mm.MinuteAdvanceRetreat(0);
            var action = ActionCode.New(clear: false, post: true, eval: false, draw: 'B');
            action.Run();
        };

        public static readonly Action<Form> 시간초기설정 = form =>
        {
            if (!g.test) return;
            g.Npts[1]--;
            if (g.Npts[1] < 2)
            {
                g.Npts[0] = 0;
                g.Npts[1] = 2;
            }
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
            action.Run();

        };

        public static readonly Action<Form> 매수1호가매도 = form =>
        {
            //if (g.ActiveBookBid != null)
            //{
            //    var stock = g.ActiveBookBid.Tag as string; // assuming you assigned stock name to Tag
            //    mc.SellStock(stock);
            //    return true;

            //}
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
        };

        public static readonly Action<Form> 매수1호가매수 = form =>
        {
            //if (g.ActiveBookBid != null)
            //{
            //    var stock = g.ActiveBookBid.Tag as string; // assuming you assigned stock name to Tag
            //    mc.SellStock(stock);
            //    return true;

            //}
            if (!g.connected) return;
            //Form f = cl.GetActiveForm();
            //if (f == null)
            //{
            //    return;
            //}
            string stock = f.Name;
            string buySell = "매수";
            if (!wk.isStock(stock))
            {
                mc.Sound("돈", "not hoga");
                return;
            }
            //int 거래가격 = hg.HogaGetValue(stock, -1, 1); // 0 : 매도1호가 라인, 1 : 호가 column
            int Urgency = 100;
            // dl.deal_sett(stock, buySell, 거래가격, Urgency);
        };

        public static readonly Action<Form> ConfirmSellToggle = form =>
        {
            g.confirm_sell = !g.confirm_sell;

            if (g.confirm_sell)
                mc.Sound("Keys", "confirm sell");
            else
                mc.Sound("Keys", "no confirm sell");
        };

        public static readonly Action<Form> 짧은시간앞뒤로 = form =>
        {
            if (!g.test) return;

            if (g.EndNptsBeforeExtend == 0) // time extension
                mm.MinuteAdvanceRetreat(g.v.q_advance_lines);
            else
                mm.MinuteAdvanceRetreat(0);
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
            action.Run();

        };

        public static readonly Action<Form> 일분앞으로 = form =>
        {
            if (!g.test) return;

            g.Npts[1]++;

            if (g.Npts[1] > g.MAX_ROW)
            {
                g.Npts[0] = 0;
                g.Npts[1] = 2;
            }
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
            action.Run();

        };

        public static readonly Action<Form> 일분뒤로 = form =>
        {
            if (!g.test) return;

            g.Npts[1]--;
            if (g.Npts[1] < 2)
            {
                g.Npts[0] = 0;
                g.Npts[1] = 2;
            }
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
            action.Run();

        };

        public static readonly Action<Form> 무게변수조정스크롤 = form =>
        {
            Form Form_무게 = new Form_무게(); // grup
            Form_무게.Show();
            // form to the top
            Form_무게.TopMost = true;
        };

        public static readonly Action<Form> 보조차트그순관심토글 = form =>
        {
            Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
            if (Form_보조_차트 != null)
            {
                List<string> list_6 = new List<string> { "그순", "관심" };
                g.v.SubKeyStr = mc.CycleStrings(g.v.SubKeyStr, list_6);
                Form_보조_차트.Form_보조_차트_DRAW();
            }
        };

        public static readonly Action<Form> 축소그림시간구간줄임 = form =>
        {
            g.NptsForShrinkDraw -= 10;
            if (g.NptsForShrinkDraw <= 10)
            {
                g.NptsForShrinkDraw = 10;
            }
            var action = ActionCode.New(clear: true, post: false, eval: false, draw: 'B');
            action.Run();
        };

        public static readonly Action<Form> 축소그림시간구간늘임 = form =>
        {
            g.NptsForShrinkDraw += 10;
            if (g.NptsForShrinkDraw <= 10)
            {
                g.NptsForShrinkDraw = 10;
            }
            var action = ActionCode.New(clear: true, post: false, eval: false, draw: 'B');
            action.Run();
        };

        public static readonly Action<Form> 시간10초앞으로 = form =>
        {
            if (!g.test) return;

            if (g.draw_selection == 1)
            {
                g.Npts[1] += 10;
                if (g.Npts[1] > g.MAX_ROW)
                {
                    g.Npts[0] = 0;
                    g.Npts[1] = 10;
                }
                var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
                action.Run();
            }
            else
                g.npts_fi_dwm += 10;

        };

        public static readonly Action<Form> 시간10초뒤로 = form =>
        {
            if (!g.test) { return; }

            if (g.draw_selection == 1)
            {
                g.Npts[1] -= 10;
                var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
                action.Run();
            }
            else
                g.npts_fi_dwm -= 10;

        };

        public static readonly Action<Form> 시간30초앞으로 = form =>
        {
            if (!g.test) return;

            if (g.draw_selection == 1)
            {
                g.Npts[1] += 30;
                if (g.Npts[1] > g.MAX_ROW)
                {
                    g.Npts[0] = 0;
                    g.Npts[1] = 10;
                }
                var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
                action.Run();
            }
            else
                g.npts_fi_dwm += 30;
        };

        public static readonly Action<Form> 시간30초뒤로 = form =>
        {
            if (!g.test) { return; }

            if (g.draw_selection == 1)
            {
                g.Npts[1] -= 30;
                var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'B');
                action.Run();
            }
            else
                g.npts_fi_dwm -= 30;

        };

        public static readonly Action<Form> 뉴스dwm토글 = form =>
        {
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
        };

        public static readonly Action<Form> 볼린저 = form =>
        {
            g.draw_selection = 3;
        };

        public static readonly Action<Form> 외인기관가격변동추이그래프 = form =>
        {
            g.draw_selection = 2;
        };

        public static readonly Action<Form> 일반종목 = form =>
        {
            g.draw_selection = 1;
        };

        public static readonly Action<Form> 피올닥올 = form =>
        {
            List<string> list = new List<string> { "피올", "닥올" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);

            //g.v.columnsofoGl_data = 0; // number of column for group = 0
            g.q = "o&s";
            g.gid = 0;
            Form se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
            
        };

        public static readonly Action<Form> 관심종목자동추가 = form =>
        {
            if (!g.connected || g.test)
                return;

            g.add_interest = !g.add_interest;
            if (g.add_interest)
                mc.Sound("일반", "add interest");
            else
                mc.Sound("일반", "no add interest");
        };

        public static readonly Action<Form> 메모열기 = form =>
        {
            Form memo = new Memo();
            memo.Size = new Size(359, 405);
            memo.Location = new Point(809, 509);
            memo.Show();
        };

        public static readonly Action<Form> 프누종누토글 = form =>
        {
            List<string> list_2 = new List<string> { "프누", "종누" };
            g.v.KeyString = mc.cycleStrings(g.v.KeyString, list_2);
            g.q = "o&s";
            g.gid = 0;
            se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 프편종편토글 = form =>
        {
            List<string> list_3 = new List<string> { "프편", "종편" };
            g.v.KeyString = mc.cycleStrings(g.v.KeyString, list_3);
            g.q = "o&s";
            g.gid = 0;
            se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 현재시간기준모두저장 = form =>
        {
            if (g.connected && !g.test)
            {
                string caption = "Save all stocks ?";
                string message = "모든 파일 현재 시간 기준 저장";
                string default_option = "No";
                string result = mc.message(caption, message, default_option);

                if (result == "Yes")
                    wr.SaveAllStocks();
            }
        };
        public static readonly Action<Form> 푀분총점토글 = form =>
        {
            List<string> list_7 = new List<string> { "푀분", "총점" };
            g.v.KeyString = mc.cycleStrings(g.v.KeyString, list_7);
            g.q = "o&s";
            g.gid = 0;
            se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 배차 = form =>
        {
            g.v.KeyString = "배차";
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 보조차트코피코닥관심토글 = form =>
        {
            Form_보조_차트 fa = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
            List <string> list_3 = new List<string> { "코피", "코닥", "관심" };
            g.v.SubKeyStr = mc.CycleStrings(g.v.SubKeyStr, list_3);
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 's');
            action.Run();
        };

        public static readonly Action<Form> 분거 = form =>
        {
            g.v.KeyString = "분거";
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 상순저순토글 = form =>
        {
            List<string> list_4 = new List<string> { "상순", "저순" };
            g.v.KeyString = mc.cycleStrings(g.v.KeyString, list_4);
            g.q = "o&s";
            g.gid = 0;
            se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 편차평균토글 = form =>
        {
            List<string> list_5 = new List<string> { "편차", "평균" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list_5);
            g.q = "o&s";
            g.gid = 0;
            se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.KeyString;
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> HistoryFowardBackwardToggle = form =>
        {
            if (g.test)
                g.draw_history_forwards = !g.draw_history_forwards;
        };

        public static readonly Action<Form> 제어txt열기 = form =>
        {
            string 바탕화면 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filename = 바탕화면 + @"\제어.txt";
            Process.Start(filename);
        };

        public static readonly Action<Form> 상관txt열기 = form =>
        {
            string filename = @"C:\병신\data\상관.txt";
            Process.Start(filename);
        };

        public static readonly Action<Form> 메모txt열기 = form =>
        {
            string filename = @"C:\병신\감\메모.txt";
            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }
            Process.Start(filename);
        };

        public static readonly Action<Form> 최적매매시행여부토글 = form =>
        {
            g.optimumTrading = !g.optimumTrading;
            if (g.optimumTrading)
                mc.Sound("돈", "optimum");
            else
                mc.Sound("돈", "non optimum");
            break;
        };

        public static readonly Action<Form> 웹창전부죽이기 = form =>
        {
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
        };

        public static readonly Action<Form> 호가종목전부제거 = form =>
        {
            for (int i = g.StockManager.InterestedWithBidList.Count - 1; i >= 0; i--)
            {
                // rd.read_관심제거추가(g.StockManager.InterestedWithBidList[i]); // this does nothing
                g.StockManager.InterestedWithBidList.Remove(g.StockManager.InterestedWithBidList[i]);
            }
        };

        public static readonly Action<Form> 관심종목전부제거 = form =>
        {
            for (int i = g.StockManager.InterestedOnlyList.Count - 1; i >= 0; i--)
            {
                // rd.read_관심제거추가(g.StockManager.InterestedOnlyList[i]); // this does nothing
                g.StockManager.InterestedOnlyList.Remove(g.StockManager.InterestedOnlyList[i]);
            }
        };

        public static readonly Action<Form> HistoryForward = form =>
        {
            if (g.test)
            {
                wk.date_backwards_forwards("forwards");
            }
        };

        public static readonly Action<Form> HistoryBackward = form =>
        {
            if (g.test)
            {
                wk.date_backwards_forwards("backwards");
            }
        };

        public static readonly Action<Form> HistoryCurrentSetting = form =>
        {

        };

        public static readonly Action<Form> 창의다음리스트 = form =>
        {
            switch (g.q)
            {
                case "o&s":
                    int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count;
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
            var action = ActionCode.New(clear: false, post: false, eval: false, draw: 'm');
            action.Run();
        };

        public static readonly Action<Form> 창의전리스트 = form =>
        {
            switch (g.q)
            {
                case "o&s":
                    //case "e&s":
                    int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count;
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
            var action = ActionCode.New(clear: false, post: false, eval: false, draw: 'm');
            action.Run();
        };
    };
}

