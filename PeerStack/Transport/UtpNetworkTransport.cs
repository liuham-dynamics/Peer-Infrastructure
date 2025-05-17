using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// UTP transport network protocol
    /// </summary>
    public class UtpNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 302;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "utp";

    }
}
