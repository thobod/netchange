using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        static public Dictionary<ushort, Dictionary<ushort, ushort>> n_distanceTable = new Dictionary<ushort, Dictionary<ushort, ushort>>(); //A table that indexes with a neighbour port, whose value is a dictionary which indexes with the destination port, value being estimated distance

        static void Main(string[] args)
        {
            Neighbours = new Dictionary<ushort, Connection>();
            myPort = ushort.Parse(args[0]);
            Console.Title = "NetChange" + myPort;
            new Server(myPort);
            ushort[] portsToConnect = new ushort[args.Length-1];
            for (int i = 1; i < args.Length; i++)
            {
                portsToConnect[i - 1] = ushort.Parse(args[i]);
            }

            //Init the starting routing table
            initTable(portsToConnect);

            //Instantiates the connections
            for (int i = 0; i < portsToConnect.Length; i++)
            {
                if (myPort > portsToConnect[i]) continue;
                CreateConnection(portsToConnect[i]);
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
                else if (input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(' ');
                    ushort port = ushort.Parse(delen[1]);
                    if (!routingTable.ContainsKey(port))
                        Console.WriteLine("Poort " + port + " is niet bekend");
                    else
                    {
                        SendMessage(routingTable[port].Item2, "Message " + port + " " + delen[2]);
                    }

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
                else if (input.StartsWith("N"))
                {
                    PrintNDisTable();
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



        public static void addIncoming(ushort port)
        {
            if(!routingTable.ContainsKey(port))
            routingTable.Add(port, new Tuple<ushort, ushort>(1, port));
        }

        public static void CreateConnection(ushort port)
        {
            if (!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());
            lock (LockObjects[port])
            {
                if (Neighbours.ContainsKey(port)) return;
                if (!n_distanceTable.ContainsKey(port)) n_distanceTable.Add(port, new Dictionary<ushort, ushort>());
                n_distanceTable[port].Add(myPort, 1);
                n_distanceTable[port].Add(port, 0);
                if (!routingTable.ContainsKey(port)) routingTable.Add(port, new Tuple<ushort, ushort>(1, port));
                routingTable[port] = new Tuple<ushort, ushort>(1, port);

                    bool p = true;
                while (p)
                {
                    try
                    {
                        Neighbours.Add(port, new Connection(port));
                        Console.WriteLine("Verbonden: " + port);
                        p = false;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(200);
                    }

                }

                //Update your connected server about your routing table
                foreach (KeyValuePair<ushort, Tuple<ushort, ushort>> distances in routingTable)
                {
                    SendMessage(port, "MyDist " + myPort + " " + distances.Key + " " + distances.Value.Item1);
                }

                foreach (KeyValuePair<ushort, Connection> neighbour in Neighbours)
                {
                    SendMessage(neighbour.Key, "MyDist " + myPort + " " + port + " 1");
                }
            }
        }

        public static void AcceptConnection(ushort port, StreamReader clientIN, StreamWriter clientOUT)
        {
            if (!LockObjects.ContainsKey(port)) LockObjects[port] = new object();
            lock (LockObjects[port])
            {
                if (Neighbours.ContainsKey(port)) return;
                Neighbours.Add(port, new Connection(port, clientIN, clientOUT));
                Console.WriteLine("Client maakt verbinding: " + port);
                if (!routingTable.ContainsKey(port)) routingTable.Add(port, new Tuple<ushort, ushort>(1, port));
                routingTable[port] = new Tuple<ushort, ushort>(1, port);
                if (!n_distanceTable.ContainsKey(port)) n_distanceTable.Add(port, new Dictionary<ushort, ushort>());
                n_distanceTable[port].Add(myPort, 1);
                n_distanceTable[port].Add(port, 0);
                //Update your connected client about your routing table
                foreach (KeyValuePair<ushort, Tuple<ushort, ushort>> distances in routingTable)
                {
                    SendMessage(port, "MyDist " + myPort + " " + distances.Key + " " + distances.Value.Item1);
                }
            }
        }

        public static void AcceptUpdate(ushort clientport, ushort destPort, ushort distance)
        {
            if (!LockObjects.ContainsKey(clientport)) LockObjects[clientport] = new object();
            lock (LockObjects[clientport])
            {
                if (!n_distanceTable.ContainsKey(clientport)) n_distanceTable.Add(clientport, new Dictionary<ushort, ushort>());
                if (!n_distanceTable[clientport].ContainsKey(destPort)) n_distanceTable[clientport].Add(destPort, distance);
                else n_distanceTable[clientport][destPort] = distance;
            }
            Recompute(destPort);
        }

        public static Dictionary<int, int[]> getOtherPortsTable(int port)
        {
            return new Dictionary<int, int[]>(); //zodat er geen errors zijn.
            //not sure how yet 
        }

        public static void Recompute(ushort port)
        {
            if (!LockObjects.ContainsKey(port)) LockObjects[port] = new object();
            //Lock all things to do with current destination
            lock (LockObjects[port])
            {
                if (!routingTable.ContainsKey(port)) routingTable.Add(port, new Tuple<ushort,ushort>(ushort.MaxValue, myPort));
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

                //Informs each neighbour of their updated distance, if the distance changed
                if(d != oldD)
                {
                    foreach(KeyValuePair<ushort, Connection> neighbour in Neighbours)
                    {
                        SendMessage(neighbour.Key, "MyDist " + myPort + " " + port + " " + d);
                    }
                }
            }
        }

        //Returns a tuple of the distance + the neighbour thats on the shortest path
        private static Tuple<ushort,ushort> ShortestPathNeighbour(ushort destinationPort)
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

        private static void AddToNDistanceTable(ushort originPort, ushort destinationPort, ushort distance)
        {

        }

        private static void SendMessage(ushort port, string message)
        {
            Neighbours[port].Write.WriteLine(message);
            Console.WriteLine("stuur berichtje " + message + " naar " + port);
        }

        public static void HandleMessage(string message)
        {
            string[] splitstring = message.Split(' ');
            string[] realMessage = new string[splitstring.Length - 2];
            for (int i = 2; i < splitstring.Length; i++)
            {
                realMessage[i - 2] = splitstring[i];
            }

            ushort targetPort = ushort.Parse(splitstring[1]);

            if (targetPort == myPort) Console.WriteLine(string.Join("",realMessage));
            else
            {
                if (!routingTable.ContainsKey(targetPort)) Console.WriteLine("Poort " + targetPort + " is niet bekend");
                else
                {
                    SendMessage(routingTable[targetPort].Item2, message);
                    Console.WriteLine("Bericht voor " + targetPort + " wordt doorgestuurd naar " + routingTable[targetPort].Item2);
                }
            }
        }


        public static void printTable()
        {
            ushort[] routeKeysArray = routingTable.Keys.ToArray();
            for (int i = 0; i < routingTable.Count; i++)
            {
                Console.Write(routeKeysArray[i] + " " + routingTable[routeKeysArray[i]].Item1 + " ");
                if (routingTable[routeKeysArray[i]].Item2 == myPort)
                    Console.WriteLine("local");
                else if (routingTable[routeKeysArray[i]].Item2 == 0)
                    Console.WriteLine("undef");
                else
                    Console.WriteLine(routingTable[routeKeysArray[i]].Item2);
            }
        }

        public static void PrintNDisTable()
        {
            ushort[] keyArray1 = n_distanceTable.Keys.ToArray();
            for(int i = 0; i < n_distanceTable.Count; i++)
            {
                ushort[] keyArray2 = n_distanceTable[keyArray1[i]].Keys.ToArray();
                for(int j = 0; j < n_distanceTable[keyArray1[i]].Count; j++)
                {
                    Console.WriteLine(keyArray1[i] + " to " + keyArray2[j] + " is " + n_distanceTable[keyArray1[i]][keyArray2[j]]);
                }
            }
        }
    }
}
