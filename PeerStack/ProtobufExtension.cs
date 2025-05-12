using Google.Protobuf;
using System.Reflection;

namespace PeerStack
{
    public static class ProtobufExtension
    {
        //
        private static readonly MethodInfo WriteRawBytes = typeof(CodedOutputStream)
                                                   .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                   .Single(m => m.Name == "WriteRawBytes" && m.GetParameters().Length == 1);

        //
        private static readonly MethodInfo ReadRawBytes = typeof(CodedInputStream)
                                                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                    .Single(m => m.Name == "ReadRawBytes");

        /// <summary>
        /// Write Primitive from coded output stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        /// <param name="bytes">Array of bytes</param>
        public static void WritePrimitiveBytes(this CodedOutputStream stream, byte[] bytes)
        {
            WriteRawBytes.Invoke(stream, [bytes]);
        }

        /// <summary>
        /// Read primitive bytes from coded input stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        /// <param name="length">Int32</param>
        /// <returns></returns>
        public static byte[]? ReadPrimitiveBytes(this CodedInputStream stream, int length)
        {
            return ReadRawBytes.Invoke(stream, [length]) as byte[];
        }
    }
}
