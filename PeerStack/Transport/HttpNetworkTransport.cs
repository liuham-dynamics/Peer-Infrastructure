using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// HTTP transport network protocol
    /// </summary>
    public class HttpNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 480;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "http";
  
    }
}
