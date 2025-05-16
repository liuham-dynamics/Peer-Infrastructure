using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    public class IpfsNetworkTransport : P2pNetworkTransport
    {
        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "ipfs";
    }
}
