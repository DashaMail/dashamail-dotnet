using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Address lists (address books), their subscribers and merge fields.
    /// https://dashamail.ru/api/lists/
    /// </summary>
    public class Lists : BaseResource
    {
        public Lists(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /lists — all address lists, newest first.</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists", parameters);
        }

        /// <summary>GET /lists/{listId}</summary>
        public Task<DashaMailResponse> GetAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId, parameters);
        }

        /// <summary>POST /lists</summary>
        public Task<DashaMailResponse> CreateAsync(string name, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists", null, With(parameters, "name", name));
        }

        /// <summary>PUT /lists/{listId}</summary>
        public Task<DashaMailResponse> UpdateAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/lists/" + listId, null, parameters);
        }

        /// <summary>DELETE /lists/{listId}</summary>
        public Task<DashaMailResponse> DeleteAsync(object listId)
        {
            return Client.RequestAsync("DELETE", "/lists/" + listId);
        }

        // -- Members ----------------------------------------------------------

        /// <summary>GET /lists/{listId}/members — start, limit, order, state, email, member_id, segment_id</summary>
        public Task<DashaMailResponse> MembersAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/members", parameters);
        }

        /// <summary>GET /lists/{listId}/members/{email}</summary>
        public Task<DashaMailResponse> GetMemberAsync(object listId, string email)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email));
        }

        /// <summary>POST /lists/{listId}/members — merge_1..merge_N and the rest go in parameters.</summary>
        public Task<DashaMailResponse> AddMemberAsync(object listId, string email, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists/" + listId + "/members", null, With(parameters, "email", email));
        }

        /// <summary>POST /lists/{listId}/members/batch — batch is a list of member dictionaries, each at least {"email": ...}.</summary>
        public Task<DashaMailResponse> AddMembersBatchAsync(object listId, IList<object> batch, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists/" + listId + "/members/batch", null, With(parameters, "batch", batch));
        }

        /// <summary>POST /lists/{listId}/members/import — import subscribers from a file.</summary>
        public Task<DashaMailResponse> ImportMembersAsync(object listId, string email, string type, IDictionary<string, object> parameters = null)
        {
            var body = With(With(parameters, "email", email), "type", type);
            return Client.RequestAsync("POST", "/lists/" + listId + "/members/import", null, body);
        }

        /// <summary>GET /lists/{listId}/members/import — result of the last import job.</summary>
        public Task<DashaMailResponse> GetImportResultAsync(object listId)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/members/import");
        }

        /// <summary>GET /lists/{listId}/import-history</summary>
        public Task<DashaMailResponse> GetImportHistoryAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/import-history", parameters);
        }

        /// <summary>PUT /lists/{listId}/members/{email}</summary>
        public Task<DashaMailResponse> UpdateMemberAsync(object listId, string email, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email), null, parameters);
        }

        /// <summary>DELETE /lists/{listId}/members/{email}</summary>
        public Task<DashaMailResponse> DeleteMemberAsync(object listId, string email, object memberId)
        {
            var body = new Dictionary<string, object> { { "member_id", memberId } };
            return Client.RequestAsync("DELETE", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email), null, body);
        }

        /// <summary>GET /members — find a subscriber address across every list in the account.</summary>
        public Task<DashaMailResponse> FindMemberAsync(string email)
        {
            return Client.RequestAsync("GET", "/members", new Dictionary<string, object> { { "email", email } });
        }

        /// <summary>POST /lists/{listId}/members/{email}/unsubscribe</summary>
        public Task<DashaMailResponse> UnsubscribeMemberAsync(object listId, string email, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email) + "/unsubscribe", null, parameters);
        }

        /// <summary>POST /lists/{listId}/members/{email}/move — move a subscriber to another list.</summary>
        public Task<DashaMailResponse> MoveMemberAsync(object listId, string email, object toListId, object memberId)
        {
            var body = new Dictionary<string, object> { { "to_list_id", toListId }, { "member_id", memberId } };
            return Client.RequestAsync("POST", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email) + "/move", null, body);
        }

        /// <summary>POST /lists/{listId}/members/{email}/copy — copy a subscriber to another list.</summary>
        public Task<DashaMailResponse> CopyMemberAsync(object listId, string email, object toListId, object memberId)
        {
            var body = new Dictionary<string, object> { { "to_list_id", toListId }, { "member_id", memberId } };
            return Client.RequestAsync("POST", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email) + "/copy", null, body);
        }

        /// <summary>GET /lists/{listId}/members/{email}/activity</summary>
        public Task<DashaMailResponse> MemberActivityAsync(object listId, string email, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/members/" + Uri.EscapeDataString(email) + "/activity", parameters);
        }

        /// <summary>GET /lists/{listId}/last-status — current subscription state of an address.</summary>
        public Task<DashaMailResponse> LastStatusAsync(object listId, string email)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/last-status", new Dictionary<string, object> { { "email", email } });
        }

        /// <summary>GET /lists/{listId}/check-email — validate an address before subscribing it.</summary>
        public Task<DashaMailResponse> CheckEmailAsync(object listId, string email)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/check-email", new Dictionary<string, object> { { "email", email } });
        }

        /// <summary>POST /lists/{listId}/clean — purge bounced/complained/unsubscribed members.</summary>
        public Task<DashaMailResponse> CleanAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists/" + listId + "/clean", null, parameters);
        }

        /// <summary>GET /lists/{listId}/unsubscribed</summary>
        public Task<DashaMailResponse> UnsubscribedAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/unsubscribed", parameters);
        }

        /// <summary>GET /lists/{listId}/complaints</summary>
        public Task<DashaMailResponse> ComplaintsAsync(object listId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/lists/" + listId + "/complaints", parameters);
        }

        // -- Merge fields -------------------------------------------------------

        /// <summary>POST /lists/{listId}/fields — type is one of the merge field types, e.g. "text", "choice".</summary>
        public Task<DashaMailResponse> AddFieldAsync(object listId, string type, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/lists/" + listId + "/fields", null, With(parameters, "type", type));
        }

        /// <summary>PUT /lists/{listId}/fields/{mergeId}</summary>
        public Task<DashaMailResponse> UpdateFieldAsync(object listId, object mergeId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/lists/" + listId + "/fields/" + mergeId, null, With(parameters, "merge_id", mergeId));
        }

        /// <summary>DELETE /lists/{listId}/fields/{mergeId}</summary>
        public Task<DashaMailResponse> DeleteFieldAsync(object listId, object mergeId)
        {
            return Client.RequestAsync("DELETE", "/lists/" + listId + "/fields/" + mergeId);
        }
    }
}
