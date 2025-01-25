using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class ChildSubscription
    {
        #region Properities
        public int ChildSubscriptionId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime SubscriptionBegin {  get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public int VisitsNumber { get; set; }
        public int Total { get; set; }
        public int Payed { get; set; }
        public int Remaining { get; set; }
        public string? BillImageName { get; set; }
        public bool HaveDebt { get; set; } = false;
        public PaymentMethod? DebtPaymentMethod { get; set; } = null;
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        #endregion

        #region Foreign Keys
        public int ChildId { get; set; }
        public Child Child { get; set; } = null!;
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        #endregion

        #region Relations

        #endregion
    }
}
