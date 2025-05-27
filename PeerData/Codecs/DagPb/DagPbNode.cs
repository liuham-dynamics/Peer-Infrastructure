using Google.Protobuf;
using PeerStack;
using PeerStack.Multiformat;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    ///   Represents a node in the IPLD Merkle DAG using the dag-pb codec.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   <b>DagPbNode</b> implements <see cref="IMerkleNode{IMerkleLink}"/> and models a node in a Merkle Directed Acyclic Graph (DAG) as used by IPFS and IPLD.
    ///   Each node contains opaque data (<see cref="DataBytes"/>) and a set of navigable links (<see cref="Links"/>) to other nodes.
    ///   </para>
    ///   <para>
    ///   The links are always sorted in ascending order by <see cref="IMerkleLink.Name"/> (treating <c>null</c> as an empty string) and are immutable.
    ///   </para>
    ///   <para>
    ///   The node is immutable; all mutation methods return a new instance.
    ///   </para>
    ///   <para>
    ///   The node supports serialization and deserialization to and from the Protobuf-based dag-pb binary format.
    ///   </para>
    /// </remarks>
    [DataContract]
    public class DagPbNode : IMerkleNode<IMerkleLink>
    {
        private Cid id = new();
        private long? size;
        private string hashAlgorithm = MultiHash.DefaultAlgorithmName;
        private byte[]? _cachedBytes;

        [DataMember]
        private readonly ImmutableArray<IMerkleLink> links;

        /// <inheritdoc />
        [DataMember]
        public Cid Identifier
        {
            get
            {
                if (id is null || string.IsNullOrEmpty(id.Encoding))
                {
                    ComputeHash();
                }
                return id!;
            }
            private set
            {
                id = value;
                if (id is not null & !string.IsNullOrEmpty(id?.Hash?.Coder?.Name))
                {
                    hashAlgorithm = id!.Hash.Coder.Name!;
                }
            }
        }

        /// <summary>
        ///   Gets the serialized size in bytes of the node.
        /// </summary>
        [DataMember]
        public long Size
        {
            get
            {
                if (!size.HasValue)
                {
                    ComputeSize();
                }
                return size!.Value;
            }
        }

        /// <inheritdoc />
        [DataMember]
        public ReadOnlyMemory<byte> DataBytes { get; private set; }

        /// <inheritdoc />
        public Stream DataStream => new MemoryStream(DataBytes.ToArray(), false);

        /// <inheritdoc />
        public IEnumerable<IMerkleLink> Links => links;

        /// <summary>
        ///   Computes and caches the content identifier (CID) and size for this node using the specified hash algorithm.
        /// </summary>
        private void ComputeHash()
        {
            var bytes = ToArray();
            using var ms = new MemoryStream(bytes, false);
            size = ms.Length;
            ms.Position = 0;
            id = MultiHash.ComputeHash(ms, hashAlgorithm);
        }

        /// <summary>
        ///   Computes and caches the serialized size of this node.
        /// </summary>
        private void ComputeSize()
        {
            var bytes = ToArray();
            size = bytes.Length;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbNode"/> class with the specified data and links.
        /// </summary>
        /// <param name="data">
        ///   The opaque data for the node. May be empty.
        /// </param>
        /// <param name="links">
        ///   The links to other nodes. If provided, links are sorted by name.
        /// </param>
        /// <param name="hashAlgorithm">
        ///   The name of the hashing algorithm to use for the CID. Defaults to <see cref="MultiHash.DefaultAlgorithmName"/>.
        /// </param>
        public DagPbNode(ReadOnlyMemory<byte> data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
        {
            DataBytes = data;
            if (links is not null)
            {
                this.links = [.. links.OrderBy(link => link.Name ?? string.Empty)];
            }
            else
            {
                this.links = [];
            }
            this.hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbNode"/> class from a binary <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the Protobuf-encoded binary representation of the node.
        /// </param>
        public DagPbNode(Stream stream)
        {
            DataBytes = new ReadOnlyMemory<byte>();
            Read(stream);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbNode"/> class from a <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="CodedInputStream"/> containing the Protobuf-encoded binary representation of the node.
        /// </param>
        public DagPbNode(CodedInputStream stream)
        {
            DataBytes = new ReadOnlyMemory<byte>();
            Read(stream);
        }

        /// <summary>
        ///   Returns a new <see cref="DagPbNode"/> with the specified link added.
        /// </summary>
        /// <param name="link">The link to add.</param>
        /// <returns>A new <see cref="DagPbNode"/> with the link added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagPbNode AddLink(IMerkleLink link)
        {
            var newLinks = links.Add(link);
            return new DagPbNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagPbNode"/> with the specified links added.
        /// </summary>
        /// <param name="newLinks">The sequence of links to add.</param>
        /// <returns>A new <see cref="DagPbNode"/> with the links added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagPbNode AddLinks(IEnumerable<IMerkleLink> newLinks)
        {
            var combined = links.AddRange(newLinks);
            return new DagPbNode(DataBytes, combined, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagPbNode"/> with the specified link removed.
        /// </summary>
        /// <param name="link">The link to remove.</param>
        /// <returns>A new <see cref="DagPbNode"/> with the link removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if the link does not exist.
        /// </remarks>
        public DagPbNode RemoveLink(IMerkleLink link)
        {
            var newLinks = links.Remove(link);
            return new DagPbNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagPbNode"/> with the specified links removed.
        /// </summary>
        /// <param name="removeLinks">The sequence of links to remove.</param>
        /// <returns>A new <see cref="DagPbNode"/> with the links removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if any of the links do not exist.
        /// </remarks>
        public DagPbNode RemoveLinks(IEnumerable<IMerkleLink> removeLinks)
        {
            var newLinks = links.RemoveRange(removeLinks);
            return new DagPbNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Reads the binary representation of the node from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        private void Read(Stream stream)
        {
            using (var cis = new CodedInputStream(stream, true))
            {
                Read(cis);
            }
        }

        /// <summary>
        ///   Reads the binary representation of the node from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="CodedInputStream"/> to read from.</param>
        private void Read(CodedInputStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var newLinks = new List<IMerkleLink>();
            bool done = false;

            while (!stream.IsAtEnd && !done)
            {
                var tag = stream.ReadTag();
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        DataBytes = stream!.ReadPrimitiveBytes(stream.ReadLength())!;
                        done = true;
                        break;
                    case 2:
                        using (var ms = new MemoryStream(stream!.ReadPrimitiveBytes(stream!.ReadLength())!))
                        {
                            newLinks.Add(new DagPbLink(ms));
                        }
                        break;
                    default:
                        throw new InvalidDataException("Unknown field number");
                }
            }

            DataBytes = DataBytes.IsEmpty ? new ReadOnlyMemory<byte>() : DataBytes;
            links.Clear();
            foreach (var link in newLinks)
            {
                links.Add(link);
            }
        }

        /// <summary>
        ///   Writes the binary representation of the node to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public void Write(Stream stream)
        {
            using (var cos = new CodedOutputStream(stream, true))
            {
                Write(cos);
            }
        }

        /// <summary>
        ///   Writes the binary representation of the node to the specified <see cref="CodedOutputStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="CodedOutputStream"/> to write to.</param>
        public void Write(CodedOutputStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using var linkStream = new MemoryStream();
            foreach (DagPbLink link in Links.Cast<DagPbLink>())
            {
                linkStream.SetLength(0);
                link.Write(linkStream);
                var msg = linkStream.ToArray();
                stream.WriteTag(2, WireFormat.WireType.LengthDelimited);
                stream.WriteLength(msg.Length);
                stream.WritePrimitiveBytes(msg);
            }

            if (!DataBytes.IsEmpty)
            {
                stream.WriteTag(1, WireFormat.WireType.LengthDelimited);
                stream.WriteLength(DataBytes.Length);
                stream.WritePrimitiveBytes(DataBytes.ToArray());
            }
        }

        /// <inheritdoc />
        public IMerkleLink ToLink(string name = "") => new DagPbLink(name, Identifier, Size);

        /// <summary>
        ///   Returns the IPFS dag-pb binary representation of the node as a byte array.
        ///   The result is cached for performance.
        /// </summary>
        /// <returns>A byte array containing the binary representation of the node.</returns>
        public byte[] ToArray()
        {
            if (_cachedBytes is not null)
            {
                return _cachedBytes;
            }

            using var ms = new MemoryStream();
            Write(ms);
            _cachedBytes = ms.ToArray();
            return _cachedBytes;
        }
    }
}
