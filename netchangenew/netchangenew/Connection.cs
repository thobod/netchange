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
                    else if (s.StartsWith("Forward"))
                    {
                        string[] splitstring = s.Split(' ');
                        string[] message = new string[splitstring.Length - 2];
                        for (int i = 2; i < splitstring.Length; i++)
                        {
                            message[i - 2] = splitstring[i];
                        }

                        Program.forwardMessage(ushort.Parse(splitstring[1]), string.Join("",message));
                        //Console.WriteLine("ik ben nu hier");
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
