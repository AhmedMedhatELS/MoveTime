using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Subscription
    {
        #region Properities
        public int SubscriptionId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations
        public ICollection<SubscriptionNote> Notes { get; set; } = [];
        public ICollection<SubscriptionPlan> Plans { get; set; } = [];
        #endregion
    }
}
