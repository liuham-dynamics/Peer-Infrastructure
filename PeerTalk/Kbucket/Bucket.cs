using PeerTalk.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   A binary tree node in the <see cref="KBucket{T}"/>.
    /// </summary>
    public class Bucket<T> where T : class, IContact
    {
        /// <summary>
        ///   The contacts in the bucket.
        /// </summary>
        public List<T> Contacts = [];

        /// <summary>
        ///  Determines if the bucket can be split.
        /// </summary>
        public bool DontSplit;

        /// <summary>
        ///   The left hand node.
        /// </summary>
        public Bucket<T> Left;

        /// <summary>
        ///   The right hand node.
        /// </summary>
        public Bucket<T> Right;

        /// <summary>
        ///   Determines if the <see cref="Contacts"/> contains the item.
        /// </summary>
        public bool Contains(T item)
        {
            if (Contacts == null)
            {
                return false;
            }
            return Contacts.Any(c => c.Identifier.SequenceEqual(item.Identifier));
        }

        /// <summary>
        ///   Gets the first contact with the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        ///   The matching contact or <b>null</b>.
        /// </returns>
        public T Get(byte[] id)
        {
            return Contacts?.FirstOrDefault(c => c.Identifier.SequenceEqual(id));
        }

        internal int IndexOf(byte[] id)
        {
            if (Contacts == null)
                return -1;
            return Contacts.FindIndex(c => c.Identifier.SequenceEqual(id));
        }

        internal int DeepCount()
        {
            var n = 0;
            if (Contacts != null)
                n += Contacts.Count;
            if (Left != null)
                n += Left.DeepCount();
            if (Right != null)
                n += Right.DeepCount();

            return n;
        }

        internal IEnumerable<T> AllContacts()
        {
            if (Contacts != null)
            {
                foreach (var contact in Contacts)
                {
                    yield return contact;
                }
            }
            if (Left != null)
            {
                foreach (var contact in Left.AllContacts())
                {
                    yield return contact;
                }
            }
            if (Right != null)
            {
                foreach (var contact in Right.AllContacts())
                {
                    yield return contact;
                }
            }
        }
    }
}
