
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using New_Tradegy.Library.Utils;

namespace New_Tradegy.Library
{
    public class mc
    {
        static CPUTILLib.CpStockCode _cpstockcode;

        /* static CPUTILLib.CpCybos _cpcybos;
		 static DSCBO1Lib.StockMst2 _stockmst2;
		 private static CPSYSDIBLib.CpSvrNew7222 _cpsvrnew7222;
		 private static DSCBO1Lib.CpFore8311 _cpfore8311;
		 private CPSYSDIBLib.K200Expect _k200expect;
		 private CPSYSDIBLib.K200Expect _k201expect;
		 CPTRADELib.CpTdUtil _CpTdUtil;
		 */

        





        public static string cycleStrings(string givenString, List<string> stringList)
        {
            string nextString;
            int index = stringList.IndexOf(givenString);
            if (index == stringList.Count - 1 || index < 0)
                return stringList[0];
            else
                return stringList[index + 1];

        }

        // not used
        public static int sender_to_chart_id(string name)
        {
            // for chart1, chart2, chart3
            string resultString = Regex.Match(name, @"\d+").Value;
            int chart_id = int.Parse(resultString);

            return chart_id;
        }





        public static void Sound_돈(int total_amount)
        {
            if (g.optimumTrading)
            {
                switch (total_amount)
                {
                    case 0:
                        mc.Sound("돈", "single stock opt");
                        break;
                    case 100:
                        mc.Sound("돈", "one hundred opt");
                        break;
                    case 500:
                        mc.Sound("돈", "five hundred opt");
                        break;
                    case 1000:
                        mc.Sound("돈", "one thousand opt");
                        break;
                    case 2000:
                        mc.Sound("돈", "two thousand opt");
                        break;
                    case 4000:
                        mc.Sound("돈", "four thousand opt");
                        break;
                    case 8000:
                        mc.Sound("돈", "eight thousand opt");
                        break;
                    case 16000:
                        mc.Sound("돈", "sixteen thousand opt");
                        break;
                    case 32000:
                        mc.Sound("돈", "thirty two thousand opt");
                        break;
                    case 64000:
                        mc.Sound("돈", "sixty four thousand opt");
                        break;
                    default:
                        mc.Sound("돈", "limit exceed opt");
                        break;
                }
            }
            else
            {
                switch (total_amount)
                {
                    case 0:
                        mc.Sound("돈", "single stock");
                        break;
                    case 100:
                        mc.Sound("돈", "one hundred");
                        break;
                    case 500:
                        mc.Sound("돈", "five hundred");
                        break;
                    case 1000:
                        mc.Sound("돈", "one thousand");
                        break;
                    case 2000:
                        mc.Sound("돈", "two thousand");
                        break;
                    case 4000:
                        mc.Sound("돈", "four thousand");
                        break;
                    case 8000:
                        mc.Sound("돈", "eight thousand");
                        break;
                    case 16000:
                        mc.Sound("돈", "sixteen thousand");
                        break;
                    case 32000:
                        mc.Sound("돈", "thirty two thousand");
                        break;
                    case 64000:
                        mc.Sound("돈", "sixty four thousand");
                        break;
                    default:
                        mc.Sound("돈", "limit exceed");
                        break;
                }
            }
        }

       
  

        public static void marketeye_record_변곡_write(g.stock_data o)
        {
            string file = @"C:\병신\변곡\" + g.date.ToString() + "\\" + o.stock + ".txt";

            if (!File.Exists(file))
                File.Create(file).Dispose();

            string str = "";
            for (int i = 0; i < o.변곡.array_count; i++) // record
            {
                str += o.변곡.틱의시간[i].ToString();
                str += "\t" + o.변곡.틱의가격[i].ToString();
                str += "\t" + o.변곡.틱프로천[i].ToString(); // 누적된 값

                str += "\n";
            }

            StreamWriter sw = File.AppendText(file);
            sw.WriteLine(str);
            sw.Close();

            o.변곡.array_count = 0;
        }


