using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    public class Ipv4NetworkTransport : AIpNetworkTransport
    {
        private static readonly int AddressSize = IPAddress.Any.GetAddressBytes().Length;

        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 4;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "ip4";

        /// <summary>
        /// Read IP address values from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        /// <exception cref="FormatException"></exception>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            if (Address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new FormatException(string.Format("'{0}' is not a valid IPv4 address.", Value));
            }
        }

        /// <summary>
        /// Read IP address values from primitive stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        /// <exception cref="NullReferenceException"></exception>
        public override void ReadValue(CodedInputStream stream)
        {
            var a = stream.ReadPrimitiveBytes(AddressSize);
            if(a is not null)
            {
                Address = new IPAddress(a);
                Value = Address.ToString();
            }
            else
            {
                throw new NullReferenceException("Stream content is empty.");
            }
        }
    }
}
