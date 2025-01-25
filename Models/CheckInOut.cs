using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class CheckInOut
    {
        #region Properities
        public int CheckInOutId { get; set; }

        public CheckStatus Status { get; set; } = CheckStatus.In;
        public bool HaveDebt { get; set; } = false;
        public bool IsEscort { get; set; } = false;

        public TimeSpan CheckIn { get; set; }
        public TimeSpan? ExpectedCheckout { get; set; }
        public TimeSpan? ActualCheckout { get; set; }
        public DateTime DateLog { get; set; } = DateTime.Today;

        public PaymentMethod? InPayment { get; set; }
        public PaymentMethod? OutPayment { get; set; }

        public int InTotal { get; set; }
        public int OutTotal { get; set; }

        public CheckInBy CheckInBy { get; set; } = CheckInBy.ساعة;
        public bool ISAlerted { get; set; } = false;
        #endregion

        #region Foreign Keys
        public int? ChildSubscriptionId { get; set; } = null;
        public ChildSubscription ChildSubscription { get; set; } = null!;

        public int? EventId { get; set; }
        public Event Event { get; set; } = null!;
        #endregion

        #region Relations
        public ICollection<CheckProduct> CheckProducts { get; set; } = [];
        public ICollection<Product> Products { get; set; } = [];

        public ICollection<CheckChild> CheckChildren { get; set; } = [];
        public ICollection<Child> Children { get; set; } = [];
        #endregion
    }
}