        // not used
        public static void marketeye_record_변곡(g.stock_data o)
        {
            if (MathUtils.IsSafeToDivide(o.통계.프분_dev))
            {
                if (Math.Abs(o.틱프로천[0]) > 0)
                {
                    if (o.변곡.틱프로잠정합_천 > 0) // if o.변곡.틱프로잠정합_천 == 0 ? 
                    {
                        if (o.틱프로천[0] < 0) // 변곡
                        {
                            o.변곡.틱의시간[o.변곡.array_count] = o.틱의시간[0];
                            o.변곡.틱의가격[o.변곡.array_count] = o.틱의가격[0];
                            o.변곡.틱프로천[o.변곡.array_count++] = o.변곡.틱프로잠정합_천; // 누적된 값
                            o.변곡.틱프로잠정합_천 = o.틱프로천[0];
                        }
                        else
                        {
                            o.변곡.틱프로잠정합_천 += o.틱프로천[0];
                        }
                    }
                    else
                    {
                        if (o.틱프로천[0] < 0)
                        {
                            o.변곡.틱프로잠정합_천 += o.틱프로천[0];
                        }
                        else  // 변곡
                        {
                            o.변곡.틱의시간[o.변곡.array_count] = o.틱의시간[0];
                            o.변곡.틱의가격[o.변곡.array_count] = o.틱의가격[0];
                            o.변곡.틱프로천[o.변곡.array_count++] = o.변곡.틱프로잠정합_천; // 누적된 값
                            o.변곡.틱프로잠정합_천 = o.틱프로천[0];
                        }
                    }
                    if (o.변곡.array_count == g.틱_array_size)
                    {
                        marketeye_record_변곡_write(o);
                    }
                }
                else
                    return;
            }
        }


        // not used
        public static void marketeye_record(g.stock_data o) // 20220720 o.틱외돈천[0] not included
        {
            string temp_file = @"C:\병신\temo.txt";

            lock (g.lockObject)
            {
                if (File.Exists(temp_file))
                    File.Delete(temp_file);
            }

            string str = sr.r3_display_lines_after_recalculation(o); // marketeye_record

            for (int i = g.틱_array_size - 1; i >= 0; i--) // record
            {
                str += o.틱의시간[i].ToString();
                str += "\t" + o.틱의가격[i].ToString();
                str += "\t" + o.틱의수급[i].ToString();
                str += "\t" + o.틱의체강[i].ToString();

                str += "\t" + o.틱매수배[i].ToString();
                str += "\t" + o.틱매도배[i].ToString();

                str += "\t\t" + Math.Round((o.틱거래천[i] / 10.0), 1);
                str += "\t" + Math.Round(((o.틱프로천[i] + o.틱외인천[i]) / 10.0), 1);
                if (o.틱거래천[i] > 0)
                    str += "\t" + (Math.Round((o.틱프로천[i] + o.틱외인천[i]) / (double)o.틱거래천[i] * 100.0), 1) + "%";
                else
                    str += "0";


                str += "\n";
            }





            File.WriteAllText(temp_file, str);
            Process.Start(temp_file);
        }


        //public static void Speech(string speech)
        //{
        //    var synthesizer = new SpeechSynthesizer();
        //    synthesizer.SetOutputToDefaultAudioDevice();
        //    synthesizer.Rate = -3;

        //    synthesizer.Speak(speech);
        //}

        // not used
        //public static void revovling_naver(int kospi_or_kosdaq)
        //{

        //    wk.시총순서(g.sl); // revolving_naver

        //    if (kospi_or_kosdaq == 1) // kospi
        //    {
        //        for (int i = g.revolving_number_for_kospi; i <= g.sl.Count; i++)
        //        {
        //            if (i == g.sl.Count)
        //                g.revolving_number_for_kospi = 0;

        //            if (rd.read_코스피코스닥시장구분(g.sl[i]) == 'S')
        //            {
        //                wk.call_네이버(g.sl[i], "main"); // 1 fchart, 2 main, 3 frgn
        //                g.revolving_number_for_kospi = i + 1;
        //                break;
        //            }
        //        }
        //    }
        //    else // kosdaq
        //    {
        //        for (int i = g.revolving_number_for_kosdaq; i <= g.sl.Count; i++)
        //        {
        //            if (i == g.sl.Count)
        //                if (i == g.sl.Count)
        //                    g.revolving_number_for_kosdaq = 0;

        //            if (rd.read_코스피코스닥시장구분(g.sl[i]) == 'D')
        //            {
        //                wk.call_네이버(g.sl[i], "main"); // 1 chart, 2 main, 3 frgn
        //                g.revolving_number_for_kosdaq = i + 1;
        //                break;
        //            }
        //        }
        //    }
        //}


        // 천만원 단위로 리턴


       



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

