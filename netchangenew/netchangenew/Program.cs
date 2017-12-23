using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netchangenew
{
    class Program
    {
        static public ushort myPort;
        static public Dictionary<ushort, Connection> Neighbours;
        static readonly public Dictionary<int, object> LockObjects = new Dictionary<int, object>(); //deze moet nog aangemaakt worden maar is readonly
        static public Dictionary<ushort, Tuple<ushort, ushort>> routingTable; //Routing table, key being the destination port, value being a tuple of <distance, Neighbour closest to destination port>
        static public Dictionary<ushort, Dictionary<ushort, ushort>> n_distanceTable; //A table that indexes with a neighbour port, whose value is a dictionary which indexes with the destination port, value being estimated distance

        static void Main(string[] args)
        {
            Neighbours = new Dictionary<ushort, Connection>();
            args = Console.ReadLine().Split(' ');
            myPort = ushort.Parse(args[0]);
            Console.Title = "NetChange" + myPort;
            new Server(myPort);
            ushort[] portsToConnect = new ushort[args.Length-1];
            Array.Copy(args, portsToConnect, 1);

            //Init the starting routing table
            initTable(portsToConnect);

            //Instantiates the connections
            for (int i = 1; i < args.Length; i++)
            {
                ushort port = ushort.Parse(args[i]);
                CreateConnection(port);
            }


            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("C"))
                {
                    ushort port = ushort.Parse(input.Split(' ')[1]);
                    if (Neighbours.ContainsKey(port))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        CreateConnection(port);
                    }
                }
                else if(input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(' ');
                    ushort port = ushort.Parse(delen[1]);
                    if (!Neighbours.ContainsKey(port))
                        Console.WriteLine("Poort " + port + " is niet bekend");
                    else
                        SendMessage(port, myPort + ": " + delen[2]);
                }
                else if (input.StartsWith("D"))
                {
                    //Verwijder port
                    ushort port = ushort.Parse(input.Split()[1]);
                    if (!Neighbours.ContainsKey(port))
                        Console.WriteLine("Poort " + port + " is niet bekend");
                    else
                    {
                        Neighbours.Remove(port);
                    }
                }
                else if (input.StartsWith("R"))
                {
                    printTable();
                }
            }
        }
        public static void initTable(ushort[] ports) //sets all the direct connections as fastest connection in routingtable.
        {
            routingTable = new Dictionary<ushort, Tuple<ushort, ushort>>();
            routingTable.Add(myPort, new Tuple<ushort, ushort>(0, myPort) );
            for (int i = 0; i < ports.Length; i++)
            {
                //Add port to routing table
                routingTable.Add(ports[i], new Tuple<ushort,ushort>(1, ports[i]));
            }
        }

        public static void CreateConnection(ushort port)
        {
            foreach(KeyValuePair<ushort, Connection> neighbour in Neighbours)
            {
                SendMessage(neighbour.Key, "MyDist " + port + " 1");
            }
            if (!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());

            bool p = true;
            while(p)
            {
                try
                {
                    Neighbours.Add(port, new Connection(port));
                    p = false;
                }
                catch
                {
                    System.Threading.Thread.Sleep(200);
                }
            }


            foreach(KeyValuePair<ushort, Tuple<ushort, ushort>> distances in routingTable)
            {
                SendMessage(port, "MyDist " + distances.Key + " " + distances.Value.Item1);
            }
        }

        public static Dictionary<int, int[]> getOtherPortsTable(int port)
        {
            return new Dictionary<int, int[]>(); //zodat er geen errors zijn.
            //not sure how yet 
        }

        public void Recompute(ushort port)
        {
            //Lock all things to do with current destination
            lock (LockObjects[port])
            {
                ushort oldD = routingTable[port].Item1;
                ushort d;
                //If port is current port, do nothing.
                if (port == myPort)
                {
                    return;
                }
                //Else compute shortest hop
                else
                {
                    Tuple<ushort, ushort> closest = ShortestPathNeighbour(port);
                    d = (ushort)(1 + closest.Item1);
                    //Checks if d exceeds node count, which would mean the node is porbably inaccessible
                    if (d < 20)
                    {
                        routingTable[port] = new Tuple<ushort, ushort>(d, closest.Item2);
                    }
                    else
                    {
                        //Node is inaccesible: make distance ushort.maxvalue, destination port 0 (which doesn't exist)
                        routingTable[port] = new Tuple<ushort, ushort>(ushort.MaxValue, 0);
                        Console.WriteLine("Onbereikbaar: " + port);
                    }
                }
                if(d != oldD)
                {
                    foreach(KeyValuePair<ushort, Connection> neighbour in Neighbours)
                    {
                        SendMessage(neighbour.Key, "MyDist " + d + " " + port);
                    }
                }
            }
        }

        //Returns a tuple of the distance + the neighbour thats on the shortest path
        private Tuple<ushort,ushort> ShortestPathNeighbour(ushort destinationPort)
        {
            ushort dist = ushort.MaxValue;
            ushort port = 0;
            foreach(KeyValuePair<ushort, Connection> neighbour in Neighbours)
            {
                ushort tempDist = n_distanceTable[neighbour.Key][destinationPort];
                if (tempDist < dist)
                {
                    dist = tempDist;
                    port = neighbour.Key;
                } 
            }
            return new Tuple<ushort, ushort>(dist, port);
        }

        private static void SendMessage(ushort port, string message)
        {
            Neighbours[port].Write.WriteLine(message);
        }

        public static void printTable()
        {
            ushort[] routeKeysArray = routingTable.Keys.ToArray();
            for (int i = 0; i < routingTable.Count; i++)
            {
                Console.Write(routeKeysArray[i] + " " + routingTable[routeKeysArray[i]].Item1 + " ");
                if (routingTable[routeKeysArray[i]].Item2 == myPort)
                    Console.WriteLine("local");
                else
                    Console.WriteLine(routingTable[routeKeysArray[i]].Item2);
            }
        }
    }
}
