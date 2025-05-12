
namespace PeerStack.Encoding
{
    /// <summary>
    ///   A Base58 codec 
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   A codec for Base-58, <see cref="Encode"/> and <see cref="Decode"/>.  
    ///   Adds the extension method <see cref="ToBase58"/> to encode a byte 
    ///   array and <see cref="FromBase58"/> to decode a Base-58 string.
    ///   </para>
    ///   <para>
    ///   This is just a thin wrapper of <see href="https://github.com/ssg/SimpleBase"/>.
    ///   </para>
    ///   <para>
    ///   This codec uses the BitCoin alphabet <b>not Flickr's</b>.
    ///   </para>
    /// </remarks>
    public static class Base58
    {
        /// <summary>
        ///   Converts an array of bytes to its equivalent string representation that is
        ///   encoded with base-58 characters.
        /// </summary>
        /// <param name="bytes">
        ///   An array of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 58, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string Encode(byte[] bytes)
        {
            return SimpleBase.Base58.Bitcoin.Encode(bytes);
        }
        
        /// <summary>
        ///   Converts a span of bytes to its equivalent string representation that is
        ///   encoded with base-58 characters.
        /// </summary>
        /// <param name="bytes">
        ///   A span of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 58, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string Encode(ReadOnlySpan<byte> bytes)
        {
            return SimpleBase.Base58.Bitcoin.Encode(bytes);
        }

        /// <summary>
        ///   Converts the specified base-58 string to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-58 string to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] Decode(string s)
        {
            return SimpleBase.Base58.Bitcoin.Decode(s);
        }
        
        /// <summary>
        ///   Converts the specified base-58 span of characters to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-58 character span to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] Decode(ReadOnlySpan<char> s)
        {
            return SimpleBase.Base58.Bitcoin.Decode(s);
        }



        /// <summary>
        ///   Converts an array of bytes to its equivalent string representation that is
        ///   encoded with base-58 characters.
        /// </summary>
        /// <param name="bytes">
        ///   An array of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 58, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string ToBase58(this byte[] bytes)
        {
            return Encode(bytes);
        }
        

        /// <summary>
        ///   Converts a span of bytes to its equivalent string representation that is
        ///   encoded with base-58 characters.
        /// </summary>
        /// <param name="bytes">
        ///   A span of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 58, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string ToBase58(this ReadOnlySpan<byte> bytes)
        {
            return Encode(bytes);
        }


        /// <summary>
        ///   Converts the specified base-58 string to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-58 string to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] FromBase58(this string s)
        {
            return Decode(s);
        }

        /// <summary>
        /// Converts the specified base-58 span of characters to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-58 span of characters to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] FromBase58(this ReadOnlySpan<char> s)
        {
            return Decode(s);
        }
    }
}
