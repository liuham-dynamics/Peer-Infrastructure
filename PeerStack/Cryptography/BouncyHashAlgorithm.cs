using System.Security.Cryptography;

namespace PeerStack.Cryptography
{
    /// <summary>
    ///   A thin wrapper around the bouncy castle digests.
    /// </summary>
    /// <remarks>
    ///   Makes a Bouncy Castle IDigest speak .Net HashAlgorithm.
    /// </remarks>
    /// <remarks>
    ///   Wrap the bouncy castle Digest.
    /// </remarks>
    internal sealed class BouncyHashAlgorithm(Org.BouncyCastle.Crypto.IDigest digest) : HashAlgorithm
    {
        /// <inheritdoc/>
        private readonly Org.BouncyCastle.Crypto.IDigest Digest = digest;

        /// <inheritdoc/>
        public override void Initialize()
        {
            Digest.Reset();
        }

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            Digest.BlockUpdate(array, ibStart, cbSize);
        }

        /// <inheritdoc/>
        protected override byte[] HashFinal()
        {
            var output = new byte[Digest.GetDigestSize()];
            Digest.DoFinal(output, 0);
            return output;
        }
    }
}
