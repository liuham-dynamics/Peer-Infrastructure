using PeerStack.Multiformat;

namespace PeerData
{
    /// <summary>
    /// A link to a node in IPLD.
    /// </summary>
    public interface IMerkleLink
    {
        /// <summary>
        /// The unique ID of the link.
        /// </summary>
        /// <value>
        ///   A <see cref="Cid"/> of the content.
        /// </value>
        Cid Identifier { get; }

        /// <summary>
        /// A name associated with the linked node.
        /// </summary>
        /// <value>A <see cref="string"/> or <b>null</b>.</value>
        /// <remarks>
        /// <note type="warning">
        /// IPLD considers a <b>null</b> name different from a <see cref="string.Empty"/> name;
        /// </note>
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// The serialized size (in bytes) of the linked node.
        /// </summary>
        /// <value>Number of bytes.</value>
        long Size { get; }
    }
}
