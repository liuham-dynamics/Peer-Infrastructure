﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerData
{
    /// <summary>
    ///   A Directed Acyclic Graph (DAG).
    /// </summary>
    /// <remarks>
    ///   A <b>MerkleNode</b> has a sequence of navigable <see cref="Links"/> and some 
    ///   data (<see cref="IDataBlock.DataBytes"/> or <see cref="IDataBlock.DataStream"/>).
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
        /// Links to other nodes.
        /// </summary>
        /// <value>
        /// A sequence of <typeparamref name="Link"/>.
        /// </value>
        /// <remarks>
        /// It is never <b>null</b>.
        /// <para>
        /// The links are sorted ascending by <see cref="IMerkleLink.Name"/>. 
        /// A <b>null</b> name is compared as "".
        /// </para>
        /// </remarks>
        IEnumerable<Link> Links { get; }

        /// <summary>
        /// Returns a link to the node.
        /// </summary>
        /// <param name="name">
        /// A <see cref="IMerkleLink.Name"/> for the link; defaults to "".
        /// </param>
        /// <returns>
        /// A new <see cref="IMerkleLink"/> to the node.
        /// </returns>
        Link ToLink(string name = "");
    }
}
