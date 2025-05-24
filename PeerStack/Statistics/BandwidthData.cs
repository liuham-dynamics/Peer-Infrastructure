using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Statistics
{
    /// <summary>
    /// The statistics for <see cref="IStatisticService.BandwidthAsync"/>.
    /// </summary>
    public record class BandwidthData
    {
        /// <summary>
        ///   The number of bytes received.
        /// </summary>
        public ulong TotalIn;

        /// <summary>
        ///   The number of bytes sent.
        /// </summary>
        public ulong TotalOut;

        /// <summary>
        ///   TODO
        /// </summary>
        public double RateIn;

        /// <summary>
        ///   TODO
        /// </summary>
        public double RateOut;
    }
}
