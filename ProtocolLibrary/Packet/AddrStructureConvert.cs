using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolLibrary.Packet
{
    class AddrStructureConvert
    {
        /// <summary>
        /// This routine converts an IPEndPoint into a byte array that represents the
        /// underlying sockaddr structure of the correct type. Currently this routine
        /// supports IPv4 and IPv6 socket address structures. A sockaddr structure
        /// InterNetwork (v4) could be { 2, 0, 0, 156, 207, 46, 197, 32, 0, 0, 0, 0, 0, 0, 0, 0 }.
        /// The first two bytes represents the address family, the next two bytes the
        /// port number in big-endian. The next 4 bytes are reserved for the ip-address.
        /// 
        /// The in6_addr structure represents an IPv6 internet address. This
        /// InterNetwork (v6) structure is longer and holds a 16 byte long ip-address.
        /// 
        /// This method will always return 32 bytes, padding from the right with zeros.
        /// </summary>
        /// <param name="endPoint">IPEndPoint to convert to a binary form</param>
        /// <returns>Binary array of the serialized socket address structure</returns>
        static public byte[] GetBytes(IPEndPoint endPoint)
        {
            SocketAddress socketAddress = endPoint.Serialize();

            byte[] sockaddrBytes = new byte[socketAddress.Size];

            for (int i = 0; i < socketAddress.Size; i++)
            {
                sockaddrBytes[i] = socketAddress[i];
            }

            byte[] buffer = new byte[32];

            Array.Copy(sockaddrBytes, buffer, socketAddress.Size);

            return buffer;
        }

        /// <summary>
        /// This routine converts the binary representation of a sockaddr structure back
        /// into an IPEndPoint object. This is done by looking at the first 2 bytes of the
        /// serialized byte array which always indicate the address family of the underlying
        /// structure. From this we can construct the appropriate IPEndPoint object.
        /// </summary>
        /// <param name="sockaddrBytes">The serialized sockaddr structure</param>
        /// <returns>The corresponding ip endpoint</returns>
        static public IPEndPoint GetEndPoint(byte[] sockaddrBytes)
        {
            IPEndPoint unpackedEndpoint = null;
            IPAddress unpackedAddress;
            ushort addressFamily, unpackedPort;

            // Reconstruct the 16-bit (short) value representing the address family    
            addressFamily = BitConverter.ToUInt16(sockaddrBytes, 0);

            if (addressFamily == 2)
            {   // AF_INET 
                // Bitconverter.ToUInt16 returns little-endian, so we have to reorder
                // since the sockaddr structure holds the port number in big-endian style.
                unpackedPort = (UInt16)(sockaddrBytes[2] << 8 | sockaddrBytes[3]);
                unpackedAddress = new IPAddress(BitConverter.ToUInt32(sockaddrBytes, 4));
                unpackedEndpoint = new IPEndPoint(unpackedAddress, unpackedPort);
            }
            else if (addressFamily == 23)
            {   // AF_INET6
                byte[] addressBytes = new byte[16];
                unpackedPort = (UInt16)(sockaddrBytes[2] << 8 | sockaddrBytes[3]);
                Array.Copy(sockaddrBytes, 8, addressBytes, 0, 16);
                unpackedAddress = new IPAddress(addressBytes);
                unpackedEndpoint = new IPEndPoint(unpackedAddress, unpackedPort);
            }
            else
            {
                throw new TypeLoadException("GetEndPoint: Unknown address family: " + addressFamily);
            }

            return unpackedEndpoint;
        }
    }
}