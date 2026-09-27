using System.Collections.Generic;

namespace DashaMail.Resources
{
    public abstract class BaseResource
    {
        protected readonly HttpTransport Client;

        protected BaseResource(HttpTransport client)
        {
            Client = client;
        }

        /// <summary>Copies <paramref name="parameters"/> (or starts empty) and sets/overrides one field — used to merge a required field into the caller's optional-params dictionary.</summary>
        protected static IDictionary<string, object> With(IDictionary<string, object> parameters, string key, object value)
        {
            var result = new Dictionary<string, object>();
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in parameters)
                {
                    result[pair.Key] = pair.Value;
                }
            }
            result[key] = value;
            return result;
        }
    }
}
