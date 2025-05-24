using PeerStack;
using PeerStack.Multiformat;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    /// A node in the IPLD Merkle DAG.
    /// </summary>
    /// <remarks>
    /// A <b>DagNode</b> has opaque <see cref="DagJsonNode.DataBytes"/> and a set of navigable <see cref="DagJsonNode.Links"/>.
    /// </remarks>
    [DataContract]
    public class DagJsonNode : IMerkleNode<IMerkleLink>
    {
        private Cid id = new();
        private long? size;
        private string hashAlgorithm = MultiHash.DefaultAlgorithmName;

        [JsonInclude]
        private readonly HashSet<IMerkleLink> links = [];

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
        /// The serialized size in bytes of the node.
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
        public byte[] DataBytes { get; private set; }

        /// <inheritdoc />
        public Stream DataStream => new MemoryStream(DataBytes, false);

        /// <inheritdoc />
        public IEnumerable<IMerkleLink> Links => links;

        private void ComputeHash()
        {
            using (var ms = new MemoryStream())
            {
                WriteAsync(ms);
                size = ms.Position;
                ms.Position = 0;
                id = MultiHash.ComputeHash(ms, hashAlgorithm);
            }
        }

        private void ComputeSize()
        {
            using (var ms = new MemoryStream())
            {
                WriteAsync(ms);
                size = ms.Position;
            }
        }

        /// <summary>
        /// Create a new instance of a <see cref="DagJsonNode"/> with the 
        /// specified <see cref="DagJsonNode.DataBytes"/> and <see cref="DagJsonNode.Links"/>
        /// </summary>
        /// <param name="data">
        ///   The opaque data, can be <b>null</b>.
        /// </param>
        /// <param name="links">
        ///   The links to other nodes.
        /// </param>
        /// <param name="hashAlgorithm">
        ///   The name of the hashing algorithm to use; defaults to
        ///   <see cref="MultiHash.DefaultAlgorithmName"/>.
        /// </param>
        public DagJsonNode(byte[]? data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
        {
            DataBytes = data ?? [];
            if (links is not null)
            {
                var sortedLinks = links.OrderBy(link => link.Name ?? "");
                foreach (var link in sortedLinks)
                {
                    this.links.Add(link);
                }
            }
            this.hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DagJsonNode"/> class from the
        /// specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> containing the binary representation of the <b>DagNode</b>.
        /// </param>
        public DagJsonNode(Stream stream)
        {
            DataBytes = [];
            Read(stream);
        }
         
        /// <summary>
        /// Adds a link.
        /// </summary>
        /// <param name="link">
        /// The link to add.
        /// </param>
        /// <returns>
        /// A new <see cref="DagJsonNode"/> with the existing and new links.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// </remarks>
        public DagJsonNode AddLink(IMerkleLink link)
        {
            var newLinks = new List<IMerkleLink>(links) { link };
            newLinks.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Adds a sequence of links.
        /// </summary>
        /// <param name="links">
        /// The sequence of links to add.
        /// </param>
        /// <returns>
        /// A new <see cref="DagJsonNode"/> with the existing and new links.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// </remarks>
        public DagJsonNode AddLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = new List<IMerkleLink>(this.links);
            newLinks.AddRange(links);
            newLinks.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Removes a link.
        /// </summary>
        /// <param name="link">
        /// The <see cref="IMerkleLink"/> to remove.
        /// </param>
        /// <returns>
        /// A new <see cref="DagJsonNode"/> with the <paramref name="link"/> removed.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// <para>
        /// No exception is raised if the <paramref name="link"/> does not exist.
        /// </para>
        /// </remarks>
        public DagJsonNode RemoveLink(IMerkleLink link)
        {
            var newLinks = new List<IMerkleLink>(links);
            newLinks.RemoveAll(l => l.Equals(link));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Remove a sequence of links.
        /// </summary>
        /// <param name="links">
        /// The sequence of <see cref="IMerkleLink"/> to remove.
        /// </param>
        /// <returns>
        /// A new <see cref="DagJsonNode"/> with the <paramref name="links"/> removed.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// <para>
        /// No exception is raised if any of the <paramref name="links"/> do not exist.
        /// </para>
        /// </remarks>
        public DagJsonNode RemoveLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = new List<IMerkleLink>(this.links);
            newLinks.RemoveAll(l => links.Contains(l));
            return new DagJsonNode(DataBytes, newLinks, hashAlgorithm);
        }

        private void Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var jsonNode = JsonSerializer.Deserialize<DagJsonNode>(stream);
            if (jsonNode is null)
            {
                throw new NullReferenceException("Invalid JSON UTF-8 stream");
            }

            if (jsonNode?.Links?.Any() == true)
            {
                links.Clear();
                this.AddLinks(jsonNode.Links);
            }

            DataBytes = jsonNode!.DataBytes ?? [];
        }

        /// <summary>
        /// Writes the binary representation of the node to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to write to.
        /// </param>
        public async void WriteAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            await JsonSerializer.SerializeAsync(stream, this).ConfigureAwait(false);
        }
         
        /// <inheritdoc />
        public IMerkleLink ToLink(string name = "") => new DagPbLink(name, Identifier, Size);

        /// <summary>
        /// Returns the IPFS binary representation as a byte array.
        /// </summary>
        /// <returns>
        /// A byte array.
        /// </returns>
        public byte[] ToArrayAsync()
        {
            using var ms = new MemoryStream();
            WriteAsync(ms);
            return ms.ToArray();
        }

    }
}
