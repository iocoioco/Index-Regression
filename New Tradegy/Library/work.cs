using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Concurrent;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.IO;
using New_Tradegy.Library.UI.KeyBindings;

namespace New_Tradegy.Library
{
    internal class wk
    {
        static CPUTILLib.CpStockCode _cpstockcode;

        public static void deleteChartAreaAnnotation(Chart chartName, string stockName)
        {
            // Check if the ChartArea exists
            if (chartName.ChartAreas.IndexOf(stockName) >= 0)
            {
                // Get the ChartArea
                var chartArea = chartName.ChartAreas[stockName];


                var annotationsToRemove = chartName.Annotations.Where(a => a.Name == stockName).ToList();

                foreach (var annotation in annotationsToRemove)
                {
                    chartName.Annotations.Remove(annotation); // Remove the annotation from the chart
                }

                // Remove all series associated with this ChartArea
                var seriesToRemove = chartName.Series
                    .Where(s => s.ChartArea == stockName)
                    .ToList();

                foreach (var series in seriesToRemove)
                {
                    chartName.Series.Remove(series);
                    //Console.WriteLine($"Removed series: {series.Name}");
                }
                chartName.ChartAreas.Remove(chartArea);
            }

            else
            {
                //Console.WriteLine($"ChartArea with name {stockName} does not exist.");
            }


        }

        public static void date_backwards_forwards(string backwards_or_forwards)
        {
            if (backwards_or_forwards == "backwards")
            {
                int return_date = wk.directory_분전후(g.date, -1); // 거래전일
                if (return_date == -1)
                {
                    return;
                }
                else
                {
                    g.date = return_date;
                }

                Utils.SoundUtils.Sound("time", "date backwards");
            }

            else
            {
                int return_date = wk.directory_분전후(g.date, 1); // 거래익일
                if (return_date == -1)
                {
                    return;
                }
                else
                {
                    g.date = return_date;
                }

                Utils.SoundUtils.Sound("time", "date forwards");
            }
            FileIn.read_or_set_stocks(); // date forward with stocks in the list of g.ogl_data
            
            // MOD info date modification
            int month_1 = g.date % 10000 / 100;
            int day_1 = g.date % 10000 % 100;
            g.제어.dtb.Rows[0][0] = month_1.ToString() + "/" + day_1.ToString();
            RankLogic.EvalStock(); // date backwards forwards
            ActionCode.New(true, false, eval: true, draw: 'B').Run();
        }

        public static bool isWorkingHour()
        {
            DateTime now = DateTime.Now;

            // ⏰ Market date check
            int currentDate = Convert.ToInt32(now.ToString("yyyyMMdd"));
            if (g.date != currentDate)
                return false;

            // 📆 Skip weekends
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // 🕘 Market hours (adjust as needed)
            int HHmm = now.Hour * 100 + now.Minute;
            if (HHmm < 800 || HHmm > 1530)
                return false;

            return true;
        }


        public static bool isStock(string stock)
        {
            _cpstockcode = new CPUTILLib.CpStockCode();
            if (stock == "")
                return false;


            string code = _cpstockcode.NameToCode(stock); // 코스피혼합, 코스닥혼합 code.Length = 0 제외될 것임
            if (code.Length == 7)
                return true;
            else
                return false;
        }

       

