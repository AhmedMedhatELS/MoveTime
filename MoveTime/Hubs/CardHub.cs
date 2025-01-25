using DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Models.ViewModels;
using Models;
using Shared;
using Utility;

namespace MoveTime.Hubs
{
    public class CardHub(CalculatePrice calculatePrice,
        IServiceProvider serviceProvider,
        SubscriptionCheck subscriptionCheck) : Hub
    {
        private readonly CalculatePrice _calculatePrice = calculatePrice;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SubscriptionCheck _subscriptionCheck = subscriptionCheck;

        public async Task ChildCards(string searchText)
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var childs = _unitOfWork.Repository<Child>().Get(
                e => !e.IsDeleted &&
                (e.Name.ToLower().Contains(searchText.ToLower()) ||
                    e.WhatsappNumber.Contains(searchText))
                ).ToList();

            List<WhichChild> children = [];

            if(childs.Count != 0) 
                foreach (var child in childs)
                    children.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved
                    });

            await Clients.Caller.SendAsync("ChildCardsResult", children);
        }

        public async Task DuplicateNames()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(e => !e.IsDeleted).ToList();

            var duplicateChildren = children
                .GroupBy(c => c.Name)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            List<WhichChild> DuplicatedChildren = [];

            if (duplicateChildren.Count != 0)
                foreach (var child in duplicateChildren)
                    DuplicatedChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        NationalId = child.NationalId,
                        WhatsAppNumber = child.WhatsappNumber
                    });

            await Clients.Caller.SendAsync("DuplicateChildCardsResult", DuplicatedChildren);
        }

        public async Task DuplicatePhones()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(e => !e.IsDeleted).ToList();

            var duplicateChildren = children
                .GroupBy(c => c.WhatsappNumber)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            List<WhichChild> DuplicatedChildren = [];

            if (duplicateChildren.Count != 0)
                foreach (var child in duplicateChildren)
                    DuplicatedChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        NationalId = child.NationalId,
                        WhatsAppNumber = child.WhatsappNumber
                    });

            await Clients.Caller.SendAsync("DuplicateChildCardsResult", DuplicatedChildren);
        }

        public async Task DuplicateNationalId()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(e => !e.IsDeleted).ToList();

            var duplicateChildren = children
                .GroupBy(c => c.NationalId)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            List<WhichChild> DuplicatedChildren = [];

            if (duplicateChildren.Count != 0)
                foreach (var child in duplicateChildren)
                    DuplicatedChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        NationalId = child.NationalId,
                        WhatsAppNumber = child.WhatsappNumber
                    });

            await Clients.Caller.SendAsync("DuplicateChildCardsResult", DuplicatedChildren);
        }

        public async Task NotApproved()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(
                e => !e.IsDeleted && !e.IsApproved
                ).ToList();

            List<WhichChild> NotApprovedChildren = [];

            if (children.Count != 0)
                foreach (var child in children)
                    NotApprovedChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        NationalId = child.NationalId,
                        WhatsAppNumber = child.WhatsappNumber
                    });

            await Clients.Caller.SendAsync("ChildCardsResult", NotApprovedChildren);
        }

        public async Task BlackList()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(
                e => !e.IsDeleted && e.BlackList
                ).ToList();

            List<WhichChild> BlackListChildren = [];

            if (children.Count != 0)
                foreach (var child in children)
                    BlackListChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        NationalId = child.NationalId,
                        WhatsAppNumber = child.WhatsappNumber
                    });

            await Clients.Caller.SendAsync("ChildCardsResult", BlackListChildren);
        }

        public async Task DebtsList()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var children = _unitOfWork.Repository<Child>().Get(
                e => !e.IsDeleted && 
                    (
                        e.ChildSubscriptions.Any(cs => cs.HaveDebt) ||
                        e.CheckInOuts.Any(c => c.InPayment == PaymentMethod.دین || 
                                                c.OutPayment == PaymentMethod.دین)
                    ),
                cs => cs.ChildSubscriptions.Where(e => e.HaveDebt),
                cio => cio.CheckInOuts.Where(e => e.InPayment == PaymentMethod.دین || 
                    e.OutPayment == PaymentMethod.دین)
                ).ToList();

            List<WhichChild> DebtsListChildren = [];

            if (children.Count != 0)
                foreach (var child in children)
                    DebtsListChildren.Add(new WhichChild
                    {
                        Id = child.ChildId,
                        ChildImage = child.ChildImageName,
                        Name = child.Name,
                        OnBlackList = child.BlackList,
                        IsApproved = child.IsApproved,
                        TotalDept = child.ChildSubscriptions.Sum(e => e.Remaining) + 
                                    child.CheckInOuts.Where(e => e.InPayment == PaymentMethod.دین).Sum(e => e.InTotal) +
                                    child.CheckInOuts.Where(e => e.OutPayment == PaymentMethod.دین).Sum(e => e.OutTotal) 
                    });

            await Clients.Caller.SendAsync("DebtChildCardsResult", DebtsListChildren);
        }
    }
}
