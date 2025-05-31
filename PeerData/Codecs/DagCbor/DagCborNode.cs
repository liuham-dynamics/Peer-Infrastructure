using PeerData.Codecs.DagJson;
using PeerStack.Multiformat;
using PeterO.Cbor;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PeerData.Codecs.DagCbor
{
    [DataContract]
     public record class DagCborNode : IMerkleNode<IMerkleLink>
    {
        private byte[]? _memberCachedBytes;

        private Cid id = new();
        private long? size;
        private string hashAlgorithm = MultiHash.DefaultAlgorithmName;

       
        private readonly ImmutableArray<IMerkleLink> links;

        /// <inheritdoc />
   
        public Cid Identifier
        {
            get
            {
                if (id is null || string.IsNullOrEmpty(id.Encoding))
                {
                    ComputeHash();
                }
                return id!;
            }
            private set
            {
                id = value;
                if (id is not null & !string.IsNullOrEmpty(id?.Hash?.Coder?.Name))
                {
                    hashAlgorithm = id!.Hash.Coder.Name!;
                }
            }
        }

        /// <summary>
        ///   Gets the serialized size in bytes of the node.
        /// </summary>
        
        public long Size
        {
            get
            {
                if (!size.HasValue)
                {
                    ComputeSize();
                }
                return size!.Value;
            }
        }

        /// <inheritdoc />
      
        public ReadOnlyMemory<byte> DataBytes { get; private set; }

        /// <inheritdoc />
        public Stream DataStream => new MemoryStream(DataBytes.ToArray(), false);

        /// <inheritdoc />
        public IEnumerable<IMerkleLink> Links => links;

        /// <summary>
        ///   Computes and caches the content identifier (CID) and size for this node using the specified hash algorithm.
        /// </summary>
        private void ComputeHash()
        {
            var bytes = _memberCachedBytes ?? ToArrayAsync().GetAwaiter().GetResult();
            using var ms = new MemoryStream(bytes, false);
            size = ms.Length;
            ms.Position = 0;
            id = MultiHash.ComputeHash(ms, hashAlgorithm);
        }

        /// <summary>
        ///   Computes and caches the serialized size of this node.
        /// </summary>
        private void ComputeSize()
        {
            var bytes = _memberCachedBytes ?? ToArrayAsync().GetAwaiter().GetResult();
            size = bytes.Length;
        }

        
        public DagCborNode(ReadOnlyMemory<byte> data, IEnumerable<IMerkleLink>? links = null, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
        {
            DataBytes = data;
            if (links is not null)
            {
                var sortedLinks = links.OrderBy(link => link.Name ?? "");
                this.links = [.. sortedLinks];
            }
            else
            {
                this.links = ImmutableArray<IMerkleLink>.Empty;
            }
            this.hashAlgorithm = hashAlgorithm;
        }

      
        public DagCborNode(Stream stream)
        {
            DataBytes = Array.Empty<byte>();
            size = DataBytes.Length;
            links = ImmutableArray<IMerkleLink>.Empty;
            Read(stream);
        }

        /// <summary>
        ///   Returns a new <see cref="DagCborNode"/> with the specified link added.
        /// </summary>
        /// <param name="link">The link to add.</param>
        /// <returns>A new <see cref="DagCborNode"/> with the link added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagCborNode AddLink(IMerkleLink link)
        {
            var newLinks = links.Add(link).Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagCborNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagCborNode"/> with the specified links added.
        /// </summary>
        /// <param name="links">The sequence of links to add.</param>
        /// <returns>A new <see cref="DagCborNode"/> with the links added.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        /// </remarks>
        public DagCborNode AddLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = this.links.AddRange(links).Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagCborNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagCborNode"/> with the specified link removed.
        /// </summary>
        /// <param name="link">The link to remove.</param>
        /// <returns>A new <see cref="DagCborNode"/> with the link removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if the link does not exist.
        /// </remarks>
        public DagCborNode RemoveLink(IMerkleLink link)
        {
            var newLinks = links.Remove(link);
            return new DagCborNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Returns a new <see cref="DagCborNode"/> with the specified links removed.
        /// </summary>
        /// <param name="links">The sequence of links to remove.</param>
        /// <returns>A new <see cref="DagCborNode"/> with the links removed.</returns>
        /// <remarks>
        ///   The node is immutable; this method does not modify the current instance.
        ///   No exception is thrown if any of the links do not exist.
        /// </remarks>
        public DagCborNode RemoveLinks(IEnumerable<IMerkleLink> links)
        {
            var newLinks = this.links.RemoveRange(links)
                               .Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return new DagCborNode(DataBytes, newLinks, hashAlgorithm);
        }

        /// <summary>
        ///   Reads the CBOR representation of the node from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        private void Read(Stream stream)
        {
           
            ArgumentNullException.ThrowIfNull(stream);
             
            var cborObject = CBORObject.Read(stream);
            if (cborObject.Type != CBORType.Map)
            {
                throw new CBORException("Invalid Dag-CBOR format.");
            }

            if (!cborObject.HasOneTag(42))
            {
                throw new InvalidOperationException("Invalid Dag-CBOR object: expected a single tag for CID.");
            }

            _memberCachedBytes = cborObject[0].GetByteString();
            Identifier = Cid.Read(_memberCachedBytes);
            size = cborObject[1].AsInt64Value();
            DataBytes =  cborObject[2].GetByteString();
            
            var cborEntries = cborObject[3].Values;
            links.Clear();
            foreach (var entry in cborEntries)
            {
               //
                links.Add(new DagCborLink(Cid.Read(entry[0].GetByteString()), entry[1].AsString(), entry[2].AsInt64Value()));
            }
        }

        /// <summary>
        ///   Writes the CBOR representation of the node to the specified <see cref="Stream"/> asynchronously.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public async Task WriteAsync(Stream stream)
        {
            await WriteAsync(stream, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        ///   Writes the CBOR representation of the node to the specified <see cref="Stream"/> asynchronously, with cancellation support.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            //
            var cborLinks = CBORObject.NewMap();
            foreach (var link in Links)
            {
                var cborLink = CBORObject.NewArray()
                    .Add(link.Identifier.AsSpan().ToArray()).WithTag(42)
                    .Add(link.Name)
                    .Add(link.Size);
                cborLinks.Add(link.Name, cborLink);
            }

            // The following creates a CBOR map and adds
            // several kinds of objects to it
            var cbor = CBORObject.NewArray()
               .Add(Identifier.AsSpan().ToArray()).WithTag(42)
               .Add(Size)
               .Add(DataBytes)
               .Add(cborLinks);

            // The following converts the map to CBOR
            byte[] bytes = cbor.EncodeToBytes();

            // Write the CBOR bytes to the stream
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IMerkleLink ToLink(string name = "") => new DagJsonLink(name, Identifier, Size);

        /// <summary>
        ///   Returns the dag-cbor binary representation of the node as a byte array.
        ///   The result is cached for performance if the object is immutable.
        /// </summary>
        /// <returns>A byte array containing the JSON representation of the node.</returns>
        public async Task<byte[]> ToArrayAsync()
        {
            if (_memberCachedBytes is not null)
                return _memberCachedBytes;

            using var ms = new MemoryStream();
            await WriteAsync(ms).ConfigureAwait(false);
            _memberCachedBytes = ms.ToArray();
            return _memberCachedBytes;
        }
    }
}
