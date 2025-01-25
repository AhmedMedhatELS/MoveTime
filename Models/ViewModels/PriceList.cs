using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class PriceList
    {
        public string MorningHeader { get; set; } = null!;
        public string EveningHeader { get; set; } = null!;
        public List<HourPriceList> MorningHours { get; set; } = [];
        public List<HourPriceList> EveningHours { get; set; } = [];
    }
}
