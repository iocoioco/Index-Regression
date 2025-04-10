using New_Tradegy.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Core
{
    public class GroupManager
    {
        private List<GroupData> _groups = new List<GroupData>();

        public GroupManager()
        {
            _groups = GroupRepository.LoadGroups(); // Use static directly
        }

        public void Save()
        {
            GroupRepository.SaveGroups(_groups); // Use static directly
        }

        public List<GroupData> GetAll() => _groups;

        public GroupData FindByTitle(string title)
        {
            return _groups.FirstOrDefault(g => g.Title == title);
        }

        public void AddGroup(GroupData group)
        {
            _groups.Add(group);
        }

        public void SortByTotalScoreDescending()
        {
            _groups = _groups.OrderByDescending(g => g.TotalScore).ToList();
        }

 
    }

}
