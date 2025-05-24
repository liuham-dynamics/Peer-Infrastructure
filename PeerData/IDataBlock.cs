using PeerStack.Multiformat;

namespace PeerData
{
    /// <summary>
    /// Some data that is stored in a format that do not care about what is being stored.
    /// </summary>
    /// <remarks>
    /// A <b>DataBlock</b> has an <see cref="Identifier">unique ID</see>
    /// and some data (<see cref="IDataBlock.DataBytes"/>
    /// or <see cref="IDataBlock.DataStream"/>).
    /// <para>
    /// It is useful to talk about them as "blocks" in IPLD and other 
    /// things that do not care about what is being stored.
    /// </para>
    /// </remarks>
    public interface IDataBlock
    {

        /// <summary>
        /// The unique ID of the data.
        /// </summary>
        /// <value>
        ///   A <see cref="Cid"/> of the content.
        /// </value>
        Cid Identifier { get; }

        /// <summary>
        /// The size (in bytes) of the data.
        /// </summary>
        /// <value>Number of bytes.</value>
        long Size { get; }

        /// <summary>
        ///   Contents as a byte array.
        /// </summary>
        /// <remarks>
        ///   It is never <b>null</b>.
        /// </remarks>
        /// <value>
        ///   The contents as a sequence of bytes.
        /// </value>
        byte[] DataBytes { get; }

        /// <summary>
        ///   Contents as a stream of bytes.
        /// </summary>
        /// <value>
        ///   The contents as a stream of bytes.
        /// </value>
        Stream DataStream { get; }

    }
}
