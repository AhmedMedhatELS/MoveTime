using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Models.ViewModels.CheckViews;
using Shared;
using Utility;

namespace MoveTime.Hubs
{
    public class CheckInOutHub(CalculatePrice calculatePrice,
        IServiceProvider serviceProvider,
        SubscriptionCheck subscriptionCheck,
        Statistics statistics) : Hub
    {
        private readonly CalculatePrice _calculatePrice = calculatePrice;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SubscriptionCheck _subscriptionCheck = subscriptionCheck;
        private readonly Statistics _statistics = statistics;

        public async Task IntervalPrice(string begin,string expected, string end)
        {
            if (TimeSpan.TryParse(begin, out var beginTime) && TimeSpan.TryParse(end, out var endTime))
            {
                var Isexpected = TimeSpan.TryParse(expected, out var expectedtime);

                var total = _calculatePrice.GetPrice(beginTime, endTime);
                var checkinPrice = _calculatePrice.GetPrice(beginTime, expectedtime);
                
                int? price = total == null ? null : !Isexpected ? total : total - checkinPrice;
                
                int minutes = total == null ? 0 : (int)(endTime - beginTime).TotalMinutes;

                await Clients.Caller.SendAsync("CalculatedPrice", price, minutes);
            }
            else
            {
                throw new ArgumentException("Invalid time format");
            }
        }

        public async Task IntervalSubscriptionPrice(string begin, string expected, string end,int planId)
        {
            if (TimeSpan.TryParse(begin, out var beginTime) &&
                TimeSpan.TryParse(end, out var endTime))
            {
                var Isexpected = TimeSpan.TryParse(expected, out var expectedtime);

                var total = _calculatePrice.GetSubscriptionIntervalprice(beginTime, endTime, planId);
                var checkinPrice = _calculatePrice.GetSubscriptionIntervalprice(beginTime, expectedtime, planId);

                int? price = total == null ? null : !Isexpected ? total : total - checkinPrice;
                
                int minutes = total == null ? 0 : (int)(endTime - beginTime).TotalMinutes;

                await Clients.Caller.SendAsync("CalculatedSubPrice", price, minutes);
            }
            else
            {
                throw new ArgumentException("Invalid time format or Invalid Plan Id");
            }
        }

        public async Task NewCheckIn(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var checkin = unitOfWork.Repository<CheckInOut>().GetOne(
                e => e.CheckInOutId == id,
                childern => childern.Children);

            CheckInTable? inTable = null;

            if (checkin != null)
            {
                inTable = new()
                {
                    Id = id,
                    CheckInTime = checkin.CheckIn,
                    CheckOutTime = checkin.ExpectedCheckout
                };

                foreach (var child in checkin.Children)
                    inTable.ChildChecks.Add(new ChildCheckInTable
                    {
                        Id = child.ChildId,
                        ImageName = child.ChildImageName,
                        Name = child.Name
                    });
            }

            await Clients.All.SendAsync("ShowNewCheckIn", inTable);
        }

        public async Task LoggedOutRemove(int id)
        {
            await Clients.All.SendAsync("RemoveLogged", id);
        }

        public async Task ChildrenSearch(string SearchText, string ChildIds)
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            List<ChildCVM> childCVMs = [];

            //var IsId = int.TryParse(SearchText, out var id);
            List<int> Ids = [];

            if (!string.IsNullOrEmpty(ChildIds))
                Ids = ChildIds.Split('-').Select(id => int.Parse(id)).ToList();

            var childern = _unitOfWork.Repository<Child>().Get(
                e =>
                    e.IsApproved && !e.IsDeleted && !Ids.Contains(e.ChildId) &&
                    (e.Name.ToLower().Contains(SearchText.ToLower()) ||
                     e.WhatsappNumber.Contains(SearchText)),
                check => check.CheckInOuts
                    );

            #region childs view
            foreach (var child in childern)
            {
                if (child.CheckInOuts.Any(e => e.Status == CheckStatus.In)) continue;

                child.CheckChildren = _unitOfWork.Repository<CheckChild>().Get(
                    e => e.ChildId == child.ChildId && e.Note != null).ToList() ?? [];

                childCVMs.Add(new ChildCVM
                {
                    ID = child.ChildId,
                    BlackList = child.BlackList,
                    BlackListReason = string.IsNullOrEmpty(child.BlackListReason) ? "لا يوجد" : child.BlackListReason,
                    DisableDescription = string.IsNullOrEmpty(child.DisableDescription) ? "لا يوجد اعاقه" : child.DisableDescription,
                    EscortReasonString = EscortReasonTranslations.Translations[child.EscortReason].ToString(),
                    HealthCondition = string.IsNullOrEmpty(child.HealthCondition) ? "لا يوجد مشاكل صحية" : child.HealthCondition,
                    ImageName = child.ChildImageName,
                    Name = child.Name,
                    ParentsNote = string.IsNullOrEmpty(child.ParentsNote) ? "لا توجد ملاحظات" : child.ParentsNote,
                    SupervisorNote = string.IsNullOrEmpty(child.SupervisorNote) ? "لا توجد ملاحظات" : child.SupervisorNote,
                    WhatsappNumber = child.WhatsappNumber,
                    TotalTime = _statistics.ChildOverAllTime(child.ChildId),
                    TotalDebt = _statistics.ChildDebtAmount(child.ChildId)
                });

                foreach (var note in child.CheckChildren)
                    if (note.Note != null)
                        childCVMs.Last().CheckInOutNotes.Add(note.Note);
            }
            #endregion
            await Clients.Caller.SendAsync("ChildrenSearchResult", childCVMs);
        }

        public async Task ProductSearch(string searchText,string productsIds)
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            List<ProductCVM> ProductCVMs = [];
            List<int> Ids = [];

            if(!string.IsNullOrEmpty(productsIds))
            {
                var productQ = productsIds.Split('|');

                foreach (var item in productQ)                
                    Ids.Add(int.Parse(item.Split("-")[0]));                
            }

            var products = _unitOfWork.Repository<Product>().Get(
                e =>
                    !e.IsDeleted && e.Quantity > 0 && !Ids.Contains(e.ProductId) &&
                        e.Name.ToLower().Contains(searchText.ToLower())
                    );

            #region products view
            foreach (var product in products)
                ProductCVMs.Add(new ProductCVM
                {
                    Id = product.ProductId,
                    Name = product.Name,
                    ImageName = product.ImageName,
                    Quantity = product.Quantity ?? 0,
                    Price = product.Price
                });
            #endregion

            await Clients.Caller.SendAsync("ProductsSearchResult", ProductCVMs);
        }

        public async Task ChildrenSearchSub(string searchText)
        {
            await Clients.Caller.SendAsync("ChildrenSubResult", _subscriptionCheck.GetChildsForCheckInBySubscription(searchText));
        }
    }
}
