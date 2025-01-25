using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.CheckViews
{
    public class CheckInTable
    {
        public int Id { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string CheckInBy { get; set; } = null!;
        public List<ChildCheckInTable> ChildChecks { get; set; } = [];
    }
}
