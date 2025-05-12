using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PeerStack.Cryptography
{
    internal abstract class AKeccakHashAlgorithm : HashAlgorithm
    {
  
        public const int KeccakB = 1600;
        public const int KeccakNumberOfRounds = 24;
        public const int KeccakLaneSizeInBits = 8 * 8;

        public readonly ulong[] RoundConstants;

        protected ulong[] state;
        protected byte[] buffer;
        protected int buffLength;
 
        protected int keccakR;

        /// <inheritdoc/>
        public int KeccakR
        {
            get => keccakR;
            protected set => keccakR = value;
        }

        /// <inheritdoc/>
        public int SizeInBytes => KeccakR / 8;

        /// <inheritdoc/>
        public int HashByteLength => HashSizeValue / 8;

        /// <inheritdoc/>
        public override bool CanReuseTransform => true;

        /// <inheritdoc/>
        public override byte[] Hash => HashValue;

        /// <inheritdoc/>
        public override int HashSize => HashSizeValue;

        /// <inheritdoc/>
        protected AKeccakHashAlgorithm(int hashBitLength)
        {
              
            if (hashBitLength != 224 && hashBitLength != 256 && hashBitLength != 384 && hashBitLength != 512)
            {
                throw new ArgumentException("Hash bit length must be 224, 256, 384, or 512", nameof(hashBitLength));
            }

            state = [];
            buffer = [];

            Initialize();

            HashSizeValue = hashBitLength;
            switch (hashBitLength)
            {
                case 224:
                    KeccakR = 1152;
                    break;
                case 256:
                    KeccakR = 1088;
                    break;
                case 384:
                    KeccakR = 832;
                    break;
                case 512:
                    KeccakR = 576;
                    break;
            }
           
            RoundConstants =
            [
                0x0000000000000001UL,
                0x0000000000008082UL,
                0x800000000000808aUL,
                0x8000000080008000UL,
                0x000000000000808bUL,
                0x0000000080000001UL,
                0x8000000080008081UL,
                0x8000000000008009UL,
                0x000000000000008aUL,
                0x0000000000000088UL,
                0x0000000080008009UL,
                0x000000008000000aUL,
                0x000000008000808bUL,
                0x800000000000008bUL,
                0x8000000000008089UL,
                0x8000000000008003UL,
                0x8000000000008002UL,
                0x8000000000000080UL,
                0x000000000000800aUL,
                0x800000008000000aUL,
                0x8000000080008081UL,
                0x8000000000008080UL,
                0x0000000080000001UL,
                0x8000000080008008UL
            ];
        }

        /// <inheritdoc/>
        protected ulong ROL(ulong a, int offset)
        {
            return (((a) << ((offset) % KeccakLaneSizeInBits)) ^ ((a) >> (KeccakLaneSizeInBits - ((offset) % KeccakLaneSizeInBits))));
        }

        /// <inheritdoc/>
        protected void AddToBuffer(byte[] array, ref int offset, ref int count)
        {
            int amount = Math.Min(count, buffer.Length - buffLength);
            Buffer.BlockCopy(array, offset, buffer, buffLength, amount);
            offset += amount;
            buffLength += amount;
            count -= amount;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            buffLength = 0;
            state = new ulong[5 * 5];//1600 bits
            HashValue = null;
        }

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            ArgumentNullException.ThrowIfNull(array);
            if (ibStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ibStart));
            }
           else if (cbSize > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(cbSize));
            }
            else if (ibStart + cbSize > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
 
    }
}
