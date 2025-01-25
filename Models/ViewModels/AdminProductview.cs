using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AdminProductview
    {
        [ValidateNever]
        public List<Product> Products { get; set; } = [];

        public int ProductId { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "السعر مطلوب.")]
        [Range(1, int.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من الصفر.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "الصورة مطلوبة.")]
        public IFormFile Imagefile { get; set; } = null!;

        [Required(ErrorMessage = "الكمية مطلوبة.")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون رقمًا صحيحًا أكبر من الصفر.")]
        public int? Quantity { get; set; }
    }

}
