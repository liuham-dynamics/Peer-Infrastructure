using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Multiformat
{
    /// <summary>
    ///   Self identifying base encodings.
    /// </summary>
    /// <remarks>
    ///   <b>MultiBase</b> is a protocol for distinguishing base encodings
    ///   and other simple string encodings.
    ///   See the <see cref="MultiBaseAlgorithm">registry</see> for supported algorithms.
    /// </remarks>
    /// <seealso href="https://github.com/multiformats/multibase"/>
    public static class MultiBase 
    {
        /// <summary>
        ///   The default multi-base algorithm is "base58btc".
        /// </summary>
        public const string DefaultAlgorithmName = "base58btc";

        /// <summary>
        ///   Gets the <see cref="MultiBaseCoder"/> with the specified multibase code.
        /// </summary>
        /// <param code="code">
        ///   The code of an codec or algorithm, see
        ///   <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/> for defined names.
        /// </param>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref code="code"/> is not registered.
        /// </exception>
        private static MultiBaseCoder GetAlgorithm(char code)
        {
            var codec = MultiBaseCoder.Codecs.FirstOrDefault(o => o.Code == code);
            if (codec is null | string.IsNullOrEmpty(codec?.Name))
            {
                throw new KeyNotFoundException($"Multibase algorithm '{code}' is not registered.");
            }
            return codec!;
        }
        
        /// <summary>
        ///   Gets the <see cref="MultiBaseCoder"/> with the specified multibase code.
        /// </summary>
        /// <param code="name">
        ///   The code of an codec or algorithm, see
        ///   <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/> for defined names.
        /// </param>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref code="name"/> is not registered.
        /// </exception>
        private static MultiBaseCoder GetAlgorithm(string name)
        {
            var codec = MultiBaseCoder.Codecs.FirstOrDefault(o => o.Name == name);
            if (codec is null | string.IsNullOrEmpty(codec?.Name))
            {
                throw new KeyNotFoundException($"Multibase algorithm '{name}' is not registered.");
            }
            return codec!;
        }

        /// <summary>
        ///   Converts an array of 8-bit unsigned integers to its equivalent string representation.
        /// </summary>
        /// <param code="bytes">
        ///   An array of 8-bit unsigned integers.
        /// </param>
        /// <param code="algorithmName">
        ///   The code of the multi-base algorithm to use. See <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> starting with the algorithm's <see cref="MultiBaseAlgorithm.Code"/> and
        ///   followed by the encoded string representation of the <paramref code="bytes"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref code="algorithmName"/> is not registered.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   When <paramref code="bytes"/> fails to be encoded into a string representation.
        /// </exception>
        public static string Encode(byte[] bytes, string algorithmName = DefaultAlgorithmName)
        {
            //
            ArgumentNullException.ThrowIfNull(bytes);

            var codec = GetAlgorithm(algorithmName);
            var encoded = codec!.Encode(bytes);
            if (string.IsNullOrEmpty(encoded))
            {
                throw new InvalidOperationException(encoded);
            }
            return codec.Code + encoded;
        }

        /// <summary>
        ///   Converts the specified <see cref="string"/>, which encodes binary data,
        ///   to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param code="s">
        ///   The multi-base string to convert.
        /// </param>
        /// <returns>
        ///   An array of 8-bit unsigned integers that is equivalent to <paramref code="s"/>.
        /// </returns>
        /// <exception cref="FormatException">
        ///   When the <paramref code="s"/> can not be decoded.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   When <paramref code="s"/> fails to be decoded into a byte array representation.
        /// </exception>
        public static byte[] Decode(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(nameof(s));
            }

            //
            try
            {
                MultiBaseCoder codec = GetAlgorithm(s[0]);
                var decoded = codec!.Decode(s[1..]);
                if(decoded is null | decoded?.Length == 0)
                {
                    throw new InvalidOperationException(nameof(decoded));
                }
                return decoded!;
            }
            catch (Exception e)
            {
                throw new FormatException($"MultiBase '{s}' is invalid; decode failed.", e);
            }
        }
    }
}
