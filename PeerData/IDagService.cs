﻿using PeerStack.Multiformat;
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
    /// Manages the IPLD (linked data) Directed Acrylic Graph.
    /// </summary>
    /// <remarks>
    ///   The DAG API is a replacement of the <see cref="IObjectService"/>, which only supported 'dag-pb'.
    ///   This API supports other IPLD formats, such as cbor, ethereum-block, git, json...
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/blob/master/SPEC/DAG.md">Dag API spec</seealso>
    public interface IDagService
    {
        /// <summary>
        ///   Get an IPLD node.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="Cid"/> of the IPLD node.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous get operation. The task's value
        ///   contains the node's content as JSON.
        /// </returns>
        Task<JsonObject> GetAsync(Cid id, CancellationToken cancel = default);

        /// <summary>
        ///   Gets the content of an IPLD node.
        /// </summary>
        /// <param name="path">
        ///   A path, such as "cid", "/p2p/cid/" or "cid/a".
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous get operation. The task's value
        ///   contains the path's value.
        /// </returns>
        Task<JsonElement> GetAsync(string path, CancellationToken cancel = default);

        /// <summary>
        ///   Get an IPLD node of the specific type.
        /// </summary>
        /// <typeparam name="T">
        ///   The object's type.
        /// </typeparam>
        /// <param name="id">
        ///   The <see cref="Cid"/> of the IPLD node.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous get operation. The task's value
        ///   is a new instance of the <typeparamref name="T"/> class.
        /// </returns>
        Task<T> GetAsync<T>(Cid id, CancellationToken cancel = default);

        /// <summary>
        ///  Put JSON data as an IPLD node.
        /// </summary>
        /// <param name="data">
        ///   The JSON data to send to the network.
        /// </param>
        /// <param name="contentType">
        ///   The content type or format of the <paramref name="data"/>; such as "dag-pb" or "dag-cbor".
        ///   See <see cref="MultiCodec"/> for more details.  Defaults to "dag-cbor".
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <b>true</b> the <paramref name="data"/> is pinned to local storage and will not be
        ///   garbage collected.  The default is <b>true</b>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous put operation. The task's value is
        ///   the data's <see cref="Cid"/>.
        /// </returns>
        Task<Cid> PutAsync(JsonObject data, string contentType = ContentTypes.Cbor, 
                           string multiHash = MultiHash.DefaultAlgorithmName, 
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true, 
                           CancellationToken cancel = default);

        /// <summary>
        ///  Put a stream of JSON as an IPLD node.
        /// </summary>
        /// <param name="data">
        ///   The stream of JSON.
        /// </param>
        /// <param name="contentType">
        ///   The content type or format of the <paramref name="data"/>; such as "dag-pb" or "dag-cbor".
        ///   See <see cref="MultiCodec"/> for more details.  Defaults to "dag-cbor".
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <b>true</b> the <paramref name="data"/> is pinned to local storage and will not be
        ///   garbage collected.  The default is <b>true</b>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous put operation. The task's value is
        ///   the data's <see cref="Cid"/>.
        /// </returns>
        Task<Cid> PutAsync(Stream data, string contentType = ContentTypes.Cbor, 
                           string multiHash = MultiHash.DefaultAlgorithmName, 
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true, 
                           CancellationToken cancel = default);

        /// <summary>
        ///  Put an object as an IPLD node.
        /// </summary>
        /// <param name="data">
        ///   The object to add.
        /// </param>
        /// <param name="contentType">
        ///   The content type or format of the <paramref name="data"/>; such as "dag-pb" or "dag-cbor".
        ///   See <see cref="MultiCodec"/> for more details.  Defaults to "dag-cbor".
        /// </param>
        /// <param name="multiHash">
        ///   The <see cref="MultiHash"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="encoding">
        ///   The <see cref="MultiBase"/> algorithm name used to produce the <see cref="Cid"/>.
        /// </param>
        /// <param name="pin">
        ///   If <b>true</b> the <paramref name="data"/> is pinned to local storage and will not be
        ///   garbage collected.  The default is <b>true</b>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous put operation. The task's value is
        ///   the data's <see cref="Cid"/>.
        /// </returns>
        Task<Cid> PutAsync(object data, string contentType = ContentTypes.Cbor, 
                           string multiHash = MultiHash.DefaultAlgorithmName, 
                           string encoding = MultiBase.DefaultAlgorithmName, bool pin = true, 
                           CancellationToken cancel = default);

    }
}
