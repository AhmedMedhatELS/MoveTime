using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.CheckViews
{
    public class CheckOut
    {
        public int Id { get; set; }
        public TimeSpan ActualCheckout { get; set; }
        public PaymentMethod? CheckOutPayment { get; set; }
        public bool AddCheckInPrice { get; set; } = false;
        public string SelectedProducts { get; set; } = null!;
        public List<string> ChildsNotes { get; set; } = [];
    }
}
