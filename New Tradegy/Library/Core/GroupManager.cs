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
        private GroupRepository _repository;

        public GroupManager(GroupRepository repository)
        {
            _repository = repository;
            _groups = _repository.LoadGroups(); // Load at start
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

        public void Save()
        {
            _repository.SaveGroups(_groups);
        }
    }

}
