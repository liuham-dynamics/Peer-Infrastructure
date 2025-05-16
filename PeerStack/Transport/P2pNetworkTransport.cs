using Google.Protobuf;
using PeerStack.Multiformat;

namespace PeerStack.Transport
{
    /// <summary>
    /// P2P transport network protocol
    /// </summary>
    public class P2pNetworkTransport : ANetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 421;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "p2p";
     
         /// <summary>
         /// Multi-hash
         /// </summary>
        public MultiHash? MultiHash { get; private set; }

        /// <summary>
        /// Read p2p values from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            MultiHash = new MultiHash(Value);
        }

        /// <summary>
        /// Read p2p values from primitive stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        public override void ReadValue(CodedInputStream stream)
        {
            stream.ReadLength();
            MultiHash = new MultiHash(stream);
            Value = MultiHash.ToBase58();
        }

        /// <summary>
        /// Write p2p values to stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = MultiHash?.ToArray();
            if(bytes is not null)
            {
                stream.WriteLength(bytes.Length);
                stream.WritePrimitiveBytes(bytes);
            }
        }
    }
}
