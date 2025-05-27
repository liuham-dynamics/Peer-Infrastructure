using Google.Protobuf;
using PeerStack.Multiformat;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    ///   Represents an immutable link to another node in the IPFS Merkle DAG using the dag-pb codec.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   <b>DagPbLink</b> implements <see cref="IMerkleLink"/> and provides a reference to another node in a Merkle Directed Acyclic Graph (DAG) as used by IPFS and IPLD.
    ///   Each link contains a unique content identifier (<see cref="Identifier"/>), an optional name (<see cref="Name"/>), and the serialized size (<see cref="Size"/>) of the linked node.
    ///   </para>
    ///   <para>
    ///   The class supports construction from explicit values, another <see cref="IMerkleLink"/>, or from a binary stream (using Protobuf encoding).
    ///   It also provides methods for serializing and deserializing the link to and from binary representations compatible with the dag-pb format.
    ///   </para>
    ///   <para>
    ///   The <see cref="ToArray"/> method caches the binary representation for performance when the object is immutable.
    ///   </para>
    ///   <para>
    ///   This class is thread-safe and designed for use in distributed, content-addressed systems.
    ///   </para>
    /// </remarks>
    public record class DagPbLink : IMerkleLink
    {
        private byte[]? _memberCachedBytes;

        /// <inheritdoc />
        public Cid Identifier { get; private set; } = new();

        /// <inheritdoc />
        public string Name { get; private set; } = string.Empty;

        /// <inheritdoc />
        public long Size { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbLink"/> class with the specified name, identifier, and size.
        /// </summary>
        /// <param name="name">The name associated with the linked node.</param>
        /// <param name="id">The <see cref="Cid"/> of the linked node.</param>
        /// <param name="size">The serialized size (in bytes) of the linked node.</param>
        public DagPbLink(string name, Cid id, long size)
        {
            Name = name;
            Identifier = id;
            Size = size;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbLink"/> class from an existing <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">An existing Merkle link to copy values from.</param>
        public DagPbLink(IMerkleLink link) : this(link.Name, link.Identifier, link.Size) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbLink"/> class from a binary <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the Protobuf-encoded binary representation of the link.
        /// </param>
        public DagPbLink(Stream stream)
        {
            Read(stream);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagPbLink"/> class from a <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="CodedInputStream"/> containing the Protobuf-encoded binary representation of the link.
        /// </param>
        public DagPbLink(CodedInputStream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;
            Read(stream);
        }

        /// <summary>
        ///   Reads the binary representation of the link from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        private void Read(Stream stream)
        {
            using (var ciStream = new CodedInputStream(stream, true))
            {
                Read(ciStream);
            }
        }

        /// <summary>
        ///   Reads the binary representation of the link from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="CodedInputStream"/> to read from.</param>
        private void Read(CodedInputStream stream)
        {
            while (!stream.IsAtEnd)
            {
                var tag = stream.ReadTag();
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        Identifier = Cid.Read(stream);
                        break;
                    case 2:
                        Name = stream.ReadString();
                        break;
                    case 3:
                        Size = stream.ReadInt64();
                        break;
                    default:
                        throw new InvalidDataException("Unknown field number");
                }
            }
        }

        /// <summary>
        ///   Writes the binary representation of the link to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public void Write(Stream stream)
        {
            var coStream = new CodedOutputStream(stream, leaveOpen: true);
            Write(coStream);
            coStream.Flush();
        }

        /// <summary>
        ///   Writes the binary representation of the link to the specified <see cref="CodedOutputStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="CodedOutputStream"/> to write to.</param>
        public void Write(CodedOutputStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            stream.WriteTag(1, WireFormat.WireType.LengthDelimited);
            Identifier.Write(stream);

            if (!string.IsNullOrEmpty(Name))
            {
                stream.WriteTag(2, WireFormat.WireType.LengthDelimited);
                stream.WriteString(Name);
            }

            stream.WriteTag(3, WireFormat.WireType.Varint);
            stream.WriteInt64(Size);
        }

        /// <summary>
        ///   Returns the IPFS dag-pb binary representation of the link as a byte array.
        ///   The result is cached for performance if the object is immutable.
        /// </summary>
        /// <returns>A byte array containing the binary representation of the link.</returns>
        public byte[] ToArray()
        {
            if (_memberCachedBytes is not null)
            {
                return _memberCachedBytes;
            }

            using var ms = new MemoryStream();
            Write(ms);
            _memberCachedBytes = ms.ToArray();
            return _memberCachedBytes;
        }
 
        /// <summary>
        ///   Returns a hash code for the current <see cref="DagPbLink"/>.
        /// </summary>
        /// <returns>A hash code for the current link.</returns>
        public override int GetHashCode() => HashCode.Combine(Identifier, Name, Size);
    }
}
