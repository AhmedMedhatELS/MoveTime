using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class CheckInSubView
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int RemaningVists { get; set; }
        public string SubName { get; set; } = null!;

        public int ChildSubId { get; set; }

        public int ChildId { get; set; }

        public int Duration { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public bool IsDuration { get; set; }

        public string TotalTime { get; set; } = null!;
        public int TotalDebt { get; set; }

        public List<EventCVM> Events { get; set; } = [];
        public List<string> Notes { get; set; } = [];
    }
}
