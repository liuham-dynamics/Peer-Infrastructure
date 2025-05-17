using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// Secure Web Sockets transport network protocol
    /// </summary>
    public class WssNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 478;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "wss";
      
    }
}
