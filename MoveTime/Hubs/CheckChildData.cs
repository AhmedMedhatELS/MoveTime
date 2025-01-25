using DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Models.ViewModels;
using Models;
using Shared;

namespace MoveTime.Hubs
{
    public class CheckChildData(
        IServiceProvider serviceProvider
        ) : Hub
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task ISDuplicated(string name, string whatsAppNumber, string nationalId)
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var nameExsits = _unitOfWork.Repository<Child>().Get(
                e => e.Name == name
                ).ToList();

            var whatsAppNumberExsits = _unitOfWork.Repository<Child>().Get(
               e => e.WhatsappNumber == whatsAppNumber
               ).ToList();

            var nationalIdExsits = _unitOfWork.Repository<Child>().Get(
               e => e.NationalId == nationalId
               ).ToList();


            await Clients.Caller.SendAsync("ISDuplicatedResult",
                nameExsits.Count > 0,
                whatsAppNumberExsits.Count > 0,
                nationalIdExsits.Count > 0
                );
        }
    }
}
