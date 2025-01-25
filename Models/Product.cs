using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Product
    {
        #region Properities
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public int Price { get; set; }
        public string ImageName { get; set; } = null!;
        public int? Quantity { get; set; }
        public bool IsDeleted { get; set; } = false;
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations
        public ICollection<CheckProduct> CheckProducts { get; set; } = [];
        public ICollection<CheckInOut> CheckInOuts { get; set; } = [];
        #endregion
    }
}
