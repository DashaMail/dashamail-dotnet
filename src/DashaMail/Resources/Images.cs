using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashaMail.Resources
{
    /// <summary>
    /// Resize (&gt;1600px wide) and recompress an image the same way DashaMail's own
    /// file manager does, without changing its format or storing anything.
    /// https://dashamail.ru/api/images/
    /// </summary>
    public class Images : BaseResource
    {
        public Images(HttpTransport client)
            : base(client)
        {
        }

        /// <summary>
        /// Optimize an image already loaded in memory (JSON body, base64-encoded).
        /// </summary>
        /// <param name="binaryData">Raw image bytes (JPEG/PNG/GIF).</param>
        /// <param name="parameters">resize (bool, default true), max_width (int), lossy (bool).</param>
        /// <returns>A DashaMailResponse whose Data["image"] is the base64-encoded result.</returns>
        public Task<DashaMailResponse> OptimizeAsync(byte[] binaryData, IDictionary<string, object> parameters = null)
        {
            string base64 = System.Convert.ToBase64String(binaryData);
            return Client.RequestAsync("POST", "/images/optimize", null, With(parameters, "image", base64));
        }

        /// <summary>Optimize an image already sitting on disk, uploaded as multipart/form-data.</summary>
        public Task<DashaMailResponse> OptimizeFileAsync(string filePath, IDictionary<string, object> parameters = null)
        {
            return Client.RequestMultipartAsync("POST", "/images/optimize", null, parameters, "file", filePath);
        }

        /// <summary>Same as OptimizeFileAsync, but returns raw optimized bytes (?response=binary).</summary>
        public Task<DashaMailBinaryResponse> OptimizeFileBinaryAsync(string filePath, IDictionary<string, object> parameters = null)
        {
            var query = new Dictionary<string, object> { { "response", "binary" } };
            return Client.RequestMultipartBinaryAsync("POST", "/images/optimize", query, parameters, "file", filePath);
        }

        /// <summary>Same as OptimizeAsync, but returns raw optimized bytes (?response=binary).</summary>
        public Task<DashaMailBinaryResponse> OptimizeBinaryAsync(byte[] binaryData, IDictionary<string, object> parameters = null)
        {
            string base64 = System.Convert.ToBase64String(binaryData);
            var query = new Dictionary<string, object> { { "response", "binary" } };
            return Client.RequestBinaryAsync("POST", "/images/optimize", query, With(parameters, "image", base64));
        }
    }
}
