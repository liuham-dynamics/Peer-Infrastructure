using Google.Protobuf;
using PeerStack.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PeerStack.Multiformat
{
    /// <summary>
    ///   A protocol for differentiating outputs from various well-established cryptographic hash functions,
    ///   addressing size + encoding considerations.
    /// </summary>
    /// <remarks>
    ///   See the <see cref="MultiHashCoder">registry</see> for supported algorithms.
    /// </remarks>
    /// <seealso href="https://github.com/jbenet/multihash"/>
    public class MultiHash : IEquatable<MultiHash>
    {

        /// <summary>
        ///   The cached base-58 encoding of the multi-hash.
        /// </summary>
        private string b58String;

        /// <summary>
        ///   The default hashing algorithm is "sha2-256".
        /// </summary>
        public const string DefaultAlgorithmName = "sha2-256";


        /// <summary>
        ///   The multihash coder.
        /// </summary>
        /// <value>
        ///   Details on the hashing algorithm.
        /// </value>
        public MultiHashCoder Coder { get; private set; }

        /// <summary>
        ///   The hashing algorithm's digest value.
        /// </summary>
        /// <value>
        ///   The output of the hashing algorithm.
        /// </value>
        public byte[] Digest { get; private set; }

        /// <summary>
        ///   Determines if the identity hash algorithm is in use.
        /// </summary>
        /// <value>
        ///   <b>true</b> if the identity hash algorithm is used; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        ///   The identity hash is used to inline a small amount of data into a <see cref="Cid"/>.
        ///   When <b>true</b>, the <see cref="Digest"/> is also the content.
        /// </remarks>
        public bool IsIdentityHash => Coder.Code == 0;


        /// <summary>
        ///   Creates a new instance of the <see cref="MultiHash"/> class with the
        ///   specified <see cref="MultiHashCoder">Algorithm name</see> and <see cref="Digest"/> value.
        /// </summary>
        /// <param name="algorithmName">
        ///   A valid libp2p hashing algorithm name, e.g. "sha2-256" or "sha2-512".
        /// </param>
        /// <param name="digest">
        ///    The digest value as a byte array.
        /// </param>
        public MultiHash(string algorithmName, byte[] digest)
        {
            b58String = string.Empty;

            ArgumentException.ThrowIfNullOrWhiteSpace(algorithmName);

            ArgumentNullException.ThrowIfNull(digest);

            var coder = MultiHashCoder.GetAlgorithmMetadata(algorithmName)
                ?? throw new ArgumentException(string.Format("The libp2p multihash coder '{0}' is unknown.", algorithmName));
            Coder = coder;

            if (Coder.DigestSize != 0)
            {
                // NOTE: AS of .NET 7 the Digest for shake-128 (and a times shake-256)
                //       always appears off with additional character (double it's length)
                //       a check to adjust for that is necessary pending when we figure it out
                Digest = Coder.DigestSize != digest.Length
                                               ? digest.AsSpan(0, Coder.DigestSize).ToArray()
                                               : digest;
            }
            else
            {
                Digest = []; // Identity digest
            }
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="MultiHash"/> class from the specified byte array.
        /// </summary>
        /// <param name="buffer">
        ///   A sequence of bytes containing the binary representation of the <b>MultiHash</b>.
        /// </param>
        /// <remarks>
        ///   Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="buffer"/>.
        ///   <para>
        ///   The binary representation is a <see cref="Varint"/> of the <see cref="MultiHashCoder.Code"/>,
        ///   <see cref="Varint"/> of the <see cref="MultiHashCoder.DigestSize"/> followed by the <see cref="Digest"/>.
        ///   </para>
        ///   <para>
        ///   When an unknown <see cref="MultiHashCoder.Code">hashing algorithm number</see> is encountered
        ///   a new hashing algorithm is <see cref="MultiHashCoder.Register">registered</see>.  This new algorithm does not support
        ///   matching nor computing a hash.
        ///   This behavior allows parsing of any well formed <see cref="MultiHash"/> even when
        ///   the hashing algorithm is unknown.
        ///   </para>
        /// </remarks>
        /// <seealso cref="ToArray"/>
        public MultiHash(byte[] buffer)
        {
            b58String = string.Empty;
            Digest = [];
            
            using (var ms = new MemoryStream(buffer, false))
            {
                Read(ms);
            }
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="MultiHash"/> class from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the binary representation of the <b>MultiHash</b>.
        /// </param>
        /// <remarks>
        ///   Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="stream"/>.
        ///   <para>
        ///   The binary representation is a <see cref="Varint"/> of the <see cref="MultiHashCoder.Code"/>,
        ///   <see cref="Varint"/> of the <see cref="MultiHashCoder.DigestSize"/> followed by the <see cref="Digest"/>.
        ///   </para>
        ///   <para>
        ///   When an unknown <see cref="MultiHashCoder.Code">hashing algorithm number</see> is encountered
        ///   a new hashing algorithm is <see cref="MultiHashCoder.Register">registered</see>.  This new algorithm does not support
        ///   matching nor computing a hash.
        ///   This behavior allows parsing of any well formed <see cref="MultiHash"/> even when
        ///   the hashing algorithm is unknown.
        ///   </para>
        /// </remarks>
        public MultiHash(Stream stream)
        {
            b58String = string.Empty;
            Digest = [];

            Read(stream);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="MultiHash"/> class from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="CodedInputStream"/> containing the binary representation of the <b>MultiHash</b>.
        /// </param>
        /// <remarks>
        ///   Reads the binary representation of <see cref="MultiHash"/> from the <paramref name="stream"/>.
        ///   <para>
        ///   The binary representation is a <see cref="Varint"/> of the <see cref="MultiHashCoder.Code"/>,
        ///   <see cref="Varint"/> of the <see cref="MultiHashCoder.DigestSize"/> followed by the <see cref="Digest"/>.
        ///   </para>
        ///   <para>
        ///   When an unknown <see cref="MultiHashCoder.Code">hashing algorithm number</see> is encountered
        ///   a new hashing algorithm is <see cref="MultiHashCoder.Register">registered</see>.  This new algorithm does not support
        ///   matching nor computing a hash.
        ///   This behaviour allows parsing of any well formed <see cref="MultiHash"/> even when
        ///   the hashing algorithm is unknown.
        ///   </para>
        /// </remarks>
        public MultiHash(CodedInputStream stream)
        {
            b58String = string.Empty;
            Digest = [];

            Read(stream);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="MultiHash"/> class from the specified <see cref="Base58"/> encoded <see cref="string"/>.
        /// </summary>
        /// <param name="s">
        ///   A <see cref="Base58"/> encoded <b>MultiHash</b>.
        /// </param>
        /// <remarks>
        ///   <para>
        ///   When an unknown <see cref="MultiHashCoder.Code">hashing algorithm number</see> is encountered
        ///   a new hashing algorithm is <see cref="MultiHashCoder.Register">registered</see>.  This new algorithm does not support
        ///   matching nor computing a hash.
        ///   This behaviour allows parsing of any well formed <see cref="MultiHash"/> even when
        ///   the hashing algorithm is unknown.
        ///   </para>
        /// </remarks>
        /// <seealso cref="ToBase58"/>
        public MultiHash(string s)
        {
            b58String = string.Empty;
            Digest = [];

            using (var ms = new MemoryStream(s.FromBase58(), false))
            {
                Read(ms);
            }
        }

        /// <summary>
        /// Implicit casting of a <see cref="string"/> to a <see cref="MultiHash"/>.
        /// </summary>
        /// <param name="s">
        ///   A <see cref="Base58"/> encoded <b>MultiHash</b>.
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiHash"/>.
        /// </returns>
        /// <remarks>
        ///    Equivalent to <code>new MultiHash(s)</code>
        /// </remarks>
        public static implicit operator MultiHash(string s)
        {
            return new MultiHash(s);
        }


        /// <summary>
        ///   Gets the <see cref="HashAlgorithm"/> with the specified libp2p multi-hash name.
        /// </summary>
        /// <param name="name">
        ///   The name of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        ///   for libp2p defined names.
        /// </param>
        /// <returns>
        ///   The hashing implementation associated with the <paramref name="name"/>.
        ///   After using the hashing algorithm it should be disposed.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static HashAlgorithm GetHashAlgorithm(string name = DefaultAlgorithmName)
        {
            // get algorithm
            var algo = MultiHashCoder.GetAlgorithm(name)
                    ?? throw new KeyNotFoundException($"Hash algorithm '{name}' is not registered.");

            // return
            return algo;
        }

        /// <summary>
        ///   Gets the name of hashing algorithm name with the specified code.
        /// </summary>
        /// <param name="code">
        ///   The code of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        ///   for libp2p defined codes.
        /// </param>
        /// <returns>
        ///   The name assigned to <paramref name="code"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="code"/> is not registered.
        /// </exception>
        public static string GetHashAlgorithmName(int code)
        {
            var algo = MultiHashCoder.Hashes.FirstOrDefault(o => o.Code == code);
            if (algo is null || string.IsNullOrEmpty(algo?.Name))
            {
                throw new KeyNotFoundException($"Hash algorithm with code '{code}' is not registered.");
            }

            //
            return algo!.Name;
        }


        /// <summary>
        ///   Writes the binary representation of the multihash to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to write to.
        /// </param>
        /// <remarks>
        ///   The binary representation is a 1-byte <see cref="HashingAlgorithm.Code"/>,
        ///   1-byte <see cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
        /// </remarks>
        public void Write(Stream stream)
        {
            using (var cos = new CodedOutputStream(stream, true))
            {
                Write(cos);
            }
        }

        /// <summary>
        ///   Writes the binary representation of the multihash to the specified <see cref="CodedOutputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        /// <remarks>
        ///   The binary representation is a <see cref="Varint"/> of the <see cref="HashingAlgorithm.Code"/>,
        ///   <see cref="Varint"/> of the <see cref="HashingAlgorithm.DigestSize"/> followed by the <see cref="Digest"/>.
        /// </remarks>
        public void Write(CodedOutputStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteInt32(Coder.Code);
            stream.WriteLength(Digest.Length);
            stream.WritePrimitiveBytes(Digest);
        }

        private void Read(Stream stream)
        {
            using (var cis = new CodedInputStream(stream, true))
            {
                Read(cis);
            }
        }

        private void Read(CodedInputStream stream)
        {
            //
            var code = stream.ReadInt32();
            var digestSize = stream.ReadLength();

            //
            var coder = MultiHashCoder.Hashes.FirstOrDefault(o => o.Code == code);
            if(coder is null | string.IsNullOrEmpty(Coder?.Name))
            {
                Coder = MultiHashCoder.Register("p2p-" + code, code, digestSize);
            }
            else
            {
                Coder = coder!;
            }
          
            //
            if (Coder.DigestSize != 0 && digestSize != Coder.DigestSize)
            {
                throw new InvalidDataException(string.Format("The digest size {0} is wrong for {1}; it should be {2}.", digestSize, coder!.Name, coder!.DigestSize));
            }
             
            //
            var digest = stream.ReadPrimitiveBytes(digestSize);
            if(digest is not null)
            {
                Digest = digest;
            }
        }
         
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            var that = obj as MultiHash;
            return (that is not null) && this.Equals(that);
        }

        /// <inheritdoc />
        public bool Equals(MultiHash? that)
        {
            if(that is null)
            {
                return false;
            }

            return this.Coder.Code == that!.Coder.Code
                && this.Digest.SequenceEqual(that.Digest);
        }

        /// <summary>
        ///   Value equality.
        /// </summary>
        public static bool operator ==(MultiHash a, MultiHash b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null) return false;
            if (b is null) return false;

            return a.Equals(b);
        }

        /// <summary>
        ///   Value inequality.
        /// </summary>
        public static bool operator !=(MultiHash a, MultiHash b)
        {
            return !(a == b);
        }

        /// <summary>
        ///   Returns the <see cref="Base58"/> encoding of the <see cref="MultiHash"/>.
        /// </summary>
        /// <returns>
        ///   A base-58 representation of the MultiHash.
        /// </returns>
        /// <seealso cref="ToBase58"/>
        public override string ToString()
        {
            return this.ToBase58();
        }

        /// <summary>
        ///   Returns the <see cref="Base58"/> encoding of the <see cref="MultiHash"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="Base58"/> representation of the <see cref="MultiHash"/>.
        /// </returns>
        public string ToBase58()
        {
            if (b58String != null)
            {
                return b58String;
            }

            using (var ms = new MemoryStream())
            {
                Write(ms);
                b58String = ms.ToArray().ToBase58();
                return b58String;
            }
        }

        /// <summary>
        ///   Returns the <see cref="Base32"/> encoding of the <see cref="MultiHash"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="Base32"/> representation of the <see cref="MultiHash"/>.
        /// </returns>
        public string ToBase32()
        {
            return ToArray().ToBase32();
        }

        /// <summary>
        ///   Returns the libp2p binary representation as a byte array.
        /// </summary>
        /// <returns>
        ///   A byte array.
        /// </returns>
        /// <remarks>
        ///   The binary representation is a sequence of <see cref="MultiHash"/>.
        /// </remarks>
        public byte[] ToArray()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///   Determines if the data matches the hash.
        /// </summary>
        /// <param name="data">
        ///   The data to check.
        /// </param>
        /// <returns>
        ///   <b>true</b> if the data matches the <see cref="MultiHash"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <remarks>
        ///   <b>Matches</b> is used to ensure data integrity.
        /// </remarks>
        public bool Matches(byte[] data)
        {
            var hasher = Coder.Hasher();
            if(hasher is null)
            {
                return false;
            }

            var digest = hasher.ComputeHash(data);
            for (int i = digest.Length - 1; 0 <= i; --i)
            {
                if (digest[i] != Digest[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///   Determines if the stream data matches the hash.
        /// </summary>
        /// <param name="data">
        ///   The <see cref="Stream"/> containing the data to check.
        /// </param>
        /// <returns>
        ///   <b>true</b> if the data matches the <see cref="MultiHash"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <remarks>
        ///   <b>Matches</b> is used to ensure data integrity.
        /// </remarks>
        public bool Matches(Stream data)
        {
            var hasher = Coder.Hasher();
            if(hasher is null)
            {
                return false;
            }

            var digest = hasher.ComputeHash(data);
            for (int i = digest.Length - 1; 0 <= i; --i)
            {
                if (digest[i] != Digest[i])
                {
                    return false;
                }
            }
            return true;
        }
         
        /// <summary>
        ///   Generate the multihash for the specified byte array.
        /// </summary>
        /// <param name="data">
        ///   The byte array containing the data to hash.
        /// </param>
        /// <param name="algorithmName">
        ///   The name of the hashing algorithm to use; defaults to <see cref="DefaultAlgorithmName"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiHash"/> for the <paramref name="data"/>.
        /// </returns>
        public static MultiHash ComputeHash(byte[] data, string algorithmName = DefaultAlgorithmName)
        {
            using (var alg = GetHashAlgorithm(algorithmName))
            {
                return new MultiHash(algorithmName, alg.ComputeHash(data));
            }
        }

        /// <summary>
        ///   Generate the multihash for the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="data">
        ///   The <see cref="Stream"/> containing the data to hash.
        /// </param>
        /// <param name="algorithmName">
        ///   The name of the hashing algorithm to use; defaults to <see cref="DefaultAlgorithmName"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="MultiHash"/> for the <paramref name="data"/>.
        /// </returns>
        public static MultiHash ComputeHash(Stream data, string algorithmName = DefaultAlgorithmName)
        {
            using (var alg = GetHashAlgorithm(algorithmName))
            {
                return new MultiHash(algorithmName, alg.ComputeHash(data));
            }
        }
    }
}
