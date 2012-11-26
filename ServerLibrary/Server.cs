using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary
{
    class Server
    {
        public Server(string ip, int port)
        {
            _client = new List<ClientWorker>();
        }

        private List<ClientWorker> _client;
    }    
}
