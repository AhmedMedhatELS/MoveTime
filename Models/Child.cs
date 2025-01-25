using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class Child
    {
        #region Properities
        public int ChildId { get; set; }
        public string Name { get; set; } = null!;
        public string ChildImageName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public DateOnly BirthDay { get; set; }
        [NotMapped]
        public int Oldyears
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var age = today.Year - BirthDay.Year;

                return today < BirthDay.AddYears(age) ? --age : age;
            }
        }
        public Gender Gender { get; set; }
        public string WhatsappNumber { get; set; } = null!;
        public string? Phone { get; set; }
        public string? DisclaimerImage { get; set; }
        public bool HaveHealthCondition { get; set; }
        public string? HealthCondition { get; set; }
        public bool IsDisabled { get; set; }
        public string? DisableDescription { get; set; }
        public string? ParentsNote { get; set; }
        public bool BlackList { get; set; } = false;
        public string? BlackListReason { get; set; } = null; 
        public bool IsEscort { get; set; }
        public EscortReason EscortReason { get; set; } = EscortReason.None;
        public bool IsApproved { get; set; } = false;
        public bool IsAllowedToSubscribe { get; set; }
        public string? SupervisorNote { get; set; }
        public bool IsDeleted { get; set; } = false;
        #endregion

        #region Foreign Keys
        public string? ParentId { get; set; }
        public ApplicationUser Parent { get; set; } = null!;
        #endregion

        #region Relations
        public ICollection<SubscriptionPlan> subscriptionPlans { get; set; } = [];
        public ICollection<ChildSubscription> ChildSubscriptions { get; set; } = [];

        public ICollection<CheckChild> CheckChildren { get; set; } = [];
        public ICollection<CheckInOut> CheckInOuts { get; set; } = [];
        #endregion
    }
}
