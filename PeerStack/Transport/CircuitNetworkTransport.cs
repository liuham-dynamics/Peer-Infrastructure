using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// Circuit transport network protocol
    /// </summary>
    public class CircuitNetworkTransport :AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 290;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "p2p-circuit";
    }
}
