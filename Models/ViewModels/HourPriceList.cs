using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HourPriceList
    {
        public string HoursHead { get; set; } = null!;
        public List<string> HourRangePrices { get; set; } = [];
    }
}
