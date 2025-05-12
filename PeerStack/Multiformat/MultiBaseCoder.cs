using PeerStack.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Multiformat
{
    /// <summary>
    ///   Multibase is codec protocol for disambiguating the "base encoding" used to 
    ///   express binary data in text formats from the expression alone.
    /// </summary>
    /// <remarks>
    ///   Multibase assigns codec unique <see cref="Name"/> and <see cref="Code"/> to each codec.
    ///   See <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/> for
    ///   the currently defined multi-base algorithms.
    ///   <para>
    ///   The commonly supported algorithms or codecs are: base58btc, base58flickr, base64,
    ///   base64pad, base64url, base16, base32, base32z, base32pad, base32hex, and base32hexpad.
    ///   </para>
    /// </remarks>
    public sealed class MultiBaseCoder : IMultiFormatCoder
    {
        // 
        private static HashSet<MultiBaseCoder> _memberCodecs = [];

        /// <summary>
        ///   A collection consisting of all algorithms.
        /// </summary>
        public static IEnumerable<MultiBaseCoder> Codecs => _memberCodecs;


        /// <summary>
        ///   The multibase code assigned to the codec.
        /// </summary>
        /// <value>
        ///   Valid codes at <see href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/>.
        /// </value>
        public char Code { get; private set; }

        /// <summary>
        ///   The name of the codec.
        /// </summary>
        /// <value>
        ///   A unique name.
        /// </value>
        public string? Name { get; private set; }

        /// <summary>
        /// The specific multiformat type
        /// </summary>
        public MultiFormatType FormatType => MultiFormatType.MultiBase;

        /// <summary>
        ///   Returns codec function that can return codec string from codec byte array.
        /// </summary>
        public Func<byte[], string>? Encode { get; private set; }

        /// <summary>
        ///   Returns codec function that can return codec byte array from codec string.
        /// </summary>
        public Func<string, byte[]>? Decode { get; private set; }


        /// <summary>
        ///   Register the standard multi-base codecs for libp2p and multibase.
        /// </summary>
        /// <seealso href="https://github.com/multiformats/multibase/blob/master/multibase.csv"/>
        static MultiBaseCoder()
        {
            Register("base16", 'f', bytes => SimpleBase.Base16.LowerCase.Encode(bytes), text => SimpleBase.Base16.Decode(text).ToArray());
            Register("BASE16", 'F', bytes => SimpleBase.Base16.UpperCase.Encode(bytes), text => SimpleBase.Base16.Decode(text).ToArray());
          
            Register("BASE32", 'B', bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, false), text => SimpleBase.Base32.Rfc4648.Decode(text));
            Register("base32z", 'h', bytes => Base32.Base32zCodec.Encode(bytes, false), text => Base32.Base32zCodec.Decode(text));
            Register("base32", 'b', bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, false).ToLowerInvariant(), text => SimpleBase.Base32.Rfc4648.Decode(text));
            Register("BASE32PAD", 'C', bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, true), text => SimpleBase.Base32.Rfc4648.Decode(text));
            Register("base32pad", 'c', bytes => SimpleBase.Base32.Rfc4648.Encode(bytes, true).ToLowerInvariant(), text => SimpleBase.Base32.Rfc4648.Decode(text));
            Register("base32hex", 'v', bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, false).ToLowerInvariant(), text => SimpleBase.Base32.ExtendedHex.Decode(text));
            Register("base32hexpad", 't', bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, true).ToLowerInvariant(), text => SimpleBase.Base32.ExtendedHex.Decode(text));
            Register("BASE32HEX", 'V', bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, false), text => SimpleBase.Base32.ExtendedHex.Decode(text));
            Register("BASE32HEXPAD", 'T', bytes => SimpleBase.Base32.ExtendedHex.Encode(bytes, true), text => SimpleBase.Base32.ExtendedHex.Decode(text));

            Register("base58btc", 'z', bytes => SimpleBase.Base58.Bitcoin.Encode(bytes), text => SimpleBase.Base58.Bitcoin.Decode(text));
            Register("base58flickr", 'Z', bytes => SimpleBase.Base58.Flickr.Encode(bytes), text => SimpleBase.Base58.Flickr.Decode(text));
           
            Register("base64", 'm', bytes => bytes.ToBase64NoPad(), text => text.FromBase64NoPad());
            Register("base64pad", 'M', bytes => Convert.ToBase64String(bytes), text => Convert.FromBase64String(text));
            Register("base64url", 'u', bytes => bytes.ToBase64Url(), text => text.FromBase64Url());
        }


        /// <summary>
        ///   Use <see cref="Register"/> to create codec new instance of codec <see cref="MultiBaseCoder"/>.
        /// </summary>
        private MultiBaseCoder()
        {
            Name = string.Empty;
        }
         
        /// <summary>
        ///   Register a new multibase codec.
        /// </summary>
        /// <param name="name">
        ///   The name of the codec.
        /// </param>
        /// <param name="code">
        ///   The multibase code assigned to the codec.
        /// </param>
        /// <param name="encode">
        ///   A <c>Func</c> to encode a byte array.  If not specified, then a <c>Func</c> is created to
        ///   throw a <see cref="NotImplementedException"/>.
        /// </param>
        /// <param name="decode">
        ///   A <c>Func</c> to decode a string.  If not specified, then a <c>Func</c> is created to
        ///   throw a <see cref="NotImplementedException"/>.
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiBaseAlgorithm"/>.
        /// </returns>
        public static MultiBaseCoder Register(string name, char code, Func<byte[], string> encode = null, Func<string, byte[]> decode = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
             

            encode ??= (_) => throw new NotImplementedException(string.Format("The multibase encode algorithm '{0}' is not implemented.", name));
            decode ??= (_) => throw new NotImplementedException(string.Format("The multibase decode algorithm '{0}' is not implemented.", name));

            var codec = new MultiBaseCoder
            {
                Name = name,
                Code = code,
                Encode = encode,
                Decode = decode
            };

            // add to member set
            _memberCodecs.Add(codec);
            
            return codec;
        }

        /// <summary>
        ///   Remove a multibase codec from the registry.
        /// </summary>
        /// <param name="codec">
        ///   The <see cref="MultiBaseCoder"/> to remove.
        /// </param>
        public static void Deregister(MultiBaseCoder codec)
        {
            _memberCodecs.Remove(codec);
        }

        /// <summary>
        ///   The <see cref="Name"/> of the codec.
        /// </summary>
        public override string ToString()
        {
            return Name?? "Unknown";
        }

    }
}
