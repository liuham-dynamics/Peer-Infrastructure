using Google.Protobuf;
using PeerStack.Encoding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PeerStack.Multiformat
{
    /// <summary>
    ///   Wraps other formats with a tiny bit of self-description.
    /// </summary>
    /// <remarks>
    ///   <b>MultiCodec</b> is a self-describing multiformat, it wraps other formats with a
    ///   tiny bit of self-description. A multi-codec identifier is both a Varint and the code
    ///   identifying the following data.
    ///   <para>
    ///   Adds the following extension methods to <see cref="Stream"/>
    ///    <list type="bullet">
    ///      <item><description>ReadMultiCodec</description></item>
    ///      <item><description><see cref="WriteMultiCodec"/></description></item>
    ///    </list>
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/multiformats/multicodec"/>
    /// <seealso cref="Registry.Codec"/>
    public static class MultiCodec
    {
        /// <summary>
        ///   Reads a <see cref="Codec"/> from the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A multi-codec encoded <see cref="Stream"/>.
        /// </param>
        /// <returns>The codec.</returns>
        /// <remarks>
        ///   If the <b>code</b> does not exist, a new <see cref="Codec"/> is
        ///   registered with the <see cref="MultiCodecCoder.Name"/> "codec-x"; where
        ///   'x' is the code's decimal representation.
        /// </remarks>
        public static MultiCodecCoder ReadMultiCodec(this Stream stream)
        {
            // get codec code from stream
            var code = stream.ReadVarint32();

            // get associate codec
            var codec = MultiCodecCoder.Codecs.FirstOrDefault(o => o.Code == code);
            return codec is null | string.IsNullOrEmpty(codec?.Name)
                ? MultiCodecCoder.Register($"codec-{code}", code, MultiCodecTags.Unknow)
                : codec!;
        }

        /// <summary>
        ///   Reads a <see cref="Codec"/> from the <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A multi-codec encoded <see cref="CodedInputStream"/>.
        /// </param>
        /// <returns>The codec.</returns>
        /// <remarks>
        ///   If the <b>code</b> does not exist, a new <see cref="Codec"/> is
        ///   registered with the <see cref="Codec.Name"/> "codec-x"; where
        ///   'x' is the code's decimal representation.
        /// </remarks>
        public static MultiCodecCoder ReadMultiCodec(this CodedInputStream stream)
        {
            // get codec code from stream
            var code = stream.ReadInt32();
            
            // get associate codec
            var codec = MultiCodecCoder.Codecs.FirstOrDefault(o => o.Code == code);
            return codec is null | string.IsNullOrEmpty(codec?.Name)
                ? MultiCodecCoder.Register($"codec-{code}", code, MultiCodecTags.Unknow)
                : codec!;
        }

        /// <summary>
        ///   Writes a <see cref="Codec"/> to the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A multi-codec encoded <see cref="Stream"/>.
        /// </param>
        /// <param name="name">
        ///   The <see cref="Codec.Name"/>.
        /// </param>
        /// <remarks>
        ///   Writes the <see cref="Varint"/> of the <see cref="Codec.Code"/> to
        ///   the <paramref name="stream"/>.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static void WriteMultiCodec(this Stream stream, string name)
        {
            // get associate codec
            var codec = MultiCodecCoder.Codecs.FirstOrDefault(o => o.Name == name);
            if (codec is null | string.IsNullOrEmpty(codec?.Name))
            {
                throw new KeyNotFoundException($"Codec '{name}' is not registered.");
            }
            stream.WriteVarint(codec!.Code);
        }
    }
}
