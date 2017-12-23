using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace netchangenew
{
    class Server
    {
        public Server(int port)
        {
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            new Thread(() => AcceptLoop(server)).Start();
        }

        private void AcceptLoop(TcpListener handle)
        {
            while (true)
            {
                TcpClient client = handle.AcceptTcpClient();
                StreamReader clientIn = new StreamReader(client.GetStream());
                StreamWriter clientOut = new StreamWriter(client.GetStream());
                clientOut.AutoFlush = true;

                // De server weet niet wat de poort is van de client die verbinding maakt, de client geeft dus als onderdeel van het protocol als eerst een bericht met zijn poort
                ushort zijnPoort = ushort.Parse(clientIn.ReadLine().Split()[1]);

               // Console.WriteLine("Client maakt verbinding: " + zijnPoort);

                // Zet de nieuwe verbinding in de verbindingslijst
                // Zet de nieuwe verbinding in de verbindingslijst
                if (!Program.Neighbours.ContainsKey(zijnPoort))
                {
                    Program.addIncoming(zijnPoort);
                    Program.AcceptConnection(zijnPoort, clientIn, clientOut);
                }

            }
        }
    }
}
