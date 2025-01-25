using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models;
using Models.ViewModels.UserViews;
using Shared;
using System.Text.RegularExpressions;
using Utility;
using Models.ViewModels.ChildFile;

namespace MoveTime.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ChildController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region add new child
        public IActionResult AddChild() => View(new NewChildUser());

        [HttpPost]
        public IActionResult AddChild(NewChildUser addChild)
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

            #region parent

            parentId = _userManager.GetUserId(User);

            if (parentId == null)
                ErrorMessages.Add("خطأ فى معلومات حاول مرة أخرى");

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
                SupervisorNote = null,
                ParentId = parentId,
                BlackList = false,
                DisclaimerImage = DisclaimerImageName,
                IsApproved = false,
                IsAllowedToSubscribe = false,
                HaveHealthCondition = addChild.HaveHealthCondition,
                HealthCondition = addChild.HaveHealthCondition ? addChild.HealthCondition : null,
                IsDisabled = addChild.IsDisabled,
                DisableDescription = addChild.IsDisabled ? addChild.DisableDescription : null,
                IsEscort = false,
                EscortReason = EscortReason.None
            };

            _unitOfWork.Repository<Child>().Add(child);
            #endregion

            TempData["SuccessMessage"] = $"تم اضافة طفل {addChild.Name} بنجاح كود الخاص به {child.ChildId}.";

            return View();
        }
        #endregion
        #region child file
        public IActionResult ChildFile(int id)
        {
            #region check data
            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == id && !e.IsDeleted,
                parent => parent.Parent);

            if (child == null)
            {
                return RedirectToAction("index","Home");
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
                ChildEscortReason = EscortReasonTranslations.Translations[child.EscortReason]
            };
            #endregion
            #region child subscriptions
            var childSub = _unitOfWork.Repository<ChildSubscription>().Get(
                e => e.ChildId == id, pl => pl.SubscriptionPlan, su => su.SubscriptionPlan.Subscription
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
                    ev => ev.Event, ch => ch.CheckChildren
                ).ToList();

            if (childchecks.Count > 0)
            {
                foreach (var inout in childchecks)
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
        #region Edit Child
        public IActionResult EditChild(int id)
        {
            var child = _unitOfWork.Repository<Child>().GetOne(e => e.ChildId == id);

            if (child == null)
            {
                TempData["ErrorMessage"] = "خطأ فى المعلومات";
                return RedirectToAction("Index","Home");
            }

            var view = new EditChild
            {
                Id = child.ChildId,
                Name = child.Name,
                ChildImageName = child.ChildImageName,
                BirthDay = child.BirthDay,
                DisableDescription = child.DisableDescription,
                Gender = child.Gender,
                HealthCondition = child.HealthCondition,
                HaveHealthCondition = child.HaveHealthCondition,
                Oldyears = child.Oldyears,
                NationalId = child.NationalId,
                IsDisabled = child.IsDisabled,
                Phone = child.Phone,
                ParentsNote = child.ParentsNote,
                WhatsappNumber = child.WhatsappNumber
            };

            return View(view);
        }

        [HttpPost]
        public async Task<IActionResult> EditChild(EditChild addChild)
        {
            #region check 
            if (addChild == null) return RedirectToAction("Index","Home");

            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == addChild.Id && !e.IsDeleted
                );

            if (child == null) return RedirectToAction("Index", "Home");

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

            if (!string.IsNullOrEmpty(ChildImageName))
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
            child.DisclaimerImage = DisclaimerImageName ?? child.DisclaimerImage;
            child.IsApproved = false;
            child.HaveHealthCondition = addChild.HaveHealthCondition;
            child.HealthCondition = addChild.HaveHealthCondition ? addChild.HealthCondition : null;
            child.IsDisabled = addChild.IsDisabled;
            child.DisableDescription = addChild.IsDisabled ? addChild.DisableDescription : null;

            _unitOfWork.Repository<Child>().Update(child);
            #endregion

            TempData["SuccessMessage"] = $"تم تعديل بيانات طفل {addChild.Name} بنجاح كود الخاص به {child.ChildId}.";

            return RedirectToAction("ChildFile", new { id = addChild.Id });
        }
        #endregion
    }
}
