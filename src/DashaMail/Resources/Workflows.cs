using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Visual-builder automation scenarios.
    /// https://dashamail.ru/api/automations/#workflows
    /// </summary>
    public class Workflows : BaseResource
    {
        public Workflows(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /workflows</summary>
        public Task<DashaMailResponse> AllAsync()
        {
            return Client.RequestAsync("GET", "/workflows");
        }

        /// <summary>GET /workflows/{workflowId}</summary>
        public Task<DashaMailResponse> GetAsync(object workflowId)
        {
            return Client.RequestAsync("GET", "/workflows/" + workflowId);
        }

        /// <summary>DELETE /workflows/{workflowId}</summary>
        public Task<DashaMailResponse> DeleteAsync(object workflowId)
        {
            return Client.RequestAsync("DELETE", "/workflows/" + workflowId);
        }

        /// <summary>POST /workflows/{workflowId}/copy</summary>
        public Task<DashaMailResponse> CopyAsync(object workflowId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/workflows/" + workflowId + "/copy", null, parameters);
        }
    }
}
