using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PeerTalk.Mdns
{
    /// <summary>
    ///   Performs the magic to send and receive datagrams over multicast
    ///   sockets.
    /// </summary>
    internal class MulticastClient : IDisposable
    {
        private static readonly IPAddress _memberMulticastAddressIp4 = IPAddress.Parse("224.0.0.251");
        private static readonly IPAddress _memberMulticastAddressIp6 = IPAddress.Parse("FF02::FB");
        private static readonly IPEndPoint _memberMdnsEndpointIp4 = new(_memberMulticastAddressIp4, MulticastPort);
        private static readonly IPEndPoint _memberMdnsEndpointIp6 = new(_memberMulticastAddressIp6, MulticastPort);
         
        private readonly List<UdpClient> _memberReceivers;
        private readonly ConcurrentDictionary<IPAddress, UdpClient> _memberSenders = new();

        public event EventHandler<UdpReceiveResult> MessageReceived;

        /// <summary>
        ///   The port number assigned to Multicast DNS.
        /// </summary>
        /// <value>
        ///   Port number 5353.
        /// </value>
        public static readonly int MulticastPort = 5353;

 
        public MulticastClient(bool useIPv4, bool useIpv6, IEnumerable<NetworkInterface> nics)
        {
            // Setup the _memberReceivers.
            _memberReceivers = [];

            UdpClient receiver4 = new();
            if (useIPv4)
            {
                receiver4 = new UdpClient(AddressFamily.InterNetwork);
                receiver4.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                receiver4.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));
                _memberReceivers.Add(receiver4);
            }

            UdpClient receiver6 = new();
            if (useIpv6)
            {
                receiver6 = new UdpClient(AddressFamily.InterNetworkV6);
                receiver6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                receiver6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, MulticastPort));
                _memberReceivers.Add(receiver6);
            }

            // Get the IP addresses that we should send to.
            var addreses = nics.SelectMany(GetNetworkInterfaceLocalAddresses)
                               .Where(a => (useIPv4 && a.AddressFamily == AddressFamily.InterNetwork)
                                        || (useIpv6 && a.AddressFamily == AddressFamily.InterNetworkV6));
            foreach (var address in addreses)
            {
                if (_memberSenders.ContainsKey(address))
                {
                    continue;
                }

                var localEndpoint = new IPEndPoint(address, MulticastPort);
                var sender = new UdpClient(address.AddressFamily);
                try
                {
                    switch (address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            receiver4.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_memberMulticastAddressIp4, address));
                            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            sender.Client.Bind(localEndpoint);
                            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_memberMulticastAddressIp4));
                            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                            break;

                        case AddressFamily.InterNetworkV6:
                            receiver6.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(_memberMulticastAddressIp6, address.ScopeId));
                            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            sender.Client.Bind(localEndpoint);
                            sender.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(_memberMulticastAddressIp6));
                            sender.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, true);
                            break;

                        default:
                            throw new NotSupportedException($"Address family {address.AddressFamily}.");
                    }
                     
                    if (!_memberSenders.TryAdd(address, sender)) // Should not fail
                    {
                        sender.Dispose();
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressNotAvailable)
                {
                    sender.Dispose(); // VPN NetworkInterfaces
                }
                catch (Exception)
                {
                    sender.Dispose();
                }
            }

            // Start listening for messages.
            foreach (var r in _memberReceivers)
            {
                Listen(r);
            }
        }

        public async Task SendAsync(byte[] message)
        {
            foreach (var sender in _memberSenders)
            {
                try
                {
                    var endpoint = sender.Key.AddressFamily == AddressFamily.InterNetwork ? _memberMdnsEndpointIp4 : _memberMdnsEndpointIp6;
                    await sender.Value.SendAsync(message, message.Length, endpoint).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // TODO: Add logging
                }
            }
        }

        private void Listen(UdpClient receiver)
        {
            // ReceiveAsync does not support cancellation.  So the receiver is disposed
            // to stop it. See https://github.com/dotnet/corefx/issues/9848
            Task.Run(async () =>
            {
                try
                {
                    var task = receiver.ReceiveAsync();

                    _ = task.ContinueWith(x => Listen(receiver), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.RunContinuationsAsynchronously);

                    _ = task.ContinueWith(x => MessageReceived?.Invoke(this, x.Result), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.RunContinuationsAsynchronously);

                    await task.ConfigureAwait(false);
                }
                catch
                {
                    return;
                }
            });
        }

        private IEnumerable<IPAddress> GetNetworkInterfaceLocalAddresses(NetworkInterface nic)
        {
            return nic.GetIPProperties()
                      .UnicastAddresses
                      .Select(x => x.Address)
                      .Where(x => x.AddressFamily != AddressFamily.InterNetworkV6 || x.IsIPv6LinkLocal);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MessageReceived = null;

                    foreach (var receiver in _memberReceivers)
                    {
                        try
                        {
                            receiver.Dispose();
                        }
                        catch
                        {
                            // eat it.
                        }
                    }
                    _memberReceivers.Clear();

                    foreach (var address in _memberSenders.Keys)
                    {
                        if (_memberSenders.TryRemove(address, out var sender))
                        {
                            try
                            {
                                sender.Dispose();
                            }
                            catch
                            {
                                // eat it.
                            }
                        }
                    }
                    _memberSenders.Clear();
                }

                disposedValue = true;
            }
        }

        ~MulticastClient()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
