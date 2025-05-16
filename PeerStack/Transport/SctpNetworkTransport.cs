using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// SCTP transport network protocol
    /// </summary>
    public class SctpNetworkTransport : TcpNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 132;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "sctp";
    }
}
