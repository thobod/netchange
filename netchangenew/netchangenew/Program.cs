﻿using System;
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
        static public Dictionary<int,int[]> routingTable;

        static void Main(string[] args)
        {
            Neighbours = new Dictionary<int, Connection>();
            args = Console.ReadLine().Split(' ');
            myPort = int.Parse(args[0]);
            Console.Title = "NetChange" + myPort;
            new Server(myPort);
            int[] portsToConnect = new int[args.Length-1];
            for(int i = 1; i < args.Length; i++)
            {
                portsToConnect[i - 1] = int.Parse(args[i]); 
                int port = int.Parse(args[i]);
                Neighbours.Add(port, new Connection(port));
                //if(!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());
            }
            initTable(portsToConnect);
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("C"))
                {
                    int port = int.Parse(input.Split(' ')[1]);
                    if (Neighbours.ContainsKey(port))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Neighbours.Add(port, new Connection(port));
                        //if (!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());
                    }
                }
                else if(input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(' ');
                    int port = int.Parse(delen[1]);
                    if (!Neighbours.ContainsKey(port))
                        Console.WriteLine("Poort " + port + " is niet bekend");
                    else
                        Neighbours[port].Write.WriteLine(myPort + ": " + delen[2]);
                }
                else if (input.StartsWith("D"))
                {
                    //Verwijder port
                    int port = int.Parse(input.Split()[1]);
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
        public static void initTable(int[] ports) //sets all the direct connections as fastest connection in routingtable.
        {
            routingTable = new Dictionary<int, int[]>();
            for (int i = 0; i < ports.Length; i++)
            {
                int[] x = new int[2];
                x[0] = 1; //distance to next node
                x[1] = ports[i]; //first step to next node (also actually next node)
                routingTable.Add(ports[i], x);
            }
        }
        public static void addToTable(int port, int costToGetToPort, Dictionary<int, int[]> otherPortsTable)//gets the routingtable of another port, and adds these to the routingtable of this node(if they are faster than excisting nodes, or a route to this node doesnt yet exist)
        {
            int[] otherPorts = otherPortsTable.Keys.ToArray();

            for (int i = 0; i < otherPorts.Length; i++)
            {
                int[] tuple = otherPortsTable[otherPorts[i]];
                if (!routingTable.ContainsKey(otherPorts[i]) || tuple[0] >= costToGetToPort + 1) //if its not in the table, or the known path is longer than the one we just found.
                {
                    int[] x = new int[2];
                    x[0] = costToGetToPort + 1; //not really sure if this is right ???
                    x[1] = port;
                    routingTable.Add(otherPorts[i], x);
                } 
            }
        }
        public static void printTable()
        {

            int[,] routearray = new int[routingTable.Count,3];
            int[] routeKeysArray = routingTable.Keys.ToArray();
            int[][] routeValuesArray = routingTable.Values.ToArray();
            for (int i = 0; i < routingTable.Count; i++)
            {
                routearray[i, 0] = routeKeysArray[i];
                routearray[i, 1] = routeValuesArray[i][0];
                routearray[i, 2] = routeValuesArray[i][1];
            }
            for (int i = 0; i < routingTable.Count; i++)
            {
                Console.Write(routearray[i,0] + " ");
                Console.Write(routearray[i, 1] + " ");
                Console.WriteLine(routearray[i, 2]);
            }
        }
    }
}
