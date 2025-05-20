using PeerStack.Multiformat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack
{
    /// <summary>
    ///   A daemon node on the IPFS network.
    /// </summary>
    /// <remarks>
    ///   Equality is based solely on the peer's <see cref="Identifier"/>.
    /// </remarks>
    public sealed class Peer : IEquatable<Peer>
    {
        //
        private const string unknown = "unknown/0.0";
        private static readonly HashSet<MultiAddress> _memberAddresses = [];
       

        /// <summary>
        /// Universally unique identifier.
        /// </summary>
        /// <value>
        ///   This is the <see cref="MultiHash"/> of the peer's protobuf encoded
        ///   <see cref="PublicKey"/>.
        /// </value>
        /// <seealso href="https://github.com/libp2p/specs/pull/100"/>
        public required MultiHash Identifier { get; set; }

        /// <summary>
        ///   The public key of the node.
        /// </summary>
        /// <value>
        ///   The base 64 encoding of the node's public key.  The default is an empty string
        /// </value>
        /// <remarks>
        ///   The IPFS public key is the base-64 encoding of a protobuf encoding containing
        ///   a type and the DER encoding of the PKCS Subject Public Key Info.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc5280#section-4.1.2.7"/>
        public  string PublicKey { get; set; } = string.Empty;

        /// <summary>
        ///   The <see cref="MultiAddress"/> that the peer is connected on.
        /// </summary>
        /// <value>
        ///   <b>null</b> when the peer is not connected to.
        /// </value>
        public MultiAddress ConnectedAddress { get; set; }

        /// <summary>
        /// The multiple addresses of the node.
        /// </summary>
        /// <value>
        ///   Where the peer can be found. The default is an empty set.
        /// </value>
        public IEnumerable<MultiAddress> Addresses { get; set; } = _memberAddresses;

        /// <summary>
        ///   The name and version of the IPFS software.
        /// </summary>
        /// <value>
        ///   For example "go-ipfs/0.4.17/".
        /// </value>
        /// <remarks>
        ///   There is no specification that describes the agent version string.  
        ///   The default is "unknown/0.0".
        /// </remarks>
        public string AgentVersion { get; set; } = unknown;

        /// <summary>
        ///  The name and version of the supported IPFS protocol.
        /// </summary>
        /// <value>
        ///   For example "ipfs/0.1.0".
        /// </value>
        /// <remarks>
        ///   There is no specification that describes the protocol version string. 
        ///   The default is "unknown/0.0".
        /// </remarks>
        public string ProtocolVersion { get; set; } = unknown;

        /// <summary>
        /// The round-trip time it takes to get data from the peer.
        /// </summary>
        public TimeSpan? Latency { get; set; }

        /// <summary>
        ///   Determines if the information on the peer is valid.
        /// </summary>
        /// <returns>
        ///   <b>true</b> if all validation rules pass; otherwise <b>false</b>.
        /// </returns>
        /// <remarks>
        ///    Verifies that
        ///    <list type="bullet">
        ///      <item><description>The <see cref="Identifier"/> is defined</description></item>
        ///      <item><description>The <see cref="Identifier"/> is a hash of the <see cref="PublicKey"/></description></item>
        ///    </list>
        /// </remarks>
        public bool IsValid()
        {
            return Identifier is null ? false 
                              : PublicKey == null || Identifier.Matches(Convert.FromBase64String(PublicKey));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            var that = obj as Peer;
            return (that is not null) && Equals(that);
        }

        /// <inheritdoc />
        public bool Equals(Peer? that)
        {
            return that is not null && Identifier == that.Identifier;
        }

        /// <summary>
        /// Value equality.
        /// </summary>
        public static bool operator ==(Peer a, Peer b)
        {
            return ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));
        }

        /// <summary>
        /// Value inequality.
        /// </summary>
        public static bool operator !=(Peer a, Peer b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Returns the <see cref="Base58"/> encoding of the <see cref="Identifier"/>.
        /// </summary>
        /// <returns>
        /// A Base58 representation of the peer.
        /// </returns>
        public override string ToString()
        {
            return Identifier is null ? string.Empty : Identifier.ToBase58();
        }

        /// <summary>
        ///   Implicit casting of a <see cref="string"/> to a <see cref="Peer"/>.
        /// </summary>
        /// <param name="s">
        ///   A <see cref="Base58"/> encoded <see cref="Identifier"/>.
        /// </param>
        /// <returns>
        ///   A new <see cref="Peer"/>.
        /// </returns>
        /// <remarks>
        ///    Equivalent to <code>new Peer { Identifier = s }</code>
        /// </remarks>
        public static implicit operator Peer(string s)
        {
            return new Peer { Identifier = s };
        }
    }
}
