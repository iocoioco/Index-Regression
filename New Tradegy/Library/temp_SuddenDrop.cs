using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    internal class temp_SuddenDrop
    {

        // Structure to store each buy/sell transaction
        class Trade
        {
            public DateTime Time { get; set; }
            public double Price { get; set; }
            public double Volume { get; set; }  // Positive for buy, negative for sell
        }

        static List<Trade> tradeHistory = new List<Trade>();
        static double vwap = 0.0;  // Volume Weighted Average Price
        static double totalBuyVolume = 0.0;
        static double currentPrice = 0.0;
        static double initialPumpPrice = 0.0;
        static double peakPrice = 0.0;
        static double stopLossTight = 0.0;
        static double stopLossLoose = 0.0;

        //static void Main()
        //{
        //    Console.WriteLine("Tracking Program Buy/Sell Activities...");

        //    // Simulated trade data (can be replaced with real-time data)
        //    SimulateTrades();

        //    // Calculate VWAP
        //    vwap = CalculateVWAP();

        //    // Estimate unrealized loss
        //    double unrealizedLoss = (vwap - currentPrice) * totalBuyVolume;

        //    // Determine urgency level
        //    string urgencyLevel = AssessUrgency(unrealizedLoss);

        //    // Check stop-loss levels
        //    string stopLossStatus = CheckStopLoss();

        //    // Display results
        //    Console.WriteLine($"VWAP: {vwap:F2}, Current Price: {currentPrice:F2}, Unrealized Loss: {unrealizedLoss:F2} KRW");
        //    Console.WriteLine($"Urgency Level: {urgencyLevel}");
        //    Console.WriteLine($"Stop-Loss Status: {stopLossStatus}");
        //}

        static void SimulateTrades()
        {
            // Example transactions from 10:00 AM to 3:30 PM
            tradeHistory.Add(new Trade { Time = DateTime.Parse("10:00"), Price = 520000, Volume = 10000 });
            tradeHistory.Add(new Trade { Time = DateTime.Parse("10:30"), Price = 530000, Volume = 20000 });
            tradeHistory.Add(new Trade { Time = DateTime.Parse("11:00"), Price = 550000, Volume = 30000 });
            tradeHistory.Add(new Trade { Time = DateTime.Parse("11:30"), Price = 560000, Volume = 25000 });
            tradeHistory.Add(new Trade { Time = DateTime.Parse("12:00"), Price = 570000, Volume = -10000 }); // Partial sell
            tradeHistory.Add(new Trade { Time = DateTime.Parse("12:30"), Price = 575000, Volume = 15000 }); // Peak price buy
            tradeHistory.Add(new Trade { Time = DateTime.Parse("13:00"), Price = 560000, Volume = 20000 }); // Supporting buy
            tradeHistory.Add(new Trade { Time = DateTime.Parse("14:00"), Price = 540000, Volume = 10000 }); // Supporting buy
            tradeHistory.Add(new Trade { Time = DateTime.Parse("15:00"), Price = 530000, Volume = 5000 });  // Supporting buy
            tradeHistory.Add(new Trade { Time = DateTime.Parse("15:30"), Price = 520000, Volume = 0 });  // Market close price

            // Set key prices
            initialPumpPrice = 520000;  // First +3% price
            peakPrice = 575000;  // Peak price +11%
            currentPrice = 520000; // Latest market price
            stopLossTight = initialPumpPrice - (initialPumpPrice * 0.015); // 1.5% below initial pump
            stopLossLoose = initialPumpPrice - (initialPumpPrice * 0.03); // 3% below initial pump
        }

        static double CalculateVWAP()
        {
            double totalValue = 0.0;
            totalBuyVolume = 0.0;

            foreach (var trade in tradeHistory)
            {
                if (trade.Volume > 0) // Only count buys
                {
                    totalValue += trade.Price * trade.Volume;
                    totalBuyVolume += trade.Volume;
                }
            }

            return totalBuyVolume > 0 ? totalValue / totalBuyVolume : 0.0;
        }

        static string AssessUrgency(double unrealizedLoss)
        {
            if (unrealizedLoss > 50_000_000_000) return "🚨 HIGH URGENCY (Massive Loss)";
            if (unrealizedLoss > 10_000_000_000) return "⚠️ MEDIUM URGENCY (Significant Loss)";
            return "✅ LOW URGENCY (Manageable Loss)";
        }

        static string CheckStopLoss()
        {
            if (currentPrice <= stopLossLoose)
                return "❌ Loose Stop-Loss Hit (Program May Abandon Support)";
            if (currentPrice <= stopLossTight)
                return "⚠️ Tight Stop-Loss Approaching (Program Might Defend Aggressively)";
            return "✅ Stop-Loss Levels Not Hit Yet";
        }
    }

}


