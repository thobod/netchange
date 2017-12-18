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
                if(!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());
            }

            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("C"))
                {
                    int port = int.Parse(input.Split()[1]);
                    if (Neighbours.ContainsKey(port))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Neighbours.Add(port, new Connection(port));
                        if (!LockObjects.ContainsKey(port)) LockObjects.Add(port, new object());
                    }
                }
                else if(input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int port = int.Parse(delen[0]);
                    if (!Neighbours.ContainsKey(port))
                        Console.WriteLine("Poort " + port + " is niet bekend");
                    else
                        Neighbours[port].Write.WriteLine(myPort + ": " + delen[1]);
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

                }
            }
        }
    }
}
