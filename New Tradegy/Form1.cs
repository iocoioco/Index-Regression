using CPSYSDIBLib;
using New_Tradegy.Library;
using New_Tradegy.Library.Core;
using New_Tradegy.Library.Deals;
using New_Tradegy.Library.IO;
using New_Tradegy.Library.Listeners;
using New_Tradegy.Library.Models;
using New_Tradegy.Library.Trackers;
using New_Tradegy.Library.UI.ChartClickHandlers;
using New_Tradegy.Library.UI.KeyBindings;
using New_Tradegy.Library.Utils;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using New_Tradegy.Library.Trackers.Charting;
using System.Data;

namespace New_Tradegy // added for test on 20241020 0300
{

    public partial class Form1 : Form
    {
        private static CPUTILLib.CpCybos _cpcybos;
        private static CPSYSDIBLib.CpSvrNew7222 _cpsvrnew7222;
        private DSCBO1Lib.CpSvr8091S _cpsvr8091s;

        private System.Timers.Timer _timerConnection;
        private int _timerCount;

        private PingAndSpeedMonitor networkMonitor;
        public static Form1 Instance { get; private set; } // Form1.Instance.SomeMethod();

        public Form1()
        {
            InitializeComponent();
            Instance = this;

            string path = @"C:\병신\temp.txt";

            if (File.Exists(path))
                File.Delete(path);

            if (!File.Exists(path))
                File.Create(path).Dispose();

            //ms.Speech("testing");
            //ts.지수합계점검();
            //return;

            SoundUtils.Sound("일반", "by 2032");
        }

