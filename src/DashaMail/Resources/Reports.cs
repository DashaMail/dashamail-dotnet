using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Campaign statistics: sent/delivered/opened/clicked/bounced, click and bounce
    /// breakdowns, geography, mail clients, event feed, A/B test results.
    /// https://dashamail.ru/api/reports/
    /// </summary>
    public class Reports : BaseResource
    {
        public Reports(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>GET /reports/{campaignId}/summary</summary>
        public Task<DashaMailResponse> SummaryAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/summary", parameters);
        }

        /// <summary>GET /reports/{campaignId}/timeline — metric values bucketed over time.</summary>
        public Task<DashaMailResponse> TimelineAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/timeline", parameters);
        }

        /// <summary>GET /reports/{campaignId}/variants — A/B test results.</summary>
        public Task<DashaMailResponse> AbAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/variants");
        }

        /// <summary>POST /reports/compare — compare metrics across periods/campaigns/lists.</summary>
        public Task<DashaMailResponse> CompareAsync(IList<object> periods, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("POST", "/reports/compare", null, With(parameters, "periods", periods));
        }

        /// <summary>GET /reports/{campaignId}/{metric} — recipient list for one event.</summary>
        public Task<DashaMailResponse> MetricAsync(object campaignId, string metric, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/" + metric, parameters);
        }

        /// <summary>GET /reports/{campaignId}/sent</summary>
        public Task<DashaMailResponse> SentAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "sent", parameters);
        }

        /// <summary>GET /reports/{campaignId}/delivered</summary>
        public Task<DashaMailResponse> DeliveredAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "delivered", parameters);
        }

        /// <summary>GET /reports/{campaignId}/opened</summary>
        public Task<DashaMailResponse> OpenedAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "opened", parameters);
        }

        /// <summary>GET /reports/{campaignId}/clicked</summary>
        public Task<DashaMailResponse> ClickedAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "clicked", parameters);
        }

        /// <summary>GET /reports/{campaignId}/bounced</summary>
        public Task<DashaMailResponse> BouncedAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "bounced", parameters);
        }

        /// <summary>GET /reports/{campaignId}/complained</summary>
        public Task<DashaMailResponse> ComplainedAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "complained", parameters);
        }

        /// <summary>GET /reports/{campaignId}/unsubscribed</summary>
        public Task<DashaMailResponse> UnsubscribedAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return MetricAsync(campaignId, "unsubscribed", parameters);
        }

        /// <summary>GET /reports/{campaignId}/events — full event feed with filters.</summary>
        public Task<DashaMailResponse> EventsAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/events", parameters);
        }

        /// <summary>GET /reports/{campaignId}/clickstat — clicks broken down by link.</summary>
        public Task<DashaMailResponse> ClickstatAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/clickstat");
        }

        /// <summary>GET /reports/{campaignId}/userclicks — who clicked a specific link.</summary>
        public Task<DashaMailResponse> UserclicksAsync(object campaignId, string url)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/userclicks", new Dictionary<string, object> { { "url", url } });
        }

        /// <summary>GET /reports/{campaignId}/bouncestat — bounces broken down by SMTP code.</summary>
        public Task<DashaMailResponse> BouncestatAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/bouncestat");
        }

        /// <summary>GET /reports/{campaignId}/domains — metrics broken down by recipient mail domain.</summary>
        public Task<DashaMailResponse> DomainsAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/domains", parameters);
        }

        /// <summary>GET /reports/{campaignId}/geo — geography of opens.</summary>
        public Task<DashaMailResponse> GeoAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/geo");
        }

        /// <summary>GET /reports/{campaignId}/clients — mail clients and devices.</summary>
        public Task<DashaMailResponse> ClientsAsync(object campaignId)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/clients");
        }

        /// <summary>GET /reports/{campaignId}/codes — confirmation codes.</summary>
        public Task<DashaMailResponse> CodesAsync(object campaignId, IDictionary<string, object> parameters = null)
        {
            return Client.RequestAsync("GET", "/reports/" + campaignId + "/codes", parameters);
        }
    }
}
