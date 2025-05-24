using System.Text.Json;
using System.Text.Json.Serialization;
using PeerStack.Multiformat;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    ///   A link to another node in the IPFS Merkle DAG.
    /// </summary>
    public class DagJsonLink : IMerkleLink
    {
        /// <inheritdoc />
        [JsonInclude]
        public Cid Identifier { get; private set; }

        /// <inheritdoc />
        [JsonInclude]
        public string Name { get; private set; }

        /// <inheritdoc />
        [JsonInclude]
        public long Size { get; private set; }


        /// <summary>
        /// Create a new instance of <see cref="DagJsonLink"/> class.
        /// </summary>
        /// <param name="name">The name associated with the linked node.</param>
        /// <param name="id">The <see cref="Cid"/> of the linked node.</param>
        /// <param name="size">The serialized size (in bytes) of the linked node.</param>
        [JsonConstructor]
        public DagJsonLink(string name, Cid id, long size)
        {
            Name = name;
            Identifier = id;
            Size = size;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DagJsonLink"/> class from the specified <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">
        /// Some type of a Merkle link.
        /// </param>
        public DagJsonLink(IMerkleLink link) : this(link.Name, link.Identifier, link.Size) { }

        /// <summary>
        ///   Creates a new instance of the <see cref="DagJsonLink"/> class from the
        ///   specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the binary representation of the
        ///   <b>DagJsonLink</b>.
        /// </param>
        public DagJsonLink(Stream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;

            Read(stream);
        }

        private void Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var jsonLink = JsonSerializer.Deserialize<DagJsonLink>(stream) 
                         ?? throw new NullReferenceException("Invalid JSON UTF-8 stream");

            Identifier = jsonLink.Identifier;
            Name = jsonLink.Name;
            Size = jsonLink.Size;
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


        /// <summary>
        /// Returns the binary representation as a byte array.
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