        public static async Task task_marketeye_alarm(int HHmm)
        {
            int[] alarm_HHmm = { 1000, 1030, 1450, 1455, 1500, 1505, 1510, 1515, 1520, 1525, 1528, 1529 };

            for (int i = 0; i < alarm_HHmm.Length; i++)
            {
                if (HHmm == alarm_HHmm[i] && HHmm != g.alamed_hhmm)
                {
                    g.alamed_hhmm = HHmm;
                    if (HHmm == 1000)
                    {
                        mc.Sound("time", "taiwan open");
                    }
                    else if (HHmm == 1030)
                        mc.Sound("time", "china open");
                    else if (HHmm == 1450)
                        mc.Sound("time", "30");
                    else if (HHmm == 1455)
                        mc.Sound("time", "25");
                    else if (HHmm == 1500)
                        mc.Sound("time", "20");
                    else if (HHmm == 1505)
                        mc.Sound("time", "15");
                    else if (HHmm == 1510)
                        mc.Sound("time", "10");
                    else if (HHmm == 1515)
                        mc.Sound("time", "5");
                    else if (HHmm == 1518)
                        mc.Sound("time", "2");
                    else if (HHmm == 1519)
                        mc.Sound("time", "1");
                }
            }
        }


        public static void Sound(string sub_directory, string sound)
        {
            if (sound == "")
                return;

            string sound_file;
            if (sub_directory == "")
                sound_file = @"C:\병신\data\소\" + sound + ".wav";
            else
                sound_file = @"C:\병신\data\소\" + sub_directory + "\\" + sound + ".wav";

            if (!File.Exists(sound_file))
            {
                return;
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = sound_file;

            player.Play();
        }


        public static void process_start(int date, string stock)
        {
            int index = wk.return_index_of_ogldata(stock);

            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }

            g.stock_data o = g.ogl_data[index];

            string temp_file = @"C:\병신\" + stock + ".txt";

            lock (g.lockObject)
            {
                if (File.Exists(temp_file))
                {
                    File.Delete(temp_file);
                }
            }

            Stream FS = new FileStream(temp_file, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            if (g.test)
            {
                int[,] x = new int[g.MAX_ROW, 12];
                int nrow = rd.read_Stock_Minute(date, stock, x);

                if (nrow < 382)
                {
                    return;
                }
                for (int i = g.Npts[0]; i < g.Npts[1]; i++)
                {
                    if (x[i, 0] == 0) // lines less than g.Npts[1]
                        break;
                    for (int j = 0; j < 12; j++)
                    {
                        sw.Write("{0}\t", x[i, j]);
                    }
                    sw.WriteLine();
                }
            }
            else
            {
                for (int i = 0; i < o.nrow; i++)
                {
                    if (i >= g.Npts[0] && i < g.Npts[1])
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            sw.Write("{0}\t", o.x[i, j]);
                        }
                        sw.WriteLine();
                    }
                }
            }
            sw.Close();
            Process.Start(temp_file);
        }


        // 지수 혼합 설정
        //public static void setting_코스피_코스닥합성()
        //{
        //    string file;
        //    file = @"C:\병신\data\지수_비중" + ".txt";

        //    if (!File.Exists(file))
        //        File.Create(file).Dispose();
        //    string str_add = "";

        //    List<string> copy = new List<string>(g.코스피합성);
        //    double total_value = 0;
        //    foreach (string t in copy)
        //    {
        //        string[] words = t.Split('\t');

        //        int index = wk.return_index_of_ogldata(words[0]);
        //        if (index < 0) continue;

        //        total_value += (int)g.ogl_data[index].전일종가 * int.Parse(words[1], NumberStyles.AllowThousands);
        //    }
        //    g.코스피합성.Clear();
        //    foreach (string t in copy)
        //    {
        //        string[] words = t.Split('\t');

        //        int index = wk.return_index_of_ogldata(words[0]);
        //        if (index < 0) continue;

        //        double individual_value = g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
        //        double factor = individual_value / total_value;
        //        g.코스피합성.Add(words[0] + "\t" + factor.ToString());

        //        string x = Math.Round((factor * 100), 1) + "%" + "\n";
        //        str_add += String.Format("{0, -20}  {1, 10}", words[0], x);
        //    }
        //    str_add += "\n";

        //    List<string> copy1 = new List<string>(g.코스닥합성);
        //    total_value = 0;
        //    foreach (string t in copy1)
        //    {
        //        string[] words = t.Split('\t');

        //        int index = wk.return_index_of_ogldata(words[0]);
        //        if (index < 0)
        //            continue;

        //        total_value += g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
        //    }
        //    g.코스닥합성.Clear();
        //    foreach (string t in copy1)
        //    {
        //        string[] words = t.Split('\t');

        //        int index = wk.return_index_of_ogldata(words[0]);
        //        if (index < 0)
        //            continue;

