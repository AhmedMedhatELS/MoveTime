using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.ChildFile
{
    public class EditChild

    {
        #region Child

        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [RegularExpression(@"^[\u0621-\u064A\u0660-\u0669a-zA-Z\s]+$", ErrorMessage = "يجب أن يحتوي الاسم على أحرف عربية أو إنجليزية فقط")]
        public string Name { get; set; } = null!;

        [ValidateNever]
        public IFormFile ChildImageFile { get; set; } = null!;
        [ValidateNever]
        public string ChildImageName { get; set; } = null!;

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

        #endregion
        #region Employee part
        [ValidateNever]
        public bool IsEscort { get; set; }
        [ValidateNever]
        public EscortReason EscortReason { get; set; } = EscortReason.None;
        [ValidateNever]
        public bool IsApproved { get; set; } = false;
        [ValidateNever]
        public bool IsAllowedToSubscribe { get; set; }
        [ValidateNever]
        public string? SupervisorNote { get; set; }

        public bool BlackList { get; set; } = false;
        public string? BlackListReason { get; set; } = null;
        #endregion
        #region for Parent Account
        [ValidateNever]
        public string? Email { get; set; }
        //in case of new account
        [ValidateNever]
        public string? Password { get; set; }
        [ValidateNever]
        public string? ConfirmPassword { get; set; }
        #endregion
    }

}
