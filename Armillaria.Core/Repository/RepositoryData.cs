using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armillaria.Core.Repository
{
    /// <summary>
    ///   The statistics data for Repository />.
    /// </summary>
    public record class RepositoryData
    {
        /// <summary>
        ///   The number of blocks in the repository.
        /// </summary>
        /// <value>
        ///   The number of blocks in the repository.
        /// </value>
        public ulong BlockCount { get; set; }

        /// <summary>
        ///   The total number bytes in the repository.
        /// </summary>
        /// <value>
        ///   The total number bytes in the repository
        /// </value>
        public ulong TotalSize { get; set; }

        /// <summary>
        /// The maximum number of bytes allowed in the repository.
        /// </summary>
        /// <value>
        ///  Max bytes allowed in the repository
        /// </value>
        public ulong MaximumSize { get; set; }

        /// <summary>
        /// The fully qualified path to the repository.
        /// </summary>
        /// <value>
        ///   The directory of the  repository
        /// </value>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// The version number of the repository.
        /// </summary>
        /// <value>
        /// The version number of the repository
        /// </value>
        public string Version { get; set; } = string.Empty;

    }
}
