using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    public class Onion3NetworkTransport: OnionNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 445;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "onion3";

    }
}
