using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class EventController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region Add Edit Delete Event
        public IActionResult ManageEvents() => View(new EventView
            {Events = _unitOfWork.Repository<Event>().Get(e => e.Status != EventStatus.Deleted).ToList()});
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddEditEvent(EventView view)
        {
            if (!ModelState.IsValid || view.Status == EventStatus.Deleted) return RedirectToAction("ManageEvents");

            Event _event = new()
            {
                EventId = view.EventId,
                EventDescription = view.EventDescription,
                EventName = view.EventName,
                Price = view.Price,
                Status = view.Status
            };

            if (view.EventId == 0)
            {
                _unitOfWork.Repository<Event>().Add(_event);
                TempData["SuccessMessage"] = "تم إضافة الفعالية بنجاح.";
            }
            else
            {
                _unitOfWork.Repository<Event>().Update(_event);
                TempData["SuccessMessage"] = "تم تحديث الفعالية بنجاح.";
            }

            return RedirectToAction("ManageEvents");
        }
        public IActionResult DeleteEvent(int id)
        {
            //get event
            var _event = _unitOfWork.Repository<Event>().GetOne(e => e.EventId == id);

            if (_event == null)
            {
                TempData["ErrorMessage"] = "الفعالية غير موجود.";
            }
            else
            {
                _event.Status = EventStatus.Deleted;
                _unitOfWork.Repository<Event>().Update(_event);
                TempData["SuccessMessage"] = "تم حذف الفعالية بنجاح.";
            }

            return RedirectToAction("ManageEvents");
        }
        #endregion
    }
}
