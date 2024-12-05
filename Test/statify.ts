import * as readline from 'readline';
import * as deasync from 'deasync'

enum Function {
    Write, Compile
}

interface Packet {
    function: Function;
    parameters: any[];
}

function readSuccess(): boolean {
    const readlineInterface = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
        terminal: false
    });

    let line = "";
    let isDone = false;

    readlineInterface.once("line", (input) => {
        line = input;
        isDone = true;
        readlineInterface.close();
    });

    while (!isDone) {
        deasync.runLoopOnce();
    }

    return line == "True";
}

function sendPacket(packet: Packet): boolean
{
    let json: string = JSON.stringify(packet);
    console.log(json);

    return readSuccess();
}


function write(content: string): boolean
{
    return sendPacket({
        function: Function.Write,
        parameters: [content]
    });
}

function compile(context: { [index: string]: any })
{
    return sendPacket({
        function: Function.Compile,
        parameters: [context]
    })
}

export default { write, compile }