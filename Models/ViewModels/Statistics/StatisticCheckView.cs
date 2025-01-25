using Models.ViewModels.CheckViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.Statistics
{
    public class StatisticCheckView
    {
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
        
        public string CheckInBy { get; set; } = null!;

        public int InTotal { get; set; }
        public int OutTotal { get; set; }
        public bool InDebt { get; set; } = false;
        public bool OutDebt { get; set; } = false;

        public List<ChildCheckInTable> ChildChecks { get; set; } = [];
    }
}
