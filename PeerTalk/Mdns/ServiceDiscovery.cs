using PeerTalk.Dns;
using PeerTalk.Dns.Records;
using PeerTalk.Dns.Resolving;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Mdns
{
    /// <summary>
    ///   DNS based Service Discovery is a way of using standard DNS programming interfaces, servers,
    ///   and packet formats to browse the network for services.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc6763">RFC 6763 DNS-Based Service Discovery</seealso>
    public class ServiceDiscovery : IDisposable
    {
        private static readonly DomainName _memberLocalDomain = new("local");
        private static readonly DomainName _memberSubName = new("_sub");

        private List<ServiceProfile> _memberProfiles = [];
        private readonly bool _memberOwnsMdns;
        


        /// <summary>
        ///   The service discovery service name.
        /// </summary>
        /// <value>
        ///   The service name used to enumerate other services.
        /// </value>
        public static readonly DomainName ServiceName = new("_services._dns-sd._udp.local");

        /// <summary>
        ///   Gets the multicasting service.
        /// </summary>
        /// <value>
        ///   Is used to send and recieve multicast <see cref="Message">DNS messages</see>.
        /// </value>
        public MulticastService Mdns { get; private set; }

        /// <summary>
        ///   Add the additional records into the answers.
        /// </summary>
        /// <value>
        ///   Defaults to <b>false</b>.
        /// </value>
        /// <remarks>
        ///   Some malformed systems, such as js-ipfs and go-ipfs, only examine
        ///   the <see cref="Message.Answers"/> and not the <see cref="Message.AdditionalRecords"/>.
        ///   Setting this to <b>true</b>, will move the additional records
        ///   into the answers.
        ///   <para>
        ///   This never done for DNS-SD answers.
        ///   </para>
        /// </remarks>
        public bool AnswersContainsAdditionalRecords { get; set; }

        /// <summary>
        ///   Gets the name server.
        /// </summary>
        /// <value>
        ///   Is used to answer questions.
        /// </value>
        public NameServer NameServer { get; } = new NameServer
        {
            Catalog = [],
            AnswerAllQuestions = true
        };

        /// <summary>
        ///   Creates a new instance of the <see cref="ServiceDiscovery"/> class.
        /// </summary>
        public ServiceDiscovery() : this(new MulticastService())
        {
            _memberOwnsMdns = true;
            Mdns.Start();  // Auto start.
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="ServiceDiscovery"/> class with
        ///   the specified <see cref="MulticastService"/>.
        /// </summary>
        /// <param name="mdns">
        ///   The underlaying <see cref="MulticastService"/> to use.
        /// </param>
        public ServiceDiscovery(MulticastService mdns)
        {
            this.Mdns = mdns;
            mdns.QueryReceived += OnQuery;
            mdns.AnswerReceived += OnAnswer;
        }

        /// <summary>
        ///   Raised when a DNS-SD response is received.
        /// </summary>
        /// <value>
        ///   Contains the service name.
        /// </value>
        /// <remarks>
        ///   <b>ServiceDiscovery</b> passively monitors the network for any answers
        ///   to a DNS-SD query. When an anwser is received this event is raised.
        ///   <para>
        ///   Use <see cref="QueryAllServices"/> to initiate a DNS-SD question.
        ///   </para>
        /// </remarks>
        public event EventHandler<DomainName> ServiceDiscovered;

        /// <summary>
        ///   Raised when a servive instance is discovered.
        /// </summary>
        /// <value>
        ///   Contains the service instance name.
        /// </value>
        /// <remarks>
        ///   <b>ServiceDiscovery</b> passively monitors the network for any answers.
        ///   When an answer containing a PTR to a service instance is received
        ///   this event is raised.
        /// </remarks>
        public event EventHandler<ServiceInstanceDiscoveryEventArgs> ServiceInstanceDiscovered;

        /// <summary>
        ///   Raised when a servive instance is shutting down.
        /// </summary>
        /// <value>
        ///   Contains the service instance name.
        /// </value>
        /// <remarks>
        ///   <b>ServiceDiscovery</b> passively monitors the network for any answers.
        ///   When an answer containing a PTR to a service instance with a
        ///   TTL of zero is received this event is raised.
        /// </remarks>
        public event EventHandler<ServiceInstanceShutdownEventArgs> ServiceInstanceShutdown;

        /// <summary>
        ///    Asks other MDNS services to send their service names.
        /// </summary>
        /// <remarks>
        ///   When an answer is received the <see cref="ServiceDiscovered"/> event is raised.
        /// </remarks>
        public Task QueryAllServices()
        {
          return Mdns.SendQuery(ServiceName, type: DnsType.PTR);
        }

        /// <summary>
        ///    Asks other MDNS services to send their service names;
        ///    accepts unicast and/or broadcast answers.
        /// </summary>
        /// <remarks>
        ///   When an answer is received the <see cref="ServiceDiscovered"/> event is raised.
        /// </remarks>
        public Task QueryUnicastAllServices()
        {
          return Mdns.SendUnicastQuery(ServiceName, type: DnsType.PTR);
        }

        /// <summary>
        ///   Asks instances of the specified service to send details.
        /// </summary>
        /// <param name="service">
        ///   The service name to query. Typically of the form "_<i>service</i>._tcp".
        /// </param>
        /// <remarks>
        ///   When an answer is received the <see cref="ServiceInstanceDiscovered"/> event is raised.
        /// </remarks>
        /// <seealso cref="ServiceProfile.ServiceName"/>
        public Task QueryServiceInstances(DomainName service)
        {
           return Mdns.SendQuery(DomainName.Join(service, _memberLocalDomain), type: DnsType.PTR);
        }

        /// <summary>
        ///   Asks instances of the specified service with the subtype to send details.
        /// </summary>
        /// <param name="service">
        ///   The service name to query. Typically of the form "_<i>service</i>._tcp".
        /// </param>
        /// <param name="subtype">
        ///   The feature that is needed.
        /// </param>
        /// <remarks>
        ///   When an answer is received the <see cref="ServiceInstanceDiscovered"/> event is raised.
        /// </remarks>
        /// <seealso cref="ServiceProfile.ServiceName"/>
        public Task QueryServiceInstances(DomainName service, string subtype)
        {
            var name = DomainName.Join(new DomainName(subtype),
                                       _memberSubName, service, _memberLocalDomain);
          return Mdns.SendQuery(name, type: DnsType.PTR);
        }

        /// <summary>
        ///   Asks instances of the specified service to send details.
        ///   accepts unicast and/or broadcast answers.
        /// </summary>
        /// <param name="service">
        ///   The service name to query. Typically of the form "_<i>service</i>._tcp".
        /// </param>
        /// <remarks>
        ///   When an answer is received the <see cref="ServiceInstanceDiscovered"/> event is raised.
        /// </remarks>
        /// <seealso cref="ServiceProfile.ServiceName"/>
        public Task QueryUnicastServiceInstances(DomainName service)
        {
           return Mdns.SendUnicastQuery(DomainName.Join(service, _memberLocalDomain), type: DnsType.PTR);
        }

        /// <summary>
        ///   Advertise a service profile.
        /// </summary>
        /// <param name="service">
        ///   The service profile.
        /// </param>
        /// <remarks>
        ///   Any queries for the service or service instance will be answered with
        ///   information from the profile.
        ///   <para>
        ///   Besides adding the profile's resource records to the <see cref="Catalog"/> PTR records are
        ///   created to support DNS-SD and reverse address mapping (DNS address lookup).
        ///   </para>
        /// </remarks>
        public void Advertise(ServiceProfile service)
        {
            _memberProfiles.Add(service);

            var catalog = NameServer.Catalog;
            catalog.Add(new PTRRecord { Name = ServiceName, DomainName = service.QualifiedServiceName }, authoritative: true);
            catalog.Add(new PTRRecord { Name = service.QualifiedServiceName, DomainName = service.FullyQualifiedName }, authoritative: true);

            foreach (var subtype in service.Subtypes)
            {
                var ptr = new PTRRecord
                {
                    Name = DomainName.Join(new DomainName(subtype),
                                            _memberSubName, service.QualifiedServiceName),
                    DomainName = service.FullyQualifiedName
                };
                catalog.Add(ptr, authoritative: true);
            }

            foreach (var r in service.Resources)
            {
                catalog.Add(r, authoritative: true);
            }

            catalog.IncludeReverseLookupRecords();
        }

        /// <summary>
        ///    Sends an unsolicited MDNS response describing the
        ///    service profile.
        /// </summary>
        /// <param name="profile">
        ///   The profile to describe.
        /// </param>
        /// <remarks>
        ///   Sends a MDNS response <see cref="Message"/> containing the pointer
        ///   and resource records of the <paramref name="profile"/>.
        ///   <para>
        ///   To provide increased robustness against packet loss,
        ///   two unsolicited responses are sent one second apart.
        ///   </para>
        /// </remarks>
        public async Task Announce(ServiceProfile profile)
        {
            var message = new Message { QR = true };

            // Add the shared records.
            var ptrRecord = new PTRRecord { Name = profile.QualifiedServiceName, DomainName = profile.FullyQualifiedName };
            message.Answers.Add(ptrRecord);

            // Add the resource records.
            profile.Resources.ForEach((resource) => message.Answers.Add(resource));

            await Mdns.SendAnswer(message, checkDuplicate: false).ConfigureAwait(false);
            await Task.Delay(1360);
            await Mdns.SendAnswer(message, checkDuplicate: false).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a goodbye message for the provided
        /// profile and removes its pointer from the name sever.
        /// </summary>
        /// <param name="profile">The profile to send a goodbye message for.</param>
        public async Task Unadvertise(ServiceProfile profile)
        {
            var message = new Message { QR = true };
            var ptrRecord = new PTRRecord
            {
                Name = profile.QualifiedServiceName,
                DomainName = profile.FullyQualifiedName,
                TTL = TimeSpan.Zero
            };

            message.Answers.Add(ptrRecord);
            profile.Resources.ForEach((resource) =>
            {
                resource.TTL = TimeSpan.Zero;
                message.AdditionalRecords.Add(resource);
            });

           await Mdns.SendAnswer(message).ConfigureAwait(false); 

            NameServer.Catalog.TryRemove(profile.QualifiedServiceName, out var _);
        }

        /// <summary>
        /// Sends a goodbye message for each anounced service.
        /// </summary>
        public async Task Unadvertise()
        {
            _memberProfiles.ForEach(async profile => await Unadvertise(profile).ConfigureAwait(false));
        }

        private void OnAnswer(object? sender, MessageEventArgs e)
        {
            var msg = e.Message;
              
            // Any DNS-SD answers?

            var sd = msg.Answers.OfType<PTRRecord>()
                        .Where(ptr => ptr.Name.IsSubdomainOf(_memberLocalDomain));
            foreach (var ptr in sd)
            {
                if (ptr.Name == ServiceName)
                {
                    ServiceDiscovered?.Invoke(this, ptr.DomainName);
                }
                else if (ptr.TTL == TimeSpan.Zero)
                {
                    var args = new ServiceInstanceShutdownEventArgs
                    {
                        ServiceInstanceName = ptr.DomainName,
                        Message = msg
                    };
                    ServiceInstanceShutdown?.Invoke(this, args);
                }
                else
                {
                    var args = new ServiceInstanceDiscoveryEventArgs
                    {
                        ServiceInstanceName = ptr.DomainName,
                        Message = msg
                    };
                    ServiceInstanceDiscovered?.Invoke(this, args);
                }
            }
        }

        private async void OnQuery(object? sender, MessageEventArgs e)
        {
            var request = e.Message;
             
            // Determine if this query is requesting a unicast response
            // and normalise the Class.
            var QU = false; // unicast query response?
            foreach (var r in request.Questions)
            {
                if (((ushort)r.Class & 0x8000) != 0)
                {
                    QU = true;
                    r.Class = (DnsClass)((ushort)r.Class & 0x7fff);
                }
            }

            var response = NameServer.ResolveAsync(request).Result;

            if (response.Status != MessageStatus.NoError)
            {
                return;
            }

            // Many bonjour browsers don't like DNS-SD response
            // with additional records.
            if (response.Answers.Any(a => a.Name == ServiceName))
            {
                response.AdditionalRecords.Clear();
            }

            if (AnswersContainsAdditionalRecords)
            {
                response.Answers.AddRange(response.AdditionalRecords);
                response.AdditionalRecords.Clear();
            }

            if (!response.Answers.Any(a => a.Name == ServiceName))
            {
            }

            if (QU)
            {
              await  Mdns.SendAnswer(response, e).ConfigureAwait(false);
            }
            else
            {
              await  Mdns.SendAnswer(response, e).ConfigureAwait(false);
            }
             
        }

        #region IDisposable Support

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Mdns is not null)
                {
                    Mdns.QueryReceived -= OnQuery;
                    Mdns.AnswerReceived -= OnAnswer;
                    if (_memberOwnsMdns)
                    {
                        Mdns.Dispose();
                    }
                    Mdns = null;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}
