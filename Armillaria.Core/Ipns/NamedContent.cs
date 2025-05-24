using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Core.Ipns
{
    /// <summary>
    /// Content that has an associated name.
    /// </summary>
    public class NamedContent
    {
        /// <summary>
        ///   Path to the name.
        /// </summary>
        /// <value>
        ///   Typically <c>/ipns/...</c>.
        /// </value>
        public string NamePath { get; set; } = string.Empty;

        /// <summary>
        ///   Path to the content.
        /// </summary>
        /// <value>
        ///   Typically <c>/p2p/...</c>.
        /// </value>
        public string ContentPath { get; set; } = string.Empty;
    }
}
