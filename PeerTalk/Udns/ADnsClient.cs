using PeerTalk.Dns;
using PeerTalk.Dns.Records;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using PeerStack.Utilities.Extensions;

namespace PeerTalk.Udns
{
    /// <summary>
    ///   Base class for a DNS client.
    /// </summary>
    /// <remarks>
    ///   Sends and receives DNS queries and answers to unicast DNS servers.
    /// </remarks>
    public abstract class ADnsClient : IDnsClient
    {
        private int _memberNextQueryId = new Random().Next((int)ushort.MaxValue + 1);

        /// <inheritdoc />
        public bool ThrowResponseError { get; set; } = true;

        /// <inheritdoc />
        public ushort NextQueryId()
        {
            var next = Interlocked.Increment(ref _memberNextQueryId);
            return (ushort)next;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IPAddress>> ResolveAsync(DomainName name, CancellationToken cancel = default)
        {
            var a = QueryAsync(name, DnsType.A, cancel);
            var aaaa = QueryAsync(name, DnsType.AAAA, cancel);
            var responses = await Task.WhenAll(a, aaaa).ConfigureAwait(false);
            return responses
                        .SelectMany(m => m.Answers)
                        .Where(rr => rr.Type == DnsType.A || rr.Type == DnsType.AAAA)
                        .Select(rr => rr.Type == DnsType.A
                                    ? ((ARecord)rr).Address : ((AAAARecord)rr).Address);
        }

        /// <inheritdoc />
        public Task<Message> QueryAsync(DomainName name, DnsType rtype, CancellationToken cancel = default)
        {
            var query = new Message
            {
                Id = NextQueryId(),
                RD = true,
            };
            query.Questions.Add(new Question { Name = name, Type = rtype });

            return QueryAsync(query, cancel);
        }

        /// <inheritdoc />
        public Task<Message> SecureQueryAsync(DomainName name, DnsType rtype, CancellationToken cancel = default)
        {
            var query = new Message
            {
                Id = NextQueryId(),
                RD = true
            }.UseDnsSecurity();
            query.Questions.Add(new Question { Name = name, Type = rtype });

            return QueryAsync(query, cancel);
        }

        /// <inheritdoc />
        public async Task<DomainName> ResolveAsync(IPAddress address, CancellationToken cancel = default)
        {
            var response = await QueryAsync(address.GetArpaName(), DnsType.PTR, cancel).ConfigureAwait(false);
            return response?.Answers
                           .OfType<PTRRecord>()
                           .Select(p => p.DomainName)
                           .FirstOrDefault()!;
        }

        /// <inheritdoc />
        public abstract Task<Message> QueryAsync(Message request, CancellationToken cancel = default);

        /// <summary>
        ///   Another name for <see cref="QueryAsync(Message, CancellationToken)"/>.
        /// </summary>
        public Task<Message> ResolveAsync(Message request, CancellationToken cancel = default)
        {
            return QueryAsync(request, cancel);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /// <summary>
        ///   Dispose the client.
        /// </summary>
        /// <param name="disposing">
        ///   <b>true</b> if managed resources should be disposed.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

    }
}
