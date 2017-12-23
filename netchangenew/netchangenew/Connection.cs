using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace netchangenew
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
        private ushort clientport;

        public Connection(ushort port)
        {
            TcpClient client = new TcpClient("localhost", port);
            clientport = port;
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        public Connection(ushort port, StreamReader read, StreamWriter write)
        {
            clientport = port;
            Read = read; Write = write;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string s = Read.ReadLine();
                    if (s.StartsWith("MyDist"))
                    {
                        string[] ssplit = s.Split(' ');
                        Program.AcceptUpdate(clientport, ushort.Parse(ssplit[1]), ushort.Parse(ssplit[2]));
                    }
                    else
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Woah shit broke");
            }
        }
    }
}
