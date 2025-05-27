using PeerStack.Multiformat;

namespace PeerData
{
    /// <summary>
    ///   Represents a generic block of data with a unique content identifier, used in content-addressed and distributed systems.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   An <b>IDataBlock</b> abstracts a unit of data that is identified by a unique <see cref="Identifier"/> (<see cref="Cid"/>)
    ///   and provides access to its raw contents as either a byte array (<see cref="DataBytes"/>) or a stream (<see cref="DataStream"/>).
    ///   </para>
    ///   <para>
    ///   This interface is designed for use in IPLD, IPFS, and similar systems where the structure and meaning of the data are not interpreted by the block itself.
    ///   It is useful to refer to these as "blocks" in distributed, content-addressed storage, regardless of the actual data format.
    ///   </para>
    ///   <para>
    ///   The <see cref="Identifier"/> is a self-describing, content-addressed identifier that ensures data integrity and deduplication.
    ///   </para>
    /// </remarks>
    public interface IDataBlock
    {
        /// <summary>
        /// Gets the unique content identifier (CID) for this data block.
        /// </summary>
        /// <value>
        ///   A <see cref="Cid"/> representing the content address of the data.
        /// </value>
        Cid Identifier { get; }

        /// <summary>
        /// Gets the size (in bytes) of the data block.
        /// </summary>
        /// <value>
        ///   The number of bytes in the data block.
        /// </value>
        long Size { get; }

        /// <summary>
        /// Gets the contents of the data block as a byte array.
        /// </summary>
        /// <remarks>
        ///   This property is never <c>null</c>.
        /// </remarks>
        /// <value>
        ///   The contents as a sequence of bytes.
        /// </value>
        ReadOnlyMemory<byte> DataBytes { get; }

        /// <summary>
        /// Gets the contents of the data block as a stream of bytes.
        /// </summary>
        /// <value>
        ///   The contents as a readable <see cref="Stream"/>.
        /// </value>
        Stream DataStream { get; }
    }
}

