using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Multiformat
{
    public static class MultiCodecTags
    {
        public static string Unknow => "unknown";
        public static string Cid => "cid";
        public static string Encryption => "encryption";
        public static string Filecoin => "filecoin";
        public static string Hash => "hash";
        public static string Holochain => "holochain";
        public static string Ipld => "ipld";
        public static string Key => "key";
        public static string Libp2p => "libp2p";
        public static string Multiaddr => "multiaddr";
        public static string Multiformat => "multiformat";
        public static string Multihash => "multihash";
        public static string Multikey => "multikey";
        public static string Multisig => "multisig";
        public static string Namespace => "namespace";
        public static string Nonce => "nonce";
        public static string Serialization => "serialization";
        public static string Shelter => "shelter";
        public static string Softhash => "softhash";
        public static string Transport => "transport";
        public static string Varsig => "varsig";
        public static string Vlad => "vlad";
        public static string Zeroxcert => "zeroxcert";
    }
}