        //        double individual_value = g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
        //        double factor = individual_value / total_value;
        //        g.코스닥합성.Add(words[0] + "\t" + factor.ToString());

        //        string x = Math.Round((factor * 100), 1) + "%" + "\n";
        //        str_add += String.Format("{0, -20}  {1}", words[0], x);
        //    }

        //    File.WriteAllText(file, str_add);
        //}


        //public static void setting_kodex_magnifier_shifter()
        //{
        //    // KODEX 
        //    for (int i = 0; i < 4; i++)
        //    {
        //        for (int j = 0; j < 3; j++)
        //        {
        //            if (i == 0 || i == 2)
        //            {
        //                if (j == 2)
        //                    g.k.magnifier[i, j] = 1.0;
        //                else
        //                    g.k.magnifier[i, j] = 1.0;
        //            }

        //            else
        //                g.k.magnifier[i, j] = 1.0;
        //        }
        //    }
        //}





        //public static void read_or_set_stocks_Empty_Creation(string file)
        //{
        //    File.Create(file).Close();
        //    string minutestr = "85959\t0\t100\t10000\t0\t0\t0\t0\t0\t0\t0\t0"; // 12 items
        //    using (StreamWriter w = File.AppendText(file))
        //    {
        //        w.WriteLine("{0}", minutestr);
        //        w.Close(); 
        //}





        public static string six_digit_integer_time_to_string_time(int value)
        {
            int sec = value % 100;
            int min = value % 10000 / 100;
            int hour = value / 10000;
            return hour + ":" + min + ":" + sec;
        }


        public static int time_to_int(string value)
        {
            string[] words = value.Split(':');
            return Convert.ToInt32(words[0]) * 10000 +
                Convert.ToInt32(words[1]) * 100 +
                Convert.ToInt32(words[2]);
        }
        public static double total_Seconds(int from, int to)
        {
            string string_type_from = six_digit_integer_time_to_string_time(from);
            string string_type_to = six_digit_integer_time_to_string_time(to);
            double total_seconds = DateTime.Parse(string_type_to).Subtract(DateTime.Parse(string_type_from)).TotalSeconds;
            return total_seconds;
        }
        public static double total_Seconds(string from, string to)
        {
            return DateTime.Parse(to).Subtract(DateTime.Parse(from)).TotalSeconds;
        }


        //public static double t()
        //{
        //    return 0.1;
        //}


        //      public static void divide_그룹()
        //{
        //	_cpstockcode = new CPUTILLib.CpStockCode();

        //	int accumulated = 0;
        //	int ncodes = 0;
        //	bool first = true;

        //	for (int i = 0; i < g.sl.Count; i++)
        //	{
        //		string code = _cpstockcode.NameToCode(g.sl[i]);

        //		if (first)
        //		{
        //			g.codes[ncodes] += code;
        //			first = false;
        //		}
        //		else
        //		{
        //			g.codes[ncodes] += "," + code;
        //		}

        //		accumulated++;

        //		if (accumulated == g.stock_datasinCode)
        //		{
        //			accumulated = 0;
        //			ncodes++;
        //			first = true;
        //		}
        //	}
        //}

        //public static void divide_그룹_기()
        //{
        //	_cpstockcode = new CPUTILLib.CpStockCode();

        //	int accumulated = 0;
        //	int ncodes = 0;
        //	bool first = true;

        //	for (int i = 0; i < g.sl.Count; i++)
        //	{
        //		string code = _cpstockcode.NameToCode(g.sl[i]);

        //		if (first)
        //		{
        //			g.codes[ncodes] += code;
        //			first = false;
        //		}
        //		else
        //		{
        //			g.codes[ncodes] += "," + code;
        //		}

        //		accumulated++;

        //		if (accumulated == g.stock_datasinCode)
        //		{
        //			accumulated = 0;
        //			ncodes++;
        //			first = true;
        //		}
        //	}
        //}


        /*
        public void buy_종목 (string stock)
        {
        DateTime date = DateTime.Now; // Or whatever
        string temp = date.ToString("HHmm") + " " + stock + " :  buy";
        string path = @"C:\병신\매매.txt";
        StreamWriter sw = File.AppendText(path);
        sw.WriteLine("{0}", temp);
        sw.Close();
        }


        public void sell_종목(string stock)
        {
        DateTime date = DateTime.Now; // Or whatever
        string temp = date.ToString("HHmm") + " " + stock + " : sell";
        string path = @"C:\병신\매매.txt";
        StreamWriter sw = File.AppendText(path);
        sw.WriteLine("{0}", temp);
        sw.Close();
        }
        */

    }
}
