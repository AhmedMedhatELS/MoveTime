using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.ChildFile
{
    public class ChildSubscriptionsFileView
    {
        public int ID { get; set; }
        public string SubName { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string SubscriptionBegin { get; set; } = null!;
        public string SubscriptionEnd { get; set; } = null!;
        public int VisitsNumber { get; set; }

        public int Total { get; set; }
        public int Payed { get; set; }
        public int Remaining { get; set; }
        public bool DebtPayed { get; set; } = false;
        public string? DebtPaymentMethod { get; set; }
        public string Status { get; set; } = null!;

        public int Duration { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public bool IsDuration { get; set; }
    }
}
