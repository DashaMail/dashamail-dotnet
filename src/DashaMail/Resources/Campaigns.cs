using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Bulk campaigns: draft, build, launch, pause, A/B tests, attachments, folders.
    /// https://dashamail.ru/api/campaigns/
    /// </summary>
    public class Campaigns : BaseResource
    {
        public Campaigns(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /campaigns</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/campaigns", parameters);
        }

        /// <summary>GET /campaigns/{campaignId}</summary>
        public Task<DashaMailResponse> GetAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/campaigns/" + campaignId, parameters);
        }

        /// <summary>POST /campaigns — requires list_id, subject, from_email, from_name; see the API docs for the rest.</summary>
        public Task<DashaMailResponse> CreateAsync(IDictionary<string, object> parameters)
        {
            return Client.RequestAsync("POST", "/campaigns", null, parameters);
        }

        /// <summary>PUT /campaigns/{campaignId}</summary>
        public Task<DashaMailResponse> UpdateAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/campaigns/" + campaignId, null, parameters);
        }

        /// <summary>DELETE /campaigns/{campaignId}</summary>
        public Task<DashaMailResponse> DeleteAsync(object campaignId)
        {
            return Client.RequestAsync("DELETE", "/campaigns/" + campaignId);
        }

        /// <summary>POST /campaigns/{campaignId}/copy</summary>
        public Task<DashaMailResponse> CopyAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/copy", null, parameters);
        }

        /// <summary>POST /campaigns/{campaignId}/pause</summary>
        public Task<DashaMailResponse> PauseAsync(object campaignId)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/pause");
        }

        /// <summary>POST /campaigns/{campaignId}/resume</summary>
        public Task<DashaMailResponse> ResumeAsync(object campaignId)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/resume");
        }

        /// <summary>POST /campaigns/{campaignId}/schedule</summary>
        public Task<DashaMailResponse> ScheduleAsync(object campaignId, string deliveryTime, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/schedule", null, With(parameters, "delivery_time", deliveryTime));
        }

        /// <summary>POST /campaigns/{campaignId}/send — send a draft right now.</summary>
        public Task<DashaMailResponse> SendAsync(object campaignId)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/send");
        }

        /// <summary>POST /campaigns/{campaignId}/unschedule — pull a scheduled campaign back to DRAFT.</summary>
        public Task<DashaMailResponse> UnscheduleAsync(object campaignId)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/unschedule");
        }

        /// <summary>POST /campaigns/{campaignId}/test — send a test copy to your own address.</summary>
        public Task<DashaMailResponse> TestAsync(object campaignId, string email)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/test", null, new Dictionary<string, object> { { "email", email } });
        }

        /// <summary>GET /campaigns/{campaignId}/preview — browser preview link.</summary>
        public Task<DashaMailResponse> PreviewAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/campaigns/" + campaignId + "/preview");
        }

        /// <summary>GET /campaigns/{campaignId}/estimate — how many emails would be sent.</summary>
        public Task<DashaMailResponse> EstimateAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/campaigns/" + campaignId + "/estimate");
        }

        /// <summary>POST /campaigns/{campaignId}/resend — resend to recipients who did not open.</summary>
        public Task<DashaMailResponse> ResendAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/resend", null, parameters);
        }

        /// <summary>GET /campaigns/{campaignId}/attachments</summary>
        public Task<DashaMailResponse> GetAttachmentsAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/campaigns/" + campaignId + "/attachments");
        }

        /// <summary>POST /campaigns/{campaignId}/attachments — attach a file by URL.</summary>
        public Task<DashaMailResponse> AddAttachmentAsync(object campaignId, string url, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/attachments", null, With(parameters, "url", url));
        }

        /// <summary>DELETE /campaigns/{campaignId}/attachments/{id}</summary>
        public Task<DashaMailResponse> DeleteAttachmentAsync(object campaignId, object attachmentId)
        {
            return Client.RequestAsync("DELETE", "/campaigns/" + campaignId + "/attachments/" + attachmentId);
        }

        /// <summary>GET /campaigns/folders</summary>
        public Task<DashaMailResponse> GetFoldersAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/campaigns/folders", parameters);
        }

        /// <summary>POST /campaigns/{campaignId}/move — move a campaign into a folder.</summary>
        public Task<DashaMailResponse> MoveToFolderAsync(object campaignId, object folderId)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/move", null, new Dictionary<string, object> { { "folder_id", folderId } });
        }

        // -- A/B testing --------------------------------------------------------

        /// <summary>POST /campaigns/{campaignId}/ab — turn a draft into an A/B test.</summary>
        public Task<DashaMailResponse> CreateAbAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/ab", null, parameters);
        }

        /// <summary>GET /campaigns/{campaignId}/ab</summary>
        public Task<DashaMailResponse> GetAbAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/campaigns/" + campaignId + "/ab");
        }

        /// <summary>PUT /campaigns/{campaignId}/ab</summary>
        public Task<DashaMailResponse> UpdateAbAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("PUT", "/campaigns/" + campaignId + "/ab", null, parameters);
        }

        /// <summary>DELETE /campaigns/{campaignId}/ab — dismantle the A/B test back into a plain campaign.</summary>
        public Task<DashaMailResponse> DeleteAbAsync(object campaignId)
        {
            return Client.RequestAsync("DELETE", "/campaigns/" + campaignId + "/ab");
        }

        /// <summary>POST /campaigns/{campaignId}/ab/winner — pick the winning variant and schedule the rest.</summary>
        public Task<DashaMailResponse> AbWinnerAsync(object campaignId, object variantId, string deliveryTime)
        {
            var body = new Dictionary<string, object> { { "variant_id", variantId }, { "delivery_time", deliveryTime } };
            return Client.RequestAsync("POST", "/campaigns/" + campaignId + "/ab/winner", null, body);
        }

        /// <summary>DELETE /campaigns/{campaignId}/ab/winner — cancel a previously chosen winner.</summary>
        public Task<DashaMailResponse> CancelAbWinnerAsync(object campaignId)
        {
            return Client.RequestAsync("DELETE", "/campaigns/" + campaignId + "/ab/winner");
        }
    }
}
