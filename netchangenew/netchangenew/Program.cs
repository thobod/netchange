using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netchangenew
{
    class Program
    {
        static public int myPort;
        static public Dictionary<int, Connection> Neighbours;
        static readonly public Dictionary<int, object> LockObjects;

        static void Main(string[] args)
        {
            myPort = int.Parse(args[0]);
            Console.Title = "NetChange" + myPort;
            new Server(myPort);
            for(int i = 1; i < args.Length; i++)
            {
                int port = int.Parse(args[i]);
                Neighbours.Add(port, new Connection(port));
                LockObjects.Add(port, new object());
            }
        }
    }
}
