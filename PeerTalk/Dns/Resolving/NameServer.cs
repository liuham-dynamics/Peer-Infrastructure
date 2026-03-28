using PeerTalk.Dns.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeerTalk.Dns.Resolving
{
    /// <summary>
    ///   Anwsers questions from the local <see cref="Catalog"/>.
    /// </summary>
    public class NameServer : IResolver
    {
        /// <summary>
        ///   Information about some portion of the DNS database.
        /// </summary>
        /// <value>
        ///   A subset of the DNS database. Typically (1) one or more zones or (2) a cache of received
        ///   responses.
        /// </value>
        public Catalog Catalog { get; set; } = [];

        /// <summary>
        ///   Determines how multiple questions are answered.
        /// </summary>
        /// <value>
        ///   <b>false</b> to answer <b>any</b> of the questions.
        ///   <b>false</b> to answer <b>all</b> of the questions.
        ///   The default is <b>false</b>.
        /// </value>
        /// <remarks>
        ///   Standard DNS specifies that only one of the questions need to be answered.
        ///   Multicast DNS specifies that all the questions need to be answered.
        /// </remarks>
        public bool AnswerAllQuestions { get; set; }

        /// <inheritdoc />
        public async Task<Message> ResolveAsync(Message request, CancellationToken cancel = default)
        {
            var response = request.CreateResponse();

            foreach (var question in request.Questions)
            {
                await ResolveAsync(question, response, cancel);
                if (response.Answers.Count > 0 && !AnswerAllQuestions)
                {
                    break;
                }
            }

            if (response.Answers.Count > 0)
            {
                response.Status = MessageStatus.NoError;
            }

            // Remove duplicate records.
            if (response.Answers.Count > 1)
            {
                response.Answers = [.. response.Answers.Distinct()];
            }
            if (response.AuthorityRecords.Count > 1)
            {
                response.AuthorityRecords = [.. response.AuthorityRecords.Distinct()];
            }

            // Remove additional records that are also answers.
            if (response.AdditionalRecords.Count > 0)
            {
                response.AdditionalRecords = [.. response.AdditionalRecords.Where(a => !response.Answers.Contains(a))];
            }

            return await AddSecurityExtensionsAsync(request, response);
        }

        /// <summary>
        ///   Get an answer to a question.
        /// </summary>
        /// <param name="question">
        ///   The question to answer.
        /// </param>
        /// <param name="response">
        ///   Where the answers are added.  If <b>null</b>, then a new <see cref="Message"/> is
        ///   created.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's value is
        ///   a <see cref="Message"/> response to the <paramref name="question"/>.
        /// </returns>
        /// <remarks>
        ///   If the question's domain does not exist, then the closest authority
        ///   (<see cref="SOARecord"/>) is added to the <see cref="Message.AuthorityRecords"/>.
        /// </remarks>
        public async Task<Message> ResolveAsync(Question question, Message? response = null, CancellationToken cancel = default)
        {
            response ??= new Message
            {
                QR = true
            };

            // Get answer and details of the domain.
            bool found = await FindAnswerAsync(question, response, cancel);
            var soa = FindAuthority(question.Name);
            if (!found && response.Status == MessageStatus.NoError)
            {
                response.Status = MessageStatus.NameError;
            }

            // Add the NS records for the answered domain into the
            // the authority section.
            if (found && soa is not null)
            {
                var res = new Message();
                var q = new Question { Name = soa.Name, Class = soa.Class, Type = DnsType.NS };
                await FindAnswerAsync(q, res, cancel);
                response.AuthorityRecords.AddRange(res.Answers.OfType<NSRecord>());
            }

            // If a name error, then add the domain authority.
            if (response.Status == MessageStatus.NameError)
            {
                if (soa is not null)
                {
                    response.AuthorityRecords.Add(soa);
                }
            }

            // Add additonal records.
           await AddAdditionalRecords(response);

            return response;
        }

        /// <summary>
        ///   Find an answer to the <see cref="Question"/>.
        /// </summary>
        /// <param name="question">
        ///   The question to answer.
        /// </param>
        /// <param name="response">
        ///   Where the answers are added.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation.  The task's value
        ///   is <b>true</b> if the resolver added an answer.
        /// </returns>
        /// <remarks>
        ///   Derived classes must implement this method.
        /// </remarks>
        protected async Task<bool> FindAnswerAsync(Question question, Message response, CancellationToken cancel)
        {
            if (!Catalog.TryGetValue(question.Name, out var node) || node is null)
            {
                return false;
            }

            // https://tools.ietf.org/html/rfc1034#section-3.7.1
            response.AA |= node.Authoritative && question.Class != DnsClass.ANY;

            //  Find the resources that match the question.
            var resources = node.Resources
                .Where(r => (question.Class == DnsClass.ANY || r.Class == question.Class)
                && (question.Type == DnsType.ANY || r.Type == question.Type)
                && (node.Authoritative || !r.IsExpired(question.CreationTime)))
                .ToArray();
            if (resources.Length > 0)
            {
                response.Answers.AddRange(resources);
                return true;
            }

            // If node is alias (CNAME), then find answers for the alias' target.
            // The CNAME is added to the answers.
            var cname = node.Resources.OfType<CNAMERecord>().FirstOrDefault();
            if (cname is not null)
            {
                response.Answers.Add(cname);
                question = question.Clone<Question>();
                question.Name = cname.Target;
                return await FindAnswerAsync(question, response, cancel);
            }

            // Nothing more can be done.
            return false;
        }

        private SOARecord? FindAuthority(DomainName domainName)
        {
            var name = domainName;
            while (name is not null)
            {
                if (Catalog.TryGetValue(name, out var node) && node is not null)
                {
                    var soa = node.Resources.OfType<SOARecord>().FirstOrDefault();
                    if (soa is not null)
                    {
                        return soa;
                    }
                }

                name = name.Parent();
            }

            return default;
        }

        private async Task AddAdditionalRecords(Message response)
        {
            var extras = new Message();
            var resources = response.Answers
                .Concat(response.AdditionalRecords)
                .Concat(response.AuthorityRecords);
            var question = new Question();
            foreach (var resource in resources)
            {
                switch (resource.Type)
                {
                    case DnsType.A:
                        question.Class = resource.Class;
                        question.Name = resource.Name;
                        question.Type = DnsType.AAAA;
                        await FindAnswerAsync(question, extras, default);
                        break;

                    case DnsType.AAAA:
                        question.Class = resource.Class;
                        question.Name = resource.Name;
                        question.Type = DnsType.A;
                        await FindAnswerAsync(question, extras, default);
                        break;

                    case DnsType.NS:
                        await FindAddresses(((NSRecord)resource).Authority, resource.Class, extras);
                        break;

                    case DnsType.PTR:
                        var ptr = (PTRRecord)resource;

                        question.Class = resource.Class;
                        question.Name = ptr.DomainName;
                        question.Type = DnsType.ANY;
                        await FindAnswerAsync(question, extras, default);
                        break;

                    case DnsType.SOA:
                        await FindAddresses(((SOARecord)resource).PrimaryName, resource.Class, extras);
                        break;

                    case DnsType.SRV:
                        question.Class = resource.Class;
                        question.Name = resource.Name;
                        question.Type = DnsType.TXT;
                        await FindAnswerAsync(question, extras, default);

                        await FindAddresses(((SRVRecord)resource).Target, resource.Class, extras);
                        break;

                    default:
                        break;
                }
            }

            // Add extras with no duplication.
            extras.Answers = [.. extras.Answers
                                .Where(a => !response.Answers.Contains(a) && !response.AdditionalRecords.Contains(a))
                                .Distinct()];
            response.AdditionalRecords.AddRange(extras.Answers);

            // Add additionals for any extras.
            if (extras.Answers.Count > 0)
            {
                await AddAdditionalRecords(response);
            }
        }

        private async Task FindAddresses(DomainName name, DnsClass klass, Message response)
        {
            var question = new Question
            {
                Name = name,
                Class = klass,
                Type = DnsType.A
            };

            await FindAnswerAsync(question, response, default);
            question.Type = DnsType.AAAA;
            await FindAnswerAsync(question, response, default);
        }

        /// <summary>
        /// Add Security Extensions
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<Message> AddSecurityExtensionsAsync(Message request, Message response)
        {
            // If requestor doesn't do DNSSEC, then nothing more to do.
            if (!request.DO)
            {
                return response;
            }
            response.DO = true;

            await AddSecurityResourcesAsync(response.Answers);
            await AddSecurityResourcesAsync(response.AuthorityRecords);
            await AddSecurityResourcesAsync(response.AdditionalRecords);

            return response;
        }

        /// <summary>
        ///   Add the DNSSEC resources for the resource record set.
        /// </summary>
        /// <param name="rrset">
        ///   The set of resource records.
        /// </param>
        /// <remarks>
        ///   Add the signature records (RRSIG) for each resource in the set.
        /// </remarks>
        private async Task AddSecurityResourcesAsync(List<ResourceRecord> rrset)
        {
            // Get the signature _memberNames and types that are needed.  Then
            // add the corresponding RRSIG records to the rrset.
            var neededSignatures = rrset
                .Where(r => r.CanonicalName != string.Empty) // ignore pseudo records as
                .GroupBy(r => new { r.CanonicalName, r.Type, r.Class })
                .Select(g => g.First());
            foreach (var need in neededSignatures)
            {
                var signatures = new Message();
                var question = new Question { Name = need.Name, Class = need.Class, Type = DnsType.RRSIG };
                if (!await FindAnswerAsync(question, signatures, CancellationToken.None))
                {
                    continue;
                }
                rrset.AddRange(signatures.Answers
                     .OfType<RRSIGRecord>()
                     .Where(r => r.TypeCovered == need.Type)
                        );
            }
        }
    }
}
