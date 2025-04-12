using New_Tradegy.Library.Core;
using New_Tradegy.Library.Models;
using System.Collections.Generic;
using System.Linq;
using System;

public class GroupManager
{
    private List<GroupData> _groups;
    public List<GroupData> RankingList { get; private set; } = new List<GroupData>();

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
}
