using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtocolLibrary;
using ProtocolLibrary.Event;
using ProtocolLibrary.Message;
using ProtocolLibrary.Packet;


namespace ServerLibrary
{
    public class ClientWorker
    {
        public ClientWorker(Socket clientSocket)
        {
            _socket = clientSocket;
            LocalIPEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            _bwReceiver = new BackgroundWorker();
            _bwReceiver.DoWork += new DoWorkEventHandler(ReceivePacketFromClient);
            _bwReceiver.RunWorkerAsync();
        }

        private void ReceivePacketFromClient(object sender, DoWorkEventArgs e)
        {
            bool blockingState = _socket.Blocking;

            try
            {
                while (true)
                {
                    IPacket packet = Protocol.CreatePacket(SocketType.Stream);
                    
                }

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ClientInformation ClientInfo { get; set; }

        public IPEndPoint LocalIPEndPoint { get; set; }

        private BackgroundWorker _bwReceiver;

        private Socket _socket;

        private static Mutex _mutex = new Mutex();

        public event PacketReceivedEventHandler PacketReceivedEvent;

    }
}
