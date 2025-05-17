using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// Domain named transport network protocol
    /// </summary>
    public class DnsNetworkTransport : ADomainNameNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 53;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "dns";

    }
}
