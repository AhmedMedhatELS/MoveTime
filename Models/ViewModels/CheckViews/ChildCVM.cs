using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.CheckViews
{
    public class ChildCVM
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
        public string ImageName { get; set; } = null!;
        public string WhatsappNumber { get; set; } = null!;
        public string? HealthCondition { get; set; }
        public string? DisableDescription { get; set; }
        public string? ParentsNote { get; set; }
        public bool BlackList { get; set; } = false;
        public string? BlackListReason { get; set; } = null;
        public EscortReason EscortReason { get; set; } = EscortReason.None;
        public string EscortReasonString { get; set; } = "لا يوجد";
        public string? SupervisorNote { get; set; }
        public string TotalTime { get; set; } = null!;
        public int TotalDebt { get; set; } = 0;
        public List<string> CheckInOutNotes { get; set; } = [];
    }
}
