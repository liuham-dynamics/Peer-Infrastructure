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
    public abstract class AValuelessNetworkTransport : ANetworkTransport
    {
        public override void ReadValue(CodedInputStream stream)
        {
            // No value to read
        }

        public override void ReadValue(TextReader stream)
        {
            // No value to read
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            // No value to write
        }
    }
}
