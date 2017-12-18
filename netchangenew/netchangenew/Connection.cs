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

        public Connection(int port)
        {
            TcpClient client = new TcpClient("localhost", port);

        }
    }
}
