using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using Utility;

namespace Models
{
    public class SubscriptionPlan
    {
        #region Properities
        public int SubscriptionPlanId { get; set; }
        public int Price { get; set; }
        public int VisitsNumber { get; set; }
        public int ActiveDays { get; set; }
        [NotMapped]
        public List<Days> DaysOfWeek { get; set; } = [];
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private string WhichDays
        {
            get => JsonConvert.SerializeObject(DaysOfWeek);
            set => DaysOfWeek = JsonConvert.DeserializeObject<List<Days>>(value) ?? [];
        }
        public int Duration { get; set; }
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }
        public bool IsDuration {  get; set; }
        #endregion

        #region Foreign Keys
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;
        #endregion

        #region Relations
        public ICollection<Child> Children { get; set; } = [];
        public ICollection<ChildSubscription> ChildSubscriptions { get; set; } = [];
        #endregion
    }
}
