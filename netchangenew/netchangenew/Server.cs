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
        }
    }
}
