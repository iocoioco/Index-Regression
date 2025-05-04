using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace New_Tradegy.Library.Core
{
    public class GroupManager
    {
        private List<GroupData> _groups;
        public List<GroupData> RankingList { get; private set; } = new List<GroupData>();

        public List<string> GetTopStocksFromTopGroups(int groupLimit = 5, int stockPerGroup = 3, List<string> existing = null)
        {
            var result = new List<string>();
            if (existing == null)
                existing = new List<string>();

            int count = Math.Min(groupLimit, RankingList.Count);

            for (int i = 0; i < count; i++)
            {
                var group = RankingList[i];
                int added = 0;

                foreach (var stock in group.Stocks)
                {
                    if (!existing.Contains(stock))
                    {
                        result.Add(stock);
                        added++;
                    }
                    else
                    {
                        result.Add(""); // placeholder for alignment
                    }

                    if (added == stockPerGroup)
                        break;
                }

                // Fill with empty strings if fewer than required stocks
                while (added++ < stockPerGroup)
                {
                    result.Add("");
                }
            }

            return result;
        }

        public GroupManager()
        {
            _groups = GroupRepository.LoadGroups();
        }

        public void Save()
        {
            GroupRepository.SaveGroups(_groups);
        }

        public List<GroupData> GetAll() => _groups;

        public GroupData FindByTitle(string title)
        {
            return _groups.FirstOrDefault(g => g.Title == title);
        }

        public List<string> GetStocksByTitle(string title, List<string> existing)
        {
            var result = new List<string>();
            var group = FindByTitle(title);
            if (group != null)
            {
                foreach (var stock in group.Stocks)
                {
                    if (!existing.Contains(stock))
                        result.Add(stock);
                }
            }
            return result;
        }

        public void AddGroup(GroupData group)
        {
            if (!_groups.Any(g => g.Title == group.Title))
                _groups.Add(group);
        }

        public void ReplaceGroups(List<GroupData> newGroups)
        {
            _groups = newGroups;
        }

        public int Count => _groups.Count;

        public void SortBy(Func<GroupData, double> selector)
        {
            RankingList = _groups.OrderByDescending(selector).ToList();
        }

        public GroupData FindGroupByStock(string stockCode)
        {
            return _groups.FirstOrDefault(g => g.Stocks.Contains(stockCode));
        }

        public static void gen_oGL_data(List<string> oGL_title, List<List<string>> oGL)
        {
            var groupList = new List<GroupData>();

            foreach (var stock in g.StockRepository.AllDatas)
            {
                stock.Misc.oGL_sequence_id = -1; // Optional tagging
            }

            int groupIndex = 0;

            for (int i = 0; i < oGL.Count; i++)
            {
                if (oGL[i].Count < 2)
                    continue;

                var items = new List<Tuple<double, string>>();

                foreach (var stockName in oGL[i])
                {
                    var stock = g.StockRepository.TryGetStockOrNull(stockName);
                    if (stock != null)
                    {
                        stock.Misc.oGL_sequence_id = groupIndex; // Optional
                        items.Add(Tuple.Create(stock.Statistics.시총, stock.Stock));
                    }
                }

                var sorted = items
                    .OrderByDescending(t => t.Item1)
                    .Select(t => t.Item2)
                    .ToList();

                if (sorted.Count < 2)
                    continue;

                var group = new GroupData(oGL_title[i])
                {
                    Stocks = sorted
                    // Other metrics (프누, 종누, etc.) will remain default (0.0) unless assigned later
                };

                groupList.Add(group);
                groupIndex++;
            }

            g.GroupManager.ReplaceGroups(groupList); // Store into GroupManager
        }
    }
}
