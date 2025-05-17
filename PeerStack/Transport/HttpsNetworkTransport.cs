using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    /// HTTPS transport network protocol
    /// </summary>
    public class HttpsNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 443;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "https";

    }
}
