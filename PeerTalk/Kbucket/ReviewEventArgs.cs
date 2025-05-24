﻿using PeerTalk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Kbucket
{
    /// <summary>
    ///   The contacts that should be checked.
    /// </summary>
    /// <seealso cref="KBucket{T}.Ping"/>
    public class ReviewEventArgs<T> : EventArgs where T : IContact
    {
        /// <summary>
        ///   The contacts that should be checked.
        /// </summary>
        public IEnumerable<T> Oldest { get; set; } = [];

        /// <summary>
        ///   A new contact that wants to be added.
        /// </summary>
        public required T Newest { get; set; }
    }
}
