using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class MassagesView
    {
        public string Massage { get; set; } = null!;
        public string NumbersToSendTo { get; set; } = null!;
        public List<WhichChild> Children { get; set; } = [];
    }
}
