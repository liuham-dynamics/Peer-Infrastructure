using PeerTalk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Kbucket
{
    /// <summary>
    /// Provides data for events that review contacts in a KBucket.
    /// </summary>
    /// <seealso cref="KBucket{T}.Ping"/>
    public class ReviewEventArgs<T> : EventArgs where T : IContact
    {
        /// <summary>
        /// Gets or sets the collection of the oldest contacts that are candidates for review.
        /// </summary>
        public IEnumerable<T> Oldest { get; set; } = [];

        /// <summary>
        /// A new contact that wants to be added.
        /// </summary>
        public required T Newest { get; set; }
    }
}
