using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    public static class Index
    {
        public static List<string> GetValidDirectories(string rootDir)
        {
            List<string> allDirs = Directory.GetDirectories(rootDir).ToList();

            // Sort newest to oldest
            allDirs.Sort((a, b) => String.Compare(Path.GetFileName(b), Path.GetFileName(a)));

            List<string> validDirs = new List<string>();

            foreach (string dir in allDirs)
            {
                if (IsValidDirectoryBySamsungData(dir))
                {
                    validDirs.Add(dir);
                }
            }

            return validDirs;
        }


        public static bool IsValidDirectoryBySamsungData(string dirPath)
        {
            string filePath = Path.Combine(dirPath, "삼성전자.txt");
            if (!File.Exists(filePath))
                return false;

            string[] lines = File.ReadAllLines(filePath);
            return lines.Length >= 300;
        }


        public static List<string> GetExistingStocksInDirectory(string dirPath, List<string> expectedStocks)
        {
            List<string> existingStocks = new List<string>();

            foreach (string stock in expectedStocks)
            {
                string filePath = Path.Combine(dirPath, $"{stock}.txt");
                if (File.Exists(filePath))
                {
                    existingStocks.Add(stock);
                }
            }

            return existingStocks;
        }


        public static List<string> GetAvailableStocks(string directory, List<string> stockList)
        {
            var availableStocks = new List<string>();

            foreach (var stock in stockList)
            {
                string filePath = Path.Combine(directory, stock + ".txt");
                if (File.Exists(filePath))
                    availableStocks.Add(stock);
            }

            // Skip if fewer than 35
            if (availableStocks.Count < 35)
                return new List<string>(); // or return null or throw, depending on your logic

            return availableStocks;
        }


        public static int ElapsedTimeInSeconds(int t1, int t2)
        {
            int h1 = t1 / 10000;
            int m1 = (t1 / 100) % 100;
            int s1 = t1 % 100;

            int h2 = t2 / 10000;
            int m2 = (t2 / 100) % 100;
            int s2 = t2 % 100;

            var ts1 = new TimeSpan(h1, m1, s1);
            var ts2 = new TimeSpan(h2, m2, s2);
            return (int)(ts1 - ts2).TotalSeconds;
        }


    }
}
