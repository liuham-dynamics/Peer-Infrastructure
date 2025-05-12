using Org.BouncyCastle.Crypto.Digests;
using PeerStack.Cryptography;
using System.Security.Cryptography;

namespace PeerStack.Multiformat
{
    /// <summary>
    ///   Metadata and implementations of a multi-hashing algorithms.
    /// </summary>
    /// <remarks>
    ///   Multiformat assigns a unique <see cref="Name"/> and <see cref="Code"/> to a hashing algorithm.
    ///   See <see href="https://github.com/multiformats/multicodec/blob/master/table.csv">hashtable.csv</see>
    ///   for the currently defined hashing algorithms.
    ///   <para>
    ///   These algorithms are implemented:
    ///   <list type="bullet">
    ///   <item><description>blake2b-160, blake2b-256 blake2b-384 and blake2b-512</description></item>
    ///   <item><description>blake2s-128, blake2s-160, blake2s-224 a nd blake2s-256</description></item>
    ///   <item><description>keccak-224, keccak-256, keccak-384 and keccak-512</description></item>
    ///   <item><description>md4 and md5</description></item>
    ///   <item><description>sha1</description></item>
    ///   <item><description>sha2-256, sha2-512 and dbl-sha2-256</description></item>
    ///   <item><description>sha3-224, sha3-256, sha3-384 and sha3-512</description></item>
    ///   <item><description>shake-128 and shake-256</description></item>
    ///   </list>
    ///   </para>
    ///   <para>
    ///   The <c>identity</c> hash is also implemented;  which just returns the input bytes.
    ///   This is used to inline a small amount of data into a <see cref="Cid"/>.
    ///   </para>
    ///   <para>
    ///     Use <see cref="Register(string, int, int, Func{HashAlgorithm})"/> to add a new
    ///     hashing algorithm.
    ///   </para>
    /// </remarks>
    public sealed class MultiHashCoder : IMultiFormatCoder
    {
        //
        internal static HashSet<MultiHashCoder> _memberHashes = [];
        internal static Dictionary<string, string> _memberHashAllies = [];

        /// <summary>
        ///   The multi-hash code number assigned to the hashing algorithm.
        /// </summary>
        /// <value>
        ///   Valid hash codes at <see href="https://github.com/multiformats/multicodec/blob/master/table.csv">hashtable.csv</see>.
        /// </value>
        public int Code { get; private set; }

        /// <summary>
        ///   The multi-hash name of the algorithm.
        /// </summary>
        /// <value>
        ///   A unique name.
        /// </value>
        public string? Name { get; private set; }

        /// <summary>
        ///   The size, in bytes, of the Digest value.
        /// </summary>
        /// <value>
        ///   The Digest value size in bytes. Zero indicates that the Digest is non fixed.
        /// </value>
        public int DigestSize { get; private set; }

        /// <summary>
        /// The specific multiformat type
        /// </summary>
        public MultiFormatType FormatType => MultiFormatType.MultiHash;

        /// <summary>
        ///   Returns a cryptographic hash algorithm that can compute a hash (Digest).
        /// </summary>
        public Func<HashAlgorithm>? Hasher { get; private set; }

        /// <summary>
        ///   A set consisting of all <see cref="MultiHashCoder">hashing algorithms</see>.
        /// </summary>
        /// <value>
        ///   The currently registered hashing algorithms.
        /// </value>
        public static IEnumerable<MultiHashCoder> Hashes => _memberHashes;

        /// <summary>
        ///   Register the standard hash algorithms common for libp2p and IPFS.
        /// </summary>
        /// <seealso href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        static MultiHashCoder()
        {
            Register("identity", 0, 0, () => new IdentityHashAlgorithm());
            RegisterAlias("id", "identity");

            Register("md4", 0xd4, 128 / 8, () => new BouncyHashAlgorithm(new MD4Digest()));
            Register("md5", 0xd5, 128 / 8, MD5.Create);

            Register("sha1", 0x11, 20, SHA1.Create);
            Register("sha2-256", 0x12, 32, SHA256.Create);
            Register("sha2-512", 0x13, 64, SHA512.Create);

            Register("dbl-sha2-256", 0x56, 32, () => new DoubleSha256HashAlgorithm());

            Register("keccak-224", 0x1A, 224 / 8, () => new KeccakHashAlgorithm(224));
            Register("keccak-256", 0x1B, 256 / 8, () => new KeccakHashAlgorithm(256));
            Register("keccak-384", 0x1C, 384 / 8, () => new KeccakHashAlgorithm(384));
            Register("keccak-512", 0x1D, 512 / 8, () => new KeccakHashAlgorithm(512));

            Register("sha3-224", 0x17, 224 / 8, () => new BouncyHashAlgorithm(new Sha3Digest(224)));
            Register("sha3-256", 0x16, 256 / 8, () => new BouncyHashAlgorithm(new Sha3Digest(256)));
            Register("sha3-384", 0x15, 384 / 8, () => new BouncyHashAlgorithm(new Sha3Digest(384)));
            Register("sha3-512", 0x14, 512 / 8, () => new BouncyHashAlgorithm(new Sha3Digest(512)));

            Register("shake-128", 0x18, 128 / 8, () => new BouncyHashAlgorithm(new ShakeDigest(128)));
            Register("shake-256", 0x19, 256 / 8, () => new BouncyHashAlgorithm(new ShakeDigest(256)));

            Register("blake2b-160", 0xb214, 160 / 8, () => new BouncyHashAlgorithm(new Blake2bDigest(160)));
            Register("blake2b-256", 0xb220, 256 / 8, () => new BouncyHashAlgorithm(new Blake2bDigest(256)));
            Register("blake2b-384", 0xb230, 384 / 8, () => new BouncyHashAlgorithm(new Blake2bDigest(384)));
            Register("blake2b-512", 0xb240, 512 / 8, () => new BouncyHashAlgorithm(new Blake2bDigest(512)));
            Register("blake2s-128", 0xb250, 128 / 8, () => new BouncyHashAlgorithm(new Blake2sDigest(128)));
            Register("blake2s-160", 0xb254, 160 / 8, () => new BouncyHashAlgorithm(new Blake2sDigest(160)));
            Register("blake2s-224", 0xb25c, 224 / 8, () => new BouncyHashAlgorithm(new Blake2sDigest(224)));
            Register("blake2s-256", 0xb260, 256 / 8, () => new BouncyHashAlgorithm(new Blake2sDigest(256)));
          
        }

