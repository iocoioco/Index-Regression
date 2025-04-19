using New_Tradegy.Library;
using New_Tradegy.Library.UI.KeyBindings;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace New_Tradegy.KeyBindings
{
    // 🧱 Refactored Key Bindings for Stock Trading System
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;

    namespace New_Tradegy.KeyBindings
    {
        // 🧱 Refactored Key Bindings for Stock Trading System
        using System;
        using System.Windows.Forms;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.IO;

        namespace New_Tradegy.KeyBindings
        {
            public static class StockKeyBindings
            {
                public static void RegisterAll()
                {
                    KeyBindingManager.Register('g', false, false, false, 상순_저순);
                    KeyBindingManager.Register('f', false, false, false, 코피_코닥_관심);
                    KeyBindingManager.Register('h', false, false, false, 편차_평균);
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
                    KeyBindingManager.Register(''', false, false, false, 열기_메모);
                }

                public static readonly Action<Form> 상순_저순 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "상순", "저순" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, eval: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 코피_코닥_관심 = form =>
                {
                    g.v.SubKeyStr = mc.CycleStrings(g.v.SubKeyStr, new List<string> { "코피", "코닥", "관심" });
                    ActionCode.New(post: true, draw: 's').Run();
                };

                public static readonly Action<Form> 편차_평균 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "편차", "평균" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, eval: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 푀분_총점 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "푀분", "총점" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, eval: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 피올_닥올 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "피올", "닥올" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, eval: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 프누_종누 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "프누", "종누" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 프편_종편 = form =>
                {
                    g.v.KeyString = mc.CycleStrings(g.v.KeyString, new List<string> { "프편", "종편" });
                    g.q = "o&s";
                    g.gid = 0;
                    Application.OpenForms["Form1"].Text = g.v.KeyString;
                    ActionCode.New(clear: true, post: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 배차 = form =>
                {
                    g.v.KeyString = "배차";
                    ActionCode.New(clear: true, post: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 분거 = form =>
                {
                    g.v.KeyString = "분거";
                    ActionCode.New(clear: true, post: true, eval: true, draw: 'm').Run();
                };

                public static readonly Action<Form> 토글_옵티멈 = form =>
                {
                    g.optimumTrading = !g.optimumTrading;
                    mc.Sound("돈", g.optimumTrading ? "optimum" : "non optimum");
                };

                public static readonly Action<Form> 화면_다음 = form =>
                {
                    if (g.q == "o&s")
                    {
                        int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count;
                        int move = (g.nCol - 2) * g.nRow - count;
                        g.gid = (g.gid + move < g.sl.Count) ? g.gid + move : 0;
                    }
                    else if (g.q == "h&s")
                    {
                        for (int i = 1; i < (g.nCol - 2) * g.nRow; i++)
                        {
                            int date = g.draw_history_forwards ? wk.directory_분전후(g.moving_reference_date, +1) : wk.directory_분전후(g.moving_reference_date, -1);
                            if (date == -1) return;
                            g.moving_reference_date = date;
                        }
                    }
                    ActionCode.New(draw: 'm').Run();
                };

                public static readonly Action<Form> 화면_이전 = form =>
                {
                    if (g.q == "o&s")
                    {
                        int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count;
                        int move = (g.nCol - 2) * g.nRow - count;
                        g.gid = (g.gid - move >= 0) ? g.gid - move : 0;
                    }
                    else if (g.q == "h&s")
                    {
                        for (int i = 1; i < (g.nCol - 2) * g.nRow; i++)
                        {
                            int date = wk.directory_분전후(g.moving_reference_date, 1);
                            if (date == -1) return;
                            g.moving_reference_date = date;
                        }
                    }
                    ActionCode.New(draw: 'm').Run();
                };

                public static readonly Action<Form> 열기_제어 = form =>
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "제어.txt");
                    Process.Start(path);
                };

                public static readonly Action<Form> 열기_상관 = form =>
                {
                    string path = "C:\\병신\\data\\상관.txt";
                    Process.Start(path);
                };

                public static readonly Action<Form> 열기_메모 = form =>
                {
                    string path = "C:\\병신\\감\\메모.txt";
                    if (!File.Exists(path)) File.Create(path).Close();
                    Process.Start(path);
                };
            }
        }

    }

}
