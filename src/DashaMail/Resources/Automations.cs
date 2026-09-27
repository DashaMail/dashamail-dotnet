using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Event-triggered emails (subscribe, add, open, click, field change...).
    /// https://dashamail.ru/api/automations/
    /// </summary>
    public class Automations : BaseResource
    {
        public Automations(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /automations/events — reference of available trigger events.</summary>
        public Task<DashaMailResponse> EventsAsync()
        {
            return Client.RequestAsync("GET", "/automations/events");
        }

        /// <summary>GET /automations</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/automations", parameters);
        }

        /// <summary>POST /automations — requires list_id, subject, from_email, from_name; see the API docs for the rest.</summary>
        public Task<DashaMailResponse> CreateAsync(IDictionary<string, object> parameters)
        {
            return Client.RequestAsync("POST", "/automations", null, parameters);
        }

        /// <summary>PUT /automations/{campaignId}</summary>
        public Task<DashaMailResponse> UpdateAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/automations/" + campaignId, null, parameters);
        }

        /// <summary>DELETE /automations/{campaignId}</summary>
        public Task<DashaMailResponse> DeleteAsync(object campaignId)
        {
            return Client.RequestAsync("DELETE", "/automations/" + campaignId);
        }

        /// <summary>POST /automations/{campaignId}/trigger — force-run for one subscriber (fails with code 37 before moderation).</summary>
        public Task<DashaMailResponse> TriggerAsync(object campaignId, string email, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/automations/" + campaignId + "/trigger", null, With(parameters, "email", email));
        }
    }
}
