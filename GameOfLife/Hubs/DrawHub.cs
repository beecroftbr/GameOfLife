using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Json.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System;

namespace GameOfLife.Hubs
{
    public class DrawHub : Hub
    {
        
        
        public async Task SendDraw(string livePixels, int totalTileDepth, int updateGameState)
        {
            var json = JsonConvert.DeserializeObject(livePixels);
            List<int> listArrayOfInts = ((JArray)json).Select(a => (int?) a ?? 0).ToList();
            for(int i = 0; i < totalTileDepth * totalTileDepth; i++)
                if (i >= listArrayOfInts.Count()) listArrayOfInts.Add(0);
            int[] arrayOfThings = listArrayOfInts.ToArray();

            int[] returnableArray = updateGameState == 0 ? arrayOfThings : applyGameRule(arrayOfThings, totalTileDepth);

            await Clients.All.SendAsync("ReceiveDraw", returnableArray);
        }

        private int[] applyGameRule(int[] clientArray, int totalTileDepth)
        {
            // Any live cell with fewer than two live neighbors dies.
            // Any live cell with two or three live neighbors lives.
            // Any live cell with more than three live neighbors dies.
            // Any dead cell with exactly three live neighbors becomes a live cell.

            int[] newGameArray = new int[clientArray.Length];
            for (int i = totalTileDepth + 1; i < newGameArray.Length - totalTileDepth - 1; i++)
            {
                // fail fast:  we already know that if i % totalTileDepth = 0 
                // OR if i + 1 / totalTileDepth = 0
                // OR if i < totalTileDepth
                // OR if i > clientArray.Length - totalTileDepth
                // then it is an out of bounds square.
                if ((i + 1) / totalTileDepth == 0
                    || i < totalTileDepth
                    || i % totalTileDepth == 0
                    || i > clientArray.Length - totalTileDepth
                    )
                {
                    newGameArray[i] = 0;
                    continue;
                }

                var row = Math.Floor((double)i / totalTileDepth);
                var column = i % totalTileDepth;
                List<int> proxemalTiles;
                int[] intTiles = new int[] {
                    clientArray[i - totalTileDepth - 1], // top left tile
                    clientArray[i - totalTileDepth], // top tile
                    clientArray[i - totalTileDepth + 1], // top right tile
                    clientArray[i - 1], // left tile
                    //clientArray[i], // Tile itself
                    clientArray[i + 1], // right tile
                    clientArray[i + totalTileDepth - 1], // bottom left tile
                    clientArray[i + totalTileDepth], // bottom tile
                    clientArray[i + totalTileDepth + 1], // bottom right tile
                };
                proxemalTiles = intTiles.ToList();
                // Is the current cell alive?
                if (clientArray[i] == 1)
                {
                    if (proxemalTiles.Where(a => a == 1).Count() < 2 || proxemalTiles.Where(a => a == 1).Count() > 3) // dies
                    {
                        newGameArray[i] = 0;

                    }
                    else
                    {
                        newGameArray[i] = 1;
                    }
                }
                // If it's dead, does it have enough living neighbors to become alive?
                else if(clientArray[i] == 0)
                {
                    if(proxemalTiles.Where(a => a == 1).Count() == 3)
                    {
                        newGameArray[i] = 1;
                    }
                }
            }
            //if (newGameArray[liveDiePosition] == 0) newGameArray[liveDiePosition] = 1;
            //else newGameArray[liveDiePosition] = 0;
            return newGameArray;
        }
    }
}


//TO do list
//convert one dimensional array to a 2d array (or work with 1D array) 
//create the rules of the game
//create color changing 
//Potentially put game logic in seperate function.
