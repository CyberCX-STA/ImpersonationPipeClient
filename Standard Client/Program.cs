// See https://aka.ms/new-console-template for more information
using Client;
using System.IO.Pipes;
using static System.Formats.Asn1.AsnWriter;

Obnoxious.MOTD();

var pipeClient = Functions.initialise();

Console.WriteLine("\nEncoding:\n1. UTF-16\n2. UTF-8\n3. Raw");
var escape = true;
int encodingType = 0;
while (escape)
{
    Console.Write("\nSelect an option: ");
    var inp = Console.ReadLine();
    switch (inp)
    {
        case "1":
            escape = false;
            break;
        case "2":
            encodingType = 1;
            escape = false;
            break;
        case "3":
            Console.WriteLine("Not Implemented Yet...");
            break;
        default:
            Console.WriteLine("Invalid option. Try again...");
            break;
    }
}

Console.WriteLine("\nAction:\n1. Send\n2. Receive\n3. Send only\n4. Receive only");
while (true)
{
    Console.Write("\nSelect an option: ");
    var inp = Console.ReadLine();
    switch (inp)
    {
        case "1":
            Functions.writePipe(pipeClient, encodingType, true);
            break;
        case "2":
            Functions.readPipe(pipeClient, encodingType, true);
            break;
        case "3":
            Functions.writePipe(pipeClient, encodingType, false);
            break;
        case "4":
            Functions.readPipe(pipeClient, encodingType, false);
            break;
        default:
            Console.WriteLine("Invalid option. Try again...");
            break;
    }
}