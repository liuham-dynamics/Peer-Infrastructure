using System.Security.Cryptography;

namespace PeerStack.Cryptography
{
    /// <inheritdoc/>
    internal sealed class DoubleSha256HashAlgorithm : HashAlgorithm
    {
        //
        private readonly HashAlgorithm Digest = SHA256.Create();
        private byte[]? RoundComputedHash;

        /// <inheritdoc/>
        public override void Initialize()
        {
            Digest.Initialize();
            RoundComputedHash = null;
        }

        /// <inheritdoc/>
        public override int HashSize => Digest.HashSize;

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (RoundComputedHash != null)
            {
                throw new NotSupportedException("Compute hash already called.");
            }

            RoundComputedHash = Digest.ComputeHash(array, ibStart, cbSize);
        }

        /// <inheritdoc/>
        protected override byte[] HashFinal()
        {
            Digest.Initialize();
            return Digest.ComputeHash(buffer: RoundComputedHash?? []);
        }
    }
}
