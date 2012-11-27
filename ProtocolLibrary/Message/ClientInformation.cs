using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace ProtocolLibrary.Message
{
    [Serializable]
    public class ClientInformation : IComparable
    {
        public ClientInformation(IPEndPoint ipEndPoint)
        {
            CalculateIdentifier(ipEndPoint);
        }

        public void CalculateIdentifier(IPEndPoint ipEndPoint)
        {
            if (ipEndPoint != null)
            {
                Identifier = ipEndPoint.Address.ToString() + " | " + ipEndPoint.Port;
            }
            else
            {
                Identifier = "Not initialized!";
            }

        }

        public int CompareTo(object obj)
        {
            ClientInformation otherClientInfo = obj as ClientInformation;

            if (otherClientInfo != null)
            {
                return Identifier.CompareTo(otherClientInfo.Identifier);
            }
            else
            {
                throw new ArgumentException("Object is not a ClientInfo");
            }
        }

        public string Identifier { get; private set; }
    }
}
