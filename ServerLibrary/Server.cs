using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel;
using System.Net.Sockets;
using System.Diagnostics;
using ProtocolLibrary.Packet;
using ProtocolLibrary.Event;

namespace ServerLibrary
{
    public class Server
    {
        public Server(string ip, int port)
        {
            _clients = new List<ClientWorker>();
            ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public bool Start()
        {
            _bwListener = new BackgroundWorker();
            _bwListener.WorkerSupportsCancellation = true;
            _bwListener.WorkerReportsProgress = true;
            _bwListener.DoWork += new DoWorkEventHandler(StartToListen);
            _bwListener.RunWorkerAsync();
            return true;
        }

        private void StartToListen(object sender, DoWorkEventArgs e)
        {
            _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(ServerIPEndPoint);
            _socket.Listen(200);

            while (true)
            {
                try
                {
                    CreateNewClient(_socket.Accept());
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine(ex.Message);
                }                
            }
        }

        public void CreateNewClient(Socket socket)
        {
            ClientWorker newClient = new ClientWorker(socket);
            newClient.PacketReceivedEvent += new PacketReceivedEventHandler(PacketReceived);
            newClient.ClientDisconnectedEvent += new ClientDisconnectedEventHandler(ClientDisconnected);
            RemoveDuplicates(newClient);
            _clients.Add(newClient);
            LogConsole("connected.", newClient.LocalIPEndPoint);
        }

        public void PacketReceived(object sender, PacketEventArgs e)
        {
            switch (e.Packet.Type)
            {
                case PacketType.LoginRequest:
                    break;
                case PacketType.StoreRequest:
                    break;
                default:
                    break;
            }
        }

        public void ClientDisconnected(object sender, ClientEventArgs e)
        {
            if (RemoveClient(e.EndPoint))
            {
                LogConsole("disconnected.", e.EndPoint);
            }
        }

        public bool RemoveClient(IPEndPoint ipEndPoint)
        {
            return RemoveClient(GetClient(ipEndPoint));
        }

        public bool RemoveClient(ClientWorker client)
        {
            lock (this)
            {
                return _clients.Remove(client);
            }
        }

        public ClientWorker GetClient(IPEndPoint ipEndPoint)
        {
            foreach (ClientWorker client in _clients)
            {
                if(client.LocalIPEndPoint.Equals(ipEndPoint))
                {
                    return client;
                }
            }
            return null;
        }

        public void RemoveDuplicates(ClientWorker client)
        {
            if (RemoveClient(client))
            {
                LogConsole("removed mysterious duplicate.", client.LocalIPEndPoint);
            }
        }

        public void LogConsole(string status, IPEndPoint endPoint)
        {
            Console.WriteLine("{0}: Client <{1}:{2}> {3}", DateTime.Now.ToString(),
                endPoint.Address.ToString(), endPoint.Port, status);
        }

        public void Stop()
        {
            if (_clients != null)
            {
                foreach (ClientWorker client in _clients)
                {
                    client.Disconnect();
                }

                _bwListener.CancelAsync();
                _bwListener.Dispose();
                _socket.Close();
                GC.Collect();
            }
        }





        private List<ClientWorker> _clients;

        public IPEndPoint ServerIPEndPoint { get; set; }

        private BackgroundWorker _bwListener { get; set; }

        private Socket _socket;
    }    
}