        public static bool gen_ogl_data(string stock, ConcurrentDictionary<string, double> map)
        {
            if (FileIn.read_단기과열(stock))
                return false;

            int days = 20;
            if (!종목일중변동자료계산(stock, days, out double 일간변동평균, out double 일간변동편차,
                out int 일평균거래액, out int 일최저거래액, out int 일최대거래액, out ulong 일평균거래량, out string 일간변동평균편차) ||
                일최대거래액 < 30 || string.IsNullOrEmpty(일간변동평균편차))
            {
                return false;
            }
            if (일최대거래액 < 30) // 일최대거래액 30억 이하 제외
                return false;

            var cpStockCode = new CPUTILLib.CpStockCode();
            string code = cpStockCode.NameToCode(stock);
            long 전일종가 = FileIn.read_전일종가(stock);
            if (code.Length != 7 || 전일종가 < 1000)
                return false;

            if (!map.TryGetValue(stock, out var 시총값))
                return false;

            double 전일거래액_천만원 = FileIn.read_전일종가_전일거래액_천만원(stock);
            if (전일거래액_천만원 == -1)
                return false;

            char 시장구분 = FileIn.read_코스피코스닥시장구분(stock);
            if (시장구분 != 'S' && 시장구분 != 'D')
                return false;

            var data = new StockData
            {
                Stock = stock
            };

            // 🧮 Set values into Statistics
            data.Statistics.일간변동평균 = 일간변동평균;
            data.Statistics.일간변동편차 = 일간변동편차;
            data.Statistics.일평균거래액 = 일평균거래액;
            data.Statistics.일최저거래액 = 일최저거래액;
            data.Statistics.일최대거래액 = 일최대거래액;
            data.Statistics.일평균거래량 = 일평균거래량;
            data.Statistics.일간변동평균편차 = 일간변동평균편차;
            data.Statistics.시장구분 = 시장구분;
            data.Statistics.시총 = 시총값 / 100.0;

            // 📊 API section (전일종가 + calculated 전일거래액_천만원)
            data.Api.전일종가 = 전일종가;
            data.Api.전일거래액_천만원 = 전일거래액_천만원;

            // 📌 Set default Score
            data.Score.그순 = 1000;

            // 🗃️ Save to repository
            g.StockRepository.AddOrUpdate(stock, data);

            return true;
        }

        
        public static void 일평균거래액일정액이상종목선택(List<string> tsl, int 최소거래액이상_억원)
        {
            var tuple = new List<Tuple<ulong, string>> { };

            int days = 20;
            foreach (var stock in tsl)
            {
                string path = @"C:\병신\data\일\" + stock + ".txt";
                if (!File.Exists(path))
                    continue;

                List<string> lines = File.ReadLines(path).Reverse().Take(days).ToList(); // 파일 후반 읽기

                #region
                // 20일간 평균 일 거래액 구하기
                //ulong sum_day_dealt_money = 0;
                //foreach (var line in lines)
                //{
                //    string[] words = line.Split(' ');
                //    ulong day_dealt_money = (ulong)(Convert.ToDouble(words[4]) * Convert.ToUInt64(words[5]) / g.억원); // 종가 * 당일거래량
                //    sum_day_dealt_money += day_dealt_money;
                //}
                //ulong avr_day_dealt_money = sum_day_dealt_money / (ulong)days;
                //tuple.Add(Tuple.Create(avr_day_dealt_money, stock));
                #endregion

                // 최대 일 거래액 구하기
                ulong max_day_dealt_money = 0;
                foreach (var line in lines)
                {
                    string[] words = line.Split(' ');
                    ulong day_dealt_money = (ulong)(Convert.ToDouble(words[4]) * Convert.ToUInt64(words[5]) / g.억원); // 종가 * 당일거래량
                    if (day_dealt_money > max_day_dealt_money)
                        max_day_dealt_money = day_dealt_money;
                }
                tuple.Add(Tuple.Create(max_day_dealt_money, stock));
            }
            tuple = tuple.OrderByDescending(t => t.Item1).ToList();


            // max_day_dealt_money가 최소거래액_이상인 종목만 선택
            // 10억원 이상인 경우 1710개
            // 20억원 이상인 경우  1387개
            // 30억원 이상인 경우  1209개
            // 40억원 이상인 경우  1088개
            // 50억원 이상인 경우  992개
            // 60억원 이상인 경우  918개
            // 70억원 이상인 경우  865개
            // 80억원 이상인 경우  809개
            // 90억원 이상인 경우  773개
            // 100억원 이상인 경우  743개
            tsl.Clear();

            foreach (var item in tuple)
            {
                if (item.Item1 < (ulong)최소거래액이상_억원)
                    continue;
                tsl.Add(item.Item2);
            }
        }

