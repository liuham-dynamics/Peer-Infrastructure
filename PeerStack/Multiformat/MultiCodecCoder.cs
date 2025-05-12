using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PeerStack.Multiformat
{
    /// <summary>
    ///  Multiformat multi-codec for Metadata.
    /// </summary>
    /// <remarks>
    ///   Distributed systems assigns a unique <see cref="Name"/> and <see cref="Code"/> to codecs.
    ///   See <see href="https://github.com/multiformats/multicodec/blob/master/table.csv">table.csv</see>
    ///   for the currently defined multi-codecs.
    /// </remarks>
    /// <seealso href="https://github.com/multiformats/multicodec"/>
    public sealed class MultiCodecCoder : IMultiFormatCoder
    {
        private static HashSet<MultiCodecCoder> _memberCodecs = [];
         
        /// <summary>
        ///   The code assigned to the codec.
        /// </summary>
        /// <value>
        ///   Valid codes at <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>.
        /// </value>
        public int Code { get; private set; }

        /// <summary>
        ///   The name of the codec.
        /// </summary>
        /// <value>
        ///   A unique name.
        /// </value>
        public string? Name { get; private set; }
        
        /// <summary>
        ///   The tag categorization of the codec.
        /// </summary>
        /// <value>
        ///   A unique tag.
        /// </value>
        public string? Tag { get; private set; }

        /// <summary>
        /// The specific multiformat type
        /// </summary>
        public MultiFormatType FormatType => MultiFormatType.MultiCodec;
         
        /// <summary>
        ///   A sequence consisting of all codecs.
        /// </summary>
        /// <value>
        ///   All the registered codecs.
        /// </value>
        public static IEnumerable<MultiCodecCoder> Codecs => _memberCodecs;
         
        /// <summary>
        ///   Register the standard and/or common multi-codecs.
        /// </summary>
        /// <seealso href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        static MultiCodecCoder()
        {
            // register all codecs
            Register("cidv1", 0x01, "cid");
            Register("cidv2", 0x02, "cid");
            Register("cidv3", 0x03, "cid");

            Register("aes-gcm-256", 0x2000, "encryption");

            Register("fil-commitment-unsealed", 0xf101, "filecoin");
            Register("fil-commitment-sealed", 0xf102, "filecoin");

            Register("sha256a", 0x7012, "hash");
            Register("xxh-32", 0xb3e1, "hash");
            Register("xxh-64", 0xb3e2, "hash");
            Register("xxh3-64", 0xb3e3, "hash");
            Register("xxh3-128", 0xb3e4, "hash");
            Register("murmur3-x64-128", 0x1022, "hash");
            Register("murmur3-x64-64", 0x22, "hash");
            Register("murmur3-32", 0x23, "hash");
            Register("crc32", 0x0132, "hash");
            Register("crc64-ecma", 0x0164, "hash");

            Register("holochain-adr-v0", 0x807124, "holochain");
            Register("holochain-adr-v1", 0x817124, "holochain");
            Register("holochain-key-v0", 0x947124, "holochain");
            Register("holochain-key-v1", 0x957124, "holochain");
            Register("holochain-sig-v0", 0xa27124, "holochain");
            Register("holochain-sig-v1", 0xa37124, "holochain");

            Register("rdfc-1", 0xb403, "ipld");
            Register("json-jcs", 0xb601, "ipld");
            Register("stellar-block", 0xd0, "ipld");
            Register("stellar-tx", 0xd1, "ipld");
            Register("decred-block", 0xe0, "ipld");
            Register("decred-tx", 0xe1, "ipld");
            Register("dash-block", 0xf0, "ipld");
            Register("dash-tx", 0xf1, "ipld");
            Register("swarm-manifest", 0xfa, "ipld");
            Register("swarm-feed", 0xfb, "ipld");
            Register("beeson", 0xfc, "ipld");
            Register("dag-json", 0x0129, "ipld");
            Register("swhid-1-snp", 0x01f0, "ipld");
            Register("json", 0x0200, "ipld");
            Register("cbor", 0x51, "ipld");
            Register("raw", 0x55, "ipld");
            Register("eth-block", 0x90, "ipld");
            Register("eth-block-list", 0x91, "ipld");
            Register("eth-tx-trie", 0x92, "ipld");
            Register("eth-tx", 0x93, "ipld");
            Register("eth-tx-receipt-trie", 0x94, "ipld");
            Register("eth-tx-receipt", 0x95, "ipld");
            Register("eth-state-trie", 0x96, "ipld");
            Register("eth-account-snapshot", 0x97, "ipld");
            Register("eth-storage-trie", 0x98, "ipld");
            Register("eth-receipt-log-trie", 0x99, "ipld");
            Register("eth-receipt-log", 0x9a, "ipld");
            Register("dag-pb", 0x70, "ipld");
            Register("dag-cbor", 0x71, "ipld");
            Register("libp2p-key", 0x72, "ipld");
            Register("git-raw", 0x78, "ipld");
            Register("torrent-info", 0x7b, "ipld");
            Register("torrent-file", 0x7c, "ipld");
            Register("blake3-hashseq", 0x80, "ipld");
            Register("leofcoin-block", 0x81, "ipld");
            Register("leofcoin-tx", 0x82, "ipld");
            Register("leofcoin-pr", 0x83, "ipld");
            Register("dag-jose", 0x85, "ipld");
            Register("dag-cose", 0x86, "ipld");
            Register("bitcoin-block", 0xb0, "ipld");
            Register("bitcoin-tx", 0xb1, "ipld");
            Register("bitcoin-witness-commitment", 0xb2, "ipld");
            Register("zcash-block", 0xc0, "ipld");
            Register("zcash-tx", 0xc1, "ipld");

            Register("aes-128", 0xa0, "key");
            Register("aes-192", 0xa1, "key");
            Register("aes-256", 0xa2, "key");
            Register("chacha-128", 0xa3, "key");
            Register("chacha-256", 0xa4, "key");
            Register("p256-pub", 0x1200, "key");
            Register("p384-pub", 0x1201, "key");
            Register("p521-pub", 0x1202, "key");
            Register("ed448-pub", 0x1203, "key");
            Register("x448-pub", 0x1204, "key");
            Register("rsa-pub", 0x1205, "key");
            Register("sm2-pub", 0x1206, "key");
            Register("secp256k1-pub", 0xe7, "key");
            Register("bls12_381-g1-pub", 0xea, "key");
            Register("bls12_381-g2-pub", 0xeb, "key");
            Register("x25519-pub", 0xec, "key");
            Register("ed25519-pub", 0xed, "key");
            Register("bls12_381-g1g2-pub", 0xee, "key");
            Register("sr25519-pub", 0xef, "key");
            Register("mlkem-512-pub", 0x120b, "key");
            Register("mlkem-768-pub", 0x120c, "key");
            Register("mlkem-1024-pub", 0x120d, "key");
            Register("ed25519-priv", 0x1300, "key");
            Register("secp256k1-priv", 0x1301, "key");
            Register("x25519-priv", 0x1302, "key");
            Register("sr25519-priv", 0x1303, "key");
            Register("rsa-priv", 0x1305, "key");
            Register("p256-priv", 0x1306, "key");
            Register("p384-priv", 0x1307, "key");
            Register("p521-priv", 0x1308, "key");
            Register("bls12_381-g1-priv", 0x1309, "key");
            Register("bls12_381-g2-priv", 0x130a, "key");
            Register("bls12_381-g1g2-priv", 0x130b, "key");
            Register("bls12_381-g1-pub-share", 0x130c, "key");
            Register("bls12_381-g2-pub-share", 0x130d, "key");
            Register("bls12_381-g1-priv-share", 0x130e, "key");
            Register("bls12_381-g2-priv-share", 0x130f, "key");
            Register("sm2-priv", 0x1310, "key");
            Register("lamport-sha3-512-pub", 0x1a14, "key");
            Register("lamport-sha3-384-pub", 0x1a15, "key");
            Register("lamport-sha3-256-pub", 0x1a16, "key");
            Register("lamport-sha3-512-priv", 0x1a24, "key");
            Register("lamport-sha3-384-priv", 0x1a25, "key");
            Register("lamport-sha3-256-priv", 0x1a26, "key");
            Register("lamport-sha3-512-priv-share", 0x1a34, "key");
            Register("lamport-sha3-384-priv-share", 0x1a35, "key");
            Register("lamport-sha3-256-priv-share", 0x1a36, "key");
            Register("jwk_jcs-pub", 0xeb51, "key");

            Register("libp2p-peer-record", 0x0301, "libp2p");
            Register("libp2p-relay-rsvp", 0x0302, "libp2p");
            Register("memorytransport", 0x0309, "libp2p");

            Register("udt", 0x012d, "multiaddr");
            Register("utp", 0x012e, "multiaddr");
            Register("udp", 0x0111, "multiaddr");
            Register("p2p-webrtc-star", 0x0113, "multiaddr");
            Register("p2p-webrtc-direct", 0x0114, "multiaddr");
            Register("p2p-stardust", 0x0115, "multiaddr");
            Register("webrtc-direct", 0x0118, "multiaddr");
            Register("webrtc", 0x0119, "multiaddr");
            Register("p2p-circuit", 0x0122, "multiaddr");
            Register("unix", 0x0190, "multiaddr");
            Register("thread", 0x0196, "multiaddr");
            Register("p2p", 0x01a5, "multiaddr");
            Register("https", 0x01bb, "multiaddr");
            Register("onion", 0x01bc, "multiaddr");
            Register("onion3", 0x01bd, "multiaddr");
            Register("garlic64", 0x01be, "multiaddr");
            Register("garlic32", 0x01bf, "multiaddr");
            Register("tls", 0x01c0, "multiaddr");
            Register("sni", 0x01c1, "multiaddr");
            Register("noise", 0x01c6, "multiaddr");
            Register("shs", 0x01c8, "multiaddr");
            Register("quic", 0x01cc, "multiaddr");
            Register("quic-v1", 0x01cd, "multiaddr");
            Register("webtransport", 0x01d1, "multiaddr");
            Register("certhash", 0x01d2, "multiaddr");
            Register("ws", 0x01dd, "multiaddr");
            Register("wss", 0x01de, "multiaddr");
            Register("p2p-websocket-star", 0x01df, "multiaddr");
            Register("http", 0x01e0, "multiaddr");
            Register("http-path", 0x01e1, "multiaddr");
            Register("dccp", 0x21, "multiaddr");
            Register("sctp", 0x84, "multiaddr");
            Register("dns", 0x35, "multiaddr");
            Register("dns4", 0x36, "multiaddr");
            Register("dns6", 0x37, "multiaddr");
            Register("dnsaddr", 0x38, "multiaddr");
            Register("ip6", 0x29, "multiaddr");
            Register("ip6zone", 0x2a, "multiaddr");
            Register("ipcidr", 0x2b, "multiaddr");
            Register("ip4", 0x04, "multiaddr");
            Register("tcp", 0x06, "multiaddr");
            Register("plaintextv2", 0x706c61, "multiaddr");
            Register("scion", 0xd02000, "multiaddr");
            Register("silverpine", 0x3f42, "multiaddr");

            Register("multisig", 0x1239, "multiformat");
            Register("multikey", 0x123a, "multiformat");
            Register("multicodec", 0x30, "multiformat");
            Register("multihash", 0x31, "multiformat");
            Register("multiaddr", 0x32, "multiformat");
            Register("multibase", 0x33, "multiformat");
            Register("varsig", 0x34, "multiformat");
            Register("caip-50", 0xca, "multiformat");
            Register("multidid", 0x0d1d, "multiformat");

            Register("sha2-256-trunc254-padded", 0x1012, "multihash");
            Register("sha2-224", 0x1013, "multihash");
            Register("sha2-512-224", 0x1014, "multihash");
            Register("sha2-512-256", 0x1015, "multihash");
            Register("md4", 0xd4, "multihash");
            Register("md5", 0xd5, "multihash");
            Register("identity", 0x00, "multihash");
            Register("dbl-sha2-256", 0x56, "multihash");
            Register("sha1", 0x11, "multihash");
            Register("sha2-256", 0x12, "multihash");
            Register("sha2-512", 0x13, "multihash");
            Register("sha3-512", 0x14, "multihash");
            Register("sha3-384", 0x15, "multihash");
            Register("sha3-256", 0x16, "multihash");
            Register("sha3-224", 0x17, "multihash");
            Register("shake-128", 0x18, "multihash");
            Register("shake-256", 0x19, "multihash");
            Register("keccak-224", 0x1a, "multihash");
            Register("keccak-256", 0x1b, "multihash");
            Register("keccak-384", 0x1c, "multihash");
            Register("keccak-512", 0x1d, "multihash");
            Register("blake3", 0x1e, "multihash");
            Register("sha2-384", 0x20, "multihash");
            Register("ripemd-128", 0x1052, "multihash");
            Register("ripemd-160", 0x1053, "multihash");
            Register("ripemd-256", 0x1054, "multihash");
            Register("ripemd-320", 0x1055, "multihash");
            Register("x11", 0x1100, "multihash");
            Register("sm3-256", 0x534d, "multihash");
            Register("poseidon-bls12_381-a2-fc1", 0xb401, "multihash");
            Register("poseidon-bls12_381-a2-fc1-sc", 0xb402, "multihash");
            Register("ssz-sha2-256-bmt", 0xb502, "multihash");
            Register("sha2-256-chunked", 0xb510, "multihash");
            Register("bcrypt-pbkdf", 0xd00d, "multihash");
            Register("kangarootwelve", 0x1d01, "multihash");
            Register("blake2b-8", 0xb201, "multihash");
            Register("blake2b-16", 0xb202, "multihash");
            Register("blake2b-24", 0xb203, "multihash");
            Register("blake2b-32", 0xb204, "multihash");
            Register("blake2b-40", 0xb205, "multihash");
            Register("blake2b-48", 0xb206, "multihash");
            Register("blake2b-56", 0xb207, "multihash");
            Register("blake2b-64", 0xb208, "multihash");
            Register("blake2b-72", 0xb209, "multihash");
            Register("blake2b-80", 0xb20a, "multihash");
            Register("blake2b-88", 0xb20b, "multihash");
            Register("blake2b-96", 0xb20c, "multihash");
            Register("blake2b-104", 0xb20d, "multihash");
            Register("blake2b-112", 0xb20e, "multihash");
            Register("blake2b-120", 0xb20f, "multihash");
            Register("blake2b-128", 0xb210, "multihash");
            Register("blake2b-136", 0xb211, "multihash");
            Register("blake2b-144", 0xb212, "multihash");
            Register("blake2b-152", 0xb213, "multihash");
            Register("blake2b-160", 0xb214, "multihash");
            Register("blake2b-168", 0xb215, "multihash");
            Register("blake2b-176", 0xb216, "multihash");
            Register("blake2b-184", 0xb217, "multihash");
            Register("blake2b-192", 0xb218, "multihash");
            Register("blake2b-200", 0xb219, "multihash");
            Register("blake2b-208", 0xb21a, "multihash");
            Register("blake2b-216", 0xb21b, "multihash");
            Register("blake2b-224", 0xb21c, "multihash");
            Register("blake2b-232", 0xb21d, "multihash");
            Register("blake2b-240", 0xb21e, "multihash");
            Register("blake2b-248", 0xb21f, "multihash");
            Register("blake2b-256", 0xb220, "multihash");
            Register("blake2b-264", 0xb221, "multihash");
            Register("blake2b-272", 0xb222, "multihash");
            Register("blake2b-280", 0xb223, "multihash");
            Register("blake2b-288", 0xb224, "multihash");
            Register("blake2b-296", 0xb225, "multihash");
            Register("blake2b-304", 0xb226, "multihash");
            Register("blake2b-312", 0xb227, "multihash");
            Register("blake2b-320", 0xb228, "multihash");
            Register("blake2b-328", 0xb229, "multihash");
            Register("blake2b-336", 0xb22a, "multihash");
            Register("blake2b-344", 0xb22b, "multihash");
            Register("blake2b-352", 0xb22c, "multihash");
            Register("blake2b-360", 0xb22d, "multihash");
            Register("blake2b-368", 0xb22e, "multihash");
            Register("blake2b-376", 0xb22f, "multihash");
            Register("blake2b-384", 0xb230, "multihash");
            Register("blake2b-392", 0xb231, "multihash");
            Register("blake2b-400", 0xb232, "multihash");
            Register("blake2b-408", 0xb233, "multihash");
            Register("blake2b-416", 0xb234, "multihash");
            Register("blake2b-424", 0xb235, "multihash");
            Register("blake2b-432", 0xb236, "multihash");
            Register("blake2b-440", 0xb237, "multihash");
            Register("blake2b-448", 0xb238, "multihash");
            Register("blake2b-456", 0xb239, "multihash");
            Register("blake2b-464", 0xb23a, "multihash");
            Register("blake2b-472", 0xb23b, "multihash");
            Register("blake2b-480", 0xb23c, "multihash");
            Register("blake2b-488", 0xb23d, "multihash");
            Register("blake2b-496", 0xb23e, "multihash");
            Register("blake2b-504", 0xb23f, "multihash");
            Register("blake2b-512", 0xb240, "multihash");
            Register("blake2s-8", 0xb241, "multihash");
            Register("blake2s-16", 0xb242, "multihash");
            Register("blake2s-24", 0xb243, "multihash");
            Register("blake2s-32", 0xb244, "multihash");
            Register("blake2s-40", 0xb245, "multihash");
            Register("blake2s-48", 0xb246, "multihash");
            Register("blake2s-56", 0xb247, "multihash");
            Register("blake2s-64", 0xb248, "multihash");
            Register("blake2s-72", 0xb249, "multihash");
            Register("blake2s-80", 0xb24a, "multihash");
            Register("blake2s-88", 0xb24b, "multihash");
            Register("blake2s-96", 0xb24c, "multihash");
            Register("blake2s-104", 0xb24d, "multihash");
            Register("blake2s-112", 0xb24e, "multihash");
            Register("blake2s-120", 0xb24f, "multihash");
            Register("blake2s-128", 0xb250, "multihash");
            Register("blake2s-136", 0xb251, "multihash");
            Register("blake2s-144", 0xb252, "multihash");
            Register("blake2s-152", 0xb253, "multihash");
            Register("blake2s-160", 0xb254, "multihash");
            Register("blake2s-168", 0xb255, "multihash");
            Register("blake2s-176", 0xb256, "multihash");
            Register("blake2s-184", 0xb257, "multihash");
            Register("blake2s-192", 0xb258, "multihash");
            Register("blake2s-200", 0xb259, "multihash");
            Register("blake2s-208", 0xb25a, "multihash");
            Register("blake2s-216", 0xb25b, "multihash");
            Register("blake2s-224", 0xb25c, "multihash");
            Register("blake2s-232", 0xb25d, "multihash");
            Register("blake2s-240", 0xb25e, "multihash");
            Register("blake2s-248", 0xb25f, "multihash");
            Register("blake2s-256", 0xb260, "multihash");
            Register("skein256-8", 0xb301, "multihash");
            Register("skein256-16", 0xb302, "multihash");
            Register("skein256-24", 0xb303, "multihash");
            Register("skein256-32", 0xb304, "multihash");
            Register("skein256-40", 0xb305, "multihash");
            Register("skein256-48", 0xb306, "multihash");
            Register("skein256-56", 0xb307, "multihash");
            Register("skein256-64", 0xb308, "multihash");
            Register("skein256-72", 0xb309, "multihash");
            Register("skein256-80", 0xb30a, "multihash");
            Register("skein256-88", 0xb30b, "multihash");
            Register("skein256-96", 0xb30c, "multihash");
            Register("skein256-104", 0xb30d, "multihash");
            Register("skein256-112", 0xb30e, "multihash");
            Register("skein256-120", 0xb30f, "multihash");
            Register("skein256-128", 0xb310, "multihash");
            Register("skein256-136", 0xb311, "multihash");
            Register("skein256-144", 0xb312, "multihash");
            Register("skein256-152", 0xb313, "multihash");
            Register("skein256-160", 0xb314, "multihash");
            Register("skein256-168", 0xb315, "multihash");
            Register("skein256-176", 0xb316, "multihash");
            Register("skein256-184", 0xb317, "multihash");
            Register("skein256-192", 0xb318, "multihash");
            Register("skein256-200", 0xb319, "multihash");
            Register("skein256-208", 0xb31a, "multihash");
            Register("skein256-216", 0xb31b, "multihash");
            Register("skein256-224", 0xb31c, "multihash");
            Register("skein256-232", 0xb31d, "multihash");
            Register("skein256-240", 0xb31e, "multihash");
            Register("skein256-248", 0xb31f, "multihash");
            Register("skein256-256", 0xb320, "multihash");
            Register("skein512-8", 0xb321, "multihash");
            Register("skein512-16", 0xb322, "multihash");
            Register("skein512-24", 0xb323, "multihash");
            Register("skein512-32", 0xb324, "multihash");
            Register("skein512-40", 0xb325, "multihash");
            Register("skein512-48", 0xb326, "multihash");
            Register("skein512-56", 0xb327, "multihash");
            Register("skein512-64", 0xb328, "multihash");
            Register("skein512-72", 0xb329, "multihash");
            Register("skein512-80", 0xb32a, "multihash");
            Register("skein512-88", 0xb32b, "multihash");
            Register("skein512-96", 0xb32c, "multihash");
            Register("skein512-104", 0xb32d, "multihash");
            Register("skein512-112", 0xb32e, "multihash");
            Register("skein512-120", 0xb32f, "multihash");
            Register("skein512-128", 0xb330, "multihash");
            Register("skein512-136", 0xb331, "multihash");
            Register("skein512-144", 0xb332, "multihash");
            Register("skein512-152", 0xb333, "multihash");
            Register("skein512-160", 0xb334, "multihash");
            Register("skein512-168", 0xb335, "multihash");
            Register("skein512-176", 0xb336, "multihash");
            Register("skein512-184", 0xb337, "multihash");
            Register("skein512-192", 0xb338, "multihash");
            Register("skein512-200", 0xb339, "multihash");
            Register("skein512-208", 0xb33a, "multihash");
            Register("skein512-216", 0xb33b, "multihash");
            Register("skein512-224", 0xb33c, "multihash");
            Register("skein512-232", 0xb33d, "multihash");
            Register("skein512-240", 0xb33e, "multihash");
            Register("skein512-248", 0xb33f, "multihash");
            Register("skein512-256", 0xb340, "multihash");
            Register("skein512-264", 0xb341, "multihash");
            Register("skein512-272", 0xb342, "multihash");
            Register("skein512-280", 0xb343, "multihash");
            Register("skein512-288", 0xb344, "multihash");
            Register("skein512-296", 0xb345, "multihash");
            Register("skein512-304", 0xb346, "multihash");
            Register("skein512-312", 0xb347, "multihash");
            Register("skein512-320", 0xb348, "multihash");
            Register("skein512-328", 0xb349, "multihash");
            Register("skein512-336", 0xb34a, "multihash");
            Register("skein512-344", 0xb34b, "multihash");
            Register("skein512-352", 0xb34c, "multihash");
            Register("skein512-360", 0xb34d, "multihash");
            Register("skein512-368", 0xb34e, "multihash");
            Register("skein512-376", 0xb34f, "multihash");
            Register("skein512-384", 0xb350, "multihash");
            Register("skein512-392", 0xb351, "multihash");
            Register("skein512-400", 0xb352, "multihash");
            Register("skein512-408", 0xb353, "multihash");
            Register("skein512-416", 0xb354, "multihash");
            Register("skein512-424", 0xb355, "multihash");
            Register("skein512-432", 0xb356, "multihash");
            Register("skein512-440", 0xb357, "multihash");
            Register("skein512-448", 0xb358, "multihash");
            Register("skein512-456", 0xb359, "multihash");
            Register("skein512-464", 0xb35a, "multihash");
            Register("skein512-472", 0xb35b, "multihash");
            Register("skein512-480", 0xb35c, "multihash");
            Register("skein512-488", 0xb35d, "multihash");
            Register("skein512-496", 0xb35e, "multihash");
            Register("skein512-504", 0xb35f, "multihash");
            Register("skein512-512", 0xb360, "multihash");
            Register("skein1024-8", 0xb361, "multihash");
            Register("skein1024-16", 0xb362, "multihash");
            Register("skein1024-24", 0xb363, "multihash");
            Register("skein1024-32", 0xb364, "multihash");
            Register("skein1024-40", 0xb365, "multihash");
            Register("skein1024-48", 0xb366, "multihash");
            Register("skein1024-56", 0xb367, "multihash");
            Register("skein1024-64", 0xb368, "multihash");
            Register("skein1024-72", 0xb369, "multihash");
            Register("skein1024-80", 0xb36a, "multihash");
            Register("skein1024-88", 0xb36b, "multihash");
            Register("skein1024-96", 0xb36c, "multihash");
            Register("skein1024-104", 0xb36d, "multihash");
            Register("skein1024-112", 0xb36e, "multihash");
            Register("skein1024-120", 0xb36f, "multihash");
            Register("skein1024-128", 0xb370, "multihash");
            Register("skein1024-136", 0xb371, "multihash");
            Register("skein1024-144", 0xb372, "multihash");
            Register("skein1024-152", 0xb373, "multihash");
            Register("skein1024-160", 0xb374, "multihash");
            Register("skein1024-168", 0xb375, "multihash");
            Register("skein1024-176", 0xb376, "multihash");
            Register("skein1024-184", 0xb377, "multihash");
            Register("skein1024-192", 0xb378, "multihash");
            Register("skein1024-200", 0xb379, "multihash");
            Register("skein1024-208", 0xb37a, "multihash");
            Register("skein1024-216", 0xb37b, "multihash");
            Register("skein1024-224", 0xb37c, "multihash");
            Register("skein1024-232", 0xb37d, "multihash");
            Register("skein1024-240", 0xb37e, "multihash");
            Register("skein1024-248", 0xb37f, "multihash");
            Register("skein1024-256", 0xb380, "multihash");
            Register("skein1024-264", 0xb381, "multihash");
            Register("skein1024-272", 0xb382, "multihash");
            Register("skein1024-280", 0xb383, "multihash");
            Register("skein1024-288", 0xb384, "multihash");
            Register("skein1024-296", 0xb385, "multihash");
            Register("skein1024-304", 0xb386, "multihash");
            Register("skein1024-312", 0xb387, "multihash");
            Register("skein1024-320", 0xb388, "multihash");
            Register("skein1024-328", 0xb389, "multihash");
            Register("skein1024-336", 0xb38a, "multihash");
            Register("skein1024-344", 0xb38b, "multihash");
            Register("skein1024-352", 0xb38c, "multihash");
            Register("skein1024-360", 0xb38d, "multihash");
            Register("skein1024-368", 0xb38e, "multihash");
            Register("skein1024-376", 0xb38f, "multihash");
            Register("skein1024-384", 0xb390, "multihash");
            Register("skein1024-392", 0xb391, "multihash");
            Register("skein1024-400", 0xb392, "multihash");
            Register("skein1024-408", 0xb393, "multihash");
            Register("skein1024-416", 0xb394, "multihash");
            Register("skein1024-424", 0xb395, "multihash");
            Register("skein1024-432", 0xb396, "multihash");
            Register("skein1024-440", 0xb397, "multihash");
            Register("skein1024-448", 0xb398, "multihash");
            Register("skein1024-456", 0xb399, "multihash");
            Register("skein1024-464", 0xb39a, "multihash");
            Register("skein1024-472", 0xb39b, "multihash");
            Register("skein1024-480", 0xb39c, "multihash");
            Register("skein1024-488", 0xb39d, "multihash");
            Register("skein1024-496", 0xb39e, "multihash");
            Register("skein1024-504", 0xb39f, "multihash");
            Register("skein1024-512", 0xb3a0, "multihash");
            Register("skein1024-520", 0xb3a1, "multihash");
            Register("skein1024-528", 0xb3a2, "multihash");
            Register("skein1024-536", 0xb3a3, "multihash");
            Register("skein1024-544", 0xb3a4, "multihash");
            Register("skein1024-552", 0xb3a5, "multihash");
            Register("skein1024-560", 0xb3a6, "multihash");
            Register("skein1024-568", 0xb3a7, "multihash");
            Register("skein1024-576", 0xb3a8, "multihash");
            Register("skein1024-584", 0xb3a9, "multihash");
            Register("skein1024-592", 0xb3aa, "multihash");
            Register("skein1024-600", 0xb3ab, "multihash");
            Register("skein1024-608", 0xb3ac, "multihash");
            Register("skein1024-616", 0xb3ad, "multihash");
            Register("skein1024-624", 0xb3ae, "multihash");
            Register("skein1024-632", 0xb3af, "multihash");
            Register("skein1024-640", 0xb3b0, "multihash");
            Register("skein1024-648", 0xb3b1, "multihash");
            Register("skein1024-656", 0xb3b2, "multihash");
            Register("skein1024-664", 0xb3b3, "multihash");
            Register("skein1024-672", 0xb3b4, "multihash");
            Register("skein1024-680", 0xb3b5, "multihash");
            Register("skein1024-688", 0xb3b6, "multihash");
            Register("skein1024-696", 0xb3b7, "multihash");
            Register("skein1024-704", 0xb3b8, "multihash");
            Register("skein1024-712", 0xb3b9, "multihash");
            Register("skein1024-720", 0xb3ba, "multihash");
            Register("skein1024-728", 0xb3bb, "multihash");
            Register("skein1024-736", 0xb3bc, "multihash");
            Register("skein1024-744", 0xb3bd, "multihash");
            Register("skein1024-752", 0xb3be, "multihash");
            Register("skein1024-760", 0xb3bf, "multihash");
            Register("skein1024-768", 0xb3c0, "multihash");
            Register("skein1024-776", 0xb3c1, "multihash");
            Register("skein1024-784", 0xb3c2, "multihash");
            Register("skein1024-792", 0xb3c3, "multihash");
            Register("skein1024-800", 0xb3c4, "multihash");
            Register("skein1024-808", 0xb3c5, "multihash");
            Register("skein1024-816", 0xb3c6, "multihash");
            Register("skein1024-824", 0xb3c7, "multihash");
            Register("skein1024-832", 0xb3c8, "multihash");
            Register("skein1024-840", 0xb3c9, "multihash");
            Register("skein1024-848", 0xb3ca, "multihash");
            Register("skein1024-856", 0xb3cb, "multihash");
            Register("skein1024-864", 0xb3cc, "multihash");
            Register("skein1024-872", 0xb3cd, "multihash");
            Register("skein1024-880", 0xb3ce, "multihash");
            Register("skein1024-888", 0xb3cf, "multihash");
            Register("skein1024-896", 0xb3d0, "multihash");
            Register("skein1024-904", 0xb3d1, "multihash");
            Register("skein1024-912", 0xb3d2, "multihash");
            Register("skein1024-920", 0xb3d3, "multihash");
            Register("skein1024-928", 0xb3d4, "multihash");
            Register("skein1024-936", 0xb3d5, "multihash");
            Register("skein1024-944", 0xb3d6, "multihash");
            Register("skein1024-952", 0xb3d7, "multihash");
            Register("skein1024-960", 0xb3d8, "multihash");
            Register("skein1024-968", 0xb3d9, "multihash");
            Register("skein1024-976", 0xb3da, "multihash");
            Register("skein1024-984", 0xb3db, "multihash");
            Register("skein1024-992", 0xb3dc, "multihash");
            Register("skein1024-1000", 0xb3dd, "multihash");
            Register("skein1024-1008", 0xb3de, "multihash");
            Register("skein1024-1016", 0xb3df, "multihash");
            Register("skein1024-1024", 0xb3e0, "multihash");

            Register("chacha20-poly1305", 0xa000, "multikey");

            Register("es256k-msig", 0xd01300, "multisig");
            Register("bls12_381-g1-msig", 0xd01301, "multisig");
            Register("bls12_381-g2-msig", 0xd01302, "multisig");
            Register("eddsa-msig", 0xd01303, "multisig");
            Register("bls12_381-g1-share-msig", 0xd01304, "multisig");
            Register("bls12_381-g2-share-msig", 0xd01305, "multisig");
            Register("lamport-msig", 0xd01306, "multisig");
            Register("lamport-share-msig", 0xd01307, "multisig");
            Register("es256-msig", 0xd01308, "multisig");
            Register("es384-msig", 0xd01309, "multisig");
            Register("es521-msig", 0xd0130a, "multisig");
            Register("rs256-msig", 0xd0130b, "multisig");
            Register("lamport-sha3-512-sig", 0x1a44, "multisig");
            Register("lamport-sha3-384-sig", 0x1a45, "multisig");
            Register("lamport-sha3-256-sig", 0x1a46, "multisig");
            Register("lamport-sha3-512-sig-share", 0x1a54, "multisig");
            Register("lamport-sha3-384-sig-share", 0x1a55, "multisig");
            Register("lamport-sha3-256-sig-share", 0x1a56, "multisig");

            Register("skynet-ns", 0xb19910, "namespace");
            Register("arweave-ns", 0xb29910, "namespace");
            Register("subspace-ns", 0xb39910, "namespace");
            Register("kumandra-ns", 0xb49910, "namespace");
            Register("path", 0x2f, "namespace");
            Register("lbry", 0x8c, "namespace");
            Register("streamid", 0xce, "namespace");
            Register("ipld", 0xe2, "namespace");
            Register("ipfs", 0xe3, "namespace");
            Register("swarm", 0xe4, "namespace");
            Register("ipns", 0xe5, "namespace");
            Register("zeronet", 0xe6, "namespace");
            Register("dnslink", 0xe8, "namespace");

            Register("nonce", 0x123b, "nonce");

            Register("provenance-log", 0x1208, "serialization");
            Register("provenance-log-entry", 0x1209, "serialization");
            Register("provenance-log-script", 0x120a, "serialization");
            Register("ssz", 0xb501, "serialization");
            Register("car-index-sorted", 0x0400, "serialization");
            Register("car-multihash-index-sorted", 0x0401, "serialization");
            Register("messagepack", 0x0201, "serialization");
            Register("car", 0x0202, "serialization");
            Register("ipns-record", 0x0300, "serialization");
            Register("rlp", 0x60, "serialization");
            Register("bencode", 0x63, "serialization");
            Register("protobuf", 0x50, "serialization");

            Register("shelter-contract-manifest", 0x511e00, "shelter");
            Register("shelter-contract-text", 0x511e01, "shelter");
            Register("shelter-contract-data", 0x511e02, "shelter");
            Register("shelter-file-manifest", 0x511e03, "shelter");
            Register("shelter-file-chunk", 0x511e04, "shelter");

            Register("iscc", 0xcc01, "softhash");

            Register("transport-bitswap", 0x0900, "transport");
            Register("transport-graphsync-filecoinv1", 0x0910, "transport");
            Register("transport-ipfs-gateway-http", 0x0920, "transport");

            Register("es256k", 0xd0e7, "varsig");
            Register("bls12_381-g1-sig", 0xd0ea, "varsig");
            Register("bls12_381-g2-sig", 0xd0eb, "varsig");
            Register("eddsa", 0xd0ed, "varsig");
            Register("eip-191", 0xd191, "varsig");
            Register("es256", 0xd01200, "varsig");
            Register("es284", 0xd01201, "varsig");
            Register("es512", 0xd01202, "varsig");
            Register("rs256", 0xd01205, "varsig");
            Register("nonstandard-sig", 0xd000, "varsig");
            Register("vlad", 0x1207, "vlad");

            Register("zeroxcert-imprint-256", 0xce11, "zeroxcert");
        }

        /// <summary>
        ///   Use <see cref="Register"/> to create codec new instance of codec <see cref="Codec"/>.
        /// </summary>
        private MultiCodecCoder()
        {
            Name = string.Empty;
            Tag = string.Empty;
        }

        /// <summary>
        ///   Register codec new IPFS codec.
        /// </summary>
        /// <param name="name">
        ///   The name of the codec.
        /// </param>
        /// <param name="code">
        ///   The IPFS code assigned to the codec.
        /// </param>
        /// <returns>
        ///   A new <see cref="MultiCodecCoder"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///   When the <paramref name="name"/> or <paramref name="code"/> is already defined.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   When the <paramref name="name"/> is null or empty.
        /// </exception>
        public static MultiCodecCoder Register(string name, int code, string tag)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
             
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentNullException(nameof(tag));
            }
             
            var codec = new MultiCodecCoder
            {
                Name = name,
                Code = code,
                Tag = tag
            };

            _memberCodecs.Add(codec);

            return codec;
        }

        /// <summary>
        ///   Remove an IPFS codec from the registry.
        /// </summary>
        /// <param name="codec">
        ///   The <see cref="MultiCodecCoder"/> to remove.
        /// </param>
        public static void Deregister(MultiCodecCoder codec)
        {
            _memberCodecs.Remove(codec);
        }


        /// <summary>
        ///   The <see cref="Name"/> of the codec.
        /// </summary>
        /// <value>
        ///   The <see cref="Name"/> of the codec.
        /// </value>
        public override string ToString()
        {
            return Name ?? "Unknown";
        }
    }
}
