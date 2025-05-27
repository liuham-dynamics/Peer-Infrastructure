using System.Text.Json;
using System.Text.Json.Serialization;
using PeerStack.Multiformat;

namespace PeerData.Codecs.DagJson
{
    /// <summary>
    ///   Represents an immutable link to another node in the IPFS Merkle DAG using the dag-json codec.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   <b>DagJsonLink</b> implements <see cref="IMerkleLink"/> and provides a reference to another node in a Merkle Directed Acyclic Graph (DAG) as used by IPFS and IPLD.
    ///   Each link contains a unique content identifier (<see cref="Identifier"/>), an optional name (<see cref="Name"/>), and the serialized size (<see cref="Size"/>) of the linked node.
    ///   </para>
    ///   <para>
    ///   This class is designed for use with the dag-json codec and supports JSON serialization and deserialization via <see cref="System.Text.Json"/>.
    ///   </para>
    ///   <para>
    ///   The class is immutable and thread-safe. All properties are set at construction and cannot be modified.
    ///   </para>
    ///   <para>
    ///   The <see cref="ToArrayAsync"/> method caches the binary representation for performance if the object is immutable.
    ///   </para>
    /// </remarks>
    public record class DagJsonLink : IMerkleLink
    {
        private byte[]? _memberCachedBytes;

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
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class with the specified name, identifier, and size.
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
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class from an existing <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">An existing Merkle link to copy values from.</param>
        public DagJsonLink(IMerkleLink link) : this(link.Name, link.Identifier, link.Size) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class from a <see cref="Stream"/> containing its JSON representation.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the JSON representation of the <b>DagJsonLink</b>.
        /// </param>
        public DagJsonLink(Stream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;
            Read(stream);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="DagJsonLink"/> class from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the JSON representation of the <b>DagJsonLink</b>.
        /// </param>
        /// <returns>
        ///   A new instance of <see cref="DagJsonLink"/>.
        /// </returns>
        public static DagJsonLink FromStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            return JsonSerializer.Deserialize<DagJsonLink>(stream)
                ?? throw new NullReferenceException("Invalid JSON UTF-8 stream");
        }

        /// <summary>
        ///   Reads the JSON representation of the link from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
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
        ///   Writes the JSON representation of the link to the specified <see cref="Stream"/> asynchronously.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public async Task WriteAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            await JsonSerializer.SerializeAsync(stream, this).ConfigureAwait(false);
        }

        /// <summary>
        ///   Writes the JSON representation of the link to the specified <see cref="Stream"/> asynchronously, with cancellation support.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            await JsonSerializer.SerializeAsync(stream, this, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///   Returns the JSON binary representation of the link as a byte array.
        ///   The result is cached for performance if the object is immutable.
        /// </summary>
        /// <returns>A byte array containing the JSON representation of the link.</returns>
        public async Task<byte[]> ToArrayAsync()
        {
            if (_memberCachedBytes is not null)
            {
                return _memberCachedBytes;
            }

            using var ms = new MemoryStream();
            await WriteAsync(ms).ConfigureAwait(false);
            _memberCachedBytes = ms.ToArray();
            return _memberCachedBytes;
        }
         
        /// <summary>
        ///   Returns a hash code for the current <see cref="DagJsonLink"/>.
        /// </summary>
        /// <returns>A hash code for the current link.</returns>
        public override int GetHashCode() => HashCode.Combine(Identifier, Name, Size);
    }
}
