using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Utils
{
    internal class StringUtils
    {
        public static string[] CollectWordsFromString(string text)
        {
            return text.Split(new char[] { ' ', ',', '.', '!', '?', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string r3_display_lines_body(StockData t)
        {
            int start_row, end_row;
            if (g.test)
            {
                end_row = (t.Api.nrow < g.Npts[1]) ? t.Api.nrow : g.Npts[1];
            }
            else
            {
                end_row = t.Api.nrow;
            }

            start_row = g.Npts[0];

            if (end_row - g.v.q_advance_lines - 7 > start_row)
                start_row = end_row - g.v.q_advance_lines - 7;

            string str = "";

            if (t.Stock.Contains("KODEX"))
            {
                for (int j = start_row; j < end_row; j++)
                {
                    if (j < 0) continue;
                    if (t.Api.x[j, 0] == 0 || t.Api.x[j, 0] > 152100) break;

                    for (int k = 0; k < 12; k++)
                    {
                        if (k == 7) continue;

                        if (end_row - j - 1 == g.v.q_advance_lines && k == 0)
                            str += string.Format("{0, 7}", "* " + t.Api.x[j, k]);
                        else
                            str += string.Format("{0, 7}", t.Api.x[j, k]);
                    }

                    str += "\n";
                }
            }
            else
            {
                for (int j = start_row; j < end_row; j++)
                {
                    if (j < 0) continue;
                    if (t.Api.x[j, 0] == 0 || t.Api.x[j, 0] > 152100) break;

                    string str_add = "";

                    for (int k = 0; k < 13; k++)
                    {
                        if (k == 4 || k == 5 || k == 6 || k == 7 || k == 9 || k == 11)
                            continue;

                        if (k == 12)
                        {
                            double money_factor = t.Api.전일종가 / g.천만원;
                            double dealt = 0.0, prog = 0.0, foreign = 0.0, percentage = 0.0;

                            double acc_foreign = t.Api.x[j, 5] * money_factor;
                            double acc_program = t.Api.x[j, 4] * money_factor;

                            if (j - 1 >= 0)
                            {
                                prog = (t.Api.x[j, 4] - t.Api.x[j - 1, 4]) * money_factor;
                                foreign = (t.Api.x[j, 5] - t.Api.x[j - 1, 5]) * money_factor;
                                dealt = (t.Api.x[j, 7] - t.Api.x[j - 1, 7]) * money_factor;

                                if (MathUtils.IsSafeToDivide(dealt))
                                {
                                    percentage = prog / dealt * 100.0;
                                }
                            }

                            string tstring = "     " + (int)dealt + "  " +
                                             (int)prog + "  " +
                                             (int)percentage + "%" + "  " +
                                             (int)acc_program;

                            str_add = string.Format("{0}", tstring.PadLeft(30));
                        }
                        else if (k == 8)
                        {
                            str_add = string.Format("{0,15}", t.Api.x[j, k] + " / " + t.Api.x[j, k + 1]);
                        }
                        else if (k == 10)
                        {
                            str_add = string.Format("{0,12}", t.Api.x[j, k] + " / " + t.Api.x[j, k + 1]);
                        }
                        else
                        {
                            if (end_row - j - 1 == g.v.q_advance_lines && k == 0)
                            {
                                str_add = string.Format("{0, 10}", "*" + t.Api.x[j, k]);
                            }
                            else if (k == 3)
                            {
                                str_add = string.Format("{0, 11}", t.Api.x[j, k] / 100);
                            }
                            else
                            {
                                str_add = string.Format("{0, 11}", t.Api.x[j, k]);
                            }
                        }

                        str += str_add;
                    }

                    str += "\n";
                }
            }

            return str;
        }

        public static string r3_display_매수_매도(StockData t)
        {
            int start_row, end_row;

            if (g.test)
            {
                end_row = (t.Api.nrow < g.Npts[1]) ? t.Api.nrow : g.Npts[1];
            }
            else
            {
                end_row = t.Api.nrow;
            }

            start_row = end_row - 5;
            if (start_row < 0)
                start_row = 0;

            string str = "";

            if (t.Stock.Contains("KODEX"))
            {
                for (int j = start_row; j < end_row; j++)
                {
                    if (j < 0) continue;
                    if (t.Api.x[j, 0] == 0 || t.Api.x[j, 0] > 152100) break;

                    for (int k = 1; k < 12; k++)
                    {
                        if (k == 7) continue;

                        if (end_row - j - 1 == g.v.q_advance_lines && k == 0)
                            str += string.Format("{0, 7}", "* " + t.Api.x[j, k]);
                        else
                            str += string.Format("{0, 7}", t.Api.x[j, k]);
                    }
                    str += "\n";
                }
            }
            else
            {
                for (int j = start_row; j < end_row; j++)
                {
                    if (j < 0) continue;
                    if (t.Api.x[j, 0] == 0 || t.Api.x[j, 0] > 152100) break;

                    string str_add = "";

                    for (int k = 1; k < 13; k++)
                    {
                        if (k == 4 || k == 5 || k == 6 || k == 7 || k == 9 || k == 11)
                            continue;

                        if (k == 12)
                        {
                            double money_factor = t.Api.전일종가 / g.천만원;
                            double dealt_money_per_minute = 0.0;
                            double program_money_per_minute = 0.0;
                            double foreign_money_per_minute = 0.0;
                            double percentage_program_per_minute = 0.0;

                            double accumulated_foreign_money = t.Api.x[j, 5] * money_factor;
                            double accumulated_program_money = t.Api.x[j, 4] * money_factor;

                            if (j - 1 >= 0)
                            {
                                program_money_per_minute = (t.Api.x[j, 4] - t.Api.x[j - 1, 4]) * money_factor;
                                foreign_money_per_minute = (t.Api.x[j, 5] - t.Api.x[j - 1, 5]) * money_factor;
                                dealt_money_per_minute = (t.Api.x[j, 7] - t.Api.x[j - 1, 7]) * money_factor;

                                if (MathUtils.IsSafeToDivide(dealt_money_per_minute))
                                {
                                    percentage_program_per_minute = (program_money_per_minute / dealt_money_per_minute) * 100.0;
                                }
                            }

                            str_add = "     " + (int)dealt_money_per_minute + "  " +
                                (int)program_money_per_minute + "  " +
                                (int)percentage_program_per_minute + "%" + "  " +
                                (int)accumulated_program_money;
                        }
                        else if (k == 8)
                        {
                            str_add = string.Format("{0,12}", t.Api.x[j, k] + " / " + t.Api.x[j, k + 1]);
                        }
                        else if (k == 10)
                        {
                            str_add = string.Format("{0,12}", t.Api.x[j, k] + " / " + t.Api.x[j, k + 1]);
                        }
                        else
                        {
                            if (end_row - j - 1 == g.v.q_advance_lines && k == 0)
                                str_add = "*" + string.Format("{0, 7}", t.Api.x[j, k]);
                            else
                                str_add = string.Format("{0, 8}", t.Api.x[j, k]);
                        }

                        str += str_add;
                    }

                    str += "\n";
                }
            }

            return str;
        }

        public static string r3_display_lines_header(StockData o)
        {
            string str = "";

            if (o.Stock.Contains("KODEX"))
            {
                str = o.Stock + "\n\n";
            }
            else
            {
                str = o.Stock + "   " + Math.Round(o.Statistics.일간변동편차, 1) + "  " + (o.Post.종거천 / 10).ToString("F0") + "\n\n";

                for (int i = 0; i < 5; i++)
                {
                    if (i >= o.Misc.Friends.Count)
                        break;

                    var words = o.Misc.Friends[i].Split('\t');
                    str += string.Format("{0, -20} {1, 30}", words[1], words[0]) + "\n";
                }
                str += "\n";

                str +=
                    "  " + o.Statistics.푀분_avr.ToString("F1") + "\t" + o.Statistics.푀분_dev.ToString("F1") + "\t" +
                    o.Statistics.거분_avr.ToString("F1") + "\t" + o.Statistics.거분_dev.ToString("F1") + "\t      푀분     거분 (단위 : 천만)\n" +
                    "  " + o.Statistics.배차_avr.ToString("F0") + "\t" + o.Statistics.배차_dev.ToString("F0") + "\t" +
                    o.Statistics.배합_avr.ToString("F0") + "\t" + o.Statistics.배합_dev.ToString("F0") + "\t      배차     배합\n\n";
            }

            return str;
        }

        public static void r3_display_lines(Chart chart, string stock, int row_id, int col_id)
        {
            if (!g.StockRepository.Contains(stock))
                return;

            StockData data = g.StockRepository.TryGetDataOrNull(stock); // or .TryGetDataOrNull(stock)
            if (data == null)
                return;

            string str = "";

            str = r3_display_lines_header(data);
            str += r3_display_lines_body(data);

            string temp_file = @"C:\병신\temp.txt";

            lock (g.lockObject)
            {
                if (File.Exists(temp_file))
                    File.Delete(temp_file);
            }

            File.WriteAllText(temp_file, str);
            Process.Start(temp_file);
        }

        public static string reacal_only(int[,] x, int 전일종가)
        {
            string str = "";

            for (int j = 0; j < x.Length / 12; j++) // bound exist not the size
            {
                if (j < 0)
                    continue;

                if (x[j, 0] == 0 || x[j, 0] > 152100) // if time is not set then stop writing
                    break;

                string str_add = "";

                for (int k = 0; k < 13; k++) // bound exist not the size
                {
                    // 분 디렉토리 내의 날짜별 종목별 일반 데이터 순서
                    // 0       1       2    3      4      5       6      7       8      9       10     11
                    // 시간 가격  수급  강도  프로  외인   기관  거량   양배  음배   수연  강연

                    // r3 후 컬럼 순서, 실제 4(외인누적매수량), 5(기관누적매수량) 컬럼은 잘 보지않음
                    // 0      1      2      3      4       5       6      7       8      9        10
                    // 시간 가격  수급  강도  외인   기관   양배  음배   수연  강연   거분/프외/%/누적프외
                    if (k == 4 || k == 5 || k == 6 || k == 7 || k == 9 || k == 11) // 프돈_천만원/분 / 프돈 %
                    {
                        continue;
                    }
                    if (k == 12)
                    {
                        double money_factor = 전일종가 / g.천만원; // 천만원
                        double dealt_money_per_minute = 0.0;
                        double program_money_per_minute = 0.0;
                        double foreign_money_per_minute = 0.0;
                        double percentage_program_per_minute = 0.0;

                        double accumulated_foreign_money = x[j, 5] * money_factor;
                        double accumulated_program_money = x[j, 4] * money_factor;
                        if (j - 1 >= 0)
                        {
                            program_money_per_minute = (x[j, 4] - x[j - 1, 4]) * money_factor;
                            foreign_money_per_minute = (x[j, 5] - x[j - 1, 5]) * money_factor;
                            dealt_money_per_minute = (x[j, 7] - x[j - 1, 7]) * money_factor;
                            if (MathUtils.IsSafeToDivide(dealt_money_per_minute))
                            {
                                //percentage_program_and_foreign_per_minute = (double)(program_money_per_minute + foreign_money_per_minute) /
                                //    dealt_money_per_minute * 100.0;
                                // 20230416 외돈 제외
                                percentage_program_per_minute = (double)(program_money_per_minute) /
                                    dealt_money_per_minute * 100.0;
                            }
                        }

                        //str_add = "     " + Math.Round(dealt_money_per_minute / 10.0, 1) + "  " +
                        //   Math.Round(program_money_per_minute / 10.0 + foreign_money_per_minute / 10.0, 1) + "  " +
                        //   (int)percentage_program_and_foreign_per_minute + "%" + "  " +
                        //   Math.Round(accumulated_program_money / 10.0 + accumulated_foreign_money / 10.0, 1);
                        // 20230416 외돈 제외
                        str_add = "     " + (int)dealt_money_per_minute + "  " +
                           (int)program_money_per_minute + "  " +
                           (int)percentage_program_per_minute + "%" + "  " +
                           (int)accumulated_program_money;

                        if (x.Length - j - 1 == g.v.q_advance_lines)
                            str_add += "   ***";
                    }
                    else if (k == 8)
                        str_add = String.Format("{0,12}", x[j, k] + " / " + x[j, k + 1]);
                    else if (k == 10)
                        str_add = String.Format("{0,12}", x[j, k] + " / " + x[j, k + 1]);
                    else
                    {
                        if (x.Length - j - 1 == g.v.q_advance_lines && k == 0)

                            str_add = "*" + String.Format("{0, 7}", x[j, k]); // FORMAT COLUMN CONTROL
                        else
                            str_add = String.Format("{0, 8}", x[j, k]);
                    }
                    str += str_add;
                }
                str += "\n";
            }
            return str;
        }

        public static string r3_display_lines_after_recalculation(StockData o)
        {
            string str = o.Stock + "   " + Math.Round(o.Statistics.일간변동편차, 1) + "  " + (o.Post.종거천 / 10).ToString("F0") + "\n\n";
            int end_row = 0;

            for (int i = 0; i < 5; i++)
            {
                if (i >= o.Misc.Friends.Count)
                    break;

                str += "  " + o.Misc.Friends[i] + "\n";
            }
            str += "\n";

            str += "  " + Math.Round(o.Statistics.푀분_avr * 10, 0) + "  " + Math.Round(o.Statistics.푀분_dev * 10, 0) + "    " +
                          Math.Round(o.Statistics.거분_avr * 10, 0) + "  " + Math.Round(o.Statistics.거분_dev * 10, 0) + "      푀분     거분\n" +
                   "  " + Math.Round(o.Statistics.배차_avr, 0) + "  " + Math.Round(o.Statistics.배차_dev, 0) + "    " +
                          Math.Round(o.Statistics.배합_avr, 0) + "  " + Math.Round(o.Statistics.배합_dev, 0) + "      배차     배합\n\n";

            if (g.test)
            {
                end_row = (o.Api.nrow < g.Npts[1]) ? o.Api.nrow : g.Npts[1];
            }
            else
            {
                end_row = o.Api.nrow;
            }

            int start_row = g.Npts[0];
            if (end_row - g.v.q_advance_lines - 10 > start_row)
                start_row = end_row - g.v.q_advance_lines - 10;

            for (int j = start_row; j < end_row; j++)
            {
                if (j < 0) continue;
                if (o.Api.x[j, 0] == 0 || o.Api.x[j, 0] > 152100) break;

                string str_add = "";

                for (int k = 0; k < 13; k++)
                {
                    if (k == 4 || k == 5 || k == 6 || k == 7 || k == 9 || k == 11)
                        continue;

                    if (k == 12)
                    {
                        double money_factor = o.Api.전일종가 / g.천만원;
                        double dealt = 0.0, prog = 0.0, foreign = 0.0, percentage = 0.0;

                        double acc_foreign = o.Api.x[j, 5] * money_factor;
                        double acc_program = o.Api.x[j, 4] * money_factor;

                        if (j - 1 >= 0)
                        {
                            prog = (o.Api.x[j, 4] - o.Api.x[j - 1, 4]) * money_factor;
                            foreign = (o.Api.x[j, 5] - o.Api.x[j - 1, 5]) * money_factor;
                            dealt = (o.Api.x[j, 7] - o.Api.x[j - 1, 7]) * money_factor;

                            if (MathUtils.IsSafeToDivide(dealt))
                            {
                                percentage = prog / dealt * 100.0;
                            }
                        }

                        str_add = "     " + (int)dealt + "  " +
                                  (int)prog + "  " +
                                  (int)percentage + "%" + "  " +
                                  (int)acc_program;

                        if (end_row - j - 1 == g.v.q_advance_lines)
                            str_add += "   ***";
                    }
                    else if (k == 8)
                    {
                        str_add = string.Format("{0,12}", o.Api.x[j, k] + " / " + o.Api.x[j, k + 1]);
                    }
                    else if (k == 10)
                    {
                        str_add = string.Format("{0,12}", o.Api.x[j, k] + " / " + o.Api.x[j, k + 1]);
                    }
                    else
                    {
                        if (end_row - j - 1 == g.v.q_advance_lines && k == 0)
                            str_add = "*" + string.Format("{0, 7}", o.Api.x[j, k]);
                        else
                            str_add = string.Format("{0, 8}", o.Api.x[j, k]);
                    }

                    str += str_add;
                }

                str += "\n";
            }

            return str;
        }

        // Sensei 20250316
        public static int ExtractIntFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0; // Return 0 for empty or null input

            long value = 0;
            bool isNegative = false;

            foreach (char c in input)
            {
                if (c == '-' && value == 0)
                {
                    isNegative = true; // Handle negative sign (only at start)
                }
                else if (c >= '0' && c <= '9')
                {
                    value = value * 10 + (c - '0');

                    // Check for int overflow
                    if (value > int.MaxValue)
                        return isNegative ? int.MinValue : int.MaxValue;
                }
            }

            return isNegative ? (int)-value : (int)value;
        }

        // Sensei 20250316
        public static void SwapValues<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        // Sensei 20250419
        public static string CycleStrings(string current, List<string> options)
        {
            if (options == null || options.Count == 0)
                return current; // or throw if you prefer strictness

            int index = options.IndexOf(current);
            return (index < 0 || index == options.Count - 1)
                ? options[0]
                : options[index + 1];
        }

        public static string message(string caption, string message, string button_selection)
        {
            DialogResult result;
            if (button_selection == "Yes")
                result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            else
                result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);


            if (result == System.Windows.Forms.DialogResult.No)
                return "No";
            else
                return "Yes";
        }

        public static void message(string message)
        {
            MessageBox.Show(message);
        }
    }
}
