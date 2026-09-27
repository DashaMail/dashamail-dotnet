using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// One-off transactional emails: send, status, log, stats.
    /// Requires a verified sending domain — see Account.AddDomainAsync().
    /// https://dashamail.ru/api/transactional/
    /// </summary>
    public class Transactional : BaseResource
    {
        public Transactional(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>
        /// POST /transactional/messages
        /// </summary>
        /// <param name="to">A single address (string), or a list of addresses/objects.</param>
        /// <param name="fromEmail">Verified sending address.</param>
        /// <param name="message">HTML body.</param>
        /// <param name="parameters">from_name, subject, plain_text, message_id, cc, bcc, headers, attachments, inline, delivery_time, domain, stat_domain...</param>
        public Task<DashaMailResponse> SendAsync(object to, string fromEmail, string message, IDictionary<string, object> parameters = null)
        {
            var body = With(With(With(parameters, "to", to), "from_email", fromEmail), "message", message);
            return Client.RequestAsync("POST", "/transactional/messages", null, body);
        }

        /// <summary>GET /transactional/messages/{transactionId} — delivery status of one message.</summary>
        public Task<DashaMailResponse> CheckAsync(string transactionId)
        {
            return Client.RequestAsync("GET", "/transactional/messages/" + Uri.EscapeDataString(transactionId));
        }

        /// <summary>GET /transactional/log</summary>
        public Task<DashaMailResponse> LogAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/transactional/log", parameters);
        }

        /// <summary>GET /transactional/stats</summary>
        public Task<DashaMailResponse> StatsAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/transactional/stats", parameters);
        }
    }
}
