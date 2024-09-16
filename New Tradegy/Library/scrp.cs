using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using HtmlAgilityPack;
using New_Tradegy.Library;



namespace New_Tradegy.Library
{
    internal class sc
    {

        // row 2 kwd_usd, // row 3 kospi, kosdaq
        public static void task_usd_krw()
        {
            return;
                while (true)
                {
                    if (!wk.isWorkingHour())
                    {
                        Thread.Sleep(10 * 1000);
                        continue;
                    }

                    string url = "https://kr.investing.com/currencies/usd-krw";

                    try
                    {
                        // Set up ChromeDriver
                        var options = new ChromeOptions();
                        options.AddArgument("--headless"); // Run in headless mode
                        options.AddArgument("--disable-gpu"); // Disable GPU
                        options.AddArgument("--no-sandbox"); // Bypass OS security model
                        options.AddArgument("--disable-dev-shm-usage"); // Overcome limited resource problems

                        using (var driver = new ChromeDriver(options))
                        {
                            driver.Navigate().GoToUrl(url);
                            Thread.Sleep(5000); // Wait for the page to load

                            // Locate the element directly using Selenium
                            var element = driver.FindElement(By.CssSelector("div[data-test='instrument-price-last']"));

                            if (element != null)
                            {
                                string priceString = element.Text;
                                Console.WriteLine("Extracted price string: " + priceString); // Log extracted price string
                                if (float.TryParse(priceString, out float usdKrw))
                                {
                                    g.제어.dtb.Rows[1][1] = (int)usdKrw;
                                }
                                else
                                {
                                    // Log parsing error
                                    Console.WriteLine("Failed to parse the USD/KRW exchange rate.");
                                }
                            }
                            else
                            {
                                // Log element not found
                                Console.WriteLine("Exchange rate element not found.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log unexpected exceptions and continue
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }

                    Thread.Sleep(60 * 1000);
                }
            }







        // row 4 : Nasdaq, SP


        public static void task_us_indices_futures()
        {
            // Wait for initial delay before starting the task
            Thread.Sleep(1000 * 11);

            while (true)
            {
                // If not working hour, wait and continue
                if (!wk.isWorkingHour())
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                string url = "https://www.investing.com/indices/indices-futures";

                HtmlAgilityPack.HtmlDocument doc = null;

                // Attempt to load the document
                try
                {
                    HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                    doc = web.Load(url);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // If document is not loaded, wait and continue
                if (doc == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // Extract data from the loaded document
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table")
                    .Descendants("tr")
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .ToList();

                // If the table is empty, continue
                if (table.Count == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // Parse the data into variables
                float.TryParse(table[1][7].Trim('%'), out g.SP_지수);
                float.TryParse(table[2][7].Trim('%'), out g.Nasdaq_지수);

                // Update the global data table
                g.제어.dtb.Rows[1][2] = g.Nasdaq_지수;
                g.제어.dtb.Rows[1][3] = g.SP_지수;

                DateTime date = DateTime.Now;
                int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                // Check if the task should run
                if (HHmm < 900 || HHmm >= 1521 || date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday || g.test)
                {
                    Thread.Sleep(1000 * 30);
                    continue;
                }

                // Run the task to append or replace index data
                run_us_index_append_or_replace();

                // Wait before the next iteration
                Thread.Sleep(1000 * 30);
            }
        }

     

        // row 5 상해종합지수, 항생지수, // row 6 니케이지수 대만가권;
        public static void task_major_indices() // !!
        {
            Thread.Sleep(1000 * 12);
            while (true)
            {
                if (!wk.isWorkingHour())
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                string url = "https://www.investing.com/indices/major-indices";

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc = null;
                // doc = web.Load(url);

                try
                {
                    doc = web.Load(url); // FileNotFoundExceptions are handled here.
                }
                catch (Exception e)
                {
                    continue;
                }

                if (doc == null)
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }


                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table") //cross_rate_1']")
                    .Descendants("tr")
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .ToList();

                string t = table[32][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.상해종합지수);

                t = table[36][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.항생지수);

                t = table[29][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.니케이지수);

                float 대만가권;
                t = table[37][6].Trim(new Char[] { '%' });
                float.TryParse(t, out 대만가권);

                g.제어.dtb.Rows[2][0] = g.상해종합지수;
                g.제어.dtb.Rows[2][1] = g.항생지수;

                g.제어.dtb.Rows[2][2] = g.니케이지수;
                g.제어.dtb.Rows[2][3] = 대만가권;

                Thread.Sleep(1000 * 30);
            }
        }


        public static void task_bitcoin() // !!
        {
            Thread.Sleep(1000 * 13);
            while (true)
            {

                if (!wk.isWorkingHour())
                {
                    Thread.Sleep(1000 * 120);
                    continue;
                }

                string url = "https://www.investing.com/crypto/bitcoin";

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc = null;
                // doc = web.Load(url);

                try
                {
                    doc = web.Load(url); // FileNotFoundExceptions are handled here.
                }
                catch (Exception e)
                {
                    continue;
                }

                if (doc == null)
                {
                    Thread.Sleep(12000);
                    continue;
                }

                float bitcoin;
                var t = doc.DocumentNode.SelectSingleNode(".//span[contains(@class, 'pid-1057391-last')]").InnerHtml;
                float.TryParse(t, out bitcoin);

                g.제어.dtb.Rows[3][1] = bitcoin;

                Thread.Sleep(12000);
            }
        }


        public static void task_commodities_futures() // !!
        {

            Thread.Sleep(1000 * 14);
            while (true)
            {
                if (!wk.isWorkingHour())
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                string url = "https://www.investing.com/commodities/real-time-futures";

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc = null;
               //  doc = web.Load(url);

                try
                {
                    doc = web.Load(url); // FileNotFoundExceptions are handled here.
                }
                catch (Exception e)
                {
                    continue;
                }

                if (doc == null)
                {
                    Thread.Sleep(10000);
                    continue;
                }



                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table") //cross_rate_1']")
                    .Descendants("tr") 
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .ToList();

                float gold;
                string t = table[0][3];
                float.TryParse(t, out gold);

                float copper; // 10
                t = table[4][3];
                float.TryParse(t, out copper);

                float wti; // 8
                t = table[7][3];
                float.TryParse(t, out wti);

                float gas; // 9
                t = table[9][3];
                float.TryParse(t, out gas);

                float aluminum; // 11
                t = table[13][3];
                float.TryParse(t, out aluminum);

                float wheat; // 12
                t = table[17][3];
                float.TryParse(t, out wheat);

                float lumber; // 13
                t = table[32][3];
                float.TryParse(t, out lumber);

                int row = 9;
                //g.제어.dtb.Rows[row][0] = "WTI";
                //g.제어.dtb.Rows[row][1] = Math.Round(wti, 1);

                //g.제어.dtb.Rows[row + 1][0] = "Gas";
                //g.제어.dtb.Rows[row + 1][1] = Math.Round(gas, 2);

                //g.제어.dtb.Rows[row + 2][0] = "Cu";
                //g.제어.dtb.Rows[row + 2][1] = Math.Round(copper, 2);

                //g.제어.dtb.Rows[row + 3][0] = "Al";
                //g.제어.dtb.Rows[row + 3][1] = (int)aluminum;

                //g.제어.dtb.Rows[row + 4][0] = "Wheat";
                //g.제어.dtb.Rows[row + 4][1] = (int)wheat;




            }
        }


        public static void run_us_index_append_or_replace()
        {
            int time_now_6int = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            List<string> four_index = new List<string>();

            four_index.Add("KODEX 레버리지");
            four_index.Add("KODEX 200선물인버스2X");
            four_index.Add("KODEX 코스닥150레버리지");
            four_index.Add("KODEX 코스닥150선물인버스");

            foreach (var item in four_index)
            {
                int index = wk.return_index_of_ogldata(item);
                if (index < 0)
                { continue; }
                g.stock_data o = g.ogl_data[index];
                int time_befr_6int = o.x[o.nrow - 1, 0];
                bool append;

                // 초는 포함하지 않는 시간 비교
                if (time_now_6int / 100 != time_befr_6int / 100) // times differ or time is 859, append
                    append = true;
                else
                    append = false;

                int append_or_replace_row;
                if (append)
                    append_or_replace_row = o.nrow;
                else
                    append_or_replace_row = o.nrow - 1;

                if (append_or_replace_row >= g.MAX_ROW)
                    return;

                o.x[append_or_replace_row, 10] = (int)(g.Nasdaq_지수 * g.HUNDRED); // AAA teethed pattern
            }
        }


        public static void task_jsb()
        {
            while (true)
            {
                int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm")); // timerEvalDrawTick

                if ((HHmm % 100 == 59 || HHmm % 100 == 29) && g.alamed_hhmm != HHmm)
                {
                    ms.Sound("일반", "to jsb");
                    g.alamed_hhmm = HHmm;
                    Thread.Sleep(59 * 1000);
                }
            }
        }

        public static void task_ikl()
        {
            while (true)
            {

            }
        }
    }
}
