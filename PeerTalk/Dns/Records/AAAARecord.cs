using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Contains the IPv6 address of the named resource.
    /// </summary>
    public sealed class AAAARecord : AddressRecordBase
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="AAAARecord"/> class.
        /// </summary>
        public AAAARecord() : base()
        {
            Type = DnsType.AAAA;
        }
    }
}