        private void StartNetworkMonitor()
        {
            networkMonitor = new PingAndSpeedMonitor(
            host: "daishin.co.kr",
            logFilePath: "@\"C:\\병신\\ping_log.txt",
            wavFilePath: @"Resources\alert.wav",
            logIntervalSeconds: 600,         // log every 10 min
            pingThresholdMs: 300,            // warn if ping > 300ms
            speedThresholdMbps: 10.0);       // warn if speed < 10 Mbps

            networkMonitor.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _cpcybos = new CPUTILLib.CpCybos();
            _cpcybos.OnDisconnect += CpCybos_OnDisconnect;

            if (_cpcybos.IsConnect == 0) // 0 : not connected
            {
                ChangeMainTitleConnection(); // repeatedly reconnect, but currently double slashed
            }
            else
                g.connected = true;

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
            g.screenWidth = workingRectangle.Width;
            g.screenHeight = workingRectangle.Height;

            chart1.Size = new Size(workingRectangle.Width, (int)(workingRectangle.Height) - 20);
            if (Environment.MachineName == "HP")
            {
                this.Location = new Point(0, 0);
                chart1.Location = new Point(0, 0);
            }
            else
            {
                chart1.Location = new Point(0, 0);
            }


            this.Controls.Add(this.chart1);
            this.chart1.SendToBack();

            this.WindowState = FormWindowState.Maximized; // this.WindowState

            this.Text = g.v.MainChartDisplayMode; // 시초에는 푀분

            g.chart1 = chart1;
            g.BookBidManager = new BookBidManager();
            g.ChartManager = new ChartManager();
            g.ChartManager.SetChart1(chart1);

            g.StockRepository = StockRepository.Instance;
            g.StockManager = new StockManager(g.StockRepository);





            g.Npts[0] = 0; //
            g.Npts[1] = g.MAX_ROW; //

            g.q = "o&s";

            g.gid = 0;
            g.Gid = 0;

            FileIn.read_제어();

            this.FormClosing += Form1_FormClosing;
            StartNetworkMonitor();

            FileIn.read_삼성_코스피_코스닥_전체종목();  // duration 0.001 seconds
            var NaverList = FileIn.read_그룹_네이버_업종();



            g.StockManager.AddIfMissing(new[] {
                "KODEX 레버리지",
                "KODEX 코스닥150레버리지",
                "KODEX 200선물인버스2X",
                "KODEX 코스닥150선물인버스"
            });

            g.StockManager.AddIfMissing(g.kospi_mixed.stock);
            g.StockManager.AddIfMissing(g.kosdaq_mixed.stock);
            g.StockManager.AddIfMissing(NaverList);



            KeyBindingRegistrar.RegisterAll();








            FileIn.read_변수(); //
            FileIn.read_무게(); //


            FileIn.gen_ogl_data(); // duration : 1051 stocks : 11.8 seconds

            g.GroupManager = new GroupManager(); // read 상관 inside
            var groups = g.GroupManager.GetAll();
            GroupRepository.SaveFilteredGroups(groups, "C:\\병신\\data\\상관_결과.txt"); // check 

            GroupManager.gen_oGL_data(); // generate oGL_data

            FileIn.read_or_set_stocks(); // duration : 0.36 seconds
            FileIn.read_파일관심종목(); // duration 0.000 seconds





            string newdirectory = @"C:\병신\변곡\" + g.date.ToString(); // for writing 변곡 not used in marketeye_received
            Directory.CreateDirectory(newdirectory); // testing





            var controlDgv = new DataGridView();
            var controlDtb = new DataTable();
            this.Controls.Add(controlDgv); // ✅ added to Form
            g.controlPane = new ControlPane(controlDgv, controlDtb); // logic wrapper
            controlDgv.BringToFront(); // placed under chart without it

            var tradeDgv = new DataGridView();
            var tradeDtb = new DataTable();
            this.Controls.Add(tradeDgv); // ✅ added to Form
            g.tradePane = new TradePane(tradeDgv, tradeDtb); // logic wrapper
            tradeDgv.BringToFront(); // placed under chart without it



            if (!g.test && g.connected) // for market trading
            {
                OrderItemCybosListener.Init_CpConclusion();

                DealManager.DealProcessing();
                DealManager.DealHold(); // Initialize g.StockManager.HoldingList
                DealManager.DealDeposit(); // button1 tr(1)
                // subscribe_8091S(); 회원사별 종목 매수현황

                // updated on 20241020 0300
                Task Task_marketeye = Task.Run(async () => await MarketEyeBatchDownloader.StartDownloaderAsync());

                // updated on 20241020 0300
                Task task_us_indices_futures = Task.Run(async () =>
                {
                    await Scraper.task_us_indices_futures();
                });

                // updated on 20241020 0300
                Task Task_major_indices = Task.Run(async () =>
                {
                    await Scraper.task_major_indices();
                });

                // updated on 20241020 0300
                Task taskKOSPIUpdater = Task.Run(async () => await runKOSPIUpdater());
                Task taskKOSDAQUpdater = Task.Run(async () => await runKOSDAQUpdater());
            }

            // how to call // 🔔 Notify listeners
            // StockManagerEvents.NotifyChanged();
            StockManagerEvents.ListsChanged += () =>
            {
                if (Form1.Instance.InvokeRequired)
                {
                    Form1.Instance.Invoke((MethodInvoker)(() => g.ChartMain.RefreshMainChart()));
                }
                else
                {
                    g.ChartMain.RefreshMainChart();
                }
            };



            // use Panel in RankLogic
            // RankLogic.EvalStock(); // duration : 0.025 ~ 0.054 seconds

            g.ChartMain = new ChartMain(); // all new, Form_1 start
            g.ChartMain.RefreshMainChart();

            Form Form_보조_차트 = new Form_보조_차트();
            Form_보조_차트.Show(); // second chart


            // updated on 20241020 0300
            Task taskJsb = Task.Run(async () => await Scraper.task_jsb());
            SoundUtils.Sound("일반", "to jsb");

            return;
        }



        // it runs automatically, when keys in
        // msg: Windows message (low-level OS event info).
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // KeyBindingManager.TryHandle encapsulates logics for what to do with certain keys.
            if (KeyBindingManager.TryHandle(keyData))
                return true;

            // custom logic doesn't handle
            return base.ProcessCmdKey(ref msg, keyData);
        }

        static async Task runKOSPIUpdater()
        {
            KOSPIUpdater updater = new KOSPIUpdater();
            await updater.StartAsync();
            updater.Stop();
        }

        static async Task runKOSDAQUpdater()
        {
            KOSDAQUpdater updater = new KOSDAQUpdater();
            await updater.StartAsync();
            updater.Stop();
        }

        public class KOSPIUpdater
        {
            //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private CpSvrNew7222 _cpsvrnew7222;
            private CancellationTokenSource _cancellationTokenSource;

