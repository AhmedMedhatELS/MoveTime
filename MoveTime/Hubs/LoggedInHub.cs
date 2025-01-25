using DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Models.ViewModels;
using Models;
using Shared;
using Models.ViewModels.CheckViews;
using Utility;

namespace MoveTime.Hubs
{
    public class LoggedInHub(CalculatePrice calculatePrice,
        IServiceProvider serviceProvider,
        SubscriptionCheck subscriptionCheck) : Hub
    {
        private readonly CalculatePrice _calculatePrice = calculatePrice;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SubscriptionCheck _subscriptionCheck = subscriptionCheck;

        public async Task LoggedIn()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var loggedInList = _unitOfWork.Repository<CheckInOut>().Get(
                e => e.Status == CheckStatus.In,ch => ch.Children
                ).ToList();

            List<CheckInLeftBar> children = [];

            foreach (var check in loggedInList) 
            {
                children.Add(new CheckInLeftBar
                {
                    Id = check.CheckInOutId,
                    IsItGroub = check.Children.Count > 1,
                    CheckOutTime = check.ExpectedCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)check.ExpectedCheckout)).ToString("h:mm tt") : "غير محدد",
                    ImageName = check.Children.ToList()[0].ChildImageName,
                    Name = check.Children.ToList()[0].Name,
                    Status = check.ExpectedCheckout == null ? 0 : check.ExpectedCheckout < DateTime.Now.TimeOfDay ? 2 : 1
                });
            }

            await Clients.Caller.SendAsync("LoggedInResult", children);
        }

        public async Task LoggedOutRemoveLeftBar(int id)
        {
            await Clients.All.SendAsync("RemoveLoggedLeftBar", id);
        }

        public async Task IsAnyOneExceededItsTime()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<CheckInOut>().Get(
                e => e.Status == CheckStatus.In && !e.ISAlerted && e.ExpectedCheckout != null
                && e.ExpectedCheckout < DateTime.Now.TimeOfDay, ch => ch.Children
                ).ToList();

            List<CheckInLeftBar> view = [];

            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    child.ISAlerted = true;

                    view.Add(new CheckInLeftBar
                    {
                        Id = child.CheckInOutId,
                        Name = child.Children.ToList()[0].Name,
                        ImageName = child.Children.ToList()[0].ChildImageName,
                        IsItGroub = child.Children.ToList().Count > 0,
                        CheckOutTime = child.ExpectedCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)child.ExpectedCheckout)).ToString("h:mm tt") : "غير محدد",
                        Status = child.ExpectedCheckout == null ? 0 : child.ExpectedCheckout < DateTime.Now.TimeOfDay ? 2 : 1
                    });
                }

                _unitOfWork.Repository<CheckInOut>().UpdateRange(children);
            }

            await Clients.All.SendAsync("ChildrenExceededTimeResult", view);
        }

    }
}
