"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var readline = require("readline");
var deasync = require("deasync");
var Function;
(function (Function) {
    Function[Function["Write"] = 0] = "Write";
    Function[Function["Compile"] = 1] = "Compile";
})(Function || (Function = {}));
function readSuccess() {
    var readlineInterface = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
        terminal: false,
    });
    var line = "";
    var isDone = false;
    readlineInterface.once("line", function (input) {
        line = input;
        isDone = true;
        readlineInterface.close();
    });
    while (!isDone) {
        deasync.runLoopOnce();
    }
    return line == "True";
}
function sendPacket(packet) {
    var json = JSON.stringify(packet);
    console.log(json);
    return readSuccess();
}
function write(content) {
    return sendPacket({
        function: Function.Write,
        parameters: [content],
    });
}
function compile(context) {
    return sendPacket({
        function: Function.Compile,
        parameters: [context],
    });
}
exports.default = { write: write, compile: compile };
