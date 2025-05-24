using Google.Protobuf;
using PeerStack;
using PeerStack.Multiformat;
using System.Runtime.Serialization;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    /// A node in the IPLD Merkle DAG.
    /// </summary>
    /// <remarks>
    /// A <b>DagNode</b> has opaque <see cref="DagNode.DataBytes"/> and a set of navigable <see cref="DagNode.Links"/>.
    /// </remarks>
    [DataContract]
    public class DagNode : IMerkleNode<IMerkleLink>
    {
        private Cid id = new();
        private long? size;
        private string hashAlgorithm = MultiHash.DefaultAlgorithmName;

        [DataMember]
        private readonly HashSet<IMerkleLink> links = [];

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
        /// The serialized size in bytes of the node.
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
        public byte[] DataBytes { get; private set; }

        /// <inheritdoc />
        public Stream DataStream => new MemoryStream(DataBytes, false);

        /// <inheritdoc />
        public IEnumerable<IMerkleLink> Links => links;


        private void ComputeHash()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                size = ms.Position;
                ms.Position = 0;
                id = MultiHash.ComputeHash(ms, hashAlgorithm);
            }
        }

        private void ComputeSize()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                size = ms.Position;
            }
        }

        /// <summary>
        /// Create a new instance of a <see cref="DagNode"/> with the 
        /// specified <see cref="DagNode.DataBytes"/> and <see cref="DagNode.Links"/>
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
        public DagNode(byte[]? data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
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
        /// Creates a new instance of the <see cref="DagNode"/> class from the
        /// specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> containing the binary representation of the <b>DagNode</b>.
        /// </param>
        public DagNode(Stream stream)
        {
            DataBytes = [];
            Read(stream);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DagNode"/> class from the
        /// specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">(
        /// A <see cref="CodedInputStream"/> containing the binary representation of the <b>DagNode</b>.
        /// </param>
        public DagNode(CodedInputStream stream)
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
        /// A new <see cref="DagNode"/> with the existing and new links.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// </remarks>
        public DagNode AddLink(IMerkleLink link)
        {
            var newLinks = new List<IMerkleLink>(links) { link };
            newLinks.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Adds a sequence of links.
        /// </summary>
        /// <param name="links">
        /// The sequence of links to add.
        /// </param>
        /// <returns>
        /// A new <see cref="DagNode"/> with the existing and new links.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// </remarks>
        public DagNode AddLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = new List<IMerkleLink>(this.links);
            newLinks.AddRange(links);
            newLinks.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Removes a link.
        /// </summary>
        /// <param name="link">
        /// The <see cref="IMerkleLink"/> to remove.
        /// </param>
        /// <returns>
        /// A new <see cref="DagNode"/> with the <paramref name="link"/> removed.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// <para>
        /// No exception is raised if the <paramref name="link"/> does not exist.
        /// </para>
        /// </remarks>
        public DagNode RemoveLink(IMerkleLink link)
        {
            var newLinks = new List<IMerkleLink>(links);
            newLinks.RemoveAll(l => l.Equals(link));
            return new DagNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        /// Remove a sequence of links.
        /// </summary>
        /// <param name="links">
        /// The sequence of <see cref="IMerkleLink"/> to remove.
        /// </param>
        /// <returns>
        /// A new <see cref="DagNode"/> with the <paramref name="links"/> removed.
        /// </returns>
        /// <remarks>
        /// A <b>DagNode</b> is immutable.
        /// <para>
        /// No exception is raised if any of the <paramref name="links"/> do not exist.
        /// </para>
        /// </remarks>
        public DagNode RemoveLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = new List<IMerkleLink>(this.links);
            newLinks.RemoveAll(l => links.Contains(l));
            return new DagNode(DataBytes, newLinks, hashAlgorithm);
        }

        private void Read(Stream stream)
        {
            using (var cis = new CodedInputStream(stream, true))
            {
                Read(cis);
            }
        }

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
                            newLinks.Add(new DagLink(ms));
                        }
                        break;
                    default:
                        throw new InvalidDataException("Unknown field number");
                }
            }

            DataBytes ??= [];
            links.Clear();
            foreach (var link in newLinks)
            {
                links.Add(link);
            }
        }


        /// <summary>
        /// Writes the binary representation of the node to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to write to.
        /// </param>
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
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        public void Write(CodedOutputStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            foreach (DagLink link in Links.Cast<DagLink>())
            {
                using (var linkStream = new MemoryStream())
                {
                    link.Write(linkStream);
                    var msg = linkStream.ToArray();
                    stream.WriteTag(2, WireFormat.WireType.LengthDelimited);
                    stream.WriteLength(msg.Length);
                    stream.WritePrimitiveBytes(msg);
                }
            }

            if (DataBytes.Length > 0)
            {
                stream.WriteTag(1, WireFormat.WireType.LengthDelimited);
                stream.WriteLength(DataBytes.Length);
                stream.WritePrimitiveBytes(DataBytes);
            }
        }

        /// <inheritdoc />
        public IMerkleLink ToLink(string name = "") => new DagLink(name, Identifier, Size);

        /// <summary>
        /// Returns the IPFS binary representation as a byte array.
        /// </summary>
        /// <returns>
        /// A byte array.
        /// </returns>
        public byte[] ToArray()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }

    }
}
