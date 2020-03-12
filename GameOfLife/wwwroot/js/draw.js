"use strict";

//var connection = new signalR.HubConnectionBuilder().withUrl("http://students.cs.weber.edu/ThreeMenWithBeards/drawHub").build(); // Server setting
var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build(); // Localhost setting

// Disable the canvas until the page is loaded.
//document.getElementById("lifeCanvas").disabled = true;

var numberOfTiles = 16;
var totalTileDepth=numberOfTiles+2;
var tiles = [];
tiles[136] = 1;
tiles[153] = 1;
tiles[171] = 1;
tiles[172] = 1;
var canvasElement = document.getElementById("lifeCanvas");
var tileHeight = canvasElement.height / numberOfTiles;
var tileWidth = canvasElement.width / numberOfTiles;
var ctx = canvasElement.getContext('2d');
for (let i = totalTileDepth; i < totalTileDepth * totalTileDepth; i++) {
    var row = Math.floor(i / totalTileDepth);
    var column = i % totalTileDepth;
    if (row == 0 || row > numberOfTiles || column == 0 || column > numberOfTiles)
        continue;

    if (tiles[i] == 1)
        ctx.fillRect((column - 1) * tileWidth, (row - 1) * tileHeight, tileHeight, tileWidth);
}

connection.on("ReceiveDraw", function (livePixels) {
    //var chatData = document.getElementById("hiddenData");
    //chatData.innerHTML = livePixels;
    tiles = livePixels;
    //redraw the board upon connection of user selection
    ctx.clearRect(0, 0, canvasElement.width, canvasElement.height);
    for (let i = totalTileDepth; i < totalTileDepth * totalTileDepth; i++) {
        var row = Math.floor(i / totalTileDepth);
        var column = i % totalTileDepth;
        if (row == 0 || row > numberOfTiles || column == 0 || column > numberOfTiles)
            continue;

        if (livePixels[i] == 1)
            ctx.fillRect((column - 1) * tileWidth, (row - 1) * tileHeight, tileHeight, tileWidth);
    }
    
});

connection.start().then(function () {
    document.getElementById("lifeCanvas").disabled = false;
    // TODO:  retrieve latest canvas data from server.

}).catch(function (err) {
    return console.error(err.toString());
});

String.prototype.replaceAt = function (index, replacement) {
    return this.substr(0, index) + replacement + this.substr(index + replacement.length);
}
function getMouseY(canvasElement, event) {
    let rect = canvasElement.getBoundingClientRect();
    let y = event.clientY - rect.top;
    return y;
} 
function getMouseX(canvasElement, event) {
    let rect = canvasElement.getBoundingClientRect();
    let x = event.clientX - rect.left;
    return x;
}


document.getElementById("lifeCanvas").addEventListener("click", function (event) {
    //var userData = document.getElementById("hiddenData").innerHTML;
    var canvasX = getMouseX(canvasElement, event);
    var canvasY = getMouseY(canvasElement, event);
    var trueXPixel = Math.ceil(canvasX / tileWidth);
    var trueYPixel = Math.ceil(canvasY / tileHeight);
    //userData = userData.replaceAt(trueXPixel + ((trueYPixel) * (numberOfTiles + 2)), "1");
    //document.getElementById("hiddenData").innerHTML = userData;
    if (tiles[(trueXPixel + ((trueYPixel) * (numberOfTiles + 2)))] == "1")
        tiles[(trueXPixel + ((trueYPixel) * (numberOfTiles + 2)))] = "0";
    else
        tiles[(trueXPixel + ((trueYPixel) * (numberOfTiles + 2)))] = "1";
    //ctx.fillStyle='red';
    //ctx.fillRect(0,0,canvasElement.width,canvasElement.height);
    
    connection.invoke("SendDraw", JSON.stringify(tiles), totalTileDepth, 0).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


document.getElementById("nextButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendDraw", JSON.stringify(tiles), totalTileDepth, 1).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


//to do
//we need to send the width and lenght to the server
//once we recieve the data back fill the tiles that have 1s
//draw the grid above the filled squares