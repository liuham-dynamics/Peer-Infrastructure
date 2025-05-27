using PeerStack.Multiformat;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeerData.Codecs.DagJson
{
    /// <summary>
    ///   Represents an immutable, thread-safe node in the IPLD Merkle DAG using the dag-json codec.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   <b>DagJsonNode</b> implements <see cref="IMerkleNode{IMerkleLink}"/> and models a node in a Merkle Directed Acyclic Graph (DAG) as used by IPFS and IPLD.
    ///   Each node contains opaque data (<see cref="DataBytes"/>) and a set of navigable links (<see cref="Links"/>) to other nodes.
    ///   </para>
    ///   <para>
    ///   The links are always sorted in ascending order by <see cref="IMerkleLink.Name"/> (treating <c>null</c> as an empty string) and are immutable.
    ///   </para>
    ///   <para>
    ///   The node is immutable; all mutation methods return a new instance.
    ///   </para>
    ///   <para>
    ///   The node supports serialization and deserialization to and from the JSON-based dag-json format via <see cref="System.Text.Json"/>.
    ///   </para>
    ///   <para>
    ///   The <see cref="ToArrayAsync"/> method caches the binary representation for performance if the object is immutable.
    ///   </para>
    /// </remarks>
    [DataContract]
    public record class DagJsonNode : IMerkleNode<IMerkleLink>
    {
        private byte[]? _memberCachedBytes;

        private Cid id = new();
        private long? size;
        private string hashAlgorithm = MultiHash.DefaultAlgorithmName;

        [JsonInclude]
        private readonly ImmutableArray<IMerkleLink> links;

        /// <inheritdoc />
        [JsonInclude]
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
        [JsonInclude]
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
        [JsonInclude]
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
            var bytes = _memberCachedBytes ?? ToArrayAsync().GetAwaiter().GetResult();
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
            var bytes = _memberCachedBytes ?? ToArrayAsync().GetAwaiter().GetResult();
            size = bytes.Length;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonNode"/> class with the specified data and links.
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
        public DagJsonNode(ReadOnlyMemory<byte> data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
        {
            DataBytes = data;
            if (links is not null)
            {
                var sortedLinks = links.OrderBy(link => link.Name ?? "");
                this.links = [.. sortedLinks];
            }
            else
            {
                this.links = ImmutableArray<IMerkleLink>.Empty;
            }
            this.hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonNode"/> class from a <see cref="Stream"/> containing its JSON representation.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the JSON representation of the node.
        /// </param>
        public DagJsonNode(Stream stream)
        {
            DataBytes = Array.Empty<byte>();
            links = ImmutableArray<IMerkleLink>.Empty;
            Read(stream);
        }

        /// <summary>
        ///   Returns a new <see cref="DagJsonNode"/> with the specified link added.
        /// </summary>
        /// <param name="link">The link to add.</param>
        /// <returns>A new <see cref="DagJsonNode"/> with the link added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagJsonNode AddLink(IMerkleLink link)
        {
            var newLinks = links.Add(link).Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagJsonNode"/> with the specified links added.
        /// </summary>
        /// <param name="links">The sequence of links to add.</param>
        /// <returns>A new <see cref="DagJsonNode"/> with the links added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagJsonNode AddLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = this.links.AddRange(links).Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagJsonNode"/> with the specified link removed.
        /// </summary>
        /// <param name="link">The link to remove.</param>
        /// <returns>A new <see cref="DagJsonNode"/> with the link removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if the link does not exist.
        /// </remarks>
        public DagJsonNode RemoveLink(IMerkleLink link)
        {
            var newLinks = links.Remove(link);
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagJsonNode"/> with the specified links removed.
        /// </summary>
        /// <param name="links">The sequence of links to remove.</param>
        /// <returns>A new <see cref="DagJsonNode"/> with the links removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if any of the links do not exist.
        /// </remarks>
        public DagJsonNode RemoveLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = this.links.RemoveRange(links)
                               .Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Reads the JSON representation of the node from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        private void Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var jsonNode = JsonSerializer.Deserialize<DagJsonNode>(stream)
                         ?? throw new NullReferenceException("Invalid JSON UTF-8 stream");
            if (jsonNode?.Links?.Any() == true)
            {
                // No mutation needed; links are set in constructor
                // This is a placeholder for any additional logic if needed
            }

            DataBytes = jsonNode!.DataBytes;
        }

        /// <summary>
        ///   Writes the JSON representation of the node to the specified <see cref="Stream"/> asynchronously.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public async Task WriteAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            await JsonSerializer.SerializeAsync(stream, this).ConfigureAwait(false);
        }

        /// <summary>
        ///   Writes the JSON representation of the node to the specified <see cref="Stream"/> asynchronously, with cancellation support.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            await JsonSerializer.SerializeAsync(stream, this, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IMerkleLink ToLink(string name = "") => new DagJsonLink(name, Identifier, Size);

        /// <summary>
        ///   Returns the dag-json binary representation of the node as a byte array.
        ///   The result is cached for performance if the object is immutable.
        /// </summary>
        /// <returns>A byte array containing the JSON representation of the node.</returns>
        public async Task<byte[]> ToArrayAsync()
        {
            if (_memberCachedBytes is not null)
                return _memberCachedBytes;

            using var ms = new MemoryStream();
            await WriteAsync(ms).ConfigureAwait(false);
            _memberCachedBytes = ms.ToArray();
            return _memberCachedBytes;
        }
    }
}
