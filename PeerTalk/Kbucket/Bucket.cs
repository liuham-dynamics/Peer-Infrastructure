using PeerTalk.Core;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   Represents a node in the binary tree structure of a <see cref="KBucket{T}"/>.
    /// </summary>
    public class Bucket<T> where T : class, IContact
    {
        /// <summary>
        /// Gets the list of contacts stored in this bucket.
        /// </summary>
        public readonly List<T> Contacts = [];

        /// <summary>
        /// Gets or sets the left child node of this bucket.
        /// </summary>
        public Bucket<T>? Left { get; set; }

        /// <summary>
        /// Gets or sets the right child node of this bucket.
        /// </summary>
        public Bucket<T>? Right { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this bucket should not be split.
        /// </summary>
        public bool DontSplit { get; set; }

        /// <summary>
        /// Determines whether the specified contact exists in the <see cref="Contacts"/> list.
        /// </summary>
        /// <param name="item">The contact to locate.</param>
        /// <returns><c>true</c> if the contact is found; otherwise, <c>false</c>.</returns>
        public bool Contains(T item)
        {
            if (Contacts is null)
            {
                return false;
            }

            return Contacts.Any(c => c.Identifier.SequenceEqual(item.Identifier));
        }

        /// <summary>
        /// Retrieves the first contact with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns>The matching contact if found; otherwise, <c>null</c>.</returns>
        public T Get(byte[] id)
        {
            return Contacts.FirstOrDefault(c => c.Identifier.SequenceEqual(id))!;
        }

        /// <inheritdoc/>
        internal int IndexOf(byte[] id)
        {
            return Contacts is null ? -1 : Contacts.FindIndex(c => c.Identifier.SequenceEqual(id));
        }

        /// <inheritdoc/>
        internal int DeepCount()
        {
            var n = 0;
            if (Contacts is not null)
            {
                n += Contacts.Count;
            }

            if (Left is not null)
            {
                n += Left.DeepCount();
            }

            if (Right is not null)
            {
                n += Right.DeepCount();
            }

            return n;
        }

        /// <inheritdoc/>
        internal IEnumerable<T> AllContacts()
        {
            if (Contacts is not null)
            {
                foreach (var contact in Contacts)
                {
                    yield return contact;
                }
            }

            if (Left is not null)
            {
                foreach (var contact in Left.AllContacts())
                {
                    yield return contact;
                }
            }

            if (Right is not null)
            {
                foreach (var contact in Right.AllContacts())
                {
                    yield return contact;
                }
            }
        }
    }
}
