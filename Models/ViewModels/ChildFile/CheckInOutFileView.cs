using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.ChildFile
{
    public class CheckInOutFileView
    {
        public string CheckInBy { get; set; } = null!;

        public bool IsEscort { get; set; } = false;

        public string CheckIn { get; set; } = null!;
        public string ExpectedCheckout { get; set; } = "غير محدد";
        public string ActualCheckout { get; set; } = null!;
        public string DateLog { get; set; } = null!;

        public string InPayment { get; set; } = null!;
        public string OutPayment { get; set; } = null!;

        public int InTotal { get; set; }
        public int OutTotal { get; set; }

        public string EventName { get; set; } = "لم يتم اختيار فعالية";
        public int ChildrenNumber { get; set; }
    }
}