            public KOSPIUpdater()
            {
                _cpsvrnew7222 = new CpSvrNew7222();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            public async Task StartAsync()
            {
                //Logger.Info("Starting KOSPI updater...");
                await RunPeriodicTask(_cancellationTokenSource.Token);
            }

            public void Stop()
            {
                //Logger.Info("Stopping KOSPI updater...");
                _cancellationTokenSource.Cancel();
                //LogManager.Shutdown();
            }

            private async Task RunPeriodicTask(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));

                    if (HHmm >= 0700 && HHmm <= 1530)
                    {
                        //Logger.Info("Attempting to fetch KOSPI data...");

                        _cpsvrnew7222.SetInputValue(0, 'B');
                        _cpsvrnew7222.SetInputValue(1, 0);
                        _cpsvrnew7222.SetInputValue(2, '1');
                        _cpsvrnew7222.SetInputValue(4, '2');

                        if (_cpsvrnew7222.GetDibStatus() == 1)
                        {
                            //Logger.Error("GetDibStatus returned an error");
                            continue;
                        }

                        int retryCount = 0;
                        bool success = false;

                        while (retryCount < 3 && !success)
                        {
                            if (_cpsvrnew7222.BlockRequest() == 0)
                            {
                                //데이터 저장 편의로 marketeye_Recevied에서 코스피개인매수액을 당일외인순매수량 컬럼에 저장
                                MajorIndex.Instance.KospiRetailNetBuy = (int)(_cpsvrnew7222.GetDataValue(1, 0) / g.HUNDRED); // 억 단위로 변환
                                MajorIndex.Instance.KospiInstitutionNetBuy = (int)(_cpsvrnew7222.GetDataValue(3, 0) / g.HUNDRED);
                                MajorIndex.Instance.KospiInvestmentNetBuy = (int)(_cpsvrnew7222.GetDataValue(4, 0) / g.HUNDRED);
                                MajorIndex.Instance.KospiPensionNetBuy = (int)(_cpsvrnew7222.GetDataValue(9, 0) / g.HUNDRED);
                                //int KOSPI = (int)_cpsvrnew7222.GetDataValue(1, 0);
                                //Logger.Info($"KOSPI: {KOSPI}");
                                success = true;
                            }
                            else
                            {
                                retryCount++;
                                //Logger.Warn($"BlockRequest failed. Retrying {retryCount}/3...");
                                await Task.Delay(5000, cancellationToken); // Wait 5 seconds before retrying
                            }
                        }

                        if (!success)
                        {
                            //Logger.Error("Failed to fetch KOSPI data after 3 retries.");
                        }
                    }

