using System.IO;

namespace DashaMail
{
    /// <summary>
    /// Raw bytes returned by an endpoint that can answer outside JSON —
    /// currently only <c>POST /images/optimize?response=binary</c>.
    /// </summary>
    public class DashaMailBinaryResponse
    {
        public byte[] Body { get; }

        public string ContentType { get; }

        public int HttpStatus { get; }

        public DashaMailBinaryResponse(byte[] body, string contentType, int httpStatus)
        {
            Body = body;
            ContentType = contentType;
            HttpStatus = httpStatus;
        }

        /// <summary>Writes the bytes to a file.</summary>
        public void SaveTo(string path)
        {
            File.WriteAllBytes(path, Body);
        }
    }
}
