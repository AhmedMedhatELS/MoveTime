using DataAccess.Repository.IRepository;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Utility;
using Models.ViewModels.CheckViews;
using Newtonsoft.Json;

namespace Shared
{
    public class SubscriptionCheck(IUnitOfWork unitOfWork,Statistics statistics) : BaseService(unitOfWork)
    {
        //sunday = 1
        private readonly Days CurrentDay = (Days)(DateTime.Now.DayOfWeek + 1);

        private readonly Statistics _Statistics = statistics;

        public IEnumerable<SubscriptionPlan> GetActivePlansForToday()
        {
            var plans = _unitOfWork.Repository<SubscriptionPlan>().Get(null);

            return plans.Where(e => e.DaysOfWeek.Any(e => e == CurrentDay 
                                                     || e == Days.AllDays));
        }

        public IEnumerable<ChildSubscription> GetchildrenWithActivePlansForToday(string searchText)
        {
            var plans = GetActivePlansForToday().ToList();

            IEnumerable<ChildSubscription> children = [];

            if (plans.Count <= 0 || string.IsNullOrEmpty(searchText)) return children;

            //var IsId = int.TryParse(searchText, out var id);

            children = _unitOfWork.Repository<ChildSubscription>().Get(
                e => e.Status == SubscriptionStatus.Active &&
                (e.Child.Name.ToLower().Contains(searchText.ToLower()) ||
                 e.Child.WhatsappNumber.Contains(searchText)),
                child => child.Child,
                plan => plan.SubscriptionPlan,
                sub => sub.SubscriptionPlan.Subscription);

            if (children.ToList().Count <= 0) return children;

            var todayDate = DateTime.Now;
            List<ChildSubscription> childrenUpdata = [];

            children = children.Where(child =>
            {
                if (child.SubscriptionEnd < todayDate)
                {
                    child.Status = SubscriptionStatus.Expired;
                    childrenUpdata.Add(child);
                    return false;
                }

                return plans.Contains(child.SubscriptionPlan);
            });

            if (childrenUpdata.Count > 0)
                _unitOfWork.Repository<ChildSubscription>().UpdateRange(childrenUpdata);

            return children;
        }

        public IEnumerable<SubscribedChild> GetChildsForCheckInBySubscription(string searchText)
        {
            var children = GetchildrenWithActivePlansForToday(searchText).ToList();

            List<SubscribedChild> SubscribedChildren = [];

            if (children.Count <= 0) return SubscribedChildren;

            Dictionary<int, SubscribedChild> pairs = [];

            foreach(var child in children)
            {
                var checkin = _unitOfWork.Repository<CheckInOut>().Get(
                    e => e.CheckChildren.Any(e => e.ChildId == child.ChildId) &&
                    e.Status == CheckStatus.In).ToList();

                if (checkin.Count > 0) continue;

                if(!pairs.TryGetValue(child.ChildId, out SubscribedChild? value))
                {
                    value = new()
                    {
                        Id = child.ChildId,
                        ImageName = child.Child.ChildImageName,
                        Name = child.Child.Name,
                        BlackList = child.Child.BlackList,
                        TotalTime = _Statistics.ChildOverAllTime(child.ChildId),
                        TotalDebt = _Statistics.ChildDebtAmount(child.ChildId),
                    };

                    pairs[child.ChildId] = value;

                    SubscribedChildren.Add(value);
                }

                value.ChildPlans.Add(new SubscribedChildPlan
                {
                    Id = child.ChildSubscriptionId,
                    PlanName = child.SubscriptionPlan.Subscription.Name,
                    RemainingVisits = child.VisitsNumber,
                    IsDuration = child.SubscriptionPlan.IsDuration,
                    Duration = child.SubscriptionPlan.Duration,
                    From = child.SubscriptionPlan.From != null ? (new DateTime(1, 1, 1).Add((TimeSpan)child.SubscriptionPlan.From)).ToString("h:mm tt") : "",
                    To = child.SubscriptionPlan.To != null ? (new DateTime(1, 1, 1).Add((TimeSpan)child.SubscriptionPlan.To)).ToString("h:mm tt") : ""
                });       
            }

            foreach (var plans in SubscribedChildren)
                plans.ChildPlansJson = JsonConvert.SerializeObject(plans.ChildPlans);

            return SubscribedChildren;
        }
    }
}
