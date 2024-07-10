using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Microfichas_App.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task SendProgress(int progress)
        {
            await Clients.All.SendAsync("ReceiveProgress", progress);
        }
    }
}
