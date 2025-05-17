using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// DNS4 transport network protocol
    /// </summary>
    public class Dns4NetworkTransport : ADomainNameNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 54;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "dns4";

    }
}
