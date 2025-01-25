using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.UserViews
{
    public class NewChildUser
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [RegularExpression(@"^[\u0621-\u064A\u0660-\u0669a-zA-Z\s]+$", ErrorMessage = "يجب أن يحتوي الاسم على أحرف عربية أو إنجليزية فقط")]
        public string Name { get; set; } = null!;
        [ValidateNever]
        public IFormFile ChildImageFile { get; set; } = null!;
        [Required(ErrorMessage = "رقم الهوية الوطنية مطلوب")]
        public string NationalId { get; set; } = null!;
        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly BirthDay { get; set; }
        [ValidateNever]
        public int Oldyears { get; set; }
        [Required(ErrorMessage = "الجنس مطلوب")]
        public Gender Gender { get; set; }
        [Required(ErrorMessage = "رقم الواتساب مطلوب")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "رقم الواتساب غير صالح")]
        public string WhatsappNumber { get; set; } = null!;
        [ValidateNever]
        public string? Phone { get; set; }
        [ValidateNever]
        public IFormFile DisclaimerImage { get; set; } = null!;
        [ValidateNever]
        public bool HaveHealthCondition { get; set; }
        [ValidateNever]
        public string? HealthCondition { get; set; }
        [ValidateNever]
        public bool IsDisabled { get; set; }
        [ValidateNever]
        public string? DisableDescription { get; set; }
        [ValidateNever]
        public string? ParentsNote { get; set; }
    }
}
