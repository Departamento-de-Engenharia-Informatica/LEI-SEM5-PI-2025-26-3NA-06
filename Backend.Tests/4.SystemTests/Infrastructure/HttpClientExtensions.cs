using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.Tests.SystemTests.Infrastructure;

/// <summary>
/// Extension methods for HttpClient to simplify test operations.
/// </summary>
public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Sets the Authorization header with a Bearer token.
    /// </summary>
    public static void SetAuthorizationHeader(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the Authorization header.
    /// </summary>
    public static void ClearAuthorizationHeader(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Sends a GET request and deserializes the response to the specified type.
    /// </summary>
    public static async Task<T?> GetFromJsonAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// Sends a POST request with JSON body and returns the response.
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        return await client.PostAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Sends a PUT request with JSON body and returns the response.
    /// </summary>
    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        return await client.PutAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Reads the response content as JSON and deserializes to the specified type.
    /// </summary>
    public static async Task<T?> ReadFromJsonAsync<T>(this HttpContent content)
    {
        return await content.ReadFromJsonAsync<T>(JsonOptions);
    }
}
