using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class CheckInView
    {
        public List<ChildCVM> ChildCVMs { get; set; } = [];
        public List<EventCVM> EventCVMs { get; set;} = [];
        public List<ProductCVM> ProductCVMs { get; set;} = [];
        public CheckIn CheckIn { get; set; } = null!;
    }
}
