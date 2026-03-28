using SimpleBase;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Parameters needed by authoritative servers to calculate hashed owner _memberNames.
    /// </summary>
    /// <remarks>
    ///   Defined by <see href="https://tools.ietf.org/html/rfc5155#section-4">RFC 5155 - DNS Security (DNSSEC) Hashed Authenticated Denial of Existence</see>.
    /// </remarks>
    public sealed class NSEC3PARAMRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="NSEC3PARAMRecord"/> class.
        /// </summary>
        public NSEC3PARAMRecord() : base()
        {
            Type = DnsType.NSEC3PARAM;
            Salt = [];
        }

        /// <summary>
        ///   The cryptographic hash algorithm used to create the hashed owner name.
        /// </summary>
        /// <value>
        ///   One of the <see cref="DigestType"/> value.
        /// </value>
        public DigestType HashAlgorithm { get; set; }

        /// <summary>
        ///   Not used, must be zero.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        ///   Number of times to perform the <see cref="HashAlgorithm"/>.
        /// </summary>
        public ushort Iterations { get; set; }

        /// <summary>
        ///   Appended to the original owner name before hashing.
        /// </summary>
        /// <remarks>
        ///   Used to defend against pre-calculated dictionary attacks.
        /// </remarks>
        public byte[] Salt { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            _ = reader.Position + length;

            HashAlgorithm = (DigestType)reader.ReadByte();
            Flags = reader.ReadByte();
            Iterations = reader.ReadUInt16();
            Salt = reader.ReadByteLengthPrefixedBytes();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            writer.WriteByte((byte)HashAlgorithm);
            writer.WriteByte(Flags);
            writer.WriteUInt16(Iterations);
            writer.WriteByteLengthPrefixedBytes(Salt);
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            HashAlgorithm = (DigestType)reader.ReadByte();
            Flags = reader.ReadByte();
            Iterations = reader.ReadUInt16();

            var salt = reader.ReadString();
            if (salt != "-")
                Salt = Base16.Decode(salt).ToArray();
        }

        /// <inheritdoc />
        public override void WriteData(PresentationWriter writer)
        {
            writer.WriteByte((byte)HashAlgorithm);
            writer.WriteByte((byte)Flags);
            writer.WriteUInt16(Iterations);

            if (Salt == null || Salt.Length == 0)
            {
                writer.WriteString("-");
            }
            else
            {
                writer.WriteBase16String(Salt, appendSpace: false);
            }
        }
    }
}
