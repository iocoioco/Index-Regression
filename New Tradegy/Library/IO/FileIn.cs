using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;

using New_Tradegy.Library.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using New_Tradegy.Library.PostProcessing;
using System.Windows.Forms;


namespace New_Tradegy.Library.IO
{
    internal class FileIn
    {
        static CPUTILLib.CpCodeMgr _cpcodemgr;
        static CPUTILLib.CpStockCode _cpstockcode;



        public static List<string> txtFilesInGivenDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }
            var txtFiles = Directory.GetFiles(directory, "*.txt")
                     .Select(Path.GetFileName)
                     .ToList();

            return txtFiles;
        }

        public static void read_dl_stocks_only_for_given_date(List<string> dl)
        {
            string path = @"C:\병신\분\" + g.date.ToString();
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var stock in dl)
            {
                string file_path = path + "/" + stock + ".txt";
                if (!File.Exists(path))
                {
                    continue;
                }
            }
        }

        public static void read_all_stocks_for_given_date(List<string> gl)
        {
            g.sl.Clear();
            string path = @"C:\병신\분\" + g.date.ToString();
            if (!Directory.Exists(path))
            {
                return;
            }
            var sl = Directory.GetFiles(path, "*.txt")
                     .Select(Path.GetFileName)
                     .ToList();

            List<string> 제외종목 = new List<string>();
            제외종목 = read_제외();  // read_all_stocks_for_given_date


            g.StockManager.InterestedWithBidList.Clear();
            foreach (var stock in sl)
            {
                if (제외종목.Contains(stock))
                {
                    continue;
                }
                if (stock.Contains("processed"))
                {
                    continue;
                }
                string stock_without_txt = stock.Replace(".txt", "");

                gl.Add(stock_without_txt);
            }
        }

        public static void read_통계()
        {
            string[] grlines = File.ReadAllLines(@"C:\병신\data\통계.txt");
            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');
                if (words.Length != 14)
                    continue;

                var stockData = g.StockRepository.TryGetStockOrNull(words[0]);
                if (stockData == null || stockData.Api.전일종가 <= 0)
                    continue;

                int prevClose = (int)stockData.Api.전일종가;

                stockData.Pass.Month = (int)((Convert.ToInt32(words[1]) - prevClose) * 10000.0 / prevClose);
                stockData.Pass.Quarter = (int)((Convert.ToInt32(words[2]) - prevClose) * 10000.0 / prevClose);
                stockData.Pass.Half = (int)((Convert.ToInt32(words[3]) - prevClose) * 10000.0 / prevClose);
                stockData.Pass.Year = (int)((Convert.ToInt32(words[4]) - prevClose) * 10000.0 / prevClose);

                var stat = stockData.Statistics;

                stat.프분_count = string.IsNullOrEmpty(words[5]) ? 0 : Convert.ToInt32(words[5]);
                stat.프분_avr = string.IsNullOrEmpty(words[6]) ? 0.0 : Convert.ToDouble(words[6]);
                stat.프분_dev = string.IsNullOrEmpty(words[7]) ? 0.0 : Convert.ToDouble(words[7]);

                stat.거분_avr = string.IsNullOrEmpty(words[8]) ? 0.0 : Convert.ToDouble(words[8]);
                stat.거분_dev = string.IsNullOrEmpty(words[9]) ? 0.0 : Convert.ToDouble(words[9]);

                stat.배차_avr = string.IsNullOrEmpty(words[10]) ? 0.0 : Convert.ToDouble(words[10]);
                stat.배차_dev = string.IsNullOrEmpty(words[11]) ? 0.0 : Convert.ToDouble(words[11]);

                stat.배합_avr = string.IsNullOrEmpty(words[12]) ? 0.0 : Convert.ToDouble(words[12]);
                stat.배합_dev = string.IsNullOrEmpty(words[13]) ? 0.0 : Convert.ToDouble(words[13]);
            }
        }


        public static void read_시간별거래비율(List<List<string>> 누적)
        {
            string[] grlines = File.ReadAllLines(@"C:\병신\data\누적.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                foreach (string item in words)
                {
                    if (item == "") continue; // WN code check needed for misspelling

                    alist.Add(item);
                }
                누적.Add(alist);
            }
        }

        public static void read_누적(double[] 누적)
        {
            string[] grlines = File.ReadAllLines(@"C:\병신\data\누적.txt", Encoding.Default);
            int count = 0;
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                누적[count++] = Convert.ToDouble(words[1]);
            }
        }

        public static void read_or_set_stocks()
        {
            string path = @"C:\병신\분" + "\\" + g.date.ToString();
            Directory.CreateDirectory(path);

            if (g.StockManager.IndexList.Count == 0)
            {
                g.StockManager.IndexList.Add("KODEX 레버리지");
                g.StockManager.IndexList.Add("KODEX 200선물인버스2X");
                g.StockManager.IndexList.Add("KODEX 코스닥150레버리지");
                g.StockManager.IndexList.Add("KODEX 코스닥150선물인버스");
            }

            foreach (var t in g.StockRepository.AllDatas)
            {
                string file = Path.Combine(path, t.Stock + ".txt");

                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                    string line = "85959\t0\t100\t0\t0\t0\t0\t0\t0\t0\t0\t0";
                    using (var writer = File.AppendText(file))
                        writer.WriteLine(line);
                }

                var lines = File.ReadAllLines(file);
                if (lines.Length == 0)
                {
                    File.Delete(file);
                    File.Create(file).Close();
                    string line = g.StockManager.IndexList.Contains(t.Stock)
                        ? "85959\t0\t100\t0\t0\t0\t0\t0\t0\t0\t0\t0"
                        : "85959\t0\t100\t10000\t0\t0\t0\t0\t0\t0\t0\t0";

                    using (var writer = File.AppendText(file))
                        writer.WriteLine(line);

                    lines = File.ReadAllLines(file);
                }

                t.Api.nrow = 0;

                foreach (var line in lines)
                {
                    var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length < 12) continue;

                    int time = words[0].Contains(":")
                        ? Utils.TimeUtils.time_to_int(words[0])
                        : Convert.ToInt32(words[0]);

                    if (time >= 85959 && time < 152100)
                    {
                        t.Api.x[t.Api.nrow, 0] = time;
                        for (int j = 1; j < words.Length; j++)
                        {
                            t.Api.x[t.Api.nrow, j] = Convert.ToInt32(words[j]);
                        }
                        t.Api.nrow++;
                    }
                    else
                        break;
                }

                for (int j = t.Api.nrow; j < g.MAX_ROW; j++)
                    for (int m = 0; m < 12; m++)
                        t.Api.x[j, m] = 0;

                if (t.Api.nrow == 0) continue;

                int row = t.Api.nrow - 1;
                int[] last = new int[12];
                for (int k = 0; k < 12; k++)
                    last[k] = t.Api.x[row, k];

                int 거래량 = last[7];
                double intensity = last[3] / (double)g.HUNDRED;

                t.Api.당일프로그램순매수량 = last[4];
                t.Api.당일외인순매수량 = last[5];
                t.Api.당일기관순매수량 = last[6];

                t.Api.틱의시간[0] = last[0];
                t.Api.틱의가격[0] = last[1];
                t.Api.틱의수급[0] = last[2];
                t.Api.틱의체강[0] = last[3];

                if (intensity > 0)
                {
                    t.Api.틱매수량[0] = (int)(거래량 * intensity / (100.0 + intensity));
                    t.Api.틱매도량[0] = (int)(거래량 * 100.0 / (100.0 + intensity));
                }

                t.Api.틱매수배[0] = last[8];
                t.Api.틱매도배[0] = last[9];

                t.Post.종거천 = (int)(t.Api.전일종가 * 거래량 / g.천만원 * wk.누적거래액환산율(last[0]));

                if (!g.StockManager.IndexList.Contains(t.Stock) && !t.Stock.Contains("혼합") && t.Api.nrow >= 2)
                {
                    for (int j = 1; j < t.Api.nrow; j++)
                    {
                        var x = t.Api.x;

                        x[j, 10] = (x[j, 7] == x[j - 1, 7])
                            ? x[j - 1, 10]
                            : (x[j, 2] > x[j - 1, 2] ? x[j - 1, 10] + 1 : 0);

                        x[j, 11] = (x[j, 7] == x[j - 1, 7])
                            ? x[j - 1, 11]
                            : ((x[j, 3] / g.HUNDRED) > (x[j - 1, 3] / g.HUNDRED) ? x[j - 1, 11] + 1 : 0);
                    }
                }

                for (int j = 0; j < t.Api.nrow; j++)
                    PostProcessor.PostPassing(t, j, false);
            }

            var data = g.StockRepository.TryGetStockOrNull("KODEX 레버리지");
            if (data != null && data.Api.x[0, 3] > 1000)
            {
                PostProcessor.post_코스닥_코스피_프외_순매수_배차_합산_382();
            }
        }




        // 업종, 10억이상, 상관, 상관, 통계
        public static void gen_ogldata_oGLdata()
        {

            List<string> tgl_title = new List<string>();
            List<List<string>> tgl = new List<List<string>>();


            //if (!g.shortform)
            //{
            //    g.StockManager.AddIfMissing(FileIn.read_그룹_네이버_업종()); // replaces total_stock_list = ...
            //}



            var 시총Map = new ConcurrentDictionary<string, double>(
    File.ReadAllLines(@"C:\병신\data\시총.txt", Encoding.Default)
        .Select(line => line.Trim().Split(' '))
        .Where(parts => parts.Length >= 2)
        .GroupBy(parts => parts[0].Replace("_", " "))
        .ToDictionary(
            group => group.Key,
            group => double.TryParse(group.First()[1], out var v) ? v : -1
        )
);




            foreach (var stock in g.StockManager.TotalStockList) // StockManager.TotalStockList is used only in this method
            {
                wk.gen_ogl_data(stock, 시총Map);
            }

            FileIn.read_통계();

            FileIn.read_절친();

            var sortedList = g.StockRepository
                .AllDatas
                .OrderByDescending(x => x.Api.전일거래액_천만원)
                .ToList();

            g.sl.Clear(); // optional: clear before adding

            foreach (var stock in sortedList)
            {
                g.sl.Add(stock.Stock); // `Stock` is the name or code property
            }

            FileIn.read_파일관심종목(); // g.ogl_data에 없는 종목은 skip as g.StockManager.InterestedWithBidList

            GroupManager.gen_oGL_data(); // generate oGL_data

            FileIn.read_write_kodex_magnifier("read"); // duration 0.001 seconds
        }


        public static void read_파일관심종목()
        {
            string filename = @"C:\병신\data\관심.txt";

            g.StockManager.InterestedInFile.Clear();

            if (File.Exists(filename))
            {
                string[] grlines = File.ReadAllLines(filename, Encoding.Default);

                List<string> temp_list = new List<string>();
                foreach (string line in grlines)
                {
                    string[] words = line.Split(' '); // empty spaces also recognized as words, word.lenght can be larger than 4

                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == "//")
                            break;
                        if (words[i] == "")
                            continue;
                        string stock = words[i].Replace('_', ' ');
                        if (g.StockManager.TotalStockList.Contains(stock))
                            g.StockManager.InterestedInFile.Add(stock);
                    }
                }
            }
        }



        public static bool read_단기과열(string stock)
        {
            _cpcodemgr = new CPUTILLib.CpCodeMgr();
            _cpstockcode = new CPUTILLib.CpStockCode();
            int t = (int)_cpcodemgr.GetOverHeating(_cpstockcode.NameToCode(stock));

            if (t == 2 || t == 3)
                return true;
            else
                return false;
        }

        public static char read_코스피코스닥시장구분(string stock)
        {
            _cpcodemgr = new CPUTILLib.CpCodeMgr();
            _cpstockcode = new CPUTILLib.CpStockCode();
            int marketKind = (int)_cpcodemgr.GetStockMarketKind(_cpstockcode.NameToCode(stock));
            if (marketKind == 1)

            {
                return 'S';

            }
            else if (marketKind == 2)
            {
                return 'D';
            }
            else
                return 'N';
        }

        public static int read_데이터컬럼들(string filename, int[] c_id, string[,] x)
        {
            /* 파일이름, 구하고자하는 컬럼 번호를 주면 x[,] 저장 nrow 반환
        * public static int read_데이터컬럼들
         (string filename, int[] c_id, string[,] x)
        * */

            if (!File.Exists(filename)) return -1;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);

            int nrow = 0;
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                for (int k = 0; k < c_id.Length; k++)
                {
                    x[nrow, k] = words[c_id[k]];
                }
                nrow++;


            }
            return nrow;
        }

        public static void read_제어()
        {
            string 바탕화면 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filename = 바탕화면 + @"\제어.txt";


            if (!File.Exists(filename)) return;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);


            if (grlines[0] == "t")
                g.test = true;
            else
                g.test = false;

            if (grlines[1] == "s")
                g.shortform = true;
            else
                g.shortform = false;

            string[] strs = grlines[2].Split(' ');
            g.Account = strs[0];

            strs = grlines[3].Split(' ');
            g.date = Convert.ToInt32(strs[0]);
            //if (strs[1] == "w" || strs[1] == "W")
            //{
            //    g.workingday = true;
            //}

            //strs = grlines[3 + add].Split(' ');
            //g.deal_maximum_loss = Convert.ToInt32(strs[0]);
            //g.deal_finish_time = Convert.ToInt32(strs[1]);
            //g.deal_total_profit = Convert.ToInt32(strs[2]);

            //strs = grlines[4 + add].Split(' ');

            //g.전일종가이상 = Convert.ToInt32(strs[0]);
        }

        public static void read_변수()
        {
            string filename = @"C:\병신\data\변수.txt";

            if (!File.Exists(filename)) return;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);


            foreach (string line in grlines)
            {
                var words = line.Split('\t');

                switch (words[0])
                {
                    //case "textbox_date_char_to_string":
                    //    라인분리(line, ref g.v.textbox_date_char_to_string);
                    //    break;

                    //case "neglectable_price_differ":
                    //    라인분리(line, ref g.v.neglectable_price_differ);
                    //    break;

                    //case "files_to_open_by_clicking_edge":
                    //    라인분리(line, ref g.v.files_to_open_by_clicking_edge);
                    //    break;
                    case "q_advance_lines":
                        라인분리(line, ref g.v.q_advance_lines);
                        break;
                    case "Q_advance_lines":
                        라인분리(line, ref g.v.Q_advance_lines);
                        break;
                    case "r3_display_lines":
                        라인분리(line, ref g.v.r3_display_lines);
                        break;

                    // old version EvalKODEX()
                    //case "kospi_difference_for_sound":
                    //    라인분리(line, ref g.v.kospi_difference_for_sound);
                    //    break;
                    //case "kosdq_difference_for_sound":
                    //    라인분리(line, ref g.v.kosdq_difference_for_sound);
                    //    break;
                    // new version eval_index()
                    //case "index_difference_sound":
                    //    라인분리(line, ref g.v.index_difference_sound);
                    //    break;



                    //case "dev":
                    //    라인분리(line, ref g.s.dev);
                    //    break;
                    //case "mkc":
                    //    //라인분리(line, ref g.s.mkc);
                    //    break;

                    //case "돌파":
                    //    라인분리(line, ref g.s.돌파);
                    //    break;
                    //case "눌림":
                    //    라인분리(line, ref g.s.눌림);
                    //    break;

                    case "가연":
                        라인분리(line, ref g.s.가연);
                        break;
                    case "가분":
                        라인분리(line, ref g.s.가분);
                        break;
                    case "가틱":
                        라인분리(line, ref g.s.가틱);
                        break;
                    case "가반":
                        라인분리(line, ref g.s.가반);
                        break;
                    case "가지":
                        라인분리(line, ref g.s.가지);
                        break;

                    case "수연":
                        라인분리(line, ref g.s.수연);
                        break;
                    case "수지":
                        라인분리(line, ref g.s.수지);
                        break;
                    case "수위":
                        라인분리(line, ref g.s.수위);
                        break;

                    case "강연":
                        라인분리(line, ref g.s.강연);
                        break;
                    case "강지":
                        라인분리(line, ref g.s.강지);
                        break;
                    case "강위":
                        라인분리(line, ref g.s.강위);
                        break;


                    case "푀분":
                        라인분리(line, ref g.s.푀분);
                        break;
                    case "프틱":
                        라인분리(line, ref g.s.프틱);
                        break;
                    case "프지":
                        라인분리(line, ref g.s.프지);
                        break;
                    case "프퍼":
                        라인분리(line, ref g.s.프퍼);
                        break;
                    case "푀누":
                        라인분리(line, ref g.s.푀누);
                        break;


                    case "거분":
                        라인분리(line, ref g.s.거분);
                        break;
                    case "거틱":
                        라인분리(line, ref g.s.거틱);
                        break;
                    case "거일":
                        라인분리(line, ref g.s.거일);
                        break;

                    case "배차":
                        라인분리(line, ref g.s.배차);
                        break;
                    case "배반":
                        라인분리(line, ref g.s.배반);
                        break;
                    case "배합":
                        라인분리(line, ref g.s.배합);
                        break;


                    case "급락":
                        라인분리(line, ref g.s.급락);
                        break;
                    case "잔잔":
                        라인분리(line, ref g.s.잔잔);
                        break;

                    case "그룹":
                        라인분리(line, ref g.s.그룹);
                        break;

                    default:
                        break;
                }
            }
        }

        public static void read_무게()
        {
            string filePath = @"C:\병신\data\무게.txt";

            if (File.Exists(filePath))
            {
                string[] values = File.ReadAllLines(filePath);
                if (values.Length >= 7)
                {
                    // Convert values to double
                    g.s.푀분_wgt = Convert.ToDouble(values[0]);
                    g.s.거분_wgt = Convert.ToDouble(values[1]);
                    g.s.배차_wgt = Convert.ToDouble(values[2]);
                    g.s.배합_wgt = Convert.ToDouble(values[3]);
                    g.s.그룹_wgt = Convert.ToDouble(values[4]);
                    g.s.피로_wgt = Convert.ToDouble(values[5]); // New text box
                    g.s.기타_wgt = Convert.ToDouble(values[6]); // New text box
                }
            }
        }

        public static void ReadBestFriends()
        {
            string filePath = @"C:\병신\data\Correlation.txt";

            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath, Encoding.Default);
            StockData data = null;

            foreach (var line in lines)
            {
                var parts = line.Split('\t');

                if (parts.Length == 1)
                {
                    string stock = parts[0];
                    data = g.StockRepository.TryGetStockOrNull(stock);  // e.g., "삼성전자"

                    if (data == null)
                        continue;
                }
                else if (parts.Length == 2 && data != null)
                {
                    if (data.Misc.Friends.Count < 9)
                    {
                        data.Misc.Friends.Add(line);
                    }
                }
            }
        }

        public static void read_절친()
        {
            string filename = @"C:\병신\data\Correlation.txt";
            if (!File.Exists(filename)) return;

            string[] grlines = File.ReadAllLines(filename, Encoding.Default);

            StockData data = null;

            foreach (string line in grlines)
            {
                var words = line.Split('\t');

                if (words.Length == 1)
                {
                    data = g.StockRepository.TryGetStockOrNull(words[0]);
                }
                else if (words.Length == 2 && data != null)
                {
                    if (data.Misc.Friends.Count < 9)
                    {
                        data.Misc.Friends.Add(line);  // stores the full line like "0.72\t삼성전자"
                    }
                }
            }
        }


        public static void 라인분리(string line, ref int data) // scalar -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');
            Int32.TryParse(words[1], out data);
        }

        public static void 라인분리(string line, ref string[] data) // single vector -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');

            for (int i = 1; i < words.Length; i++)
            {
                data[i - 1] = words[i];
            }
        }

        public static void 라인분리(string line, ref int[] data) // single vector -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');

            for (int i = 1; i < words.Length; i++)
            {
                bool success = false;
                if (words[i].Contains("//"))
                    continue;

                success = Int32.TryParse(words[i], out data[i - 1]);
                if (!success)
                    return;
            }
        }

        public static void 라인분리(string line, ref List<List<double>> data) // double list, with 비중, dev. mkc
        {
            data.Clear();

            string[] words = line.Split('\t');

            List<double> t = new List<double>();
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Contains("//"))
                    break;

                string[] items = words[i].Split('/');

                if (items.Length == 1)
                {
                    double a;
                    bool success = false;
                    success = double.TryParse(items[0], out a);

                    if (success)
                        t = new List<double> { a, 0.0 };
                    else
                        continue;
                }

                if (items.Length == 2)
                {
                    double a, b;

                    bool success_1 = false;
                    success_1 = double.TryParse(items[0], out a);

                    bool success_2 = false;
                    success_2 = double.TryParse(items[1], out b);
                    if (!success_2)
                        break;

                    if (success_1 && success_2)
                        t = new List<double> { a, b };
                    else
                        break;
                }
                data.Add(t);
            }

            // Integrity Check
            if (line.Contains("dev") || line.Contains("mkc") || line.Contains("잔잔"))
            {
            }
            else
            {
                bool error_exist = false;
                for (int i = 2; i < data.Count - 1; i++)
                {
                    if (data[i + 1][0] < data[i][0])
                        error_exist = true;
                    if (data[i + 1][1] < data[i][1])
                        error_exist = true;
                }
                if (error_exist)
                    MessageBox.Show("Error in 변수 파일 : " + line);
            }

            // the following lines to check the integrity of data
            string path = @"C:\병신\temp_변수.txt";
            StreamWriter sw = File.AppendText(path);

            string str = line;
            if (data.Count > 0)
                str += "\t" + data[data.Count - 1][0].ToString() + "/" + data[data.Count - 1][1].ToString() +
                         "\t" + data.Count.ToString();
            sw.WriteLine("{0}", str);
            sw.Close();
        }

        public static int read_stock_minute(int date, string stock, int[,] x)
        {
            if (date < 10)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file)) return -1;
            string[] grlines = System.IO.File.ReadAllLines(file, Encoding.Default);

            int nrow = 0;
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                for (int k = 0; k < words.Length; k++)
                {
                    if (k == 4)
                    {
                        x[nrow, k] = Convert.ToInt32((int)Convert.ToDouble(words[k]));
                    }
                    else
                    {
                        x[nrow, k] = Convert.ToInt32(words[k]);
                    }
                }
                if (x[nrow, 0] < 10)
                    break;
                else
                    nrow++;
            }
            return nrow;
        }

        //public static double read_시총(string stock)
        //{
        //    string[] grlines = File.ReadAllLines(@"C:\병신\data\시총.txt", Encoding.Default);
        //    foreach (string line in grlines)
        //    {
        //        string[] words = line.Split(' ');
        //        string newname = words[0].Replace("_", " ");
        //        if (string.Equals(newname, stock))
        //        {
        //            return Convert.ToDouble(words[1]);
        //        }
        //    }
        //    return -1;
        //}

        public static List<string> read_제외()
        {
            List<string> gl_list = new List<string>();

            string filename = @"C:\병신\data\제외.txt"; ;

            string[] grlines = File.ReadAllLines(filename);

            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                foreach (string stock in words)
                {
                    string newname = stock.Replace("_", " ");
                    if (!wk.isStock(newname) || gl_list.Contains(newname))
                        continue;

                    gl_list.Add(newname); // for single
                }
            }
            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }


        public static void read_write_kodex_magnifier(string to_do)
        {
            string filename = @"C:\병신\data\kodex_magnifier.txt";

            if (to_do == "read")
            {
                if (!File.Exists(filename))
                {
                    MessageBox.Show("kodex_magnifier.txt Not Exist");
                    return;
                }

                string[] grlines = File.ReadAllLines(filename);
                List<string> list = new List<string>();

                int id = 0;
                foreach (string line in grlines)
                {
                    string[] words = line.Split('\t');
                    for (int i = 0; i < 4; i++)
                    {
                        g.kodex_magnifier[id, i] = Convert.ToDouble(words[i]);
                    }
                    id++;
                }
            }

            else
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                string str = "";
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (MathUtils.IsSafeToDivide(g.kodex_magnifier[i, j]))
                        {
                            str += g.kodex_magnifier[i, j];
                            if (j < 3)
                                str += "\t";
                            else
                            {
                                str += "\n";
                            }
                        }
                        else
                        {
                            g.kodex_magnifier[i, j] = 1.0;
                        }
                    }
                }
                File.WriteAllText(filename, str);
            }
        }

        public static void read_삼성_코스피_코스닥_전체종목()
        {
            string filename = @"C:\병신\data\삼성_코스피_코스닥_전체종목.txt";

            if (!File.Exists(filename))
            {
                MessageBox.Show("삼성_코스피_코스닥_전체종목.txt Not Exist");
                return;
            }

            string[] grlines = File.ReadAllLines(filename);

            int kospi_mixed_stock_count = 0;
            int kosdaq_mixed_stock_count = 0;
            bool empty_line_met = false;
            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');
                if (words[0] == "")
                    empty_line_met = true;

                double kospi_weight_sum = 0.0;
                double kosdaq_weight_sum = 0.0;
                if (words[0].Length > 0)
                {
                    if (empty_line_met == false)
                    {
                        //?
                        if (kospi_mixed_stock_count <= 20)
                        {
                            string stock = words[1];
                            g.kospi_mixed.stock.Add(stock);
                            string t = words[5].Trim(new Char[] { '%', '(', ')' });
                            double d = Convert.ToDouble(t);
                            g.kospi_mixed.weight.Add(d); // 22 items sum 38.21
                            kospi_weight_sum += d;
                            kospi_mixed_stock_count++;
                        }
                        
                    }
                    else
                    {
                        //?
                        if(kosdaq_mixed_stock_count <= 20)

                        {
                            string stock = words[1];
                            g.kosdaq_mixed.stock.Add(stock);
                            string t = words[5].Trim(new Char[] { '%', '(', ')' });
                            double d = Convert.ToDouble(t);
                            g.kosdaq_mixed.weight.Add(d); // 24 items sum 38.47
                            kosdaq_weight_sum += d;
                            kosdaq_mixed_stock_count++;
                        }
                        
                    }
                }
            }

            g.kospi_mixed.weight = AdjustToSumOne(g.kospi_mixed.weight);
            g.kosdaq_mixed.weight = AdjustToSumOne(g.kosdaq_mixed.weight);
            //g.평불종목.Add("코스피혼합");
            //g.평불종목.Add("코스닥혼합");

            //g.지수종목.Clear();
            //foreach (string stock in g.kospi_mixed.stock)
            //{
            //    g.지수종목.Add(stock);
            //}
            //foreach (string stock in g.kosdaq_mixed.stock)
            //{
            //    g.지수종목.Add(stock);
            //}

            //g.지수종목.Add("KODEX 레버리지"); // 0504
            //g.지수종목.Add("KODEX 200선물인버스2X");
            //g.지수종목.Add("KODEX 코스닥150레버리지");
            //g.지수종목.Add("KODEX 코스닥150선물인버스");
        }

        static List<double> AdjustToSumOne(List<double> numbers)
        {
            // Calculate the sum of the List
            double sum = 0;
            foreach (double num in numbers)
            {
                sum += num;
            }

            // Create a new List to hold adjusted values
            List<double> adjustedNumbers = new List<double>();

            // Adjust each number proportionally so the sum becomes 1.0
            foreach (double num in numbers)
            {
                adjustedNumbers.Add(num / sum);
            }

            // Return the adjusted List
            return adjustedNumbers;
        }



        public static void read_그룹_네이버_테마(List<string> tsl, List<string> tsl_그룹_상관, List<string> GL_title, List<List<string>> GL)
        {
            string filepath = @"C:\병신\data\그룹_네이버_테마.txt"; // QQQ
            if (!File.Exists(filepath))
                return;

            var lines = File.ReadAllLines(filepath, Encoding.Default);

            List<List<string>> temp_GL = new List<List<string>>(); // temporary working space for group list
            List<string> temp_Gl = new List<string>();

            foreach (var item in lines)
            {
                if (item == "" && temp_Gl.Count > 1)
                {
                    GL_title.Add(temp_Gl[0]);
                    temp_Gl.RemoveAt(0);
                    GL.Add(temp_Gl.ToList());

                    foreach (var stock in temp_Gl)
                    {
                        if (!tsl.Contains(stock))
                            tsl.Add(stock);
                    }
                    temp_Gl.Clear();
                }
                if (item != "" && !tsl_그룹_상관.Contains(item))
                    temp_Gl.Add(item);
            }
        }

        public static void read_상관(List<string> GL_title, List<List<string>> GL, List<string> stocks_over_deal_per_day)
        {
            string filename = @"C:\병신\data\상관.txt";

            if (!File.Exists(filename))
            {
                return;
            }
            string[] grlines = File.ReadAllLines(filename, Encoding.Default);

            List<string> temp_gl = new List<string>();
            List<string> temp_Gl = new List<string>();
            string temp_title = "";

            foreach (var line in grlines)
            {
                if (line == "") continue;
                string[] words = line.Split(' ', '\t', '\n', ',', '(');

                if (words[0] == ("//")) // read in title as temporary
                {
                    if (temp_Gl.Count >= 2)
                    {
                        if (!GL_title.Contains(temp_title))
                        {
                            if (g.shortform)
                            {
                                if (GL.Count == 4)
                                    break;
                            }
                            GL_title.Add(temp_title);
                            GL.Add(temp_Gl.ToList());
                        }
                    }
                    temp_Gl.Clear();
                    string title = "";
                    for (int i = 1; i < words.Length; i++)
                    {
                        title += words[i];
                    }
                    if (words.Length > 1)
                        temp_title = title;

                    continue;
                }
                else
                {
                    string new_name = "";
                    foreach (string stock in words)
                    {
                        if (stock == "//") // not a first word, then go to next line
                            break;

                        new_name = stock.Replace("_", " ");
                        if (!wk.isStock(new_name))
                            continue;

                        if (!stocks_over_deal_per_day.Contains(new_name)) // 상관에 있더라도 평균거래액 10억 이하는 제외
                            continue;

                        if (!temp_Gl.Contains(new_name))
                            temp_Gl.Add(new_name); // large group GL 

                    }
                }
            }

            if (temp_Gl.Count >= 2) // 그룹 상관, 2개 이상의 종목으로 구성된 그룹
            {
                GL_title.Add(temp_title);
                GL.Add(temp_Gl.ToList()); // 임시저장으로
            }
        }

        public static int read_Stock_Seven_Lines_Reverse(int date, string stock, int nline, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return -1;

            int n = File.ReadAllLines(file).Length;
            if (n < nline)
            {
                nline = n;
            }
            List<string> lines = File.ReadLines(file).Reverse().Take(nline).ToList();

            // Error Prone : Below 8 lines not Tested Exactly
            //if (g.test)
            //{
            //    if (n < g.Npts[1] + 1)
            //    {
            //        g.Npts[1] = n;
            //    }
            //    lines = File.ReadLines(file).Skip(g.Npts[1] - nline).Take(nline).Reverse().ToList();
            //}

            int nrow = 0;
            foreach (var line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[nrow, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                nrow++;
                if (nrow == nline) // can not exceeed the maximum allocation of x[nline,]
                {
                    break;
                }
            }
            return nrow;
        }

        // This is for eval. of stock for testing purpose
        public static int read_Stock_Seven_Lines_Reverse_From_Endtime
            (int date, string stock, int end_time, int nline, int[,] x)
        {
            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return -1;

            int n = File.ReadAllLines(file).Length;

            if (end_time > n)
            {
                end_time = n;
            }
            else
            {
                if (end_time < nline)
                {
                    nline = end_time;
                }
            }


            //end_time - 1 : - 1 because start from end_time to reverse
            // Skip, Reverse, Take does not work, Reverse is done by using Reverse.array();
            var lines = File.ReadLines(file).Skip(end_time - nline).Take(nline);

            n = nline - 1; // to reverse

            int nrow = 0;
            foreach (var line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[n, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[n, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[n, 1] = Convert.ToInt32(words[1]);   // price
                x[n, 2] = Convert.ToInt32(words[2]);   // amount
                x[n, 3] = Convert.ToInt32(words[3]);   // intensity

                x[n, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[n, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[n, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[n, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[n, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[n, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                n--;

                nrow++;
            }

            //x[nrow, 0] = 0; // used to check the end of data, no need actually
            //if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended
            //{
            //    nrow = 0;
            //}

            return nrow; // nrow = nline as the result from above
        }

        public static int read_Stock_Minute(int date, string stock, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
            {
                return 0;
            }



            string[] lines = File.ReadAllLines(file, Encoding.Default);
            if (lines.Length == 0)
            {
                return 0;
            }

            int nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[nrow, 0] = Convert.ToInt32(words[0]); // words[0] = time[0], no difference
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 10000 + Convert.ToInt32(time[1]) * 100 + Convert.ToInt32(time[2]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                if (words.Length == 12)
                {
                    x[nrow, 10] = Convert.ToInt32(words[10]);   // buy multiple 10 times
                    x[nrow, 11] = Convert.ToInt32(words[11]);   // sell multiple 10 times
                }
                nrow++;
                if (nrow == g.MAX_ROW)
                    break;
                //if (x[nrow - 1 , 0] > 152100) //0505 from nrow > 390 to current if
                //{
                //    x[nrow - 1, 0] = 0;
                //    nrow--;
                //    break;
                //}
            }
            for (int i = nrow; i < g.MAX_ROW; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    x[i, j] = 0;
                }
            }

            //x[nrow, 0] = 0; // used to check the end of data, no need actually
            //if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended, 아래 3 라인이 없으면 축 개체 문제 발생
            //{
            //    nrow = 0;
            //}

            if (g.dl.Count == 4) // 코스피혼합 & 코스닥혼합 인버스로 만들기 위해 가격 X -1 & 매수배수 매도배수 
            {
                if ((g.dl[0] == "KODEX 200선물인버스2X" && stock == "코스피혼합") ||
                   (g.dl[2] == "KODEX 코스닥150선물인버스" && stock == "코스닥혼합"))
                {
                    for (int i = 0; i < nrow; i++)
                    {
                        x[i, 1] *= -1;
                        StringUtils.SwapValues(ref x[i, 8], ref x[i, 9]);
                    }
                }
            }



            return nrow;
        }

        public static int read_Stock_Minute_no_multiply(int date, string stock, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return 0;


            string[] lines = File.ReadAllLines(file, Encoding.Default);

            int nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {

                    x[nrow, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                if (words.Length == 12)
                {
                    x[nrow, 10] = Convert.ToInt32(words[10]);   // buy multiple 10 times
                    x[nrow, 11] = Convert.ToInt32(words[11]);   // sell multiple 10 times
                }
                nrow++;
                if (x[nrow - 1, 0] > 1420) //0505 from nrow > 390 to current if
                {
                    x[nrow - 1, 0] = 0;
                    nrow--;
                    break;
                }
            }

            x[nrow, 0] = 0; // used to check the end of data, no need actually
            if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended
            {
                nrow = 0;
            }
            return nrow;
        }

        public static int read_일주월(string stock, string dwm, int nrow, int[] col, int[,] x)
        {
            string file = "";
            switch (dwm)
            {
                case "일":
                    file = @"C:\병신\data\일\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                case "주":
                    file = @"C:\병신\data\주\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                case "월":
                    file = @"C:\병신\data\월\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                default:
                    break;
            }
            if (!File.Exists(file))
                return 0;

            List<string> lines = File.ReadLines(file).Reverse().Take(nrow).ToList();

            nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                for (int i = 0; i < col.Length; i++)
                {
                    x[nrow, i] = Convert.ToInt32(words[col[i]]);

                }
                nrow++;
            }
            return nrow;
        }

        public static int read_전일종가_전일거래액_천만원(string stock)
        {
            string path = @"C:\병신\data\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }

            string lastline = File.ReadLines(path).Last(); // last line read 

            string[] words = lastline.Split(' ');
            int 전일종가 = Convert.ToInt32(words[4]);
            ulong 전일거래량 = Convert.ToUInt32(words[5]);
            return (int)(전일종가 * (전일거래량 / g.천만원));
        }

        public static int read_전일종가_GivenDate(string stock, int date)
        {

            string path = @"C:\병신\data\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }
            string[] grlines = File.ReadAllLines(path);
            int FoundLine = 0;
            foreach (string line in grlines)
            {
                string[] wordss = line.Split(' ');
                if (Convert.ToInt32(wordss[0]) == date)
                    break;
                else
                    FoundLine++;
            }
            string aline = File.ReadLines(path).Skip(FoundLine - 1).Take(1).First();

            string[] words = aline.Split(' ');
            return Convert.ToInt32(words[4]);
        }

        public static int read_전일종가(string stock)
        {

            string path = @"C:\병신\data\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }

            string lastline = File.ReadLines(path).Last(); // last line read 

            string[] words = lastline.Split(' ');
            return Convert.ToInt32(words[4]);
        }


        public static List<string> read_그룹_네이버_업종_old(List<List<string>> Gl, List<List<string>> GL)
        {
            _cpstockcode = new CPUTILLib.CpStockCode();

            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }

            List<string> 제외 = new List<string>();
            제외 = read_제외(); // read_그룹_네이버_업종

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            List<string> GL_list = new List<string>();

            int count = 0;
            foreach (string line in grlines)
            {
                List<string> Gl_list = new List<string>();

                string[] words = line.Split('\t');

                if (words[0] != "")
                {
                    string stock = words[0].Replace(" *", "");
                    string code = _cpstockcode.NameToCode(stock);
                    if (code.Length != 7)
                    {
                        continue;
                    }
                    if (code[0] != 'A')
                        continue;

                    char marketKind = read_코스피코스닥시장구분(stock);
                    if (marketKind == 'S' || marketKind == 'D')
                    { }
                    else
                        continue;

                    if (제외.Contains(stock))
                        continue;

                    gl_list.Add(stock); // for single
                    GL_list.Add(stock); // for small group
                    if (count == grlines.Length - 1) // the last stock added as group
                    {
                        GL.Add(GL_list.ToList()); //modified to create a new List when adding
                        GL_list.Clear();
                    }
                }
                else
                {
                    GL.Add(GL_list.ToList()); // to create a new List when adding
                    GL_list.Clear();
                }
                count++;


            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }


        public static List<string> read_그룹_네이버_업종() // this is for single list of stocks in 그룹_네이버_업종
        {
            _cpstockcode = new CPUTILLib.CpStockCode();

            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }

            List<string> 제외 = new List<string>();
            제외 = read_제외(); // read_그룹_네이버_업종

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            List<string> GL_list = new List<string>();


            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');

                if (words[0] != "")
                {
                    string stock = words[0].Replace(" *", "");
                    string code = _cpstockcode.NameToCode(stock);
                    if (code.Length != 7)
                    {
                        continue;
                    }
                    if (code[0] != 'A')
                        continue;

                    char marketKind = read_코스피코스닥시장구분(stock);
                    if (marketKind == 'S' || marketKind == 'D')
                    { }
                    else
                        continue;

                    if (제외.Contains(stock))
                        continue;

                    gl_list.Add(stock); // for single
                }
            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }
    }
}
