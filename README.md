# dashamail-dotnet

Официальный .NET SDK для [DashaMail REST API v2](https://dashamail.ru/api/) — адресные
базы, рассылки, автоматизации, транзакционные письма, отчёты, диалоги, обработка входящей
почты и оптимизация изображений из одного клиента без единой зависимости, кроме базовой
библиотеки классов (`System.Net.Http`, `System.Text.Json`).

## Требования

- .NET 6.0 или новее

`System.Text.Json` идёт в составе рантайма (без отдельного пакета) только начиная с .NET 5,
поэтому SDK нацелен на `net6.0`, а не на `netstandard2.0` — со старым таргетом под .NET
Framework пришлось бы тащить сторонний пакет ради JSON.

## Установка

```bash
dotnet add package DashaMail
```

## Где взять API-ключ

Личный кабинет → Аккаунт → API и интеграции. Обращайтесь с ним как с паролем: он действует
от имени всего аккаунта.

## Быстрый старт

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

Более полный пример — в [`examples/QuickStart/Program.cs`](examples/QuickStart/Program.cs),
а полный справочник параметров каждого эндпоинта — на
[dashamail.ru/api](https://dashamail.ru/api/): SDK повторяет его метод в метод.
Необязательные параметры передаются завершающим `IDictionary<string, object>` с теми же
именами полей, что и в API (snake_case, например `merge_1`, `from_email`) — полей у API
слишком много, чтобы выделять под каждое отдельный типизированный параметр.

## Ресурсы

`DashaMailClient` — это объект с одним свойством на каждый раздел API, все типы — в
пространстве имён `DashaMail.Resources`. Каждый метод основан на `Task` (суффикс `Async`) и
возвращает [`DashaMailResponse`](src/DashaMail/DashaMailResponse.cs) (оборачивает полезную
нагрузку `data`, перебирается через `foreach`, если это список) либо бросает
`DashaMailApiException`.

| Свойство           | Класс                          | Что покрывает |
|---------------------|-----------------------------------|--------|
| `.Lists`          | `Resources.Lists`         | Адресные базы, подписчики, дополнительные поля, импорт |
| `.Segments`       | `Resources.Segments`      | Сохранённые сегменты подписчиков |
| `.Campaigns`      | `Resources.Campaigns`     | Рассылки: черновик, запуск, пауза, A/B-тесты, вложения, папки |
| `.Automations`    | `Resources.Automations`   | Письма по событиям подписчика |
| `.Workflows`      | `Resources.Workflows`     | Сценарии визуального конструктора |
| `.Templates`      | `Resources.Templates`     | Сохранённые HTML-шаблоны и шаблонные рассылки |
| `.Reports`        | `Resources.Reports`       | Статистика рассылок, лента событий, разбивки по кликам/возвратам/гео, результаты A/B |
| `.Transactional`  | `Resources.Transactional` | Одиночные транзакционные письма: отправка, статус, журнал, статистика |
| `.Account`        | `Resources.Account`       | Баланс, отправители, домены отправки, webhooks |
| `.Dialogs`        | `Resources.Dialogs`       | Ответы подписчиков на рассылки |
| `.Router`         | `Resources.Router`        | Обработка входящей почты: домены, правила маршрутизации, сохранённые письма |
| `.Images`         | `Resources.Images`        | Уменьшение веса изображения перед вставкой в рассылку |

**Про названия:** публичные типы намеренно с префиксом `DashaMail` там, где голое имя было
бы слишком общим и могло бы столкнуться с другими библиотеками в проекте —
`DashaMailClient`, `DashaMailResponse`, `DashaMailBinaryResponse`, `DashaMailApiException`
и его подклассы. Низкоуровневый HTTP-слой называется `HttpTransport`, а классы ресурсов
(`Lists`, `Campaigns`, ...) живут в пространстве имён `DashaMail.Resources` без префикса,
поскольку до них обычно добираются через `DashaMailClient`, а не обращаются к ним напрямую.

## Работа с ответом

`DashaMailResponse.Data` хранит разобранную полезную нагрузку как обычные типы BCL —
`Dictionary<string, object>` для объекта, `List<object>` для списка или скаляр, поскольку
форма ответа API заранее не известна статически:

```csharp
var members = await dashamail.Lists.MembersAsync(listId, new Dictionary<string, object> { { "limit", 100 } });

foreach (var member in members)                 // DashaMailResponse — перечислимый объект
{
    var dict = (Dictionary<string, object>)member;
    Console.WriteLine(dict["email"]);
}

members.Data;                                   // сырой Dictionary/List, если перебирать не нужно
members.HasMore;                                // true, если есть следующая страница (см. «Пагинация» ниже)
```

## Пагинация

Списочные эндпоинты (`Lists.MembersAsync`, `Lists.UnsubscribedAsync`,
`Transactional.LogAsync` и другие) не возвращают общее количество записей — вместо этого
DashaMail сообщает, есть ли ещё страница:

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

## Обработка ошибок

Любой ответ не из диапазона 2xx бросает подкласс `DashaMailApiException`, выбранный по
HTTP-статусу:

| Исключение                          | HTTP-статус |
|--------------------------------------|------|
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
    // e.ApiCode     — собственный устойчивый код ошибки DashaMail (см. https://dashamail.ru/api/errors/)
    // e.HttpStatus  — HTTP-статус ответа
    // e.Details     — дополнительный структурированный контекст от API, если есть
    Console.Error.WriteLine($"{e.ApiCode}: {e.Message}");
}
```

Если ответ вообще не пришёл (обрыв DNS, TLS, таймаут...), бросается
`DashaMailNetworkException`.

## Изображения

`POST /images/optimize` — единственный эндпоинт, у которого вход и выход не просто JSON:

```csharp
// Из байтов, уже загруженных в память:
var result = await dashamail.Images.OptimizeAsync(File.ReadAllBytes("banner.png"),
    new Dictionary<string, object> { { "max_width", 1600 } });
File.WriteAllBytes("banner-optimized.png", Convert.FromBase64String((string)result["image"]));

// Прямо с диска, без предварительной загрузки в byte[]:
var result2 = await dashamail.Images.OptimizeFileAsync("/path/to/banner.png");

// То же самое, но без промежуточного base64 — сразу сырые байты:
var binary = await dashamail.Images.OptimizeFileBinaryAsync("/path/to/banner.png");
binary.SaveTo("/path/to/banner-optimized.png");
```

## Расширенная настройка

```csharp
var dashamail = new DashaMailClient(
    "YOUR_API_KEY",
    timeoutSeconds: 60,                          // по умолчанию 30
    baseUrl: "https://api.dashamail.com/v2",     // переопределить для тестов/прокси
    userAgent: "my-app/1.0 (+dashamail-dotnet)",
    httpClient: myPooledHttpClient);             // использовать свой HttpClient вместо создания нового

// Способ вызвать эндпоинт, для которого в SDK ещё нет отдельного метода:
await dashamail.Client.RequestAsync("GET", "/some/new/endpoint", new Dictionary<string, object> { { "foo", "bar" } });
```

## Тесты

```bash
dotnet test tests/DashaMail.Tests/DashaMail.Tests.csproj
```

## Лицензия

MIT, см. [LICENSE](LICENSE).
