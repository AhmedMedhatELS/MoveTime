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
    public class WhatsAppController(WhatsAppService whatsAppService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly WhatsAppService _whatsAppService = whatsAppService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        [HttpGet]
        public IActionResult SendMessageForm()
        {
            var childs = _unitOfWork.Repository<Child>().Get(e => !e.IsDeleted && e.IsApproved);
            MassagesView view = new();

            foreach (var child in childs)
                view.Children.Add(new WhichChild
                {
                    Id = child.ChildId,
                    ChildImage = child.ChildImageName,
                    Name = child.Name,
                    OnBlackList = child.BlackList,
                    WhatsAppNumber = child.WhatsappNumber
                });

            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(MassagesView massages)
        {
            #region cheack data
            if(massages == null || massages.Massage == null || massages.NumbersToSendTo == null)
            {
                TempData["ErrorMessage"] = "خطا فى المعلومات!!";
                return RedirectToAction("SendMessageForm");
            }

            #endregion

            #region send massages 
            _whatsAppService.SendWhatsAppMessageList([.. massages.NumbersToSendTo.Split("-")], massages.Massage);
            
            TempData["SuccessMessage"] = "لقد تم ارسال الرساله بنجاح";
            #endregion

            return RedirectToAction("SendMessageForm");
        }

    }
}
