using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   An authoritative name server.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   NS records cause both the usual additional section processing to locate
    ///   a type A record, and, when used in a referral, a special search of the
    ///   zone in which they reside for glue information.
    /// </para>
    /// <para>
    ///   The NS RR states that the named host should be expected to have a zone
    ///   starting at owner name of the specified class.  Note that the class may
    ///   not indicate the protocol family which should be used to communicate
    ///   with the host, although it is typically a strong hint.For example,
    ///   hosts which are name servers for either Internet (IN) or Hesiod (HS)
    ///   class information are normally queried using IN class protocols.
    /// </para>
    /// </remarks>
    public sealed class NSRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="NSRecord"/> class.
        /// </summary>
        public NSRecord() : base()
        {
            Type = DnsType.NS;
            Authority = DomainName.Root;
        }

        /// <summary>
        ///   A domain-name which specifies a host which should be
        ///   authoritative for the specified class and domain.
        /// </summary>
        public DomainName Authority { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            Authority = reader.ReadDomainName();
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            Authority = reader.ReadDomainName();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            writer.WriteDomainName(Authority);
        }

        /// <inheritdoc />
        public override void WriteData(PresentationWriter writer)
        {
            writer.WriteDomainName(Authority, appendSpace: false);
        }
    }
}
