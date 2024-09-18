
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using CPSYSDIBLib;
using New_Tradegy.Library;
namespace New_Tradegy.Library
{
    public class ts
    {

        public static void testing()
        {
            g.test = true;
            g.q = "h&s";

            testing_상한가다음날종가매수후그다음날장중수익시매도();

            // 큰일이
            // testing_표준화_거분_프분_외분_배차_배합_draw();
            //testing_표준화_거분_프분_외분_배차_배합(20230102, 20230207);


            // testing_분데이터_382라인이며프돈변동있는디렉토리남기고나머지지우기(20200102, 20230113);

            // testing_배수_잔잔_급증(20220401, 20220402);

            //while (true)
            //{
            //    DateTime date = DateTime.Now;
            //    int HHmm = Convert.ToInt32(date.ToString("HHmm")); // run_task_read_eval_score()
            //    int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));


            //    double 일중거래액환산율 = wk.시분초_일중환산율(HHmmss); // return value of 분별누적인수 > 0 && 분별누적인수 <=1.0 // testing
            //}
            // testing_분당프돈및분당프돈퍼센티지높은종목(20220701, 20220819);


            //testing_개인매수액분증이추후가격에미치는영향(20200801, 20200814);

            //testing_체결배수_가격점프_매수결과(20200110, 20200110); // 시작날짜 마지막날짜 디렉토리 내의  _proceeded 파일 읽고 계산

            //testing_가격급등후일정시간가격추이해석(sw);
            //testing_수급체결지상후가격변화_종목별(sw);

            //testing_양매수(); // 전 날 외,기 양매수한 경우 무조건 매수 후 당일 승률 및 평균 대비
            //testing_ETF(); // etf를 전 날 장마감에 사서 다음 날 시초가에 던지는 경우 시뮬레이션
            //testing_ETF일중분별평균(sw)

        }


        public static void testing_표준화_거분_프분_외분_배차_배합_draw()
        {

            string filename = @"C:\병신\프분최대.txt"; // QQQ

            if (!File.Exists(filename)) return;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);

            g.nCol = 6;
            g.nRow = 3;

            int count = 0;
            while (true)
            {
                var words = grlines[count++].Split(' ');
                string stock = words[0];
                for (int i = 0; i < 17; i++)
                {
                    words = grlines[count++].Split('\t');
                    g.date = Convert.ToInt32(words[0]);
                    int minute_happend = Convert.ToInt32(words[1]);
                    g.time[0] = minute_happend - 10;
                    g.time[1] = minute_happend + 10;

                    if (g.time[0] < 0)
                        g.time[0] = 0;
                    if (g.time[1] >= 382)
                        g.time[1] = 382;

                    dr.draw_stock(g.chart1, g.nRow, g.nCol, i, stock);
                }

            }
            return;
        }



        public static void 지수합계점검()
        {
       
            g.stock_data o = new g.stock_data();

            string file = @"C:\병신\" + "temp.txt";
            if (File.Exists(file))
                File.Delete(file);

            if (!File.Exists(file))
                File.Create(file).Dispose();

            
            for (int i = 20240726; i <= 20240726; i++)
            {
                string str = "";

                str += i.ToString() + "\n";
                g.전일종가이상 =
                g.date = i;
                string directory = @"C:\병신\분\" + g.date.ToString();
                if (!Directory.Exists(directory))
                    continue;

                o.nrow = 0;
                //o.nrow = rd.read_Stock_Minute(g.date, "KODEX 코스닥150레버리지", o.x);
                o.nrow = rd.read_Stock_Minute(g.date, "KODEX 레버리지", o.x);

                for (int j = 1; j < o.nrow; j++)
                {
                    ps.post_지수_프외_배차_합산(j);
                    str += (o.x[j, 3]).ToString() + "\t" + g.코스피지수순매수 + "\n";

                }
                str += "\n\n";

                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                sw.WriteLine("{0}", str);
                sw.Close();
            }
            
        }


        public static void testing_표준화_거분_프분_외분_배차_배합(int from_date, int to_date)
        {
            // 거분, 프분, 외분 1천만 이상
            // 배차, 배합
            var a_tuple = new List<Tuple<double, int, int>> { };

            int[,] x = new int[400, 12];
            string[] str = new string[2];


            from_date = 20201201;
            to_date = 20230203;

            double 프분_평균 = 0.0;
            double 프분_편차 = 0.0;


            // g.sl is in the order of dealt in money on the specified day
            foreach (string stock in g.sl)
            {
                int 전일종가 = rd.read_전일종가(stock);

                for (int i = from_date; i <= to_date; i++)
                {
                    g.date = i;


                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                        continue;

                    int nrow = rd.read_Stock_Minute(i, stock, x); // i -> date

                    for (int j = 1; j < nrow; j++)
                    {
                        if (x[j, 0] / 100 - x[j - 1, 0] / 100 > 1) // 150000 이후 매수 안 함
                        {
                            continue;
                        }
                        long 분프원 = (long)(x[j, 4] - x[j - 1, 4]) * 전일종가;
                        double 분프천 = 분프원 / (double)g.천만원;


                        a_tuple.Add(Tuple.Create(분프천, i, j));
                    }
                }
                a_tuple = a_tuple.OrderByDescending(t => t.Item1).ToList();

                for (int i = 0; i < 18; i++)
                {
                    if (i >= a_tuple.Count)
                        break;
                    //str[0] = a_tuple[i].Item2.ToString();
                    //str[1] = a_tuple[i].Item3.ToString();


                    g.date = Convert.ToInt32(a_tuple[i].Item2);
                    int minute_happend = Convert.ToInt32(a_tuple[i].Item3);
                    g.time[0] = minute_happend - 10;
                    g.time[1] = minute_happend + 10;

                    if (g.time[0] < 0)
                        g.time[0] = 0;
                    if (g.time[1] >= 382)
                        g.time[1] = 382;

                    dr.draw_stock(g.chart1, g.nRow, g.nCol, i, stock);

                }
                return;

                // Thread.Sleep(6000);

                //foreach (Series series in g.chart1.Series)
                //{
                //    series.Points.Clear();
                //}

                foreach (Series series in g.chart1.Series)
                {
                    series.Points.Clear();
                }
                g.chart1.Series.Clear();
                g.chart1.ChartAreas.Clear();
                g.chart1.Legends.Clear();


            }
        }
        public static void testing_분데이터_382라인이며프돈변동있는디렉토리남기고나머지지우기(int from_date, int to_date)
        {
            g.stock_data o = new g.stock_data();

            string file = @"C:\병신\" + "temp.txt";
            if (File.Exists(file))
                File.Delete(file);

            if (!File.Exists(file))
                File.Create(file).Dispose();

            for (int i = from_date; i <= to_date; i++)
            {
                g.date = i;
                string directory = @"C:\병신\분\" + g.date.ToString();
                if (!Directory.Exists(directory))
                    continue;

                bool directory_delete = false;

                o.nrow = 0;
                o.nrow = rd.read_Stock_Minute(g.date, "삼성전자", o.x);

                int equal_count = 0;
                if (o.nrow != 382)
                {
                    directory_delete = true;
                }
                else
                {
                    for (int j = 0; j < o.nrow - 1; j++)
                    {
                        if (o.x[j, 4] == o.x[j + 1, 4])
                            equal_count++;
                    }
                    if (equal_count > 100)
                    {
                        directory_delete = true;
                    }
                }

                string[] t = new string[3];
                t[0] = i.ToString() + '\t';
                t[1] = equal_count.ToString() + "/" + o.nrow.ToString() + "\t";
                if (directory_delete)
                    t[2] = "delete";
                else
                {
                    string to_directory = @"E:\분\" + g.date.ToString();
                    Directory.CreateDirectory(to_directory); // testing
                    var dir = new DirectoryInfo(directory);

                    foreach (FileInfo f in dir.GetFiles())
                    {
                        string targetFilePath = Path.Combine(to_directory, f.Name);
                        long fileSizeibBytes = f.Length;

                        if (File.Exists(targetFilePath))
                        {
                            File.Delete(targetFilePath);
                        }

                        if (fileSizeibBytes > 10000)
                            f.CopyTo(targetFilePath);
                    }
                }



            }
        }

