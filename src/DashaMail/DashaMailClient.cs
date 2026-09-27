using System.Net.Http;
using DashaMail.Resources;

namespace DashaMail
{
    /// <summary>
    /// Entry point of the DashaMail .NET SDK.
    ///
    /// <code>
    /// var dashamail = new DashaMailClient("YOUR_API_KEY");
    /// var lists = await dashamail.Lists.AllAsync();
    /// await dashamail.Transactional.SendAsync("user@example.com", "sender@yourdomain.com", "&lt;p&gt;Hi!&lt;/p&gt;");
    /// </code>
    /// </summary>
    public class DashaMailClient
    {
        public HttpTransport Client { get; }

        public Lists Lists { get; }

        public Segments Segments { get; }

        public Campaigns Campaigns { get; }

        public Automations Automations { get; }

        public Workflows Workflows { get; }

        public Templates Templates { get; }

        public Reports Reports { get; }

        public Transactional Transactional { get; }

        public Account Account { get; }

        public Dialogs Dialogs { get; }

        public Router Router { get; }

        public Images Images { get; }

        /// <param name="apiKey">Account API key — Личный кабинет → Аккаунт → API и интеграции.</param>
        /// <param name="baseUrl">Override the API origin, e.g. for a proxy or a mock server.</param>
        /// <param name="timeoutSeconds">Request timeout in seconds. Default 30.</param>
        /// <param name="userAgent">Override the User-Agent header.</param>
        /// <param name="httpClient">Supply your own <see cref="HttpClient"/> (e.g. to reuse a pooled instance); one is created otherwise.</param>
        public DashaMailClient(string apiKey, string baseUrl = null, int timeoutSeconds = 30, string userAgent = null, HttpClient httpClient = null)
        {
            Client = new HttpTransport(apiKey, baseUrl, timeoutSeconds, userAgent, httpClient);

            Lists = new Lists(Client);
            Segments = new Segments(Client);
            Campaigns = new Campaigns(Client);
            Automations = new Automations(Client);
            Workflows = new Workflows(Client);
            Templates = new Templates(Client);
            Reports = new Reports(Client);
            Transactional = new Transactional(Client);
            Account = new Account(Client);
            Dialogs = new Dialogs(Client);
            Router = new Router(Client);
            Images = new Images(Client);
        }
    }
}
