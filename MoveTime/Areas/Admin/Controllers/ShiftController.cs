using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System.Globalization;
using Utility;

namespace MoveTime.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class ShiftController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : Controller
    {
        #region Start Up
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly string morningName = ConstantNames.MorningName;        
        //TempData["SuccessMessage"]
        //TempData["ErrorMessage"]
        #endregion
        #region Morning Shift
        public IActionResult EditMorningHours()
        {
            var morningshift = _unitOfWork.Repository<Shift>().GetOne(e => e.ShiftName == morningName);

            #region if there is no morning shift make one
            if (morningshift == null)
            {
                morningshift = new Shift
                {
                    ShiftName = morningName,
                    StartTime = null,
                    EndTime = null
                };
                _unitOfWork.Repository<Shift>().Add(morningshift);
            }
            #endregion

            return View(morningshift);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMorningHours(Shift shift)
        {
            if (shift.EndTime != null &&
                shift.StartTime != null &&
                shift.ShiftId != 0 &&
                shift.ShiftName == morningName)
            {
                _unitOfWork.Repository<Shift>().Update(shift);
                TempData["SuccessMessage"] = "تم حفظ تعديل على فترة الصباح بنجاح";
            }
            else
                TempData["ErrorMessage"] = "خطأ: لم يتم حفظ البيانات. يرجى التحقق من صحة المدخلات.";

            return RedirectToAction("EditMorningHours");
        }
        #endregion
        #region Morning / Evening
        #region show all the hours
        public IActionResult EditPrices(WhichShift whichShift)
        {
            #region cheack 1st if there is morning shift
            var morningshift = _unitOfWork.Repository<Shift>().GetOne(e => e.ShiftName == morningName);
            if (morningshift == null || morningshift.StartTime == null || morningshift.EndTime == null)
            {
                TempData["ErrorMessage"] = "يجب اضافة فترة الصباح اولا";
                return RedirectToAction("EditMorningHours");
            }
            #endregion
            #region prebare the lists
            List<EditHour> editHours = [];

            var hours_ME = _unitOfWork.Repository<ShiftHour>().Get(
                e =>
                    e.WhichShift == whichShift,
                range => range.MinuteRanges
                ).OrderBy(e => e.HourNumber).ToList();
            #endregion

            if (hours_ME == null || hours_ME.Count == 0) //If no hours add the 1st one
            {
                #region add the 1st shift hour and the 1st minutes range to DB
                var firstHour = new ShiftHour
                {
                    HourNumber = 1,
                    WhichShift = whichShift
                };

                _unitOfWork.Repository<ShiftHour>().Add(firstHour);

                firstHour.MinuteRanges.Add(new MinuteRange
                {
                    ShiftHourId = firstHour.ShiftHourId,
                    Start = 1
                });

                _unitOfWork.Repository<MinuteRange>().AddRange(firstHour.MinuteRanges);
                #endregion
                #region add to the view list
                editHours.Add(new EditHour
                {
                    AsPrevious = false,
                    Id = firstHour.ShiftHourId,
                    HourNumber = firstHour.HourNumber,
                    MinutesRangeList =
                    [
                        new() {
                            Start = firstHour.MinuteRanges.ToList()[0].Start,
                            End = firstHour.MinuteRanges.ToList()[0].End,
                            Price = firstHour.MinuteRanges.ToList()[0].Price
                        }
                    ]
                });
                #endregion
            }
            else //if there is hours add it to the view list
            {
                #region add each hour to the view list
                foreach (var hour in hours_ME) 
                {
                    var elementEditHour = new EditHour
                    {
                        HourNumber = hour.HourNumber,
                        AsPrevious = hour.AsPrevious,
                        Id = hour.ShiftHourId                        
                    };
                    #region add each range in the hour to the view list
                    foreach (var minute in hour.MinuteRanges)
                    {
                        elementEditHour.MinutesRangeList.Add(new()
                                {
                                    Start = minute.Start,
                                    End = minute.End,
                                    Price = minute.Price
                                }
                            );
                    }
                    #endregion
                    editHours.Add(elementEditHour);
                }
                #endregion
                #region add new hour
                if (hours_ME.All(e => e.IsCompleated))
                {
                    int lastHour = hours_ME.Max(hour => hour.HourNumber);

                    var newHour = new ShiftHour
                    {
                        HourNumber = lastHour + 1,
                        WhichShift = whichShift
                    };

                    _unitOfWork.Repository<ShiftHour>().Add(newHour);

                    newHour.MinuteRanges.Add(new MinuteRange
                    {
                        ShiftHourId = newHour.ShiftHourId,
                        Start = 1
                    });

                    _unitOfWork.Repository<MinuteRange>().AddRange(newHour.MinuteRanges);

                    editHours.Add(new EditHour
                    {
                        AsPrevious = false,
                        Id = newHour.ShiftHourId,
                        HourNumber = newHour.HourNumber,
                        MinutesRangeList =
                        [
                            new() {
                            Start = newHour.MinuteRanges.ToList()[0].Start,
                            End = newHour.MinuteRanges.ToList()[0].End,
                            Price = newHour.MinuteRanges.ToList()[0].Price
                        }
                        ]
                    });
                }
                #endregion
            }

            #region for the views
            TempData["whichShift"] = whichShift.ToString();
            TempData["whichShiftAdminPartial"] = whichShift.ToString();
            #endregion
            return View(editHours);
        }
        #endregion
        #region save the hour
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHour(SaveHour SaveHour)
        {
            #region cheack the vaildation of the revivied data
            #region cheack for the revived data does it exists or not
            if (SaveHour == null)
            {
                TempData["ErrorMessage"] = "لا يوجد معلومات لتسجل.";
                return RedirectToAction("EditMorningHours");
            }
            //cheack if the hour is exists or not
            var shiftHour = _unitOfWork.Repository<ShiftHour>().GetOne(
                e =>
                    e.ShiftHourId == SaveHour.Id,
                ranges => ranges.MinuteRanges);
            if (shiftHour == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ساعة الوردية المطلوبة.";
                return RedirectToAction("EditPrices", new { whichShift = SaveHour.WhichShift });
            }
            if(shiftHour.AsPrevious)
            {
                TempData["ErrorMessage"] = "هذه الساعة مطابقة لسابقتها لا يمكن تعديل ";
                return RedirectToAction("EditPrices", new { whichShift = SaveHour.WhichShift });
            }
            if (SaveHour.HourData == null)
            {
                TempData["ErrorMessage"] = "يجب إدخال بيانات الساعة.";
                return RedirectToAction("EditPrices", new { whichShift = SaveHour.WhichShift });
            }
            #endregion
            #region cheack for the ranges that revcived

            //${start}-${end}:${price}
            var ranges = SaveHour.HourData.Split(",");
            List<EditMinuteRange> rangesList = [];
            int lastStart = 1,start = 0,end = 0,price = 0;
            bool IsRangeFinshed = false;

            foreach (var range in ranges) 
            {
                var parts = range.Split(['-', ':']);

                if (!IsRangeFinshed &&
                    parts.Length == 3 &&
                    int.TryParse(parts[0], out start) &&
                    int.TryParse(parts[1],out end) &&
                    int.TryParse(parts[2],out price) &&
                    start == lastStart &&
                    end > start && end <= 60 &&
                    price >= 0
                    )
                {
                    lastStart = end + 1;
                    IsRangeFinshed = end == 60;
                    rangesList.Add(new()
                    {
                        End = end,
                        Start = start,
                        Price = price
                    });
                }
                else // if there is error
                {
                    #region Create a specific error message
                    if (parts.Length != 3)
                    {
                        TempData["ErrorMessage"] = "يجب أن يحتوي النطاق على 3 أجزاء: البداية، النهاية، والسعر.";
                    }
                    else if (!int.TryParse(parts[0], out _))
                    {
                        TempData["ErrorMessage"] = "توقيت البداية غير صالح.";
                    }
                    else if (!int.TryParse(parts[1], out _))
                    {
                        TempData["ErrorMessage"] = "توقيت النهاية غير صالح.";
                    }
                    else if (!int.TryParse(parts[2], out _))
                    {
                        TempData["ErrorMessage"] = "السعر غير صالح.";
                    }
                    else if (start != lastStart)
                    {
                        TempData["ErrorMessage"] = "يجب أن تكون بداية النطاق متساوية مع القيمة السابقة.";
                    }
                    else if (end <= start || end > 60)
                    {
                        TempData["ErrorMessage"] = "يجب أن يكون توقيت النهاية أكبر من البداية وألا يتجاوز 60.";
                    }
                    else if (price < 0)
                    {
                        TempData["ErrorMessage"] = "يجب أن يكون السعر أكبر من أو يساوي 0.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "حدث خطأ غير معروف.";
                    }
                    return RedirectToAction("EditPrices", new { whichShift = SaveHour.WhichShift });
                    #endregion
                }
            }
            #endregion
            #endregion
            #region saving the hour details
            _unitOfWork.Repository<MinuteRange>().RemoveRange(shiftHour.MinuteRanges);

            foreach(var range in rangesList)            
                shiftHour.MinuteRanges.Add(new()
                {
                    End = range.End,
                    Start = range.Start,
                    Price = range.Price,
                    ShiftHourId = shiftHour.ShiftHourId
                });            

            _unitOfWork.Repository<MinuteRange>().AddRange(shiftHour.MinuteRanges);

            #region if the hour was new
            if (!shiftHour.IsCompleated)
            {
                shiftHour.IsCompleated = true;
                _unitOfWork.Repository<ShiftHour>().Update(shiftHour);
            }
            #endregion
            TempData["SuccessMessage"] = $"تم حفظ الساعة {shiftHour.HourNumber}";
            #endregion
            return RedirectToAction("EditPrices",new { whichShift = SaveHour.WhichShift}); 
        }
        #endregion
        #region delete hour and all the hours that comes after it
        public IActionResult DeleteHour(int id,WhichShift whichShift)
        {
            #region cheack if thier is hour with the revived id
            var hour = _unitOfWork.Repository<ShiftHour>()
                .GetOne(e => e.ShiftHourId == id,range => range.MinuteRanges);

            if (hour == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ساعة الوردية المطلوبة.";
                return RedirectToAction("EditPrices", new { whichShift });
            }
            #endregion
            #region get all the hours after it and delete them all
            var hoursAfterIt = _unitOfWork.Repository<ShiftHour>().Get(e =>
                e.HourNumber > hour.HourNumber && e.WhichShift == whichShift,
                range => range.MinuteRanges
            );

            foreach(var hourAfter in  hoursAfterIt)            
                _unitOfWork.Repository<MinuteRange>().RemoveRange(hourAfter.MinuteRanges);

            _unitOfWork.Repository<ShiftHour>().RemoveRange(hoursAfterIt);

            _unitOfWork.Repository<MinuteRange>().RemoveRange(hour.MinuteRanges);

            _unitOfWork.Repository<ShiftHour>().Remove(hour);

            TempData["SuccessMessage"] = "تم حذف الساعة والساعات التي تليها والبيانات المتعلقة بهم بنجاح.";
            #endregion
            return RedirectToAction("EditPrices", new { whichShift });
        }
        #endregion
        #region Match Previous Hour
        public IActionResult MatchPreviousHour(int id, WhichShift whichShift) 
        {
            #region cheack if there is a hour with this id
            var hour = _unitOfWork.Repository<ShiftHour>().GetOne(
                e =>
                    e.ShiftHourId == id && !e.AsPrevious && e.HourNumber != 1,
                ranges => ranges.MinuteRanges);

            if (hour == null) 
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ساعة الوردية المطلوبة.";
                return RedirectToAction("EditPrices", new { whichShift });
            }
            #endregion
            #region match the hour with privous one
            _unitOfWork.Repository<MinuteRange>().RemoveRange(hour.MinuteRanges);
            hour.AsPrevious = true;
            hour.IsCompleated = true;
            _unitOfWork.Repository<ShiftHour>().Update(hour);
            TempData["SuccessMessage"] = $"تم مطابقة الساعة {hour.HourNumber} مع التي قبلها.";
            #endregion
            return RedirectToAction("EditPrices", new { whichShift });
        }
        #endregion
        #region Remove Matching
        public IActionResult RemoveMatching(int id, WhichShift whichShift)
        {
            #region cheack if there is a hour with this id
            var hour = _unitOfWork.Repository<ShiftHour>().GetOne(
                e =>
                    e.ShiftHourId == id && e.AsPrevious && e.HourNumber != 1,
                ranges => ranges.MinuteRanges);

            if (hour == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ساعة الوردية المطلوبة.";
                return RedirectToAction("EditPrices", new { whichShift });
            }
            #endregion
            #region find which hour
            ShiftHour? shiftHour;
            int hourNumber = hour.HourNumber - 1;

            while (true)
            {
                shiftHour = _unitOfWork.Repository<ShiftHour>().GetOne(
                    e =>
                        e.HourNumber == hourNumber &&
                        e.WhichShift == whichShift &&
                        !e.AsPrevious,
                    renges => renges.MinuteRanges);

                if (shiftHour != null)
                    break;

                hourNumber--;

                if (hourNumber <= 0) 
                {
                    TempData["ErrorMessage"] = "حدث خطا فالنظام غير معلوم!!!";
                    return RedirectToAction("EditPrices", new { whichShift });
                }
            }
            #endregion
            #region remove the mathching
            List<MinuteRange> ranges = [];

            foreach(var range in shiftHour.MinuteRanges)            
                ranges.Add(new()
                {
                    End = range.End,
                    Start = range.Start,
                    Price = range.Price,
                    ShiftHourId = id
                });
            
            _unitOfWork.Repository<MinuteRange>().AddRange(ranges);
            hour.AsPrevious = false;
            _unitOfWork.Repository<ShiftHour>().Update(hour);
            TempData["SuccessMessage"] = $"لقد تم فك ربط الساعة {hour.HourNumber} بنجاح.";
            #endregion

            return RedirectToAction("EditPrices", new { whichShift });
        }
        #endregion
        #endregion
        #region List of prices     
        public IActionResult ShowPrices()
        {
            #region check for the morning hours
            var morningshift = _unitOfWork.Repository<Shift>().GetOne(
                e => e.ShiftName == morningName);

            if (morningshift == null || morningshift.StartTime == null || morningshift.EndTime == null)
            {
                TempData["ErrorMessage"] = "يجب اضافة فترة الصباح اولا";
                return RedirectToAction("EditMorningHours");
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

            if(eveningHours.Count <= 0 || morningHours.Count <= 0)
            {
                TempData["ErrorMessage"] = "يجب ايضافة اسعار الساعات اولا.";
                return RedirectToAction("EditPrices", new { whichShift = eveningHours.Count <= 0 ? WhichShift.Evening : WhichShift.Morning });
            }
            #endregion

            #region prepare hours list for the view
            //morning hours
            foreach( var hourData in morningHours )
            {
                if(hourData.AsPrevious)
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
    }
}
