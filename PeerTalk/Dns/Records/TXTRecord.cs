using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Text strings.
    /// </summary>
    /// <remarks>
    ///   TXT RRs are used to hold descriptive _memberTextReader.  The semantics of the _memberTextReader
    ///   depends on the domain where it is found.
    /// </remarks>
    public sealed class TXTRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="TXTRecord"/> class.
        /// </summary>
        public TXTRecord() : base()
        {
            Type = DnsType.TXT;
        }

        /// <summary>
        ///  The sequence of strings.
        /// </summary>
        public List<string> Strings { get; set; } = new List<string>();

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            while (length > 0)
            {
                var s = reader.ReadString();
                Strings.Add(s);
                length -= Encoding.UTF8.GetByteCount(s) + 1;
            }
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            while (!reader.IsEndOfLine())
            {
                Strings.Add(reader.ReadString());
            }
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            foreach (var s in Strings)
            {
                writer.WriteString(s);
            }
        }

        /// <inheritdoc />
        public override void WriteData(PresentationWriter writer)
        {
            bool next = false;
            foreach (var s in Strings)
            {
                if (next)
                {
                    writer.WriteSpace();
                }
                writer.WriteString(s, appendSpace: false);
                next = true;
            }
        }
    }
}
