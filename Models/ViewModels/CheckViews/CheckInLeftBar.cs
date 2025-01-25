using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class CheckInLeftBar
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageName { get; set; } = null!;
        public string CheckOutTime { get; set; } = null!;
        public bool IsItGroub { get; set; } = false;
        public int Status { get; set; }
    }
}
