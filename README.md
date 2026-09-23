# dashamail-dotnet

Official .NET SDK for the [DashaMail REST API v2](https://dashamail.ru/api/) — address
lists, campaigns, automations, transactional email, reports, dialogs, inbound mail routing
and image optimization, from a single client with no dependencies beyond the base class
library (`System.Net.Http`, `System.Text.Json`).

## Requirements

- .NET 6.0 or newer

`System.Text.Json` only ships in-box (no extra package needed) from .NET 5 onward, which is
why this SDK targets `net6.0` rather than `netstandard2.0` — the older .NET Framework
target would otherwise force a third-party JSON dependency.

## Installation

```bash
dotnet add package DashaMail
```

## Getting an API key

Личный кабинет → Аккаунт → API и интеграции. Treat it like a password: it acts on behalf
of the whole account.

## Quickstart

```csharp
using DashaMail;

var dashamail = new DashaMailClient("YOUR_API_KEY");

var balance = await dashamail.Account.BalanceAsync();
Console.WriteLine(balance["balance"]);

var created = await dashamail.Lists.CreateAsync("Newsletter");
await dashamail.Lists.AddMemberAsync(created["list_id"], "subscriber@example.com",
    new Dictionary<string, object> { { "merge_1", "Иван" } });

await dashamail.Transactional.SendAsync(
    "subscriber@example.com",
    "sender@yourdomain.com",
    "<p>Спасибо за подписку!</p>",
    new Dictionary<string, object> { { "subject", "Добро пожаловать" } });
```

See [`examples/QuickStart/Program.cs`](examples/QuickStart/Program.cs) for a fuller
walkthrough, and [dashamail.ru/api](https://dashamail.ru/api/) for the full parameter
reference of every endpoint — the SDK mirrors it method-for-method. Optional parameters are
passed as a trailing `IDictionary<string, object>` matching the API's field names
(snake_case, e.g. `merge_1`, `from_email`), since the API's surface is too large to give
every optional field its own strongly-typed parameter.

## Resources

`DashaMailClient` exposes one property per section of the API, all under the
`DashaMail.Resources` namespace. Every method is `Task`-based (suffixed `Async`) and
returns a [`DashaMailResponse`](src/DashaMail/DashaMailResponse.cs) (wraps the `data`
payload, enumerable when it's a list) or throws a `DashaMailApiException`.

| Property          | Class                          | Covers |
|-------------------|-----------------------------------|--------|
| `.Lists`          | `Resources.Lists`         | Address lists, subscribers, merge fields, imports |
| `.Segments`       | `Resources.Segments`      | Saved subscriber segments |
| `.Campaigns`      | `Resources.Campaigns`     | Bulk campaigns: draft, launch, pause, A/B tests, attachments, folders |
| `.Automations`    | `Resources.Automations`   | Event-triggered emails |
| `.Workflows`      | `Resources.Workflows`     | Visual-builder automation scenarios |
| `.Templates`      | `Resources.Templates`     | Saved HTML templates and templated campaigns |
| `.Reports`        | `Resources.Reports`       | Campaign statistics, events, click/bounce/geo breakdowns, A/B results |
| `.Transactional`  | `Resources.Transactional` | One-off transactional email: send, status, log, stats |
| `.Account`        | `Resources.Account`       | Balance, senders, sending domains, webhooks |
| `.Dialogs`        | `Resources.Dialogs`       | Subscriber replies to campaigns |
| `.Router`         | `Resources.Router`        | Inbound mail: domains, routing rules, stored messages |
| `.Images`         | `Resources.Images`        | Resize/recompress an image before using it in a campaign |

**Naming note:** public types are prefixed with `DashaMail` where the bare name would be
too generic and likely to collide with other libraries in a consuming project —
`DashaMailClient`, `DashaMailResponse`, `DashaMailBinaryResponse`, `DashaMailApiException`
and its subclasses. The lower-level HTTP layer is `HttpTransport`, and resource classes
(`Lists`, `Campaigns`, ...) live in the `DashaMail.Resources` namespace instead, since
they're normally reached through `DashaMailClient` rather than named directly.

## Working with responses

`DashaMailResponse.Data` holds the parsed payload as plain BCL types — a
`Dictionary<string, object>` for an object, a `List<object>` for a list, or a scalar —
since the API's shape isn't known statically:

```csharp
var members = await dashamail.Lists.MembersAsync(listId, new Dictionary<string, object> { { "limit", 100 } });

foreach (var member in members)                 // DashaMailResponse is enumerable
{
    var dict = (Dictionary<string, object>)member;
    Console.WriteLine(dict["email"]);
}

members.Data;                                   // the raw Dictionary/List, if you'd rather not iterate
members.HasMore;                                // true if there's another page (see Pagination below)
```

## Pagination

List endpoints (`Lists.MembersAsync`, `Lists.UnsubscribedAsync`, `Transactional.LogAsync`,
...) don't return a total count — DashaMail tells you instead whether there's another page:

```csharp
int start = 0;
while (true)
{
    var page = await dashamail.Lists.MembersAsync(listId, new Dictionary<string, object> { { "start", start }, { "limit", 100 } });
    foreach (var member in page) { /* ... */ }
    start += page.Limit ?? 0;
    if (!page.HasMore) break;
}
```

## Error handling

Every non-2xx response throws a subclass of `DashaMailApiException`, chosen by HTTP status:

| Exception                          | HTTP status |
|-------------------------------------|------|
| `DashaMailAuthenticationException`  | 401 |
| `DashaMailPaymentRequiredException` | 402 |
| `DashaMailAuthorizationException`   | 403 |
| `DashaMailNotFoundException`        | 404 |
| `DashaMailConflictException`        | 409 |
| `DashaMailPayloadTooLargeException` | 413 |
| `DashaMailValidationException`      | 422 |
| `DashaMailRateLimitException`       | 429 |
| `DashaMailServerException`          | 5xx |

```csharp
try
{
    await dashamail.Campaigns.CreateAsync(new Dictionary<string, object>
    {
        { "list_id", 1 }, { "subject", "Hi" }, { "from_email", "a@b.com" }, { "from_name", "A" },
    });
}
catch (DashaMailRateLimitException e)
{
    await Task.Delay(TimeSpan.FromSeconds(e.RetryAfter ?? 60));
}
catch (DashaMailApiException e)
{
    // e.ApiCode     — DashaMail's own stable error code (see https://dashamail.ru/api/errors/)
    // e.HttpStatus  — the HTTP status of the response
    // e.Details     — any extra structured context the API attached
    Console.Error.WriteLine($"{e.ApiCode}: {e.Message}");
}
```

A request that never got an HTTP response at all (DNS, TLS, timeout...) throws
`DashaMailNetworkException` instead.

## Images

`POST /images/optimize` is the one endpoint that isn't plain JSON in and out:

```csharp
// From bytes already in memory:
var result = await dashamail.Images.OptimizeAsync(File.ReadAllBytes("banner.png"),
    new Dictionary<string, object> { { "max_width", 1600 } });
File.WriteAllBytes("banner-optimized.png", Convert.FromBase64String((string)result["image"]));

// Straight from disk, without loading it into a byte[] first:
var result2 = await dashamail.Images.OptimizeFileAsync("/path/to/banner.png");

// Same, but skip the base64 round-trip and get raw bytes back:
var binary = await dashamail.Images.OptimizeFileBinaryAsync("/path/to/banner.png");
binary.SaveTo("/path/to/banner-optimized.png");
```

## Advanced configuration

```csharp
var dashamail = new DashaMailClient(
    "YOUR_API_KEY",
    timeoutSeconds: 60,                          // default 30
    baseUrl: "https://api.dashamail.com/v2",     // override for testing/proxying
    userAgent: "my-app/1.0 (+dashamail-dotnet)",
    httpClient: myPooledHttpClient);             // reuse your own HttpClient instead of creating one

// Escape hatch for an endpoint the SDK doesn't wrap yet:
await dashamail.Client.RequestAsync("GET", "/some/new/endpoint", new Dictionary<string, object> { { "foo", "bar" } });
```

## Testing

```bash
dotnet test tests/DashaMail.Tests/DashaMail.Tests.csproj
```

## License

MIT, see [LICENSE](LICENSE).
