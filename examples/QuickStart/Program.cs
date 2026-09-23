using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DashaMail;

namespace QuickStart
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var dashamail = new DashaMailClient(Environment.GetEnvironmentVariable("DASHAMAIL_API_KEY") ?? "YOUR_API_KEY");

            try
            {
                // Account balance.
                DashaMailResponse balance = await dashamail.Account.BalanceAsync();
                Console.WriteLine("Balance: " + balance["balance"]);

                // Create a list and add a subscriber.
                DashaMailResponse created = await dashamail.Lists.CreateAsync("Example list");
                object listId = created["list_id"];

                await dashamail.Lists.AddMemberAsync(listId, "subscriber@example.com", new Dictionary<string, object> { { "merge_1", "Иван" } });

                // Iterate subscribers, page by page.
                int start = 0;
                while (true)
                {
                    DashaMailResponse page = await dashamail.Lists.MembersAsync(listId, new Dictionary<string, object> { { "start", start }, { "limit", 100 } });
                    foreach (object member in page)
                    {
                        var dict = (Dictionary<string, object>)member;
                        Console.WriteLine(dict["email"]);
                    }
                    start += page.Limit ?? 0;
                    if (!page.HasMore)
                    {
                        break;
                    }
                }

                // Send a transactional email.
                await dashamail.Transactional.SendAsync(
                    "subscriber@example.com",
                    "sender@yourdomain.com",
                    "<p>Спасибо за подписку!</p>",
                    new Dictionary<string, object> { { "subject", "Добро пожаловать" } });
            }
            catch (DashaMailRateLimitException e)
            {
                Console.WriteLine("Rate limited, retry after " + e.RetryAfter + "s");
            }
            catch (DashaMailApiException e)
            {
                Console.WriteLine("DashaMail API error " + e.ApiCode + ": " + e.Message);
            }
        }
    }
}
