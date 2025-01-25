using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.CheckViews
{
    public class CheckOutView
    {
        public int Id { get; set; }

        public int ChildernNumber { get; set; }
        public List<ChildCOV> Childern { get; set; } = [];
        public List<ProductCVM> Products { get; set; } = [];

        public TimeSpan CheckIn { get; set; }
        public TimeSpan? ExpectedCheckout { get; set; }
        public TimeSpan? ActualCheckout { get; set; }

        public string EventName { get; set; } = null!;

        public int CheckInTotal { get; set; }
        public int ProductsPrice { get; set; }
        public int IntervalPrice { get; set; }
        public int EventPrice { get; set; }
        public PaymentMethod CheckInPayment {  get; set; }

        public bool IsDebt { get; set; } = false;
        public bool ActiveCheckIn { get; set; } = false;

        public int TotalMinutes { get; set; }

        #region for sub
        public string Name { get; set; } = null!;
        public int RemaningVists { get; set; }
        public string SubName { get; set; } = null!;

        public int PlanId { get; set; }

        public int Duration { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public bool IsDuration { get; set; }
        public List<string> Notes { get; set; } = [];
        #endregion

    }
}
