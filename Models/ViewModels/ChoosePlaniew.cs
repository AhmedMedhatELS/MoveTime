using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ChoosePlaniew
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public int VisitsNumber { get; set; }
        public int ActiveDays { get; set; }
        public string DaysOfWeek { get; set; } = null!;
        public int Duration { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public bool IsDuration { get; set; }
    }
}
