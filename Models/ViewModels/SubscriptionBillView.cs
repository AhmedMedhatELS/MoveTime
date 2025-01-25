using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels
{
    public class SubscriptionBillView
    {
        #region inputs
        public int ChildId { get; set; }
        public int PlanId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime SubscriptionBegin { get; set; } = DateTime.Now;
        public DateTime SubscriptionEnd { get; set; }
        public int VisitsNumber { get; set; }
        public int Total { get; set; }
        public int Payed { get; set; }
        public int Remaining { get; set; }
        public IFormFile? BillImageFile { get; set; }
        #endregion

        #region view
        public string BillHeader { get; set; } = null!;
        public string ChildName { get; set; } = null!;
        public string SubscriptionDescription { get; set; } = null!;
        public List<string> Notes { get; set; } = [];
        public string ContactNumber { get; set; } = null!;
        public string DaysOfWeek { get; set; } = null!;
        public int Duration { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public bool IsDuration { get; set; }
        #endregion
    }
}
