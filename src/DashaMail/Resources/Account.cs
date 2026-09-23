using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Account balance and limits, confirmed senders, sending domains, webhooks.
    /// https://dashamail.ru/api/account/
    /// </summary>
    public class Account : BaseResource
    {
        public Account(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /account/balance</summary>
        public Task<DashaMailResponse> BalanceAsync()
        {
            return Client.RequestAsync("GET", "/account/balance");
        }

        /// <summary>GET /account/senders — confirmed From: addresses.</summary>
        public Task<DashaMailResponse> SendersAsync()
        {
            return Client.RequestAsync("GET", "/account/senders");
        }

        /// <summary>POST /account/senders/confirm — confirm a sender address with the code emailed to it.</summary>
        public Task<DashaMailResponse> ConfirmSenderAsync(string email, string code)
        {
            var body = new Dictionary<string, object> { { "email", email }, { "code", code } };
            return Client.RequestAsync("POST", "/account/senders/confirm", null, body);
        }

        /// <summary>GET /account/domains — sending domains.</summary>
        public Task<DashaMailResponse> DomainsAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/account/domains", parameters);
        }

        /// <summary>POST /account/domains — add a sending domain.</summary>
        public Task<DashaMailResponse> AddDomainAsync(string domain, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/account/domains", null, With(parameters, "domain", domain));
        }

        /// <summary>GET /account/domains/check — check DNS (DKIM/SPF) validity of sending domains.</summary>
        public Task<DashaMailResponse> CheckDomainsAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/account/domains/check", parameters);
        }

        /// <summary>DELETE /account/domains/{domain}</summary>
        public Task<DashaMailResponse> DeleteDomainAsync(string domain, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("DELETE", "/account/domains/" + Uri.EscapeDataString(domain), null, parameters);
        }

        /// <summary>GET /account/webhooks — bulk-campaign webhooks.</summary>
        public Task<DashaMailResponse> WebhooksAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/account/webhooks", parameters);
        }

        /// <summary>POST /account/webhooks — event is one of: open, click, hard, spam, unsub, subscribe, confirm.</summary>
        public Task<DashaMailResponse> AddWebhookAsync(string eventName, string url, IDictionary<string, object> parameters = null)
        {
            var body = With(With(parameters, "event", eventName), "url", url);
            return Client.RequestAsync("POST", "/account/webhooks", null, body);
        }

        /// <summary>DELETE /account/webhooks/{eventName}</summary>
        public Task<DashaMailResponse> DeleteWebhookAsync(string eventName)
        {
            return Client.RequestAsync("DELETE", "/account/webhooks/" + Uri.EscapeDataString(eventName));
        }

        /// <summary>GET /account/webhooks/transactional</summary>
        public Task<DashaMailResponse> TransactionalWebhooksAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/account/webhooks/transactional", parameters);
        }

        /// <summary>POST /account/webhooks/transactional — event is one of: send, delivered, dropped, open, click, hard, spam, unsub.</summary>
        public Task<DashaMailResponse> AddTransactionalWebhookAsync(string eventName, string url, IDictionary<string, object> parameters = null)
        {
            var body = With(With(parameters, "event", eventName), "url", url);
            return Client.RequestAsync("POST", "/account/webhooks/transactional", null, body);
        }

        /// <summary>DELETE /account/webhooks/transactional/{eventName}</summary>
        public Task<DashaMailResponse> DeleteTransactionalWebhookAsync(string eventName)
        {
            return Client.RequestAsync("DELETE", "/account/webhooks/transactional/" + Uri.EscapeDataString(eventName));
        }
    }
}