        public static void 이십일중일최대거래액일정액이상종목선택(List<string> tsl, int 일거래액기준_억원)
        {
            var tuple = new List<Tuple<ulong, string>> { };

            int days = 20;
            foreach (var stock in tsl)
            {
                string path = @"C:\병신\data\일\" + stock + ".txt";
                if (!File.Exists(path))
                    continue;

                List<string> lines = File.ReadLines(path).Reverse().Take(days).ToList(); // 파일 후반 읽기


                // 지난 20일 중 최대 일 거래액 구하기
                ulong max_day_dealt_money = 0;
                foreach (var line in lines)
                {
                    string[] words = line.Split(' ');
                    ulong day_dealt_money = (ulong)(Convert.ToDouble(words[4]) * Convert.ToUInt64(words[5]) / g.억원); // 종가 * 당일거래량
                    if (day_dealt_money > max_day_dealt_money)
                        max_day_dealt_money = day_dealt_money;
                }
                tuple.Add(Tuple.Create(max_day_dealt_money, stock));
            }
            tuple = tuple.OrderByDescending(t => t.Item1).ToList();


            // max_day_dealt_money가 최소거래액_이상인 종목만 선택
            // 10억원 이상인 경우 1710개
            // 20억원 이상인 경우  1387개
            // 30억원 이상인 경우  1209개
            // 40억원 이상인 경우  1088개
            // 50억원 이상인 경우  992개
            // 60억원 이상인 경우  918개
            // 70억원 이상인 경우  865개
            // 80억원 이상인 경우  809개
            // 90억원 이상인 경우  773개
            // 100억원 이상인 경우  743개
            tsl.Clear();

            foreach (var item in tuple)
            {
                if (item.Item1 < (ulong)일거래액기준_억원)
                    continue;
                tsl.Add(item.Item2);
            }
        }





        public static void 거분순서(List<string> stocks)
        {
            stocks.Sort((a, b) =>
            {
                var sa = g.StockRepository.TryGetStockOrNull(a);
                var sb = g.StockRepository.TryGetStockOrNull(b);

                if (sa == null && sb == null) return 0;
                if (sa == null) return 1;
                if (sb == null) return -1;

                double va = sa.Api.분거래천?[0] ?? double.MinValue;
                double vb = sb.Api.분거래천?[0] ?? double.MinValue;

                return vb.CompareTo(va); // descending order
            });
        }







        public static double ComputeCoeff(string stockname, double[] values1, double[] values2)
        {
            if (values1.Length != values2.Length)
            {
                throw new ArgumentException("values must be the same length");
            }


            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }

        //     public static int read_외인기관일별매수동향(string stockname, g.stock_data data)
        //     {
        //         /* 양매수신뢰도 // 상관
        // 양매수수 // 전일 1, 2 전전일 3,4, 등
        // 외량 // 16거래일 외인거래량 
        // 기량 // 16거래일 기관거래량
        // 개량 // 16거래일 개인거래량
        //*/

        //         string path = @"C:\병신\data\매\" + stockname + ".txt";
        //         // 16일 데이터만 있는 데 신규는 적을 수 있음
        //         if (!File.Exists(path))
        //         {
        //             //string t = "매 file not exist " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }

        //         string[] grlines = System.IO.File.ReadAllLines(path, Encoding.Default);

        //         int count_매 = 0;
        //         int last_day_매 = 0;
        //         foreach (var line in grlines)
        //         {
        //             string[] words = line.Split(' ');
        //             if (count_매 == 0)
        //                 last_day_매 = Convert.ToInt32(words[0]);

        //             data.개량[count_매] = Convert.ToInt32(words[1]);
        //             data.외량[count_매] = Convert.ToInt32(words[2]);
        //             data.기량[count_매] = Convert.ToInt32(words[3]);
        //             count_매++;
        //         }

        //         path = @"C:\병신\일\" + stockname + ".txt";
        //         if (!File.Exists(path))
        //         {
        //             //string t = "일 file not exist " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }


        //         if (count_매 < 16)
        //         {
        //             count_매--;
        //         }
        //         List<string> lines = File.ReadLines(path).Reverse().Take(count_매 + 1).ToList();
        //         double[] pricediffer = new double[count_매];
        //         double[] instandfrig = new double[count_매];
        //         double[] closeprice = new double[count_매 + 1];
        //         double[] day_amount = new double[count_매 + 1];

        //         int count_일 = 0;
        //         int last_day_일 = 0;
        //         foreach (var line in lines)
        //         {
        //             string[] words = line.Split(' ');
        //             if (count_일 == 0)
        //                 last_day_일 = Convert.ToInt32(words[0]);
        //             closeprice[count_일] = Convert.ToDouble(words[4]);
        //             day_amount[count_일] = Convert.ToDouble(words[5]);
        //             count_일++;
        //         }
        //         if (last_day_일 != last_day_매)
        //         {
        //             //string t = "일 매 파일들의 마지막 날짜 불일치 " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }

