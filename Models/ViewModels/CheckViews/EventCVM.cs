﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.CheckViews
{
    public class EventCVM
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public string Name { get; set; } = null!;
    }
}
