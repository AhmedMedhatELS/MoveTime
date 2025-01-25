using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class SubscribedChildPlan
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = null!;
        public int RemainingVisits { get; set; }

        public int Duration { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public bool IsDuration { get; set; }
    }
}
