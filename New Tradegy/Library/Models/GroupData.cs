using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library.Models
{
    public class GroupData // Sector group of stocks
    {
        public string Title { get; set; } // Group title (e.g., Semiconductor)
        public List<string> Stocks { get; set; } = new List<string>(); // Stock codes in the group

        // Overall Scores
        public double TotalScore { get; set; }
        public double 수평 { get; set; }  // e.g., 수급 평점 (supply rating)
        public double 강평 { get; set; }  // e.g., 강도 평점 (strength rating)

        // Sector metrics (Korean for now, you can rename if needed)
        public double 푀누 { get; set; }
        public double 종누 { get; set; }
        public double 거분 { get; set; }
        public double 배합 { get; set; }
        public double 푀분 { get; set; }
        public double 배차 { get; set; }
        public double 가증 { get; set; }
        public double 분거 { get; set; }
        public double 상순 { get; set; }
        public double 저순 { get; set; }

        // Optional: Constructor
        public GroupData(string title)
        {
            Title = title;
        }
    }
}
