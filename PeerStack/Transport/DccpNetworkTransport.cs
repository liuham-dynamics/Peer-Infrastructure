using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// DCCP transport network protocol
    /// </summary>
    public class DccpNetworkTransport : TcpNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 33;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "dccp";
  
    }
}
