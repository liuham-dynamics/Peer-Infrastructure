using Google.Protobuf;
using System.Globalization;
using System.Net;

namespace PeerStack.Transport
{
    /// <summary>
    /// TCP transport network protocol
    /// </summary>
    public class TcpNetworkTransport : ANetworkTransport
    {
       
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 6;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "tcp";
       
        /// <summary>
        /// Network Port
        /// </summary>
        public ushort Port { get; set; }


        /// <summary>
        /// Read Transport protocol port value
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="FormatException"></exception>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                Port = ushort.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid port number.", Value), e);
            }
        }

        /// <summary>
        /// Read Transport protocol port value
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="NullReferenceException"></exception>
        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadPrimitiveBytes(2);
            if(bytes is not null)
            {
                Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
                Value = Port.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                throw new NullReferenceException("Stream content is empty.");
            }
        }

        /// <summary>
        /// Write Transport protocol port value
        /// </summary>
        /// <param name="stream"></param>
        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WritePrimitiveBytes(bytes);
        }
    }
}
