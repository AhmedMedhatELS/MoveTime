using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels.ChildFile
{
    public class SubChildEdit
    {
        public int SubChildId { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime SubscriptionBegin {  get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public int VisitsNumber { get; set; }
    }
}
