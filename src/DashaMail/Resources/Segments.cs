using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Saved subscriber segments: condition sets, field/operator reference, size counting.
    /// https://dashamail.ru/api/segments/
    /// </summary>
    public class Segments : BaseResource
    {
        public Segments(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /segments</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/segments", parameters);
        }

        /// <summary>GET /segments/{segmentId}</summary>
        public Task<DashaMailResponse> GetAsync(object segmentId)
        {
            return Client.RequestAsync("GET", "/segments/" + segmentId);
        }

        /// <summary>POST /segments — esegment is a condition tree, e.g. {"match": "all", "c": [...]}.</summary>
        public Task<DashaMailResponse> CreateAsync(object listId, string name, IDictionary<string, object> esegment, IDictionary<string, object> parameters = null)
        {
            var body = With(With(With(parameters, "list_id", listId), "name", name), "esegment", esegment);
            return Client.RequestAsync("POST", "/segments", null, body);
        }

        /// <summary>PUT /segments/{segmentId}</summary>
        public Task<DashaMailResponse> UpdateAsync(object segmentId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/segments/" + segmentId, null, parameters);
        }

        /// <summary>DELETE /segments/{segmentId}</summary>
        public Task<DashaMailResponse> DeleteAsync(object segmentId)
        {
            return Client.RequestAsync("DELETE", "/segments/" + segmentId);
        }

        /// <summary>POST /segments/count — recompute a segment's size, by id or by passing list_id + esegment directly.</summary>
        public Task<DashaMailResponse> CountAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/segments/count", null, parameters);
        }

        /// <summary>GET /segments/fields — which fields/operators are available for a list's segments.</summary>
        public Task<DashaMailResponse> FieldsAsync(object listId)
        {
            return Client.RequestAsync("GET", "/segments/fields", new Dictionary<string, object> { { "list_id", listId } });
        }
    }
}
