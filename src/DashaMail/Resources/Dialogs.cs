using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Subscriber replies to campaigns, threaded per subscriber.
    /// https://dashamail.ru/api/dialogs/
    /// </summary>
    public class Dialogs : BaseResource
    {
        public Dialogs(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /dialogs</summary>
        public Task<DashaMailResponse> AllAsync(IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/dialogs", parameters);
        }

        /// <summary>GET /dialogs/{dialogId}</summary>
        public Task<DashaMailResponse> GetAsync(object dialogId)
        {
            return Client.RequestAsync("GET", "/dialogs/" + dialogId);
        }

        /// <summary>GET /dialogs/{dialogId}/messages</summary>
        public Task<DashaMailResponse> MessagesAsync(object dialogId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/dialogs/" + dialogId + "/messages", parameters);
        }

        /// <summary>POST /dialogs/{dialogId}/reply — reply as the campaign's sender.</summary>
        public Task<DashaMailResponse> ReplyAsync(object dialogId, string bodyText, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/dialogs/" + dialogId + "/reply", null, With(parameters, "body_text", bodyText));
        }

        /// <summary>POST /dialogs/{dialogId}/read</summary>
        public Task<DashaMailResponse> MarkReadAsync(object dialogId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/dialogs/" + dialogId + "/read", null, parameters);
        }

        /// <summary>POST /dialogs/{dialogId}/unread</summary>
        public Task<DashaMailResponse> MarkUnreadAsync(object dialogId)
        {
            return Client.RequestAsync("POST", "/dialogs/" + dialogId + "/unread");
        }

        /// <summary>POST /dialogs/{dialogId}/close</summary>
        public Task<DashaMailResponse> CloseAsync(object dialogId)
        {
            return Client.RequestAsync("POST", "/dialogs/" + dialogId + "/close");
        }

        /// <summary>POST /dialogs/{dialogId}/open</summary>
        public Task<DashaMailResponse> OpenAsync(object dialogId)
        {
            return Client.RequestAsync("POST", "/dialogs/" + dialogId + "/open");
        }

        /// <summary>GET /dialogs/unread-count</summary>
        public Task<DashaMailResponse> UnreadCountAsync()
        {
            return Client.RequestAsync("GET", "/dialogs/unread-count");
        }

        /// <summary>GET /dialogs/attachments/{attachmentId} — link to a reply's attachment.</summary>
        public Task<DashaMailResponse> AttachmentAsync(object attachmentId)
        {
            return Client.RequestAsync("GET", "/dialogs/attachments/" + attachmentId);
        }
    }
}
