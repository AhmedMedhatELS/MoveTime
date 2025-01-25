using DataAccess;
using DataAccess.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Shared
{
    public class CalculatePrice(IUnitOfWork unitOfWork) : BaseService(unitOfWork)
    {
        private readonly string morningName = ConstantNames.MorningName;
        private List<ShiftHour> morningPrices = [];
        private List<ShiftHour> eveningPrices = [];
        private Shift? morningShift;

        public int? GetSubscriptionIntervalprice(TimeSpan Begin, TimeSpan End,int SubscriptionPlanId)
        {
            int? price = null;

            var SubPlan = _unitOfWork.Repository<SubscriptionPlan>().GetOne(
                e => e.SubscriptionPlanId == SubscriptionPlanId
                );

            if(SubPlan == null || End <= Begin) return price;

            if(SubPlan.IsDuration)
            {
                if ((End - TimeSpan.FromHours(SubPlan.Duration)) <= Begin)
                    price = 0;
                else
                {
                    var total = GetPrice(Begin, End);
                    var freeperiod = GetPrice(Begin, Begin + TimeSpan.FromHours(SubPlan.Duration));

                    price = total == null || freeperiod == null ? null : total - freeperiod;
                }                                      
            }
            else if(!(SubPlan.From == null || SubPlan.To == null))
            {
                if (Begin >= SubPlan.From && End <= SubPlan.To)
                    price = 0;
                else
                {
                    //total price 
                    var total = GetPrice(Begin,End);
                    //price of the interval before plan (from : to)
                    var before = Begin < SubPlan.From ? GetPrice(Begin, (TimeSpan)SubPlan.From) : 0;
                    //price of the interval from the Begin to the plan end time
                    var beginToEndPlan = End > SubPlan.To && Begin < SubPlan.To ? GetPrice(Begin, (TimeSpan)SubPlan.To) : 0;
                    
                    if (total == null || before == null || beginToEndPlan == null) return price;

                    var freeperiod = total * (Begin < SubPlan.From && End <= SubPlan.To ? 1 : 0)
                                    - before
                                    + beginToEndPlan;

                    price = total - freeperiod;
                }
            }

            return price;
        }

        public int? GetPrice(TimeSpan Begin, TimeSpan End)
        {
            int? price = null;

            #region get shifts and prices and cheack for the data
            morningShift = _unitOfWork.Repository<Shift>().GetOne(e => e.ShiftName == morningName);
           
            morningPrices = _unitOfWork.Repository<ShiftHour>().Get(
                e => e.WhichShift == WhichShift.Morning && e.IsCompleated,
                range => range.MinuteRanges
                ).OrderBy(e => e.HourNumber).ToList();
            
            eveningPrices = _unitOfWork.Repository<ShiftHour>().Get(
                e => e.WhichShift == WhichShift.Evening && e.IsCompleated,
                range => range.MinuteRanges
                ).OrderBy(e => e.HourNumber).ToList();

            if(morningShift ==  null || 
                morningShift.StartTime == null || morningShift.EndTime == null || 
                morningPrices == null || morningPrices.Count == 0 ||
                eveningPrices == null || eveningPrices.Count == 0 ||
                Begin >= End || Begin < morningShift.StartTime)
                return price;
            #endregion

            #region cheack in which shift 
            if (morningShift.StartTime <= Begin && morningShift.EndTime >= End)
                price = CalMinutes((End - Begin).TotalMinutes, morningPrices);
            else if(morningShift.EndTime <= Begin)
                price = CalMinutes((End - Begin).TotalMinutes, eveningPrices);
            else
                price = CalMorningEvening(Begin, End,morningShift.EndTime ?? new TimeSpan());
            #endregion

            return price;
        }

        private static int CalMinutes(double minutes, List<ShiftHour> hours,int hourNumber = 1)
        {
            int price = 0;
            var hour = hours.First(e => e.HourNumber == hourNumber);
            int lastHour = hours.Max(e => e.HourNumber);

            while (minutes > 0) 
            {

                if (minutes >= 60)
                    price += hour.MinuteRanges.First(e => e.End == 60).Price;
                else
                    price += hour.MinuteRanges.First(e => e.Start <= minutes && e.End >= minutes).Price;

                minutes -= 60;            
                hourNumber++;

                if(hourNumber <= lastHour)                
                    hour = hours.FirstOrDefault(e => e.HourNumber == hourNumber && !e.AsPrevious) ?? hour; 
            }

            return price;
        }

        private int CalMorningEvening(TimeSpan Begin, TimeSpan End,TimeSpan MorningEnd)
        {
            var morningMinutes = (MorningEnd - Begin).TotalMinutes;
            var eveningMinutes = (End - MorningEnd).TotalMinutes;

            int hourNumber = ((int)morningMinutes / 60) + 1;
            int eveningLastHour = eveningPrices.Max(e => e.HourNumber);

            hourNumber = Math.Min(hourNumber, eveningLastHour);

            while (eveningPrices.First(e => e.HourNumber == hourNumber).AsPrevious)
                    hourNumber--;
            
            return CalMinutes(morningMinutes, morningPrices) +
                        CalMinutes(eveningMinutes, eveningPrices, hourNumber);
        }
    }
}
