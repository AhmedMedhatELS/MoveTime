using DataAccess.Repository.IRepository;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Models;
using Models.ViewModels.Statistics;
using Models.ViewModels.CheckViews;

namespace Shared
{
    public class Statistics(IUnitOfWork unitOfWork) : BaseService(unitOfWork)
    {
        //sunday = 0,1,2,3,4,5,6
        //private readonly int CurrentDay = (int)(DateTime.Now.DayOfWeek);
        //private readonly DateTime currentDate = DateTime.Now;
        //private readonly int CurrentMonth = DateTime.Now.Month;
        #region unfinshed
        //private static StatisticsTableView PrepareView(List<CheckInOut> checkInOuts)
        //{
        //    StatisticsTableView view = new()
        //    {
        //        Total = checkInOuts.Sum(e => e.InTotal + e.OutTotal)
        //    };

        //    if (checkInOuts.Count > 0)
        //        foreach (var check in checkInOuts)
        //        {
        //            if (check.InPayment == PaymentMethod.دین)
        //                view.Dept += check.InTotal;
        //            else
        //                view.Payed += check.InTotal;

        //            if (check.OutPayment == PaymentMethod.دین)
        //                view.Dept += check.OutTotal;
        //            else
        //                view.Payed += check.OutTotal;

        //            view.KidsNumber += check.Children.Count;

        //            view.CheckViews.Add(new StatisticCheckView
        //            {
        //                CheckInBy = check.CheckInBy.ToString(),
        //                CheckInTime = check.CheckIn,
        //                CheckOutTime = check.ActualCheckout ?? check.CheckIn,
        //                InTotal = check.InTotal,
        //                OutTotal = check.OutTotal,
        //                InDebt = check.InPayment == PaymentMethod.دین,
        //                OutDebt = check.OutPayment == PaymentMethod.دین,
        //            });

        //            foreach (var child in check.Children)
        //                view.CheckViews.Last().ChildChecks.Add(new ChildCheckInTable
        //                {
        //                    Id = child.ChildId,
        //                    ImageName = child.ChildImageName,
        //                    Name = child.Name
        //                });

        //        }

        //    return view;
        //}

        //public StatisticsTableView? LoggedInByDay(int targetDay)
        //{
        //    if (targetDay < 0 || targetDay > 6) return null;

        //    int daysDifference = targetDay - CurrentDay;

        //    if(daysDifference > 0)            
        //        daysDifference -= 7;

        //    var targetDate = currentDate.AddDays(daysDifference).Date;

        //    var checkInOutForTheSpcificDay = _unitOfWork.Repository<CheckInOut>().Get(
        //        e => e.Status == CheckStatus.Out && e.DateLog == targetDate,
        //        ch => ch.Children
        //        ).ToList();

        //    return PrepareView(checkInOutForTheSpcificDay);
        //}
        //public StatisticsTableView? LoggedInByMonth(int targetMonth)
        //{
        //    if (targetMonth < 1 || targetMonth > 12) return null;

        //    int monthsDifference = targetMonth - CurrentMonth;

        //    if (monthsDifference > 0)
        //        monthsDifference -= 7;

        //    var targetDate = currentDate.AddMonths(monthsDifference).Date;

        //    var checkInOutForTheSpcificDay = _unitOfWork.Repository<CheckInOut>().Get(
        //        e => e.Status == CheckStatus.Out && e.DateLog == targetDate,
        //        ch => ch.Children
        //        ).ToList();

        //    return PrepareView(checkInOutForTheSpcificDay);
        //}
        #endregion

        public StatisticsTableView LogHistory(DateTime From,DateTime To)
        {
            if(From > To) return new StatisticsTableView();

            var checkInOuts = _unitOfWork.Repository<CheckInOut>().Get(
              e => e.Status == CheckStatus.Out 
              && e.DateLog >= From.Date 
              && e.DateLog <= To.Date,
              ch => ch.Children
              ).ToList();

            StatisticsTableView view = new()
            {
                Total = checkInOuts.Sum(e => e.InTotal + e.OutTotal)
            };

            if (checkInOuts.Count > 0)
                foreach (var check in checkInOuts)
                {
                    if (check.InPayment == PaymentMethod.دین)
                        view.Dept += check.InTotal;
                    else
                        view.Payed += check.InTotal;

                    if (check.OutPayment == PaymentMethod.دین)
                        view.Dept += check.OutTotal;
                    else
                        view.Payed += check.OutTotal;

                    view.KidsNumber += check.Children.Count;

                    view.CheckViews.Add(new StatisticCheckView
                    {
                        CheckInBy = check.CheckInBy.ToString(),
                        CheckInTime = check.CheckIn,
                        CheckOutTime = check.ActualCheckout ?? check.CheckIn,
                        InTotal = check.InTotal,
                        OutTotal = check.OutTotal,
                        InDebt = check.InPayment == PaymentMethod.دین,
                        OutDebt = check.OutPayment == PaymentMethod.دین,
                    });

                    foreach (var child in check.Children)
                        view.CheckViews.Last().ChildChecks.Add(new ChildCheckInTable
                        {
                            Id = child.ChildId,
                            ImageName = child.ChildImageName,
                            Name = child.Name
                        });
                }

            return view;
        }

        public string ChildOverAllTime(int id)
        {
            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == id,
                ch => ch.CheckInOuts.Where(e => e.Status == CheckStatus.Out)
                );

            if (child == null) return "";

            double TotalMinutes = 0;

            foreach (var checkInOut in child.CheckInOuts)
                TotalMinutes += ((checkInOut.ActualCheckout ?? TimeSpan.Zero) - checkInOut.CheckIn).TotalMinutes;

            return $"{(int)TotalMinutes/60} ساعة و {(int)TotalMinutes%60} دقيقة";
        }

        public int ChildDebtAmount(int id)
        {
            var child = _unitOfWork.Repository<Child>().GetOne(
                e => e.ChildId == id && !e.IsDeleted,
                cs => cs.ChildSubscriptions.Where(e => e.HaveDebt),
                c => c.CheckInOuts.Where(e => e.InPayment == PaymentMethod.دین ||
                    e.OutPayment == PaymentMethod.دین)
                );

            if (child == null) return 0;

            return child.ChildSubscriptions.Sum(e => e.Remaining) +
                   child.CheckInOuts.Where(e => e.InPayment == PaymentMethod.دین).Sum(e => e.InTotal) +
                   child.CheckInOuts.Where(e => e.OutPayment == PaymentMethod.دین).Sum(e => e.OutTotal); 
        }
    }
}
