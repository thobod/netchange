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

        public Connection(ushort port)
        {
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        public Connection(ushort port, StreamReader read, StreamWriter write)
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
                    if (s.StartsWith("MyDist"))
                    {
                        //Console.WriteLine("updating table with " + s);
                        string[] ssplit = s.Split(' ');
                        Program.AcceptUpdate(ushort.Parse(ssplit[1]), ushort.Parse(ssplit[2]), ushort.Parse(ssplit[3]));
                    }
                    else if (s.StartsWith("Message"))
                    {
                        //Console.WriteLine("Handel inkomend bericht af");
                        Program.HandleMessage(s);
                        //Console.WriteLine("ik ben nu hier");
                    }
                    else
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch(Exception e)
            {
                //Console.WriteLine("Woah shit broke");
                //Console.WriteLine(e);
            }
        }
    }
}
