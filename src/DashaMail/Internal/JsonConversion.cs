using System.Collections.Generic;
using System.Text.Json;

namespace DashaMail.Internal
{
    /// <summary>
    /// Converts a parsed <see cref="JsonElement"/> tree into plain BCL types
    /// (<see cref="Dictionary{TKey,TValue}"/>, <see cref="List{T}"/>, string,
    /// long, double, bool, null) so callers can work with the API's JSON
    /// payloads without depending on System.Text.Json types directly.
    /// </summary>
    internal static class JsonConversion
    {
        public static object ToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (JsonProperty prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ToObject(prop.Value);
                    }
                    return dict;

                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        list.Add(ToObject(item));
                    }
                    return list;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }
    }
}
