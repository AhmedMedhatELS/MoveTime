using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.ChildFile
{
    public class ChildFileView
    {
        public int ChildId { get; set; }
        public string Name { get; set; } = null!;
        public string ChildImageName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string BirthDay { get; set; } = null!;
        public int Oldyears { get; set; }

        public string Gender { get; set; } = null!;
        public string WhatsappNumber { get; set; } = null!;
        public string Phone { get; set; } = "لا يوجد رقم اخر";
        public string DisclaimerImage { get; set; } = null!;
        public string HealthCondition { get; set; } = "لا يوجد حاله صحية";
        public string DisableDescription { get; set; } = "لا يوجد اعاقة";
        public string ParentsNote { get; set; } = "لا يوجد ملاحظة من الوالدين";
        public string BlackListReason { get; set; } = "ليس على قائمة السوداء";
        public string ChildEscortReason { get; set; } = null!;
        public bool IsApproved { get; set; } = false;
        public bool IsAllowedToSubscribe { get; set; }
        public string SupervisorNote { get; set; } = "لا يوجد ملاحظة من المشرف";

        public string ParentEmail { get; set; } = "الطفل غير مرتبط بحساب";

        public string TotalTime { get; set; } = null!;

        public List<ChildSubscriptionsFileView> ChildSubscriptions { get; set; } = [];
        public List<CheckInOutFileView> CheckInOuts { get; set; } = [];
    }
}
