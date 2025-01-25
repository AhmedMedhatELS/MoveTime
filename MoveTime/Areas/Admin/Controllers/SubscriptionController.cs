using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System.Numerics;
using System.Text;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class SubscriptionController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region add new Subscription
        public IActionResult ManageSubscription() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSubscription(AdminSubscriptionView AdminSubscriptionView)
        {
            #region cheack the vaildation for the recived data
            StringBuilder ErrorMassages = new();

            if (AdminSubscriptionView == null)
                return BadRequest();
            else
            {
                // Validate Subscription Name
                if (string.IsNullOrEmpty(AdminSubscriptionView.SubscriptionName))
                    ErrorMassages.AppendLine("يجب إدخال اسم الاشتراك."); // "Subscription name is required."

                // Validate Subscription Description
                if (string.IsNullOrEmpty(AdminSubscriptionView.SubscriptionDescription))
                    ErrorMassages.AppendLine("يجب إدخال وصف الاشتراك."); // "Subscription description is required."

                // Validate Subscription Notes
                if (AdminSubscriptionView.SubscriptionNotes.Any(e => string.IsNullOrWhiteSpace(e)))
                    ErrorMassages.AppendLine("يجب إدخال جميع ملاحظات الاشتراك."); // "All subscription notes must be filled."

                // Validate Plans
                if (AdminSubscriptionView.Plans.Count <= 0)
                    ErrorMassages.AppendLine("يجب إضافة خطة واحدة على الأقل."); // "At least one subscription plan must be added."

                var errorcount = ErrorMassages.Length;

                // Validate each Plan
                foreach (var plan in AdminSubscriptionView.Plans)
                {
                    if (plan.Price <= 0)
                        ErrorMassages.AppendLine("يجب إدخال سعر صحيح للخطة."); // "A valid price must be entered for the plan."

                    if (plan.VisitsNumber <= 0)
                        ErrorMassages.AppendLine("يجب إدخال عدد زيارات صحيح للخطة."); // "A valid visit number must be entered for the plan."
                    
                    if (plan.ActiveDays <= 0)
                        ErrorMassages.AppendLine("يجب إدخال عدد تفعيل ايام للبرنامج.");

                    if (plan.SelectedDaysOfWeek.Count <= 0)
                        ErrorMassages.AppendLine("يجب اختيار يوم واحد على الأقل للخطة."); // "At least one day must be selected for the plan."

                    if (plan.IsDuration && plan.Duration <= 0)
                        ErrorMassages.AppendLine("يجب إدخال مدة صحيحة للخطة."); // "A valid duration must be entered for the plan."

                    if (!plan.IsDuration && (plan.From == null || plan.To == null))
                        ErrorMassages.AppendLine("يجب إدخال أوقات البدء والانتهاء للخطة."); // "Start and end times must be provided for the plan."

                    if (errorcount < ErrorMassages.Length)
                        break;
                }
            }

            if (ErrorMassages.Length > 0)
            {
                TempData["ErrorMessage"] = ErrorMassages.ToString().Replace(Environment.NewLine, "<br/>");

                return RedirectToAction("ManageSubscription");
            }
            #endregion

            #region add Subscription
            var Subscription = new Subscription 
            {
                Name = AdminSubscriptionView.SubscriptionName,
                Description = AdminSubscriptionView.SubscriptionDescription
            };

            _unitOfWork.Repository<Subscription>().Add(Subscription);
            #endregion
            #region add notes
            if (AdminSubscriptionView.SubscriptionNotes.Count > 0) 
            {
                List<SubscriptionNote> notes = [];
                foreach (var note in AdminSubscriptionView.SubscriptionNotes)
                    notes.Add(new SubscriptionNote 
                    {
                        Note = note,
                        SubscriptionId = Subscription.SubscriptionId
                    });
                _unitOfWork.Repository<SubscriptionNote>().AddRange(notes);
            }
            #endregion
            #region add plans
            List<SubscriptionPlan> plans = [];
            foreach (var plan in AdminSubscriptionView.Plans)
                plans.Add(new SubscriptionPlan
                {
                    DaysOfWeek = plan.SelectedDaysOfWeek.Count == 7 ?
                                    [Days.AllDays] : plan.SelectedDaysOfWeek,
                    Price = plan.Price,
                    VisitsNumber = plan.VisitsNumber,
                    IsDuration = plan.IsDuration,
                    Duration = plan.IsDuration ? plan.Duration : 0,
                    From = !plan.IsDuration ? plan.From : null,                    
                    To = !plan.IsDuration ? plan.To : null,
                    ActiveDays = plan.ActiveDays,
                    SubscriptionId = Subscription.SubscriptionId
                });
            _unitOfWork.Repository<SubscriptionPlan>().AddRange(plans);
            #endregion

            TempData["SuccessMessage"] = "تم اضافة الاشتراك بنجاح.";

            return RedirectToAction("ManageSubscription");
        }
        #endregion
        #region edit Subscription
        public IActionResult EditSubscription()
        {
            List<AdminSubscriptionView> SubscriptionsView = [];
            #region get all the Subscriptions
            var Subscriptions = _unitOfWork.Repository<Subscription>().Get(
                null,
                notes => notes.Notes,
                plans => plans.Plans
                );
            #endregion
            #region add Subscriptions in view 
            foreach (var sub in Subscriptions)
            {
                SubscriptionsView.Add(new AdminSubscriptionView 
                {
                    SubscriptionId = sub.SubscriptionId,
                    SubscriptionName = sub.Name,
                    SubscriptionDescription = sub.Description
                });

                foreach (var note in sub.Notes)
                    SubscriptionsView.Last().SubscriptionNotes.Add(note.Note);

                foreach (var plan in sub.Plans)
                    SubscriptionsView.Last().Plans.Add(new AdminSubscriptionPlanView 
                    {
                        Duration = plan.Duration,
                        From = plan.From,
                        IsDuration = plan.IsDuration,
                        To = plan.To,
                        Price = plan.Price,
                        VisitsNumber = plan.VisitsNumber,
                        ActiveDays = plan.ActiveDays,
                        SelectedDaysOfWeek = [.. plan.DaysOfWeek]
                    });
            }
            #endregion
            return View(SubscriptionsView);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEditSubscription(AdminSubscriptionView adminSubscriptionView)
        {
            #region cheack the vaildation for the recived data
            StringBuilder ErrorMassages = new();

            if (adminSubscriptionView == null) return BadRequest();
            else
            {
                // Validate Subscription Name
                if (string.IsNullOrEmpty(adminSubscriptionView.SubscriptionName))
                    ErrorMassages.AppendLine("يجب إدخال اسم الاشتراك."); // "Subscription name is required."

                // Validate Subscription Description
                if (string.IsNullOrEmpty(adminSubscriptionView.SubscriptionDescription))
                    ErrorMassages.AppendLine("يجب إدخال وصف الاشتراك."); // "Subscription description is required."

                // Validate Subscription Notes
                if (adminSubscriptionView.SubscriptionNotes.Any(e => string.IsNullOrWhiteSpace(e)))
                    ErrorMassages.AppendLine("يجب إدخال جميع ملاحظات الاشتراك."); // "All subscription notes must be filled."

                // Validate Plans
                if (adminSubscriptionView.Plans.Count <= 0)
                    ErrorMassages.AppendLine("يجب إضافة خطة واحدة على الأقل."); // "At least one subscription plan must be added."

                var errorcount = ErrorMassages.Length;

                // Validate each Plan
                foreach (var plan in adminSubscriptionView.Plans)
                {
                    if (plan.Price <= 0)
                        ErrorMassages.AppendLine("يجب إدخال سعر صحيح للخطة."); // "A valid price must be entered for the plan."

                    if (plan.VisitsNumber <= 0)
                        ErrorMassages.AppendLine("يجب إدخال عدد زيارات صحيح للخطة."); // "A valid visit number must be entered for the plan."
                    
                    if (plan.ActiveDays <= 0)
                        ErrorMassages.AppendLine("يجب إدخال عدد تفعيل ايام للبرنامج.");

                    if (plan.SelectedDaysOfWeek.Count <= 0)
                        ErrorMassages.AppendLine("يجب اختيار يوم واحد على الأقل للخطة."); // "At least one day must be selected for the plan."

                    if (plan.IsDuration && plan.Duration <= 0)
                        ErrorMassages.AppendLine("يجب إدخال مدة صحيحة للخطة."); // "A valid duration must be entered for the plan."

                    if (!plan.IsDuration && (plan.From == null || plan.To == null))
                        ErrorMassages.AppendLine("يجب إدخال أوقات البدء والانتهاء للخطة."); // "Start and end times must be provided for the plan."

                    if (errorcount < ErrorMassages.Length)
                        break;
                }
            }

            if (ErrorMassages.Length > 0)
            {
                TempData["ErrorMessage"] = ErrorMassages.ToString().Replace(Environment.NewLine, "<br/>");

                return RedirectToAction("EditSubscription");
            }            

            var Subscription = _unitOfWork.Repository<Subscription>().GetOne(
                e =>
                    e.SubscriptionId == adminSubscriptionView.SubscriptionId,
                    notes => notes.Notes,
                    Plan => Plan.Plans);

            if (Subscription == null)
            {
                TempData["ErrorMessage"] = "الاشتراك المطلوب غير موجود.";
                return RedirectToAction("EditSubscription");
            }
            #endregion
            #region edit Subscription           
            Subscription.Name = adminSubscriptionView.SubscriptionName;
            Subscription.Description = adminSubscriptionView.SubscriptionDescription;
            _unitOfWork.Repository<Subscription>().Update(Subscription);
            #endregion
            #region edit notes
            _unitOfWork.Repository<SubscriptionNote>().RemoveRange(Subscription.Notes);

            if (adminSubscriptionView.SubscriptionNotes.Count > 0)
            {
                List<SubscriptionNote> notes = [];
                foreach (var note in adminSubscriptionView.SubscriptionNotes)
                    notes.Add(new SubscriptionNote
                    {
                        Note = note,
                        SubscriptionId = Subscription.SubscriptionId
                    });
                _unitOfWork.Repository<SubscriptionNote>().AddRange(notes);
            }
            #endregion
            #region edit plans
            _unitOfWork.Repository<SubscriptionPlan>().RemoveRange(Subscription.Plans);

            List<SubscriptionPlan> plans = [];

            foreach (var plan in adminSubscriptionView.Plans)
                plans.Add(new SubscriptionPlan
                {
                    DaysOfWeek = plan.SelectedDaysOfWeek.Count == 7 ?
                                    [Days.AllDays] : plan.SelectedDaysOfWeek,
                    Price = plan.Price,
                    VisitsNumber = plan.VisitsNumber,
                    IsDuration = plan.IsDuration,
                    Duration = plan.IsDuration ? plan.Duration : 0,
                    From = !plan.IsDuration ? plan.From : null,
                    To = !plan.IsDuration ? plan.To : null,
                    ActiveDays = plan.ActiveDays,
                    SubscriptionId = Subscription.SubscriptionId
                });

            _unitOfWork.Repository<SubscriptionPlan>().AddRange(plans);
            #endregion

            TempData["SuccessMessage"] = "تم تعديل الاشتراك بنجاح.";

            return RedirectToAction("EditSubscription");
        }
        public IActionResult DeleteSubscription(int id)
        {
            var Subscription = _unitOfWork.Repository<Subscription>().GetOne(
                e =>
                    e.SubscriptionId == id,
                    notes => notes.Notes,
                    Plan => Plan.Plans);

            if (Subscription == null)
                TempData["ErrorMessage"] = "الاشتراك المطلوب غير موجود.";
            else
            {
                _unitOfWork.Repository<SubscriptionNote>().RemoveRange(Subscription.Notes);
                _unitOfWork.Repository<SubscriptionPlan>().RemoveRange(Subscription.Plans);
                _unitOfWork.Repository<Subscription>().Remove(Subscription);
                TempData["SuccessMessage"] = "تم حذف الاشتراكز";
            }

            return RedirectToAction("EditSubscription");
        }
        #endregion
        #region show all Subscriptions
        public IActionResult ViewSubscriptions() => View(_unitOfWork.Repository<Subscription>().Get(null,n => n.Notes,p => p.Plans).ToList());
        #endregion
    }
}
