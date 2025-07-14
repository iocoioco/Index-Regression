using New_Tradegy.Library.Trackers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy
{
    public partial class FormMonitor : Form
    {
        // Public properties accessible from outside
        public bool BuyMode { get; set; }
        public bool SellMode { get; set; }
        public int AmountForDeal { get; set; }
        public int BuyInterval { get; set; }
        public double AggressiveFactor { get; set; }

        private DataGridView dgvWeight;
        public string[] factorNames;
        public double[] factorWeights;
        private TableLayoutPanel controlPanel = null; // 이미 있다면 생략

        public TrackBar trackAmount;
        public TrackBar trackInterval;
        public TrackBar trackAggressive;
        public FormMonitor()
        {
            InitializeComponent();
            SetupLayout();
        }

        private void SetupLayout()
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;

            int width = workingArea.Width * 2 / 10;
            int height = workingArea.Height;
            int x = workingArea.Width * 3 / 10;
            int y = 0;

            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = new Rectangle(x, y, width, height);
            this.Text = "Monitor Panel";

            // === Main Layout: 1 Column ===
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                AutoScroll = true,
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Top controls + weight
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // KOSPI
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // KOSDAQ

            // === Top: Split Controls & Weight ===
            int controlHeight = 240;

            var topPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 1,
                Height = controlHeight
            };
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // === LEFT CONTROLS ===
            var controlWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,       // ✅ Scroll enabled
                AutoSize = false,
            };

            //var controlPanel = new TableLayoutPanel
            //{
            //    Dock = DockStyle.Top,
            //    RowCount = 10,
            //    ColumnCount = 1,
            //    AutoSize = true,
            //};

            var controlPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 10,
                ColumnCount = 1,
            };

            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 매수
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 매도
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // spacing if needed
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 수량
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 간격
            controlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 공격성


            // === Toggle Buttons ===
            var toggleBuyMode = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var lblBuy = new Label { Text = "매수", Width = 80 };
            var buyMode = new Button { Text = "수동", Width = 80 };
            toggleBuyMode.Controls.AddRange(new Control[] { lblBuy, buyMode });

            var toggleSellMode = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var lblSell = new Label { Text = "매도", Width = 80 };
            var sellMode = new Button { Text = "자동", Width = 80 };
            toggleSellMode.Controls.AddRange(new Control[] { lblSell, sellMode });

            BuyMode = true;
            SellMode = false;
            buyMode.Click += (s, e) =>
            {
                BuyMode = !BuyMode;
                buyMode.Text = BuyMode ? "수동" : "자동";
            };
            sellMode.Click += (s, e) =>
            {
                SellMode = !SellMode;
                sellMode.Text = SellMode ? "자동" : "수동";
            };

            // === Sliders ===

            trackAmount = new TrackBar { Minimum = 1, Maximum = 100, TickFrequency = 10, Dock = DockStyle.Fill };
            var lblAmount = new Label { Text = "수량: " + trackAmount.Value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(0) };
            trackAmount.Scroll += (s, e) =>
            {
                AmountForDeal = trackAmount.Value;
                lblAmount.Text = "수량: " + AmountForDeal;
            };

            trackInterval = new TrackBar { Minimum = 1, Maximum = 60, TickFrequency = 5, Dock = DockStyle.Fill };
            var lblInterval = new Label { Text = "간격(초): " + trackInterval.Value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(0) };
            trackInterval.Scroll += (s, e) =>
            {
                BuyInterval = trackInterval.Value;
                lblInterval.Text = "간격(초): " + BuyInterval;
            };

            trackAggressive = new TrackBar { Minimum = 80, Maximum = 120, TickFrequency = 5, Dock = DockStyle.Fill };
            var lblAggressive = new Label { Text = "공격성 x" + (trackAggressive.Value / 100.0).ToString("0.00"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(0) };
            trackAggressive.Scroll += (s, e) =>
            {
                AggressiveFactor = trackAggressive.Value / 100.0;
                lblAggressive.Text = "공격성 x" + AggressiveFactor.ToString("0.00");
            };

            AmountForDeal = trackAmount.Value;
            BuyInterval = trackInterval.Value;
            AggressiveFactor = trackAggressive.Value / 100.0;

            // === Wrap each Label + TrackBar together ===
            TableLayoutPanel MakeSliderBlock(Label label, TrackBar trackBar)
            {
                var panel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 15)); // tight label
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panel.Controls.Add(label, 0, 0);
                panel.Controls.Add(trackBar, 0, 1);
                return panel;
            }

            // === Add to controlPanel ===
            controlPanel.Controls.Add(toggleBuyMode);
            controlPanel.Controls.Add(toggleSellMode);
            controlPanel.Controls.Add(MakeSliderBlock(lblAmount, trackAmount));
            controlPanel.Controls.Add(MakeSliderBlock(lblInterval, trackInterval));
            controlPanel.Controls.Add(MakeSliderBlock(lblAggressive, trackAggressive));


            controlWrapper.Controls.Add(controlPanel);
            topPanel.Controls.Add(controlWrapper, 0, 0);

            // === RIGHT: Weight Table ===
            dgvWeight = SetupWeightTable();
            dgvWeight.Height = controlHeight;

            var weightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            weightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            weightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            weightPanel.Controls.Add(new Label { Text = "요인 가중치", Dock = DockStyle.Top }, 0, 0);
            weightPanel.Controls.Add(dgvWeight, 0, 1);

            topPanel.Controls.Add(weightPanel, 1, 0);
            mainLayout.Controls.Add(topPanel, 0, 0);

            // === KOSPI ===
            var dgvKospi = new DataGridView { Dock = DockStyle.Fill };
            dgvKospi.RowHeadersVisible = false;
            dgvKospi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKospi.Columns.Add("Stock", "종목");
            dgvKospi.Columns.Add("Score", "점수");

            var kospiPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            kospiPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            kospiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            kospiPanel.Controls.Add(new Label { Text = "KOSPI Summary", Dock = DockStyle.Top }, 0, 0);
            kospiPanel.Controls.Add(dgvKospi, 0, 1);
            mainLayout.Controls.Add(kospiPanel, 0, 1);
           
            // === KOSDAQ ===
            var dgvKosdaq = new DataGridView { Dock = DockStyle.Fill };
            dgvKosdaq.RowHeadersVisible = false;
            dgvKosdaq.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKosdaq.Columns.Add("Stock", "종목");
            dgvKosdaq.Columns.Add("Score", "점수");

            var kosdaqPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            kosdaqPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            kosdaqPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            kosdaqPanel.Controls.Add(new Label { Text = "KOSDAQ Summary", Dock = DockStyle.Top }, 0, 0);
            kosdaqPanel.Controls.Add(dgvKosdaq, 0, 1);
            mainLayout.Controls.Add(kosdaqPanel, 0, 2);

            // === Final ===
            this.Controls.Add(mainLayout);
        }



        private DataGridView SetupWeightTable()
        {
            string weightFilePath = @"C:\병신\data work\WeightForIndexTrading.txt";

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                ColumnHeadersVisible = true,
                ScrollBars = ScrollBars.None
            };

            dgv.Columns.Add("Factor", "요인");
            dgv.Columns.Add("Weight", "가중치");

            List<string> Names = new List<string>();
            List<double> weights = new List<double>();

            foreach (var line in File.ReadLines(weightFilePath))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && double.TryParse(parts[1], out double weight))
                {
                    Names.Add(parts[0]);
                    weights.Add(weight);
                    dgv.Rows.Add(parts[0], weight.ToString("0.0"));
                }
            }

            dgv.CellEndEdit += (s, e) =>
            {
                if (e.ColumnIndex == 1)
                {
                    var cellValue = dgv.Rows[e.RowIndex].Cells[1].Value?.ToString();
                    if (double.TryParse(cellValue, out double newValue))
                    {
                        weights[e.RowIndex] = newValue;
                    }
                    else
                    {
                        dgv.Rows[e.RowIndex].Cells[1].Value = weights[e.RowIndex].ToString("0.0");
                    }
                }
                File.WriteAllLines(weightFilePath, Names.Select((name, i) => $"{name}\t{weights[i]:0.0}"));

                this.factorNames = Names.ToArray();
                this.factorWeights = weights.ToArray();
            };

            this.factorNames = Names.ToArray();
            this.factorWeights = weights.ToArray();

            return dgv;
        }




        //private void SetupLayout()
        //{
        //    var workingArea = Screen.PrimaryScreen.WorkingArea;

        //    int width = workingArea.Width * 2 / 10;
        //    int height = workingArea.Height;
        //    int x = workingArea.Width * 3 / 10;
        //    int y = 0;

        //    this.StartPosition = FormStartPosition.Manual;
        //    this.Bounds = new Rectangle(x, y, width, height);
        //    this.Text = "Monitor Panel";

        //    var mainLayout = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        ColumnCount = 2,
        //        RowCount = 1,
        //    };
        //    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        //    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        //    // === LEFT: Control Panel ===
        //    controlPanel = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        RowCount = 10,
        //        ColumnCount = 1,
        //        AutoScroll = false,
        //    };

        //    // Toggle Panel (Buy/Sell)
        //    var toggleBuyMode = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        FlowDirection = FlowDirection.LeftToRight,
        //        AutoSize = true,
        //    };
        //    var lblBuy = new Label { Text = "매수", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        //    var buyMode = new Button { Text = "수동", Width = 80 };
        //    toggleBuyMode.Controls.AddRange(new Control[] { lblBuy, buyMode });

        //    var toggleSellMode = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        FlowDirection = FlowDirection.LeftToRight,
        //        AutoSize = true,
        //    };
        //    var lblSell = new Label { Text = "매도", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        //    var sellMode = new Button { Text = "자동", Width = 80 };
        //    toggleSellMode.Controls.AddRange(new Control[] { lblSell, sellMode });

        //    var toggleWrapper = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Top,
        //        FlowDirection = FlowDirection.TopDown,
        //        AutoSize = true,
        //    };
        //    toggleWrapper.Controls.Add(toggleBuyMode);
        //    toggleWrapper.Controls.Add(toggleSellMode);
        //    controlPanel.Controls.Add(toggleWrapper);

        //    BuyMode = true;
        //    SellMode = false;
        //    buyMode.Click += (sender, e) =>
        //    {
        //        BuyMode = !BuyMode;
        //        buyMode.Text = BuyMode ? "수동" : "자동";
        //    };
        //    sellMode.Click += (sender, e) =>
        //    {
        //        SellMode = !SellMode;
        //        sellMode.Text = SellMode ? "자동" : "수동";
        //    };

        //    // Sliders
        //    trackAmount = new TrackBar { Minimum = 1, Maximum = 100, TickFrequency = 10, Dock = DockStyle.Top };
        //    var lblAmount = new Label { Text = "수량: " + trackAmount.Value };
        //    trackInterval = new TrackBar { Minimum = 1, Maximum = 60, TickFrequency = 5, Dock = DockStyle.Top };
        //    var lblInterval = new Label { Text = "간격(초): " + trackInterval.Value };
        //    trackAggressive = new TrackBar { Minimum = 80, Maximum = 120, TickFrequency = 5, Dock = DockStyle.Top };
        //    var lblAggressive = new Label { Text = "공격성 x" + (trackAggressive.Value / 100.0).ToString("0.00") };

        //    trackAmount.Scroll += (s, e) =>
        //    {
        //        AmountForDeal = trackAmount.Value;
        //        lblAmount.Text = "수량: " + AmountForDeal;
        //    };
        //    trackInterval.Scroll += (s, e) =>
        //    {
        //        BuyInterval = trackInterval.Value;
        //        lblInterval.Text = "간격(초): " + BuyInterval;
        //    };
        //    trackAggressive.Scroll += (s, e) =>
        //    {
        //        AggressiveFactor = trackAggressive.Value / 100.0;
        //        lblAggressive.Text = "공격성 x" + AggressiveFactor.ToString("0.00");
        //    };

        //    AmountForDeal = trackAmount.Value;
        //    BuyInterval = trackInterval.Value;
        //    AggressiveFactor = trackAggressive.Value / 100.0;

        //    SetupWeightTable();

        //    controlPanel.Controls.Add(lblAmount);
        //    controlPanel.Controls.Add(trackAmount);
        //    controlPanel.Controls.Add(lblInterval);
        //    controlPanel.Controls.Add(trackInterval);
        //    controlPanel.Controls.Add(lblAggressive);
        //    controlPanel.Controls.Add(trackAggressive);

        //    // === RIGHT: Summary Panel ===
        //    var summaryPanel = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        RowCount = 3,
        //        ColumnCount = 1,
        //    };
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Weight table
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // KOSPI
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // KOSDAQ

        //    // 1. Weight table on top
        //    var weightWrapper = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        //    weightWrapper.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //    weightWrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        //    weightWrapper.Controls.Add(new Label { Text = "요인 가중치", Dock = DockStyle.Top }, 0, 0);
        //    weightWrapper.Controls.Add(dgvWeight, 0, 1);
        //    summaryPanel.Controls.Add(weightWrapper, 0, 0);

        //    // 2. KOSPI Summary
        //    var dgvKospi = new DataGridView { Dock = DockStyle.Fill };
        //    dgvKospi.Columns.Add("Stock", "종목");
        //    dgvKospi.Columns.Add("Score", "점수");
        //    var kospiPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        //    kospiPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //    kospiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        //    kospiPanel.Controls.Add(new Label { Text = "KOSPI Summary", Dock = DockStyle.Top }, 0, 0);
        //    kospiPanel.Controls.Add(dgvKospi, 0, 1);
        //    summaryPanel.Controls.Add(kospiPanel, 0, 1);

        //    // 3. KOSDAQ Summary
        //    var dgvKosdaq = new DataGridView { Dock = DockStyle.Fill };
        //    dgvKosdaq.Columns.Add("Stock", "종목");
        //    dgvKosdaq.Columns.Add("Score", "점수");
        //    var kosdaqPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        //    kosdaqPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //    kosdaqPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        //    kosdaqPanel.Controls.Add(new Label { Text = "KOSDAQ Summary", Dock = DockStyle.Top }, 0, 0);
        //    kosdaqPanel.Controls.Add(dgvKosdaq, 0, 1);
        //    summaryPanel.Controls.Add(kosdaqPanel, 0, 2);

        //    // Final assembly
        //    mainLayout.Controls.Add(controlPanel, 0, 0);
        //    mainLayout.Controls.Add(summaryPanel, 1, 0);
        //    this.Controls.Add(mainLayout);
        //}



        //private void SetupLayout()
        //{
        //    var workingArea = Screen.PrimaryScreen.WorkingArea;

        //    int width = workingArea.Width * 2 / 10;
        //    int height = workingArea.Height;
        //    int x = workingArea.Width * 3 / 10;
        //    int y = 0;

        //    this.StartPosition = FormStartPosition.Manual;

        //    this.Bounds = new Rectangle(x, y, width, height);

        //    this.Text = "Monitor Panel";

        //    var mainLayout = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        ColumnCount = 2,
        //        RowCount = 1,
        //    };
        //    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        //    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        //    // ==== Column 0: Controls ====
        //    controlPanel = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        RowCount = 6,
        //        ColumnCount = 1,
        //        AutoScroll = false,
        //    };

        //    // 1. Buy/Sell toggle
        //    var toggleBuyMode = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        FlowDirection = FlowDirection.LeftToRight, // 가로로 정렬 (기존대로)
        //        AutoSize = true,
        //    };
        //    var lblBuy = new Label { Text = "매수", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        //    var buyMode = new Button { Text = "수동", Width = 80 };
        //    toggleBuyMode.Controls.AddRange(new Control[] { lblBuy, buyMode });

        //    // 2. Manual/Auto toggle
        //    var toggleSellMode = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        FlowDirection = FlowDirection.LeftToRight, // 가로로 정렬 (기존대로)
        //        AutoSize = true,
        //    };
        //    var lblSell = new Label { Text = "매도", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        //    var sellMode = new Button { Text = "자동", Width = 80 };
        //    toggleSellMode.Controls.AddRange(new Control[] { lblSell, sellMode });

        //    // 두 개를 포함하는 패널을 하나 만들어 수직으로 배치
        //    var toggleWrapper = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Top,
        //        FlowDirection = FlowDirection.TopDown,
        //        AutoSize = true,
        //    };
        //    toggleWrapper.Controls.Add(toggleBuyMode);
        //    toggleWrapper.Controls.Add(toggleSellMode);

        //    // 최종적으로 ControlPanel에 추가
        //    controlPanel.Controls.Add(toggleWrapper);

        //    // Initial modes
        //    BuyMode = true;  // true = 수동, false = 자동
        //    SellMode = false;

        //    // Event handler for Buy button
        //    buyMode.Click += (sender, e) =>
        //    {
        //        BuyMode = !BuyMode;
        //        buyMode.Text = BuyMode ? "수동" : "자동";
        //    };

        //    // Event handler for Sell button
        //    sellMode.Click += (sender, e) =>
        //    {
        //        SellMode = !SellMode;
        //        sellMode.Text = SellMode ? "자동" : "수동";
        //    };

        //    /// 3. Scrolls: amount, interval, aggressiveness
        //    trackAmount = new TrackBar { Minimum = 1, Maximum = 100, TickFrequency = 10, Dock = DockStyle.Top };
        //    var lblAmount = new Label { Text = "수량: " + trackAmount.Value };

        //    trackInterval = new TrackBar { Minimum = 1, Maximum = 60, TickFrequency = 5, Dock = DockStyle.Top };
        //    var lblInterval = new Label { Text = "간격(초): " + trackInterval.Value };

        //    trackAggressive = new TrackBar { Minimum = 80, Maximum = 120, TickFrequency = 5, Dock = DockStyle.Top };
        //    var lblAggressive = new Label { Text = "공격성 x" + (trackAggressive.Value / 100.0).ToString("0.00") };

        //    // Event handlers update both label and corresponding public property
        //    trackAmount.Scroll += (s, e) =>
        //    {
        //        AmountForDeal = trackAmount.Value;
        //        lblAmount.Text = "수량: " + AmountForDeal;
        //    };

        //    trackInterval.Scroll += (s, e) =>
        //    {
        //        BuyInterval = trackInterval.Value;
        //        lblInterval.Text = "간격(초): " + BuyInterval;
        //    };

        //    trackAggressive.Scroll += (s, e) =>
        //    {
        //        AggressiveFactor = trackAggressive.Value / 100.0;
        //        lblAggressive.Text = "공격성 x" + AggressiveFactor.ToString("0.00");
        //    };

        //    AmountForDeal = trackAmount.Value;
        //    BuyInterval = trackInterval.Value;
        //    AggressiveFactor = trackAggressive.Value / 100.0;

        //    SetupWeightTable();

        //    // 5. Market Summary
        //    var txtSummary = new TextBox { Multiline = true, Dock = DockStyle.Fill, Height = 100, ScrollBars = ScrollBars.Vertical };

        //    controlPanel.Controls.Add(toggleBuyMode);
        //    controlPanel.Controls.Add(toggleSellMode);
        //    controlPanel.Controls.Add(lblAmount); 
        //    controlPanel.Controls.Add(trackAmount);
        //    controlPanel.Controls.Add(lblInterval); 
        //    controlPanel.Controls.Add(trackInterval);
        //    controlPanel.Controls.Add(lblAggressive); 
        //    controlPanel.Controls.Add(trackAggressive);
        //    controlPanel.Controls.Add(dgvWeight);
        //    controlPanel.Controls.Add(new Label { Text = "시장 요약", Dock = DockStyle.Top });
        //    controlPanel.Controls.Add(txtSummary);

        //    // ==== Column 1: Summary Tables ====
        //    var summaryPanel = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        RowCount = 2,
        //    };

        //    var dgvKospi = new DataGridView { Dock = DockStyle.Fill };
        //    dgvKospi.Columns.Add("Stock", "종목");
        //    dgvKospi.Columns.Add("Score", "점수");

        //    var dgvKosdaq = new DataGridView { Dock = DockStyle.Fill };
        //    dgvKosdaq.Columns.Add("Stock", "종목");
        //    dgvKosdaq.Columns.Add("Score", "점수");

        //    summaryPanel.Controls.Add(new Label { Text = "KOSPI Summary", Dock = DockStyle.Top });
        //    summaryPanel.Controls.Add(dgvKospi);
        //    summaryPanel.Controls.Add(new Label { Text = "KOSDAQ Summary", Dock = DockStyle.Top });
        //    summaryPanel.Controls.Add(dgvKosdaq);

        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Label 1
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));    // KOSPI DGV
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Label 2
        //    summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));    // KOSDAQ DGV

        //    mainLayout.Controls.Add(controlPanel, 0, 0);
        //    mainLayout.Controls.Add(summaryPanel, 1, 0);

        //    this.Controls.Add(mainLayout);


        //}

        //private void SetupWeightTable()
        //{
        //    string weightFilePath = @"C:\병신\data work\WeightForIndexTrading.txt";

        //    dgvWeight = new DataGridView
        //    {
        //        Dock = DockStyle.Fill,
        //        Height = 200,
        //        AllowUserToAddRows = false,
        //        RowHeadersVisible = false,
        //        ColumnHeadersVisible = true,
        //        ScrollBars = ScrollBars.None
        //    };

        //    dgvWeight.Columns.Add("Factor", "요인");
        //    dgvWeight.Columns.Add("Weight", "가중치");

        //    List<string> Names = new List<string>();
        //    List<double> weights = new List<double>();

        //    // Read and parse the file
        //    foreach (var line in File.ReadLines(weightFilePath))
        //    {
        //        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (parts.Length >= 2 && double.TryParse(parts[1], out double weight))
        //        {
        //            Names.Add(parts[0]);
        //            weights.Add(weight);
        //            dgvWeight.Rows.Add(parts[0], weight.ToString("0.0"));
        //        }
        //    }

        //    // Add edit handler to update the underlying weight list
        //    dgvWeight.CellEndEdit += (s, e) =>
        //    {
        //        if (e.ColumnIndex == 1) // only allow editing weight
        //        {
        //            var cellValue = dgvWeight.Rows[e.RowIndex].Cells[1].Value?.ToString();
        //            if (double.TryParse(cellValue, out double newValue))
        //            {
        //                weights[e.RowIndex] = newValue;
        //            }
        //            else
        //            {
        //                // Revert to old value on invalid input
        //                dgvWeight.Rows[e.RowIndex].Cells[1].Value = weights[e.RowIndex].ToString("0.0");
        //            }
        //        }
        //        File.WriteAllLines(@"C:\병신\data work\WeightForIndexTrading.txt",
        //            Names.Select((name, i) => $"{name}\t{weights[i]:0.0}"));

        //        // Store for future use if needed
        //        this.factorNames = factorNames.ToArray();
        //        this.factorWeights = weights.ToArray();
        //    };

        //    // Store for future use if needed
        //    this.factorNames = Names.ToArray();
        //    this.factorWeights = weights.ToArray();

        //    controlPanel.Controls.Add(dgvWeight);
        //}
    }
}