        public static void testing_배수_잔잔_급증(int from_date, int to_date)
        {
            int[,] x = new int[400, 12];

            List<string> 제외 = new List<string>();
            제외 = rd.read_제외(); // testing_배수_잔잔_급증

            for (int i = from_date; i <= to_date; i++)
            {
                g.date = i;

                g.sl.Clear();
                rd.read_all_stocks_for_given_date(g.sl);

                foreach (string stock in g.sl)
                {
                    if (제외.Contains(stock))
                    {
                        continue;
                    }
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                    {
                        continue;
                    }

                    int column = rd.read_Stock_Minute(i, stock, x);


                    for (int j = 4; j < 360; j++)
                    {
                        if (x[j, 0] <= 0 || x[j, 0] > 150000) // 150000 이후 매수 안 함
                        {
                            continue;
                        }

                        //if (x[j, 7] < x[j - 1, 7] ||  // 거래량이 시간경과하면서 감소시 무시
                        //    x[j - 1, 7] < x[j - 2, 7] ||
                        //    x[j - 2, 7] < x[j - 3, 7])
                        //    continue;

                        if (x[j, 2] < 100 ||  // 수요계수 < 100, 체강계수 < 100 제외
                            x[j, 3] < 10000)
                            continue;

                        if (x[j, 1] - x[j - 1, 1] > 100 &&

                            //x[j - 3, 8] - x[j - 3, 9] < x[j - 2, 8] - x[j - 2, 9] &&
                            //x[j - 2, 8] - x[j - 2, 9] < x[j - 1, 8] - x[j - 1, 9] &&
                            //x[j - 1, 8] - x[j - 1, 9] < x[j - 0, 8] - x[j - 0, 9] &&

                            (x[j - 3, 8] + x[j - 3, 9] + x[j - 2, 8] + x[j - 2, 9] + x[j - 1, 8] + x[j - 1, 9]) / 3 < 30
                            && x[j, 8] - x[j, 9] > 100)
                        //&&
                        //(x[j - 3, 8] - x[j - 3, 9] + x[j - 2, 8] - x[j - 2, 9] + x[j - 1, 8] - x[j - 1, 9]) / 3 < 10) // 배수잔잔 후 가격급증)
                        {
                            string[] str = new string[3];
                            str[0] = g.date.ToString();
                            str[1] = stock;
                            str[1] += "\n";
                            for (int m = j - 3; m <= j + 10; m++) // bound exist not the size
                            {
                                if (x[m, 0] == 0 || x[m, 0] > 152100) // if time is not set then stop writing
                                    continue;

                                for (int k = 0; k < 12; k++) // bound exist not the size
                                {
                                    if (k == 0)
                                        str[2] += x[m, k];

                                    else
                                        str[2] += "\t " + x[m, k];
                                }
                                str[2] += "\n";
                            }
                            str[2] += "\n";

                            j += 10;
                        }
                    }
                }
            }

        }

        public static void testing_매수후일정시간경과결과(int date, string stock,
        int start_time_in_minute, int elapse_time)
        {
            int[,] x = new int[400, 12];

            rd.read_Stock_Minute(date, stock, x);

            int start_time = -1;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                if (x[i, 0] == start_time_in_minute)
                {
                    start_time = i;
                    break;
                }
            }
            if (start_time == -1)
                return;

            int allHigh = x[start_time, 1];
            int startPrice = x[start_time, 1];
            int exitPrice = 0;
            int max_time = 0, positive_difference = 0;
            int min_time = 0, negative_difference = 0;

            for (int i = start_time + 1; i < start_time + elapse_time; i++)
            {
                if (i == x.GetLength(0) - 1)
                    break;

                int dif = x[i, 1] - startPrice;
                if (allHigh < x[i, 1])
                    allHigh = x[i, 1];

                if (dif > positive_difference)
                {
                    positive_difference = dif;
                }
                if (dif < negative_difference)
                {
                    negative_difference = dif;
                }
                if (x[i, 1] - x[i - 1, 1] < -50)
                {
                    exitPrice = x[i, 1];
                    break;
                }
            }

            // 당일삭제 
            //int differencePrice = exitPrice - startPrice;
            // if(differencePrice  < 0)
            //{
            //    g.임시누적손실 += differencePrice;
            //    g.임시상승종목++;
            //}
            //else
            //{
            //    g.임시누적수익 += differencePrice;
            //    g.임시하락종목++;
            //}
            //g.임시종목숫자 += 1;


