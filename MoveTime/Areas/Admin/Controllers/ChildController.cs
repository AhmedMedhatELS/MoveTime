using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models;
using Models.ViewModels;
using Models.ViewModels.ChildFile;
using Shared;
using System.Configuration;
using System.Text.RegularExpressions;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class ChildController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        Statistics statistics) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly Statistics _statistics = statistics;

        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region Add new Child
        public IActionResult NewChild() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewChild(AddChild addChild)
        {
            #region check 
            if (addChild == null) return BadRequest();

            List<string> ErrorMessages = [];
            string? ChildImageName = "";
            string? DisclaimerImageName = "";
            string? parentId = null;

            #region Child data
            if (string.IsNullOrEmpty(addChild.Name))
                ErrorMessages.Add("الاسم مطلوب");

            #region child image
            if (addChild.ChildImageFile == null)
                ErrorMessages.Add("صورة الطفل مطلوبة");
            else
                ChildImageName = ImageManger.SaveImage(addChild.ChildImageFile, ImageLocation.Childrens);

            if (string.IsNullOrEmpty(ChildImageName))
                ErrorMessages.Add("حدث خطأ في حفظ صورة الطفل");
            #endregion

            if (string.IsNullOrEmpty(addChild.NationalId))
                ErrorMessages.Add("رقم الهوية الوطنية مطلوب");

            if (string.IsNullOrEmpty(addChild.WhatsappNumber))
                ErrorMessages.Add("رقم الواتساب مطلوب");

            #region Disclaimer image
            if (addChild.DisclaimerImage == null)
                ErrorMessages.Add("صورة الإقرار مطلوبة");
            else
                DisclaimerImageName = ImageManger.SaveImage(addChild.DisclaimerImage, ImageLocation.Disclaimers);

            if (string.IsNullOrEmpty(DisclaimerImageName))
                ErrorMessages.Add("حدث خطأ في حفظ صورة الإقرار");
            #endregion

            if (addChild.HaveHealthCondition && string.IsNullOrEmpty(addChild.HealthCondition))
                ErrorMessages.Add("يرجى توضيح الحالة الصحية");

            if (addChild.IsDisabled && string.IsNullOrEmpty(addChild.DisableDescription))
                ErrorMessages.Add("يرجى توضيح الإعاقة");
            #endregion
            #region Employee

            if (addChild.IsEscort && addChild.EscortReason == EscortReason.None)
                ErrorMessages.Add("يرجى توضيح سبب المرافقة");

            #endregion
            #region parent

            if (!string.IsNullOrEmpty(addChild.Email))
            {
                string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                if (!Regex.IsMatch(addChild.Email, EmailPattern))
                    ErrorMessages.Add("البريد الإلكتروني غير صحيح");
                else
                {
                    if (!string.IsNullOrEmpty(addChild.Password) || !string.IsNullOrEmpty(addChild.ConfirmPassword))
                    {
                        if (addChild.Password == addChild.ConfirmPassword)
                        {
                            ApplicationUser user = new()
                            {
                                UserName = addChild.Email,
                                Email = addChild.Email,
                            };

                            var emailexists = await _userManager.FindByEmailAsync(addChild.Email);
                            var result = await userManager.CreateAsync(user, addChild.Password ?? "");

                            if (result.Succeeded)
                                parentId = user.Id;
                            else if(emailexists != null)
                                ErrorMessages.Add("البريد الاليكترونى موجود بالفعل.");
                            else 
                                ErrorMessages.Add("كلمة المرور لا تفي بالمتطلبات الأمنية");
                        }
                        else
                            ErrorMessages.Add("كلمة المرور وتأكيد كلمة المرور غير متطابقين");
                    }
                    else
                    {
                        var user = await _userManager.FindByEmailAsync(addChild.Email);

                        if (user != null)
                            parentId = user.Id;
                        else
                            ErrorMessages.Add("لم يتم العثور على حساب بالبريد الإلكتروني المدخل");
                    }
                }
            }

            #endregion

            if (ErrorMessages.Count > 0) 
            {
                TempData["ErrorMessage"] = string.Join("-", ErrorMessages);

                if(ChildImageName != null)
                    ImageManger.DeleteImage(ChildImageName, ImageLocation.Childrens);
                if(DisclaimerImageName != null)
                    ImageManger.DeleteImage(DisclaimerImageName, ImageLocation.Disclaimers);

                return View(addChild);
            }

            #endregion

            #region Add New Child
            Child child = new()
            {
                BirthDay = addChild.BirthDay,
                ChildImageName = ChildImageName ?? "",
                Gender = addChild.Gender,
                Name = addChild.Name,
                NationalId = addChild.NationalId,
                WhatsappNumber = addChild.WhatsappNumber,
                Phone = addChild.Phone,
                ParentsNote = addChild.ParentsNote,
                SupervisorNote = addChild.SupervisorNote,
                ParentId = parentId,
                BlackList = false,
                DisclaimerImage = DisclaimerImageName,
                IsApproved = addChild.IsApproved,
                IsAllowedToSubscribe = addChild.IsAllowedToSubscribe,
                HaveHealthCondition = addChild.HaveHealthCondition,
                HealthCondition = addChild.HaveHealthCondition ? addChild.HealthCondition : null,
                IsDisabled = addChild.IsDisabled,
                DisableDescription = addChild.IsDisabled ? addChild.DisableDescription : null,
                IsEscort = addChild.IsEscort,
                EscortReason = addChild.IsEscort ? addChild.EscortReason : EscortReason.None
            };

            _unitOfWork.Repository<Child>().Add(child);
            #endregion

            TempData["SuccessMessage"] = $"تم اضافة طفل {addChild.Name} بنجاح كود الخاص به {child.ChildId}.";
            
            return View();
        }
        #endregion
        #region Child Debts
        public IActionResult ChildDebt(int id,string Where)
        {
            #region get subscriptions debts 
            var subscriptionsDebts = _unitOfWork.Repository<ChildSubscription>().Get(
                e => e.HaveDebt &&
                    e.ChildId == id,
                plan => plan.SubscriptionPlan,
                subscription => subscription.SubscriptionPlan.Subscription
                ).ToList();
            #endregion
            #region get check in/out depts
            var InOutDepts = _unitOfWork.Repository<CheckInOut>().Get(
                    e => e.CheckChildren.Any(c => c.ChildId == id) && e.Status == CheckStatus.Out
                    && (e.InPayment == PaymentMethod.دین || e.OutPayment == PaymentMethod.دین)
                ).ToList();
            #endregion
            #region add depts to the view

            List<ChildDebt> childDebts = [];

            if ( subscriptionsDebts.Count > 0 || InOutDepts.Count > 0)
            {
                if (subscriptionsDebts.Count > 0)
                    foreach (var debt in subscriptionsDebts)
                        childDebts.Add(new ChildDebt
                        {
                            Id = debt.ChildSubscriptionId,
                            Amount = debt.Remaining,
                            DebtDate = debt.SubscriptionBegin.ToString("yyyy/MM/dd"),
                            DebtName = debt.SubscriptionPlan.Subscription.Name
                        });

                if (InOutDepts.Count > 0)
                    foreach (var debt in InOutDepts)
                        childDebts.Add(new ChildDebt
                        {
                            Id = debt.CheckInOutId,
                            Amount = (debt.InTotal * (debt.InPayment == PaymentMethod.دین ? 1 : 0)) + (debt.OutTotal * (debt.OutPayment == PaymentMethod.دین ? 1 : 0)),
                            DebtDate = debt.DateLog.ToString("yyyy/MM/dd"),
                            DebtName = debt.CheckInBy.ToString()
                        });

                ViewData["ChildId"] = id;
                ViewData["where"] = Where;

                return View(childDebts);
            }
            #endregion
            if(Where == "sub")
                return RedirectToAction("ChoosePlan",new {id});
            else if(Where == "file")
            {
                TempData["SuccessMessage"] = "ليس عليه ديون";
                return RedirectToAction("ChildFile", new { id });
            }
            else
                return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayDebt(PayAllDebt pay)
        {
            #region check data

            List<string> Errors = [];

            if (pay == null)
            {
                // This is where you handle a null `pay` object
                TempData["ErrorMessage"] = "لم يتم العثور على بيانات الدفع.";
                return RedirectToAction("WhichChild");
            }

            if (pay.PaymentMethod == PaymentMethod.دین)
            {
                // Add an appropriate error message for the "PaymentMethod" check
                Errors.Add("طريقة الدفع المحددة غير صالحة.");
            }

            var childDebts = _unitOfWork.Repository<ChildSubscription>().Get(
                e => e.ChildId == pay.ChildId && e.HaveDebt).ToList();
            #region get check in/out depts
            var InOutDepts = _unitOfWork.Repository<CheckInOut>().Get(
                    e => e.CheckChildren.Any(c => c.ChildId == pay.ChildId) && e.Status == CheckStatus.Out
                    && (e.InPayment == PaymentMethod.دین || e.OutPayment == PaymentMethod.دین)
                ).ToList();
            #endregion

            if ((childDebts == null || !childDebts.Any()) && (InOutDepts == null || !InOutDepts.Any()))  // Check if no child debts exist
            {
                // This adds an error if no debts are found for the child
                Errors.Add("لا توجد ديون مستحقة لهذا الطفل.");
            }

            if (Errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join(" - ", Errors);

                if(pay.Redirect == "file")
                    return RedirectToAction("ChildFile",new {id = pay.ChildId});
                else
                    return RedirectToAction("WhichChild");
            }

            #endregion

            #region pay debts
            if (childDebts != null && childDebts.Count > 0)  // Check if no child debts exist
            {
                foreach (var childDebt in childDebts ?? [])
                {
                    childDebt.HaveDebt = false;
                    childDebt.DebtPaymentMethod = pay.PaymentMethod;
                }

                _unitOfWork.Repository<ChildSubscription>().UpdateRange(childDebts ?? []);
            }
            if (InOutDepts != null && InOutDepts.Count > 0)  // Check if no child debts exist
            {
                foreach (var childDebt in InOutDepts ?? [])
                {
                    childDebt.HaveDebt = false;

                    if(childDebt.InPayment == PaymentMethod.دین)
                        childDebt.InPayment = pay.PaymentMethod;

                    if (childDebt.OutPayment == PaymentMethod.دین)
                        childDebt.OutPayment = pay.PaymentMethod;
                }

                _unitOfWork.Repository<CheckInOut>().UpdateRange(InOutDepts ?? []);
            }
                #endregion

                TempData["SuccessMessage"] = "تم دفع جميع الديون بنجاح.";
            
            if(pay.Redirect == "file")
                return RedirectToAction("ChildFile", new { id = pay.ChildId });
            else
                return RedirectToAction("ChoosePlan", new { id = pay.ChildId });
        }
        #endregion
        #region add child to specific subscription
        public IActionResult WhichChild() => View();
        
        public IActionResult ChoosePlan(int id)
        {
            #region cheak for the id and the subscriptions
            //cheack if this id is right
            if (_unitOfWork.Repository<Child>().GetOne(e => e.ChildId == id && !e.IsDeleted) == null)
                return RedirectToAction("WhichChild");

            //get all the subscriptions
            var subscriptions = _unitOfWork.Repository<Subscription>().Get(
                null,plans => plans.Plans);

            //if no subscriptions exists must make one
            if (subscriptions == null)
            {
                TempData["ErrorMessage"] = "يجب اضافة اشتراك اولا.";
                return RedirectToAction("ManageSubscription");
            }
            #endregion
            #region prebare the subscriptions for the view 
            ChooseView chooseView = new() { ChildId = id};

            foreach (var subscription in subscriptions)
            {
                chooseView.ChildSubscriptions.Add(new ChooseSubscriptionView
                {
                    Name = subscription.Name
                });

                foreach (var plan in subscription.Plans)
                    chooseView.ChildSubscriptions.Last().ChoosePlaniews.Add(new ChoosePlaniew
                    {
                        Id = plan.SubscriptionPlanId,
                        ActiveDays = plan.ActiveDays,
                        Price = plan.Price,
                        VisitsNumber = plan.VisitsNumber,
                        IsDuration = plan.IsDuration,
                        Duration = plan.Duration,
                        From = DateTime.Today.Add(plan.From.GetValueOrDefault()).ToString("hh:mm tt"),
                        To = DateTime.Today.Add(plan.To.GetValueOrDefault()).ToString("hh:mm tt"),
                        DaysOfWeek = string.Join(",",plan.DaysOfWeek.Select(e => DaysOfWeekTranslations.Translations[e]))
                    });
            }
            #endregion

            return View(chooseView);
        }
        public IActionResult SubscriptionBill(WhichSubscription whichSubscription)
        {
            #region cheack data
            if (_unitOfWork.Repository<ChildSubscription>().Get(
                e =>
                    e.ChildId == whichSubscription.ChildId && e.HaveDebt).Any())
                return RedirectToAction("ChildDebt",new {id = whichSubscription.ChildId});

            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == whichSubscription.ChildId && !e.IsDeleted);

            var plan = _unitOfWork.Repository<SubscriptionPlan>().GetOne(
                e => e.SubscriptionPlanId == whichSubscription.PlanId,
                subscription => subscription.Subscription,
                note => note.Subscription.Notes);

            if (child == null || plan == null)
                return RedirectToAction("WhichChild");
            #endregion
            #region prepare view data
            SubscriptionBillView billView = new()
            {
                ChildId = child.ChildId,
                PlanId = plan.SubscriptionPlanId,
                ChildName = child.Name,
                BillImageFile = null,
                IsDuration = plan.IsDuration,
                Duration = plan.Duration,
                From = DateTime.Today.Add(plan.From.GetValueOrDefault()).ToString("hh:mm tt"),
                To = DateTime.Today.Add(plan.To.GetValueOrDefault()).ToString("hh:mm tt"),
                DaysOfWeek = string.Join(",", plan.DaysOfWeek.Select(e => DaysOfWeekTranslations.Translations[e])),
                Total = plan.Price,
                Payed = plan.Price,
                Remaining = 0,
                VisitsNumber = plan.VisitsNumber,
                ContactNumber = child.WhatsappNumber,
                SubscriptionDescription = plan.Subscription.Description,
                SubscriptionBegin = DateTime.Now,
                SubscriptionEnd = DateTime.Now.AddDays(plan.ActiveDays),
                BillHeader = plan.Subscription.Name + " : " + plan.ActiveDays + " زيارة / " + plan.Price + " ريال",
                Notes = plan.Subscription.Notes?.Select(e => e.Note).ToList() ?? []
            }; 
            #endregion

            return View(billView);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmSubscriptionBill(SubscriptionBillView Bill)
        {
            #region cheack data
            List<string> Errors = [];
            string? BillImage = null;

            // Check if the child exists
            if (_unitOfWork.Repository<Child>().GetOne(e => e.ChildId == Bill.ChildId && !e.IsDeleted) == null)
                Errors.Add("الطفل غير موجود");

            // Check if the subscription plan exists
            if (_unitOfWork.Repository<SubscriptionPlan>().GetOne(e => e.SubscriptionPlanId == Bill.PlanId) == null)
                Errors.Add("خطة الاشتراك غير موجودة");

            // Check if the total is greater than 0
            if (Bill.Total == 0)
                Errors.Add("المجموع يجب أن يكون أكبر من 0");

            // Check if the total matches the sum of paid and remaining
            if (Bill.Total != Bill.Payed + Bill.Remaining)
                Errors.Add("المجموع لا يتطابق مع المبلغ المدفوع والمبلغ المتبقي");

            // Check if the number of visits is greater than 0
            if (Bill.VisitsNumber <= 0)
                Errors.Add("عدد الزيارات يجب أن يكون أكبر من 0");

            // Check if the payment method is debt (دین)
            if (Bill.PaymentMethod == PaymentMethod.دین)
                Errors.Add("طريقة الدفع غير صالحة");

            // Check if the subscription start date is before the end date
            if (Bill.SubscriptionBegin >= Bill.SubscriptionEnd)
                Errors.Add("تاريخ بداية الاشتراك لا يمكن أن يكون بعد تاريخ الانتهاء");

            // Check if the bill image file is provided and valid
            if (Bill.BillImageFile != null && Errors.Count == 0)
            {
                BillImage = ImageManger.SaveImage(Bill.BillImageFile, ImageLocation.Bills);

                if (BillImage == null) // Check if the file extension is valid (.jpg or .png)
                    Errors.Add("يجب أن تكون صورة الفاتورة بصيغة .jpg أو .png");
            }
            if (Errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", Errors);
                return RedirectToAction("WhichChild");
            }
            #endregion
            #region add ChildSubscribtion to DB
            var ChildSubscribtion = new ChildSubscription
            {
                ChildId = Bill.ChildId,
                SubscriptionPlanId = Bill.PlanId,
                Payed = Bill.Payed,
                Remaining = Bill.Remaining,
                Total = Bill.Total,
                HaveDebt = Bill.Remaining > 0,
                BillImageName = BillImage,
                PaymentMethod = Bill.PaymentMethod,
                SubscriptionBegin = Bill.SubscriptionBegin,
                SubscriptionEnd = Bill.SubscriptionEnd,
                VisitsNumber = Bill.VisitsNumber,
                Status = SubscriptionStatus.Active
            };

            _unitOfWork.Repository<ChildSubscription>().Add(ChildSubscribtion);

            TempData["SuccessMessage"] = "لقد تم الاشتراك بنجاح.";
            #endregion

            return RedirectToAction("WhichChild");
        }
        #endregion
        #region child file 
        public IActionResult WhichChildFile() => View();
        public IActionResult ChildFile(int id) 
        {
            #region check data
            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == id && !e.IsDeleted,
                parent => parent.Parent);

            if (child == null)
            {
                TempData["ErrorMessage"] = "خطأ فى معلومات.";
                return RedirectToAction("WhichChildFile");
            }
            #endregion
            #region child view
            ChildFileView view = new()
            {
                ChildId = id,
                NationalId = child.NationalId,
                Gender = GenderTranslations.Translations[child.Gender],
                BirthDay = child.BirthDay.ToString(),
                Name = child.Name,
                ChildImageName = child.ChildImageName,
                DisclaimerImage = child.DisclaimerImage ?? "",
                IsAllowedToSubscribe = child.IsAllowedToSubscribe,
                IsApproved = child.IsApproved,
                Oldyears = child.Oldyears,
                BlackListReason = child.BlackListReason ?? "ليس على قائمة السوداء",
                WhatsappNumber = child.WhatsappNumber,
                Phone = child.Phone ?? "لا يوجد رقم اخر",
                DisableDescription = child.DisableDescription ?? "لا يوجد اعاقة",
                HealthCondition = child.HealthCondition ?? "لا يوجد حاله صحية",
                ParentsNote = child.ParentsNote ?? "لا يوجد ملاحظة من الوالدين",
                SupervisorNote = child.SupervisorNote ?? "لا يوجد ملاحظة من المشرف",
                ParentEmail = child.Parent?.Email ?? "الطفل غير مرتبط بحساب",
                ChildEscortReason = EscortReasonTranslations.Translations[child.EscortReason],
                TotalTime = _statistics.ChildOverAllTime(id)
            };
            #endregion
            #region child subscriptions
            var childSub = _unitOfWork.Repository<ChildSubscription>().Get(
                e => e.ChildId == id,pl => pl.SubscriptionPlan,su => su.SubscriptionPlan.Subscription
                ).ToList();

            if (childSub.Count > 0)
            {
                bool UpdateOrNot = false;

                foreach (var sub in childSub)
                {
                    if (sub.Status == SubscriptionStatus.Active && (sub.VisitsNumber <= 0 || sub.SubscriptionEnd < DateTime.Now))
                    {
                        UpdateOrNot = true;
                        sub.Status = SubscriptionStatus.Expired;
                    }

                    view.ChildSubscriptions.Add(new ChildSubscriptionsFileView
                    {
                        ID = sub.ChildSubscriptionId,
                        Status = SubscriptionStatusTranslations.Translations[sub.Status],
                        Total = sub.Total,
                        Remaining = sub.Remaining,
                        Payed = sub.Payed,
                        SubName = sub.SubscriptionPlan.Subscription.Name,
                        SubscriptionBegin = sub.SubscriptionBegin.ToString("yyyy-MM-dd"),
                        SubscriptionEnd = sub.SubscriptionEnd.ToString("yyyy-MM-dd"),
                        VisitsNumber = sub.VisitsNumber,
                        IsDuration = sub.SubscriptionPlan.IsDuration,
                        Duration = sub.SubscriptionPlan.Duration,
                        From = sub.SubscriptionPlan.From != null ? (new DateTime(1, 1, 1).Add((TimeSpan)sub.SubscriptionPlan.From)).ToString("h:mm tt") : "",
                        To = sub.SubscriptionPlan.To != null ? (new DateTime(1, 1, 1).Add((TimeSpan)sub.SubscriptionPlan.To)).ToString("h:mm tt") : "",
                        PaymentMethod = sub.PaymentMethod.ToString(),
                        DebtPaymentMethod = sub.DebtPaymentMethod.ToString(),
                        DebtPayed = sub.DebtPaymentMethod != null
                    });
                }

                if (UpdateOrNot)
                    _unitOfWork.Repository<ChildSubscription>().UpdateRange(childSub);
            }
            #endregion
            #region child CheckInOuts
            var childchecks = _unitOfWork.Repository<CheckInOut>().Get(
                    e => e.CheckChildren.Any(c => c.ChildId == id) && e.Status == CheckStatus.Out,
                    ev => ev.Event,ch => ch.CheckChildren
                ).ToList();

            if (childchecks.Count > 0)
            {
               foreach(var inout  in childchecks)
                {
                    view.CheckInOuts.Add(new CheckInOutFileView
                    {
                        ChildrenNumber = inout.CheckChildren.Count,
                        CheckInBy = inout.CheckInBy.ToString(),
                        IsEscort = inout.IsEscort,
                        InTotal = inout.InTotal,
                        OutTotal = inout.OutTotal,
                        EventName = inout.EventId != null ? inout.Event.EventName : "لم يتم اختيار فعالية",
                        DateLog = inout.DateLog.ToString("yyyy-MM-dd"),
                        InPayment = inout.InPayment.ToString() ?? "لم يتم دفع عند الدخول",
                        OutPayment = inout.OutPayment.ToString() ?? "لم يتم الدفع عند الخروج",
                        CheckIn = (new DateTime(1, 1, 1).Add(inout.CheckIn)).ToString("h:mm tt"),
                        ExpectedCheckout = inout.ExpectedCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)inout.ExpectedCheckout)).ToString("h:mm tt") : "لم يحدد",
                        ActualCheckout = inout.ActualCheckout != null ? (new DateTime(1, 1, 1).Add((TimeSpan)inout.ActualCheckout)).ToString("h:mm tt") : "لم يحدد"
                    });
                }
            }
            #endregion

            return View(view);
        }
        #endregion
        #region Edit Child Subscription
        [HttpPost]
        public IActionResult EditChildSubscription(SubChildEdit sub)
        {
            #region check data
            if (sub == null)
            {
                TempData["ErrorMessage"] = "خطأ فى المعلومات";
                return RedirectToAction("WhichChildFile");
            }

            var subChild = _unitOfWork.Repository<ChildSubscription>().GetOne(
                e => e.ChildSubscriptionId == sub.SubChildId, ch => ch.Child
                );

            if (subChild == null || subChild.Child == null)
            {
                TempData["ErrorMessage"] = "خطأ فى المعلومات";
                return RedirectToAction("WhichChildFile");
            }

            List<string> errors = [];

            if (sub.VisitsNumber < 0)
                errors.Add("عدد الزيارات يجب أن يكون أكبر من أو يساوي 0.");

            if (sub.SubscriptionEnd <= sub.SubscriptionBegin)
                errors.Add("تاريخ انتهاء الاشتراك يجب أن يكون بعد تاريخ بدء الاشتراك.");

            if (errors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", errors);

                return RedirectToAction("ChildFile", new { id = subChild.ChildId });
            }
            #endregion
            #region Edit 
            if (subChild.VisitsNumber != sub.VisitsNumber ||
                subChild.Status != sub.Status ||
                subChild.SubscriptionBegin != sub.SubscriptionBegin ||
                subChild.SubscriptionEnd != sub.SubscriptionEnd
                )
            {
                subChild.VisitsNumber = sub.VisitsNumber;
                subChild.Status = sub.Status;
                subChild.SubscriptionBegin = sub.SubscriptionBegin;
                subChild.SubscriptionEnd = sub.SubscriptionEnd;

                _unitOfWork.Repository<ChildSubscription>().Update(subChild);
                TempData["SuccessMessage"] = "تم تعديل البينات بنجاح";
            }
            else
                TempData["SuccessMessage"] = "لا يوجد بينات جديده للتعديل";
            #endregion

            return RedirectToAction("ChildFile", new { id = subChild.ChildId });
        }
        #endregion
        #region Edit Child
        public IActionResult EditChild(int id)
        {
            var child = _unitOfWork.Repository<Child>().GetOne(e => e.ChildId == id,
                pa => pa.Parent);

            if (child == null)
            {
                TempData["ErrorMessage"] = "خطأ فى المعلومات";
                return RedirectToAction("WhichChildFile");
            }

            var view = new EditChild
            {
                Id = child.ChildId,
                Name = child.Name,
                ChildImageName = child.ChildImageName,
                BirthDay = child.BirthDay,
                BlackList = child.BlackList,
                BlackListReason = child.BlackListReason,
                DisableDescription = child.DisableDescription,
                Email = child.Parent?.Email ?? null,
                Gender = child.Gender,
                EscortReason = child.EscortReason,
                HealthCondition = child.HealthCondition,
                HaveHealthCondition = child.HaveHealthCondition,
                IsAllowedToSubscribe = child.IsAllowedToSubscribe,
                IsApproved = child.IsApproved,
                IsEscort = child.IsEscort,
                Oldyears = child.Oldyears,
                NationalId = child.NationalId,
                IsDisabled = child.IsDisabled,
                Phone = child.Phone,
                ParentsNote = child.ParentsNote,
                SupervisorNote = child.SupervisorNote,
                WhatsappNumber = child.WhatsappNumber
            };

            return View(view);
        }

        [HttpPost]
        public async Task<IActionResult> EditChild(EditChild addChild)
        {
            #region check 
            if (addChild == null) return RedirectToAction("WhichChildFile");

            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == addChild.Id && !e.IsDeleted,
                pa => pa.Parent
                );

            if (child == null) return RedirectToAction("WhichChildFile");

            List<string> ErrorMessages = [];
            string? ChildImageName = null;
            string? DisclaimerImageName = null;
            string? parentId = null;

            #region Child data
            if (string.IsNullOrEmpty(addChild.Name))
                ErrorMessages.Add("الاسم مطلوب");

            #region child image
            if (addChild.ChildImageFile != null)
            {
                ChildImageName = ImageManger.SaveImage(addChild.ChildImageFile, ImageLocation.Childrens);

                if (string.IsNullOrEmpty(ChildImageName))
                    ErrorMessages.Add("حدث خطأ في حفظ صورة الطفل");
            }
            #endregion

            if (string.IsNullOrEmpty(addChild.NationalId))
                ErrorMessages.Add("رقم الهوية الوطنية مطلوب");

            if (string.IsNullOrEmpty(addChild.WhatsappNumber))
                ErrorMessages.Add("رقم الواتساب مطلوب");

            #region Disclaimer image
            if (addChild.DisclaimerImage != null)
            {
                DisclaimerImageName = ImageManger.SaveImage(addChild.DisclaimerImage, ImageLocation.Disclaimers);

                if (string.IsNullOrEmpty(DisclaimerImageName))
                    ErrorMessages.Add("حدث خطأ في حفظ صورة الإقرار");
            }
            #endregion

            if (addChild.HaveHealthCondition && string.IsNullOrEmpty(addChild.HealthCondition))
                ErrorMessages.Add("يرجى توضيح الحالة الصحية");

            if (addChild.IsDisabled && string.IsNullOrEmpty(addChild.DisableDescription))
                ErrorMessages.Add("يرجى توضيح الإعاقة");
            #endregion
            #region Employee
            if (addChild.IsEscort && addChild.EscortReason == EscortReason.None)
                ErrorMessages.Add("يرجى توضيح سبب المرافقة");

            if (addChild.BlackList && string.IsNullOrEmpty(addChild.BlackListReason))
                ErrorMessages.Add("يرجى توضيح سبب الاضافة للقأمة السوداء");
            #endregion
            #region parent

            if (!string.IsNullOrEmpty(addChild.Email))
            {
                string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                if (!Regex.IsMatch(addChild.Email, EmailPattern))
                    ErrorMessages.Add("البريد الإلكتروني غير صحيح");
                else
                {
                    if (!string.IsNullOrEmpty(addChild.Password) || !string.IsNullOrEmpty(addChild.ConfirmPassword))
                    {
                        if (addChild.Password == addChild.ConfirmPassword)
                        {
                            ApplicationUser user = new()
                            {
                                UserName = addChild.Email,
                                Email = addChild.Email,
                            };

                            var emailexists = await _userManager.FindByEmailAsync(addChild.Email);
                            var result = await userManager.CreateAsync(user, addChild.Password ?? "");

                            if (result.Succeeded)
                                parentId = user.Id;
                            else if (emailexists != null)
                                ErrorMessages.Add("البريد الاليكترونى موجود بالفعل.");
                            else
                                ErrorMessages.Add("كلمة المرور لا تفي بالمتطلبات الأمنية");
                        }
                        else
                            ErrorMessages.Add("كلمة المرور وتأكيد كلمة المرور غير متطابقين");
                    }
                    else
                    {
                        var user = await _userManager.FindByEmailAsync(addChild.Email);

                        if (user != null)
                            parentId = user.Id;
                        else
                            ErrorMessages.Add("لم يتم العثور على حساب بالبريد الإلكتروني المدخل");
                    }
                }
            }

            #endregion

            if (ErrorMessages.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("-", ErrorMessages);

                if (ChildImageName != null)
                    ImageManger.DeleteImage(ChildImageName, ImageLocation.Childrens);
                if (DisclaimerImageName != null)
                    ImageManger.DeleteImage(DisclaimerImageName, ImageLocation.Disclaimers);

                return View(addChild);
            }

            #endregion

            #region Edit Child

            if(!string.IsNullOrEmpty(ChildImageName))
                ImageManger.DeleteImage(child.ChildImageName, ImageLocation.Childrens);

            if (!string.IsNullOrEmpty(DisclaimerImageName))
                ImageManger.DeleteImage(child.DisclaimerImage ?? "", ImageLocation.Disclaimers);

            child.BirthDay = addChild.BirthDay;
            child.ChildImageName = ChildImageName ?? child.ChildImageName;
            child.Gender = addChild.Gender;
            child.Name = addChild.Name;
            child.NationalId = addChild.NationalId;
            child.WhatsappNumber = addChild.WhatsappNumber;
            child.Phone = addChild.Phone;
            child.ParentsNote = addChild.ParentsNote;
            child.SupervisorNote = addChild.SupervisorNote;
            child.ParentId = parentId;
            child.DisclaimerImage = DisclaimerImageName ?? child.DisclaimerImage;
            child.IsApproved = addChild.IsApproved;
            child.IsAllowedToSubscribe = addChild.IsAllowedToSubscribe;
            child.HaveHealthCondition = addChild.HaveHealthCondition;
            child.HealthCondition = addChild.HaveHealthCondition ? addChild.HealthCondition : null;
            child.IsDisabled = addChild.IsDisabled;
            child.DisableDescription = addChild.IsDisabled ? addChild.DisableDescription : null;
            child.IsEscort = addChild.IsEscort;
            child.EscortReason = addChild.IsEscort ? addChild.EscortReason : EscortReason.None;
            child.BlackList = addChild.BlackList;
            child.BlackListReason = addChild.BlackList ? addChild.BlackListReason : null;
            

            _unitOfWork.Repository<Child>().Update(child);
            #endregion

            TempData["SuccessMessage"] = $"تم تعديل بيانات طفل {addChild.Name} بنجاح كود الخاص به {child.ChildId}.";

            return RedirectToAction("ChildFile",new {id = addChild.Id});
        }
        #endregion
        #region Delete Child File
        public IActionResult DeleteChildFile(int id)
        {
            var child = _unitOfWork.Repository<Child>().GetOne(e => e.ChildId == id);

            if(child == null)
            {
                TempData["ErrorMessage"] = "لا يوجد طفل بهذا الكود!!";
            }
            else
            {
                child.IsDeleted = true;

                _unitOfWork.Repository<Child>().Update(child);

                TempData["SuccessMessage"] = "تم الحذف بنجاح";
            }

            return RedirectToAction("WhichChildFile");
        }
        #endregion
    }
}
