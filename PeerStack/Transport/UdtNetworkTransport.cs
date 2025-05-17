using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// UDT transport network protocol
    /// </summary>
    public class UdtNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 301;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "udt";

    }
}
