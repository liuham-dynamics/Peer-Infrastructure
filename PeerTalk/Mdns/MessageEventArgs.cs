using PeerTalk.Dns;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PeerTalk.Mdns
{
    /// <summary>
    ///   The event data for <see cref="MulticastService.QueryReceived"/> or
    ///   <see cref="MulticastService.AnswerReceived"/>.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        ///   The DNS message.
        /// </summary>
        /// <value>
        ///   The received message.
        /// </value>
        public Message Message { get; set; } = new Message();

        /// <summary>
        ///   The DNS message sender endpoint.
        /// </summary>
        /// <value>
        ///   The endpoint from the message was received.
        /// </value>
        public IPEndPoint RemoteEndPoint { get; set; } = IPEndPoint.Parse("0.0.0.0");

        /// <summary>
        ///   Determines if the sender is using legacy unicast DNS.
        /// </summary>
        /// <value>
        ///   <b>false</b> if the sender is using port 5353.
        /// </value>
        public bool IsLegacyUnicast => RemoteEndPoint.Port != MulticastClient.MulticastPort;
    }
}