        //         for (int i = 0; i < count_매; i++)
        //         {
        //             if (day_amount[i] == 0)
        //             {
        //                 continue;
        //             }
        //             pricediffer[i] = closeprice[i] - closeprice[i + 1];
        //             instandfrig[i] = data.개량[i] * -1.0 / day_amount[i];
        //             // 당일 개인 매도량 나누기 당일거래량
        //         }

        //         data.양매수신뢰도 = ComputeCoeff(stockname, pricediffer, instandfrig);


        //         if (data.외량[0] > 0 || data.기량[0] > 0)
        //             data.양매수수 = 1;
        //         if (data.외량[0] > 0 && data.기량[0] > 0)
        //             data.양매수수 = 2;
        //         if (data.외량[0] > 0 && data.기량[0] > 0 && (data.외량[1] > 0 || data.기량[1] > 0))
        //             data.양매수수 = 3;
        //         if (data.외량[0] > 0 && data.기량[0] > 0 && data.외량[1] > 0 && data.기량[1] > 0)
        //             data.양매수수 = 4;

        //         return 0;
        //     }
        public static bool 종목일중변동자료계산(string stock, int days, out double avr, out double dev,
                     out int avr_dealt, out int min_dealt, out int max_dealt, out ulong 일평균거래량, out string 일간변동평균편차)
        {
            avr = 0;
            dev = 0;
            avr_dealt = 0;
            max_dealt = 0;
            min_dealt = int.MaxValue;
            일평균거래량 = 0;
            일간변동평균편차 = "";

            string path = $@"C:\병신\data\일\{stock}.txt";
            if (!File.Exists(path)) return false;

            var lines = File.ReadLines(path).Reverse().Take(days).ToList();
            if (lines.Count < 1) return false; // 신규 상장 (less than `days` worth of data)

            var day_list = new List<double>();
            int total_dealt = 0, days_count = 0;

            foreach (var line in lines)
            {
                var words = line.Split(' ');
                if (words.Length != 10) return false; // Invalid data format

                if (!double.TryParse(words[1], out double start_price) ||  // 시가
                    !double.TryParse(words[4], out double close_price) ||  // 종가
                    !ulong.TryParse(words[5], out ulong 거래량))           // 거래량
                    return false;

                int day_dealt = (int)((close_price * 거래량) / g.억원);
                total_dealt += day_dealt;
                max_dealt = Math.Max(max_dealt, day_dealt);
                min_dealt = Math.Min(min_dealt, day_dealt);
                일평균거래량 += 거래량;

                if (start_price != 0)
                    day_list.Add((close_price - start_price) / start_price * 100); // 변동률 (%)

                days_count++;
            }

            if (days_count == 0) return false;

            avr_dealt = total_dealt / days_count;
            일평균거래량 /= (ulong)days_count;

            // Compute avr before using it in any lambda expression
            if (day_list.Count > 0)
            {
                double sum = 0;
                foreach (var val in day_list) sum += val;
                avr = sum / day_list.Count;  // Compute manually to avoid lambda usage
            }

            // Compute standard deviation safely without lambda
            if (day_list.Count > 1)
            {
                double varianceSum = 0;
                foreach (var val in day_list)
                    varianceSum += Math.Pow(val - avr, 2);

                dev = Math.Sqrt(varianceSum / (day_list.Count - 1));
            }
            else
            {
                dev = 0;
            }

            일간변동평균편차 = $"{avr:0.#}/{dev:0.#}";
            return true;
        }

        //public static bool 종목일중변동자료계산_old(string stock, int days, out double avr, out double dev,
        //                    out int avr_dealt, out int min_dealt, out int max_dealt, out ulong 일평균거래량, out string 일간변동평균편차)
        //{
        //    avr = 0;
        //    dev = 0;
        //    avr_dealt = 0;
        //    max_dealt = 0;
        //    min_dealt = int.MaxValue;
        //    일평균거래량 = 0;
        //    일간변동평균편차 = "";

        //    string path = $@"C:\병신\data\일\{stock}.txt";
        //    if (!File.Exists(path)) return false;

        //    var lines = File.ReadLines(path).Reverse().Take(days).ToList();
        //    if (lines.Count < 1) return false; // 신규 상장 (less than 20 days of data)

        //    var day_list = new List<double>();
        //    int total_dealt = 0, days_count = 0;

