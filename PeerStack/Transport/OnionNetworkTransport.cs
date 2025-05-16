using Google.Protobuf;
using PeerStack.Encoding;
using System.Globalization;
using System.Net;

namespace PeerStack.Transport
{
    /// <summary>
    /// Onion transport network protocol
    /// </summary>
    public class OnionNetworkTransport : ANetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 444;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "onion";

        /// <summary>
        /// Network Port
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// Address bytes
        /// </summary>
        public byte[] Address { get; private set; } = [];

        /// <summary>
        /// Read onion values from stream
        /// </summary>
        /// <param name="stream">TextReader</param>
        /// <exception cref="FormatException"></exception>
        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            var parts = Value.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address, missing the port number.", Value));
            }

            if (parts[0].Length != 16)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address.", Value));
            }

            try
            {
                Port = ushort.Parse(parts[1]);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value), e);
            }

            if (Port < 1)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value));
            }

            Address = parts[0].ToUpperInvariant().FromBase32();
        }

        /// <summary>
        /// Read onion values from primitive stream
        /// </summary>
        /// <param name="stream">CodedInputStream</param>
        public override void ReadValue(CodedInputStream stream)
        {
            var addressBytes = stream.ReadPrimitiveBytes(10) 
                ?? throw new NullReferenceException("Address bytes can NOT be null.");
            Address = addressBytes;

            var bytes = stream.ReadPrimitiveBytes(2) 
                ?? throw new NullReferenceException("Port bytes can NOT be bull.");
            Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = Address.ToBase32().ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write onion values to primitive stream
        /// </summary>
        /// <param name="stream">CodedOutputStream</param>
        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WritePrimitiveBytes(Address);
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WritePrimitiveBytes(bytes);
        }
    }
}
