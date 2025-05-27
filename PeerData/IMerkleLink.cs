using PeerStack.Multiformat;

namespace PeerData
{
    /// <summary>
    ///   Represents a link to a node in an IPLD (InterPlanetary Linked Data) Merkle DAG.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   An <b>IMerkleLink</b> provides a reference to another node in a Merkle Directed Acyclic Graph (DAG), such as those used in IPFS and IPLD.
    ///   Each link contains a unique content identifier (<see cref="Identifier"/>), an optional name (<see cref="Name"/>), and the serialized size (<see cref="Size"/>) of the linked node.
    ///   </para>
    ///   <para>
    ///   The <see cref="Identifier"/> is a <see cref="Cid"/> (Content Identifier), which is a self-describing, content-addressed identifier for distributed systems.
    ///   </para>
    ///   <para>
    ///   The <see cref="Name"/> property may be <c>null</c> or a string. Note that IPLD treats <c>null</c> and <see cref="string.Empty"/> as distinct values.
    ///   </para>
    ///   <para>
    ///   The <see cref="Size"/> property indicates the serialized size (in bytes) of the linked node, which can be used for efficient traversal and block management.
    ///   </para>
    /// </remarks>
    public interface IMerkleLink
    {
        /// <summary>
        /// Gets the unique content identifier (CID) of the linked node.
        /// </summary>
        /// <value>
        ///   A <see cref="Cid"/> representing the content address of the linked node.
        /// </value>
        Cid Identifier { get; }

        /// <summary>
        /// Gets the name associated with the linked node.
        /// </summary>
        /// <value>
        ///   A <see cref="string"/> or <c>null</c>.
        /// </value>
        /// <remarks>
        ///   <note type="warning">
        ///   IPLD considers a <c>null</c> name different from an empty string (<see cref="string.Empty"/>).
        ///   </note>
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the serialized size (in bytes) of the linked node.
        /// </summary>
        /// <value>
        ///   The number of bytes representing the linked node.
        /// </value>
        long Size { get; }
    }
}