        //    foreach (var line in lines)
        //    {
        //        var words = line.Split(' ');
        //        if (words.Length != 10) return false; // Invalid data format

        //        if (!double.TryParse(words[1], out double start_price) ||  // 시가
        //            !double.TryParse(words[4], out double close_price) ||  // 종가
        //            !ulong.TryParse(words[5], out ulong 거래량))           // 거래량
        //            return false;

        //        int day_dealt = (int)((close_price * 거래량) / g.억원);
        //        total_dealt += day_dealt;
        //        max_dealt = Math.Max(max_dealt, day_dealt);
        //        min_dealt = Math.Min(min_dealt, day_dealt);
        //        일평균거래량 += 거래량;

        //        if (start_price != 0)
        //            day_list.Add((close_price - start_price) / start_price * 100); // 변동률 (%)

        //        days_count++;
        //    }

        //    if (days_count == 0) return false;

        //    avr_dealt = total_dealt / days_count;
        //    일평균거래량 /= (ulong)days_count;

        //    // Calculate standard deviation
        //    avr = day_list.Average();
        //    dev = (day_list.Count > 1) ? Math.Sqrt(day_list.Average(x => Math.Pow(x - avr, 2))) : 0;

        //    일간변동평균편차 = $"{avr:0.#}/{dev:0.#}";
        //    return true;
        //}


        // avr_dealt, min_dealt, max_dealt : not used, just for reference
        public static string calcurate_종목일중변동평균편차_old(string stock, int days, ref double avr, ref double dev,
                                    ref int avr_dealt, ref int min_dealt, ref int max_dealt, ref ulong 일평균거래량)
        {
            // code simplification
            // IEnumerable<double> values as input
            //double avg = values.Average();
            //return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));


            string path = @"C:\병신\data\일\" + stock + ".txt";
            if (!File.Exists(path))
                return "";

            List<string> lines = File.ReadLines(path).Reverse().Take(days).ToList(); // 파일 후반 읽기

            if (lines.Count < 1) // 신규상장의 경우 데이터 숫자 20 보다 적음
                return "";

            List<Double> day_list = new List<Double>();
            List<Double> long_day_list = new List<Double>();

      

            avr_dealt = 0;
            max_dealt = 0;
            min_dealt = 1000000; // 단위 억원
            일평균거래량 = 0;

            int days_count = 0;
            // 20일 일평균거래량, avr, dev, 
            for (int i = 0; i < 20; i++)
            {
                if (i == lines.Count) // 신규 등 lines 숫자 20개 보다 작은 경우
                    break;

                string[] words = lines[i].Split(' ');
                if (words.Length != 10)
                    return "";

                // 거래정지 당일 해제 종목도 아래 해당,
                // 실제는 어제 거래정지 종목 또는 이전에 ... 
                //if(Convert.ToDouble(words[5]) == 0 && 
                //  Convert.ToDouble(words[6]) == 0 &&
                //  Convert.ToDouble(words[7]) == 0)
                //    return "";

                double start_price = Convert.ToDouble(words[1]); // 시가
                double close_price = Convert.ToDouble(words[4]); // 종가
                일평균거래량 += Convert.ToUInt64(words[5]); // 


       
                int day_dealt = 
                    (int)((ulong)(Convert.ToDouble(words[4]) * Convert.ToUInt64(words[5]) / g.억원)); // 종가 * 당일거래량
           



                 // 일거래량 X 종가 / 억원
                avr_dealt += day_dealt;
                if (day_dealt > max_dealt)
                    max_dealt = day_dealt;
                if (day_dealt < min_dealt)
                    min_dealt = day_dealt;

                double diff = 0.0;
                if (start_price != 0)
                    diff = (close_price - start_price) / start_price * 100;

                day_list.Add(diff);

                days_count++;
            }

            if (days_count == 0)
                return "";

            avr_dealt = avr_dealt / days_count;
            double temp_avr = 0.0;
            dev = 0.0;
            if (days_count > 0)
            {
                temp_avr = day_list.Sum() / days_count;
                if (days_count <= 1)
                    dev = 0;
                else
                    dev = Math.Sqrt(day_list.Sum(x => Math.Pow(x - temp_avr, 2)) / (days_count - 1));
            }

            string str = temp_avr.ToString("0.#") + "/" + dev.ToString("0.#");
            avr = temp_avr;

            일평균거래량 = 일평균거래량 / (ulong)days_count;

            // 120일 중 전고
            //SixMothHigh = 0;
            //foreach (var line in lines)
            //{
            //    string[] words = line.Split(' ');
            //    for (int i = 1; i <= 4; i++)
            //    {
            //        int price = Convert.ToInt32(words[i]); // 시가, 고가, 저가, 종가

            //        if (price > SixMothHigh) // currently SixMothHigh is not percentage
            //            SixMothHigh = price;
            //    }
            //}

            return str;
        }







