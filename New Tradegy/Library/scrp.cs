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
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;

using SeleniumExtras.WaitHelpers;   // For WaitHelpers



namespace New_Tradegy.Library
{
    internal class sc
    {
        // krw not resolved
        public static async Task ScrapeUSDKRW()
        {
            string url = "https://kr.investing.com/currencies/usd-krw";

            try
            {
                // Set up ChromeDriver options for headless mode
                var options = new ChromeOptions();
                options.AddArgument("--headless"); // Run Chrome in headless mode
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");

                using (var driver = new ChromeDriver(options))
                {
                    // Navigate to the target URL
                    driver.Navigate().GoToUrl(url);

                    // Wait for the page to fully load
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    // Wait for the specific element to become visible using its CSS selector
                    var element = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-test='instrument-price-last']")));

                    if (element != null)
                    {
                        // Extract the price string (e.g., "1,368.37")
                        string priceString = element.Text;

                        // Optionally remove any commas (e.g., "1,368.37" -> "1368.37")
                        priceString = priceString.Replace(",", "");

                        // Print or store the extracted value
                        Console.WriteLine("Extracted USD/KRW exchange rate: " + priceString);

                        // Try to parse the value to a float or decimal
                        if (float.TryParse(priceString, out float usdKrw))
                        {
                            Console.WriteLine($"Parsed USD/KRW exchange rate: {usdKrw}");
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse the USD/KRW exchange rate.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            await Task.Delay(1000); // Simulating an async operation
        }
        public static async Task task_usd_krw()
        {
            while (true)
            {
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(10 * 1000); // Non-blocking delay
                    continue;
                }

                string url = "https://kr.investing.com/currencies/usd-krw";

                try
                {
                    var options = new ChromeOptions();
                    options.AddArgument("--headless");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");

                    using (var driver = new ChromeDriver(options))
                    {
                        driver.Navigate().GoToUrl(url);

                        // Wait for the page to fully load
                        new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(
                            d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")
                        );

                        // Use WebDriverWait to wait for the element to load
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));  // Increased timeout

                        // Updated to use WaitHelpers from SeleniumExtras
                        var element = wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-test='instrument-price-last']"))
                        );

                        if (element != null)
                        {
                            string priceString = element.Text;
                            priceString = priceString.Replace(",", "");  // Handle commas

                            Console.WriteLine("Extracted price string: " + priceString);

                            if (float.TryParse(priceString, out float usdKrw))
                            {
                                if (g.제어.dtb.Rows[1][1].ToString() != usdKrw.ToString())
                                {
                                    g.제어.dtb.Rows[1][1] = usdKrw.ToString();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to parse the USD/KRW exchange rate.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Exchange rate element not found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                await Task.Delay(60 * 1000);  // Wait 60 seconds before the next iteration
            }
        }
        // updated on 20241020
        public static async Task task_usd_krw_new_new_old()
        {
            while (true)
            {
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(10 * 1000); // Non-blocking delay
                    continue;
                }

                string url = "https://kr.investing.com/currencies/usd-krw";

                try
                {
                    var options = new ChromeOptions();
                    options.AddArgument("--headless");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");

                    using (var driver = new ChromeDriver(options))
                    {
                        driver.Navigate().GoToUrl(url);

                        // Wait for the page to fully load
                        new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(
                            d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")
                        );

                        // Check for iframe and switch to it (if needed)
                        // driver.SwitchTo().Frame(driver.FindElement(By.CssSelector("iframe-selector"))); // Uncomment if necessary

                        // Use WebDriverWait to wait for the element to load
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));  // Increased to 60 seconds

                        // Updated to use WaitHelpers from SeleniumExtras
                        var element = wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-test='instrument-price-last']"))
                        );

                        if (element != null)
                        {
                            string priceString = element.Text;
                            priceString = priceString.Replace(",", "");  // Handle commas

                            Console.WriteLine("Extracted price string: " + priceString);

                            if (float.TryParse(priceString, out float usdKrw))
                            {
                                if (g.제어.dtb.Rows[1][1].ToString() != usdKrw.ToString())
                                {
                                    g.제어.dtb.Rows[1][1] = usdKrw.ToString();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to parse the USD/KRW exchange rate.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Exchange rate element not found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                await Task.Delay(60 * 1000);  // Wait 60 seconds before the next iteration
            }
        }
        // row 2 kwd_usd, // row 3 kospi, kosdaq
        public static void task_usd_krw_old()
        {

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
                                if (g.제어.dtb.Rows[1][1].ToString() != usdKrw.ToString())
                                {
                                    g.제어.dtb.Rows[1][1] = usdKrw.ToString();
                                }
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


        // updated on 20241020
        public static async Task task_us_indices_futures()
        {
            // Initial delay before starting the task
            //await Task.Delay(TimeSpan.FromSeconds(11));
            // Initial 15-second delay before starting
            await Task.Delay(TimeSpan.FromSeconds(15));

            while (true)
            {
                // Check if it's working hours; if not, wait for 10 seconds and continue
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));  // Replaced Task.Delay().Wait() with await Task.Delay()
                    continue;
                }

                string url = "https://www.investing.com/indices/indices-futures";
                HtmlDocument doc = null;

                try
                {
                    HtmlWeb web = new HtmlWeb();

                    // Load the page
                    doc = web.Load(url);
                    HtmlNode tableNode = doc.DocumentNode.SelectSingleNode("//table");

                    if (tableNode == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));  // Replaced Task.Delay().Wait() with await Task.Delay()
                        continue; // Retry if table is not found
                    }

                    // Extract rows directly from the table node
                    List<List<string>> table = tableNode
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();

                    if (table.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));  // Replaced Task.Delay().Wait() with await Task.Delay()
                        continue;
                    }

