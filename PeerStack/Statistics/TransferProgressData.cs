using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Statistics
{
    /// <summary>
    /// Reports the <see cref="IProgress{T}">progress</see> of a transfer operation.
    /// </summary>
    public record class TransferProgressData
    {
        /// <summary>
        /// The name of the item being transferred.
        /// </summary>
        /// <value>
        /// Typically, a relative file path.
        /// </value>
        public string Name {get; set;} = string.Empty;

        /// <summary>
        /// The cumulative number of bytes transferred for the <see cref="Name"/>.
        /// </summary>
        public ulong Bytes { get; set; }
    }
}
