using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class ProductCVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageName { get; set; } = null!;
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
