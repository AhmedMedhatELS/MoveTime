using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CheckProduct
    {
        #region Properities
        public int CheckProductId { get; set; }
        public int Quantity { get; set; }
        #endregion

        #region Foreign Keys
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int CheckInOutId { get; set; }
        public CheckInOut CheckInOut { get; set; } = null!;
        #endregion

        #region Relations

        #endregion
    }
}
