using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class WhichChild
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ChildImage { get; set; } = null!;
        public bool OnBlackList { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public string WhatsAppNumber { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public int TotalDept { get; set; }
    }
}
