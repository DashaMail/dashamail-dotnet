using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using DashaMail.Internal;

namespace DashaMail
{
    /// <summary>
    /// Thin HTTP layer over the DashaMail REST API v2 (https://dashamail.ru/api/).
    ///
    /// <para>
    /// Talks JSON over <see cref="System.Net.Http.HttpClient"/> and
    /// <see cref="System.Text.Json"/>, both part of the .NET base class
    /// library — no third-party dependencies. Resource classes
    /// (<c>DashaMail.Resources.*</c>) build on top of RequestAsync()/
    /// RequestMultipartAsync(); most applications should go through
    /// <see cref="DashaMailClient"/> rather than use this class directly.
    /// </para>
    /// </summary>
    public class HttpTransport
    {
        public const string DefaultBaseUrl = "https://api.dashamail.com/v2";
        public const string Version = "1.0.0";

        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".webp", "image/webp" },
            { ".bmp", "image/bmp" },
            { ".svg", "image/svg+xml" },
        };

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _userAgent;
        private readonly HttpClient _httpClient;

        /// <param name="apiKey">Account API key — Личный кабинет → Аккаунт → API и интеграции.</param>
        /// <param name="baseUrl">Override the API origin, e.g. for a proxy or a mock server.</param>
        /// <param name="timeoutSeconds">Request timeout in seconds. Default 30.</param>
        /// <param name="userAgent">Override the User-Agent header.</param>
        /// <param name="httpClient">Supply your own <see cref="HttpClient"/> (e.g. to reuse a pooled instance); one is created otherwise.</param>
        public HttpTransport(string apiKey, string baseUrl = null, int timeoutSeconds = 30, string userAgent = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("DashaMail API key must be a non-empty string.", nameof(apiKey));
            }

            _apiKey = apiKey;
            _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
            _userAgent = userAgent ?? "dashamail-dotnet/" + Version;
            _httpClient = httpClient ?? new HttpClient();
            if (httpClient == null)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }
        }

        /// <summary>
        /// JSON request. Body is sent as application/json; on 2xx the
        /// decoded response.data is returned wrapped in a
        /// <see cref="DashaMailResponse"/>, on error a
        /// <see cref="DashaMailApiException"/> subclass is thrown.
        /// Returns null for a 204 No Content.
        /// </summary>
        public async Task<DashaMailResponse> RequestAsync(string method, string path, IDictionary<string, object> query = null, IDictionary<string, object> body = null)
        {
            string url = BuildUrl(path, query);
            using (var request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                ApplyBaseHeaders(request);
                if (body != null)
                {
                    string json = JsonSerializer.Serialize(body, SerializerOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await SendWithErrorHandlingAsync(request).ConfigureAwait(false);
                using (response)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    return ParseJsonResponse((int)response.StatusCode, bytes);
                }
            }
        }

        /// <summary>multipart/form-data request with a single file field — used by POST /images/optimize.</summary>
        public async Task<DashaMailResponse> RequestMultipartAsync(string method, string path, IDictionary<string, object> query, IDictionary<string, object> fields, string fileFieldName, string filePath, string fileName = null, string mimeType = null)
        {
            string url = BuildUrl(path, query);
            using (var content = BuildMultipartContent(fields, fileFieldName, filePath, fileName, mimeType))
            using (var request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                ApplyBaseHeaders(request);
                request.Content = content;

                HttpResponseMessage response = await SendWithErrorHandlingAsync(request).ConfigureAwait(false);
                using (response)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    return ParseJsonResponse((int)response.StatusCode, bytes);
                }
            }
        }

        /// <summary>Same as RequestMultipartAsync, but returns a <see cref="DashaMailBinaryResponse"/> instead of decoding JSON.</summary>
        public async Task<DashaMailBinaryResponse> RequestMultipartBinaryAsync(string method, string path, IDictionary<string, object> query, IDictionary<string, object> fields, string fileFieldName, string filePath, string fileName = null, string mimeType = null)
        {
            string url = BuildUrl(path, query);
            using (var content = BuildMultipartContent(fields, fileFieldName, filePath, fileName, mimeType))
            using (var request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                ApplyBaseHeaders(request);
                request.Content = content;

                HttpResponseMessage response = await SendWithErrorHandlingAsync(request).ConfigureAwait(false);
                using (response)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string contentType = response.Content.Headers.ContentType?.ToString();
                    return ParseBinaryResponse((int)response.StatusCode, bytes, contentType);
                }
            }
        }

        /// <summary>Same as RequestAsync, but returns a <see cref="DashaMailBinaryResponse"/> instead of decoding JSON.</summary>
        public async Task<DashaMailBinaryResponse> RequestBinaryAsync(string method, string path, IDictionary<string, object> query = null, IDictionary<string, object> body = null)
        {
            string url = BuildUrl(path, query);
            using (var request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                ApplyBaseHeaders(request);
                if (body != null)
                {
                    string json = JsonSerializer.Serialize(body, SerializerOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await SendWithErrorHandlingAsync(request).ConfigureAwait(false);
                using (response)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string contentType = response.Content.Headers.ContentType?.ToString();
                    return ParseBinaryResponse((int)response.StatusCode, bytes, contentType);
                }
            }
        }

        private void ApplyBaseHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _apiKey);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        }

        private string BuildUrl(string path, IDictionary<string, object> query)
        {
            string url = _baseUrl + "/" + path.TrimStart('/');
            if (query != null && query.Count > 0)
            {
                var parts = new List<string>();
                foreach (KeyValuePair<string, object> pair in query)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    string value = Convert.ToString(pair.Value, CultureInfo.InvariantCulture);
                    parts.Add(Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(value));
                }
                if (parts.Count > 0)
                {
                    url += "?" + string.Join("&", parts);
                }
            }
            return url;
        }

        private static string GuessMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return MimeTypes.TryGetValue(ext, out string mime) ? mime : "application/octet-stream";
        }

        private static MultipartFormDataContent BuildMultipartContent(IDictionary<string, object> fields, string fileFieldName, string filePath, string fileName, string mimeType)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("File not readable: " + filePath, nameof(filePath));
            }

            var content = new MultipartFormDataContent();
            if (fields != null)
            {
                foreach (KeyValuePair<string, object> pair in fields)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    content.Add(new StringContent(Convert.ToString(pair.Value, CultureInfo.InvariantCulture)), pair.Key);
                }
            }

            string resolvedName = fileName ?? Path.GetFileName(filePath);
            string resolvedMime = mimeType ?? GuessMimeType(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(resolvedMime);
            content.Add(fileContent, fileFieldName, resolvedName);

            return content;
        }

        private DashaMailResponse ParseJsonResponse(int status, byte[] bytes)
        {
            if (status == 204 || bytes == null || bytes.Length == 0)
            {
                return null;
            }

            object decoded;
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(bytes))
                {
                    decoded = JsonConversion.ToObject(doc.RootElement);
                }
            }
            catch (JsonException ex)
            {
                throw new DashaMailNetworkException(
                    "DashaMail API returned a non-JSON response (HTTP " + status + "): " + TruncatedBody(bytes), ex);
            }

            var decodedDict = decoded as Dictionary<string, object>;

            if (status >= 200 && status < 300)
            {
                object response = decodedDict != null && decodedDict.ContainsKey("response") ? decodedDict["response"] : decoded;
                var responseDict = response as Dictionary<string, object>;
                object data = responseDict != null && responseDict.ContainsKey("data") ? responseDict["data"] : response;

                string message = null;
                if (responseDict != null && responseDict.TryGetValue("msg", out object msgValue) && msgValue is Dictionary<string, object> msgDict
                    && msgDict.TryGetValue("text", out object textValue))
                {
                    message = textValue as string;
                }

                Dictionary<string, object> meta = null;
                if (decodedDict != null && decodedDict.TryGetValue("meta", out object metaValue))
                {
                    meta = metaValue as Dictionary<string, object>;
                }

                return new DashaMailResponse(data, meta ?? new Dictionary<string, object>(), message);
            }

            Dictionary<string, object> error = null;
            if (decodedDict != null && decodedDict.TryGetValue("error", out object errorValue))
            {
                error = errorValue as Dictionary<string, object>;
            }
            throw DashaMailApiException.FromError(status, error ?? DefaultError(status));
        }

        private DashaMailBinaryResponse ParseBinaryResponse(int status, byte[] bytes, string contentType)
        {
            if (status >= 200 && status < 300)
            {
                return new DashaMailBinaryResponse(bytes, contentType ?? "application/octet-stream", status);
            }

            Dictionary<string, object> decodedDict = null;
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(bytes))
                {
                    decodedDict = JsonConversion.ToObject(doc.RootElement) as Dictionary<string, object>;
                }
            }
            catch (JsonException)
            {
                // fall through to the default error below
            }

            Dictionary<string, object> error = null;
            if (decodedDict != null && decodedDict.TryGetValue("error", out object errorValue))
            {
                error = errorValue as Dictionary<string, object>;
            }
            throw DashaMailApiException.FromError(status, error ?? DefaultError(status));
        }

        private static Dictionary<string, object> DefaultError(int status)
        {
            return new Dictionary<string, object> { { "code", status }, { "message", "Unknown DashaMail API error" } };
        }

        private static string TruncatedBody(byte[] bytes)
        {
            string text = Encoding.UTF8.GetString(bytes);
            return text.Length > 500 ? text.Substring(0, 500) : text;
        }

        /// <summary>
        /// Sends the request. Kept as its own virtual method so tests can
        /// stub it out without a real network connection — subclass
        /// HttpTransport and override SendAsync.
        /// </summary>
        protected virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> SendWithErrorHandlingAsync(HttpRequestMessage request)
        {
            try
            {
                return await SendAsync(request).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                throw new DashaMailNetworkException("Network error: " + ex.Message, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new DashaMailNetworkException("Network error: request timed out", ex);
            }
        }
    }
}
