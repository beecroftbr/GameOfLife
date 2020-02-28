using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GameOfLife.Hubs
{
    public class DrawHub : Hub
    {
        public async Task SendDraw(string user, string livePixels)
        {
            await Clients.All.SendAsync("ReceiveDraw", user, livePixels);
        }
    }
}
