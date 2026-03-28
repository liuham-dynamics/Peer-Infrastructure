using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Host information.
    /// </summary>
    /// <remarks>
    /// <para>  Standard values for CPU and OS can be found in [RFC-1010].</para>
    /// <para>
    ///   HINFO records are used to acquire general information about a host. The
    ///   main use is for protocols such as FTP that can use special procedures
    ///   when talking between machines or operating systems of the same type.
    /// </para>
    /// </remarks>
    public sealed class HINFORecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="HINFORecord"/> class.
        /// </summary>
        public HINFORecord() : base()
        {
            Type = DnsType.HINFO;
            TTL = DefaultHostTTL;
            Cpu = string.Empty;
            OS = string.Empty;
        }

        /// <summary>
        ///  CPU type.
        /// </summary>
        public string Cpu { get; set; }

        /// <summary>
        ///  Operating system type.
        /// </summary>
        public string OS { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            Cpu = reader.ReadString();
            OS = reader.ReadString();
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            Cpu = reader.ReadString();
            OS = reader.ReadString();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            writer.WriteString(Cpu);
            writer.WriteString(OS);
        }

        /// <inheritdoc />
        public override void WriteData(PresentationWriter writer)
        {
            writer.WriteString(Cpu);
            writer.WriteString(OS, appendSpace: false);
        }
    }
}
