using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels
{
    public class AdminSubscriptionPlanView
    {
        public int Price { get; set; }
        public int VisitsNumber { get; set; }
        public int ActiveDays { get; set; }
        public List<Days> SelectedDaysOfWeek { get; set; } = [];
        public int Duration { get; set; }
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }
        public bool IsDuration { get; set; }
    }
}
