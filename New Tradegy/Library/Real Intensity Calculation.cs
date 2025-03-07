using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    internal class Real_Intensity_Calculation
    {


        static (double, double) AdjustIntensity(double buyIntensity, double sellIntensity, double totalDealt, double programBuyAmount)
        {
            /*
            Adjust buy and sell intensity by excluding the program's contribution.
            :param buyIntensity: Original buy intensity
            :param sellIntensity: Original sell intensity
            :param totalDealt: Total amount dealt per minute
            :param programBuyAmount: Program buy amount per minute (+ buy, - sell)
            :return: Adjusted buy and sell intensity
            */
            double adjustedBuyIntensity = buyIntensity;
            double adjustedSellIntensity = sellIntensity;

            if (programBuyAmount > 0) // Program is buying
            {
                adjustedBuyIntensity -= programBuyAmount;
            }
            else if (programBuyAmount < 0) // Program is selling
            {
                adjustedSellIntensity += programBuyAmount; // Adding because it's negative
            }

            // Ensure values are not negative
            adjustedBuyIntensity = Math.Max(0, adjustedBuyIntensity);
            adjustedSellIntensity = Math.Max(0, adjustedSellIntensity);

            return (adjustedBuyIntensity, adjustedSellIntensity);
        }

        //static void Main()
        //{
        //    double buyIntensity = 219;
        //    double sellIntensity = 120;
        //    double totalDealt = 451;
        //    double programBuyAmount = 110; // Example: Positive for buying, negative for selling

        //    var (adjustedBuy, adjustedSell) = AdjustIntensity(buyIntensity, sellIntensity, totalDealt, programBuyAmount);
        //    Console.WriteLine($"Adjusted Buy Intensity: {adjustedBuy}, Adjusted Sell Intensity: {adjustedSell}");
        //}
    }


}

