using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary.Packet;

namespace ProtocolLibrary
{
    public class Protocol
    {
        public static IPacket CreatePacket(SocketType type)
        {
            switch (type)
            {
                case SocketType.Stream:
                    return new StreamPacket();
                case SocketType.Dgram:
                    return null;
                default:
                    return null;
            }
        }
    }
}
