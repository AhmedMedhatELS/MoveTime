using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class CheckInTicket
    {
        public int CheckInOutID { get; set; }
        public string CheckInTime { get; set; } = null!;
        public string ExpectedCheckOutTime { get; set; } = null!;
        public List<string> ChildrensNames { get; set; } = [];
    }
}
