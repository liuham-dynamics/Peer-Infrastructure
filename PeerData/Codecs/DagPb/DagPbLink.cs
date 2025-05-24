using Google.Protobuf;
using PeerStack.Multiformat;

namespace PeerData.Codecs.DagPb
{
    /// <summary>
    ///   A link to another node in the IPFS Merkle DAG.
    /// </summary>
    public class DagPbLink : IMerkleLink
    {
        /// <inheritdoc />
        public Cid Identifier { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public long Size { get; private set; }


        /// <summary>
        /// Create a new instance of <see cref="DagPbLink"/> class.
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
        /// Creates a new instance of the <see cref="DagPbLink"/> class from the specified <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">
        /// Some type of a Merkle link.
        /// </param>
        public DagPbLink(IMerkleLink link) : this(link.Name, link.Identifier, link.Size) { }

        /// <summary>
        ///   Creates a new instance of the <see cref="DagPbLink"/> class from the
        ///   specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the binary representation of the
        ///   <b>DagJsonLink</b>.
        /// </param>
        public DagPbLink(Stream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;

            Read(stream);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="DagPbLink"/> class from the
        ///   specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="CodedInputStream"/> containing the binary representation of the
        ///   <b>DagJsonLink</b>.
        /// </param>
        public DagPbLink(CodedInputStream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;

            Read(stream);
        }

        /// <inheritdoc />
        private void Read(Stream stream)
        {
            using (var ciStream = new CodedInputStream(stream, true))
            {
                Read(ciStream);
            }
        }

        /// <inheritdoc />
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
        /// Writes the binary representation of the link to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to write to.
        /// </param>
        public void Write(Stream stream)
        {
            using var coStream = new CodedOutputStream(stream, true);
            Write(coStream);
        }

        /// <summary>
        /// Writes the binary representation of the link to the specified <see cref="CodedOutputStream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        public void Write(CodedOutputStream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteTag(1, WireFormat.WireType.LengthDelimited);
            Identifier.Write(stream);

            if (Name is not null)
            {
                stream.WriteTag(2, WireFormat.WireType.LengthDelimited);
                stream.WriteString(Name);
            }

            stream.WriteTag(3, WireFormat.WireType.Varint);
            stream.WriteInt64(Size);
        }

        /// <summary>
        /// Returns the IPFS binary representation as a byte array.
        /// </summary>
        /// <returns>
        /// A byte array.
        /// </returns>
        public byte[] ToArray()
        {
            using var ms = new MemoryStream();
            Write(ms);
            return ms.ToArray();
        }
         
    }
}
