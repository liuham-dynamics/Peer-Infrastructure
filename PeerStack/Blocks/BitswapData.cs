using PeerStack.Multiformat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Blocks
{
    /// <summary>
    ///   The statistics for Bitswap exchange.
    /// </summary>
    public record class BitswapData
    {

        /// <summary>
        ///   The content that is wanted.
        /// </summary>
        public IEnumerable<Cid> WantList { get; set; } = [];

        /// <summary>
        ///   The known peers.
        /// </summary>
        public IEnumerable<MultiHash> Peers { get; set; } = [];

        /// <summary>
        /// The number of blocks sent by other peers.
        /// </summary>
        public ulong BlocksReceived { get; set; }

        /// <summary>
        /// The number of bytes sent by other peers.
        /// </summary>
        public ulong DataReceived { get; set; }

        /// <summary>
        /// The number of blocks sent to other peers.
        /// </summary>
        public ulong BlocksSent { get; set; }

        /// <summary>
        /// The number of bytes sent to other peers.
        /// </summary>
        public ulong DataSent { get; set; }

        /// <summary>
        /// The number of duplicate blocks sent by other peers.
        /// </summary>
        /// <remarks>
        /// A duplicate block is a block that is already stored in the local repository.
        /// </remarks>
        public ulong DuplicateBlocksReceived { get; set; }

        /// <summary>
        /// The number of duplicate bytes sent by other peers.
        /// </summary>
        /// <remarks>
        /// A duplicate data is a data segment that is already stored in the local repository.
        /// </remarks>
        public ulong DuplicateDataReceived { get; set; }
    }
}
