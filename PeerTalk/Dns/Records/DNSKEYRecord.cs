using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;

namespace PeerTalk.Dns.Records
{
    /// <summary>
    ///   Public key cryptography to sign and authenticate resource records.
    /// </summary>
    public class DNSKEYRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="DNSKEYRecord"/> class.
        /// </summary>
        public DNSKEYRecord() : base()
        {
            Type = DnsType.DNSKEY;
            PublicKey = [];
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="DNSKEYRecord"/> class
        ///   from the specified RSA key.
        /// </summary>
        /// <param name="key">
        ///   A public or private RSA key.
        /// </param>
        /// <param name="algorithm">
        ///   The security algorithm to use.  Only RSA types are allowed.
        /// </param>
        public DNSKEYRecord(RSA key, SecurityAlgorithm algorithm) : this()
        {
            switch (algorithm)
            {
                case SecurityAlgorithm.RSAMD5:
                case SecurityAlgorithm.RSASHA1:
                case SecurityAlgorithm.RSASHA1NSEC3SHA1:
                case SecurityAlgorithm.RSASHA256:
                case SecurityAlgorithm.RSASHA512:
                    break;

                default:
                    throw new ArgumentException($"Security algorithm '{algorithm}' is not allowed for a RSA key.");
            }
            Algorithm = algorithm;

            using (var ms = new MemoryStream())
            {
                var p = key.ExportParameters(includePrivateParameters: false);
                ms.WriteByte((byte)p.Exponent!.Length);
                ms.Write(p.Exponent, 0, p.Exponent.Length);
                ms.Write(p.Modulus!, 0, p.Modulus!.Length);
                PublicKey = ms.ToArray();
            }
        }
 
        /// <summary>
        ///  Identifies the intended usage of the key.
        /// </summary>
        public DNSKEYFlags Flags { get; set; }

        /// <summary>
        ///   Must be three.
        /// </summary>
        /// <value>
        ///   Defaults to 3.
        /// </value>
        public byte Protocol { get; set; } = 3;

        /// <summary>
        ///   Identifies the public key's cryptographic algorithm.
        /// </summary>
        /// <value>
        ///   Identifies the type of key (RSA, ECDSA, ...) and the
        ///   hashing algorithm.
        /// </value>
        /// <remarks>
        ///    Determines the format of the<see cref="PublicKey"/>.
        /// </remarks>
        public SecurityAlgorithm Algorithm { get; set; }

        /// <summary>
        ///   The public key material.
        /// </summary>
        /// <value>
        ///   The format depends on the key <see cref="Algorithm"/>.
        /// </value>
        public byte[] PublicKey { get; set; }

        /// <summary>
        ///   Calculates the key tag.
        /// </summary>
        /// <value>
        ///   A non-unique identifier for the public key.
        /// </value>
        /// <remarks>
        ///   <see href="https://tools.ietf.org/html/rfc4034#appendix-B"/> for the details.
        /// </remarks>
        public ushort KeyTag()
        {
            var key = this.GetData();
            var length = key.Length;
            int ac = 0;

            for (var i = 0; i < length; ++i)
            {
                ac += (i & 1) == 1 ? key[i] : key[i] << 8;
            }
            ac += (ac >> 16) & 0xFFFF;
            return (ushort)(ac & 0xFFFF);
        }

        /// <inheritdoc />
        public override void ReadData(DnsWireReader reader, int length)
        {
            var end = reader.Position + length;

            Flags = (DNSKEYFlags)reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBytes(end - reader.Position);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWireWriter writer)
        {
            writer.WriteUInt16((ushort)Flags);
            writer.WriteByte(Protocol);
            writer.WriteByte((byte)Algorithm);
            writer.WriteBytes(PublicKey);
        }

        /// <inheritdoc />
        public override void ReadData(PresentationReader reader)
        {
            Flags = (DNSKEYFlags)reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBase64String();
        }

        /// <inheritdoc />
        public override void WriteData(PresentationWriter writer)
        {
            writer.WriteUInt16((ushort)Flags);
            writer.WriteByte(Protocol);
            writer.WriteByte((byte)Algorithm);
            writer.WriteBase64String(PublicKey, appendSpace: false);
        }
    }
}
