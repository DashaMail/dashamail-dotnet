using System;
using System.Collections.Generic;
using System.Globalization;

namespace DashaMail
{
    /// <summary>
    /// Raised for any error response from the DashaMail API (HTTP status
    /// &gt;= 400 with a JSON <c>{"error": {"code", "message", "details"}}</c>
    /// body).
    ///
    /// <para>
    /// The HTTP status and DashaMail's own error <c>code</c> are different
    /// numbers: <c>code</c> is stable across API versions and documented at
    /// https://dashamail.ru/api/errors/, while the HTTP status is a coarser
    /// REST-ification of it.
    /// </para>
    /// </summary>
    public class DashaMailApiException : Exception
    {
        public int HttpStatus { get; }

        public int ApiCode { get; }

        public IDictionary<string, object> Details { get; }

        public DashaMailApiException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message)
        {
            HttpStatus = httpStatus;
            ApiCode = apiCode;
            Details = details ?? new Dictionary<string, object>();
        }

        /// <summary>Builds the most specific exception subclass for a given HTTP status.</summary>
        public static DashaMailApiException FromError(int httpStatus, IDictionary<string, object> error)
        {
            error = error ?? new Dictionary<string, object>();

            string message = "DashaMail API error";
            if (error.TryGetValue("message", out object messageValue) && messageValue != null)
            {
                message = messageValue.ToString();
            }

            int apiCode = httpStatus;
            if (error.TryGetValue("code", out object codeValue) && codeValue != null)
            {
                try
                {
                    apiCode = Convert.ToInt32(codeValue, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    apiCode = httpStatus;
                }
            }

            IDictionary<string, object> details = null;
            if (error.TryGetValue("details", out object detailsValue))
            {
                details = detailsValue as IDictionary<string, object>;
            }

            switch (httpStatus)
            {
                case 401:
                    return new DashaMailAuthenticationException(message, httpStatus, apiCode, details);
                case 402:
                    return new DashaMailPaymentRequiredException(message, httpStatus, apiCode, details);
                case 403:
                    return new DashaMailAuthorizationException(message, httpStatus, apiCode, details);
                case 404:
                    return new DashaMailNotFoundException(message, httpStatus, apiCode, details);
                case 409:
                    return new DashaMailConflictException(message, httpStatus, apiCode, details);
                case 413:
                    return new DashaMailPayloadTooLargeException(message, httpStatus, apiCode, details);
                case 422:
                    return new DashaMailValidationException(message, httpStatus, apiCode, details);
                case 429:
                    return new DashaMailRateLimitException(message, httpStatus, apiCode, details);
            }

            if (httpStatus >= 500)
            {
                return new DashaMailServerException(message, httpStatus, apiCode, details);
            }

            return new DashaMailApiException(message, httpStatus, apiCode, details);
        }
    }

    /// <summary>HTTP 401 — missing or invalid API key.</summary>
    public class DashaMailAuthenticationException : DashaMailApiException
    {
        public DashaMailAuthenticationException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 402 — the account's balance or plan does not allow this action.</summary>
    public class DashaMailPaymentRequiredException : DashaMailApiException
    {
        public DashaMailPaymentRequiredException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 403 — the API key is valid but lacks the rights or scope for this action.</summary>
    public class DashaMailAuthorizationException : DashaMailApiException
    {
        public DashaMailAuthorizationException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 404 — the resource (or the account itself) does not exist.</summary>
    public class DashaMailNotFoundException : DashaMailApiException
    {
        public DashaMailNotFoundException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 409 — the request conflicts with the resource's current state.</summary>
    public class DashaMailConflictException : DashaMailApiException
    {
        public DashaMailConflictException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 413 — the uploaded file or attachment is too large.</summary>
    public class DashaMailPayloadTooLargeException : DashaMailApiException
    {
        public DashaMailPayloadTooLargeException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 422 — a required field is missing or a value is invalid.</summary>
    public class DashaMailValidationException : DashaMailApiException
    {
        public DashaMailValidationException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>HTTP 429 — too many requests. See <see cref="RetryAfter"/> for how long to back off.</summary>
    public class DashaMailRateLimitException : DashaMailApiException
    {
        public DashaMailRateLimitException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }

        public int? RetryAfter
        {
            get
            {
                if (Details.TryGetValue("retry_after", out object value) && value != null)
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                return null;
            }
        }
    }

    /// <summary>HTTP 5xx — something failed on DashaMail's side. Usually safe to retry.</summary>
    public class DashaMailServerException : DashaMailApiException
    {
        public DashaMailServerException(string message, int httpStatus, int apiCode, IDictionary<string, object> details)
            : base(message, httpStatus, apiCode, details)
        {
        }
    }

    /// <summary>The request never got an HTTP response (DNS, TLS, timeout, connection reset...).</summary>
    public class DashaMailNetworkException : Exception
    {
        public DashaMailNetworkException(string message)
            : base(message)
        {
        }

        public DashaMailNetworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
