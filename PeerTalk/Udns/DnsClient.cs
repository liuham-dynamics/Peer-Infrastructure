using PeerTalk.Dns;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PeerTalk.Udns
{
    /// <summary>
    ///   Client to a unicast DNS server.
    /// </summary>
    /// <remarks>
    ///   Sends and receives DNS queries and answers to unicast DNS servers.
    /// </remarks>
    public class DnsClient : ADnsClient
    {
        private static readonly TimeSpan timeOut = TimeSpan.FromSeconds(4);
        private const int DnsPort = 53;

        private IEnumerable<IPAddress> servers;

        /// <summary>
        ///   Time to wait for a DNS UDP response.
        /// </summary>
        /// <value>
        ///   The default is 4 seconds.
        /// </value>
        public TimeSpan TimeoutUdp { get; set; } = timeOut;

        /// <summary>
        ///   Time to wait for a DNS TCP response.
        /// </summary>
        /// <value>
        ///   The default is 4 seconds.
        /// </value>
        public TimeSpan TimeoutTcp { get; set; } = timeOut;

        /// <summary>
        ///   The DNS servers to communication with.
        /// </summary>
        /// <value>
        ///   A sequence of IP addresses.  When <b>null</b> <see cref="GetServers"/>
        ///   is used. The default is <b>null</b>.
        /// </value>
        public IEnumerable<IPAddress> Servers
        {
            get
            {
                return servers ?? GetServers();
            }
            set
            {
                servers = value;
            }
        }

        /// <summary>
        ///   Get the DNS servers that can be communicated with.
        /// </summary>
        /// <returns>
        ///   A sequence of IP addresses for the DNS servers.
        /// </returns>
        /// <remarks>
        ///   Only servers with an <see cref="AddressFamily"/> supported by
        ///   the OS is returned.
        /// </remarks>
        public IEnumerable<IPAddress> AvailableServers()
        {
            return Servers
                .Where(a =>
                    (Socket.OSSupportsIPv4 && a.AddressFamily == AddressFamily.InterNetwork) ||
                    (Socket.OSSupportsIPv6 && a.AddressFamily == AddressFamily.InterNetworkV6));
        }

        /// <summary>
        ///   Get the DNS servers.
        /// </summary>
        /// <returns>
        ///   A sequence of IP addresses for the DNS servers.
        /// </returns>
        public IEnumerable<IPAddress> GetServers()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                                            && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                            && nic.NetworkInterfaceType != NetworkInterfaceType.Unknown)
                                   .SelectMany(nic => nic.GetIPProperties().DnsAddresses);
        }

        /// <summary>
        ///   Send a DNS query with the specified message.
        /// </summary>
        /// <param name="request">
        ///   A <see cref="Message"/> containing a <see cref="Question"/>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's value
        ///   contains the response <see cref="Message"/>.
        /// </returns>
        /// <exception cref="IOException">
        ///   When the DNS server returns error status or no response.
        /// </exception>
        /// <remarks>
        ///   The <paramref name="request"/> is sent with UDP.  If no response is
        ///   received (or is truncated) in <see cref="TimeoutUdp"/>, then it is resent via TCP.
        ///   <para>
        ///   Some home routers have issues with IPv6, so IPv4 servers are tried first.
        ///   </para>
        /// </remarks>
        public override async Task<Message> QueryAsync(Message request, CancellationToken cancel = default)
        {
            var servers = AvailableServers()
                .OrderBy(a => a.AddressFamily)
                .ToArray();
            if (servers.Length == 0)
            {
                throw new Exception("No DNS servers are available.");
            }

            var msg = request.ToByteArray();
            Message response = null;

            foreach (var server in servers)
            {
                response = await QueryAsync(msg, server, cancel);
                if (response != null) break;
            }

            // Check the response.
            if (response is null)
            {
                throw new IOException("No response from DNS servers.");
            }

            if (ThrowResponseError && response.Status != MessageStatus.NoError)
            {
                throw new IOException($"DNS error '{response.Status}'.");
            }

            return response;
        }

        private async Task<Message?> QueryAsync(byte[] request, IPAddress server, CancellationToken cancel)
        {
            // Try UDP first.
            using var udpCts = CancellationTokenSource.CreateLinkedTokenSource(cancel,
                                                                               new CancellationTokenSource(TimeoutUdp).Token);

            try
            {
                var response = await QueryUdpAsync(request, server, udpCts.Token).ConfigureAwait(false);

                // If not truncated, return it
                if (response?.TC == false)
                {
                    return response;
                }
            }
            catch (SocketException)
            {
                // ignore and fallback to TCP
            }
            catch (TaskCanceledException)
            {
                // ignore and fallback to TCP
            }

            // fallback to TCP
            using var tcpCts = CancellationTokenSource.CreateLinkedTokenSource(cancel,
                                                                               new CancellationTokenSource(TimeoutTcp).Token);

            try
            {
                return await QueryTcpAsync(request, server, tcpCts.Token).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }


        // Send the request via UDP and wait for the response.
        private async Task<Message> QueryUdpAsync(byte[] request, IPAddress server, CancellationToken cancel)
        {
            var endPoint = new IPEndPoint(server, DnsPort);

            using var client = new UdpClient(server.AddressFamily);

            await client.SendAsync(request, request.Length, endPoint)
                        .ConfigureAwait(false);

            var result = await client.ReceiveAsync()
                                     .WaitAsync(cancel)
                                     .ConfigureAwait(false);

            return (Message)(new Message().Read(result.Buffer));
        }

        private async Task<Message?> QueryTcpAsync(byte[] request, IPAddress server, CancellationToken cancel)
        {
           
            using (var client = new TcpClient(server.AddressFamily))
            {
                await client.ConnectAsync(server, DnsPort)
                            .WaitAsync(cancel);
                using (var stream = client.GetStream())
                {
                    // The message is prefixed with a two byte length field which gives
                    // the message length, excluding the two byte length field.
                    byte[] length = BitConverter.GetBytes((ushort)request.Length);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(length);
                    }
                    await stream.WriteAsync(length, cancel);
                    await stream.WriteAsync(request, cancel);
                    await stream.FlushAsync(cancel);

                    // Read response length
                    var buffer = new byte[2];
                    var n = await stream
                        .ReadAsync(buffer, 0, buffer.Length)
                        .WaitAsync(cancel);
                    if (n == 0)
                    {
                        return null;
                    }

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer);
                    }
                    var responseLength = BitConverter.ToUInt16(buffer, 0);

                    // Read response message
                    buffer = new byte[responseLength];
                    n = await stream.ReadAsync(buffer, cancel);
                    return (Message)(new Message().Read(buffer, 0, n));
                }
            }
        }
    }
}
