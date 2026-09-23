using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Saved markup: legacy HTML-template store (/templates) plus saved campaigns
    /// in TEMPLATE status (/templates/saved), which is where the account UI keeps them.
    /// https://dashamail.ru/api/templates/
    /// </summary>
    public class Templates : BaseResource
    {
        public Templates(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /templates — legacy HTML templates.</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/templates", parameters);
        }

        /// <summary>GET /templates/{id}</summary>
        public Task<DashaMailResponse> GetAsync(object id)
        {
            return Client.RequestAsync("GET", "/templates/" + id);
        }

        /// <summary>POST /templates — template is HTML markup, body is the plain-text markup.</summary>
        public Task<DashaMailResponse> CreateAsync(string name, string template, string body, IDictionary<string, object> parameters = null)
        {
            var payload = With(With(With(parameters, "name", name), "template", template), "body", body);
            return Client.RequestAsync("POST", "/templates", null, payload);
        }

        /// <summary>PUT /templates/{id}</summary>
        public Task<DashaMailResponse> UpdateAsync(object id, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/templates/" + id, null, parameters);
        }

        /// <summary>DELETE /templates/{id}</summary>
        public Task<DashaMailResponse> DeleteAsync(object id)
        {
            return Client.RequestAsync("DELETE", "/templates/" + id);
        }

        /// <summary>GET /templates/saved — campaigns saved as reusable templates (status TEMPLATE).</summary>
        public Task<DashaMailResponse> SavedAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/templates/saved", parameters);
        }
    }
}
