using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// CIDR mask for IP addresses transport network protocol
    /// </summary>
    public class IpCidrNetworkTransport : ANetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 43;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "ipcidr";

        /// <summary>
        /// Routing prefix
        /// </summary>
        public ushort RoutingPrefix { get; set; }

        /// <summary>
        /// Read routing prefix value from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        /// <exception cref="FormatException"></exception>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                RoutingPrefix = ushort.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid routing prefix.", Value), e);
            }
        }

        /// <summary>
        /// Read routing prefix value from primitive stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        /// <exception cref="NullReferenceException"></exception>
        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadPrimitiveBytes(2) 
                ?? throw new NullReferenceException("Routing Prefix bytes can NOT be empty.");
            RoutingPrefix = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = RoutingPrefix.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write routing prefix stream to primitive stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        /// <exception cref="NullReferenceException"></exception>
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RoutingPrefix))
                ?? throw new NullReferenceException("Routing Prefix bytes can NOT be empty.");
            stream.WritePrimitiveBytes(bytes);
        }
    }
}
