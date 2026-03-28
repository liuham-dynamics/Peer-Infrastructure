using PeerTalk.Dns;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Udns
{
    /// <summary>
    ///   Client to a DNS server over HTTPS.
    /// </summary>
    /// <remarks>
    ///   DNS over HTTPS (DoH) is an experimental protocol for performing remote
    ///   Domain Name System (DNS) resolution via the HTTPS protocol. The goal
    ///   is to increase user privacy and security by preventing eavesdropping and
    ///   manipulation of DNS data by man-in-the-middle attacks.
    ///   <para>
    ///   The <b>DohClient</b> uses the HTTP POST method to hide as much
    ///   information as is possible.  Also, it tends to generate smaller
    ///   requests.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://en.wikipedia.org/wiki/DNS_over_HTTPS"/>
    public class DohClient : ADnsClient
    {
        private HttpClient httpClient;
        private readonly object httpClientLock = new();

        private const string primaryUrl = "https://dns.google/resolve";
        private const string alternateUrl = "https://cloudflare-dns.com/dns-query";

        /// <summary>
        ///   Time to wait for a DNS response.
        /// </summary>
        /// <value>
        ///   The default is 4 seconds.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Use the primary URL to communicate with DNS
        /// </summary>
        ///       /// <value>
        ///   The default is true (Google DNS).
        /// </value>
        public bool UsePrimaryUrl { get; set; } = true;

        /// <summary>
        ///   The DNS server to communication with.
        /// </summary>
        /// <value>
        ///   Primary URL "https://dns.google/resolve".
        ///   Alternate URL "https://cloudflare-dns.com/dns-query".
        /// </value>
        public string ServerUrl { get; set; } = alternateUrl;

        /// <summary>
        ///   The MIME type for a DNS message encoded in UPD wire format.
        /// </summary>
        /// <remarks>
        ///   Previous drafts defined this as "application/dns-udpwireformat".
        /// </remarks>
        public const string DnsWireFormat = "application/dns-message";

        /// <summary>
        ///   The MIME type for a DNS message encoded in JSON.
        /// </summary>
        public const string DnsJsonFormat = "application/dns-json";

        /// <summary>
        ///   The client that sends HTTP requests and receives HTTP responses.
        /// </summary>
        /// <remarks>
        ///   It is best practice to use only one <see cref="HttpClient"/> in an
        ///   application.
        /// </remarks>
        public HttpClient HttpClient
        {
            get
            {
                if (httpClient is null)
                {
                    lock (httpClientLock)
                    {
                        httpClient = new HttpClient();
                    }
                }
                return httpClient;
            }
            set
            {
                httpClient = value;
            }
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
        public override async Task<Message> QueryAsync(Message request, CancellationToken cancel = default)
        {
            return UsePrimaryUrl ? await QueryWithGoogleAsync(request, cancel) : await QueryWithCloudFlareAsync(request, cancel);
        }

        // use Google's DNS servers
        public async Task<Message> QueryWithGoogleAsync(Message messageRequest, CancellationToken cancel)
        {
            using var timeout = new CancellationTokenSource(Timeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeout.Token);

            List<string> urlStructures = [];

            foreach (var question in messageRequest.Questions)
            {
                urlStructures.Add(
                    $"{primaryUrl}?name={question.Name}" +
                    $"&type={question.Type}" +
                    $"&ct={DnsWireFormat}" +
                    $"&cd={messageRequest.CD}" +
                    $"&do={messageRequest.DO}");
            }

            if (urlStructures.Count == 0)
            {
                return new Message();
            }

            HttpResponseMessage? httpResponse = null;

            foreach (var url in urlStructures)
            {
                httpResponse = await HttpClient.GetAsync(url, cts.Token).ConfigureAwait(false);

                httpResponse.EnsureSuccessStatusCode();

                if (httpResponse.IsSuccessStatusCode)
                {
                    break;
                }
            }

            if (httpResponse is null)
            {
                throw new HttpRequestException("No response received.");
            }

            var contentType = httpResponse.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentType) || contentType != DnsWireFormat)
            {
                throw new HttpRequestException($"Expected content-type '{DnsWireFormat}' not '{contentType}'.");
            }

            await using var body = await httpResponse.Content.ReadAsStreamAsync(cancel).ConfigureAwait(false);

            var dnsResponse = (Message)new Message().Read(body);

            if (ThrowResponseError && dnsResponse.Status != MessageStatus.NoError)
            {
                throw new IOException($"DNS error '{dnsResponse.Status}'.");
            }

            return dnsResponse;
        }

        // use Cloudflare's DNS servers
        public async Task<Message> QueryWithCloudFlareAsync(Message messageRequest, CancellationToken cancel = default)
        {
            using var timeout = new CancellationTokenSource(Timeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeout.Token);

            byte[] requestBytes;
            using (var ms = new MemoryStream())
            {
                messageRequest.Write(ms);
                requestBytes = ms.ToArray();
            }

            var base64 = Convert.ToBase64String(requestBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            var url = $"{alternateUrl}?dns={base64}";

            var httpResponse = await HttpClient.GetAsync(url, cts.Token).ConfigureAwait(false);

            var contentType = httpResponse.Content.Headers.ContentType?.MediaType;

            if (contentType != DnsWireFormat)
            {
                throw new HttpRequestException($"Expected content-type '{DnsWireFormat}' not '{contentType}'.");
            }

            await using var body = await httpResponse.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);

            var dnsResponse = (Message)new Message().Read(body);

            if (ThrowResponseError && dnsResponse.Status != MessageStatus.NoError)
            {
                throw new IOException($"DNS error '{dnsResponse.Status}'.");
            }

            return dnsResponse;
        }
    }
}