class Program
{
    class OrderBookEntry
    {
        public double Price { get; set; }
        public int Volume { get; set; }
        public DateTime Timestamp { get; set; }
    }

    static List<OrderBookEntry> askOrders = new List<OrderBookEntry>();
    static List<OrderBookEntry> bidOrders = new List<OrderBookEntry>();

    //static void Main()
    //{
    //    Console.WriteLine("Detecting Order Book Manipulation (Spoofing / Rapid Cancelations)...");

    //    // Simulated Order Book Changes (Replace with real-time data later)
    //    SimulateOrderBookChanges();

    //    // Detect Spoofing (Fake Large Orders that Disappear)
    //    DetectSpoofing();
    //}

    static void SimulateOrderBookChanges()
    {
        // Simulating order placements and removals
        askOrders.Add(new OrderBookEntry { Price = 10000, Volume = 100, Timestamp = DateTime.Now });
        System.Threading.Thread.Sleep(100); // Simulating delay
        askOrders.RemoveAll(order => order.Price == 10000); // Fake order disappears

        askOrders.Add(new OrderBookEntry { Price = 10010, Volume = 150, Timestamp = DateTime.Now });
        System.Threading.Thread.Sleep(100);
        askOrders.RemoveAll(order => order.Price == 10010); // Another fake order disappears
    }

    static void DetectSpoofing()
    {
        Console.WriteLine("Analyzing Order Book for Spoofing Patterns...");

        var suspiciousOrders = askOrders.GroupBy(o => o.Price)
            .Where(group => group.Count() >= 2) // Orders appearing/disappearing multiple times
            .Select(group => new { Price = group.Key, Count = group.Count() })
            .ToList();

        if (suspiciousOrders.Any())
        {
            Console.WriteLine("🚨 Possible Spoofing Detected at These Prices:");
            foreach (var order in suspiciousOrders)
            {
                Console.WriteLine($"Price: {order.Price}, Disappeared Orders: {order.Count}");
            }
        }
        else
        {
            Console.WriteLine("✅ No immediate spoofing detected.");
        }
    }
}










class Program1
{
    class OrderBookEntry
    {
        public double Price { get; set; }
        public int Volume { get; set; }
        public DateTime Timestamp { get; set; }
    }

    static List<OrderBookEntry> askOrders = new List<OrderBookEntry>();
    static List<OrderBookEntry> bidOrders = new List<OrderBookEntry>();

    //static void Main()
    //{
    //    Console.WriteLine("Tracking Order Book Oscillations for Manual Trading Assistance...");

    //    // Simulated Order Book Changes (Replace with real-time data later)
    //    SimulateOrderBookChanges();

    //    // Detect Rapid Bid-Ask Oscillations
    //    DetectOscillations();
    //}

