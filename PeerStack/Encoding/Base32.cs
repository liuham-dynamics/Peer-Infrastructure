using SimpleBase;

namespace PeerStack.Encoding
{
    /// <summary>
    ///   A codec for Base-32 and  Base-32z.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   A codec for Base-32, <see cref="Encode"/> and <see cref="Decode"/>. 
    ///   Adds the extension method <see cref="ToBase32"/>
    ///   to encode a byte array and <see cref="FromBase32"/> to decode a Base-32 string.
    ///   </para>
    ///   <para>
    ///   <see cref="Encode"/> and <see cref="ToBase32"/> produce the lowercase form of
    ///   <see href="https://tools.ietf.org/html/rfc4648"/> with no padding.
    ///   <see cref="Decode"/> and <see cref="FromBase32"/> are case-insensitive and
    ///   allow optional padding.
    ///   </para>
    ///   <para>
    ///   A thin wrapper around <see href="https://github.com/ssg/SimpleBase"/>.
    ///   </para>
    /// </remarks>
    public static class Base32
    {
        /// <summary>
        /// Base32 Alphabets for x-base-32 encoding/decoding
        /// </summary>
        private static readonly Base32Alphabet alphabet = new("ybndrfg8ejkmcpqxot1uwisza345h769");


        /// <summary>
        ///   The encoder/decoder for x-base-32.
        /// </summary>
        public static SimpleBase.Base32 Base32xCodec => new(alphabet);
         
        /// <summary>
        ///   The encoder/decoder for z-base-32.
        /// </summary>
        public static SimpleBase.Base32 Base32zCodec => SimpleBase.Base32.ZBase32;
         
        /// <summary>
        ///   The encoder/decoder for base-32.
        /// </summary>
        public static SimpleBase.Base32 Base32Codec => SimpleBase.Base32.Rfc4648;


        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is
        /// encoded with RFC4648 base-32 characters.
        /// </summary>
        /// <param name="bytes">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation, in RFC4648 base 32, of the contents of <paramref name="bytes"/>.</returns>
        public static string Encode(byte[] bytes)
        {
            return SimpleBase.Base32.Rfc4648.Encode(bytes, false).ToLowerInvariant();
        }
        
        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is
        /// encoded with RFC4648 base-32 characters.
        /// </summary>
        /// <param name="bytes">An span of 8-bit unsigned integers.</param>
        /// <returns>The string representation, in RFC4648 base 32, of the contents of <paramref name="bytes"/>.</returns>
        public static string Encode(ReadOnlySpan<byte> bytes)
        {
            return SimpleBase.Base32.Rfc4648.Encode(bytes, false).ToLowerInvariant();
        }

        /// <summary>
        /// Converts a stream to TextWriter representation that is encoded with RFC4648 base-32 characters.
        /// </summary>
        /// <param name="stream">Input binary stream.</param>
        /// <param name="writer">A text writer the output is written to.</param>
        /// <returns>Returns a TextWriter in RFC4648 base 32, of the contents of <paramref name="stream"/>.</returns>
        public async static Task<TextWriter> EncodeAsync(Stream stream, TextWriter writer)
        {
            await SimpleBase.Base32.Rfc4648.EncodeAsync(stream, writer, false).ConfigureAwait(false);
            return writer;
        }


        /// <summary>
        /// Converts the specified <see cref="string"/>, which encodes binary data as 
        /// an RFC4648 base-32 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The RFC4648 base-32 string to convert.</param>
        /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> is not a valid RFC4648 base-32 encoded string.</exception>
        public static byte[] Decode(string input)
        {
            try
            {
                return [.. SimpleBase.Base32.Rfc4648.Decode(input)];
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new ArgumentException("Invalid base-32 encoded string.", nameof(input), ex);
            }
        }
        

        /// <summary>
        /// Converts the specified <see cref="string"/>, which encodes binary data as 
        /// an RFC4648 base-32 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The RFC4648 base-32 character span to convert.</param>
        /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> is not a valid RFC4648 base-32 encoded string.</exception>
        public static byte[] Decode(ReadOnlySpan<char> input)
        {
            try
            {
                return [.. SimpleBase.Base32.Rfc4648.Decode(input)];
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new ArgumentException("Invalid base-32 encoded string.", nameof(input), ex);
            }
        }

        /// <summary>
        /// Converts the specified <see cref="TextReader"/>, which encodes binary data as 
        /// an RFC4648 base-32 digits, to an equivalent stream representation.
        /// </summary>
        /// <param name="reader">The RFC4648 base-32 TextReader to convert.</param>
        /// <param name="stream">The output stream to convert.</param>
        /// <returns>An RFC4648 base32 decode stream that is equivalent to <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="reader"/> is not a valid RFC4648 base-32 encoded text.</exception>
        public async static Task<Stream> DecodeAsync(TextReader reader, Stream stream)
        {
            try
            {
                await SimpleBase.Base32.Rfc4648.DecodeAsync(reader, stream).ConfigureAwait(false);
                return stream;
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new ArgumentException("Invalid base-32 encoded string.", nameof(reader), ex);
            }
        }

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string 
        /// representation that is encoded with RFC4648 base-32 characters.
        /// </summary>
        /// <param name="bytes">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation, in RFC4648 base 32, of the contents of <paramref name="bytes"/>.</returns>
        public static string ToBase32(this byte[] bytes)
        {
            return Encode(bytes);
        }
        
        /// <summary>
        /// Converts a span of 8-bit unsigned integers to its equivalent string 
        /// representation that is encoded with RFC4648 base-32 characters.
        /// </summary>
        /// <param name="bytes">An span of 8-bit unsigned integers.</param>
        /// <returns>The string representation, in RFC4648 base 32, of the contents of <paramref name="bytes"/>.</returns>
        public static string ToBase32(this ReadOnlySpan<byte> bytes)
        {
            return Encode(bytes);
        }

        /// <summary>
        /// Converts the specified <see cref="string"/>, which encodes binary data as 
        /// an RFC4648 base-32 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The RFC4648 base-32 string to convert; case-insensitive and allows padding.</param>
        /// <returns>
        /// An array of 8-bit unsigned integers that is equivalent to <paramref name="input"/>.
        /// </returns>
        public static byte[] FromBase32(this string input)
        {
            return Decode(input);
        }

        /// <summary>
        /// Converts the specified <see cref="ReadOnlySpan<char>"/>, which encodes binary data as 
        /// an RFC4648 base-32 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The RFC4648 base-32 string to convert; case-insensitive and allows padding.</param>
        /// <returns>
        /// An array of 8-bit unsigned integers that is equivalent to <paramref name="input"/>.
        /// </returns>
        public static byte[] FromBase32(this ReadOnlySpan<char> input)
        {
            return Decode(input);
        }
    }
}
