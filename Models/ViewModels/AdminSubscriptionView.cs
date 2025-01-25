using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels
{
    public class AdminSubscriptionView
    {
        [ValidateNever]
        public int SubscriptionId { get; set; }
        [Required(ErrorMessage = "اسم الاشتراك مطلوب.")]
        public string SubscriptionName { get; set; } = null!;
        [Required(ErrorMessage = "وصف الاشتراك مطلوب.")]
        public string SubscriptionDescription { get; set; } = null!;
        [ValidateNever]
        public List<string> SubscriptionNotes { get; set; } = [];
        public List<Days> SubscriptionDaysOfWeek { get; } = Enum.GetValues(typeof(Days)).Cast<Days>().ToList();
        public List<AdminSubscriptionPlanView> Plans { get; set; } = [];
    }
}
