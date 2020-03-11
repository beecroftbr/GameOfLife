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
            int[] arrayOfThings = ((JArray)json).Select(a => (int?) a ?? 0).ToArray();
            
            await Clients.All.SendAsync("ReceiveDraw", arrayOfThings);
        }
    }
}


//TO do list
//convert one dimensional array to a 2d array (or work with 1D array) 
//create the rules of the game
//create color changing 
//Potentially put game logic in seperate function.
