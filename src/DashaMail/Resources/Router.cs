using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Inbound mail processing: receiving domains, routing rules, stored
    /// messages, webhook delivery log.
    /// https://dashamail.ru/api/router/
    /// </summary>
    public class Router : BaseResource
    {
        public Router(HttpTransport client)
            : base(client)
        {
        }

        // -- Domains ------------------------------------------------------------

        /// <summary>GET /router/domains</summary>
        public Task<DashaMailResponse> DomainsAsync()
        {
            return Client.RequestAsync("GET", "/router/domains");
        }

        /// <summary>POST /router/domains — connect your own inbound domain (needs an MX record).</summary>
        public Task<DashaMailResponse> CreateDomainAsync(string domain)
        {
            return Client.RequestAsync("POST", "/router/domains", null, new Dictionary<string, object> { { "domain", domain } });
        }

        /// <summary>POST /router/domains/{domainId}/verify — check the MX record.</summary>
        public Task<DashaMailResponse> VerifyDomainAsync(object domainId)
        {
            return Client.RequestAsync("POST", "/router/domains/" + domainId + "/verify");
        }

        /// <summary>DELETE /router/domains/{domainId}</summary>
        public Task<DashaMailResponse> DeleteDomainAsync(object domainId)
        {
            return Client.RequestAsync("DELETE", "/router/domains/" + domainId);
        }

        /// <summary>GET /router/domains/mx — DNS records to configure.</summary>
        public Task<DashaMailResponse> MxInstructionsAsync()
        {
            return Client.RequestAsync("GET", "/router/domains/mx");
        }

        // -- Routes ---------------------------------------------------------------

        /// <summary>GET /router/routes</summary>
        public Task<DashaMailResponse> RoutesAsync()
        {
            return Client.RequestAsync("GET", "/router/routes");
        }

        /// <summary>GET /router/routes/{routeId}</summary>
        public Task<DashaMailResponse> GetRouteAsync(object routeId)
        {
            return Client.RequestAsync("GET", "/router/routes/" + routeId);
        }

        /// <summary>POST /router/routes — actions is 1..5 action dictionaries, each with a "type" key (webhook, store, forward, stop).</summary>
        public Task<DashaMailResponse> CreateRouteAsync(IList<object> actions, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/router/routes", null, With(parameters, "actions", actions));
        }

        /// <summary>PUT /router/routes/{routeId}</summary>
        public Task<DashaMailResponse> UpdateRouteAsync(object routeId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/router/routes/" + routeId, null, parameters);
        }

        /// <summary>DELETE /router/routes/{routeId}</summary>
        public Task<DashaMailResponse> DeleteRouteAsync(object routeId)
        {
            return Client.RequestAsync("DELETE", "/router/routes/" + routeId);
        }

        /// <summary>POST /router/routes/{routeId}/rekey — reissue the webhook signing key.</summary>
        public Task<DashaMailResponse> RekeyRouteAsync(object routeId)
        {
            return Client.RequestAsync("POST", "/router/routes/" + routeId + "/rekey");
        }

        /// <summary>POST /router/routes/reorder — order is a list of route ids in the desired priority order.</summary>
        public Task<DashaMailResponse> ReorderRoutesAsync(IList<object> order)
        {
            return Client.RequestAsync("POST", "/router/routes/reorder", null, new Dictionary<string, object> { { "order", order } });
        }

        // -- Stored messages ------------------------------------------------------

        /// <summary>GET /router/messages</summary>
        public Task<DashaMailResponse> MessagesAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/router/messages", parameters);
        }

        /// <summary>GET /router/messages/{messageId}</summary>
        public Task<DashaMailResponse> GetMessageAsync(object messageId)
        {
            return Client.RequestAsync("GET", "/router/messages/" + messageId);
        }

        /// <summary>DELETE /router/messages/{messageId}</summary>
        public Task<DashaMailResponse> DeleteMessageAsync(object messageId)
        {
            return Client.RequestAsync("DELETE", "/router/messages/" + messageId);
        }

        /// <summary>GET /router/messages/{messageId}/attachments/{attachmentId}</summary>
        public Task<DashaMailResponse> GetMessageAttachmentAsync(object messageId, object attachmentId)
        {
            return Client.RequestAsync("GET", "/router/messages/" + messageId + "/attachments/" + attachmentId);
        }

        // -- Webhook delivery log ---------------------------------------------------

        /// <summary>GET /router/deliveries</summary>
        public Task<DashaMailResponse> DeliveriesAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/router/deliveries", parameters);
        }

        /// <summary>GET /router/deliveries/{deliveryId}</summary>
        public Task<DashaMailResponse> GetDeliveryAsync(object deliveryId)
        {
            return Client.RequestAsync("GET", "/router/deliveries/" + deliveryId);
        }

        // -- Settings -----------------------------------------------------------------

        /// <summary>GET /router/settings</summary>
        public Task<DashaMailResponse> SettingsAsync()
        {
            return Client.RequestAsync("GET", "/router/settings");
        }

        /// <summary>PUT /router/settings — pass_autoreply, pass_list_mail</summary>
        public Task<DashaMailResponse> UpdateSettingsAsync(IDictionary<string, object> parameters)
        {
            return Client.RequestAsync("PUT", "/router/settings", null, parameters);
        }
    }
}
