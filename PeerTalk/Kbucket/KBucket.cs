using PeerTalk.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   Represents a Kademlia Distributed Hash Table (DHT) k-bucket for storing and managing contact (peer node) information.
    /// </summary>
    /// <typeparam name="T">
    ///   The contact type, which must implement <see cref="IContact"/>.
    /// </typeparam>
    /// <remarks>
    ///   All public methods and properties are thread-safe. This class organizes contacts in a binary tree of buckets, supporting efficient lookups and updates based on XOR distance.
    /// </remarks>
    public class KBucket<T> : ICollection<T> where T : class, IContact
    {
        private readonly ReaderWriterLockSlim _memberLock = new(LockRecursionPolicy.SupportsRecursion);
        private byte[] localContactId = new byte[20];

        /// <summary>
        ///   Gets or sets the maximum number of contacts allowed in a single bucket.
        /// </summary>
        public int ContactsPerBucket { get; set; } = 20;

        /// <summary>
        /// Gets or sets the number of contacts to ping when a bucket that should not be split becomes full.
        /// The value depends on the operating system: 3 for Android/iOS, 6 for Windows/Linux.
        /// </summary>
        public int ContactsToPing { get; set; } = OperatingSystem.IsAndroid()
                                                | OperatingSystem.IsIOS()
                                                ? 3 : 6;

        /// <summary>
        /// Gets or sets the identifier of the local contact/peer.
        /// </summary>
        public byte[] LocalContactId
        {
            get
            {
                if (localContactId.Length == 0)
                {
                    localContactId = new byte[20];
                    Random.Shared.NextBytes(localContactId);
                }
                return localContactId;
            }
            set
            {
                localContactId = value;
            }
        }

        /// <summary>
        /// Gets the total number of contacts stored in the k-bucket.
        /// </summary>
        public int Count
        {
            get
            {
                _memberLock.EnterReadLock();
                try
                {
                    return Root.DeepCount();
                }
                finally
                {
                    _memberLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only. Always returns <c>false</c>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the root node of the binary tree structure used to organize contacts.
        /// </summary>
        public Bucket<T> Root { get; private set; } = new();

        /// <summary>
        ///   Occurs when a bucket needs to be split but cannot be split further.
        ///   Used to trigger a review (ping) of the oldest contacts.
        /// </summary>
        public event EventHandler<ReviewEventArgs<T>>? Ping;

        /// <summary>
        /// Gets or sets the function used to determine which contact to keep when two contacts have the same ID.
        /// </summary>
        public Func<T, T, T> Arbiter { get; set; } = DefaultAbiter;

        /// <summary>
        /// The default arbiter function, which always selects the incumbent contact when two contacts have the same ID.
        /// </summary>
        public static T DefaultAbiter(T incumbent, T candidate) => incumbent;

        /// <summary>
        /// Calculates the XOR distance between two contacts.
        /// </summary>
        public BigInteger Distance(T a, T b)
        {
            Validate(a);
            Validate(b);
            return Distance(a.Identifier, b.Identifier);
        }

        /// <summary>
        /// Calculates the XOR distance between two contact identifiers.
        /// </summary>
        public BigInteger Distance(byte[] a, byte[] b)
        {
            // Use Span for efficient byte operations
            var min = Math.Min(a.Length, b.Length);
            var max = Math.Max(a.Length, b.Length);
            BigInteger distance = 0;
            for (int i = 0; i < min; ++i)
            {
                distance = (distance << 8) | (uint)(a[i] ^ b[i]);
            }
            for (int i = min; i < max; ++i)
            {
                distance = (distance << 8) | 0xFF;
            }
            return distance;
        }

        /// <summary>
        /// Returns the contacts closest to the specified contact, ordered by XOR distance.
        /// </summary>
        public IEnumerable<T> Closest(T contact)
        {
            Validate(contact);
            return Closest(contact.Identifier);
        }

        /// <summary>
        /// Returns the contacts closest to the specified identifier, ordered by XOR distance.
        /// </summary>
        public IEnumerable<T> Closest(byte[] id)
        {
            // Use a local list to avoid multiple enumerations
            List<T> allContacts;
            _memberLock.EnterReadLock();
            try
            {
                allContacts = Root.AllContacts().ToList();
            }
            finally
            {
                _memberLock.ExitReadLock();
            }
            return allContacts
                .Select(c => (distance: Distance(c.Identifier, id), contact: c))
                .OrderBy(x => x.distance)
                .Select(x => x.contact);
        }

        /// <summary>
        /// Adds a contact to the k-bucket. If the bucket is full and cannot be split, triggers the <see cref="Ping"/> event.
        /// </summary>
        public void Add(T item)
        {
            Validate(item);
            bool added;
            ReviewEventArgs<T>? e;

            _memberLock.EnterWriteLock();
            try
            {
                added = _Add(item, out e);
            }
            finally
            {
                _memberLock.ExitWriteLock();
            }

            if (!added && e is not null)
            {
                Ping?.Invoke(this, e);
            }
        }

        /// <summary>
        /// Removes all contacts from the k-bucket.
        /// </summary>
        public void Clear()
        {
            _memberLock.EnterWriteLock();
            try
            {
                Root = new();
            }
            finally
            {
                _memberLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether the specified contact exists in the k-bucket.
        /// </summary>
        public bool Contains(T item)
        {
            Validate(item);
            _memberLock.EnterReadLock();
            try
            {
                return _Get(item.Identifier) is not null;
            }
            finally
            {
                _memberLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Attempts to retrieve the contact associated with the specified identifier.
        /// </summary>
        public bool TryGet(byte[] id, out T? contact)
        {
            _memberLock.EnterReadLock();
            try
            {
                contact = _Get(id);
                return contact is not null;
            }
            finally
            {
                _memberLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the contacts of the k-bucket to an array, starting at the specified array index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _memberLock.EnterReadLock();
            try
            {
                foreach (var contact in Root.AllContacts())
                {
                    array[arrayIndex++] = contact;
                }
            }
            finally
            {
                _memberLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the contacts in the k-bucket.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            List<T> snapshot;
            _memberLock.EnterReadLock();
            try
            {
                snapshot = Root.AllContacts().ToList();
            }
            finally
            {
                _memberLock.ExitReadLock();
            }
            return snapshot.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Removes the specified contact from the k-bucket.
        /// </summary>
        public bool Remove(T item)
        {
            Validate(item);
            _memberLock.EnterWriteLock();
            try
            {
                return _Remove(item.Identifier);
            }
            finally
            {
                _memberLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Validates that the contact and its identifier are not null or empty.
        /// </summary>
        private static void Validate(T contact)
        {
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            if (contact.Identifier == null || contact.Identifier.Length == 0)
            {
                throw new ArgumentNullException(nameof(contact), "Contact identifier cannot be null or empty.");
            }
        }

        /// <summary>
        /// Retrieves a contact by its exact identifier.
        /// </summary>
        private T? _Get(byte[] id)
        {
            var bitIndex = 0;
            var node = Root;
            while (node.Contacts == null)
            {
                node = _DetermineNode(node, id, bitIndex++);
            }
            return node.Get(id);
        }

        /// <summary>
        /// Adds a contact to the appropriate bucket, splitting buckets as necessary.
        /// </summary>
        private bool _Add(T contact, out ReviewEventArgs<T>? ping)
        {
            ping = null;
            var bitIndex = 0;
            var node = Root;

            while (node.Contacts == null)
            {
                node = _DetermineNode(node, contact.Identifier, bitIndex++);
            }

            var index = node.IndexOf(contact.Identifier);
            if (index >= 0)
            {
                _Update(node, index, contact);
                return true;
            }

            if (node.Contacts.Count < ContactsPerBucket)
            {
                node.Contacts.Add(contact);
                return true;
            }

            if (node.DontSplit)
            {
                ping = new ReviewEventArgs<T>
                {
                    Oldest = node.Contacts.Take(ContactsToPing).ToArray(),
                    Newest = contact
                };
                return false;
            }

            _Split(node, bitIndex);
            return _Add(contact, out ping);
        }

        /// <summary>
        /// Updates the contact at the specified index using the arbiter function to determine which contact to keep.
        /// </summary>
        private void _Update(Bucket<T> node, int index, T contact)
        {
            var incumbent = node.Contacts[index];
            var selection = Arbiter(incumbent, contact);

            if (selection == incumbent && !ReferenceEquals(incumbent, contact))
            {
                return;
            }

            node.Contacts.RemoveAt(index);
            node.Contacts.Add(selection);
        }

        /// <summary>
        /// Removes the contact with the specified identifier from the k-bucket.
        /// </summary>
        private bool _Remove(byte[] id)
        {
            var bitIndex = 0;
            var node = Root;
            while (node.Contacts == null)
            {
                node = _DetermineNode(node, id, bitIndex++);
            }
            var index = node.IndexOf(id);
            if (index >= 0)
            {
                node.Contacts.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Splits the node, redistributes contacts to the new nodes, and marks the node as an inner node.
        /// </summary>
        private void _Split(Bucket<T> node, int bitIndex)
        {
            node.Left = new Bucket<T>();
            node.Right = new Bucket<T>();

            foreach (var contact in node.Contacts)
            {
                _DetermineNode(node, contact.Identifier, bitIndex).Contacts.Add(contact);
            }

            node.Contacts.Clear();

            var detNode = _DetermineNode(node, LocalContactId, bitIndex);
            var otherNode = node.Left == detNode ? node.Right : node.Left;
            otherNode.DontSplit = true;
        }

        /// <summary>
        /// Determines which child node (left or right) to traverse based on the bit at the specified index in the identifier.
        /// </summary>
        public Bucket<T> _DetermineNode(Bucket<T> node, byte[] id, int bitIndex)
        {
            var bytesDescribedByBitIndex = bitIndex >> 3;
            var bitIndexWithinByte = bitIndex % 8;
            if ((id.Length <= bytesDescribedByBitIndex) && (bitIndexWithinByte != 0))
            {
                return node.Left!;
            }
            var byteUnderConsideration = id[bytesDescribedByBitIndex];
            if (0 != (byteUnderConsideration & (1 << (7 - bitIndexWithinByte))))
            {
                return node.Right!;
            }
            return node.Left!;
        }
    }
}
