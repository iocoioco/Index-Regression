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
        public static readonly Action DealCancelOrder = () =>
        {
            SoundUtils.Sound("Keys", "cancel");
            for (int i = OrderItemTracker.OrderMap.Count - 1; i >= 0; i--)
            {
                var data = OrderItemTracker.GetOrderByRowIndex(i);
                if (data != null)
                    DealManager.DealCancelOrder(data);
            }
        };

        public static readonly Action HalfDealMoney = () =>
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
        };

        public static readonly Action DoubleDealMoney = () =>
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
        };

        public static readonly Action 코피_코닥_관심 = () =>
        {
            // logic for cycling key string or toggling view
            var list = new[] { "코피", "코닥", "관심" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
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
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
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
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
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
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 피올_닥올 = () =>
        {
            var list = new[] { "피올", "닥올" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 프누_종누 = () =>
        {
            var list = new[] { "프누", "종누" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 프편_종편 = () =>
        {
            var list = new[] { "프편", "종편" };
            g.v.KeyString = mc.CycleStrings(g.v.KeyString, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.KeyString;

            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 배차 = () =>
        {
            g.v.KeyString = "배차";
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 분거 = () =>
        {
            g.v.KeyString = "분거";
            var action = ActionCode.New(clear: false, post: true, eval: true, draw: 'm');
            action.Run();
        };

        public static readonly Action 토글_옵티멈 = () =>
        {
            g.optimumTrading = !g.optimumTrading;
            if (g.optimumTrading)
                SoundUtils.Sound("돈", "optimum");
            else
                SoundUtils.Sound("돈", "non optimum");
        };

        public static readonly Action 화면_다음 = () =>
        {
            // your paging logic
        };

        public static readonly Action 화면_이전 = () =>
        {
            // your paging logic
        };

        public static readonly Action 열기_제어 = () =>
        {
            Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "제어.txt"));
        };

        public static readonly Action 열기_상관 = () =>
        {
            Process.Start(@"C:\병신\data\상관.txt");
        };

        public static readonly Action 열기_메모 = () =>
        {
            var filename = @"C:\병신\감\메모.txt";
            if (!File.Exists(filename))
                File.Create(filename).Close();
            Process.Start(filename);
        };
    }
}
 