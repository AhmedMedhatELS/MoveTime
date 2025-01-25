using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.Statistics
{
    public class StatisticsTableView
    {
        public int Total { get; set; }
        public int Payed { get; set; }
        public int Dept { get; set; }
        public int KidsNumber { get; set; }

        public List<StatisticCheckView> CheckViews { get; set; } = [];
    }
}
