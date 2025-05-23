﻿using PeerTalk.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   Implementation of a Kademlia DHT k-bucket used for storing contact (peer node) information.
    /// </summary>
    /// <typeparam name="T">
    ///   A contact type that implements <see cref="IContact"/> .
    /// </typeparam>
    /// <remarks>
    ///   All public methods and properties are thead-safe.
    /// </remarks>
    public class KBucket<T> : ICollection<T> where T : class, IContact
    {
        private readonly ReaderWriterLockSlim rwlock = new();
        private byte[] localContactId;

        /// <summary>
        ///   The number of contacts allowed in a bucket.
        /// </summary>
        /// <value>
        ///   This is the 'K' in KBucket.  Defaults to 20.
        /// </value>
        public int ContactsPerBucket { get; set; } = 20;

        /// <summary>
        ///   The number of contacts to ping when a bucket that should not be split
        ///   becomes full.
        /// </summary>
        /// <value>
        ///   Defaults to 3.
        /// </value>
        /// <seealso cref="Ping"/>
        public int ContactsToPing { get; set; } = 3;

        /// <summary>
        ///   The ID of the local contact/peer.
        /// </summary>
        /// <value>
        ///   Defaults to 20 random bytes.
        /// </value>
        public byte[] LocalContactId
        {
            get
            {
                if (localContactId == null)
                {
                    localContactId = new byte[20];
                    new Random().NextBytes(localContactId);
                }
                return localContactId;
            }
            set
            {
                localContactId = value;
            }
        }

        /// <summary>
        ///   The root of the binary tree.
        /// </summary>
        public Bucket<T>? Root { get; private set; }

        /// <summary>
        ///   Raised when a bucket needs splitting but cannot be split.
        /// </summary>
        public event EventHandler<ReviewEventArgs<T>> Ping;

        /// <summary>
        ///   Determines which contact is used when both have the same ID.
        /// </summary>
        /// <value>
        ///   Defaults to <see cref="DefaultAbiter(T, T)"/>.
        /// </value>
        /// <remarks>
        ///   The arguments are the incumbent and the candidate.
        /// </remarks>
        public Func<T, T, T> Arbiter { get; set; } = DefaultAbiter;

        /// <summary>
        ///   Used to determine which contact should be use when both
        ///   have the same ID.
        /// </summary>
        /// <param name="incumbent">
        ///   The existing contact.
        /// </param>
        /// <param name="candidate">
        ///   The new contact.
        /// </param>
        /// <returns>
        ///   Always returns the <paramref name="incumbent"/>.
        /// </returns>
        public static T DefaultAbiter(T incumbent, T candidate)
        {
            return incumbent;
        }

        /// <summary>
        ///   Finds the XOR distance between the two contacts.
        /// </summary>
        public BigInteger Distance(T a, T b)
        {
            Validate(a);
            Validate(b);
            return Distance(a.Identifier, b.Identifier);
        }

        /// <summary>
        ///   Finds the XOR distance between the two contact IDs.
        /// </summary>
        public BigInteger Distance(byte[] a, byte[] b)
        {
            BigInteger distance = 0;
            var i = 0;
            var min = Math.Min(a.Length, b.Length);
            var max = Math.Max(a.Length, b.Length);
            for (; i < min; ++i)
            {
                distance = (distance * 256) + (a[i] ^ b[i]);
            }
            for (; i < max; ++i)
            {
                distance = (distance * 256) + 255;
            }
            return distance;
        }

        /// <summary>
        ///   Gets the closest contacts to the provided contact.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns>
        ///   An ordered sequence of contact, sorted by closeness.
        /// </returns>
        /// <remarks>
        ///   "Closest" is the XOR metric of the contact.
        /// </remarks>
        public IEnumerable<T> Closest(T contact)
        {
            Validate(contact);
            return Closest(contact.Identifier);
        }

        /// <summary>
        ///   Gets the closest contacts to the provided contact.
        /// </summary>
        /// <param name="id">
        ///   The unique <see cref="IContact.Id"/> of a contact.
        /// </param>
        /// <returns>
        ///   An ordered sequence of contact, sorted by closeness.
        /// </returns>
        /// <remarks>
        ///   "Closest" is the XOR metric of the contact.
        /// </remarks>
        public IEnumerable<T> Closest(byte[] id)
        {
            return this
                .Select(c => new { distance = Distance(c.Identifier, id), contact = c })
                .OrderBy(a => a.distance)
                .Select(a => a.contact);
        }

        /// <inheritdoc />
        public int Count => Root.DeepCount();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(T item)
        {
            Validate(item);
            bool q;
            ReviewEventArgs<T> e;
            rwlock.EnterWriteLock();
            try
            {
                q = _Add(item, out e);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }

            // Could not add.  Ping oldest contacts.
            if (!q)
            {
                Ping?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            Root = new Bucket<T>();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            Validate(item);

            rwlock.EnterReadLock();
            try
            {
                return _Get(item.Identifier) != null;
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        /// <summary>
        ///   Gets the contact associated with the specified ID.
        /// </summary>
        /// <param name="id">
        ///   The ID of an <see cref="IContact"/>.
        /// </param>
        /// <param name="contact">
        ///   When this method returns, contains the <see cref="IContact"/> associated
        ///   with the <paramref name="id"/>, if the key is found; otherwise, <b>null</b>.
        ///   This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///  <b>true</b> if the <paramref name="id"/> is found; otherwise <b>false</b>.
        /// </returns>
        public bool TryGet(byte[] id, out T contact)
        {
            rwlock.EnterReadLock();
            try
            {
                contact = _Get(id);
                return contact != null;
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var contact in this)
            {
                array[arrayIndex++] = contact;
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            rwlock.EnterReadLock();
            try
            {
                foreach (var contact in Root.AllContacts())
                {
                    yield return contact;
                }
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            Validate(item);

            rwlock.EnterWriteLock();
            try
            {
                return _Remove(item.Identifier);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Check that contact is correct.
        /// </summary>
        /// <param name="contact">
        ///   The <see cref="IContact"/> to validate.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   When <paramref name="contact"/> is null or its <see cref="IContact.Id"/>
        ///   is null or empty.
        /// </exception>
        private void Validate(T contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));
            if (contact.Identifier == null || contact.Identifier.Length == 0)
                throw new ArgumentNullException("contact.Id");
        }

        /// <summary>
        ///   Add the contact.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="ping"></param>
        /// <returns>
        ///   <b>true</b> if the <paramref name="contact"/> was added; otherwise,
        ///   <b>false</b> and a <see cref="Ping"/> event should be raised.
        /// </returns>
        private bool _Add(T contact, out ReviewEventArgs<T> ping)
        {
            ping = null;

            var bitIndex = 0;
            var node = Root;

            while (node.Contacts == null)
            {
                // this is not a leaf node but an inner node with 'low' and 'high'
                // branches; we will check the appropriate bit of the identifier and
                // delegate to the appropriate node for further processing
                node = _DetermineNode(node, contact.Identifier, bitIndex++);
            }

            // check if the contact already exists
            var index = node.IndexOf(contact.Identifier);
            if (0 <= index)
            {
                _Update(node, index, contact);
                return true;
            }

            if (node.Contacts.Count < ContactsPerBucket)
            {
                node.Contacts.Add(contact);
                return true;
            }

            // the bucket is full
            if (node.DontSplit)
            {
                // we are not allowed to split the bucket
                // we need to ping the first this.numberOfNodesToPing
                // in order to determine if they are alive
                // only if one of the pinged nodes does not respond, can the new contact
                // be added (this prevents DoS flodding with new invalid contacts)

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
        ///   Splits the node, redistributes contacts to the new nodes, and marks the
        ///   node that was split as an inner node of the binary tree of nodes by
        ///   setting this.root.contacts = null
        /// </summary>
        private void _Split(Bucket<T> node, int bitIndex)
        {
            node.Left = new Bucket<T>();
            node.Right = new Bucket<T>();

            // redistribute existing contacts amongst the two newly created nodes
            foreach (var contact in node.Contacts)
            {
                _DetermineNode(node, contact.Identifier, bitIndex)
                    .Contacts.Add(contact);
            }

            node.Contacts = null; // mark as inner tree node

            // don't split the "far away" node
            // we check where the local node would end up and mark the other one as
            // "dontSplit" (i.e. "far away")
            var detNode = _DetermineNode(node, LocalContactId, bitIndex);
            var otherNode = node.Left == detNode ? node.Right : node.Left;
            otherNode.DontSplit = true;
        }

        /// <summary>
        ///   Updates the contact selected by the arbiter.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   If the selection is our old contact and the candidate is some new contact
        ///   then the new contact is abandoned (not added).
        /// </para>
        /// <para>
        ///   If the selection is our old contact and the candidate is our old contact
        ///   then we are refreshing the contact and it is marked as most recently
        ///   contacted(by being moved to the right/end of the bucket array).
        /// </para>
        /// <para>
        ///   If the selection is our new contact, the old contact is removed and the new
        ///   contact is marked as most recently contacted.
        /// </para>
        /// </remarks>
        private void _Update(Bucket<T> node, int index, T contact)
        {
            var incumbent = node.Contacts[index];
            var selection = Arbiter(incumbent, contact);

            // if the selection is our old contact and the candidate is some new
            // contact, then there is nothing to do
            if (selection == incumbent && incumbent != contact)
            {
                return;
            }

            node.Contacts.RemoveAt(index);
            node.Contacts.Add(selection);
        }

        /// <summary>
        ///   Determines whether the id at the bitIndex is 0 or 1.
        /// </summary>
        /// <returns>
        ///   Left leaf if `id` at `bitIndex` is 0, right leaf otherwise
        /// </returns>
        /// <remarks>
        ///   This is an internal method.  It should not be directly called.
        ///   It is only public for unit testing.
        /// </remarks>
        public Bucket<T> _DetermineNode(Bucket<T> node, byte[] id, int bitIndex)
        {
            // id's that are too short are put in low bucket (1 byte = 8 bits)
            // (bitIndex >> 3) finds how many bytes the bitIndex describes
            // bitIndex % 8 checks if we have extra bits beyond byte multiples
            // if number of bytes is <= no. of bytes described by bitIndex and there
            // are extra bits to consider, this means id has less bits than what
            // bitIndex describes, id therefore is too short, and will be put in low
            // bucket
            var bytesDescribedByBitIndex = bitIndex >> 3;
            var bitIndexWithinByte = bitIndex % 8;
            if ((id.Length <= bytesDescribedByBitIndex) && (bitIndexWithinByte != 0))
            {
                return node.Left;
            }

            // byteUnderConsideration is an integer from 0 to 255 represented by 8 bits
            // where 255 is 11111111 and 0 is 00000000
            // in order to find out whether the bit at bitIndexWithinByte is set
            // we construct (1 << (7 - bitIndexWithinByte)) which will consist
            // of all bits being 0, with only one bit set to 1
            // for example, if bitIndexWithinByte is 3, we will construct 00010000 by
            // (1 << (7 - 3)) -> (1 << 4) -> 16
            var byteUnderConsideration = id[bytesDescribedByBitIndex];
            if (0 != (byteUnderConsideration & (1 << (7 - bitIndexWithinByte))))
            {
                return node.Right;
            }

            return node.Left;
        }

        /// <summary>
        ///   Get a contact by its exact ID.
        /// </summary>
        /// <param name="id">
        ///   The ID of a <see cref="IContact"/>.
        /// </param>
        /// <returns>
        ///   <b>null</b> or the found contact.
        /// </returns>
        private T _Get(byte[] id)
        {
            /*
             * If this is a leaf, loop through the bucket contents and return the correct
             * contact if we have it or null if not. If this is an inner node, determine
             * which branch of the tree to traverse and repeat.
             */
            var bitIndex = 0;
            var node = Root;
            while (node.Contacts == null)
            {
                node = _DetermineNode(node, id, bitIndex++);
            }

            return node.Get(id);
        }

        private bool _Remove(byte[] id)
        {
            var bitIndex = 0;

            var node = Root;
            while (node.Contacts == null)
            {
                node = _DetermineNode(node, id, bitIndex++);
            }

            // index of uses contact id for matching
            var index = node.IndexOf(id);
            if (0 <= index)
            {
                node.Contacts.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}
