using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns
{
    /// <summary>
    ///   SecurityAlgorithmMetadata on a <see cref="SecurityAlgorithm"/>.
    /// </summary>
    /// <remarks>
    ///   Used by the <see cref="SecurityAlgorithmRegistry"/>.
    /// </remarks>
    public sealed class SecurityAlgorithmMetadata
    {
        /// <summary>
        ///   The cryptographic hash algorithm to use.
        /// </summary>
        public DigestType HashAlgorithm { get; set; }

        /// <summary>
        ///   Other _memberNames associated with the algorithm.
        /// </summary>
        public string[] OtherNames { get; set; } = new string[0];
    }
}
