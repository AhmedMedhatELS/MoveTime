using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using Utility;

namespace Models
{
    public class ShiftHour
    {
        #region Properities
        public int ShiftHourId { get; set; }       
        public int HourNumber { get; set; }
        public WhichShift WhichShift { get; set; }
        public bool AsPrevious { get; set; } = false;
        public bool IsCompleated { get; set; } = false;
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations
        public ICollection<MinuteRange> MinuteRanges { get; set; } = [];
        #endregion
    }
}