using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DashaMail.Tests
{
    public class RecordedCall
    {
        public string Method { get; set; }

        public string Url { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }

    /// <summary>
    /// An HttpTransport that never touches the network: SendAsync is
    /// overridden to return a canned HttpResponseMessage queued up front,
    /// so resource classes can be tested against realistic API payloads.
    /// Request bodies/headers are captured eagerly, before the request is
    /// disposed by the caller.
    /// </summary>
    public class FakeApiClient : HttpTransport
    {
        private readonly Queue<HttpResponseMessage> _queue = new Queue<HttpResponseMessage>();

        public List<RecordedCall> Calls { get; } = new List<RecordedCall>();

        public FakeApiClient(string apiKey = "test-key")
            : base(apiKey)
        {
        }

        public FakeApiClient QueueResponse(int status, string jsonBody, string contentType = "application/json")
        {
            var response = new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new StringContent(jsonBody ?? string.Empty, Encoding.UTF8, contentType),
            };
            _queue.Enqueue(response);
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            string body = request.Content != null
                ? await request.Content.ReadAsStringAsync().ConfigureAwait(false)
                : null;

            var headers = new Dictionary<string, string>();
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }
            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            Calls.Add(new RecordedCall
            {
                Method = request.Method.Method,
                Url = request.RequestUri.ToString(),
                Headers = headers,
                Body = body,
            });

            if (_queue.Count == 0)
            {
                throw new InvalidOperationException("FakeApiClient: no queued response for " + request.Method + " " + request.RequestUri);
            }
            return _queue.Dequeue();
        }
    }
}
