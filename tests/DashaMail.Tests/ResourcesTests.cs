using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DashaMail.Resources;
using Xunit;

namespace DashaMail.Tests
{
    public class ResourcesTests
    {
        private static string OkBody(object data, IDictionary<string, object> meta = null)
        {
            var response = new Dictionary<string, object>
            {
                { "msg", new Dictionary<string, object> { { "err_code", 0 }, { "text", "OK" }, { "type", "message" } } },
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
        public async Task ListsCreateSendsNameInBody()
        {
            var client = new FakeApiClient();
            client.QueueResponse(201, OkBody(new Dictionary<string, object> { { "list_id", 42 } }));
            var lists = new Lists(client);

            DashaMailResponse result = await lists.CreateAsync("Клиенты", new Dictionary<string, object> { { "company", "ООО Ромашка" } });

            Assert.Equal(42L, (long)result["list_id"]);
            RecordedCall call = client.Calls[0];
            Assert.Equal("POST", call.Method);
            Assert.EndsWith("/lists", call.Url);
            using JsonDocument doc = JsonDocument.Parse(call.Body);
            Assert.Equal("Клиенты", doc.RootElement.GetProperty("name").GetString());
            Assert.Equal("ООО Ромашка", doc.RootElement.GetProperty("company").GetString());
        }

        [Fact]
        public async Task ListsGetMemberEncodesEmailInPath()
        {
            var client = new FakeApiClient();
            client.QueueResponse(200, OkBody(new Dictionary<string, object> { { "email", "a+b@example.com" } }));
            var lists = new Lists(client);

            await lists.GetMemberAsync(1, "a+b@example.com");

            Assert.Contains(Uri.EscapeDataString("a+b@example.com"), client.Calls[0].Url);
        }

        [Fact]
        public async Task ListsMoveMemberSendsRequiredFields()
        {
            var client = new FakeApiClient();
            client.QueueResponse(200, OkBody(null));
            var lists = new Lists(client);

            await lists.MoveMemberAsync(1, "a@example.com", 2, 555);

            using JsonDocument doc = JsonDocument.Parse(client.Calls[0].Body);
            Assert.Equal(2, doc.RootElement.GetProperty("to_list_id").GetInt32());
            Assert.Equal(555, doc.RootElement.GetProperty("member_id").GetInt32());
        }

        [Fact]
        public async Task ListsFindMemberHitsAccountWideEndpoint()
        {
            var client = new FakeApiClient();
            client.QueueResponse(200, OkBody(new List<object>()));
            var lists = new Lists(client);

            await lists.FindMemberAsync("a@example.com");

            Assert.Contains("/members?", client.Calls[0].Url);
            Assert.Contains("email=a%40example.com", client.Calls[0].Url);
        }

        [Fact]
        public async Task TransactionalSendBuildsBody()
        {
            var client = new FakeApiClient();
            client.QueueResponse(201, OkBody(new Dictionary<string, object> { { "transaction_id", "abc" } }));
            var transactional = new Transactional(client);

            DashaMailResponse result = await transactional.SendAsync(
                "to@example.com", "from@yourdomain.com", "<p>Hi</p>", new Dictionary<string, object> { { "subject", "Hello" } });

            Assert.Equal("abc", (string)result["transaction_id"]);
            using JsonDocument doc = JsonDocument.Parse(client.Calls[0].Body);
            Assert.Equal("to@example.com", doc.RootElement.GetProperty("to").GetString());
            Assert.Equal("from@yourdomain.com", doc.RootElement.GetProperty("from_email").GetString());
            Assert.Equal("<p>Hi</p>", doc.RootElement.GetProperty("message").GetString());
            Assert.Equal("Hello", doc.RootElement.GetProperty("subject").GetString());
        }

        [Fact]
        public async Task ImagesOptimizeBase64EncodesBinaryData()
        {
            var client = new FakeApiClient();
            byte[] raw = System.Text.Encoding.UTF8.GetBytes("raw-bytes");
            client.QueueResponse(200, OkBody(new Dictionary<string, object> { { "image", Convert.ToBase64String(raw) } }));
            var images = new Images(client);

            await images.OptimizeAsync(raw, new Dictionary<string, object> { { "max_width", 800 } });

            using JsonDocument doc = JsonDocument.Parse(client.Calls[0].Body);
            Assert.Equal(Convert.ToBase64String(raw), doc.RootElement.GetProperty("image").GetString());
            Assert.Equal(800, doc.RootElement.GetProperty("max_width").GetInt32());
        }

        [Fact]
        public async Task PaginatedMembersExposesHasMore()
        {
            var client = new FakeApiClient();
            var meta = new Dictionary<string, object> { { "has_more", true }, { "limit", 100 } };
            client.QueueResponse(200, OkBody(new List<object> { new Dictionary<string, object> { { "email", "a@example.com" } } }, meta));
            var lists = new Lists(client);

            DashaMailResponse result = await lists.MembersAsync(1);

            Assert.True(result.HasMore);
            Assert.Equal(100, result.Limit);
        }
    }
}
