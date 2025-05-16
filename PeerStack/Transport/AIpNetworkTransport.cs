using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    ///  Definition for metadata on an libp2p network protocol.
    /// </summary>
    /// <remarks>
    ///   Protocols are defined at <see href="https://github.com/multiformats/multiaddr/blob/master/protocols.csv"/>.
    /// </remarks>
    /// <seealso cref="MultiAddress"/>
    public abstract class AIpNetworkTransport : ANetworkTransport
    {
        /// <summary>
        /// An internet protocol address
        /// </summary>
        public required IPAddress Address { get; set; }

        /// <summary>
        /// Read address from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        /// <exception cref="FormatException"></exception>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                // Remove the scope id.
                int i = Value.LastIndexOf('%');
                if (i != -1)
                {
                    Value = Value[..i];
                }

                // parse
                Address = IPAddress.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid IP address.", Value), e);
            }
        }

        /// <summary>
        /// Write address to stream
        /// </summary>
        /// <param name="stream">TextWriter</param>
        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(Address.ToString());
        }

        /// <summary>
        /// Write address to primitive stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        public override void WriteValue(CodedOutputStream stream)
        {
            var ip = Address.GetAddressBytes();
            stream.WritePrimitiveBytes(ip);
        }
    }
}
