using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns
{
    /// <summary>
    ///   SecurityAlgorithmMetadata on EDNS options.
    /// </summary>
    /// <see cref="AEdnsOption"/>
    public static class EdnsOptionRegistry
    {
        /// <summary>
        ///   All the EDNS options.
        /// </summary>
        /// <remarks>
        ///   The key is the <see cref="EdnsOptionType"/>.
        ///   The value is a function that returns a new <see cref="AEdnsOption"/>.
        /// </remarks>
        public static Dictionary<EdnsOptionType, Func<AEdnsOption>> Options;

        static EdnsOptionRegistry()
        {
            Options = new Dictionary<EdnsOptionType, Func<AEdnsOption>>();
            Register<EdnsPaddingOption>();
            Register<EdnsNSIDOption>();
            Register<EdnsKeepaliveOption>();
            Register<EdnsDAUOption>();
            Register<EdnsDHUOption>();
            Register<EdnsN3UOption>();
        }

        /// <summary>
        ///   Register a new EDNS option.
        /// </summary>
        /// <typeparam name="T">
        ///   A type that is derived from <see cref="AEdnsOption"/>.
        /// </typeparam>
        public static void Register<T>() where T : AEdnsOption, new()
        {
            var option = new T();
            Options.Add(option.Type, () => new T());
        }
    }
}
