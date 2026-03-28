using PeerTalk.Dns;
using PeerTalk.Dns.Records;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PeerTalk.Udns
{
    /// <summary>
    ///   Client to a DNS server over TLS.
    /// </summary>
    /// <remarks>
    ///   DNS over TLS is a security protocol for encrypting and wrapping
    ///   DNS queries and answers via the Transport Layer Security (TLS) protocol. The goal
    ///   is to increase user privacy and security by preventing eavesdropping and
    ///   manipulation of DNS data via man-in-the-middle attacks.
    ///   <para>
    ///   All queries are padded to the closest multiple of <see cref="BlockLength"/> octets.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc7858"/>
    /// <seealso href="https://tools.ietf.org/html/rfc8310"/>
    public class DotClient : ADnsClient
    {
        private SslStream dnsServer;
        private readonly Random rng = new();

        /// <summary>
        ///   The default port of a DOT server.
        /// </summary>
        public const int DefaultPort = 853;

        /// <summary>
        ///   Known servers that support DNS over TLS.
        /// </summary>
        /// <value>
        ///   Sequence of known servers.
        /// </value>
        /// <remarks>
        ///   This is the default list that <see cref="Servers"/> uses.
        /// </remarks>
        public static readonly DotEndPoint[] PublicServers =
        [
            new DotEndPoint
            {
                Hostname = "dns.google",
                Address = IPAddress.Parse("2001:4860:4860::8888")
            },
            new DotEndPoint
            {
                Hostname = "dns.google",
                Address = IPAddress.Parse("2001:4860:4860::8844")
            },
            new DotEndPoint
            {
                Hostname = "dns.google",
                Address = IPAddress.Parse("8.8.8.8")
            },
            new DotEndPoint
            {
                Hostname = "dns.google",
                Address = IPAddress.Parse("8.8.4.4")
            },
            new DotEndPoint
            {
                Hostname = "dns.opendns.com",
                Address = IPAddress.Parse("208.67.222.222")
            },
            new DotEndPoint
            {
                Hostname = "dns.opendns.com",
                Address = IPAddress.Parse("208.67.220.220")
            },
            new DotEndPoint
            {
                Hostname = "dns.quad9.net",
                Address = IPAddress.Parse("9.9.9.9")
            },
            new DotEndPoint
            {
                Hostname = "dns.quad9.net",
                Address = IPAddress.Parse("149.112.112.112")
            },
            new DotEndPoint
            {
                Hostname = "dns11.quad9.net",
                Address = IPAddress.Parse("9.9.9.11")
            },
            new DotEndPoint
            {
                Hostname = "dns11.quad9.net",
                Address = IPAddress.Parse("149.112.112.11")
            },
            new DotEndPoint
            {
                Hostname = "family-filter-dns.cleanbrowsing.org",
                Address = IPAddress.Parse("185.228.168.168")
            },
            new DotEndPoint
            {
                Hostname = "family-filter-dns.cleanbrowsing.org",
                Address = IPAddress.Parse("185.228.169.168")
            },
            new DotEndPoint
            {
                Hostname = "alternate-dns.com",
                Address = IPAddress.Parse("76.76.19.19")
            },
            new DotEndPoint
            {
                Hostname = "alternate-dns.com",
                Address = IPAddress.Parse("76.223.122.150")
            },
            new DotEndPoint
            {
                Hostname = "cloudflare-dns.com",
                Address = IPAddress.Parse("1.1.1.1")
            },
            new DotEndPoint
            {
                Hostname = "cloudflare-dns.com",
                Address = IPAddress.Parse("1.0.0.1")
            },
            new DotEndPoint
            {
                Hostname = "cloudflare-dns.com",
                Address = IPAddress.Parse("2606:4700:4700::1111")
            },
            new DotEndPoint
            {
                Hostname = "cloudflare-dns.com",
                Address = IPAddress.Parse("2606:4700:4700::1001")
            },
            new DotEndPoint // Downwards are local DNS unique to your country or region
            {
                Hostname = "nigcomsat.ng",
                Address = IPAddress.Parse("41.57.120.177")
            }
        ];

        /// <summary>
        ///   The number of octets for padding.
        /// </summary>
        /// <value>
        ///   Defaults to 128.
        /// </value>
        /// <remarks>
        ///   All queries are padded to the closest multiple of <see cref="BlockLength"/> octets.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc8467#section-4.1"/>
        public int BlockLength { get; set; } = 128;

        /// <summary>
        ///   Time to wait for a DNS response.
        /// </summary>
        /// <value>
        ///   The default is 4 seconds.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        ///   The DNS over TLS servers to communication with.
        /// </summary>
        /// <value>
        ///   A sequence of DOT endpoints.  The default is the <see cref="PublicServers"/>.
        /// </value>
        public IEnumerable<DotEndPoint> Servers { get; set; } = PublicServers;

        /// <summary>
        ///   Outstanding requests.
        /// </summary>
        /// <value>
        ///   Key is the request's <see cref="Message.Id"/>.
        /// </value>
        /// <remarks>
        ///   Contains the requests that are waiting for a response.
        /// </remarks>
        private readonly ConcurrentDictionary<ushort, TaskCompletionSource<Message>> OutstandingRequests = new();

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
        ///   Sends the <paramref name="request"/> and waits for
        ///   the matching response.
        /// </remarks>
        public override async Task<Message> QueryAsync(Message request, CancellationToken cancel = default)
        {
            // Find a server.
            var server = await GetDnsServerAsync().ConfigureAwait(false);
            if (server is null)
            {
                throw new Exception("No DNS over TLS server can be found.");
            }

            // Build the TCP request.
            var tcpRequest = BuildRequest(request);

            // Cancel the request when either the timeout is reached or the
            // task is cancelled by the caller.
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel, new CancellationTokenSource(Timeout).Token);
            var tcs = new TaskCompletionSource<Message>();
            if (!OutstandingRequests.TryAdd(request.Id, tcs))
            {
                cts.Dispose();
                throw new Exception($"An outstanding request already exists with the ID {request.Id}.");
            }

            Message dnsResponse;
            try
            {
                await server.WriteAsync(tcpRequest, cts.Token).ConfigureAwait(false);
                await server.FlushAsync(cts.Token).ConfigureAwait(false);

                dnsResponse = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (server?.CanRead == false)
            {
                cts.Dispose();
                OutstandingRequests.TryRemove(request.Id, out var _);

                return await QueryAsync(request, cancel).ConfigureAwait(false);
            }
            finally
            {
                cts.Dispose();
                OutstandingRequests.TryRemove(request.Id, out var _);
            }

            // Checks that response is valid.
            if (ThrowResponseError)
            {
                if (!dnsResponse.IsResponse)
                {
                    throw new FormatException("DNS response is not a response.");
                }
                else if (dnsResponse.TC)
                {
                    throw new FormatException("DNS response should not be truncated.");
                }
                else if (dnsResponse.Status != MessageStatus.NoError)
                {

                    throw new IOException($"DNS error '{dnsResponse.Status}'.");
                }
            }

            return dnsResponse;
        }

        private byte[] BuildRequest(Message request)
        {
            // Always have a query ID.
            if (request.Id == 0)
            {
                request.Id = this.NextQueryId();
            }

            // Add an OPT if not already present.
            var opt = request.AdditionalRecords.OfType<OPTRecord>().FirstOrDefault();
            if (opt is null)
            {
                opt = new OPTRecord();
                request.AdditionalRecords.Add(opt);
            }

            // Keep the connection alive.
            if (!opt.Options.Any(o => o.Type == EdnsOptionType.Keepalive))
            {
                var keepalive = new EdnsKeepaliveOption
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                opt.Options.Add(keepalive);
            }
            ;

            // Always use padding. Must be the last transform.
            if (!opt.Options.Any(o => o.Type == EdnsOptionType.Padding))
            {
                var paddingOption = new EdnsPaddingOption();
                opt.Options.Add(paddingOption);
                var need = BlockLength - ((request.Length() + 2) % BlockLength);
                if (need > 0)
                {
                    paddingOption.Padding = new byte[need];
                    rng.NextBytes(paddingOption.Padding);
                }
            }
            ;

            using (var tcpRequest = new MemoryStream())
            {
                tcpRequest.WriteByte(0); // two byte length prefix
                tcpRequest.WriteByte(0);
                request.Write(tcpRequest); // udpRequest
                var length = (ushort)(tcpRequest.Length - 2);
                tcpRequest.Position = 0;
                tcpRequest.WriteByte((byte)(length >> 8));
                tcpRequest.WriteByte((byte)(length));
                return tcpRequest.ToArray();
            }
        }

        /// <summary>
        ///   Get the stream to a DNS server.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetDnsServerAsync()
        {
            // Is current server still good to go?
            if (dnsServer?.CanRead == true && dnsServer.CanWrite)
            {
                return dnsServer;
            }


            if (dnsServer?.CanRead == true && dnsServer.CanWrite)
            {
                return dnsServer;
            }

            dnsServer?.Dispose();

            var servers = Servers.Where(s =>
                (Socket.OSSupportsIPv4 && s.Address.AddressFamily == AddressFamily.InterNetwork) ||
                (Socket.OSSupportsIPv6 && s.Address.AddressFamily == AddressFamily.InterNetworkV6));
            foreach (var endPoint in servers)
            {
                try
                {
                    var socket = new Socket(endPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(endPoint.Address, endPoint.Port).ConfigureAwait(false);
                    Stream stream = new NetworkStream(socket, ownsSocket: true);
                    dnsServer = new SslStream(stream, false, // leave inner stream open
                       (sender, certificate, chain, errors) =>
                       {
                           return ValidateServerCertificate(sender, certificate, chain, errors, endPoint.Pins);
                       }, null, EncryptionPolicy.RequireEncryption);
                    await dnsServer.AuthenticateAsClientAsync(endPoint.Hostname).ConfigureAwait(false);

                    await Task.Run(() => ReadResponses(dnsServer)).ConfigureAwait(false);

                    return dnsServer;
                }
                catch (SocketException e)
                {
                    // ignore for now
                }
                catch (Exception e)
                {
                    //ignore for now
                }
            }

            return Stream.Null;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
                                                      X509Chain chain, SslPolicyErrors sslPolicyErrors, string[] pins)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            // Verify that the certificate's SPKI matches one of the PINs.
            if (pins == null || pins.Length == 0)
            {
                return true;
            }

            var cert2 = certificate as X509Certificate2 ?? new X509Certificate2(certificate);

            // SPKI hash (SHA256)
            var spki = cert2.PublicKey.EncodedKeyValue.RawData;
            var hash = SHA256.HashData(spki);
            var pin = Convert.ToBase64String(hash);

            return pins.Contains(pin, StringComparer.Ordinal);
        }

        private void ReadResponses(Stream stream)
        {
            var reader = new DnsWireReader(stream);
            while (stream.CanRead)
            {
                try
                {
                    var length = reader.ReadUInt16();
                    if (length < Message.MinLength)
                        throw new InvalidDataException("DNS response is too small.");
                    if (length > Message.MaxLength)
                        throw new InvalidDataException("DNS response exceeded max length.");
                    Message response;
                    var packet = reader.ReadBytes(length);
                    try
                    {
                        // TODO: Should work, but doesn't
                       response = (Message)new Message().Read(reader);
                       //    response = (Message)new Message().Read(packet);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                    // Find matching request.
                    if (!OutstandingRequests.TryGetValue(response.Id, out var task))
                    {
                        continue;
                    }

                    // Continue the request.
                    task.SetResult(response);
                }
                catch (EndOfStreamException)
                {
                    stream.Dispose();
                }
                catch (Exception e)
                {
                    stream.Dispose();
                }
            }

            // Cancel any outstanding queries.
            foreach (var task in OutstandingRequests.Values)
            {
                task.SetCanceled();
            }
        }


        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dnsServer?.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
