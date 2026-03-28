using PeerTalk.Core;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   Represents a node in the binary tree structure of a <see cref="KBucket{T}"/>.
    /// </summary>
    public class Bucket<T> where T : class, IContact
    {

        private readonly List<T> _memberContacts = [];
        private readonly Dictionary<string, T> _memberContactMap = []; // Cache by ID string


        /// <summary>
        /// Gets the list of contacts stored in this bucket.
        /// </summary>
        public List<T> Contacts => _memberContacts;

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
        public bool Contains(T item) => _memberContactMap.ContainsKey(Convert.ToHexString(item.Identifier));

        /// <summary>
        /// Retrieves the first contact with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns>The matching contact if found; otherwise, <c>null</c>.</returns>
        public T Get(byte[] id)
        {
            var b = _memberContacts.FirstOrDefault(c => c.Identifier.SequenceEqual(id))!;
            _ = _memberContactMap.TryAdd(Convert.ToHexString(b.Identifier), b);
            return b;
        }

        /// <inheritdoc/>
        internal int IndexOf(byte[] id)
        {
            return Contacts is null ? -1 : _memberContacts.FindIndex(c => c.Identifier.SequenceEqual(id));
        }

        /// <inheritdoc/>
        internal int DeepCount()
        {
            int count = 0;
            var stack = new Stack<Bucket<T>>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node._memberContacts is not null)
                {
                    count += node._memberContacts.Count;
                }
                else
                {
                    if (node.Left is not null) stack.Push(node.Left);
                    if (node.Right is not null) stack.Push(node.Right);
                }
            }
            return count;
        }

        /// <inheritdoc/>
        internal IEnumerable<T> AllContacts()
        {
            if (_memberContacts is not null)
            {
                foreach (var contact in _memberContacts)
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
