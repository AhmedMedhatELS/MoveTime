using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ChildDebt
    {
        public int Id { get; set; }
        public string DebtName { get; set; } = null!;
        public string DebtDate { get; set; } = null!;
        public int Amount { get; set; }
    }
}