        /// <summary>
        ///   Use <see cref="Register"/> to create a new instance of a <see cref="MultiHashCoder"/>.
        /// </summary>
        private MultiHashCoder()
        {
            Name = string.Empty;
            Hasher = null;
             
        }

        /// <summary>
        ///   Register a new IPFS hashing algorithm.
        /// </summary>
        /// <param name="name">
        ///   The name of the algorithm.
        /// </param>
        /// <param name="code">
        ///   The IPFS number assigned to the hashing algorithm.
        /// </param>
        /// <param name="digestSize">
        ///   The size, in bytes, of the Digest value.
        /// </param>
        /// <param name="hasher">
        ///   A <c>Func</c> that returns a <see cref="HashAlgorithm"/>.  If not specified, then a <c>Func</c> is created to
        ///   throw a <see cref="NotImplementedException"/>.
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiHashCoder"/>.
        /// </returns>
        public static MultiHashCoder Register(string name, int code, int digestSize, Func<HashAlgorithm> hasher = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            hasher ??= () => throw new NotImplementedException(string.Format("The hashing algorithm '{0}' is not implemented.", name));

            var a = new MultiHashCoder
            {
                Name = name,
                Code = code,
                DigestSize = digestSize,
                Hasher = hasher
            };

            _memberHashes.Add(a);

            return a;
        }

        /// <summary>
        ///   Register an alias for an IPFS hashing algorithm.
        /// </summary>
        /// <param name="alias">
        ///   The alias name.
        /// </param>
        /// <param name="name">
        ///   The name of the existing algorithm.
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiHashCoder"/>.
        /// </returns>
        public static MultiHashCoder RegisterAlias(string alias, string name)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentNullException(nameof(alias));
            }

            //
            var hash = _memberHashes.FirstOrDefault(o => o.Name == name);
          if(hash is null | string.IsNullOrEmpty(hash?.Name))
            {
                throw new ArgumentException(string.Format("The hashing algorithm '{0}' is not defined.", name));
            }

            _memberHashAllies.Add(alias, name);
             
            return hash!;
        }

        /// <summary>
        ///   Remove an hashing algorithm from the registry.
        /// </summary>
        /// <param name="algorithm">
        ///   The <see cref="MultiHashCoder"/> to remove.
        /// </param>
        public static void Deregister(MultiHashCoder algorithm)
        {
           _memberHashes.Remove(algorithm);
        }

        /// <summary>
        ///   Gets the <see cref="MultiHashCoder"/> with the specified multi-hash name.
        /// </summary>
        /// <param name="name">
        ///   The name of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/> for the defined names.
        /// </param>
        /// <returns>
        ///   The hashing implementation associated with the <paramref name="name"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static HashAlgorithm GetAlgorithm(string name)
        {
            //
            var coder = GetAlgorithmMetadata(name);
            if(coder.Hasher is null)
            {
                throw new NullReferenceException(nameof(coder.Hasher));
            }

            return coder.Hasher();
        }

        /// <summary>
        ///   Gets the metadata with the specified IPFS multi-hash name.
        /// </summary>
        /// <param name="name">
        ///   The name of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        ///   for IPFS defined names.
        /// </param>
        /// <returns>
        ///   The metadata associated with the hashing <paramref name="name"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static MultiHashCoder GetAlgorithmMetadata(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var hash = _memberHashes.FirstOrDefault(o => o.Name == name);
            if(hash is null | string.IsNullOrEmpty(hash?.Name))
            {
                throw new KeyNotFoundException($"Hash algorithm '{name}' is not registered.");
            }

            return hash!;
        }
         
        /// <summary>
        ///   The <see cref="Name"/> of the hashing algorithm.
        /// </summary>
        public override string ToString()
        {
            return Name ?? string.Empty; ;
        }

    }
}
