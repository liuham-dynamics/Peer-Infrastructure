using PeerTalk.Core;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   A binary tree node in the <see cref="KBucket{T}"/>.
    /// </summary>
    public class Bucket<T> where T : class, IContact
    {
        /// <summary>
        /// The contacts in the bucket.
        /// </summary>
        public List<T> Contacts = [];

        /// <summary>
        /// The left hand node.
        /// </summary>
        public Bucket<T>? Left { get; set; }

        /// <summary>
        /// The right hand node.
        /// </summary>
        public Bucket<T>? Right { get; set; }

        /// <summary>
        /// Determines if the bucket can be split.
        /// </summary>
        public bool DontSplit { get; set; }

        /// <summary>
        /// Determines if the <see cref="Contacts"/> contains the item.
        /// </summary>
        public bool Contains(T item)
        {
            if (Contacts is null)
            {
                return false;
            }

            return Contacts.Any(c => c.Identifier.SequenceEqual(item.Identifier));
        }

        /// <summary>
        /// Gets the first contact with the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The matching contact or <b>null</b>.
        /// </returns>
        public T Get(byte[] id)
        {
            return Contacts.FirstOrDefault(c => c.Identifier.SequenceEqual(id))!;
        }

        /// <inheridoc/> 
        internal int IndexOf(byte[] id)
        {
            return Contacts is null ? -1 : Contacts.FindIndex(c => c.Identifier.SequenceEqual(id));
        }

        /// <inheridoc/> 
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

        /// <inheridoc/> 
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
