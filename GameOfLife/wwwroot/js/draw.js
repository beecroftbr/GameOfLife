"use strict";

//var connection = new signalR.HubConnectionBuilder().withUrl("http://students.cs.weber.edu/ThreeMenWithBeards/drawHub").build(); // Server setting
var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build(); // Localhost setting

// Disable the canvas until the page is loaded.
//document.getElementById("lifeCanvas").disabled = true;

var numberOfTiles = 16;
var totalTileDepth=numberOfTiles+2;
var tiles = [];
var canvasElement = document.getElementById("lifeCanvas");
var tileHeight = canvasElement.height / numberOfTiles;
var tileWidth = canvasElement.width / numberOfTiles;
var ctx = canvasElement.getContext('2d');

connection.on("ReceiveDraw", function (livePixels) {
    //var chatData = document.getElementById("hiddenData");
    //chatData.innerHTML = livePixels;
    
    //redraw the board upon connection of user selection
    for(let i = 0; i < totalTileDepth^2; i++) {
        var row = totalTileDepth^2 / numberOfTiles;
        var column = totalTileDepth^2 % numberOfTiles;
        if (row == 0 || row > numberOfTiles || col == 0 || col > numberOfTiles) continue;
        
        ctx.fillRect(row, column, tileHeight, tileWidth);
    }
    
});

connection.start().then(function () {
    document.getElementById("lifeCanvas").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

String.prototype.replaceAt = function (index, replacement) {
    return this.substr(0, index) + replacement + this.substr(index + replacement.length);
}
function getMouseY(canvas, event) {
    let rect = canvas.getBoundingClientRect();
    let y = event.clientY - rect.top;
    return y;
} 
function getMouseX(canvas, event) {
    let rect = canvas.getBoundingClientRect();
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
    tiles[(trueXPixel + ((trueYPixel) * (numberOfTiles + 2)))] = "1";
    ctx.fillStyle='red';
    ctx.fillRect(0,0,canvasElement.width,canvasElement.height);
    
    connection.invoke("SendDraw", JSON.stringify(tiles)).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//to do
//we need to send the width and lenght to the server
//once we recieve the data back fill the tiles that have 1s
//draw the grid above the filled squares