using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class ADomainNameNetworkTransport : ANetworkTransport
    {
        /// <summary>
        /// Domain name
        /// </summary>
        public string DomainName { get; set; } = string.Empty;

        /// <summary>
        /// Read name values from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            DomainName = Value;
        }

        /// <summary>
        /// Read name values from primitive stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        public override void ReadValue(CodedInputStream stream)
        {
            Value = stream.ReadString();
            DomainName = Value;
        }

        /// <summary>
        /// Write name values to stream
        /// </summary>
        /// <param name="stream">TextWriter</param>
        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(DomainName.Trim());
        }

        /// <summary>
        /// Write name values to primitive stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WriteString(DomainName);
        }
    }
}
