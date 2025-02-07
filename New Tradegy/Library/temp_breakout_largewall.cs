using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{

// 52400 assumed to be breakout price
//    Adaptive wait time: Strong breakouts execute in 100ms, weaker ones wait 200ms.
//✅ Bid size confirmation: Ensures follow-through buying.
//✅ Early execution: If ask size at 52,500 collapses, buy earlier.

    class TickSizeOrderManager
    {
        private decimal lastBestBid;
        private decimal lastBestAsk;
        private decimal breakoutPrice;
        private decimal tickSize = 100m; // Samsung’s tick size = 100 KRW
        private int previousSellSize = 0;
        private int previousBestBidSize = 0;
        private int collapseThreshold = 50; // 50% collapse required
        private Stopwatch collapseTimer = new Stopwatch();
        private bool orderPlaced = false;

        private int fastCollapseThreshold = 70; // 70%+ collapse in 100ms = fast breakout
        private int adaptiveWaitTime = 200; // Default 200ms, adjusted dynamically
        private int bidIncreaseThreshold = 20; // Best bid size must increase by 20 lots
        private int askSizeDropThreshold = 50; // 50% drop in ask size triggers early execution

        public TickSizeOrderManager(decimal breakoutPrice)
        {
            this.breakoutPrice = breakoutPrice;
        }

        public void OnBookUpdate(decimal bestBid, decimal bestAsk, int bidSize, int askSize)
        {
            lastBestBid = bestBid;
            lastBestAsk = bestAsk;

            Console.WriteLine($"Market Update - Best Bid: {bestBid}, Best Ask: {bestAsk}, Ask Size: {askSize}");

            // Track initial large sell order at breakout price
            if (bestAsk == breakoutPrice)
            {
                if (previousSellSize == 0)
                    previousSellSize = askSize;

                int sizeDrop = previousSellSize - askSize;
                decimal percentageDrop = previousSellSize > 0 ? (sizeDrop * 100m / previousSellSize) : 0m;

                Console.WriteLine($"Ask Size Collapse: {percentageDrop:F2}%");

                // Adjust wait time if sell wall collapses > 70% in 100ms
                if (percentageDrop >= fastCollapseThreshold)
                    adaptiveWaitTime = 100;
                else
                    adaptiveWaitTime = 200; // Reset to default

                // If sell order drops by 50% or more, start timer
                if (percentageDrop >= collapseThreshold)
                {
                    if (!collapseTimer.IsRunning)
                        collapseTimer.Start();

                    // Check for early execution (if best bid size increases or ask size at 52500 drops)
                    if (collapseTimer.ElapsedMilliseconds >= 100)
                    {
                        if (bidSize >= previousBestBidSize + bidIncreaseThreshold || askSize <= previousSellSize * askSizeDropThreshold / 100)
                        {
                            Console.WriteLine("🚀 Early confirmation! Placing buy order.");
                            PlaceBreakoutLimitOrder();
                            collapseTimer.Reset();
                        }
                    }

                    // If full confirmation (adaptive wait time met), place order
                    if (collapseTimer.ElapsedMilliseconds >= adaptiveWaitTime)
                    {
                        Console.WriteLine("✅ Confirmed breakout! Placing buy order.");
                        PlaceBreakoutLimitOrder();
                        collapseTimer.Reset();
                    }
                }
                else
                {
                    collapseTimer.Reset(); // Reset if collapse is not strong
                }
            }
            else
            {
                // Reset if price moves away (indicating hesitation or fake breakout)
                previousSellSize = 0;
                collapseTimer.Reset();
            }

            // Track best bid size for future updates
            previousBestBidSize = bidSize;
        }

        private void PlaceBreakoutLimitOrder()
        {
            if (orderPlaced) return;

            decimal orderPrice = breakoutPrice + tickSize; // Buy at next valid tick
            Console.WriteLine($"✅ Final breakout confirmation! Placing limit order at: {orderPrice}");

            orderPlaced = true;

            // Simulate order execution (Replace with actual API order placement)
        }
    }

    // Simulated real-time book data feed
    class Program
    {
        private static TickSizeOrderManager orderManager = new TickSizeOrderManager(52400m);
        private static Random random = new Random();
        //static void Main()
        //{
        //    while (true)
        //    {
        //        decimal bestBid = 52300 + random.Next(0, 2) * 100;
        //        decimal bestAsk = 52400 + random.Next(0, 2) * 100;
        //        int bidSize = random.Next(100, 500);
        //        int askSize = random.Next(200, 1000);

        //        orderManager.OnBookUpdate(bestBid, bestAsk, bidSize, askSize);

        //        Thread.Sleep(100);
        //    }
        //}
    }
}