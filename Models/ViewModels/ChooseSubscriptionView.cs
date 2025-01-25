using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ChooseSubscriptionView
    {
        public string Name { get; set; } = null!;
        public List<ChoosePlaniew> ChoosePlaniews { get; set; } = [];
    }
}
