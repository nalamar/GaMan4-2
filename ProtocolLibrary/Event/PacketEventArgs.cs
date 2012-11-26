using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary.Packet;

namespace ProtocolLibrary.Event
{
    public class PacketEventArgs : EventArgs
    {
        public PacketEventArgs(IPacket packet)
        {
            Packet = packet;
        }

        public IPacket Packet { get; private set; }
    }

    public delegate void PacketReceivedEventHandler(object sender, PacketEventArgs e);
}
