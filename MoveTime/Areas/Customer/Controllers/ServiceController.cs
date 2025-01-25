using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Utility;

namespace MoveTime.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ServiceController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly string morningName = ConstantNames.MorningName;
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region List of prices     
        public IActionResult ShowPrices()
        {
            #region check for the morning hours
            var morningshift = _unitOfWork.Repository<Shift>().GetOne(
                e => e.ShiftName == morningName);

            if (morningshift == null || morningshift.StartTime == null || morningshift.EndTime == null)
            {
                return RedirectToAction("Index","Home");
            }
            #endregion

            //prebare view list
            PriceList priceList = new()
            {
                MorningHeader = $"أسعار الصباح {Format.FormatTimeInArabic(morningshift.StartTime.Value)} - {Format.FormatTimeInArabic(morningshift.EndTime.Value)}",
                EveningHeader = "أسعار المساء"
            };

            #region get all the hours
            var morningHours = _unitOfWork.Repository<ShiftHour>().Get(
                e =>
                    e.WhichShift == WhichShift.Morning &&
                    e.IsCompleated,
                ranges => ranges.MinuteRanges
                ).OrderBy(e => e.HourNumber).ToList();

            var eveningHours = _unitOfWork.Repository<ShiftHour>().Get(
                e =>
                    e.WhichShift == WhichShift.Evening &&
                    e.IsCompleated,
                ranges => ranges.MinuteRanges
                ).OrderBy(e => e.HourNumber).ToList();

            if (eveningHours.Count <= 0 || morningHours.Count <= 0)
            {
                TempData["ErrorMessage"] = "يجب ايضافة اسعار الساعات اولا.";
                return RedirectToAction("EditPrices", new { whichShift = eveningHours.Count <= 0 ? WhichShift.Evening : WhichShift.Morning });
            }
            #endregion

            #region prepare hours list for the view
            //morning hours
            foreach (var hourData in morningHours)
            {
                if (hourData.AsPrevious)
                {
                    priceList.MorningHours.Last().HoursHead += " و " + Format.NumberMapping[hourData.HourNumber];
                }
                else
                {
                    HourPriceList hourPrice = new()
                    {
                        HoursHead = Format.NumberMapping[hourData.HourNumber]
                    };

                    foreach (var range in hourData.MinuteRanges)
                        hourPrice.HourRangePrices.Add
                            ($"{range.Start} - {range.End} دقيقة: {range.Price} ريال");

                    priceList.MorningHours.Add(hourPrice);
                }
            }

            priceList.MorningHours.Last().HoursHead += ".....الخ";

            //evening hours
            foreach (var hourData in eveningHours)
            {
                if (hourData.AsPrevious)
                {
                    priceList.EveningHours.Last().HoursHead += " و " + Format.NumberMapping[hourData.HourNumber];
                }
                else
                {
                    HourPriceList hourPrice = new()
                    {
                        HoursHead = Format.NumberMapping[hourData.HourNumber]
                    };

                    foreach (var range in hourData.MinuteRanges)
                        hourPrice.HourRangePrices.Add
                            ($"{range.Start} - {range.End} دقيقة: {range.Price} ريال");

                    priceList.EveningHours.Add(hourPrice);
                }
            }

            priceList.EveningHours.Last().HoursHead += ".....الخ";
            #endregion
            return View(priceList);
        }
        #endregion
        #region show all Subscriptions
        public IActionResult ViewSubscriptions() => View(_unitOfWork.Repository<Subscription>().Get(null, n => n.Notes, p => p.Plans).ToList());
        #endregion
    }
}
