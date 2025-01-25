using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Models.ViewModels.CheckViews;
using MoveTime.Hubs;
using Newtonsoft.Json;
using Shared;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class CheckInOutController(IUnitOfWork unitOfWork,
        CalculatePrice calculatePrice,
        UserManager<ApplicationUser> userManager,
        IHubContext<CheckInOutHub> hubContext,
        IHubContext<LoggedInHub> hubContextlog,
        SubscriptionCheck subscriptionCheck,
        Statistics statistics) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CalculatePrice _calculatePrice = calculatePrice;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IHubContext<CheckInOutHub> _hubContext = hubContext;
        private readonly IHubContext<LoggedInHub> _hubContextlog = hubContextlog;
        private readonly SubscriptionCheck _subscriptionCheck = subscriptionCheck;
        private readonly Statistics _statistics = statistics;

        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        private readonly string morningName = ConstantNames.MorningName;
        #endregion
        #region By Hour
        #region check in by hour
        public IActionResult CheckIn()
        {
            #region prebare view and get the lists
            CheckInView view = new();
            var events = _unitOfWork.Repository<Event>().Get(e => e.Status == EventStatus.نشط);
            #endregion
            #region events view
            foreach (var _event in events)
                view.EventCVMs.Add(new EventCVM
                {
                    Id = _event.EventId,
                    Name = _event.EventName,
                    Price = _event.Price
                });
            #endregion

            return View(view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCheckIn(CheckIn checkIn)
        {
          
            List<string> errors = [];
            List<Product> products = [];
            List<SelectedProduct> selected = [];
            int? price = 0;
            int EventPrice = 0;
            int ProductsPrice = 0;

            if(checkIn == null)
            {
                TempData["ErrorMessage"] = "البيانات غير صحيحة.";
                return RedirectToAction("CheckIn");
            }
            #region Childern handeling

            if (string.IsNullOrEmpty(checkIn.ChildIds))
                errors.Add("يرجى تحديد الأطفال.");

            #endregion
            #region Event Handeling
            if (checkIn.EventId != 0)
            {
                var _event = _unitOfWork.Repository<Event>().GetOne(
                    e => e.EventId == checkIn.EventId && e.Status == EventStatus.نشط);

                if (_event != null)
                    EventPrice = _event.Price;
                else
                    errors.Add("الحدث المحدد غير موجود أو غير نشط.");
            }
            #endregion
            #region Products Handeling
            if (checkIn.SelectedProducts != null)
            {
                var listProducts = checkIn.SelectedProducts.Split('|');

                foreach (var p in listProducts) 
                {
                    var productQuantity = p.Split("-");
                    selected.Add(new SelectedProduct
                    {
                        Id = int.Parse(productQuantity[0]),
                        Quantity = int.Parse(productQuantity[1])
                    });
                }

                var Allproducts = _unitOfWork.Repository<Product>().Get(e => !e.IsDeleted).ToList();  

                products = Allproducts.Where(e =>
                    selected.Any(p => p.Id == e.ProductId && p.Quantity <= e.Quantity)
                ).ToList();


                if (products == null || products.Count != selected.Count)
                    errors.Add("بعض المنتجات غير متوفرة بالكميات المطلوبة.");
                else
                {
                    foreach(var product in products)
                    {
                        var q = selected.First(e => e.Id == product.ProductId).Quantity;

                        product.Quantity -= q;

                        ProductsPrice += q * product.Price;
                    }
                }
            }
            #endregion
            #region time / price handeling
            var shift = _unitOfWork.Repository<Shift>().GetOne(e => e.ShiftName == morningName);

            if (shift == null || shift.StartTime == null || shift.EndTime == null)
            {
                TempData["ErrorMessage"] = "يجب اضافة فترة الصباح اولا";
                return RedirectToAction("EditMorningHours", "Shift");
            }

            if (checkIn.CheckInTime < shift.StartTime)
                errors.Add("وقت الدخول لا يمكن أن يكون قبل بداية فترة العمل.");

            if (checkIn.CheckOutTime != null && checkIn.PaymentMethod != null)
            {
                price = _calculatePrice.GetPrice(checkIn.CheckInTime, checkIn.CheckOutTime ?? checkIn.CheckInTime);

                if (price == null || price <= 0)
                    errors.Add("السعر لا يمكن أن يكون صفر أو غير صالح.");
            }
            else if ((checkIn.CheckOutTime == null) != (checkIn.PaymentMethod == null))
                errors.Add("يجب تحديد كلا من طريقة الدفع ووقت الخروج.");
            #endregion
            #region cheack for errors
            if (errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", errors);
                return RedirectToAction("CheckIn");
            }
            #endregion
            #region check in
            var childern = checkIn.ChildIds.Split('-');

            var childernCount = childern.Length;

            CheckInOut checkInOut = new()
            {
                EventId = checkIn.EventId == 0 ? null : checkIn.EventId,
                IsEscort = checkIn.IsEscort,
                Status = CheckStatus.In,
                InPayment = checkIn.PaymentMethod,
                ActualCheckout = null,
                ExpectedCheckout = checkIn.CheckOutTime,
                CheckIn = checkIn.CheckInTime,
                HaveDebt = checkIn.PaymentMethod == PaymentMethod.دین,
                OutPayment = null,
                InTotal = checkIn.CheckOutTime != null ? (price * childernCount ?? 0) + (EventPrice * childernCount) + ProductsPrice : 0,
                OutTotal = 0
            };

            _unitOfWork.Repository<CheckInOut>().Add(checkInOut);

            #region add childern to the check in

            foreach (var ch in childern) 
            {
                checkInOut.CheckChildren.Add(new CheckChild
                {
                    CheckInOutId = checkInOut.CheckInOutId,
                    ChildId = int.Parse(ch)
                });
            }

            _unitOfWork.Repository<CheckChild>().AddRange(checkInOut.CheckChildren);
            #endregion
            #region if thier is products add them to the check in
            if (products != null)
            {
                foreach (var product in selected)
                    checkInOut.CheckProducts.Add(new CheckProduct
                    {
                        CheckInOutId = checkInOut.CheckInOutId,
                        ProductId = product.Id,
                        Quantity = product.Quantity
                    });

                _unitOfWork.Repository<CheckProduct>().AddRange(checkInOut.CheckProducts);
                _unitOfWork.Repository<Product>().UpdateRange(products);
            }
            #endregion
            #endregion

            TempData["CheckInSuccessMessage"] = $"تم حفظ بيانات الحضور بنجاح كود الخروج {checkInOut.CheckInOutId}.";

            #region Create Check In Ticket
            CheckInTicket ticket = new()
            {
                CheckInOutID = checkInOut.CheckInOutId,
                CheckInTime = DateTime.Today.Add(checkInOut.CheckIn).ToString("h:mm tt"),
                ExpectedCheckOutTime = checkInOut.ExpectedCheckout == null ? 
                                                        "لم يتم التحديد" : DateTime.Today.Add((TimeSpan)checkInOut.ExpectedCheckout).ToString("h:mm tt")
            };

            foreach (var child in checkInOut.CheckChildren)
                ticket.ChildrensNames.Add(
                    _unitOfWork.Repository<Child>().GetOne(
                        e => e.ChildId == child.ChildId)?.Name ?? "");

            TempData["Ticket"] = JsonConvert.SerializeObject(ticket);

            #endregion

            #region add new check in to the logged in table
            CheckInTable inTable = new()
            {
                CheckInTime = checkInOut.CheckIn,
                CheckOutTime = checkInOut.ExpectedCheckout,
                Id = checkInOut.CheckInOutId,
                CheckInBy = CheckInBy.ساعة.ToString()
            };

            foreach (var ch in checkInOut.CheckChildren)
            {
                var child = _unitOfWork.Repository<Child>().GetOne(e => e.ChildId == ch.ChildId);
                
                if(child != null)
                    inTable.ChildChecks.Add(new ChildCheckInTable
                    {
                        Id = child.ChildId,
                        ImageName = child.ChildImageName,
                        Name = child.Name
                    });
            }

            await _hubContext.Clients.All.SendAsync("ShowNewCheckIn", inTable);

            await _hubContextlog.Clients.All.SendAsync("LoggedInResult", new List<CheckInLeftBar> { new CheckInLeftBar
            {
                Id = checkInOut.CheckInOutId,
                IsItGroub = checkInOut.Children.Count > 1,
                CheckOutTime = checkInOut.ExpectedCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)checkInOut.ExpectedCheckout)).ToString("h:mm tt") : "غير محدد",
                ImageName = checkInOut.Children.ToList()[0].ChildImageName,
                Name = checkInOut.Children.ToList()[0].Name,
                Status = checkInOut.ExpectedCheckout == null ? 0 : checkInOut.ISAlerted ? 2 : 1
            } });
            #endregion

            return RedirectToAction("CheckIn");
        }
        #endregion
        #region check out by hour
        public IActionResult CheckOut(int id)
        {
            #region check data
            var checkinout = _unitOfWork.Repository<CheckInOut>().GetOne(
                e => 
                    e.CheckInOutId == id && e.Status == CheckStatus.In,
                _event => _event.Event,
                childern => childern.Children,
                products => products.Products,
                checkproducts => checkproducts.CheckProducts
                );

            if ( checkinout == null )
            {
                TempData["ErrorMessage"] = "لم يتم ايجاد البيانات!!";
                return RedirectToAction("GetLoggedIn");
            }

            ChildSubscription? ChildSub = null;

            if (checkinout.ChildSubscriptionId != null )
            {

                ChildSub = _unitOfWork.Repository<ChildSubscription>().GetOne(
                       e => e.ChildSubscriptionId == checkinout.ChildSubscriptionId &&
                       e.Status == SubscriptionStatus.Active,
                       ch => ch.Child, pl => pl.SubscriptionPlan,
                       su => su.SubscriptionPlan.Subscription);

                if(ChildSub == null )
                {
                    TempData["ErrorMessage"] = "لم يتم ايجاد البيانات!!";
                    return RedirectToAction("GetLoggedIn");
                }
            }
            #endregion
            #region prepare view
            CheckOutView view = new()
            {
                Id = id,
                ChildernNumber = checkinout.Children.Count,
                EventName = checkinout.Event?.EventName ?? "",
                EventPrice = checkinout.Event?.Price ?? 0,
                CheckInPayment = checkinout.InPayment ?? PaymentMethod.كاش,
                CheckIn = checkinout.CheckIn,
                ExpectedCheckout = checkinout.ExpectedCheckout,
                ActualCheckout = checkinout.ExpectedCheckout,
                IsDebt = checkinout.InPayment == PaymentMethod.دین,
                ActiveCheckIn = checkinout.InPayment != null,
                IntervalPrice = checkinout.ExpectedCheckout != null ? (_calculatePrice.GetPrice(checkinout.CheckIn, (TimeSpan)checkinout.ExpectedCheckout) ?? 0) : 0,
                CheckInTotal = checkinout.InTotal,
                ProductsPrice = checkinout.CheckProducts.Sum(e => e.Quantity * e.Product.Price),
                PlanId = -1,
                TotalMinutes = checkinout.ExpectedCheckout == null ? 0 : (int)(checkinout.ExpectedCheckout - checkinout.CheckIn).Value.TotalMinutes
            };

            if (checkinout.ChildSubscriptionId != null && ChildSub != null)
            {
                view.IntervalPrice = checkinout.ExpectedCheckout != null ? (_calculatePrice.GetSubscriptionIntervalprice(checkinout.CheckIn,(TimeSpan)checkinout.ExpectedCheckout,ChildSub.SubscriptionPlanId) ?? 0) : 0;
                view.PlanId = ChildSub.SubscriptionPlanId;
                view.Name = ChildSub.Child.Name;
                view.SubName = ChildSub.SubscriptionPlan.Subscription.Name;
                view.RemaningVists = ChildSub.VisitsNumber;
                view.IsDuration = ChildSub.SubscriptionPlan.IsDuration;
                view.Duration = ChildSub.SubscriptionPlan.Duration;
                view.From = ChildSub.SubscriptionPlan.From != null ? (new DateTime(1, 1, 1).Add((TimeSpan)ChildSub.SubscriptionPlan.From)).ToString("h:mm tt") : "";
                view.To = ChildSub.SubscriptionPlan.To != null ? (new DateTime(1, 1, 1).Add((TimeSpan)ChildSub.SubscriptionPlan.To)).ToString("h:mm tt") : "";

                var childNotes = _unitOfWork.Repository<CheckChild>().Get(e => e.ChildId == ChildSub.ChildId).ToList();

                if (childNotes.Count > 0)
                    foreach (var note in childNotes)
                        if (note.Note != null)
                            view.Notes.Add(note.Note);
            }

            foreach (var child in checkinout.Children)
                view.Childern.Add(new ChildCOV
                {
                    ID = child.ChildId,
                    ImageName = child.ChildImageName,
                    Name = child.Name
                });
            #endregion

            return View(view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCheckOut(CheckOut checkOut)
        {
            List<Product> products = [];
            List<SelectedProduct> selected = [];
            int ProductsPrice = 0;
            Dictionary<int,string> childsNotes = [];

            #region cheack intail data
            if (checkOut == null)
            {
                TempData["ErrorMessage"] = "لم يتم ايجاد البيانات!!";
                return RedirectToAction("GetLoggedIn");
            }

            var checkinout = _unitOfWork.Repository<CheckInOut>().GetOne(
                e =>
                    e.CheckInOutId == checkOut.Id && e.Status == CheckStatus.In,
                checkp => checkp.CheckProducts,
                checkc => checkc.CheckChildren,
                pr => pr.Products,
                ev => ev.Event
                );

            if (checkinout == null)
            {
                TempData["ErrorMessage"] = "لم يتم ايجاد البيانات!!";
                return RedirectToAction("GetLoggedIn");
            }
            #endregion
            #region cheack detils of data
            List<string> errors = [];

            if (checkOut.ActualCheckout <= checkinout.CheckIn)
            {
                // Validation message when ActualCheckout is less than or equal to CheckIn
                errors.Add("الوقت الفعلي للخروج لا يمكن أن يكون قبل أو يساوي وقت الدخول.");
            }

            if (checkinout.ExpectedCheckout != null)
            {
                if (checkinout.ExpectedCheckout > checkOut.ActualCheckout)
                {
                    // Validation message when ExpectedCheckout is later than ActualCheckout
                    errors.Add("الوقت المتوقع للخروج لا يمكن أن يكون بعد الوقت الفعلي للخروج.");
                }

                if (checkinout.ExpectedCheckout < checkOut.ActualCheckout && checkOut.CheckOutPayment == null)
                {
                    // Validation message when ExpectedCheckout is earlier than ActualCheckout and no payment exists
                    errors.Add("إذا كان الوقت المتوقع للخروج أقل من الوقت الفعلي للخروج، يجب أن يكون هناك دفع للخروج.");
                }

                // Updated condition for when ExpectedCheckout == ActualCheckout and there should be no payment
                if (checkinout.ExpectedCheckout == checkOut.ActualCheckout && checkOut.CheckOutPayment != null && (!checkOut.AddCheckInPrice || checkinout.InPayment != PaymentMethod.دین))
                {
                    // Validation message when ExpectedCheckout equals ActualCheckout, but there has been a payment
                    errors.Add("إذا كان الوقت المتوقع للخروج هو نفسه الوقت الفعلي للخروج، يجب ألا يكون قد تم الدفع.");
                }
            }
            #region Products Handeling
            if (checkOut.SelectedProducts != null)
            {
                var listProducts = checkOut.SelectedProducts.Split('|');

                foreach (var p in listProducts)
                {
                    var productQuantity = p.Split("-");
                    selected.Add(new SelectedProduct
                    {
                        Id = int.Parse(productQuantity[0]),
                        Quantity = int.Parse(productQuantity[1])
                    });
                }

                var Allproducts = _unitOfWork.Repository<Product>().Get(e => !e.IsDeleted).ToList();

                products = Allproducts.Where(e =>
                    selected.Any(p => p.Id == e.ProductId && p.Quantity <= e.Quantity)
                ).ToList();

                if (products == null || products.Count != selected.Count)
                    errors.Add("بعض المنتجات غير متوفرة بالكميات المطلوبة.");
                else
                {
                    foreach (var product in products)
                    {
                        var q = selected.First(e => e.Id == product.ProductId).Quantity;

                        product.Quantity -= q;

                        ProductsPrice += q * product.Price;
                    }
                }
            }
            #endregion
            #region child notes Handeling
            if (checkOut.ChildsNotes != null && checkOut.ChildsNotes.Count > 0) 
            {
                foreach(var child in checkOut.ChildsNotes)
                {
                    var sections = child.Split("-",2);
                    var id = int.Parse(sections[0]);
                    var note = sections[1];
                    if(checkinout.CheckChildren.Any(e => e.ChildId == id) && !childsNotes.ContainsKey(id))
                        childsNotes[id] = note;
                    else
                    {
                        if (!checkinout.CheckChildren.Any(e => e.ChildId == id))
                        {
                            errors.Add($"لا يوجد طفل بموجب معرف {id} في قائمة الأطفال.");
                        }
                        else if (childsNotes.ContainsKey(id))
                        {
                            errors.Add($"تم إضافة ملاحظة بالفعل للطفل ذو المعرف {id}.");
                        }
                        break;
                    }                   
                }
            }
            #endregion
            #region cheack for errors
            if (errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", errors);
                return RedirectToAction("CheckOut",new {id = checkOut.Id});
            }
            #endregion
            #endregion

            #region childs notes
            if (childsNotes.Count > 0)
            {
                foreach (var note in childsNotes)
                    checkinout.CheckChildren.First(
                        e => e.ChildId == note.Key).Note = note.Value;

                _unitOfWork.Repository<CheckChild>().UpdateRange(checkinout.CheckChildren);
            }
            #endregion
            #region products
            if (products != null && products.Count > 0)
            {
                _unitOfWork.Repository<Product>().UpdateRange(products);

                foreach(var product in selected)
                {
                    var p = checkinout.CheckProducts.FirstOrDefault(e => e.ProductId == product.Id);
                   
                    if (p != null)
                    {
                        p.Quantity += product.Quantity;
                        _unitOfWork.Repository<CheckProduct>().Update(p);
                    }
                    else
                    {
                        var newproduct = new CheckProduct
                        {
                            CheckInOutId = checkinout.CheckInOutId,
                            ProductId = product.Id,
                            Quantity = product.Quantity
                        };
                        _unitOfWork.Repository<CheckProduct>().Add(newproduct);

                        var prod = _unitOfWork.Repository<Product>().GetOne(e => e.ProductId == product.Id);

                        if (prod != null) checkinout.Products.Add(prod);
                    }
                }
            }
            #endregion
            #region Add CheckInPrice or Not
            if (checkOut.CheckOutPayment != null && checkOut.AddCheckInPrice && checkinout.InPayment == PaymentMethod.دین)
                checkinout.InPayment = checkOut.CheckOutPayment;
            #endregion
            #region check in/out
            checkinout.OutPayment = checkOut.CheckOutPayment;
            checkinout.ActualCheckout = checkOut.ActualCheckout;
            checkinout.Status = CheckStatus.Out;
            var childnumber = checkinout.CheckChildren.Count;

            if (checkinout.InTotal == 0)
            {
                int intervalPrice = 0;
                
                if(checkinout.ChildSubscriptionId == null)
                    intervalPrice = _calculatePrice.GetPrice(checkinout.CheckIn, (TimeSpan)checkinout.ActualCheckout) ?? 0;
                else
                {
                    var planId = (_unitOfWork.Repository<ChildSubscription>().GetOne(
                        e => e.ChildSubscriptionId == checkinout.ChildSubscriptionId
                        ) ?? new ChildSubscription()).SubscriptionPlanId;

                    intervalPrice = _calculatePrice.GetSubscriptionIntervalprice(checkinout.CheckIn, (TimeSpan)checkinout.ActualCheckout, planId) ?? 0;
                }


                var allproductsprice = checkinout.CheckProducts.Sum(e => e.Quantity * e.Product.Price);
                var eventprice = checkinout.Event != null ? checkinout.Event.Price : 0;

                checkinout.OutTotal = allproductsprice + (eventprice * childnumber) + (intervalPrice * childnumber);                
            }
            else if(checkinout.ExpectedCheckout != null && checkinout.ActualCheckout != null)
            {
                int total = 0;
                int checkinPrice = 0;

                if (checkinout.ChildSubscriptionId == null)
                {
                    total = _calculatePrice.GetPrice(checkinout.CheckIn, (TimeSpan)checkinout.ActualCheckout) ?? 0;
                    checkinPrice = _calculatePrice.GetPrice(checkinout.CheckIn, (TimeSpan)checkinout.ExpectedCheckout) ?? 0;
                }
                else
                {
                    var planId = (_unitOfWork.Repository<ChildSubscription>().GetOne(
                        e => e.ChildSubscriptionId == checkinout.ChildSubscriptionId
                        ) ?? new ChildSubscription()).SubscriptionPlanId;

                    total = _calculatePrice.GetSubscriptionIntervalprice(checkinout.CheckIn, (TimeSpan)checkinout.ActualCheckout, planId) ?? 0;
                    checkinPrice = _calculatePrice.GetSubscriptionIntervalprice(checkinout.CheckIn, (TimeSpan)checkinout.ExpectedCheckout, planId) ?? 0;
                }

                var intervalPrice = total - checkinPrice;

                checkinout.OutTotal = ProductsPrice + (intervalPrice * childnumber);
            }

            _unitOfWork.Repository<CheckInOut>().Update(checkinout);
            #endregion

            TempData["SuccessMessage"] = "تم تسجيل خروج بنجاح.";

            await _hubContext.Clients.All.SendAsync("RemoveLogged", checkOut.Id);

            await _hubContextlog.Clients.All.SendAsync("RemoveLoggedLeftBar", checkOut.Id);

            return RedirectToAction("GetLoggedIn"); 
        }
        #endregion
        #endregion
        #region GetLoggedIn
        public IActionResult GetLoggedIn()
        {
            List<CheckInTable> view = [];

            #region check in by hour
            var loggedByHour = _unitOfWork.Repository<CheckInOut>().Get(
                e => e.Status == CheckStatus.In,
                childern => childern.Children);

            foreach (var check in loggedByHour)
            {
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
        #region By Subscription
        #region choose which child sub
        public IActionResult ChooseSubscribedChild() => View();
        #endregion
        #region check in by Subscription
        public IActionResult SubscriptionCheckIn(int id)
        {
            #region check data
            var ChildSub = _unitOfWork.Repository<ChildSubscription>().GetOne(
                e => e.Status == SubscriptionStatus.Active && e.ChildSubscriptionId == id,
                ch => ch.Child, pl => pl.SubscriptionPlan, su => su.SubscriptionPlan.Subscription 
                );

            if(ChildSub == null)
            {
                TempData["ErrorMessage"] = "";
                return RedirectToAction("ChooseSubscribedChild");
            }

            var childNotes = _unitOfWork.Repository<CheckChild>().Get(e => e.ChildId == ChildSub.ChildId).ToList();
            #endregion
            #region get active events
            var events = _unitOfWork.Repository<Event>().Get(
                e => e.Status == EventStatus.نشط);
            #endregion
            #region view
            CheckInSubView subView = new()
            {
                Id = ChildSub.SubscriptionPlanId,
                ChildId = ChildSub.ChildId,
                ChildSubId = ChildSub.ChildSubscriptionId,
                Name = ChildSub.Child.Name,
                SubName = ChildSub.SubscriptionPlan.Subscription.Name,
                RemaningVists = ChildSub.VisitsNumber,
                IsDuration = ChildSub.SubscriptionPlan.IsDuration,
                Duration = ChildSub.SubscriptionPlan.Duration,
                TotalTime = _statistics.ChildOverAllTime(ChildSub.ChildId),
                TotalDebt = _statistics.ChildDebtAmount(ChildSub.ChildId),
                From = ChildSub.SubscriptionPlan.From != null ? (new DateTime(1, 1, 1).Add((TimeSpan)ChildSub.SubscriptionPlan.From)).ToString("h:mm tt") : "",
                To = ChildSub.SubscriptionPlan.To != null ? (new DateTime(1, 1, 1).Add((TimeSpan)ChildSub.SubscriptionPlan.To)).ToString("h:mm tt") : ""
            };

            if (childNotes.Count > 0)
                foreach (var note in childNotes)
                    if(note.Note != null)
                        subView.Notes.Add(note.Note);

            foreach (var _event in events)
                subView.Events.Add(new EventCVM
                {
                    Id = _event.EventId,
                    Name = _event.EventName,
                    Price = _event.Price
                });
            #endregion
            return View(subView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubscriptionCheckIn(CheckInSub checkIn)
        {
            List<string> errors = [];
            List<Product> products = [];
            List<SelectedProduct> selected = [];
            int? price = 0;
            int EventPrice = 0;
            int ProductsPrice = 0;

            if (checkIn == null)
            {
                TempData["ErrorMessage"] = "البيانات غير صحيحة.";
                return RedirectToAction("ChooseSubscribedChild");
            }
            #region Childern handeling

            var childSub = _unitOfWork.Repository<ChildSubscription>().GetOne(
                e => e.ChildSubscriptionId == checkIn.ChildSubId && e.Status == SubscriptionStatus.Active,
                ch => ch.Child,plan => plan.SubscriptionPlan
                );

            if (childSub == null)
            {
                TempData["ErrorMessage"] = "البيانات غير صحيحة.";
                return RedirectToAction("ChooseSubscribedChild");
            }

            if (childSub.VisitsNumber <
                (1 + (string.IsNullOrEmpty(checkIn.ChildIds) ? 0 : checkIn.ChildIds.Split("-").Length)))
                errors.Add("عدد الزيارات غير كافٍ بالنسبة لعدد الأطفال.");

            #endregion
            #region Event Handeling
            if (checkIn.EventId != 0)
            {
                var _event = _unitOfWork.Repository<Event>().GetOne(
                    e => e.EventId == checkIn.EventId && e.Status == EventStatus.نشط);

                if (_event != null)
                    EventPrice = _event.Price;
                else
                    errors.Add("الحدث المحدد غير موجود أو غير نشط.");
            }
            #endregion
            #region Products Handeling
            if (checkIn.SelectedProducts != null)
            {
                var listProducts = checkIn.SelectedProducts.Split('|');

                foreach (var p in listProducts)
                {
                    var productQuantity = p.Split("-");
                    selected.Add(new SelectedProduct
                    {
                        Id = int.Parse(productQuantity[0]),
                        Quantity = int.Parse(productQuantity[1])
                    });
                }

                var Allproducts = _unitOfWork.Repository<Product>().Get(e => !e.IsDeleted).ToList();

                products = Allproducts.Where(e =>
                    selected.Any(p => p.Id == e.ProductId && p.Quantity <= e.Quantity)
                ).ToList();


                if (products == null || products.Count != selected.Count)
                    errors.Add("بعض المنتجات غير متوفرة بالكميات المطلوبة.");
                else
                {
                    foreach (var product in products)
                    {
                        var q = selected.First(e => e.Id == product.ProductId).Quantity;

                        product.Quantity -= q;

                        ProductsPrice += q * product.Price;
                    }
                }
            }
            #endregion
            #region time / price handeling
            var shift = _unitOfWork.Repository<Shift>().GetOne(e => e.ShiftName == morningName);

            if (shift == null || shift.StartTime == null || shift.EndTime == null)
            {
                TempData["ErrorMessage"] = "يجب اضافة فترة الصباح اولا";
                return RedirectToAction("EditMorningHours", "Shift");
            }

            if (checkIn.CheckInTime < shift.StartTime)
                errors.Add("وقت الدخول لا يمكن أن يكون قبل بداية فترة العمل.");

            if (checkIn.CheckOutTime != null && checkIn.PaymentMethod != null)
            {
                price = _calculatePrice.GetSubscriptionIntervalprice(checkIn.CheckInTime, (TimeSpan)checkIn.CheckOutTime,childSub.SubscriptionPlanId);

                if (price == null || price < 0)
                    errors.Add("السعر لا يمكن أن يكون اقل من صفر أو غير صالح.");
            }
            else if ((checkIn.CheckOutTime == null) != (checkIn.PaymentMethod == null))
                errors.Add("يجب تحديد كلا من طريقة الدفع ووقت الخروج.");
            #endregion
            #region cheack for errors
            if (errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", errors);
                return RedirectToAction("SubscriptionCheckIn", new {id = checkIn.ChildSubId});
            }
            #endregion
            #region check in
            var childern = string.IsNullOrEmpty(checkIn.ChildIds) ? null : checkIn.ChildIds.Split('-');

            var childernCount = 1 + (childern == null ? 0 : childern.Length);

            childSub.VisitsNumber -= childernCount;

            if (childSub.VisitsNumber <= 0)
                childSub.Status = SubscriptionStatus.Expired;

            CheckInOut checkInOut = new()
            {
                EventId = checkIn.EventId == 0 ? null : checkIn.EventId,
                IsEscort = checkIn.IsEscort,
                Status = CheckStatus.In,
                InPayment = checkIn.PaymentMethod,
                ActualCheckout = null,
                ExpectedCheckout = checkIn.CheckOutTime,
                CheckInBy = CheckInBy.أشتراك,
                ChildSubscriptionId = checkIn.ChildSubId,
                CheckIn = checkIn.CheckInTime,
                HaveDebt = checkIn.PaymentMethod == PaymentMethod.دین,
                OutPayment = null,
                InTotal = checkIn.CheckOutTime != null ? (price * childernCount ?? 0) + (EventPrice * childernCount) + ProductsPrice : 0,
                OutTotal = 0
            };

            _unitOfWork.Repository<CheckInOut>().Add(checkInOut);

            #region add childern to the check in
            //the owner of the Sub
            checkInOut.CheckChildren.Add(new CheckChild
            {
                CheckInOutId = checkInOut.CheckInOutId,
                ChildId = childSub.ChildId,
            });

            if(childern != null)
                foreach (var ch in childern)
                {
                    checkInOut.CheckChildren.Add(new CheckChild
                    {
                        CheckInOutId = checkInOut.CheckInOutId,
                        ChildId = int.Parse(ch)
                    });
                }

            _unitOfWork.Repository<CheckChild>().AddRange(checkInOut.CheckChildren);
            #endregion
            #region if thier is products add them to the check in
            if (products != null)
            {
                foreach (var product in selected)
                    checkInOut.CheckProducts.Add(new CheckProduct
                    {
                        CheckInOutId = checkInOut.CheckInOutId,
                        ProductId = product.Id,
                        Quantity = product.Quantity
                    });

                _unitOfWork.Repository<CheckProduct>().AddRange(checkInOut.CheckProducts);
                _unitOfWork.Repository<Product>().UpdateRange(products);
            }
            #endregion
            #endregion

            _unitOfWork.Repository<ChildSubscription>().Update(childSub);

            TempData["CheckInSuccessMessage"] = $"تم حفظ بيانات الحضور بنجاح كود الخروج {checkInOut.CheckInOutId}.";

            #region Create Check In Ticket
            CheckInTicket ticket = new()
            {
                CheckInOutID = checkInOut.CheckInOutId,
                CheckInTime = DateTime.Today.Add(checkInOut.CheckIn).ToString("h:mm tt"),
                ExpectedCheckOutTime = checkInOut.ExpectedCheckout == null ?
                                                        "لم يتم التحديد" : DateTime.Today.Add((TimeSpan)checkInOut.ExpectedCheckout).ToString("h:mm tt")
            };

            foreach (var child in checkInOut.CheckChildren)
                ticket.ChildrensNames.Add(
                    _unitOfWork.Repository<Child>().GetOne(
                        e => e.ChildId == child.ChildId)?.Name ?? "");

            TempData["Ticket"] = JsonConvert.SerializeObject(ticket);

            #endregion

            #region add new check in to the logged in table
            CheckInTable inTable = new()
            {
                CheckInTime = checkInOut.CheckIn,
                CheckOutTime = checkInOut.ExpectedCheckout,
                Id = checkInOut.CheckInOutId,
                CheckInBy = CheckInBy.أشتراك.ToString()
            };

            foreach (var ch in checkInOut.CheckChildren)
            {
                var child = _unitOfWork.Repository<Child>().GetOne(e => e.ChildId == ch.ChildId);

                if (child != null)
                    inTable.ChildChecks.Add(new ChildCheckInTable
                    {
                        Id = child.ChildId,
                        ImageName = child.ChildImageName,
                        Name = child.Name
                    });
            }

            await _hubContext.Clients.All.SendAsync("ShowNewCheckIn", inTable);

            await _hubContextlog.Clients.All.SendAsync("LoggedInResult", new List<CheckInLeftBar> { new CheckInLeftBar
            {
                Id = checkInOut.CheckInOutId,
                IsItGroub = checkInOut.Children.Count > 1,
                CheckOutTime = checkInOut.ExpectedCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)checkInOut.ExpectedCheckout)).ToString("h:mm tt") : "غير محدد",
                ImageName = checkInOut.Children.ToList()[0].ChildImageName,
                Name = checkInOut.Children.ToList()[0].Name,
                Status = checkInOut.ExpectedCheckout == null ? 0 : checkInOut.ISAlerted ? 2 : 1
            } });
            #endregion

            return RedirectToAction("ChooseSubscribedChild");
        }
        #endregion
        #endregion
    }
}
