using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armillaria.Core.Repository
{
    /// <summary>
    ///   Manages all the blocks stored locally.
    /// </summary>
    /// <seealso cref="IBlockService"/>
    public interface IRepositoryService
    {

        /// <summary>
        /// Get statistics on the repository.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's result is
        ///   the current <see cref="RepositoryData"/>.
        /// </returns>
        /// <remarks>
        ///   Same as <see cref="IStatsApi.RepositoryAsync(CancellationToken)"/>.
        /// </remarks>
        Task<RepositoryData> StatisticsAsync(CancellationToken cancel = default);

        /// <summary>
        ///   Verify all blocks in repo are not corrupted.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///  True if successful
        /// </returns>
        Task<bool> VerifyAsync(CancellationToken cancel = default);

        /// <summary>
        ///   Gets the version number of the repo.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's result is
        ///   the version number of the data block repository.
        /// </returns>
        Task<string> VersionAsync(CancellationToken cancel = default);


        /// <summary>
        ///   Perform a garbage collection sweep on the repo.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   True if successful
        /// </returns>
        Task<bool> RemoveGarbageAsync(CancellationToken cancel = default);

    }
}
