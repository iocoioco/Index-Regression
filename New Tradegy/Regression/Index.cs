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

    }
}
