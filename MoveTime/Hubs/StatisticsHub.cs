using DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.ViewModels.Statistics;
using Shared;

namespace MoveTime.Hubs
{
    public class StatisticsHub(Statistics statistics,
        IServiceProvider serviceProvider) : Hub
    {
        private readonly Statistics _statistics = statistics;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task LogHistorySearch(string FromString, string ToString)
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            StatisticsTableView? view = new();

            if (!string.IsNullOrEmpty(ToString) && !string.IsNullOrEmpty(FromString))
            {
                DateTime From = DateTime.Parse(FromString);
                DateTime To = DateTime.Parse(ToString);

                if(From <= To)                
                    view = _statistics.LogHistory(From, To);                
            }

            await Clients.Caller.SendAsync("LogHistorySearchResult",view);
        }
    }
}
