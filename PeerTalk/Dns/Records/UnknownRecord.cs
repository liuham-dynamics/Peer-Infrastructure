using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   An unknown resource record.
    /// </summary>
    public class UnknownRecord : ResourceRecord
    {
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