            string path = @"C:\병신\temp.txt";
            StreamWriter sw = File.AppendText(path);
            sw.WriteLine("{0}\t\t\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", date, stock, x[start_time, 0], max_time,
                                                                           positive_difference, min_time, negative_difference);
            sw.Close();
        }

        public static void testing_개인매수액분증이추후가격에미치는영향(int from_date, int to_date)
        {
            int price_difference = 30;
            int[,] x = new int[400, 12];

            string path = @"c:\병신\temp.txt";
            StreamWriter sw = File.AppendText(path);

            //sw.close();
            for (int date = from_date; date <= to_date; date++)
            {
                int total_count = 0;
                int nrow = rd.read_Stock_Minute(date, "KODEX 레버리지", x);
                if (nrow < 10)
                {
                    continue;
                }

                for (int i = 1; i < 400; i++)
                {
                    if (x[i, 0] == 0 || x[i, 0] > 1500)
                    {
                        break;
                    }
                    int t = Math.Abs(x[i + 1, 1] - x[i, 1]);
                    if (t > price_difference)
                    {
                        total_count++;

                        //sw.WriteLine("{0}\t{1}\t{2}", x[i, 0], x[i, 1], x[i, 6]);
                        for (int j = 0; j < 10; j++)
                        {
                            sw.WriteLine("{0}\t{1}\t{2}", x[i + j, 0], x[i + j, 1], x[i + j, 6]);
                        }
                        sw.WriteLine();

                    }
                }

                sw.WriteLine("{0}\t{1}", date, total_count);

            }
            sw.Close();
        }

        public static void testing_체결배수_가격점프_매수결과_리스팅(int date, string stock,
        int start, int[,] x, double price_down, ref int end)
        {
            double earning = -3100;
            for (int i = start; i < 390; i++)
            {
                double dif = x[i + 1, 1] - x[start, 1];
                if (dif > earning)
                {
                    earning = dif;
                }
                if (dif < price_down || x[i + 1, 1] - x[i, 1] < price_down)
                {
                    end = i;
                    break;
                }
                if (end > start + 10)
                {
                    break;
                }
            }

            string path = @"C:\병신\temp.txt";
            StreamWriter sw = File.AppendText(path);


            int index = g.ogl_data.FindIndex(r => r.stock == stock);
            if (index < 0)
            {
                sw.Close();
                return;
            }


            if (g.ogl_data[index].dev > 2.5)
            {
                sw.WriteLine("{0,-10}{1,-15}{2,-10}{3,-10}{4,-10}{5,-10}{6,-10}{7,-10}",
                  date, stock, earning, x[start, 0], x[start, 1], g.ogl_data[index].dev_avr,
                  (int)((x[start, 2] + x[start, 3]) * g.ogl_data[index].전일종가 / 10000.0),
                  g.ogl_data[index].전일종가);


                /*
                for (int i = start-3; i < end+3; i++)
                {
                  if (i < 0 || i > 1530)
                  continue;

                  sw.WriteLine("{0:0}\t{1:0}\t{2:0}\t{3:0}\t{4:0.0}\t{5:0.0}", x[i, 0], x[i, 1], x[i, 2], x[i, 3], x[i, 4], x[i, 5]);
                  if (i > start + 10)
                  continue;

                }
                sw.WriteLine();*/
            }

            sw.Close();

        }

        public static void testing_분당프돈및분당프돈퍼센티지높은종목(int from_date, int to_date)
        {
            int[,] x = new int[400, 12];

            for (int i = from_date; i <= to_date; i++)
            {
                if (i % 100 > 31)
                    continue;

                g.sl.Clear();
                rd.read_all_stocks_for_given_date(g.sl);

                foreach (string stock in g.sl)
                {
                    double avr = 0.0, dev = 0.0;
                    int a = 0, b = 0, c = 0;
                    ulong e = 0;
                    //int long_high = 0;

                    wk.calcurate_종목일중변동평균편차(stock, 20, ref avr, ref dev, ref a, ref b, ref c, ref e); // a,b,c -> avrage deal, max and min
                    if (stock.Contains("KODEX") || stock.Contains("혼합") || dev < 1.0)
                    {
                        continue;
                    }

                    int nrow = rd.read_Stock_Minute(i, stock, x); // i is given date

                    int yesterday_close = rd.read_전일종가(stock);
                    if (yesterday_close < 0)
                        continue;

                    for (int j = 1; j < nrow - 11; j++)
                    {
                        double program_money = (double)(x[j, 4] - x[j - 1, 4]) * (double)yesterday_close;
                        program_money /= 10000000;

                        if (program_money > 500) // 천만 단위
                                                 //if (program_money > 10 && percentage > 50 && dev > 5.0)
                        {
                            double money_per_minute = x[j, 7] - x[j - 1, 7];
                            money_per_minute *= yesterday_close;
                            money_per_minute /= 10000000;
                            int percentage = 0;
                            if (money_per_minute > 0)
                                percentage = (int)(program_money / (double)money_per_minute * 100);

                            int currently_accumulated_money_dealt = (int)x[j, 7] * (int)yesterday_close / 10000000;
                            int closing_accumulated_money_dealt = (int)x[nrow - 1, 7] * (int)yesterday_close / 10000000;

                            if (percentage < 0)
                            {
                                continue; ;
                            }

                            if (percentage > 110)
                            {
                                continue;
                            }

                            int price_up = 0;
                            if (j - 1 > 0)
                                price_up = x[j, 1] - x[j - 1, 1];

                            int maxpos = 0, max = 0, minpos = 0, min = 0;
                            멏분후_값차이_최대_최소(x, j, 20, 1, ref maxpos, ref max, ref minpos, ref min);// 10 lapse_time, 1 column i.e. price

                            string str = g.date.ToString() + "\t";
                            str += stock + "/" + Math.Round(dev, 1) + "\t";
                            str += program_money.ToString("#") + "/" + percentage.ToString() + "\t";
                            str += price_up.ToString() + "\t";
                            str += max.ToString() + "/" + (maxpos - j).ToString() + "\t"; ;
                            str += min.ToString() + "/" + (minpos - j).ToString() + "\t"; ;
                        }
                    }
                }
            }
        }

        public static void testing_분전분당거래액작으나가격100이상급상(int from_date, int to_date)
        {
            int[,] x = new int[400, 12];


            for (int i = from_date; i <= to_date; i++)
            {
                g.date = i;
                string str1 = g.date.ToString();


                g.sl.Clear();
                rd.read_all_stocks_for_given_date(g.sl);

                foreach (string stock in g.sl)
                {
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                    {
                        continue;
                    }

                    int nrow = rd.read_Stock_Minute(i, stock, x); // i is given date

                    int 전일종가 = rd.read_전일종가(stock);
                    for (int j = 3; j < nrow - 3; j++)
                    {
                        if (x[j, 1] - x[j - 1, 1] > 100)
                        {
                            ulong 삼분전동안누적거래액 = (((ulong)x[j, 7] - (ulong)x[j - 3, 7]) * (ulong)전일종가);
                            삼분전동안누적거래액 /= 10000000;
                            ulong 현시점누적거래액 = (ulong)x[j, 7] * (ulong)전일종가 / 10000000;
                            ulong 종가후당일누적거래액 = (ulong)x[nrow - 1, 7] * (ulong)전일종가 / 10000000;
                            if (삼분전동안누적거래액 < 3)
                            {
                                string[] str = new string[6];
                                str[0] = stock;
                                str[1] = 삼분전동안누적거래액.ToString();
                                str[2] = 현시점누적거래액.ToString();
                                str[3] = 종가후당일누적거래액.ToString();
                                str[4] = "";
                                str[5] = 전일종가.ToString();

                                if (종가후당일누적거래액 > 1000)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        public static void 멏분후_값차이_최대_최소(int[,] x, int start_time, int lapse_time, int col,
                     ref int maxpos, ref int max, ref int minpos, ref int min)
        {
            min = 10000;
            max = -10000;

            for (int t = start_time + 1; t < start_time + lapse_time; t++)
            {
                int diff = x[t, col] - x[start_time, col];

                if (max < diff)
                {
                    maxpos = t;
                    max = diff;
                }

                if (min > diff)
                {
                    minpos = t;
                    min = diff;
                }


                /*
				if (min < -70 && t != start_time + 1)
				{
				  return;
				}
				*/
            }
        }

        public static void testing_시총이상_가격급상(int from_date, int to_date)
        {
            int[,] x = new int[400, 12];

            List<string> 제외 = new List<string>();
            제외 = rd.read_제외(); // testing_시총이상_가격급상

            for (int i = from_date; i <= to_date; i++)
            {
                g.date = i;

                g.sl.Clear();
                rd.read_all_stocks_for_given_date(g.sl);

                foreach (string stock in g.sl)
                {
                    if (제외.Contains(stock))
                    {
                        continue;
                    }
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                    {
                        continue;
                    }

                    int column = rd.read_Stock_Minute(i, stock, x);
                    int index = g.ogl_data.FindIndex(r => r.stock == stock);
                    if (index < 0)
                        continue;

                    for (int j = 4; j < 360; j++)
                    {
                        if (x[j, 0] <= 0 || x[j, 0] > 150000) // 150000 이후 매수 안 함
                        {
                            continue;
                        }

                        if (x[j, 1] - x[j - 1, 1] > 100 && g.ogl_data[index].시총 > 200) // 첫 마젠타 + 가격 50 이상 + 배수차 50 이상
                        {
                            string[] str = new string[3];
                            str[0] = g.date.ToString();
                            str[1] = stock;

                            for (int m = j - 3; m <= j + 10; m++) // bound exist not the size
                            {
                                if (x[m, 0] == 0 || x[m, 0] > 152100) // if time is not set then stop writing
                                    continue;

                                for (int k = 0; k < 12; k++) // bound exist not the size
                                {
                                    if (k == 0)
                                        str[2] += x[m, k];

                                    else
                                        str[2] += "\t" + x[m, k];
                                }
                                str[2] += "\n";
                            }

                            j += 10;
                        }
                    }
                }
            }
        }

        public static void testing_체결배수_가격점프_매수결과_리스팅(int date, string stock,
        int start, double[,] x, double price_down, ref int end)
        {

            double earning = -3100;
            for (int i = start; i < 390; i++)
            {
                double dif = x[i + 1, 1] - x[start, 1];
                if (dif > earning)
                {
                    earning = dif;
                }
                if (dif < price_down || x[i + 1, 1] - x[i, 1] < price_down)
                {
                    end = i;
                    break;
                }
            }

            string path = @"C:\병신\temp.txt";
            StreamWriter sw = File.AppendText(path);



            sw.WriteLine("{0}\t{1}\t{2}", date, stock, earning);
            for (int i = start - 3; i < end + 3; i++)
            {
                if (i < 0 || i > 1530)
                    continue;

                sw.WriteLine("{0:0}\t{1:0}\t{2:0}\t{3:0}\t{4:0.0}\t{5:0.0}", x[i, 0], x[i, 1], x[i, 2], x[i, 3], x[i, 4], x[i, 5]);

            }
            sw.WriteLine();
            sw.Close();

        }

        public static void read_write_7Columns(string stock)
        {
            string path = @"C:\병신\분\" + g.date.ToString() + "\\" + stock + "_processed" + ".txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);
            sw.Close();


            double[,] x = new double[400, 7];
            //wk.read_stock_marketeye(g.date, stock, x);
            int index = g.ogl_data.FindIndex(r => r.stock == stock);

            double pufac = 0;
            double pdfac = 0;



            for (int i = 0; i < 400; i++)
            {
                if (x[i, 0] == 0)
                {
                    break;
                }

                double ufac = 100.0 / (x[i, 3] + 100.0);
                double dfac = x[i, 3] / (x[i, 3] + 100.0);

                ufac = (int)(x[i, 4] * ufac);
                dfac = (int)(x[i, 4] * dfac);

                if (ufac < 0 || dfac < 0)
                {

                }


                /*

                ufac = ufac / g.ogl_data[index].일평균거래량 / g.mF[i];
                dfac = dfac / g.ogl_data[index].일평균거래량 / g.mF[i];

                x[i,2] = (int)(ufac * 100);
                x[i,3] = (int)(dfac * 100);
                */





                sw = File.AppendText(path);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4:0.0}\t{5:0.0}", x[i, 0], x[i, 1], dfac - pdfac, ufac - pufac,
                  (dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381.0, (ufac - pufac) / g.ogl_data[index].일평균거래량 * 381.0);
                sw.Close();




                /*

                // The following lines are for test purpose
                if (i > 10 && !written && (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381 > 20.0 && x[i, 1] - x[i - 1, 1] > 150)

                  //if (i > 20 && !written && (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381 > 20.0 && x[i, 1] - x[i - 1, 1] > 150 &&
                  //Convert.ToDouble(g.ogl_data[index].dev) > 2.0)
                  {
                  string file = @"C:\병신\temp.txt";
                  sw = File.AppendText(file);
                  sw.WriteLine("{0}\t{1:0.0}\t{2:0.0}\t{3:0.0}\t{4:0.0}", stock, g.ogl_data[index].avr, g.ogl_data[index].dev,
                  (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381, (double)(ufac - pufac) / g.ogl_data[index].일평균거래량 * 381.0);

                  for (int j = i-2; j < i + 12; j++)
                  {
                  sw.WriteLine("\t{0}\t{1}\t{2}\t{3}", x[j,0],x[j, 1],x[j,2],x[j,3]);
                  }

                  i = i + 10;

                  sw.WriteLine();
                  sw.Close();
                }


                */





                // The following 2 lines should be preserved
                pdfac = dfac;
                pufac = ufac;



                /*  program comment
                 *  i dn, a up then p can up for the time being
                 *  if p is higher than 1000, treat seperately or p should go up steeply for the minute considered
                 *  trend should be upwards, too low p also should be treated seperately or excluded
                 *  include ave and dev 
                 *  if p not moving, try to sell "buy p"
                 *  if i or a too low, neglect
                 */

            }
        }


        // not used
        public static void read_write_10Columns(string stock)
        {
            string path = @"C:\병신\분\" + g.date.ToString() + "\\" + stock + "_processed" + ".txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);
            sw.Close();


            int[,] x = new int[400, 10];
            rd.read_Stock_Minute(g.date, stock, x); // columns number is flexible
            int index = g.ogl_data.FindIndex(r => r.stock == stock);

            double pufac = 0;
            double pdfac = 0;


            for (int i = 0; i < 400; i++)
            {
                if (x[i, 0] == 0)
                {
                    break;
                }

                double ufac = 100.0 / (x[i, 3] + 100.0);
                double dfac = x[i, 3] / (x[i, 3] + 100.0);

                ufac = (int)(x[i, 7] * ufac);
                dfac = (int)(x[i, 7] * dfac);

                if (ufac < 0 || dfac < 0)
                {

                }

                /*
                ufac = ufac / g.ogl_data[index].일평균거래량 / g.mF[i];
                dfac = dfac / g.ogl_data[index].일평균거래량 / g.mF[i];

                x[i,2] = (int)(ufac * 100);
                x[i,3] = (int)(dfac * 100);
                */

                sw = File.AppendText(path);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7:0.0}\t{8:0.0}\t{9}", x[i, 0], x[i, 1], x[i, 2], x[i, 3],
                  x[i, 4], x[i, 5], x[i, 6],
                  (dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381.0,
                  (ufac - pufac) / g.ogl_data[index].일평균거래량 * 381.0, x[i, 7]);
                sw.Close();

                /*
                // The following lines are for test purpose
                if (i > 10 && !written && (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381 > 20.0 && x[i, 1] - x[i - 1, 1] > 150)

                  //if (i > 20 && !written && (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381 > 20.0 && x[i, 1] - x[i - 1, 1] > 150 &&
                  //Convert.ToDouble(g.ogl_data[index].dev) > 2.0)
                  {
                  string file = @"C:\병신\temp.txt";
                  sw = File.AppendText(file);
                  sw.WriteLine("{0}\t{1:0.0}\t{2:0.0}\t{3:0.0}\t{4:0.0}", stock, g.ogl_data[index].avr, g.ogl_data[index].dev,
                  (double)(dfac - pdfac) / g.ogl_data[index].일평균거래량 * 381, (double)(ufac - pufac) / g.ogl_data[index].일평균거래량 * 381.0);

                  for (int j = i-2; j < i + 12; j++)
                  {
                  sw.WriteLine("\t{0}\t{1}\t{2}\t{3}", x[j,0],x[j, 1],x[j,2],x[j,3]);
                  }

                  i = i + 10;

                  sw.WriteLine();
                  sw.Close();
                }
                */


                // The following 2 lines should be preserved
                pdfac = dfac;
                pufac = ufac;

                /*  program comment
                 *  i dn, a up then p can up for the time being
                 *  if p is higher than 1000, treat seperately or p should go up steeply for the minute considered
                 *  trend should be upwards, too low p also should be treated seperately or excluded
                 *  include ave and dev 
                 *  if p not moving, try to sell "buy p"
                 *  if i or a too low, neglect
                 */

            }
        }

        public static void testing_수급체결지상후가격변화_종목별(StreamWriter sw)
        {

            int[] time = new int[2];

            // 테스트 변수 : 아래 세팅은 오전 9시10분부터 오전 12시까지, 분당 변화 100일 경우 
            time[0] = 10;
            time[1] = 120;


            int lapse_time = 30;

            string[] dirs = Directory.GetDirectories(@"C:\병신\분", "*", SearchOption.TopDirectoryOnly);

            g.date = 20190419;



            foreach (string stock in g.sl)
            {

                int n_happening = 0;

                int earn = 0;
                int loss = 0;

                foreach (string dir in dirs)
                {
                    g.date = int.Parse(dir.Split('\\')[3]);
                    if (g.date > 20190331 || g.date < 20190102)
                    {
                        continue;
                    }

                    // KODEX 제외
                    if (stock.Contains("KODEX") || g.date < 20180102)
                    {
                        continue;
                    }

                    int[,] x = new int[400, 4];
                    //wk.read_stock2(g.date, stock, x);
                    for (int t = time[0]; t < time[1]; t++)
                    {

                        int a_positive_count = 0;
                        for (int n = 0; n < g.npts_for_magenta_cyan_mark; n++)
                        {
                            if (t - 1 - n < 0)
                            {
                                break;
                            }

                            int diff = x[t - n, 2] - x[t - 1 - n, 2];
                            if (diff >= 1)
                            {
                                a_positive_count++;
                            }
                        }

                        int i_positive_count = 0;
                        for (int n = 0; n < g.npts_for_magenta_cyan_mark; n++)
                        {
                            if (t - 1 - n < 0)
                            {
                                break;
                            }

                            int diff = x[t - n, 3] - x[t - 1 - n, 3];
                            if (diff >= 1)
                            {
                                i_positive_count++;
                            }
                        }
                        if (a_positive_count >= 4 && i_positive_count >= 4)
                        {
                            int min = 0;
                            int max = 0;

                            int col = 1;
                            int maxpos = -1;
                            int minpos = -1;
                            멏분후_값차이_최대_최소(x, t, lapse_time, col,
                                ref maxpos, ref max, ref minpos, ref min);
                            /*
                            if(x[t, 2] - x[t - 6, 2] > 20 &&  x[t, 3] - x[t - 6, 3] > 40)
                            {
                            */
                            sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4} \t{5} \t{6} \t{7} \t{8}",
                            x[t, 2] - x[t - 6, 2], x[t, 3] - x[t - 6, 3], max, min, stock, g.date, t, maxpos - t, minpos - t);

                            n_happening++;


                            earn += max;
                            loss += min;
                            /*
                            }
                            */
                        }
                    }
                }

                if (n_happening > 0)
                {
                    sw.WriteLine("{0} \t{1} \t{2}",
                        stock, earn / (double)n_happening, loss / (double)n_happening);
                }
            }
        }

        public static void testing_가격수급체결상승후일정시간가격추이해석_종목별(StreamWriter sw)
        {

            int[] time = new int[2];

            // 테스트 변수 : 아래 세팅은 오전 9시10분부터 오전 12시까지, 분당 변화 100일 경우 
            time[0] = 1;
            time[1] = 10;
            int price_difference = 0;
            double amount_times = 3;
            double intensity_times = 3;
            int lapse_time = 180;

            string[] dirs = Directory.GetDirectories(@"C:\병신\분", "*", SearchOption.TopDirectoryOnly);

            g.date = 20190419;

            int total_earn = 0;
            int total_loss = 0;
            int total_positive_happening = 0;
            int total_negative_happening = 0;


            foreach (string stock in g.sl)
            {
                int positive_happening = 0;
                int negative_happening = 0;

                int earn = 0;
                int loss = 0;

                foreach (string dir in dirs)
                {
                    g.date = int.Parse(dir.Split('\\')[3]);
                    if (g.date > 20190419)
                    {
                        continue;
                    }

                    // KODEX 제외
                    if (stock.Contains("KODEX") || g.date < 20180102)
                    {
                        continue;
                    }

                    int[,] x = new int[400, 4];
                    //wk.read_stock2(g.date, stock, x);
                    for (int t = time[0]; t < time[1]; t++)
                    {
                        // amount and intesity check
                        if (x[t, 2] < 151 || x[t, 3] < 151)
                        {
                            continue;
                        }

                        // price check

                        if (x[t, 1] - x[t - 1, 1] < price_difference)
                        {
                            continue;
                        }


                        double minute_before_100 = wk.시분초_일중환산율(t - 1); // testing
                        double minute_now_100 = wk.시분초_일중환산율(t); // testing

                        double amount_now = x[t, 2] * minute_now_100 - x[t - 1, 2] * minute_before_100;
                        double amount_times_now = amount_now / x[t - 1, 2];

                        if (amount_times_now < amount_times)
                        {
                            continue;
                        }

                        double intensity_now = x[t, 3] * minute_now_100 - x[t - 1, 3] * minute_before_100;
                        double intensity_times_now = intensity_now / x[t - 1, 3];

                        if (intensity_times_now < intensity_times)
                        {
                            continue;
                        }

                        int min = 0;
                        int max = 0;

                        int col = 1;
                        int maxpos = -1;
                        int minpos = -1;
                        멏분후_값차이_최대_최소(x, t, lapse_time, col,
                            ref maxpos, ref max, ref minpos, ref min);
                        /*
                        if(maxpos>minpos)
                        {*/
                        sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4} \t{5} \t{6}",
                            maxpos, max, minpos, min, stock, g.date, t);
                        /* For Later Use
                        sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4:F1}, \t{5:F1} \t{6} \t{7} \t{8}",
                            maxpos, max, minpos, min, amount_times_now, intensity_times_now, stock, g.date, t);
                            */

                        if (min > -70)
                        {
                            earn += max;
                            positive_happening++;
                        }
                        else
                        {
                            loss += min;
                            negative_happening++;
                        }
                        /*}*/
                    }
                }

                if (positive_happening != 0)
                {
                    sw.WriteLine("{0} \t{1}", positive_happening, earn / positive_happening);
                    total_earn += earn;
                    total_positive_happening += positive_happening;
                    if (negative_happening == 0)
                    {
                        sw.WriteLine(" ");
                    }
                }
                if (negative_happening != 0)
                {
                    sw.WriteLine("{0} \t{1}", negative_happening, loss / negative_happening);

                    total_loss += loss;
                    total_negative_happening += negative_happening;
                    sw.WriteLine(" ");
                }

            }
            if (total_positive_happening != 0)
            {
                sw.WriteLine("{0} \t{1}", total_positive_happening, total_earn / total_positive_happening);
                sw.WriteLine(" ");
                if (total_negative_happening == 0)
                {
                    sw.WriteLine(" ");
                }
            }
            if (total_negative_happening != 0)
            {
                sw.WriteLine("{0} \t{1}", total_negative_happening, total_loss / total_negative_happening);
                sw.WriteLine(" ");
            }
        }

        public static void testing_가격수급체결상승후일정시간가격추이해석_일자별(StreamWriter sw)
        {

            int[] time = new int[2];

            // 테스트 변수 : 아래 세팅은 오전 9시10분부터 오전 12시까지, 분당 변화 100일 경우 
            time[0] = 1;
            time[1] = 10;
            int price_difference = 0;
            double amount_times = 3;
            double intensity_times = 3;
            int lapse_time = 180;

            string[] dirs = Directory.GetDirectories(@"C:\병신\분", "*", SearchOption.TopDirectoryOnly);

            g.date = 20190419;

            int total_earn = 0;
            int total_loss = 0;
            int total_positive_happening = 0;
            int total_negative_happening = 0;


            foreach (string dir in dirs)
            {
                g.date = int.Parse(dir.Split('\\')[3]);
                if (g.date > 20190419)
                {
                    break;
                }


                int positive_happening = 0;
                int negative_happening = 0;

                int earn = 0;
                int loss = 0;


                foreach (string stock in g.sl)
                {

                    // KODEX 제외
                    if (stock.Contains("KODEX") || g.date < 20180102)
                    {
                        continue;
                    }

                    int[,] x = new int[400, 4];
                    //wk.read_stock2(g.date, stock, x);
                    for (int t = time[0]; t < time[1]; t++)
                    {
                        // amount and intesity check
                        if (x[t, 2] < 151 || x[t, 3] < 151)
                        {
                            continue;
                        }

                        // price check

                        if (x[t, 1] - x[t - 1, 1] < price_difference)
                        {
                            continue;
                        }


                        double minute_before_100 = wk.시분초_일중환산율(t - 1); // testing
                        double minute_now_100 = wk.시분초_일중환산율(t);

                        double amount_now = x[t, 2] * minute_now_100 - x[t - 1, 2] * minute_before_100;
                        double amount_times_now = amount_now / x[t - 1, 2];

                        if (amount_times_now < amount_times)
                        {
                            continue;
                        }

                        double intensity_now = x[t, 3] * minute_now_100 - x[t - 1, 3] * minute_before_100;
                        double intensity_times_now = intensity_now / x[t - 1, 3];

                        if (intensity_times_now < intensity_times)
                        {
                            continue;
                        }



                        int min = 0;
                        int max = 0;

                        int col = 1;
                        int maxpos = -1;
                        int minpos = -1;
                        멏분후_값차이_최대_최소(x, t, lapse_time, col,
                            ref maxpos, ref max, ref minpos, ref min);
                        /*
                        if(maxpos>minpos)
                        {*/
                        sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4} \t{5} \t{6}",
                          maxpos, max, minpos, min, stock, g.date, t);
                        /* For Later Use
                        sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4:F1}, \t{5:F1} \t{6} \t{7} \t{8}",
                          maxpos, max, minpos, min, amount_times_now, intensity_times_now, stock, g.date, t);
                          */

                        if (min > -70)
                        {
                            earn += max;
                            positive_happening++;
                        }
                        else
                        {
                            loss += min;
                            negative_happening++;
                        }
                        /*}*/
                    }
                }

                if (positive_happening != 0)
                {
                    sw.WriteLine("{0} \t{1}", positive_happening, earn / positive_happening);
                    total_earn += earn;
                    total_positive_happening += positive_happening;
                    if (negative_happening == 0)
                    {
                        sw.WriteLine(" ");
                    }
                }
                if (negative_happening != 0)
                {
                    sw.WriteLine("{0} \t{1}", negative_happening, loss / negative_happening);

                    total_loss += loss;
                    total_negative_happening += negative_happening;
                    sw.WriteLine(" ");
                }

            }
            if (total_positive_happening != 0)
            {
                sw.WriteLine("{0} \t{1}", total_positive_happening, total_earn / total_positive_happening);
                sw.WriteLine(" ");
                if (total_negative_happening == 0)
                {
                    sw.WriteLine(" ");
                }
            }
            if (total_negative_happening != 0)
            {
                sw.WriteLine("{0} \t{1}", total_negative_happening, total_loss / total_negative_happening);
                sw.WriteLine(" ");
            }
        }

        public static void testing_가격급등후일정시간가격추이해석(StreamWriter sw)
        {
            int[,] x = new int[400, 4];
            int[] time = new int[2];
            // 테스트할 시간 구간 : 아래 세팅은 오전 9시10분부터 오전 12시까지, 분당 변화 100일 경우 
            time[0] = 2;
            time[1] = 15;

            string[] dirs = Directory.GetDirectories(@"C:\병신\분", "*", SearchOption.TopDirectoryOnly);

            foreach (string dir in dirs)
            {
                g.date = int.Parse(dir.Split('\\')[3]);
                if (g.date > 20180120)
                {
                    continue;
                }


                foreach (string stock in g.sl)
                {
                    if (stock.Contains("KODEX") || g.date < 20180102)
                    {
                        continue;
                    }
                    //

                    //wk.read_stock2(g.date, stock, x);
                    for (int t = time[0]; t < time[1]; t++)
                    {
                        /*
                        int positive = 0;
                        for(int i=t; i>=t-3;i--)
                        {
                          if(x[i,1]-x[i-1,1]>0)
                          {
                            positive++;
                          }
                        }
                        */

                        if (x[t, 1] - x[t - 1, 1] > 100)
                        {
                            int min = 0;
                            int max = 0;


                            //if (x[t, 3] > 200)
                            //{
                            // time[0] start time, 30 lapse time, 1 col means price, return value 최대하락 최대상승
                            int lapse_time = 10;
                            int col = 1;
                            int maxpos = -1;
                            int minpos = -1;
                            멏분후_값차이_최대_최소(x, t, lapse_time, col,
                                ref maxpos, ref max, ref minpos, ref min);
                            // count++;
                            // min_sum += min;
                            // max_sum += max;
                            sw.WriteLine("{0} \t{1} \t{2} \t{3} \t{4}, \t{5} \t{6}",
                               minpos, min, maxpos, max, stock, g.date, t);

                            /*
                              for (int i = t - 1; i < t + 10; i++)
                              {
                              sw.WriteLine("{0} \t{1} \t{2} \t{3}", i, x[i, 1], x[i, 2], x[i, 3]);
                              }
                              sw.WriteLine(" ");
                            //} 
                            */
                        }
                    }
                }

            }
        }

        public static void testing_ETF일중분별평균(StreamWriter sw)
        {


            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 코스닥150레버리지" + ".txt";
            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 레버리지" + ".txt";
            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 200선물인버스2X" + ".txt";

            int count = 0;
            g.date = 20180903;
            int[,] avr = new int[1000, 4];


            while (true)
            {
                int return_date = wk.directory_분전후(g.moving_reference_date, 1); // 거래익일
                if (return_date == -1)
                {
                    return;
                }
                else
                {
                    g.date = return_date;
                }

                //g.date = wk.directory_분전후(g.date, 1); // 거래익일
                if (g.date > 20190307)
                {
                    break;
                }

                string stock = "KODEX 레버리지";
                string path = @"C:\병신\분\" + g.date.ToString() + "\\" + stock + ".txt";
                if (!File.Exists(path))
                {
                    continue;
                }

                int[,] x = new int[1000, 4];
                rd.read_stock_minute(g.date, stock, x);
                if (x[0, 0] != 901)
                {
                    continue;
                }

                count++;
                for (int i = 0; i < 500; i++)
                {
                    if (x[i, 0] < 100)
                    {
                        break;
                    }

                    avr[i, 0] += x[i, 0];
                    avr[i, 1] += x[i, 1];
                }
            }

            for (int i = 0; i < 500; i++)
            {
                if (avr[i, 0] < 100)
                {
                    break;
                }

                avr[i, 0] /= count;
                avr[i, 1] /= count;
                sw.WriteLine("{0} \t{1}", avr[i, 0], avr[i, 1]);
            }
        }


        public static void testing_상한가다음날종가매수후그다음날장중수익시매도()
        {
            // Simple File Open and Write
            string path = @"C:\병신\temp.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            double open_sell = 0.0;
            double low_sell = 0.0;
            double high_sell = 0.0;
            int total_count = 0;

            foreach (string stock in g.sl)
            {
                //var lines = new List<string>(path);


                string a = @"C:\병신\data\일\" + stock + ".txt";

                if (!File.Exists(a))
                {
                    return;
                }

                List<string> lines = File.ReadLines(a).ToList();


                // Read Data File
                double[,] x = new double[1000, 10];
                int n = 0;
                foreach (string line in lines)
                {
                    string[] words = line.Split(' ');

                    for (int i = 0; i < 10; i++)
                    {
                        x[n, i] = Convert.ToDouble(words[i]);
                    }
                    n++;
                }

                for (int i = 1; i < lines.Count - 2; i++)
                {
                    if ((x[i, 4] - x[i - 1, 4]) / x[i - 1, 4] * 100 > 29.5 && // 첫날 종가 기준 상한가
                        (x[i + 1, 1] - x[i, 4]) / x[i, 4] * 100 < 29.5 && // 둘째날 시가, 고가, 저가, 종가 모두 상한가 아니고
                        (x[i + 1, 2] - x[i, 4]) / x[i, 4] * 100 < 29.5 &&
                        (x[i + 1, 3] - x[i, 4]) / x[i, 4] * 100 < 29.5 &&
                        (x[i + 1, 4] - x[i, 4]) / x[i, 4] * 100 < 29.5)
                    {
                        // sw.WriteLine("{0} \t\t{1:F1} \t\t{2:F1}", stock, (x[i + 2, 1] - x[i + 1, 4]) / x[i + 1, 4] * 100, (x[i, 4] - x[i - 1, 4]) / x[i - 1, 4] * 100);

                        open_sell += (x[i + 2, 1] - x[i + 1, 4]) / x[i + 1, 4] * 100;
                        high_sell += (x[i + 2, 2] - x[i + 1, 4]) / x[i + 1, 4] * 100;
                        low_sell += (x[i + 2, 3] - x[i + 1, 4]) / x[i + 1, 4] * 100;


                        total_count++;
                    }
                }
            }
            open_sell /= total_count; // 0.07%
            high_sell /= total_count; // 7.13%
            low_sell /= total_count;   // -5.30%
            // 결론 
            // 약한 종목은 매수하지 않고 
            // 강한 종목을 매수한다면 
            // 셋째날 수익을 볼 수도 있겠다. 
            // 그러나 약한 종목의 경우 둘째날 종가가 강할 것이고
            // 강한종목은 종가가 강할 것이므로 결론은 쉽지않다
            // 셋째날 시초가에서는 50% 정도의 확율을 가지므로 추가 해석 필요
            sw.Close();
        }




        public static void testing_양매수()
        {
            // Simple File Open and Write
            string path = @"C:\병신\temp.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);



            foreach (string stock in g.sl)
            {
                //var lines = new List<string>(path);


                string a = @"C:\병신\data\일\" + stock + ".txt";

                if (!File.Exists(a))
                {
                    return;
                }

                List<string> lines = File.ReadLines(a).ToList();


                // Read Data File
                double[,] x = new double[1000, 10];
                int n = 0;
                foreach (string line in lines)
                {
                    string[] words = line.Split(' ');

                    for (int i = 0; i < 10; i++)
                    {
                        x[n, i] = Convert.ToInt32(words[i]);
                    }
                    n++;
                }

                int total_count = 0;
                int positive_count = 0;
                double positive_average = 0.0;
                int negative_count = 0;
                double negative_average = 0.0;

                if (n < 602)
                {
                    continue;
                }

                for (int i = n - 1; i > n - 201; i--)
                {

                    if ((x[i - 1, 8] - x[i - 2, 8] > 0.0 && x[i - 1, 9] - x[i - 2, 9] > 0.0) &&
                    (x[i - 2, 8] - x[i - 3, 8] > 0.0 && x[i - 2, 9] - x[i - 3, 9] > 0.0)) // 전일 기관(8), 외인(9) 양매수
                    {

                        total_count++;
                        double percent = ((x[i, 4] - x[i - 1, 4]) / x[i, 4] * 100); // 당일 대비
                        if (percent > 0) // 당일종가 - 전일종가 > 0
                        {
                            positive_average += percent;
                            positive_count++;
                        }
                        else
                        {
                            negative_average += percent;
                            negative_count++;
                        }

                    }
                }
                if (positive_count > 0)
                {
                    positive_average /= positive_count;
                }
                else
                {
                    positive_average = 0.0;
                }

                if (negative_count > 0)
                {
                    negative_average /= negative_count;
                }
                else
                {
                    negative_average = 0.0;
                }

                sw.WriteLine("{0} \t\t{1} \t{2} \t{3:N2} \t{4} \t{5:N2}", stock, total_count, positive_count, positive_average, negative_count, negative_average);

            }
            sw.Close();

        }

        public static void testing_ETF()
        {
            string path = @"C:\병신\data\" + "일" + "\\" + "KODEX 코스닥150선물인버스" + ".txt";
            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 코스닥150레버리지" + ".txt";
            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 레버리지" + ".txt";
            //string path = @"C:\병신\" + "일" + "\\" + "KODEX 200선물인버스2X" + ".txt";

            //var lines = new List<string>(path);
            if (!File.Exists(path))
            {
                return;
            }

            List<string> lines = File.ReadLines(path).ToList();


            // Read Data File
            double[,] x = new double[1000, 10];
            int n = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                for (int i = 0; i < 10; i++)
                {
                    x[n, i] = Convert.ToInt32(words[i]);
                }
                n++;
            }

            double diff = 0;
            int count = 0;
            for (int i = 2; i < n; i++)
            {
                if (true) //x[i - 1, 4] - x[i - 1, 1] > 0.0) // && x[i - 1, 9] - x[i - 2, 9] > 0.0) // 전일 기관(8), 외인(9) 양매수
                {
                    diff += ((x[i, 1] - x[i - 1, 4]) / x[i - 1, 4] * 100);
                    count++;
                }

            }
            diff /= count;
            diff -= 0.0098104; // 수수료
            diff *= count;
            /* 
             * 지난 599일 대상으로 테스트 
             * 전날 장마감 매수 익일 시초가 매도 
             *  코스피 레버리지  30.7% 누적(599일)
             *  코스피 인버스레  -5.3% 누적(626일)
             *  코스닥 레버리지  43.0% 누적(698일)
             *  코스닥 인버스레 -42.7% 누적(599일)
             * 전 날 기관매수시 무조건 매수 및 매도 365일 20.3%
             * 전 날 기관매수시 무조건 매수 및 매도 360일 -0.9%
             * 전 날 양매수시 무조건 매수 및 매도 360일 3.9%
             * 
             * 결론 기관이 모니터링하면 매수 매도하므로 기관을 따라 매수 매도하면 년 10% 가까이 가능할 것임
             * 위에 추가하여 etf는 against wind 하지말 것
             * 
             * nav 계산하여 유리할 때 매수하는 것도 추가하면 이익 증가 가능할 것으로 생각
             * (외, 기에게 2초 advantage 주고 하는 게임)
             * */
        }
    }
}
