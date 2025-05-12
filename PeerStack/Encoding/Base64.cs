using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Encoding
{
    /// <summary>
    ///     A codec for Base-64 (RFC 4648) with no padding and Base-64 URL (RFC 4648).
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     A codec for Base-64, <see cref="EncodeNoPad"/> and <see cref="DecodeNoPad"/>.  Adds the extension method <see cref="ToBase64NoPad"/>
    ///     to encode a byte array and <see cref="FromBase64NoPad"/> to decode a Base-64 string.
    ///   </para>
    ///   <para>
    ///     A codec for Base-64 URL, <see cref="EncodeUrl"/> and <see cref="DecodeUrl"/>.  Adds the extension method <see cref="ToBase64Url"/>
    ///     to encode a byte array and <see cref="FromBase64Url"/> to decode a Base-64 URL string.
    ///   </para>
    ///   <para>
    ///     The original code for Base64URL was found at <see href="https://brockallen.com/2014/10/17/base64url-encoding/"/>.
    ///   </para>
    /// </remarks>
    public static class Base64
    {
        /// <summary>
        ///   Converts an array of bytes to its equivalent string representation that is
        ///   encoded with base-64 characters without padding.
        /// </summary>
        /// <param name="bytes">
        ///   An array of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 64, of the contents of <paramref name="bytes"/> without padding.
        /// </returns>
        public static string EncodeNoPad(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=');
        }

        /// <summary>
        ///   Converts the specified base-64 string, which encodes binary data as base 64 digits,
        ///   to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-64 string to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] DecodeNoPad(string s)
        {
            // consider
            switch (s.Length % 4)
            {
                case 0: break;            // No pad chars in this case
                case 2: s += "=="; break;  // Two pad chars
                case 3: s += "="; break;   // One pad char
                default: throw new Exception("Invalid base64 string.");
            }

            // Standard base64 decoder
            return Convert.FromBase64String(s);
        }

        /// <summary>
        ///   Converts an array of bytes to its equivalent string representation that is
        ///   encoded with base-64 characters without padding.
        /// </summary>
        /// <param name="bytes">
        ///   An array of bytes.
        /// </param>
        /// <returns>
        ///   The string representation, in base 64, of the contents of <paramref name="bytes"/> without padding.
        /// </returns>
        public static string ToBase64NoPad(this byte[] bytes)
        {
            return EncodeNoPad(bytes);
        }

        /// <summary>
        ///   Converts the specified base-64 string, which encodes binary data as base 64 digits,
        ///   to an equivalent array of bytes.
        /// </summary>
        /// <param name="s">
        ///   The base-64 string to convert.
        /// </param>
        /// <returns>
        ///   An array of bytes that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] FromBase64NoPad(this string s)
        {
            return DecodeNoPad(s);
        }

        /// <summary>
        ///   Converts an array of 8-bit unsigned integers to its equivalent string 
        ///   representation that is encoded with base-64 URL characters.
        /// </summary>
        /// <param name="bytes">
        ///   An array of 8-bit unsigned integers.
        /// </param>
        /// <returns>
        ///   The string representation, in base 64 URL, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string EncodeUrl(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);

            // Replace URL-unsafe characters
            string urlSafeBase64 = base64.TrimEnd('=')
                                         .Replace('+', '-')
                                         .Replace('/', '_');
            // return
            return urlSafeBase64;
        }

        /// <summary>
        ///   Converts the specified <see cref="string"/>, which encodes binary data as base 64 URL characters,
        ///   to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="s">
        ///   The base 64 URL string to convert.
        /// </param>
        /// <returns>
        ///   An array of 8-bit unsigned integers that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] DecodeUrl(string s)
        {
            // Replace URL-safe characters
            string base64 = s.Replace('-', '+')
                             .Replace('_', '/');

            // Add padding if necessary
            int paddingLength = base64.Length % 4;
            if (paddingLength == 2)
            {
                base64 += "==";
            }
            else if (paddingLength == 3)
            {
                base64 += "=";
            }

            return Convert.FromBase64String(base64);
        }

        /// <summary>
        ///   Converts an array of 8-bit unsigned integers to its equivalent string representation that is
        ///   encoded with base-64 URL characters.
        /// </summary>
        /// <param name="bytes">
        ///   An array of 8-bit unsigned integers.
        /// </param>
        /// <returns>
        ///   The string representation, in base 64 URL, of the contents of <paramref name="bytes"/>.
        /// </returns>
        public static string ToBase64Url(this byte[] bytes)
        {
            return EncodeUrl(bytes);
        }

        /// <summary>
        ///   Converts the specified <see cref="string"/>, which encodes binary data as base 64 URL characters,
        ///   to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="s">
        ///   The base 64 URL string to convert.
        /// </param>
        /// <returns>
        ///   An array of 8-bit unsigned integers that is equivalent to <paramref name="s"/>.
        /// </returns>
        public static byte[] FromBase64Url(this string s)
        {
            return DecodeUrl(s);
        }
    }
}
