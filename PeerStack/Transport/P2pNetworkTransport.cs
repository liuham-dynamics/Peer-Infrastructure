using Google.Protobuf;
using PeerStack.Multiformat;

namespace PeerStack.Transport
{
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
     
         
        public MultiHash MultiHash { get; private set; }


        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            MultiHash = new MultiHash(Value);
        }

        public override void ReadValue(CodedInputStream stream)
        {
            stream.ReadLength();
            MultiHash = new MultiHash(stream);
            Value = MultiHash.ToBase58();
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = MultiHash.ToArray();
            stream.WriteLength(bytes.Length);
            stream.WriteSomeBytes(bytes);
        }
    }
}
