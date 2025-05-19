using Google.Protobuf;
using Microsoft.Win32;
using PeerStack.Encoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PeerStack.Multiformat
{
    /// <summary>
    ///  Identifies some content, e.g. a Content ID.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   A Cid is a self-describing content-addressed identifier for distributed systems.
    ///   </para>
    ///   <para>
    ///   Initially, IPFS used a <see cref="MultiHash"/> as the CID and this is still supported as <see cref="Version"/> 0.
    ///   Version 1 adds a self describing structure to the multi-codec, see the <see href="https://github.com/ipld/cid">spec</see>.
    ///   </para>
    ///   <note>
    ///   The <see cref="MultiHash.Algorithm">hashing algorithm</see> must be "sha2-256" for a version 0 CID.
    ///   </note>
    /// </remarks>
    /// <seealso href="https://github.com/ipld/cid"/>
    public class Cid : IEquatable<Cid>
    {
        ///// <summary>
        /////   The default <see cref="ContentType"/>.
        ///// </summary>
        //public const string DefaultContentType = "dag-pb";

        //private int version;

        //private MultiHash codec;
        //private string contentType = DefaultContentType;
        //private string encoding = MultiBase.DefaultAlgorithmName;
        private string? encodedValue;

        /// <summary>
        ///   The version of the CID.
        /// </summary>
        /// <value>
        ///   0, 1 or (reserved 2, 3).
        /// </value>
        /// <remarks>
        ///   <para>
        ///   When the <see cref="Version"/> is 0 and the following properties
        ///   are not matched, then the version is upgraded to version 1 when any
        ///   of the properties is set.
        ///   <list type="bullet">
        ///   <item><description><see cref="ContentType"/> equals "dag-pb"</description></item>
        ///   <item><description><see cref="Encoding"/> equals "base58btc"</description></item>
        ///   <item><description><see cref="Hash"/> algorithm name equals "sha2-256"</description></item>
        ///   </list>
        ///   </para>
        ///   <para>
        ///   </para>
        ///   The default <see cref="Encoding"/> is "base32" when the <see cref="Version"/> is not zero.
        /// </remarks>
        public int Version { get; set; } = 0;

        /// <summary>
        /// The <see cref="MultiBase"/> encoding of the CID.
        /// </summary>
        /// <value>
        ///   base58btc, base32, base64, etc.  Defaults to <see cref="MultiBase.DefaultAlgorithmName"/>.
        /// </value>
        /// <seealso cref="MultiBase"/>
        public string Encoding { get; set; } = MultiBase.DefaultAlgorithmName;

        /// <summary>
        ///   The content type or format of the data being addressed.
        /// </summary>
        /// <value>
        ///   dag-pb, dag-cbor, dag-json, etc.  Defaults to "dag-pb".
        /// </value>
        /// <seealso cref="MultiCodec"/>
        public string ContentType { get; set; } = "dag-pb";
         
        public MultiHash Hash { get; set; }

        /// <summary>
        ///   Creates a default CID object for version 1
        /// </summary>
        public Cid()
        {
            Version = 1;
            ContentType = string.Empty;
            Encoding = string.Empty;
            Hash = MultiHash.DefaultAlgorithmName;
        }

        /// <summary>
        ///   Creates an equivalent CID object from the specified <see cref="string"/>
        /// </summary>
        /// <param name="input">
        ///  A CID encoded string
        /// </param>
        /// <exception cref="FormatException">
        ///   When the <paramref name="input"/> can not be constructed.
        /// </exception>
        public Cid(string input)
        {
            try
            {
                // SHA2-256 MultiHash is CID v0.
                if (input.Length == 46 && input.StartsWith("Qm"))
                {
                    Version = 0;
                    ContentType = "dag-pb";
                    Encoding = MultiBase.DefaultAlgorithmName;
                    Hash = new MultiHash(input);
                }
                else
                {
                    using (var ms = new MemoryStream(MultiBase.Decode(input), false))
                    {
                        var v = ms.ReadVarint32();
                        if (v > 3)
                        {
                            throw new InvalidDataException($"Unknown CID version '{v}'.");
                        }
                        Version = v; // set version

                        var encoding = MultiBaseCoder.Codecs.FirstOrDefault(o => o.Code == input[0]);
                        if (encoding is null || string.IsNullOrEmpty(encoding?.Name))
                        {
                            throw new InvalidDataException("Invalid CID encoding.");
                        }
                        Encoding = encoding!.Name;

                        Hash = new MultiHash(ms);

                        var codec = ms.ReadMultiCodec();
                        if (codec is null || string.IsNullOrEmpty(codec?.Name))
                        {
                            throw new InvalidDataException("Invalid CID codec.");
                        }
                        ContentType = codec!.Name;
                    }
                }
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid CID '{input}'.", e);
            }
        }

        /// <summary>
        ///   Creates an equivalent CID object from the specified <see cref="buffer"/>
        /// </summary>
        /// <param name="buffer">
        ///  A CID encoded string
        /// </param>
        /// <exception cref="FormatException">
        ///   When the <paramref name="buffer"/> can not be constructed.
        /// </exception>
        public Cid(byte[] buffer)
        {
            try
            {
                if (buffer.Length == 34)
                {
                    Version = 0;
                    Hash = new MultiHash(buffer);
                }
                else
                {
                    using (var ms = new MemoryStream(buffer, false))
                    {
                        Version = ms.ReadVarint32();
                       Hash = new MultiHash(ms);

                        var codec = ms.ReadMultiCodec();
                        if (codec is null || string.IsNullOrEmpty(codec?.Name))
                        {
                            throw new InvalidDataException("Invalid CID codec.");
                        }
                       ContentType = codec!.Name;
                    }
                }
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid CID '{buffer}'.", e);
            }
        }

        /// <summary>
        ///   Creates an equivalent CID object from the specified <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">
        ///  A CID encoded stream
        /// </param>
        /// <exception cref="FormatException">
        ///   When the <paramref name="stream"/> can not be constructed.
        /// </exception>
        public Cid(Stream stream)
        {
            try
            {
                var length = stream.ReadVarint32();
                if (length == 34)
                {
                    Version = 0;
                }
                else
                {
                    Version = stream.ReadVarint32();
                    var codec = stream.ReadMultiCodec();
                    if (codec is null || string.IsNullOrEmpty(codec?.Name))
                    {
                        throw new InvalidDataException("Invalid CID codec.");
                    }
                    ContentType = codec!.Name;
                }
                Hash = new MultiHash(stream);
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid CID '{stream}'.", e);
            }
        }

        /// <summary>
        ///   Converts the CID to its equivalent string representation.
        /// </summary>
        /// <returns>
        ///   The string representation of the <see cref="Cid"/>.
        /// </returns>
        /// <remarks>
        ///   For <see cref="Version"/> 0, this is equivalent to the 
        ///   <see cref="MultiHash.ToBase58()">base58btc encoding</see>
        ///   of the <see cref="Hash"/>.
        /// </remarks>
        /// <seealso cref="Decode"/>
        public string Encode()
        {
            if (encodedValue != null)
            {
                return encodedValue;
            }

            if (Version == 0)
            {
                encodedValue = Hash.ToBase58();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    ms.WriteVarint(Version);
                    ms.WriteMultiCodec(ContentType);
                    Hash.Write(ms);
                    encodedValue = MultiBase.Encode(ms.ToArray(), Encoding);
                }
            }

            return encodedValue;
        }

        /// <inheritdoc />
        private static Cid Decode(string input)
        {
            try
            {
                // SHA2-256 MultiHash is CID v0.
                if (input.Length == 46 && input.StartsWith("Qm"))
                {
                    return (Cid)new MultiHash(input);
                }

                using (var ms = new MemoryStream(MultiBase.Decode(input), false))
                {
                    var v = ms.ReadVarint32();
                    if (v > 3)
                    {
                        throw new InvalidDataException($"Unknown CID version '{v}'.");
                    }

                    var encoding = MultiBaseCoder.Codecs.FirstOrDefault(o => o.Code == input[0]);
                    if (encoding is null || string.IsNullOrEmpty(encoding?.Name))
                    {
                        throw new InvalidDataException("Invalid CID encoding.");
                    }

                    var codec = ms.ReadMultiCodec();
                    if (codec is null || string.IsNullOrEmpty(codec?.Name))
                    {
                        throw new InvalidDataException("Invalid CID codec.");
                    }

                    return new Cid
                    {
                        Version = v,
                        Encoding = encoding!.Name,
                        ContentType = codec!.Name,
                        Hash = new MultiHash(ms)
                    };
                }
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid CID '{input}'.", e);
            }
        }

        /// <summary>
        ///   Reads the binary representation of the CID from the specified byte array.
        /// </summary>
        /// <param name="buffer">
        ///   The source of a CID.
        /// </param>
        /// <returns>
        ///   A new <see cref="Cid"/>.
        /// </returns>
        /// <remarks>
        ///   The buffer does NOT start with a Varint length prefix.
        /// </remarks>
        public static Cid Read(byte[] buffer)
        {
            var cid = new Cid();
            if (buffer.Length == 34)
            {
                cid.Version = 0;
                cid.Hash = new MultiHash(buffer);
                return cid;
            }

            using (var ms = new MemoryStream(buffer, false))
            {
                cid.Version = ms.ReadVarint32();
                cid.Hash = new MultiHash(ms);

                var codec = ms.ReadMultiCodec();
                if (codec is null || string.IsNullOrEmpty(codec?.Name))
                {
                    throw new InvalidDataException("Invalid CID codec.");
                }
                cid.ContentType = codec!.Name;
                return cid;
            }
        }

        /// <summary>
        ///   Reads the binary representation of the CID from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to read from.
        /// </param>
        /// <returns>
        ///   A new <see cref="Cid"/>.
        /// </returns>
        public static Cid Read(Stream stream)
        {
            var cid = new Cid();
            var length = stream.ReadVarint32();
            if (length == 34)
            {
                cid.Version = 0;
            }
            else
            {
                cid.Version = stream.ReadVarint32();
                var codec = stream.ReadMultiCodec();
                if (codec is null || string.IsNullOrEmpty(codec?.Name))
                {
                    throw new InvalidDataException("Invalid CID codec.");
                }
                cid.ContentType = codec!.Name;
            }
            cid.Hash = new MultiHash(stream);

            return cid;
        }

        /// <summary>
        ///   Reads the binary representation of the CID from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedInputStream"/> to read from.
        /// </param>
        /// <returns>
        ///   A new <see cref="Cid"/>.
        /// </returns>
        public static Cid Read(CodedInputStream stream)
        {
            var cid = new Cid();
            var length = stream.ReadLength();
            if (length == 34)
            {
                cid.Version = 0;
            }
            else
            {
                cid.Version = stream.ReadInt32();
                var codec = stream.ReadMultiCodec();
                if (codec is null || string.IsNullOrEmpty(codec?.Name))
                {
                    throw new InvalidDataException("Invalid CID codec.");
                }
                cid.ContentType = codec!.Name;
            }
            cid.Hash = new MultiHash(stream);

            return cid;
        }

        /// <summary>
        ///   Writes the binary representation of the CID to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to write to.
        /// </param>
        public void Write(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                if (Version != 0)
                {
                    ms.WriteVarint(Version);
                    ms.WriteMultiCodec(this.ContentType);
                }
                Hash.Write(ms);

                stream.WriteVarint(ms.Length);
                ms.Position = 0;
                ms.CopyTo(stream);
            }
        }

        /// <summary>
        ///   Writes the binary representation of the CID to the specified <see cref="CodedOutputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        public void Write(CodedOutputStream stream)
        {
            using (var ms = new MemoryStream())
            {
                if (Version != 0)
                {
                    ms.WriteVarint(Version);
                    ms.WriteMultiCodec(this.ContentType);
                }
                Hash.Write(ms);

                var bytes = ms.ToArray();
                stream.WriteLength(bytes.Length);
                stream.WritePrimitiveBytes(bytes);
            }
        }

        /// <summary>
        ///   Returns the binary representation of the CID as a byte array.
        /// </summary>
        /// <returns>
        ///   A new buffer containing the CID.
        /// </returns>
        /// <remarks>
        ///   The buffer does NOT start with a varint length prefix.
        /// </remarks>
        public byte[] ToArray()
        {
            if (Version == 0)
            {
                return Hash.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                ms.WriteVarint(Version);
                ms.WriteMultiCodec(this.ContentType);
                Hash.Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///   Implicit casting of a <see cref="MultiHash"/> to a <see cref="Cid"/>.
        /// </summary>
        /// <param name="hash">
        ///   A <see cref="MultiHash"/>.
        /// </param>
        /// <returns>
        ///   A new <see cref="Cid"/> based on the <paramref name="hash"/>.  A <see cref="Version"/> 0
        ///   CID is returned if the <paramref name="hash"/> is "sha2-356"; otherwise <see cref="Version"/> 1.
        /// </returns>
        public static implicit operator Cid(MultiHash hash)
        {
            if (hash.Coder.Name == "sha2-256")
            {
                return new Cid
                {
                    Hash = hash,
                    Version = 0,
                    Encoding = "base58btc",
                    ContentType = "dag-pb"
                };
            }

            return new Cid
            {
                Version = 1,
                Hash = hash
            };
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Encode().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            var that = obj as Cid;
            return that is not null && this.Encode() == that.Encode();
        }

        /// <inheritdoc />
        public bool Equals(Cid? that)
        {
            return that is not null && this.Encode() == that.Encode();
        }

        /// <summary>
        ///   Returns the string representation of the CID in the general format.
        /// </summary>
        /// <returns>
        ///  e.g. "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V"
        /// </returns>
        public override string ToString()
        {
            return ToString("G");
        }

        /// <summary>
        ///   Returns a string representation of the CID
        ///   according to the provided format specifier.
        /// </summary>
        /// <param name="format">
        ///   A single format specifier that indicates how to format the value of the
        ///   CID.  Can be "G" or "L".
        /// </param>
        /// <returns>
        ///   The CID in the specified <paramref name="format"/>.
        /// </returns>
        /// <exception cref="FormatException">
        ///   <paramref name="format"/> is not valid.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The "G" format specifier is the same as calling <see cref="Encode"/>.
        /// </para>
        /// <list type="table">
        /// <listheader>
        ///   <term>Specifier</term>
        ///   <description>return value</description>
        /// </listheader>
        ///  <item>
        ///    <term>G</term>
        ///    <description>QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V</description>
        ///  </item>
        ///  <item>
        ///    <term>L</term>
        ///    <description>base58btc cidv0 dag-pb sha2-256 Qm...</description>
        ///  </item>
        /// </list>
        /// </remarks>
        public string ToString(string format)
        {
            switch (format)
            {
                case "G":
                    return Encode();

                case "L":
                    var sb = new StringBuilder();
                    sb.Append(Encoding);
                    sb.Append(' ');
                    sb.Append("cidv");
                    sb.Append(Version);
                    sb.Append(' ');
                    sb.Append(ContentType);
                    if (Hash is not null)
                    {
                        sb.Append(' ');
                        sb.Append(Hash.Coder.Name);
                        sb.Append(' ');
                        sb.Append(MultiBase.Encode(Hash.ToArray(), Encoding).AsSpan(1));
                    }
                    return sb.ToString();

                default:
                    throw new FormatException($"Invalid CID format specifier '{format}'.");
            }
        }

        /// <summary>
        ///   Value equality.
        /// </summary>
        public static bool operator ==(Cid a, Cid b)
        {
            return ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));
        }

        /// <summary>
        ///   Value inequality.
        /// </summary>
        public static bool operator !=(Cid a, Cid b)
        {
            return !ReferenceEquals(a, b) && (a is null || b is null || !a.Equals(b));
        }

        /// <summary>
        ///   Implicit casting of a <see cref="string"/> to a <see cref="Cid"/>.
        /// </summary>
        /// <param name="s">
        ///   A string encoded <b>Cid</b>.
        /// </param>
        /// <returns>
        ///   A new <see cref="Cid"/>.
        /// </returns>
        /// <remarks>
        ///    Equivalent to <code> Cid.Decode(s)</code>
        /// </remarks>
        public static implicit operator Cid(string s)
        {
            return Decode(s);
        }

        /// <summary>
        ///   Implicit casting of a <see cref="Cid"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="id">
        ///   A <b>Cid</b>.
        /// </param>
        /// <returns>
        ///   A new <see cref="string"/>.
        /// </returns>
        /// <remarks>
        ///    Equivalent to <code>Cid.Encode()</code>
        /// </remarks>
        public static implicit operator string(Cid id)
        {
            return id.Encode();
        }

    }
}
