using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CheckChild
    {
        #region Properities
        public int CheckChildId { get; set; }
        public string? Note { get; set; }
        #endregion

        #region Foreign Keys
        public int ChildId { get; set; }
        public Child Child { get; set; } = null!;
        public int CheckInOutId { get; set; }
        public CheckInOut ChildInOut { get; set; } = null!;
        #endregion

        #region Relations

        #endregion
    }
}
