using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolLibrary.Packet
{
    public interface IPacket
    {
        byte[] Serialize();

        void Deserialize(Stream stream);

        byte[] Data { get; set; }

        bool Compressed { get; set; }

        bool Encrypted { get; set; }

        bool Corrupted { get; }

        string Password { get; set; }

        string Delimiter { get; }

        string ToString();

        byte ProtocolVersion { get; }

        Encoding Encoding { get; set; }

        PacketType Type { get; set; }

        IPEndPoint Source { get; set; }

        IPEndPoint Destination { get; set; }



    }
}
