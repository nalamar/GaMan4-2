using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerLibrary;

namespace GaMan4Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("::1", 4850);

            if (server.Start())
            {
                Console.WriteLine("Listening on port {0}. Press Enter to shutdown server.", server.ServerIPEndPoint.Port);
                Console.ReadLine();
                server.Stop();
            }
            else
            {
                Console.ReadLine();
            }
        }
    }
}
