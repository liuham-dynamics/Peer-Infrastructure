using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack
{
    public sealed record ContentTypes
    {
        public const string Cbor = "dag-cbor";
        public const string Protobuf = "dag-pb";
        public const string Json = "dag-json";
    }
}
