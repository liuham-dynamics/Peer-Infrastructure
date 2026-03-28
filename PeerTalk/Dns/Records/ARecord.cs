using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Contains the IPv4 address of the named resource.
    /// </summary>
    public class ARecord : AddressRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="ARecord"/> class.
        /// </summary>
        public ARecord() : base()
        {
            Type = DnsType.A;
        }
    }
}
