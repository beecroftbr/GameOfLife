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
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Hosting;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace GameOfLife.Hubs
{
    public class DrawHub : Hub
    {
        public static int[] lastArray;
        public static string[] lastColorArray;
        public static int _lastTotalTileDepth = 18;
        public static int _timerDelayInMilliseconds = 1000;
        private static bool _serverIsTicking = false;
        private static Guid _tickGuid;
        public static IHubCallerClients _lastClients;

        public DrawHub()
        {
            // Default 1s intervals
            //_serverTick = new Timer(Server_Tick, this, -1, _timerDelayInMilliseconds);
        }
        
        public async Task SendDraw(string livePixels, string colors, int totalTileDepth, int updateGameState, string color = "")
        {

            if (Clients != null) _lastClients = Clients;
            _lastTotalTileDepth = totalTileDepth;
            var json = JsonConvert.DeserializeObject(livePixels);
            var json2 = JsonConvert.DeserializeObject(colors);
            List<int> listArrayOfInts = ((JArray)json).Select(a => (int?) a ?? 0).ToList();
            List<string> listArrayofStrings = ((JArray) json2).Select(a => (string?) a ?? "").ToList();
            for (int i = 0; i < totalTileDepth * totalTileDepth; i++)
            {
                if (i >= listArrayOfInts.Count()) listArrayOfInts.Add(0);
                if (i >= listArrayofStrings.Count()) listArrayofStrings.Add("");
            }
            int[] arrayOfThings = listArrayOfInts.ToArray();
            string[] arrayOfColors = listArrayofStrings.ToArray();
            await FinalizeDraw(arrayOfThings, arrayOfColors, totalTileDepth, updateGameState);

        }

        public async Task FinalizeDraw(int[] arrayOfThings, string[] colors, int totalTileDepth, int updateGameState)
        {
            // Fail fast on data error
            if (arrayOfThings == null || colors == null) return;
            Tuple<int[], string[]> gameRuleResults = applyGameRule(arrayOfThings, colors, totalTileDepth);
            int[] returnableArray = updateGameState == 0 ? arrayOfThings : gameRuleResults.Item1;
            string[] returnableStringArray = updateGameState == 0 ? colors : gameRuleResults.Item2;
            lastArray = returnableArray;
            lastColorArray = returnableStringArray;
            
            await (Clients ?? _lastClients).All.SendAsync("ReceiveDraw", returnableArray, returnableStringArray);
        }

        private Tuple<int[], string[]> applyGameRule(int[] clientArray, string[] colorArray, int totalTileDepth)
        {
            // Any live cell with fewer than two live neighbors dies.
            // Any live cell with two or three live neighbors lives.
            // Any live cell with more than three live neighbors dies.
            // Any dead cell with exactly three live neighbors becomes a live cell.

            int[] newGameArray = new int[clientArray.Length];
            string[] newColorArray = new string[clientArray.Length];
            for (int i = totalTileDepth + 1; i < newGameArray.Length - totalTileDepth - 1; i++)
            {
                // fail fast:  we already know that if i % totalTileDepth = 0 
                // OR if i + 1 % totalTileDepth = 0
                // OR if i < totalTileDepth
                // OR if i > clientArray.Length - totalTileDepth
                // then it is an out of bounds square.
                if ((i + 1) % totalTileDepth == 0
                    || i < totalTileDepth
                    || i % totalTileDepth == 0
                    || i > clientArray.Length - totalTileDepth
                )
                {
                    newGameArray[i] = 0;
                    newColorArray[i] = "";
                    continue;
                }

                var row = Math.Floor((double) i / totalTileDepth);
                var column = i % totalTileDepth;
                List<int> proxemalTiles;
                int[] intTiles = new int[]
                {
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

                string[] colorTiles = new string[]
                {
                    colorArray[i - totalTileDepth - 1], // top left tile
                    colorArray[i - totalTileDepth], // top tile
                    colorArray[i - totalTileDepth + 1], // top right tile
                    colorArray[i - 1], // left tile
                    //clientArray[i], // Tile itself
                    colorArray[i + 1], // right tile
                    colorArray[i + totalTileDepth - 1], // bottom left tile
                    colorArray[i + totalTileDepth], // bottom tile
                    colorArray[i + totalTileDepth + 1], // bottom right tile
                };


                proxemalTiles = intTiles.ToList();

                //loop to go through color array and get average color setting it to new var averageColor
                List<string> listOfValidColors = colorTiles.Where(a => a != String.Empty && a != null).ToList();
                int rVal = 0, gVal = 0, bVal = 0;
                if (listOfValidColors.Any())
                {
                    foreach (var avgColor in listOfValidColors)
                    {
                        var workableColor = avgColor.Replace("#", "");
                        rVal += Convert.ToInt32(workableColor.Substring(0, 2), 16);
                        gVal += Convert.ToInt32(workableColor.Substring(2, 2), 16);
                        bVal += Convert.ToInt32(workableColor.Substring(4, 2), 16);
                    }

                    rVal /= listOfValidColors.Count();
                    gVal /= listOfValidColors.Count();
                    bVal /= listOfValidColors.Count();
                }

                string averageColor = "";
                if (listOfValidColors.Any())
                {
                    averageColor += "#";
                    averageColor += string.Format("{0:x2}", rVal);
                    averageColor += string.Format("{0:x2}", gVal);
                    averageColor += string.Format("{0:x2}", bVal);
                }



            // Is the current cell alive?
                if (clientArray[i] == 1)
                {
                    if (proxemalTiles.Where(a => a == 1).Count() < 2 || proxemalTiles.Where(a => a == 1).Count() > 3) // dies
                    {
                        newGameArray[i] = 0;
                        newColorArray[i] = "";


                    }
                    else
                    {
                        newGameArray[i] = 1;
                        newColorArray[i] = averageColor;
                    }
                }
                // If it's dead, does it have enough living neighbors to become alive?
                else if(clientArray[i] == 0)
                {
                    if(proxemalTiles.Where(a => a == 1).Count() == 3)
                    {
                        newGameArray[i] = 1;
                        newColorArray[i] = averageColor;
                    }
                }
            }
            //if (newGameArray[liveDiePosition] == 0) newGameArray[liveDiePosition] = 1;
            //else newGameArray[liveDiePosition] = 0;
            return new Tuple<int[], string[]>(newGameArray, newColorArray);
        }

        public async Task ClearSavedCanvas()
        {
            if(lastArray == null)
            {
                await Clients.All.SendAsync("ClearCanvas");
                return;
            }
            
            // clear last array
            lastArray = lastArray.Select(a => 0).ToArray();
            lastColorArray = lastColorArray.Select(a => "").ToArray();
            await Clients.All.SendAsync("ClearCanvas");
            
        }

        public void SetTimer(string timerValue)
        {
            int timerInterval = 0;
            try
            {
                int.TryParse(timerValue, out timerInterval);
                if (timerInterval <= 0 || timerInterval >= 10) throw new Exception();
                _timerDelayInMilliseconds = timerInterval * 1000;
            }
            catch (Exception ex)
            {
                return;
            }
            //_serverTick.Change(-1, _timerDelayInMilliseconds);
            TimerWorkerThread._timerDelayInMilliseconds = _timerDelayInMilliseconds;
        }

        public void StartTimer(string livePixels, string colors, int totalTileDepth)
        {
            if (lastArray == null)
            {
                var json = JsonConvert.DeserializeObject(livePixels);
                List<int> listArrayOfInts = ((JArray)json).Select(a => (int?)a ?? 0).ToList();
                for (int i = 0; i < totalTileDepth * totalTileDepth; i++)
                    if (i >= listArrayOfInts.Count()) listArrayOfInts.Add(0);
                lastArray = listArrayOfInts.ToArray();
            }
            if (lastColorArray == null)
            {
                var json = JsonConvert.DeserializeObject(colors);
                List<string> listArrayOfStrings = ((JArray)json).Select(a => (string)a ?? "").ToList();
                for (int i = 0; i < totalTileDepth * totalTileDepth; i++)
                    if (i >= listArrayOfStrings.Count()) listArrayOfStrings.Add("");
                lastColorArray = listArrayOfStrings.ToArray();
            }
            _lastTotalTileDepth = totalTileDepth;
            TimerWorkerThread._timerDelayInMilliseconds = _timerDelayInMilliseconds;
            if (TimerWorkerThread.cts.IsCancellationRequested)
            {
                TimerWorkerThread.cts = new CancellationTokenSource();
            }
            else
            {
                Task.Run(() =>Program.twt.StopAsync(TimerWorkerThread.cts.Token));
                TimerWorkerThread.cts = new CancellationTokenSource();
            }
            Task.Run(() => Program.twt.StartAsync(TimerWorkerThread.cts.Token));

        }

        public async void StopTimer()
        {
            //_serverTick.Change(-1, _timerDelayInMilliseconds);
            TimerWorkerThread.cts.Cancel();
        }
    }
}


//TO do list
//convert one dimensional array to a 2d array (or work with 1D array) 
//create the rules of the game
//create color changing 
//Potentially put game logic in seperate function.
