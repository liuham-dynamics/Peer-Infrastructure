using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   A null RR (EXPERIMENTAL).
    /// </summary>
    /// <remarks>
    ///  NULL records cause no additional section processing.  NULL RRs are not
    ///  allowed in master files. NULLs are used as placeholders in some
    ///  experimental extensions of the DNS.
    /// </remarks>
    public sealed class NULLRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="NULLRecord"/> class.
        /// </summary>
        public NULLRecord() : base()
        {
            Type = DnsType.NULL;
            Data = [];
        }

        /// <summary>
        ///    Specfic data for the resource.
        /// </summary>
        public byte[] Data { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            Data = reader.ReadBytes(length);
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            Data = reader.ReadResourceData();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            writer.WriteBytes(Data);
        }


    }
}
