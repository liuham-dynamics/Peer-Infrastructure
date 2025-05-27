using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerData
{
    /// <summary>
    ///   Represents a node in a Merkle Directed Acyclic Graph (DAG), a fundamental structure for content-addressed and distributed data systems.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   A <b>MerkleNode</b> is a data structure used to model content-addressed graphs, such as those found in IPFS and IPLD.
    ///   Each node contains a sequence of navigable <see cref="Links"/> to other nodes and associated data
    ///   (accessible via <see cref="IDataBlock.DataBytes"/> or <see cref="IDataBlock.DataStream"/>).
    ///   </para>
    ///   <para>
    ///   The links are always non-null and sorted in ascending order by <see cref="IMerkleLink.Name"/> (treating <c>null</c> as an empty string).
    ///   </para>
    ///   <para>
    ///   This interface extends <see cref="IDataBlock"/>, providing a unique content identifier and access to the node's raw data.
    ///   </para>
    ///   <para>
    ///   Merkle DAGs are widely used in distributed systems for efficient, verifiable, and immutable data structures.
    ///   </para>
    /// </remarks>
    /// <typeparam name="Link">
    ///   The type of <see cref="IMerkleLink"/> used by this node.
    /// </typeparam>
    /// <seealso href="https://en.wikipedia.org/wiki/Directed_acyclic_graph"/>
    /// <seealso href="https://ipld.io/docs/motivation/data-to-data-structures/"/>
    /// <seealso href="https://docs.ipfs.tech/concepts/merkle-dag/#further-resources"/>
    /// <seealso href="https://proto.school/merkle-dags"/>
    public interface IMerkleNode<out Link> : IDataBlock where Link : IMerkleLink
    {
        /// <summary>
        /// Gets the links to other nodes in the DAG.
        /// </summary>
        /// <value>
        /// A non-null, ascending-sorted sequence of <typeparamref name="Link"/>.
        /// </value>
        /// <remarks>
        /// The links are sorted by <see cref="IMerkleLink.Name"/> (treating <c>null</c> as "").
        /// </remarks>
        IEnumerable<Link> Links { get; }

        /// <summary>
        /// Creates a link to this node.
        /// </summary>
        /// <param name="name">
        /// The <see cref="IMerkleLink.Name"/> for the link; defaults to an empty string.
        /// </param>
        /// <returns>
        /// A new <see cref="IMerkleLink"/> instance referencing this node.
        /// </returns>
        Link ToLink(string name = "");
    }
}

