using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace DashaMail
{
    /// <summary>
    /// Wraps the <c>data</c> payload of a successful API call together with
    /// its <c>meta</c> (pagination info) and the human-readable
    /// <c>msg.text</c> DashaMail sent.
    ///
    /// <para>
    /// <see cref="Data"/> is typically a <see cref="Dictionary{TKey,TValue}"/>
    /// (single object), a <see cref="List{T}"/> of dictionaries (a listing),
    /// or a scalar. The response is enumerable when the payload is a list:
    /// </para>
    ///
    /// <code>
    /// var members = await dashamail.Lists.MembersAsync(listId);
    /// foreach (var member in members)
    /// {
    ///     var dict = (Dictionary&lt;string, object&gt;)member;
    ///     Console.WriteLine(dict["email"]);
    /// }
    /// </code>
    ///
    /// Paginated endpoints (<c>MembersAsync</c> and friends) additionally
    /// expose <see cref="HasMore"/>/<see cref="Limit"/> taken from the
    /// <c>meta</c> object DashaMail returns instead of a total count.
    /// </summary>
    public class DashaMailResponse : IEnumerable<object>
    {
        public object Data { get; }

        public IDictionary<string, object> Meta { get; }

        public string Message { get; }

        public DashaMailResponse(object data, IDictionary<string, object> meta, string message)
        {
            Data = data;
            Meta = meta ?? new Dictionary<string, object>();
            Message = message;
        }

        /// <summary>True when a paginated listing has more rows beyond the returned page.</summary>
        public bool HasMore
        {
            get
            {
                if (Meta.TryGetValue("has_more", out object value) && value is bool boolValue)
                {
                    return boolValue;
                }
                return false;
            }
        }

        /// <summary>The effective page size DashaMail used to answer a paginated listing.</summary>
        public int? Limit
        {
            get
            {
                if (Meta.TryGetValue("limit", out object value) && value != null)
                {
                    return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                return null;
            }
        }

        /// <summary>Indexes into <see cref="Data"/> when it is an object (a Dictionary).</summary>
        public object this[string key]
        {
            get
            {
                var dict = Data as IDictionary<string, object>;
                return dict != null ? dict[key] : null;
            }
        }

        /// <summary>Indexes into <see cref="Data"/> when it is a list.</summary>
        public object this[int index]
        {
            get
            {
                var list = Data as IList<object>;
                return list != null ? list[index] : null;
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            var list = Data as IList<object>;
            if (list != null)
            {
                foreach (object item in list)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return "DashaMailResponse { Data = " + Data + " }";
        }
    }
}
