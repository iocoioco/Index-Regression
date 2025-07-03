using New_Tradegy.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Core
{
    public static class GroupRepository
    {
        public static List<GroupData> LoadGroups()
        {
            var groups = new List<GroupData>();
            string[] lines = File.ReadAllLines(@"C:\병신\data work\상관.txt", Encoding.UTF8);

            GroupData currentGroup = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Case 1: Full-line comment → new group
                if (line.StartsWith("//"))
                {
                    string groupTitle = line.Substring(2).Trim();
                    currentGroup = new GroupData(groupTitle);
                    groups.Add(currentGroup);
                }
                else if (currentGroup != null)
                {
                    // Remove inline comment
                    int commentIndex = line.IndexOf("//");
                    if (commentIndex >= 0)
                    {
                        line = line.Substring(0, commentIndex).Trim();
                    }

                    if (string.IsNullOrEmpty(line)) continue;

                    string[] stocks = line
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(name => name.Replace('_', ' ').Trim())
                        .ToArray();

                    currentGroup.Stocks.AddRange(stocks);
                }
            }

            // ✅ Filter unqualified stocks
            var totalStockSet = new HashSet<string>(g.StockManager.TotalStockList.Select(s => s.Trim()));

            for (int i = groups.Count - 1; i >= 0; i--)
            {
                var group = groups[i];
                group.Stocks = group.Stocks
                    .Select(s => s.Trim())
                    .Where(stock => totalStockSet.Contains(stock))
                    .Distinct()
                    .ToList();

                if (group.Stocks.Count <= 1)
                {
                    groups.RemoveAt(i); // Remove group with ≤ 1 qualified stock
                }
            }

            return groups;
        }

        public static void SaveGroups(List<GroupData> groups)
        {
            using (StreamWriter writer = new StreamWriter("상관.txt"))
            {
                foreach (var group in groups)
                {
                    writer.WriteLine($"// {group.Title}");
                    foreach (var chunk in ChunkList(group.Stocks, 5)) // optional: split long lines
                    {
                        writer.WriteLine(string.Join(" ", chunk));
                    }
                    writer.WriteLine();
                }
            }
        }

        public static void SaveFilteredGroups(List<GroupData> groups, string outputPath)
        {
            using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                foreach (var group in groups)
                {
                    writer.WriteLine($"// {group.Title}");

                    foreach (var chunk in ChunkList(group.Stocks, 5)) // split long lines
                    {
                        var encoded = chunk.Select(name => name.Replace(' ', '_'));
                        writer.WriteLine(string.Join(" ", encoded));
                    }

                    writer.WriteLine(); // empty line between groups
                }
            }
        }

        private static List<List<string>> ChunkList(List<string> list, int chunkSize)
        {
            var chunks = new List<List<string>>();
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                chunks.Add(list.GetRange(i, Math.Min(chunkSize, list.Count - i)));
            }
            return chunks;
        }
    }
}