                    // Parse the indices data and update variables
                    if (float.TryParse(table[1][7].Trim('%'), out float SP_지수))
                        g.SP_지수 = SP_지수;

                    if (float.TryParse(table[2][7].Trim('%'), out float Nasdaq_지수))
                        g.Nasdaq_지수 = Nasdaq_지수;

                    // Update the global data table
                    if(g.제어.dtb.Rows[1][2].ToString() != g.Nasdaq_지수.ToString())
                    {
                        g.제어.dtb.Rows[1][2] = g.Nasdaq_지수.ToString();
                    }
                    if (g.제어.dtb.Rows[1][3].ToString() != g.SP_지수.ToString())
                    {
                        g.제어.dtb.Rows[1][3] = g.SP_지수.ToString();
                    }

                    DateTime date = DateTime.Now;
                    int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                    // Check if the task should be performed based on time and day
                    if (HHmm >= 900 && HHmm < 1521 && date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday && !g.test)
                    {
                        run_us_index_append_or_replace();
                    }

                    // Wait 30 seconds before the next iteration
                    await Task.Delay(TimeSpan.FromSeconds(30));  // Replaced Task.Delay().Wait() with await Task.Delay()
                }
                catch (Exception)
                {
                    // Retry after a delay if an exception occurs
                    await Task.Delay(TimeSpan.FromSeconds(1));  // Replaced Task.Delay().Wait() with await Task.Delay()
                }
            }
        }


        // updated on 20241020
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


        // updated on 20241020
        public static async Task task_major_indices()
        {
            // Initial delay before starting (12 seconds)
            await Task.Delay(1000 * 12);  // Replaced Thread.Sleep with await Task.Delay for non-blocking delay

            while (true)
            {
                // Check if it's working hours; if not, wait 10 seconds and continue
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(1000 * 10);  // Replaced Thread.Sleep with await Task.Delay
                    continue;
                }

                string url = "https://www.investing.com/indices/major-indices";
                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = null;

                try
                {
                    // Load the page asynchronously
                    doc = web.Load(url);  // HtmlAgilityPack does not support async loading directly, so this remains synchronous
                }
                catch (Exception e)
                {
                    // If loading fails, log the exception and continue (retry after 10 seconds)
                    Console.WriteLine($"Exception occurred: {e.Message}");
                    await Task.Delay(1000 * 10);  // Non-blocking delay before retry
                    continue;
                }

                if (doc == null)
                {
                    await Task.Delay(1000 * 10);  // Non-blocking delay before retry
                    continue;
                }

                // Extract rows directly from the table node
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table")
                    .Descendants("tr")
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .ToList();

                // Parse and update indices
                string t = table[32][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.상해종합지수);

                t = table[36][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.항생지수);

                t = table[29][6].Trim(new Char[] { '%' });
                float.TryParse(t, out g.니케이지수);

                float 대만가권;
                t = table[37][6].Trim(new Char[] { '%' });
                float.TryParse(t, out 대만가권);

                // Update global data table
                if (g.제어.dtb.Rows[2][0].ToString() != g.상해종합지수.ToString())
                {
                    g.제어.dtb.Rows[2][0] = g.상해종합지수.ToString();
                }

                if (g.제어.dtb.Rows[2][1].ToString() != g.항생지수.ToString())
                {
                    g.제어.dtb.Rows[2][1] = g.항생지수.ToString();
                }

                if (g.제어.dtb.Rows[2][2].ToString() != g.니케이지수.ToString())
                {
                    g.제어.dtb.Rows[2][2] = g.니케이지수.ToString();
                }

                if (g.제어.dtb.Rows[2][3].ToString() != 대만가권.ToString())
                {
                    g.제어.dtb.Rows[2][3] = 대만가권.ToString();
                }
                
                // Wait for 30 seconds before the next iteration
                await Task.Delay(1000 * 30);  // Non-blocking delay
            }
        }


        // updated on 20241020
        public static async Task task_bitcoin()
        {
            // Initial delay before starting (13 seconds)
            await Task.Delay(1000 * 13);  // Replaced Thread.Sleep with await Task.Delay for non-blocking delay

            while (true)
            {
                // Check if it's working hours; if not, wait for 120 seconds and continue
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(1000 * 120);  // Replaced Thread.Sleep with await Task.Delay
                    continue;
                }

                string url = "https://www.investing.com/crypto/bitcoin";

                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = null;

                try
                {
                    // Load the page synchronously (HtmlAgilityPack doesn't support async loading)
                    doc = web.Load(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred: {e.Message}");
                    await Task.Delay(12000);  // Non-blocking delay before retry
                    continue;
                }

                if (doc == null)
                {
                    await Task.Delay(12000);  // Non-blocking delay before retry
                    continue;
                }

                // Extract the Bitcoin price from the page
                float bitcoin;
                var t = doc.DocumentNode.SelectSingleNode(".//span[contains(@class, 'pid-1057391-last')]").InnerHtml;
                float.TryParse(t, out bitcoin);

                // Update global data table
                if(g.제어.dtb.Rows[3][1].ToString() != bitcoin.ToString())
                {
                    g.제어.dtb.Rows[3][1] = bitcoin.ToString();
                }

                // Wait for 12 seconds before the next iteration
                await Task.Delay(12000);  // Non-blocking delay
            }
        }


        // updated on 20241020
        public static async Task task_commodities_futures()
        {
            // Initial delay before starting (14 seconds)
            await Task.Delay(1000 * 14);  // Replaced Thread.Sleep with await Task.Delay for non-blocking delay

            while (true)
            {
                // Check if it's working hours; if not, wait for 10 seconds and continue
                if (!wk.isWorkingHour())
                {
                    await Task.Delay(1000 * 10);  // Replaced Thread.Sleep with await Task.Delay
                    continue;
                }

                string url = "https://www.investing.com/commodities/real-time-futures";
                HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = null;

                try
                {
                    // Load the page synchronously (HtmlAgilityPack doesn't support async loading)
                    doc = web.Load(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred: {e.Message}");
                    await Task.Delay(10000);  // Non-blocking delay before retry
                    continue;
                }

                if (doc == null)
                {
                    await Task.Delay(10000);  // Non-blocking delay before retry
                    continue;
                }

                // Extract data from the table
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table")
                    .Descendants("tr")
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                    .ToList();

                // Parse the commodities data
                float gold;
                string t = table[0][3];
                float.TryParse(t, out gold);

                float copper;
                t = table[4][3];
                float.TryParse(t, out copper);

                float wti;
                t = table[7][3];
                float.TryParse(t, out wti);

                float gas;
                t = table[9][3];
                float.TryParse(t, out gas);

                float aluminum;
                t = table[13][3];
                float.TryParse(t, out aluminum);

                float wheat;
                t = table[17][3];
                float.TryParse(t, out wheat);

                float lumber;
                t = table[32][3];
                float.TryParse(t, out lumber);

                // Wait for a period before running the loop again (you can adjust this delay)
                await Task.Delay(10000);  // Non-blocking delay before the next loop iteration
            }
        }


        public static async Task task_jsb()
        {
            while (true)
            {
                // Get the current time in HHmm format
                int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));

                // Check if the current time is at 59 or 29 minutes past the hour, and if it hasn't been alerted yet
                if ((HHmm % 100 == 59 || HHmm % 100 == 29) && g.alamed_hhmm != HHmm)
                {
                    // Play sound and update the last alerted time
                    mc.Sound("일반", "to jsb");
                    g.alamed_hhmm = HHmm;

                    // Wait asynchronously for 59 seconds before checking again
                    await Task.Delay(59 * 1000);
                }

                // Small delay to prevent excessive CPU usage in the loop
                await Task.Delay(1000); // Wait 1 second before checking the time again
            }
        }



        public static async Task task_ikl()
        {
            while (true)
            {
                // Example condition: Exit the loop if some condition is met
                if (SomeExitConditionMet())
                {
                    break;
                }

                // Perform some task here, such as calling a method or checking a condition

                await Task.Delay(1000); // Delay to prevent CPU overuse
            }
        }

        // Example of a method for checking a condition
        private static bool SomeExitConditionMet()
        {
            // Check some condition to determine whether to exit the loop
            return false; // Placeholder: return true when the condition is met
        }

    }
}
