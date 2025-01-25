using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Models.ViewModels.CheckViews;
using Models.ViewModels.ChildFile;
using MoveTime.Models;
using Newtonsoft.Json;
using Shared;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utility;

namespace MoveTime.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class HomeController(UserManager<ApplicationUser> userManager,
        SubscriptionCheck subscriptionCheck,
        ILogger<HomeController> logger,
        IUnitOfWork unitOfWork,
        CalculatePrice calculatePrice,
        Statistics statistics) : Controller
    {
        private readonly SubscriptionCheck _subscriptionCheck = subscriptionCheck;
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly CalculatePrice _calculatePrice = calculatePrice;
        private readonly Statistics _statistics = statistics;

        public IActionResult Index()
        {
            var id = _userManager.GetUserId(User);

            if (id == null) return NotFound();

            List<WhichChild> view = [];

            var children = _unitOfWork.Repository<Child>().Get(
                e => e.ParentId == id && !e.IsDeleted
                ).ToList();

            if (children != null && children.Count > 0)
                foreach (var child in children)
                    view.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        IsApproved = child.IsApproved,
                        Name = child.Name,
                        OnBlackList = child.BlackList
                    });

            return View(view);
        }

        #region GetLoggedIn
        public IActionResult GetLoggedIn()
        {
            List<CheckInTable> view = [];

            var parentId = _userManager.GetUserId(User);

            #region check in by hour
            var loggedByHour = _unitOfWork.Repository<CheckInOut>().Get(
                e => e.Status == CheckStatus.In,
                childern => childern.Children);

            foreach (var check in loggedByHour)
            {
                if (check.Children.All(e => e.ParentId != parentId))
                    continue;

                view.Add(new CheckInTable
                {
                    CheckInTime = check.CheckIn,
                    CheckOutTime = check.ExpectedCheckout,
                    Id = check.CheckInOutId,
                    CheckInBy = check.CheckInBy.ToString()
                });

                foreach (var child in check.Children)
                    view.Last().ChildChecks.Add(new ChildCheckInTable
                    {
                        Id = child.ChildId,
                        ImageName = child.ChildImageName,
                        Name = child.Name,
                    });
            }
            #endregion

            return View(view);
        }
        #endregion
    }
}
