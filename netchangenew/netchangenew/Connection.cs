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

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.myPort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        public Connection(StreamReader read, StreamWriter write)
        {
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
                    if (s.StartsWith("Update"))
                    {

                    }
                    else if (s.StartsWith("MyDist"))
                    {
                        string[] ssplit = s.Split(' ');
                        ushort destPort = ushort.Parse(ssplit[2]);
                        Program.n_distanceTable[clientport][destPort] = ushort.Parse(ssplit[1]);
                        Program.Recompute(destPort);
                    }
                    else
                    {
                        Console.WriteLine(Read.ReadLine());
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
    }
}
