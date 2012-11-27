using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolLibrary.Event
{
    /// <summary>
    /// A client event.
    /// </summary>
    public class ClientEventArgs : EventArgs
    {
        /// <summary>
        /// A client event.
        /// </summary>
        /// <param name="serverSocket">The server socket</param>
        public ClientEventArgs(Socket serverSocket)
        {
            EndPoint = (IPEndPoint)serverSocket.RemoteEndPoint;
        }

        /// <summary>
        /// The endpoint of the client.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }
    }

    /// <summary>
    /// Called on disconnection from the client.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ClientDisconnectedEventHandler(object sender, ClientEventArgs e);
}