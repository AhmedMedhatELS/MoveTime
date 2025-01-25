using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class Event
    {
        #region Properities
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string EventDescription { get; set; } = null!;
        public int Price { get; set; }
        public EventStatus Status { get; set; }
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations
        public ICollection<CheckInOut> CheckInOuts { get; set; } = [];
        #endregion
    }
}
