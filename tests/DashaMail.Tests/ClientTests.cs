using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DashaMail.Tests
{
    public class ClientTests
    {
        private static string OkBody(object data, IDictionary<string, object> meta = null, string message = "OK")
        {
            var response = new Dictionary<string, object>
            {
                { "msg", new Dictionary<string, object> { { "err_code", 0 }, { "text", message }, { "type", "message" } } },
                { "data", data },
            };
            var envelope = new Dictionary<string, object> { { "response", response } };
            if (meta != null)
            {
                envelope["meta"] = meta;
            }
            return JsonSerializer.Serialize(envelope);
        }

        [Fact]
        public async Task SuccessfulRequestUnwrapsResponseData()
        {
            var client = new FakeApiClient();
            var data = new List<object> { new Dictionary<string, object> { { "id", 1 }, { "name", "Клиенты" } } };
            client.QueueResponse(200, OkBody(data));

            DashaMailResponse result = await client.RequestAsync("GET", "/lists");

            Assert.NotNull(result);
            Assert.Equal("OK", result.Message);
            var list = Assert.IsType<List<object>>(result.Data);
            Assert.Single(list);
        }

        [Fact]
        public async Task PaginationMetaIsExposed()
        {
            var client = new FakeApiClient();
            var meta = new Dictionary<string, object> { { "has_more", true }, { "limit", 3 } };
            client.QueueResponse(200, OkBody(new List<object> { 1, 2, 3 }, meta));

            DashaMailResponse result = await client.RequestAsync("GET", "/lists/1/members");

            Assert.True(result.HasMore);
            Assert.Equal(3, result.Limit);
        }

        [Fact]
        public async Task NoContentReturnsNull()
        {
            var client = new FakeApiClient();
            client.QueueResponse(204, "");

            DashaMailResponse result = await client.RequestAsync("DELETE", "/lists/1");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthorizationHeaderIsSent()
        {
            var client = new FakeApiClient("secret-key-123");
            client.QueueResponse(200, OkBody(new List<object>()));

            await client.RequestAsync("GET", "/lists");

            Assert.Equal("Bearer secret-key-123", client.Calls[0].Headers["Authorization"]);
        }

        [Fact]
        public async Task JsonBodyIsEncodedAndContentTypeSet()
        {
            var client = new FakeApiClient();
            client.QueueResponse(201, OkBody(new Dictionary<string, object> { { "list_id", 5 } }));

            await client.RequestAsync("POST", "/lists", null, new Dictionary<string, object> { { "name", "Тест" } });

            Assert.Contains("application/json", client.Calls[0].Headers["Content-Type"]);
            using JsonDocument doc = JsonDocument.Parse(client.Calls[0].Body);
            Assert.Equal("Тест", doc.RootElement.GetProperty("name").GetString());
        }

        [Theory]
        [InlineData(401, typeof(DashaMailAuthenticationException))]
        [InlineData(404, typeof(DashaMailNotFoundException))]
        [InlineData(422, typeof(DashaMailValidationException))]
        [InlineData(429, typeof(DashaMailRateLimitException))]
        public async Task ErrorStatusesMapToExceptionTypes(int status, System.Type expectedType)
        {
            var client = new FakeApiClient();
            client.QueueResponse(status, "{\"error\":{\"code\":999,\"message\":\"boom\",\"details\":{}}}");

            DashaMailApiException error = await Assert.ThrowsAnyAsync<DashaMailApiException>(
                () => client.RequestAsync("GET", "/whatever"));

            Assert.IsType(expectedType, error);
        }

        [Fact]
        public async Task RateLimitExposesRetryAfterFromDetails()
        {
            var client = new FakeApiClient();
            client.QueueResponse(429, "{\"error\":{\"code\":58,\"message\":\"Limit\",\"details\":{\"limit_per_minute\":120,\"retry_after\":60}}}");

            var error = await Assert.ThrowsAsync<DashaMailRateLimitException>(
                () => client.RequestAsync("GET", "/lists"));

            Assert.Equal(60, error.RetryAfter);
            Assert.Equal(58, error.ApiCode);
        }

        [Fact]
        public async Task QueryParamsAreAppendedToUrl()
        {
            var client = new FakeApiClient();
            client.QueueResponse(200, OkBody(new List<object>()));

            await client.RequestAsync("GET", "/lists", new Dictionary<string, object> { { "state", "active" } });

            Assert.Contains("state=active", client.Calls[0].Url);
        }
    }
}