    static void SimulateOrderBookChanges()
    {
        // Simulating bid-ask oscillations
        bidOrders.Add(new OrderBookEntry { Price = 9990, Volume = 100, Timestamp = DateTime.Now });
        askOrders.Add(new OrderBookEntry { Price = 10000, Volume = 100, Timestamp = DateTime.Now });
        System.Threading.Thread.Sleep(50);
        bidOrders.RemoveAll(order => order.Price == 9990);
        askOrders.RemoveAll(order => order.Price == 10000);

        bidOrders.Add(new OrderBookEntry { Price = 9995, Volume = 150, Timestamp = DateTime.Now });
        askOrders.Add(new OrderBookEntry { Price = 10005, Volume = 150, Timestamp = DateTime.Now });
        System.Threading.Thread.Sleep(50);
        bidOrders.RemoveAll(order => order.Price == 9995);
        askOrders.RemoveAll(order => order.Price == 10005);
    }

    static void DetectOscillations()
    {
        Console.WriteLine("Analyzing Order Book for Rapid Bid-Ask Oscillations...");

        var bidPriceChanges = bidOrders.Select(o => o.Price).Distinct().Count();
        var askPriceChanges = askOrders.Select(o => o.Price).Distinct().Count();

        if (bidPriceChanges > 3 && askPriceChanges > 3)
        {
            Console.WriteLine("🚨 High-frequency oscillations detected! Program trading is active.");
        }
        else
        {
            Console.WriteLine("✅ No abnormal oscillations detected.");
        }

    }










    class Program
    {
        class SectorData
        {
            public string Name { get; set; }
            public double PriceChange { get; set; }
            public double Volume { get; set; }
            public DateTime LastActive { get; set; }
        }

        static List<SectorData> allSectors = new List<SectorData>();
        static List<SectorData> activeSectors = new List<SectorData>();
        static double activityThreshold = 2.0; // % price change needed to be considered active
        static int inactivityDaysLimit = 5; // Remove sectors inactive for 5+ days

        //static void Main()
        //{
        //    Console.WriteLine("Tracking Only Active Sectors...");

        //    // Simulated sector data (Replace with real-time data later)
        //    SimulateSectorChanges();

        //    // Update active sectors
        //    UpdateActiveSectors();

        //    // Display tracked sectors
        //    Console.WriteLine("✅ Currently Tracked Active Sectors:");
        //    foreach (var sector in activeSectors)
        //    {
        //        Console.WriteLine($"{sector.Name}: Change {sector.PriceChange}% | Volume {sector.Volume}");
        //    }
        //}

        static void SimulateSectorChanges()
        {
            allSectors = new List<SectorData>
        {
            new SectorData { Name = "Robotics", PriceChange = 3.5, Volume = 500000, LastActive = DateTime.Now },
            new SectorData { Name = "Shipbuilding", PriceChange = 2.8, Volume = 400000, LastActive = DateTime.Now },
            new SectorData { Name = "Battery", PriceChange = 0.5, Volume = 100000, LastActive = DateTime.Now.AddDays(-6) }, // Will be removed
            new SectorData { Name = "CXL", PriceChange = 4.1, Volume = 600000, LastActive = DateTime.Now },
            new SectorData { Name = "Glass PCB", PriceChange = 2.2, Volume = 300000, LastActive = DateTime.Now },
            new SectorData { Name = "Ukraine Recon.", PriceChange = 3.0, Volume = 250000, LastActive = DateTime.Now },
            new SectorData { Name = "Nuclear & SMR", PriceChange = 5.0, Volume = 700000, LastActive = DateTime.Now },
            new SectorData { Name = "Retail", PriceChange = 0.2, Volume = 50000, LastActive = DateTime.Now.AddDays(-10) }, // Will be removed
            new SectorData { Name = "Agriculture", PriceChange = 1.0, Volume = 120000, LastActive = DateTime.Now.AddDays(-8) } // Will be removed
        };
        }

        static void UpdateActiveSectors()
        {
            activeSectors = allSectors
                .Where(s => s.PriceChange >= activityThreshold || (DateTime.Now - s.LastActive).TotalDays <= inactivityDaysLimit)
                .ToList();
        }
    }
}
