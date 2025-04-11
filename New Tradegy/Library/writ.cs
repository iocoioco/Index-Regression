using New_Tradegy.Library.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{
    internal class wr
    {
        private static async Task LogErrorAsync(Exception ex, string dataGridViewName)
        {
            string logDirectory = @"C:\병신\Logs";
            string logFileName = "DataGridErrors_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] An error occurred in {dataGridViewName}: {ex.Message}");
                await writer.WriteLineAsync(ex.StackTrace);
            }
        }
        public static void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e, string dataGridViewName)
        {
            LogErrorAsync(e.Exception, dataGridViewName).ConfigureAwait(false);
            e.ThrowException = false;
        }


        // modified by Sensei
        public static async Task SaveAllStocks()
        {
            if (g.test)
                return; // ❌ Don't save during test mode

            rd.read_write_kodex_magnifier("write");

            string directory = $@"C:\병신\분\{g.date}";
            Directory.CreateDirectory(directory);

            foreach (var t in StockRepository.Instance.AllDatas)
            {
                string file = Path.Combine(directory, t.Stock + ".txt");

                if (File.Exists(file))
                    File.Delete(file); // clean slate

                StringBuilder sb = new StringBuilder();
                int lastRow = 381;
                int lastColumn = 11;

                for (int j = 0; j <= lastRow; j++)
                {
                    if (t.API.x[j, 0] == 0 || t.API.x[j, 0] > 152100)
                        break;

                    for (int k = 0; k <= lastColumn; k++)
                    {
                        if (k > 0)
                            sb.Append('\t');
                        sb.Append(t.API.x[j, k]);
                    }

                    sb.AppendLine();
                }

                File.WriteAllText(file, sb.ToString());
            }

            mc.Sound("일반", "done");
            await Task.CompletedTask; // async compliance
        }




        public static void append(string path, string t)
        {
            if (File.Exists(path))
                create_empty_file(path);

            StreamWriter sw = File.AppendText(path);

            sw.WriteLine("{0}", t);
            sw.Close();
        }

        public static void overWrite(string path, string t)
        {
            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(path, t, Encoding.Default);
        }

        public static void create_empty_file(string path)
        {
            if (File.Exists(path))
                return;

            if (!File.Exists(path))
                File.Create(path).Dispose();
        }

        public static void w(string path, double[] a)
        {
            lock (g.lockObject)
            {

                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in a)
                {
                    str += "\t" + t.ToString();
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void w(string path, List<List<double>> double_list)
        {
            lock (g.lockObject)
            {

                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in double_list)
                {
                    str += t[0] + "/" + t[1] + "\t";
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void w(string path, string t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}", t);
                sw.Close();
            }
        }

        public static void w(string path, string t0, string t1)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}", t0, t1);
                sw.Close();
            }
        }

        public static void w(string path, string t0, string t1, string t2)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}", t0, t1, t2);
                sw.Close();
            }
        }

        public static void w(string path, string t0, string t1, string t2, string t3)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", t0, t1, t2, t3);
                sw.Close();
            }
        }

        public static void w(string path, string t0, string t1, string t2, string t3, string t4)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", t0, t1, t2, t3, t4);
                sw.Close();
            }
        }

        public static void w(string path, string[] t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                // sw.WriteLine(time_now);
                for (int i = 0; i < t.Length; i++)
                {
                    sw.Write(t[i]);
                    sw.Write("\t");
                    //if(i < t.Length - 1)
                    //    sw.Write("\t");
                    //else
                    //    sw.Write("\n");
                }
                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Write("\n");
                sw.Close();
            }
        }

        public static void w(string path, List<string> GL_title, List<List<string>> GL)
        {
            lock (g.lockObject)
            {

                StreamWriter sw = File.AppendText(path);

                string str = "";
                for (int i = 0; i < GL.Count; i++)
                {
                    str += GL_title[i];
                    foreach (var u in GL[i])
                    {
                        str += "\t" + u + "\n";
                    }
                    str += "\n";
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void w(string path, int[,] x, int start_line, int end_line)
        {
            lock (g.lockObject)
            {

                StreamWriter sw = File.AppendText(path);

                for (int i = start_line; i <= end_line; i++)
                {
                    if (i < 0)
                    {
                        break;
                    }
                    for (int j = 0; j < 12; j++)
                    {
                        sw.Write("{0, 10}", x[i, j]);
                    }
                    sw.Write("\n");
                }
                sw.Close();
            }
        }

        public static void wt(string path, string t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);


                sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Close();
            }
        }

        public static void wt(string path, string[] t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");

                StreamWriter sw = File.AppendText(path);

                sw.WriteLine(time_now);
                for (int i = 0; i < t.Length; i++)
                {
                    sw.WriteLine(t[i]);
                }
                sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Close();
            }
        }




        public static void create_empty_temp_file()
        {
            string path = @"C:\병신\temp.txt";

            if (File.Exists(path))
                File.Delete(path);

            if (!File.Exists(path))
                File.Create(path).Dispose();
        }

        public static void wt(double[] a)
        {
            lock (g.lockObject)
            {
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in a)
                {
                    str += "\t" + t.ToString();
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void wt(List<List<double>> double_list)
        {
            lock (g.lockObject)
            {
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in double_list)
                {
                    str += t[0] + "/" + t[1] + "\t";
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void wt(string t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp_7221_test.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}", t);
                sw.Close();
            }
        }

        
        public static void w_temp_7221_test(string t)
        {
            lock (g.lockObject)
            {
                string path = @"C:\병신\temp_7221_test.txt";
                StreamWriter sw = File.AppendText(path);

                sw.WriteLine("{0}", t);
                sw.Close();
            }
        }

        
        public static void wt_temp_MarketeyeCount(string t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp_MarketeyeCount.txt";
                StreamWriter sw = File.AppendText(path);

                sw.WriteLine("{0}\t{1}", time_now, t);
                sw.Close();
            }
        }

       
        public static void wt_7222_count(string t0, string t1, string t2)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp_7222_count.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", time_now, t0, t1, t2);
                sw.Close();
            }
        }

        public static void wt(string t0, string t1, string t2)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}", t0, t1, t2);
                sw.Close();
            }
        }

        public static void wt(string t0, string t1, string t2, string t3)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", t0, t1, t2, t3);
                sw.Close();
            }
        }

        public static void wt(string t0, string t1, string t2, string t3, string t4)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", t0, t1, t2, t3, t4);
                sw.Close();
            }
        }

        public static void wt(string[] t)
        {
            lock (g.lockObject)
            {
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                // sw.WriteLine(time_now);
                for (int i = 0; i < t.Length; i++)
                {
                    sw.Write(t[i]);
                    sw.Write("\t");
                    //if(i < t.Length - 1)
                    //    sw.Write("\t");
                    //else
                    //    sw.Write("\n");
                }
                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Write("\n");
                sw.Close();
            }
        }

        public static void wt(List<string> GL_title, List<List<string>> GL)
        {
            lock (g.lockObject)
            {
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                for (int i = 0; i < GL.Count; i++)
                {
                    str += GL_title[i];
                    foreach (var u in GL[i])
                    {
                        str += "\t" + u + "\n";
                    }
                    str += "\n";
                }

                sw.WriteLine("{0}", str);
                sw.Close();
            }
        }

        public static void wt(int[,] x, int start_line, int end_line)
        {
            lock (g.lockObject)
            {
                string path = @"C:\병신\temp.txt";
                StreamWriter sw = File.AppendText(path);

                for (int i = start_line; i <= end_line; i++)
                {
                    if (i < 0)
                    {
                        break;
                    }
                    for (int j = 0; j < 12; j++)
                    {
                        sw.Write("{0, 10}", x[i, j]);
                    }
                    sw.Write("\n");
                }
                sw.Close();
            }
        }






        //public static void SaveAllStocks()
        //{
        //    if (!g.connected) // never save in test
        //        return;

        //    string path = @"C:\병신\분" + "\\" + g.date.ToString();
        //    Directory.CreateDirectory(path); //if Exist, will pass

        //    for (int i = 0; i < g.ogl_data.Count; i++)
        //    {
        //        g.stock_data t = g.ogl_data[i];

        //        string file = path + "\\" + t.종목 + ".txt";
        //        if (File.Exists(file)) // if file not exist, create new
        //        {
        //            File.Delete(file);
        //        }
        //        int lastRow = 381;      // int lastRow = x.GetUpperBound(0);
        //        int lastColumn = 11;   //  int lastColumn = x.GetUpperBound(1);

        //        string str = "";
        //        for (int j = 0; j <= lastRow; j++) // bound exist not the size
        //        {
        //            if (t.x[j, 0] == 0 || t.x[j, 0] > 152100) // if time is not set then stop writing
        //                break;

        //            for (int k = 0; k <= lastColumn; k++) // bound exist not the size
        //            {
        //                if (k == 0)
        //                    str += t.x[j, k];

        //                else
        //                    str += "\t" + t.x[j, k];
        //            }
        //            str += "\n";
        //        }
        //        File.WriteAllText(file, str);
        //    }

        //}

    }
}