        /*
    public static void read_외인기관일별매수동향(string stock, g.stock_data data)
    {

    string path = @"C:\병신\매\" + stock + ".txt";
    if (!File.Exists(path))
    {
        return;
    }

    string[] grlines = System.IO.File.ReadAllLines(path, Encoding.Default);

    int inc = 0;
    int day = 0;
    foreach(var line in grlines)
    {
        string[] words = line.Split(' ');
        if (inc == 0)
        day = Convert.ToInt32(words[0]);

        data.개량[inc] = Convert.ToInt32(words[1]); 
        data.외량[inc] = Convert.ToInt32(words[2]);
        data.기량[inc] = Convert.ToInt32(words[3]);
        inc++;
    }

    path = @"C:\병신\일\" + stock + ".txt";
    if (!File.Exists(path))
        return;

    List<string> lines = File.ReadLines(path).Reverse().Take(inc+1).ToList();
    double[] pricediffer = new double[inc  ];
    double[] closeprice =  new double[inc+1];
    int add = 0;
    foreach (var line in lines)
    {
        string[] words = line.Split(' ');
        closeprice[add++]= Convert.ToDouble(words[4]);
    }

    double[] instandfrig = new double[16];
    for(int i = 0; i < inc; i++)
    {
        pricediffer[i] = closeprice[i] - closeprice[i + 1];
        instandfrig[i] = data.개량[i] * -1.0;
    }

    data.양매수신뢰도 = cr.ComputeCoeff(pricediffer, instandfrig);


    if (data.외량[0] > 0 || data.기량[0] > 0)
        data.양매수수 = 1;
    if (data.외량[0] > 0 && data.기량[0] > 0)
        data.양매수수 = 2;
    if (data.외량[0] > 0 && data.기량[0] > 0 && (data.외량[1] > 0 || data.기량[1] > 0))
        data.양매수수 = 3;
    if (data.외량[0] > 0 && data.기량[0] > 0 && data.외량[1] > 0 && data.기량[1] > 0)
        data.양매수수 = 4;

    }

        /*  Calling Method : 그룹에서 전체를 읽고 group & single 반환
    *  스펠링 미스에 대한 점검 name to code를 하지 않아 오류 가능
    *  List<List<string>> groupList = new List<List<string>>();
    List<string> singleList = new List<string>();

    FileLib.Class.read_그룹(singleList, groupList);
    */
        /* 24일 거래량 중 상, 하 2개씩 극단을 제외하고 일평균거래량 환산 
    *  public static int calculate_종목20일기준일평균거래량(string stock)
    */


        /* 주어진 date, 두 개의 시간 구간 time[0]에서 time[1]로 1분 씩 증가시키면서 주어진 x[time[0], col]의 x[,col]의 값
        * 의 최대 차이, 최소 차이를 구하여 반환한다. 예를 들면 가격이 일정량 점프하였는 데 그 후 30분 내 점프한 값으로부터 
            최대 얼마나 하락할 지 최대 얼마나 상승할 지 알아보는 루틴 */





        /// <summary>
        /// used only in test
        /// </summary>
        /// <param name="hhmmss"></param>
        /// <returns></returns>
        public static double 시분초_일중환산율(int hhmmss)
        {
            int hh = hhmmss / 10000;
            int mm = hhmmss % 10000 / 100;
            int ss = hhmmss % 100;

            double numerator = 6 * 60 * 60 + 21 * 60;
            double denominator = (hh - 9) * 60 * 60 + mm * 60 + ss;
            if (denominator < 0.1)
                denominator = 1.0;
            if (denominator > numerator)
                numerator = denominator;

            return numerator / denominator;
        }


