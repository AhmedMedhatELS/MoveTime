using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels
{
    public class SaveHour
    {
        public int Id { get; set; }
        public string HourData { get; set; } = null!;
        public WhichShift WhichShift { get; set; }
    }
}
