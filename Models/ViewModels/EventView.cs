using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ViewModels
{
    public class EventView
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "اسم الحدث مطلوب.")]
        public string EventName { get; set; } = null!;

        [Required(ErrorMessage = "وصف الحدث مطلوب.")]
        public string EventDescription { get; set; } = null!;

        [Required(ErrorMessage = "السعر مطلوب.")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من 0.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "حالة الحدث مطلوبة.")]
        public EventStatus Status { get; set; }
        [ValidateNever]
        public List<Event> Events { get; set; } = [];
    }
}
