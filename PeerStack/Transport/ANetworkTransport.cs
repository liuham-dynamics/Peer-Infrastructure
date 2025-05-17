using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Transport
{
    /// <summary>
    ///  Definition for metadata on an libp2p network protocol.
    /// </summary>
    /// <remarks>
    ///   Protocols are defined at <see href="https://github.com/multiformats/multiaddr/blob/master/protocols.csv"/>.
    /// </remarks>
    /// <seealso cref="MultiAddress"/>
    public abstract class ANetworkTransport
    {
        private static readonly HashSet<Type> _memberNetworkProtocols = [];
        private static readonly Dictionary<string, uint> _memberProtocolCache = [];
         
        /// <summary>
        ///   The libp2p numeric code assigned to the network protocol.
        /// </summary>
        public abstract uint Code { get; }

        /// <summary>
        ///   The name of the network protocol.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   The string value associated with the protocol.
        /// </summary>
        /// <remarks>
        ///   For tcp and udp this is the port number. 
        ///   This can be <b>null</b> as is the case for http and https.
        /// </remarks>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        ///   A collection of registered protocols.
        /// </summary>
        /// <remarks>
        ///  Abstract network transport types based on registered protocols.
        /// </remarks>
        public IEnumerable<Type> Protocols => _memberNetworkProtocols;

        /// <summary>
        /// Registers the standard network transports/protocols for libp2p.
        /// </summary>
        static ANetworkTransport()
        {
            Register<P2pNetworkTransport>();
            RegisterAlias<IpfsNetworkTransport>();

            Register<IpCidrNetworkTransport>();

            Register<Ipv4NetworkTransport>();
            Register<Ipv6NetworkTransport>();
            Register<TcpNetworkTransport>();
            Register<UdpNetworkTransport>();
            Register<DccpNetworkTransport>();
            Register<SctpNetworkTransport>();

            Register<OnionNetworkTransport>();
            Register<Onion3NetworkTransport>(); // TODO: Review Implementation

            Register<QuicNetworkTransport>(); // TODO: Review Implementation
            Register<HttpNetworkTransport>();
            Register<HttpsNetworkTransport>();

            Register<WsNetworkTransport>();
            Register<WssNetworkTransport>();

            Register<WebRtcNetworkTransport>();
            Register<WebRtcDirectNetworkTransport>();

            Register<UdtNetworkTransport>();
            Register<UtpNetworkTransport>();

            Register<CircuitNetworkTransport>();

            Register<DnsNetworkTransport>();
            Register<Dns4NetworkTransport>();
            Register<Dns6NetworkTransport>();
            Register<DnsAddressNetworkTransport>();
        }

        /// <summary>
        ///   Register a network protocol for use.
        /// </summary>
        /// <typeparam name="T">
        ///   A <see cref="ANetworkTransport"/> to register.
        /// </typeparam>
        public static void Register<T>() where T : ANetworkTransport, new()
        {
            var protocol = new T(); //
            if (_memberProtocolCache.ContainsValue(protocol.Code))
            {
                throw new ArgumentException(string.Format("The libp2p protocol code ({0}) is already defined.", protocol.Code));
            }
            _memberProtocolCache.Add(protocol.Name, protocol.Code);
            _memberNetworkProtocols.Add(typeof(T));
        }

        /// <summary>
        ///   Register an alias to another network protocol.
        /// </summary>
        /// <typeparam name="T">
        ///   A <see cref="ANetworkTransport"/> to register.
        /// </typeparam>
        public static void RegisterAlias<T>() where T : ANetworkTransport, new()
        {
            var protocol = new T(); //
            if (_memberProtocolCache.ContainsKey(protocol.Name))
            {
                throw new ArgumentException(string.Format("The libp2p protocol '{0}' is already defined.", protocol.Name));
            }
            _memberProtocolCache.Add(protocol.Name, protocol.Code);
            _memberNetworkProtocols.Add(typeof(T));
        }

        /// <summary>
        ///   Reads the string representation from the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="TextReader"/> to read from
        /// </param>
        /// <remarks>
        ///   The string representation is "/<see cref="Name"/>" followed by
        ///   an optional "/<see cref="Value"/>".
        /// </remarks>
        public virtual void ReadValue(TextReader stream)
        {
            int c;
            while (-1 != (c = stream.Read()) && c != '/')
            {
                Value += (char)c;
            }
        }

        /// <summary>
        ///   Reads the binary representation from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to read from.
        /// </param>
        /// <remarks>
        ///   The binary representation is an option <see cref="Value"/>.
        /// </remarks>
        public abstract void ReadValue(CodedInputStream stream);


        /// <summary>
        ///   Writes the string representation to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="TextWriter"/> to write to.
        /// </param>
        /// <remarks>
        ///   The string representation of the optional <see cref="Value"/>.
        /// </remarks>
        public virtual void WriteValue(TextWriter stream)
        {
            if (Value != null)
            {
                stream.Write('/');
                stream.Write(Value);
            }
        }

        /// <summary>
        ///   Writes the binary representation to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        /// <remarks>
        ///   The binary representation of the <see cref="Value"/>.
        /// </remarks>
        public abstract void WriteValue(CodedOutputStream stream);

        /// <summary>
        ///   The <see cref="Name"/> and optional <see cref="Value"/> of the network protocol.
        /// </summary>
        public override string ToString()
        {
            using (var s = new StringWriter())
            {
                s.Write('/');
                s.Write(Name);
                WriteValue(s);
                return s.ToString();
            }
        }
    }
}
