using System.Security.Cryptography;

namespace PeerStack.Cryptography
{
    /// <inheritdoc/>
    internal sealed class IdentityHashAlgorithm : HashAlgorithm
    {
        /// <inheritdoc/>
        private byte[]? Digest;

        /// <inheritdoc/>
        public override void Initialize()
        {
            Digest = null;
        }

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (Digest == null)
            {
                Digest = new byte[cbSize];
                Buffer.BlockCopy(array, ibStart, Digest, 0, cbSize);
                return;
            }

            var buffer = new byte[Digest.Length + cbSize];
            Buffer.BlockCopy(Digest, 0, buffer, Digest.Length, Digest.Length);
            Buffer.BlockCopy(array, ibStart, Digest, Digest.Length, cbSize);
            Digest = buffer;
        }

        /// <inheritdoc/>
        protected override byte[] HashFinal()
        {
            return Digest?? [];
        }
    }
}