                    // Wait for 15 seconds before the next iteration
                    await Task.Delay(15000, cancellationToken);
                }
            }
        }

        public class KOSDAQUpdater
        {
            //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private CpSvrNew7222 _cpsvrnew7222;
            private CancellationTokenSource _cancellationTokenSource;

            public KOSDAQUpdater()
            {
                _cpsvrnew7222 = new CpSvrNew7222();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            public async Task StartAsync()
            {
                //Logger.Info("Starting KOSDAQ updater...");
                await RunPeriodicTask(_cancellationTokenSource.Token);
            }

            public void Stop()
            {
                //Logger.Info("Stopping KOSDAQ updater...");
                _cancellationTokenSource.Cancel();
                //LogManager.Shutdown();
            }

            private async Task RunPeriodicTask(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));

                        if (HHmm >= 0700 && HHmm <= 1530)
                        {
                            //Logger.Info("Attempting to fetch KOSDAQ data...");

                            _cpsvrnew7222.SetInputValue(0, 'C');
                            _cpsvrnew7222.SetInputValue(1, 0);
                            _cpsvrnew7222.SetInputValue(2, '1');
                            _cpsvrnew7222.SetInputValue(4, '2');

                            if (_cpsvrnew7222.GetDibStatus() == 1)
                            {
                                //Logger.Error("GetDibStatus returned an error");
                                continue;
                            }

                            int retryCount = 0;
                            bool success = false;

                            while (retryCount < 3 && !success)
                            {
                                if (_cpsvrnew7222.BlockRequest() == 0)
                                {
                                    MajorIndex.Instance.KosdaqRetailNetBuy = (int)(_cpsvrnew7222.GetDataValue(1, 0) / g.HUNDRED); // 억 단위로 변환
                                    //MarketData.Instance.KosdaqForeignNetBuy = (int)(_cpsvrnew7222.GetDataValue(2, 0) / g.HUNDRED); //
                                    MajorIndex.Instance.KosdaqInstitutionNetBuy = (int)(_cpsvrnew7222.GetDataValue(3, 0) / g.HUNDRED); //
                                    MajorIndex.Instance.KosdaqInvestmentNetBuy = (int)(_cpsvrnew7222.GetDataValue(4, 0) / g.HUNDRED); //
                                    MajorIndex.Instance.KosdaqPensionNetBuy = (int)(_cpsvrnew7222.GetDataValue(9, 0) / g.HUNDRED); //
                                    //int KOSDAQ = (int)_cpsvrnew7222.GetDataValue(1, 0);
                                    //Logger.Info($"KOSDAQ: {KOSDAQ}");
                                    success = true;
                                }
                                else
                                {
                                    retryCount++;
                                    //Logger.Warn($"BlockRequest failed. Retrying {retryCount}/3...");
                                    await Task.Delay(5000, cancellationToken); // Wait 5 seconds before retrying
                                }
                            }

                            if (!success)
                            {
                                //Logger.Error("Failed to fetch KOSDAQ data after 3 retries.");
                            }
                        }
                    }
                    catch (COMException comEx)
                    {
                        //Logger.Error(comEx, "COM error occurred while fetching KOSDAQ data.");
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error(ex, "An error occurred while fetching KOSDAQ data.");
                    }

                    // Wait for 15 seconds before the next iteration
                    await Task.Delay(15000, cancellationToken);
                }
            }
        }


        public static int GetRemainRQ()
        {
            if (_cpcybos == null)
                return 60;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_NONTRADE_REQUEST);  // 60건/15초
        }

        public static int GetRemainTR()
        {
            if (_cpcybos == null)
                return 20;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_TRADE_REQUEST);         // 20건/15초
        }

        public static int GetRemainSB()
        {
            if (_cpcybos == null)
                return 400;
            return _cpcybos.GetLimitRemainCount(CPUTILLib.LIMIT_TYPE.LT_SUBSCRIBE);               // 400건의 요청으로 제한   
        }

        public static void task_RQTRSB()
        {
            while (true)
            {
                if (!wk.isWorkingHour())
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                if (GetRemainRQ() == 0)
                {
                    SoundUtils.Sound("일반", "no request");
                }
                if (GetRemainTR() == 0)
                {
                    SoundUtils.Sound("일반", "no trade");
                }
                if (GetRemainSB() == 0)
                {
                    SoundUtils.Sound("일반", "no subscribe");
                }
                Thread.Sleep(1000);
            }
        }

        private bool IsMarketOpen()
        {
            int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            string day = DateTime.Today.DayOfWeek.ToString();

            return (HHmm >= 800 && HHmm <= 1530) && (day != "Sunday" && day != "Saturday");
        }

        //        안녕하세요.Plus 담당자입니다.
        //1초 마다 다운로드하게 한다는 말이 1초마다 BlockRequest() 를 요청한다는 의미인지요?
        //조회시에는[시간대별 투자자매매추이] CpSysDib.CpSvrNew7222를 사용하고
        //그 이후로는 이에 매핑되는 실시간API가 있는지를  찾아서 실시간 업데이트 로직으로 구현함이 맞아보입니다.
        //장 시작되고 ( 9시부터)  요청 주기를 길게 해주거나(30초나 1분 등) 실시간 API를 이용한 로직을 이용해주시길 바랍니다.
        //코스피 매수액(개인, 외인, 기관)

        private void subscribe_8091S()
        {

            _cpsvr8091s = new DSCBO1Lib.CpSvr8091S();
            _cpsvr8091s.Received += new DSCBO1Lib._IDibEvents_ReceivedEventHandler(_cpsvr8091s_Received);

            _cpsvr8091s.SetInputValue(0, "888"); //회원사 코드(외국계 전체는 888,회원사 전체 "*", 044 메릴린치, 042 CS)
            _cpsvr8091s.SetInputValue(1, "*"); //종목 코드 [전체 종목에 대한 요청은 "*"]

            _cpsvr8091s.Subscribe();
        }

        /// <summary>
        /// not used also blocked on 20250504
        /// </summary>
        private void _cpsvr8091s_Received()
        {
            //short 시간 = _cpsvr8091s.GetHeaderValue(0);
            //string 회원사명 = _cpsvr8091s.GetHeaderValue(1);
            //string 종목코드 = _cpsvr8091s.GetHeaderValue(2);

            //char 매수매도구분 = (char)_cpsvr8091s.GetHeaderValue(4);
            //long 매수매도량 = _cpsvr8091s.GetHeaderValue(5);
            //long 순매수 = _cpsvr8091s.GetHeaderValue(6);
            //char 순매수부호 = (char)_cpsvr8091s.GetHeaderValue(7);
            //string 종목 = _cpsvr8091s.GetHeaderValue(3);
            //long 외국계순매수량 = _cpsvr8091s.GetHeaderValue(8);

            //int index = g.ogl_data.FindIndex(x => x.stock == 종목);
            //if (index < 0)
            //    return;

            //g.stock_data p = g.ogl_data[index];

            //if (!g.StockManager.IndexList.Contains(종목)) // KODEX 종목 제외
            //{
            //    p.x[p.nrow - 1, 5] = (int)외국계순매수량;
            //    p.당일외인순매수량 = (int)외국계순매수량;
            //}
        }







        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            string selection = "";



            int row_id = 0, col_id = 0;
            //double row_percentage_below_xlabel_line = 0.0;

            // from cliccked (either left or right) setting_d
            // and xval, yval ->
            // changed selection and xval, yval, percentage, id are returned
            // g.clickedStock defined in this routine also
            // g.date set to the last date(last row and last column) in the drawn chart
            var DisplayList = g.ChartMain.DisplayList;
            g.clickedStock = ChartClickMapper.CoordinateMapping(chart1, g.nRow, g.nCol, DisplayList, e, ref selection, ref col_id, ref row_id);
            if (g.clickedStock == null)
            {
                return;
            }

            if (Control.ModifierKeys == Keys.Control)
            {
                ChartClickHandler.HandleControlClick(chart1, selection, row_id, col_id);
            }
            else
            {
                ChartClickHandler.HandleClick(chart1, selection, row_id, col_id);
            }

            //SetFocusAndReturn();
        }



        private void ChangeMainTitleConnection()
        {
            _cpcybos = null;
            _cpcybos = new CPUTILLib.CpCybos();

            if (_cpcybos.IsConnect == 1)
            {
                if (_timerConnection != null)
                {
                    _timerConnection.Stop();
                    _timerConnection.Dispose();
                    _timerConnection = null;
                }

                _timerCount = 0;

                // menuStrip1.BackColor = Color.FromArgb(228, 254, 226);

                Invoke(new MethodInvoker(ConnectionCompleted));

                MessageBox.Show("대신증권 플러스에 연결되었습니다.");

            }
            else
            {
                this.Text = "대신증권 플러스 Sample for C# (연결 안됨)";

                if (_timerCount == 0)
                {
                    //DialogConnection dialog = new DialogConnection();
                    //dialog.SetParent(this);
                    //dialog.ShowDialog(this);
                }
            }
        }

        public void ConnectionCompleted()
        {
            this.Text = "대신증권 플러스 Sample for C# (연결 완료)";
        }

        private static void CpCybos_OnDisconnect()
        {
            _cpcybos = null;
            MessageBox.Show("대신증권 플러스 연결이 종료되었습니다.");
        }





        private void timer_코스피_코스닥_Tick(object sender, EventArgs e)
        {
            _cpsvrnew7222 = new CPSYSDIBLib.CpSvrNew7222();

            if (wk.isWorkingHour())
            {
                _cpsvrnew7222.SetInputValue(0, 'B'); // 코스피
                _cpsvrnew7222.SetInputValue(1, 0); // 전체
                _cpsvrnew7222.SetInputValue(2, '1'); // 누적
                _cpsvrnew7222.SetInputValue(4, '2'); // 금액

                if (_cpsvrnew7222.GetDibStatus() != 1)
                {
                    if (_cpsvrnew7222.BlockRequest() == 0)
                    {
                        //데이터 저장 편의로 marketeye_Recevied에서 코스피개인매수액을 당일외인순매수량 컬럼에 저장
                        MajorIndex.Instance.KospiRetailNetBuy = (int)(_cpsvrnew7222.GetDataValue(1, 0) / g.HUNDRED); // 억 단위로 변환
                        // MarketData.Instance.KospiForeignNetBuy = (int)(_cpsvrnew7222.GetDataValue(2, 0) / g.HUNDRED);
                        MajorIndex.Instance.KospiInstitutionNetBuy = (int)(_cpsvrnew7222.GetDataValue(3, 0) / g.HUNDRED);
                        MajorIndex.Instance.KospiInvestmentNetBuy = (int)(_cpsvrnew7222.GetDataValue(4, 0) / g.HUNDRED);
                        MajorIndex.Instance.KospiPensionNetBuy = (int)(_cpsvrnew7222.GetDataValue(9, 0) / g.HUNDRED);
                    }
                }

                //코스닥 매수액(개인, 외인, 기관)
                _cpsvrnew7222.SetInputValue(0, 'C'); // 코스닥
                _cpsvrnew7222.SetInputValue(1, 0); // 전체
                _cpsvrnew7222.SetInputValue(2, '1'); // 누적
                _cpsvrnew7222.SetInputValue(4, '2'); // 금액

                if (_cpsvrnew7222.GetDibStatus() != 1)
                {
                    if (_cpsvrnew7222.BlockRequest() == 0)
                    {
                        MajorIndex.Instance.KosdaqRetailNetBuy = (int)(_cpsvrnew7222.GetDataValue(1, 0) / g.HUNDRED); // 억 단위로 변환
                        //MajorIndex.Instance.KosdaqForeignNetBuy = (int)(_cpsvrnew7222.GetDataValue(2, 0) / g.HUNDRED); //
                        MajorIndex.Instance.KosdaqInstitutionNetBuy = (int)(_cpsvrnew7222.GetDataValue(3, 0) / g.HUNDRED); //
                        MajorIndex.Instance.KosdaqInvestmentNetBuy = (int)(_cpsvrnew7222.GetDataValue(4, 0) / g.HUNDRED); //
                        MajorIndex.Instance.KosdaqPensionNetBuy = (int)(_cpsvrnew7222.GetDataValue(9, 0) / g.HUNDRED); //
                    }
                }
            }
        }






        // Import the necessary functions from user32.dll
        [DllImport("user32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);



        private void SetFocusAndReturn()
        {
            IntPtr handle = this.Handle;

            // Delay for a short time to allow the browser to open
            System.Threading.Thread.Sleep(100);

            // Check if the form is minimized
            if (IsIconic(handle))
            {
                // Restore the window if it's minimized
                ShowWindow(handle, SW_RESTORE);
            }

            // Set the focus back to the form
            SetForegroundWindow(handle);
            SetFocus(handle);
            // Keep focus on the TextBox
            chart1.Focus();
        }
        // Additional necessary API calls
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            networkMonitor?.Stop();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}

/*
 * 자동 매수/매도
 *  - 상승 후 가격이 혼미상태 일정시간 유지시 매도 1호가 걸어둔다, 일정 액수 이상 추가 하락시 매도(특히 독립종목은 빠르게 처리)
 *  - 그룹 종목은 그룹의 행태 반영
 *  - 상승, 하락 예측 후 자동 매수/매도 ... 매수는 상승 관성이 형상된 최상의 조건일 경우 진행하고 매도는 일정 조건 탈락시 자동
 *  - 자동매도 분당 51 이상 하락시 또는 2분 연속 하락 51 이상 하락시 자동 보고 및 표시, 75 이상 급락시 보고/표시없이 자동매도
 *  - 추세가 형성된 구간에서 계속 현위치에서 상승 가능성 예측 후 추가 매수 (실험을 통해 앞으로 타당성 추가 보완 후)
 *  - 강이 충분히 높으면 사이안 꽃은 없어도 된다 ? 
 * 
 * */

// 꺾이는 종목은 미련없이 손절하라, 추세순응, 체결강도, 매수자재
// 보유종목 급락시 소리와 함께 자동 매도하는 기능
// 시장 종목 중 급등시 소리와 함께 검토 요구하는 기능
// 가는 놈 + 강한 상관, 단독 또는 그룹으로 표시 및 추천 기능 특히 901 근처에서 ?
// ETF 급한 상상시 매수창 제시 및 Confirm 요구하는 기능
// 외인매수 주도종목 상관관계 표시
// 선형성이 강한 종목인가 ? 
// 종목별 외인매수 갯수를 그래프의 종목이름 옆 표시 : StockMember
// CpSvr7037 : 시간대별 예상체결지수 제공
// CpSvr7254 : 투자주체별 일별 기관별 매수/매도
