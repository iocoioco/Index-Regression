﻿using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using New_Tradegy.Library;
using New_Tradegy.Library.Utils;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Deals;
using System.Collections.Generic;
using New_Tradegy.Library.IO;
using System.Threading.Tasks;
using System.Linq;
using New_Tradegy.Library.PostProcessing;

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
            else
            {
                g.Npts[0] = 0;
                g.Npts[1] = 1;
            }

        };

        // F1
        public static readonly Action DealHalf_TimeFinalKey = () =>
        {
            if (g.test)
            {
                g.Npts[0] = 0;
                g.Npts[1] = g.TestMaximumRow;
            }
            else
            {
                if (g.일회거래액 > 500)
                    g.일회거래액 /= 2;
                else if (g.일회거래액 > 100)
                    g.일회거래액 = 100;
                else
                    g.일회거래액 = 0;

                SoundUtils.Sound_돈(g.일회거래액);

                if (g.controlPane.GetCellValue(0, 2) != g.일회거래액.ToString())
                {
                    g.controlPane.SetCellValue(0, 2, g.일회거래액);
                }
            }
        };

        // F2
        public static readonly Action DealDoubleKey = () =>
        {
            if (g.test)
            {
                return;
            }
            else
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


                if (g.controlPane.GetCellValue(0, 2) != g.일회거래액.ToString())
                {
                    g.controlPane.SetCellValue(0, 2, g.일회거래액);

                }
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
        public static readonly Action ConfirmSellToggle = () =>
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
            var list = new List<string> { "피올", "닥올" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            Form se = (Form)Application.OpenForms["Form1"];
            se.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 푀누_종누 = () =>
        {
            var list = new List<string> { "푀누", "종누" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 푀분_등합 = () =>
        {
            var list = new List<string> { "푀분", "등합" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 보조차트_피올_닥올 = () =>
        {
            var list = new List<string> { "피올", "닥올" };
            g.v.SubChartDisplayMode = StringUtils.CycleStrings(g.v.SubChartDisplayMode, list);

            ActionCode.New(true, post: false, eval: false, draw: 's').Run();
        };

        public static readonly Action 보조차트_순위_관심 = () =>
        {
            List<string> list = new List<string> { "그순", "관심" };
            g.v.SubChartDisplayMode = StringUtils.CycleStrings(g.v.SubChartDisplayMode, list);

            ActionCode.New(true, post: false, eval: true, draw: 's').Run();
        };

        public static readonly Action 상순_저순 = () =>
        {
            List<string> list = new List<string> { "상순", "저순" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 편차_평균 = () =>
        {
            var list = new List<string> { "편차", "평균" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 배차_분거 = () =>
        {
            var list = new List<string> { "배차", "분거" };
            g.v.MainChartDisplayMode = StringUtils.CycleStrings(g.v.MainChartDisplayMode, list);
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };

        public static readonly Action 가격증순 = () =>
        {
            g.v.MainChartDisplayMode = "가증";
            g.q = "o&s";
            g.gid = 0;
            var form = (Form)Application.OpenForms["Form1"];
            form.Text = g.v.MainChartDisplayMode;

            ActionCode.New(true, post: false, eval: true, draw: 'm').Run();
        };
        #endregion

        // Top
        #region
        public static readonly Action TimeOneForwardsKey = () =>
        {
            if (g.test) // 시간 앞으로 (테스트)
            {
                g.Npts[1]++;
                if (g.Npts[1] > g.TestMaximumRow)
                {
                    g.Npts[0] = 0;
                    g.Npts[1] = 2;
                }

                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }
        };

        public static readonly Action TimeOneBackwardsKey = () =>
        {
            if (g.test) // 시간 뒤로 (테스트)
            {
                if (g.Npts[1] > g.Npts[0] + 1) // time difference more than 2
                    g.Npts[1]--;
                if (g.Npts[1] < 2)
                {
                    g.Npts[0] = 0;
                    g.Npts[1] = 2;
                }
                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }

        };

        public static readonly Action TimeShortMoveKey = () =>
        {
            if (g.test) // 짧은 시간 앞으로 in draw
            {
                if (g.EndNptsBeforeExtend == 0) // time extensionw
                {
                    TimeUtils.MinuteAdvanceRetreat(g.v.q_advance_lines);
                    ActionCode.New(true, post: false, eval: false, draw: 'B').Run();
                }

                else
                {
                    TimeUtils.MinuteAdvanceRetreat(0);
                    ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
                }


            }
        };

        public static readonly Action TimeLongMoveKey = () =>
        {
            if (g.test) // 긴 시간 앞으로 in draw
            {
                if (g.EndNptsBeforeExtend == 0) // time extension
                {
                    TimeUtils.MinuteAdvanceRetreat(g.v.Q_advance_lines);
                    ActionCode.New(true, post: false, eval: false, draw: 'B').Run();
                }

                else
                {
                    TimeUtils.MinuteAdvanceRetreat(0);
                    ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
                }
            }
        };

        public static readonly Action WeightControlKey = () =>
        {
            FormWeights formWeights = new FormWeights();
            formWeights.Show();
            formWeights.TopMost = true; // bring to front
        };


        public static readonly Action TimeTenForwardsKey = () =>
        {
            if (g.test)
            {
                if (g.draw_selection == 1)
                {
                    g.Npts[1] += 10;
                    if (g.Npts[1] > g.RealMaximumRow)
                    {
                        g.Npts[0] = 0;
                        g.Npts[1] = 10;
                    }
                }
                else
                    g.npts_fi_dwm += 10;

                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }

        };

        public static readonly Action TimeTenBackwardsKey = () =>
        {
            if (g.test)
            {
                if (g.draw_selection == 1)
                {
                    g.Npts[1] -= 10;
                }
                else
                    g.npts_fi_dwm -= 10;

                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }

        };

        public static readonly Action TimeThirtyForwardsKey = () =>
        {
            if (g.test)
            {
                g.Npts[1] += 30;

                if (g.Npts[1] > g.TestMaximumRow)
                {
                    g.Npts[0] = 0;
                    g.Npts[1] = 30;
                }

                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }
        };

        public static readonly Action TimeThirtyBackwardsKey = () =>
        {
            if (g.test)
            {
                g.Npts[1] -= 30;

                if (g.Npts[1] < g.Npts[0] + 1)
                {
                    g.Npts[1] = g.Npts[0] + 2;
                }

                ActionCode.New(true, post: true, eval: true, draw: 'B').Run();
            }
        };

        public static readonly Action OpenFilesKey = () => // 제어, 상관, 관심 등
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\병신\data work\";
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        // Open the file with the default editor
                        Process.Start("notepad.exe", file);
                    }
                }
            }
        };

        public static readonly Action OpenMemoKey = () => // 제어, 상관, 관심 등
        {

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
        public static readonly Action ShrinkOrNotTenPlusKey = () =>
        {
            g.NptsForShrinkDraw += 10;
            ActionCode.New(false, false, eval: false, draw: 'B').Run();
        };

        public static readonly Action ShrinkOrNotTenMinusKey = () =>
        {
            g.NptsForShrinkDraw -= 10;
            if (g.NptsForShrinkDraw <= 10)
            {
                g.NptsForShrinkDraw = 10;
            }
            ActionCode.New(false, false, eval: false, draw: 'B').Run();
        };

        public static readonly Action AddInterestToggle = () =>
        {
            if (g.test)
                return;

            g.add_interest = !g.add_interest;
            if (g.add_interest)
                SoundUtils.Sound("일반", "add interest");
            else
                SoundUtils.Sound("일반", "no add interest");
        };

        public static readonly Func<Task> SaveAllStocks = async () =>
        {
            if (!g.test)
            {
                string caption = "Save all stocks ?";
                string message = "모든 파일 현재 시간 기준 저장";
                string default_option = "No";


                //Form1.Instance.BringToFront();
                //Form1.Instance.Activate();
                //Form1.Instance.Focus();

                // string result = StringUtils.message(Form1.Instance, caption, message, default_option);

                string result = StringUtils.message(caption, message, default_option);


                if (result == "Yes")
                    await FileOut.SaveAllStocks();
            }
        };

        public static readonly Func<Task> nRowDecrease = async () =>
        {
            if (g.nRow > 2)
                g.nRow--;
            ActionCode.New(false, false, eval: false, draw: 'm').Run();
        };

        public static readonly Func<Task> nRowIncrease = async () =>
        {
            g.nRow++;
            ActionCode.New(false, false, eval: false, draw: 'm').Run();
        };

        public static readonly Func<Task> nColDecrease = async () =>
        {
            if (g.nCol > 2)
                g.nCol--;
            ActionCode.New(false, false, eval: false, draw: 'm').Run();
        };

        public static readonly Func<Task> nColIncrease = async () =>
        {
            g.nCol++;
            ActionCode.New(false, false, eval: false, draw: 'm').Run();
        };

        #endregion


        // Bottom
        #region
        public static readonly Action OptimalTradingToggleKey = () =>
        {
            g.optimumTrading = !g.optimumTrading;
            if (g.optimumTrading)
                SoundUtils.Sound("돈", "optimum");
            else
                SoundUtils.Sound("돈", "non optimum");
        };

        public static readonly Action RemoveInterestedOnlyListKey = () =>
        {
            for (int i = g.StockManager.InterestedOnlyList.Count - 1; i >= 0; i--)
            {
                // rd.read_관심제거추가(g.호가종목[i]); // this does nothing
                g.StockManager.InterestedOnlyList.Remove(g.StockManager.InterestedOnlyList[i]);
            }
        };

        public static readonly Action RemoveInterestedWithBidListKey = () =>
        {
            for (int i = g.StockManager.InterestedWithBidList.Count - 1; i >= 0; i--)
            {
                // rd.read_관심제거추가(g.호가종목[i]); // this does nothing
                g.StockManager.InterestedWithBidList.Remove(g.StockManager.InterestedWithBidList[i]);
            }
        };

        public static readonly Action KillWebTxtFormKey = () =>
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
        };

        public static readonly Action PreviousPage = () =>
        {
            switch (g.q)
            {
                case "o&s":
                    //case "e&s":
                    int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count; // 지수종목
                    if (g.gid - ((g.nCol - 2) * g.nRow - count) >= 0)
                    {
                        g.gid -= (g.nCol - 2) * g.nRow - count;
                    }
                    else
                    {
                        g.gid = 0;
                    }
                    ActionCode.New(false, false, eval: false, draw: 'm').Run();
                    break;

                case "h&s":
                    for (int jndex = 1; jndex < (g.nCol - 2) * g.nRow; jndex++)
                    {
                        int return_date = wk.GetAdjacentDateFolder(g.moving_reference_date, 1); // 거래익일
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

        public static readonly Action NextPage = () =>
        {
            switch (g.q)
            {
                case "o&s":
                    int count = g.StockManager.HoldingList.Count + g.StockManager.InterestedWithBidList.Count;
                    if (g.gid + ((g.nCol - 2) * g.nRow - count) < g.StockRepository.AllGeneralDatas.Count())
                    {
                        g.gid += (g.nCol - 2) * g.nRow - count;
                    }
                    else
                    {
                        g.gid = 0;
                    }
                    ActionCode.New(false, false, eval: false, draw: 'm').Run();
                    break;

                case "h&s":
                    for (int jndex = 1; jndex < (g.nCol - 2) * g.nRow; jndex++)
                    {
                        int return_date;
                        if (g.draw_history_forwards)
                            return_date = wk.GetAdjacentDateFolder(g.moving_reference_date, +1); // 거래익일
                        else
                            return_date = wk.GetAdjacentDateFolder(g.moving_reference_date, -1); // 거래전일
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





        public static void PrevDatewisePage()
        {
            if (g.v.MainChartDisplayMode != "Datewise") return;

            g.DataOffset++;
            g.ChartMain.DisplayDatewiseStockHistory(g.clickedStock, g.DataOffset);
            Utils.SoundUtils.Sound("time", "date backwards");
        }

        public static void NextDatewisePage()
        {
            if (g.v.MainChartDisplayMode != "Datewise") return;

            if (g.DataOffset > 0)
            {
                g.DataOffset--;
                g.ChartMain.DisplayDatewiseStockHistory(g.clickedStock, g.DataOffset);
                Utils.SoundUtils.Sound("time", "date forwards");
            }
        }
    };
    #endregion
}

