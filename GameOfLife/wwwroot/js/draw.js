"use strict";

//var connection = new signalR.HubConnectionBuilder().withUrl("http://students.cs.weber.edu/ThreeMenWithBeards/drawHub").build(); // Server setting
var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build(); // Localhost setting

// Disable the canvas until the page is loaded.
//document.getElementById("lifeCanvas").disabled = true;

connection.on("ReceiveDraw", function (user, livePixels) {
    var chatData = document.getElementById("hiddenData");
    chatData.innerHTML = livePixels;
    
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
    var userData = document.getElementById("hiddenData").innerHTML;
    var canvasElement = document.getElementById("lifeCanvas");
    var canvasX = getMouseX(canvasElement, event);
    var canvasY = getMouseY(canvasElement, event);
    var trueXPixel = Math.ceil(canvasX / 10);
    var trueYPixel = Math.ceil(canvasY / 10);
    userData = userData.replaceAt(trueXPixel + ((trueYPixel) * 18), "1");
    document.getElementById("hiddenData").innerHTML = userData;

    connection.invoke("SendDraw", user, userData).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});