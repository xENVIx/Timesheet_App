using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timesheeter.Lib
{
    /// <summary>Talking to a Timesheeter server: every request carries the API key in X-Api-Key.</summary>
    public static class SyncServer
    {
        public const string ApiKeyHeader = "X-Api-Key";

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>A GET request to <paramref name="path"/> (e.g. "/api/customers") with the key attached.</summary>
        public static HttpRequestMessage Get(string serverUrl, string apiKey, string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(serverUrl.TrimEnd('/') + "/"), path.TrimStart('/')));
            request.Headers.Add(ApiKeyHeader, apiKey);
            return request;
        }

        /// <summary>
        /// Checks the address and key by asking the server for its customers. Returns null when it
        /// works, otherwise what's wrong, worded for the Settings page.
        /// </summary>
        public static async Task<string?> TestAsync(string serverUrl, string apiKey)
        {
            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return "The server address should look like https://my-server:7045";
            if (string.IsNullOrWhiteSpace(apiKey))
                return "Enter the API key the server gave you.";

            try
            {
                using var response = await Http.SendAsync(Get(serverUrl, apiKey.Trim(), "/api/customers"));
                return response.StatusCode switch
                {
                    HttpStatusCode.OK => null,
                    HttpStatusCode.Unauthorized => "The server refused the API key (wrong, or revoked).",
                    HttpStatusCode.NotFound => "That address answered, but it isn't a Timesheeter server.",
                    _ => $"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.",
                };
            }
            catch (TaskCanceledException)
            {
                return "The server didn't answer within 10 seconds.";
            }
            catch (HttpRequestException ex)
            {
                return $"Couldn't reach the server: {ex.InnerException?.Message ?? ex.Message}";
            }
        }
    }
}
