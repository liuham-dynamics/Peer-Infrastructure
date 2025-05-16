using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    public class QuicNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 460;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "quic";
    
    }
}
