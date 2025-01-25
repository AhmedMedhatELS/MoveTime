using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class EditHour
    {
        public int Id { get; set; }
        public int HourNumber { get; set; }
        public bool AsPrevious { get; set; }
        public List<EditMinuteRange> MinutesRangeList { get; set; } = [];
        public SaveHour? SaveHour { get; set; }
    }
}
