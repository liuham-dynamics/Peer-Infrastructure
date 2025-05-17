namespace PeerStack.Transport
{
    /// <summary>
    /// WEB RTC transport network protocol
    /// </summary>
    public class WebRtcNetworkTransport : AValuelessNetworkTransport
    {
        /// <summary>
        /// Network transport protocol code
        /// </summary>
        public override uint Code => 275;

        /// <summary>
        /// Network transport protocol name
        /// </summary>
        public override string Name => "webrtc";
      
    }
}
