using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.CheckViews
{
    public class CheckIn
    {
        public string ChildIds { get; set; } = null!;
        public int EventId { get; set; }
        public string? SelectedProducts { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public PaymentMethod? PaymentMethod { get; set; } = null;
        public bool IsEscort { get; set; } = false;
    }
}
