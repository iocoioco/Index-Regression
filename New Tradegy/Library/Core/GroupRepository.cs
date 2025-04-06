using New_Tradegy.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Core
{
    public class GroupRepository
    {
        public List<GroupData> LoadGroups()
        {
            // TODO: Load from file or DB
            return new List<GroupData>();
        }

        public void SaveGroups(List<GroupData> groups)
        {
            // TODO: Save to file or DB
        }
    }

}
