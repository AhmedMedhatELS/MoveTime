using DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Models.ViewModels;
using Models;
using Shared;

namespace MoveTime.Hubs
{
    public class DocHub(IServiceProvider serviceProvider) : Hub
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public readonly string LinkMainPart = "/files/documents/";
        public readonly string DefaultLink = "javascript:void(0)";

        public async Task GetDocLink()
        {
            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            string link = DefaultLink;

            var docs = _unitOfWork.Repository<Document>().Get(null).ToList();

            if(docs.Count == 1)
                link = LinkMainPart + docs[0].DocumentName;            

            await Clients.Caller.SendAsync("DocLink", link);
        }
    }
}
