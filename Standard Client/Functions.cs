using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace Client
{
    internal class Functions
    {
        public static NamedPipeClientStream initialise()
        {
            Console.WriteLine("Enter the server name ('.' for localhost): ");
            var servername = Console.ReadLine();
            Console.WriteLine("\nEnter the pipe name: ");
            var pipename = Console.ReadLine();
            Console.WriteLine("\nPipe Type:\n1. Message\n2. Byte");
            var escape = true;
            var pipetype = PipeTransmissionMode.Message;
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
                        pipetype = PipeTransmissionMode.Byte;
                        escape = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again...");
                        break;
                }
            }
            Console.WriteLine("\nPipe Direction:\n1. Duplex\n2. In\n3. Out");
            escape = true;
            var pipedir = PipeDirection.InOut;
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
                        pipedir = PipeDirection.In;
                        escape = false;
                        break;
                    case "3":
                        pipedir = PipeDirection.Out;
                        escape = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again...");
                        break;
                }
            }
            Console.WriteLine("\nImpersonation Level:\n1. Impersonation\n2. Anonymous\n3. Delegation\n4. Identification\n5. None");
            escape = true;
            var impersonationLevel = TokenImpersonationLevel.Impersonation;
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
                        impersonationLevel = TokenImpersonationLevel.Anonymous;
                        escape = false;
                        break;
                    case "3":
                        impersonationLevel = TokenImpersonationLevel.Delegation;
                        escape = false;
                        break;
                    case "4":
                        impersonationLevel = TokenImpersonationLevel.Identification;
                        escape = false;
                        break;
                    case "5":
                        impersonationLevel = TokenImpersonationLevel.None;
                        escape = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again...");
                        break;
                }
            }
            Console.WriteLine("\nConnecting to Server...");
            var pipeClient = new NamedPipeClientStream(servername, pipename, pipedir, PipeOptions.None, impersonationLevel);
            pipeClient.Connect(5000);
            if (pipeClient.IsConnected)
            {
                pipeClient.ReadMode = pipetype;
                Console.WriteLine("Connected!");
            }
            else 
            {
                Console.WriteLine("Something went wrong...");
                throw new Exception("Couldn't connect");
            }
            return pipeClient;
        }

        internal static void readPipe(NamedPipeClientStream pipeClient, int encodingType, bool cont)
        {
            if (pipeClient.ReadMode == PipeTransmissionMode.Message)
            {
                var message = ProcessSingleReceivedMessage(pipeClient, encodingType);
                Console.WriteLine("\nReceived: " + message);
            }
            else
            {
                var stream = new StreamString(pipeClient);
                Console.WriteLine("\nReceived: " + stream.ReadString());
            }
            if (cont)
            {
                writePipe(pipeClient, encodingType, true);
            }
        }

        internal static void writePipe(NamedPipeClientStream pipeClient, int encodingType, bool cont)
        {
            while (true)
            {
                Console.Write("\nSending: ");
                var message = Console.ReadLine();
                if (pipeClient.ReadMode == PipeTransmissionMode.Message)
                {
                    if (encodingType == 0)
                    {
                        pipeClient.Write(Encoding.Unicode.GetBytes(message));
                    }
                    else if (encodingType == 1)
                    {
                        pipeClient.Write(Encoding.UTF8.GetBytes(message));
                    }
                    Console.WriteLine("Sent!");
                }
                else
                {
                    var stream = new StreamString(pipeClient);
                    stream.WriteString(message);
                    Console.WriteLine("Sent!");
                }
                if (cont)
                {
                    readPipe(pipeClient, encodingType, true);
                }
            }
        }

        private static string ProcessSingleReceivedMessage(NamedPipeClientStream namedPipeClient, int encodingType)
        {
            StringBuilder messageBuilder = new StringBuilder();
            string messageChunk = string.Empty;
            byte[] messageBuffer = new byte[5];
            do
            {
                namedPipeClient.Read(messageBuffer, 0, messageBuffer.Length);
                if (encodingType == 0)
                {
                    messageChunk = Encoding.Unicode.GetString(messageBuffer);
                }
                else
                {
                    messageChunk = Encoding.UTF8.GetString(messageBuffer);
                }
                messageBuilder.Append(messageChunk);
                messageBuffer = new byte[messageBuffer.Length];
            }
            while (!namedPipeClient.IsMessageComplete);
            return messageBuilder.ToString();
        }
    }
    
    //Copied from Microsoft example documentation:
    //https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}