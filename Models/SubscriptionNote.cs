using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SubscriptionNote
    {
        #region Properities
        public int SubscriptionNoteId { get; set; }
        public string Note { get; set; } = null!;

        #endregion

        #region Foreign Keys
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;
        #endregion

        #region Relations

        #endregion
    }
}
