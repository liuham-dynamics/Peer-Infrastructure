using PeerStack.Multiformat;
using PeerStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Core.Discovery
{
    /// <summary>
    /// Find information about a peer.
    /// </summary>
    /// <remarks>
    ///   No documentation is currently available.  See the
    ///   <see href="https://godoc.org/github.com/libp2p/go-libp2p-routing">code</see>.
    /// </remarks>
    public interface IPeerRouting
    {
        /// <summary>
        ///   Information about an libp2p peer.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="MultiHash"/> ID of the peer.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is NOT raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation that returns
        ///   the <see cref="Peer"/> information or a closer peer.
        /// </returns>
        Task<Peer> FindPeerAsync(MultiHash id, CancellationToken cancel = default);
    }
}
