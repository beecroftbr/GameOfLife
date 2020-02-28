using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Json.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace GameOfLife.Hubs
{
    public class DrawHub : Hub
    {
        
        
        public async Task SendDraw(string livePixels)
        {
            var json = JsonConvert.DeserializeObject(livePixels);
            int?[] jsonTiles = ((JArray) json).Select(a => (int?) a).ToArray();
            int[] actualizedArray = new int[jsonTiles.Length];
            for(int i = 0; i < jsonTiles.Length; i++)
            {
                actualizedArray[i] = jsonTiles[i] ?? 0;
            }
            await Clients.All.SendAsync("ReceiveDraw", json);
        }
    }
}
