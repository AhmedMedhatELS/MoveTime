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
    public class PayAllDebt
    {
        [Required(ErrorMessage = "يجب تحديد رقم الطفل.")]
        public int ChildId { get; set; }

        [ValidateNever]
        public string Redirect { get; set; } = null!;

        [Required(ErrorMessage = "يجب تحديد طريقة الدفع.")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
