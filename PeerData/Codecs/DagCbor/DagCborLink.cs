using Org.BouncyCastle.Ocsp;
using PeerStack.Multiformat;
using PeterO.Cbor;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerData.Codecs.DagCbor
{
  
       public record class DagCborLink : IMerkleLink
    {
        private byte[]? _memberCachedBytes;

        /// <inheritdoc />
        
        public Cid Identifier { get; private set; }

        /// <inheritdoc />
        
        public string Name { get; private set; }

        /// <inheritdoc />
        
        public long Size { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class with the specified name, identifier, and size.
        /// </summary>
        /// <param name="name">The name associated with the linked node.</param>
        /// <param name="id">The <see cref="Cid"/> of the linked node.</param>
        /// <param name="size">The serialized size (in bytes) of the linked node.</param>
       
        public DagCborLink(Cid id, string name, long size)
        {
            Identifier = id;   
            Name = name;
            Size = size;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class from an existing <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">An existing Merkle link to copy values from.</param>
        public DagCborLink(IMerkleLink link) : this(link.Name, link.Identifier, link.Size) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="DagJsonLink"/> class from a <see cref="Stream"/> containing its JSON representation.
        /// </summary>
        /// <param name="stream">
        ///   A <see cref="Stream"/> containing the JSON representation of the <b>DagJsonLink</b>.
        /// </param>
        public DagCborLink(Stream stream)
        {
            Identifier = new Cid();
            Name = string.Empty;
            Read(stream);
        }
         
        /// <summary>
        ///   Reads the JSON representation of the link from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        private void Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
             
            var cborObject = CBORObject.Read(stream);
            if (cborObject.Type != CBORType.Array)
            {
                throw new CBORException("Invalid Dag-CBOR format.");
            }
             
            if (!cborObject.HasOneTag(42))
            {
                throw new InvalidOperationException("Invalid Dag-CBOR object: expected a single tag for CID.");
            }

            _memberCachedBytes = cborObject[0].GetByteString();
            Identifier = Cid.Read(_memberCachedBytes);
            Name = cborObject[1].AsString();
            Size  = cborObject[2].AsInt64Value();
        }

        /// <summary>
        ///   Writes the JSON representation of the link to the specified <see cref="Stream"/> asynchronously.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public async Task WriteAsync(Stream stream)
        {
            await WriteAsync(stream, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        ///   Writes the JSON representation of the link to the specified <see cref="Stream"/> asynchronously, with cancellation support.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            // The following creates a CBOR map and adds
            // several kinds of objects to it
            var cbor = CBORObject.NewArray()
               .Add(Identifier.AsSpan().ToArray()).WithTag(42)
               .Add(Name)
               .Add(Size);

            // The following converts the map to CBOR
            byte[] bytes = cbor.EncodeToBytes();

            // Write the CBOR bytes to the stream
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///   Returns the JSON binary representation of the link as a byte array.
        ///   The result is cached for performance if the object is immutable.
        /// </summary>
        /// <returns>A byte array containing the JSON representation of the link.</returns>
        public async Task<byte[]> ToArrayAsync()
        {
            if (_memberCachedBytes is not null)
            {
                return _memberCachedBytes;
            }

            using var ms = new MemoryStream();
            await WriteAsync(ms).ConfigureAwait(false);
            _memberCachedBytes = ms.ToArray();
            return _memberCachedBytes;
        }

        /// <summary>
        ///   Returns a hash code for the current <see cref="DagJsonLink"/>.
        /// </summary>
        /// <returns>A hash code for the current link.</returns>
        public override int GetHashCode() => HashCode.Combine(Identifier, Name, Size);
    }
}
