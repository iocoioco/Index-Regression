using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using New_Tradegy.Library;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Deals;

namespace New_Tradegy.Library.UI.KeyBindings
{
    public static class ActionHandlers
    {
        // Function
        #region
        // Escape
        public static readonly Action DealCancel_TimeInitalKey = () =>
        {
            if (!g.test)
            {
                SoundUtils.Sound("Keys", "cancel");
                for (int i = OrderItemTracker.OrderMap.Count - 1; i >= 0; i--)
                {
                    var data = OrderItemTracker.GetOrderByRowIndex(i);
                    if (data != null)
                        DealManager.DealCancelOrder(data);
                }
            }
        };

        // F1
        public static readonly Action DealHalf_TimeFinalKey = () =>
        {
            if (!g.test)
            {
                if (g.일회거래액 > 500)
                    g.일회거래액 /= 2;
                else if (g.일회거래액 > 100)
                    g.일회거래액 = 100;
                else
                    g.일회거래액 = 0;

                SoundUtils.Sound_돈(g.일회거래액);
                if (g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
                    g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
            }
        };

        // F2
        public static readonly Action DealDoubleKey = () =>
        {
            if (!g.test)
            {
                if (g.일회거래액 < 100)
                    g.일회거래액 = 100;
                else if (g.일회거래액 < 500)
                    g.일회거래액 = 500;
                else
                    g.일회거래액 *= 2;

                SoundUtils.Sound_돈(g.일회거래액);

                if (g.일회거래액 > 4000)
                {
                    var result = MessageBox.Show(new Form { TopMost = true },
                        "                     더블 ?", "일회거래액",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                        g.일회거래액 /= 2;
                }

                if (g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
                    g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
            }
        };
        

        // F3
        public static readonly Action DealConfirmSellKey = () =>
        {
            if (!g.test)
            {
                g.confirm_sell = !g.confirm_sell;

                if (g.confirm_sell)
                    SoundUtils.Sound("Keys", "confirm sell");
                else
                    SoundUtils.Sound("Keys", "no confirm sell");
            }
        };


        // F5
        public static readonly Action DealEmergencySellKey = () =>
        {
            if (!g.test)
            {
                g.confirm_sell = !g.confirm_sell;

                if (g.confirm_sell)
                    SoundUtils.Sound("Keys", "confirm sell");
                else
                    SoundUtils.Sound("Keys", "no confirm sell");
            }
        };
        #endregion

        // Number
        #region
        public static readonly Action 피올_닥올 = () =>
        {
            var list = new[] { "피올", "닥올" };
            g.v.key_string = StringUtils.CycleStrings(g.v.key_string, list);
            g.q = "o&s";
            g.gid = 0;
            Form se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.key_string;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 푀누_종누 = () =>
        {
            var list = new[] { "프누", "종누" };
            g.v.KeyString = StringUtils.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 푀분_총점 = () =>
        {
            var list = new[] { "푀분", "총점" };
            g.v.KeyString = StringUtils.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 상순_저순 = () =>
        {
            var list = new[] { "상순", "저순" };
            g.v.KeyString = StringUtils.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 편차_평균 = () =>
        {
            var list = new[] { "편차", "평균" };
            g.v.KeyString = StringUtils.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 배차_분거 = () =>
        {
            var list = new[] { "배차", "분거" };
            g.v.KeyString = StringUtils.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 가격증순 = () =>
        {
            g.v.KeyString = "가증";
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };
        #endregion

        // Top
        #region
        // 1
        public static readonly Action TimeShortMoveKey = () =>
        {
            if (g.test) // 짧은 시간 앞으로 in draw(테스트)
            {
                if (g.end_time_before_advance == 0) // time extension
                    dr.draw_extended_time(g.v.q_advance_lines);
                else
                    dr.draw_extended_time(0);
            }
        };


        public static readonly Action TimeOneForwardsKey = () =>
        {
            if (g.test) // 시간 앞으로 (테스트)

                g.time[1]++;
            if (g.time[1] > g.MAX_ROW)
            {
                g.time[0] = 0;
                g.time[1] = 2;
            }
        };

        public static readonly Action TimeOneBackwardsKey = () =>
        {
            if (g.test) // 시간 뒤로 (테스트)
            {
                g.time[1]--;
                if (g.time[1] < 2)
                {
                    g.time[0] = 0;
                    g.time[1] = 2;
                }
            }
        };

        public static readonly Action TimeInitializeKey = () =>
        {
            if (g.test) // 시초
            {
                g.time[0] = 0;
                g.time[1] = 1;
            }
        };

       
        public static readonly Action WeightControlKey = () =>
        {
            Form Form_무게 = new Form_무게(); // grup
            Form_무게.Show();
            // form to the top
            Form_무게.TopMost = true;
        };

        public static readonly Action TimeLongMoveKey = () =>
        {
            if (g.test) // 긴 시간 앞으로 in draw
            {
                if (g.end_time_before_advance == 0) // time extension
                    dr.draw_extended_time(g.v.Q_advance_lines);
                else
                    dr.draw_extended_time(0);
            }
        };

        public static readonly Action TimeFinalKey = () =>
        {
            if (g.test) // 긴 시간 앞으로 in draw
            {
                g.time[0] = 0;
                g.time[1] = g.MAX_ROW;
            }
        };

        public static readonly Action TimeTenForwardsKey = () =>
        {
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
        };

        public static readonly Action TimeTenBackwardsKey = () =>
        {
            if (g.test)
            {
                if (g.draw_selection == 1)
                {
                    g.time[1] -= 10;
                }
                else
                    g.npts_fi_dwm -= 10;
            }
        };

        public static readonly Action TimeThirtyForwardsKey = () =>
        {
            if (g.test)
            {
                g.time[1] += 30;

                if (g.time[1] > g.MAX_ROW)
                {
                    g.time[0] = 0;
                    g.time[1] = 30;
                }
            }
        };

        public static readonly Action TimeThirtyBackwardsKey = () =>
        {
            if (g.test)
            {
                g.time[1] -= 30;

                if (g.time[1] < 2)
                {
                    g.time[0] = 0;
                    g.time[1] = 2;
                }
            }
        };

        public static readonly Action ShrinkOrNotTenPlusKey = () =>
        {
            g.draw_shrink_time += 10;
        };

        public static readonly Action ShrinkOrNotTenMinusKey = () =>
        {
            g.draw_shrink_time -= 10;
            if (g.draw_shrink_time <= 10)
            {
                g.draw_shrink_time = 10;
            }
        };

        public static readonly Action OpenFilesKey = () => // 제어, 상관, 관심 등
        {
            if (g.test)
            {
                wk.date_backwards_forwards("forwards");
            }
        };
        public static readonly Action OpenMemoKey = () => // 제어, 상관, 관심 등
        {
            if (g.test)
            {
                wk.date_backwards_forwards("forwards");
            }
        };

        public static readonly Action NewsPeoridKey = () =>
        {
            if (g.PeoridNews == 'd')
            {
                g.PeoridNews = 'w';
                SoundUtils.Sound("일반", "news week");
            }

            else if (g.PeoridNews == 'w')
            {
                g.PeoridNews = 'm';
                SoundUtils.Sound("일반", "news month");
            }
            else
            {
                g.PeoridNews = 'd';
                SoundUtils.Sound("일반", "news day");
            }
        };

        public static readonly Action DrawBollingerKey = () =>
        {
            g.draw_selection = 3;
        };


        public static readonly Action DrawForeignAndInstituteKey = () =>
        {
            g.draw_selection = 2;
        };

        public static readonly Action DrawNormaStockKey = () =>
        {
            g.draw_selection = 1;
        };

        #endregion

        // Home
        #region
        public static readonly Action 보조차트_피올_닥올 = () =>
        {
            Form_보조_차트 fa = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
            list_3 = new List<string> { "코피", "코닥" };
            g.v.SpfKeyString = StringUtils.CycleStrings(g.v.SpfKeyString, list_3);
            dr.draw_보조_차트(g.v.SpfKeyString);
        };

        public static readonly Action AddInterest = () =>
        {
            if (g.test)
                return;

            g.add_interest = !g.add_interest;
            if (g.add_interest)
                SoundUtils.Sound("일반", "add interest");
            else
                SoundUtils.Sound("일반", "no add interest");
        };

        public static readonly Action 보조차트_순위_관심 = () =>
        {
            Form_보조_차트 Form_보조_차트 = (Form_보조_차트)Application.OpenForms["Form_보조_차트"];
            if (Form_보조_차트 != null)
            {
                List<string> list_6 = new List<string> { "그순", "관심" };
                Form_보조_차트.keyString = StringUtils.CycleStrings(Form_보조_차트.keyString, list_6);
                Form_보조_차트.Form_보조_차트_DRAW();
                //Form_보조_차트.TopMost = true;
                //Form_보조_차트.TopMost = false;
            }
        };

        public static readonly Action SaveAllStocks = () =>
        {
            if (!g.test)
            {
                string caption = "Save all stocks ?";
                string message = "모든 파일 현재 시간 기준 저장";
                string default_option = "No";
                string result = ms.message(caption, message, default_option);

                if (result == "Yes")
                    wr.SaveAllStocks();
            }
        };

        
        #endregion

        // Bottom
        public static readonly Action OptimalTradingKey = () =>
        {
            g.optimumTrading = !g.optimumTrading;
            if (g.optimumTrading)
                SoundUtils.Sound("돈", "optimum");
            else
                SoundUtils.Sound("돈", "non optimum");
        };

        public static readonly Action KillWebTxtKey = () =>
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
        };

        public static readonly Action ListingForwadsKey = () =>
        {
            if (g.test) 
            {
                wk.date_backwards_forwards("forwards");
            }
        };

        public static readonly Action ListingBackwardKey = () =>
        {
            switch (g.q)
            {
                case "o&s":
                    //case "e&s":
                    int count = g.보유종목.Count + g.호가종목.Count; // 지수종목
                    if (g.gid - ((g.nCol - 1) * g.nRow - count) >= 0)
                    {
                        g.gid -= (g.nCol - 1) * g.nRow - count;
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
        };

        public static readonly Action RemoveBookAndInterest = () =>
        {
            for (int i = g.호가종목.Count - 1; i >= 0; i--)
            {
                // rd.read_관심제거추가(g.호가종목[i]); // this does nothing
                g.호가종목.Remove(g.호가종목[i]);
            }
        };

        public static readonly Action MemoOpenKey = () =>
        {
            wn.Memo_TopMost();
        };

        public static readonly Action DateBackwardsKey = () =>
        {
            if (g.test)
            {
                wk.date_backwards_forwards("backwards");
            }
        };

        public static readonly Action DateForwardsKey = () =>
        {
            if (g.test)
            {
                wk.date_backwards_forwards("forwards");
            }
        };
        
    }
}
