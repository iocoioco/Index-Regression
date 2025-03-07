using CPTRADELib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{

    public class Order
    {
        public string Stock { get; set; }
        public int Price { get; set; }

        public int Quantity { get; set; }
        public int UrgencyLevel { get; set; }

        public Order(string stock, int price, int quantity, int urgencyLevel)
        {
            Stock = stock;
            Price = price;

            Quantity = quantity;
            UrgencyLevel = urgencyLevel;
        }
    }

    public class StockExchange
    {
        private static StockExchange _instance;
        public static StockExchange Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StockExchange();
                return _instance;
            }
        }
        private StockExchange() { } // Private constructor to prevent multiple instances


        public static List<Order> buyOrders = new List<Order>();
        public static List<Order> sellOrders = new List<Order>();

 

        public void MonitorPrices(string stock, int sellHogaVolume, int sellHogaPrice, int buyHogaVolume, int buyHogaPrice)
        {
            // buyOrders
            foreach (var order in new List<Order>(buyOrders))
            {
                if (order.Stock == stock && order.Price == sellHogaPrice)
                {
                    string str = "";
                    str += order.Stock + " : " + order.Price.ToString() + " X " + order.Quantity.ToString() +
                                       " = " + (order.Price * order.Quantity / 10000).ToString() + "만원";
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                    {
                        return;
                    }
                    str += "\n" + sr.r3_display_매수_매도(g.ogl_data[index]);

                    RemoveOrder(buyOrders, order);

                    bool isSell = false;
                    var a = new jp();
                    a.OpenOrUpdateConfirmationForm(isSell, order.Stock, order.Quantity, order.Price, order.UrgencyLevel, str);
                
                    
                }
            }

            // sellOrders
            foreach (var order in new List<Order>(sellOrders))
            {
                if (order.Stock == stock && order.Price == buyHogaPrice)
                {
                    string str = "";
                    str += order.Stock + " : " + order.Price.ToString() + " X " + order.Quantity.ToString() +
                                       " = " + (order.Price * order.Quantity / 10000).ToString() + "만원";
                    int index = wk.return_index_of_ogldata(stock);
                    if (index < 0)
                    {
                        return;
                    }
                    str += "\n" + sr.r3_display_매수_매도(g.ogl_data[index]);

                    RemoveOrder(sellOrders, order);

                    var a = new jp();
                    bool isSell = true;
                    a.OpenOrUpdateConfirmationForm(isSell, order.Stock, order.Quantity, order.Price, order.UrgencyLevel, str);

                }
            }



            
        }

        public void AddBuyOrder(string stock, int price, int quantity, int urgencyLevel)
        {
            if (DealManager.CheckPreviousLoss(stock))
            {
                return;
            }



            // Find the existing order with the same stock and price
            var existingOrder = buyOrders.Find(order => order.Stock == stock && order.Price == price);

            // If such an order exists, remove it
            if (existingOrder != null)
            {
                buyOrders.Remove(existingOrder);
            }

            buyOrders.Add(new Order(stock, price, quantity, urgencyLevel));
        }

        public void AddSellOrder(string stock, int price, int quantity, int urgencyLevel)
        {
            // Find the existing order with the same stock and price
            var existingOrder = sellOrders.Find(order => order.Stock == stock && order.Price == price);

            // If such an order exists, remove it
            if (existingOrder != null)
            {
                sellOrders.Remove(existingOrder);
            }

            sellOrders.Add(new Order(stock, price, quantity, urgencyLevel));
        }

        private bool IsSuddenDrop(int sellHogaPrice, int buyHogaPrice)
        {
            return (sellHogaPrice - buyHogaPrice) > (int)TickDifference(sellHogaPrice);
        }

        
        private int TickDifference(double x)
        {
            if (x < 2000)
            {
                return 1;
            }
            else if (x < 5000)
            {
                return 5;
            }
            else if (x < 20000)
            {
                return 10;
            }
            else if (x < 50000)
            {
                return 50;
            }
            else if (x < 200000)
            {
                return 100;
            }
            else if (x < 500000)
            {
                return 500;
            }
            else
            {
                return 1000;
            }
        }

        private bool IsHogaGapAcceptable(int sellHogaPrice, int buyHogaPrice)
        {
            double priceDifference = Math.Abs(sellHogaPrice - buyHogaPrice);
            int allowedTickDifference = TickDifference(Math.Min(sellHogaPrice, buyHogaPrice));
            return priceDifference <= allowedTickDifference;
        }

        private void RemoveOrder(List<Order> orderList, Order order)
        {
            orderList.Remove(order);
        }
    }


}
