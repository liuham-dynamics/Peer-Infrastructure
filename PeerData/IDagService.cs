using PeerStack.Multiformat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using PeerStack;

namespace PeerData
{
    /// <summary>
    ///   Provides an abstraction for managing and interacting with an IPLD (InterPlanetary Linked Data) Directed Acyclic Graph (DAG).
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The <b>IDagService</b> interface defines a high-level API for reading and writing IPLD nodes in a content-addressed, format-agnostic graph.
    ///   It supports multiple IPLD codecs (such as dag-cbor, dag-pb, dag-json, ethereum-block, git, etc.), enabling flexible and interoperable data storage.
    ///   </para>
    ///   <para>
    ///   This API supersedes the legacy service, which was limited to the 'dag-pb' format.
    ///   </para>
    ///   <para>
    ///   All operations are asynchronous and support cancellation via <see cref="CancellationToken"/>.
    ///   </para>
    ///   <para>
    ///   For more details, see the
    ///   <see href="https://github.com/ipfs/interface-ipfs-core/blob/master/SPEC/DAG.md">IPFS DAG API specification</see>.
    ///   </para>
    /// </remarks>
    public interface IDagService
    {
        /// <summary>
        ///   Retrieves an IPLD node by its content identifier (CID) and returns its content as a JSON object.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="Cid"/> of the IPLD node to retrieve.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result contains the node's content as a <see cref="JsonObject"/>.
        /// </returns>
        Task<JsonObject> GetAsync(Cid id, CancellationToken cancel = default);

        /// <summary>
        ///   Retrieves the value at a specific path within the IPLD DAG, returning the result as a JSON element.
        /// </summary>
        /// <param name="path">
        ///   A path string, such as "cid", "/p2p/cid/", or "cid/a", identifying the node or value to retrieve.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result contains the value at the specified path as a <see cref="JsonElement"/>.
        /// </returns>
        Task<JsonElement> GetAsync(string path, CancellationToken cancel = default);

        /// <summary>
        ///   Retrieves an IPLD node by its CID and deserializes it to a strongly-typed object.
        /// </summary>
        /// <typeparam name="T">
        ///   The type to which the node's content will be deserialized.
        /// </typeparam>
        /// <param name="id">
        ///   The <see cref="Cid"/> of the IPLD node to retrieve.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result is an instance of <typeparamref name="T"/>.
        /// </returns>
        Task<T> GetAsync<T>(Cid id, CancellationToken cancel = default);

        /// <summary>
        ///   Stores a JSON object as an IPLD node in the DAG.
        /// </summary>
        /// <param name="data">
        ///   The JSON data to store.
        /// </param>
        /// <param name="contentType">
        ///   The IPLD codec or format for the data (e.g., "dag-pb", "dag-cbor"). Defaults to "dag-cbor".
        ///   See <see cref="MultiCodec"/> for supported codecs.
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to generate the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> encoding used for the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <c>true</c>, the data is pinned to local storage and will not be garbage collected. Defaults to <c>true</c>.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result is the <see cref="Cid"/> of the stored data.
        /// </returns>
        Task<Cid> PutAsync(JsonObject data, string contentType = ContentTypes.Cbor,
                           string multiHash = MultiHash.DefaultAlgorithmName,
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true,
                           CancellationToken cancel = default);

        /// <summary>
        ///   Stores a stream of JSON data as an IPLD node in the DAG.
        /// </summary>
        /// <param name="data">
        ///   The stream containing JSON data to store.
        /// </param>
        /// <param name="contentType">
        ///   The IPLD codec or format for the data (e.g., "dag-pb", "dag-cbor"). Defaults to "dag-cbor".
        ///   See <see cref="MultiCodec"/> for supported codecs.
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to generate the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> encoding used for the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <c>true</c>, the data is pinned to local storage and will not be garbage collected. Defaults to <c>true</c>.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result is the <see cref="Cid"/> of the stored data.
        /// </returns>
        Task<Cid> PutAsync(Stream data, string contentType = ContentTypes.Cbor,
                           string multiHash = MultiHash.DefaultAlgorithmName,
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true,
                           CancellationToken cancel = default);

        /// <summary>
        ///   Stores an object as an IPLD node in the DAG.
        /// </summary>
        /// <param name="data">
        ///   The object to serialize and store.
        /// </param>
        /// <param name="contentType">
        ///   The IPLD codec or format for the data (e.g., "dag-pb", "dag-cbor"). Defaults to "dag-cbor".
        ///   See <see cref="MultiCodec"/> for supported codecs.
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to generate the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> encoding used for the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <c>true</c>, the data is pinned to local storage and will not be garbage collected. Defaults to <c>true</c>.
        /// </param>
        /// <param name="cancel">
        ///   A <see cref="CancellationToken"/> to cancel the operation. If cancelled, a <see cref="TaskCanceledException"/> is thrown.
        /// </param>
        /// <returns>
        ///   A task representing the asynchronous operation. The result is the <see cref="Cid"/> of the stored data.
        /// </returns>
        Task<Cid> PutAsync(object data, string contentType = ContentTypes.Cbor,
                           string multiHash = MultiHash.DefaultAlgorithmName,
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true,
                           CancellationToken cancel = default);
    }
}

