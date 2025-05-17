using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// Web Sockets transport network protocol
    /// </summary>
    public class WsNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 477;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "ws";
      
    }
}
