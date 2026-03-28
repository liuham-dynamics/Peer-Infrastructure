using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns
{
    /// <summary>
    ///   Registry of implemented <see cref="SecurityAlgorithm"/>.
    /// </summary>
    /// <remarks>
    ///   IANA maintains a list of all known types at <see href="https://www.iana.org/assignments/dns-sec-alg-numbers/dns-sec-alg-numbers.xhtml#dns-sec-alg-numbers-1"/>.
    /// </remarks>
    /// <see cref="SecurityAlgorithm"/>
    public static class SecurityAlgorithmRegistry
    {
        /// <summary>
        ///   Defined security algorithms.
        /// </summary>
        /// <remarks>
        ///   The key is the <see cref="SecurityAlgorithm"/>.
        ///   The value is th <see cref="SecurityAlgorithmMetadata"/>.
        /// </remarks>
        public static Dictionary<SecurityAlgorithm, SecurityAlgorithmMetadata> Algorithms;

        /// <summary>
        ///  
        /// </summary>
        static SecurityAlgorithmRegistry()
        {
            Algorithms = new Dictionary<SecurityAlgorithm, SecurityAlgorithmMetadata>
            {
                {
                    SecurityAlgorithm.RSASHA1,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha1,
                    }
                },
                {
                    SecurityAlgorithm.RSASHA256,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha256,
                    }
                },
                {
                    SecurityAlgorithm.RSASHA512,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha512,
                    }
                },
                {
                    SecurityAlgorithm.DSA,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha1,
                    }
                },
                {
                    SecurityAlgorithm.ECDSAP256SHA256,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha256,
                        OtherNames = ["nistP256", "ECDSA_P256"],
                    }
                },
                {
                    SecurityAlgorithm.ECDSAP384SHA384,
                    new SecurityAlgorithmMetadata
                    {
                        HashAlgorithm = DigestType.Sha384,
                        OtherNames = ["nistP384", "ECDSA_P384"],
                    }
                }
            };

            Algorithms.Add(SecurityAlgorithm.RSASHA1NSEC3SHA1, Algorithms[SecurityAlgorithm.RSASHA1]);
            Algorithms.Add(SecurityAlgorithm.DSANSEC3SHA1, Algorithms[SecurityAlgorithm.DSA]);
        }

        /// <summary>
        ///   Gets the meta data for the <see cref="SecurityAlgorithm"/>.
        /// </summary>
        /// <param name="algorithm">
        ///   One of the <see cref="SecurityAlgorithm"/> values.
        /// </param>
        /// <returns>
        ///   The <see cref="SecurityAlgorithmMetadata"/> for the <paramref name="algorithm"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        ///   When the <paramref name="algorithm"/> is not defined.
        /// </exception>
        public static SecurityAlgorithmMetadata GetMetadata(SecurityAlgorithm algorithm)
        {
            if (Algorithms.TryGetValue(algorithm, out var metadata) && metadata is not null)
            {
                return metadata;
            }

            throw new NotSupportedException($"The security algorithm '{algorithm}' is not defined.");
        }
    }
}