        public static double 누적거래액환산율(int hhmmss)
        {
            double value = 0;
            if (hhmmss > 10000) // if 6 digit is passed, make it 4 digit
                hhmmss /= 100;

            int hh = Convert.ToInt32(hhmmss) / 100;
            int mm = Convert.ToInt32(hhmmss) % 100;

            if (hh >= 15) // 시작시간 9시
                          //if (hh >= 16) // 시작시간 10시
            {
                if (mm > 20)
                {
                    mm = 20;
                }
            }
            value = (hh - 9) * 60 + mm + 1; // 시작시간 9시
                                            //value = (hh - 10) * 60 + mm + 1; // 시작시간 10시
            if (value <= 0 || value > 6 * 60 + 21) // value가 0 이하 또는 381보다 크면 value = 381
                value = 6 * 60 + 21;


            double return_value = 381.0 / value;
            return return_value;
        }

        /// <summary>
        /// given current directory date, -1, +1 forward 
        /// and backward directory date search
        /// </summary>
        /// <param name="date_int"></param>
        /// <param name="updn"></param>
        /// <returns></returns>
        public static int directory_분전후(int date_int, int updn)
        {
            var subdirs = Directory.GetDirectories(@"C:\병신\분")
                    .Select(Path.GetFileName).ToList();

            List<string> selected_subdirs = new List<string>();   // changing single list

            foreach (var item in subdirs)
            {
                if (item.Length == 8)
                    selected_subdirs.Add(item);
            }

            string date_string = date_int.ToString();
            int index = selected_subdirs.IndexOf(date_string);

            if (updn == 1)
            {
                if (index == selected_subdirs.Count - 1)
                    return -1;
                date_string = selected_subdirs[++index];
            }
            if (updn == -1)
            {
                if (index == 0)
                    return -1;
                date_string = selected_subdirs[--index];
            }

            return Convert.ToInt32(date_string);
        }




     





        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringToFront()
        {
            return;
            Process[] AllBrowsers = Process.GetProcesses();
            foreach (var process in AllBrowsers)
            {
                if (process.MainWindowTitle != "")
                {
                    string s = process.ProcessName.ToLower();
                    if (s.Contains("new tradegy"))
                        SetForegroundWindow(process.MainWindowHandle);
                }
            }
        }

        public static void call_네이버(string stock, string selection)
        {
            CPUTILLib.CpStockCode _cd = new CPUTILLib.CpStockCode();

            if (stock == null)
                Process.Start("chrome.exe", "https://finance.naver.com/");
            //Process.Start("microsoft-edge:https://finance.naver.com/");
            else
            {
                string basestring;
                if (selection == "fchart") // fchart
                {
                    basestring = "https://finance.naver.com/item/fchart.nhn?code=";
                    //basestring = "microsoft-edge:https://finance.naver.com/item/fchart.nhn?code=";
                }
                else if (selection == "main") // main
                {
                    basestring = "https://finance.naver.com/item/main.naver?code="; // 투자자별 매매동향
                    //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code="; // 종합정보
                    //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code=";
                }
                else // foreign & institute buying history
                {
                    basestring = "https://finance.naver.com/item/frgn.naver?code=";
                }

                string code = _cd.NameToCode(stock);
                code = new String(code.Where(Char.IsDigit).ToArray());
                basestring += code;
                //OpenTabBySelenium(basestring);
                Process.Start("chrome.exe", basestring);
            }
        }

  

        public static void call_네이버_차트(string stock, int selection, double xval)
        {
            CPUTILLib.CpStockCode _cd = new CPUTILLib.CpStockCode();

            if (stock == null)
                Process.Start("https://finance.naver.com/");
            //Process.Start("microsoft-edge:https://finance.naver.com/");
            else
            {
                string basestring;
                if (selection == 0)
                {
                    //basestring = "https://finance.naver.com/item/frgn.nhn?code="; // 투자자별 매매동향
                    basestring = "https://finance.naver.com/item/main.nhn?code="; // 종합정보
                                                                                  //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code=";
                }
                else
                {
                    //basestring = "https://finance.naver.com/item/fchart.nhn?code=";
                    basestring = "microsoft-edge:https://finance.naver.com/item/fchart.nhn?code=";
                }

                string code = _cd.NameToCode(stock);
                code = new String(code.Where(Char.IsDigit).ToArray());
                basestring += code;
                Process.Start(basestring);
            }
        }
    }
}


