using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class SubscribedChild
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageName { get; set; } = null!;
        public bool BlackList { get; set; } = false;
        public string TotalTime { get; set; } = null!;
        public int TotalDebt { get; set; }
        public List<SubscribedChildPlan> ChildPlans { get; set; } = [];
        public string? ChildPlansJson { get; set; }
    }
}
