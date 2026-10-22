using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeautyBeast.Web.Auth;

namespace BeautyBeast.Web.Services;

// Thin wrapper around HttpClient that:
//  - Prepends the API base URL
//  - Attaches the bearer token from TokenStore
//  - Throws ApiException with the response body on non-2xx
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _tokens;

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    public HttpClient Raw => _http;

    private HttpRequestMessage Prepare(HttpMethod method, string path, HttpContent? body = null)
    {
        var req = new HttpRequestMessage(method, path) { Content = body };
        if (_tokens.IsAuthenticated)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.Token);
        return req;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Get, path), ct);
        return await ReadAsync<T>(resp, ct);
    }

    public async Task<T?> PostAsync<T>(string path, object? body, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Post, path, body is null ? null : JsonContent.Create(body, options: Json)), ct);
        return await ReadAsync<T>(resp, ct);
    }

    public async Task PostAsync(string path, object? body, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Post, path, body is null ? null : JsonContent.Create(body, options: Json)), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task PutAsync(string path, object? body, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Put, path, body is null ? null : JsonContent.Create(body, options: Json)), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Delete, path), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task<TResp?> PostMultipartAsync<TResp>(string path, MultipartFormDataContent content, CancellationToken ct = default)
    {
        using var resp = await _http.SendAsync(Prepare(HttpMethod.Post, path, content), ct);
        return await ReadAsync<TResp>(resp, ct);
    }

    private async Task<T?> ReadAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        await EnsureSuccessAsync(resp, ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;
        if (resp.Content.Headers.ContentLength is 0) return default;
        return await resp.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;
        // A 401 means the token is invalid or the user no longer exists.
        // Signal MainLayout to call LogoutAsync (which wipes browser storage too).
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _tokens.NotifyUnauthorized();
        }
        var text = await resp.Content.ReadAsStringAsync(ct);
        throw new ApiException(resp.StatusCode, text);
    }
}

public class ApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string Body { get; }
    public ApiException(System.Net.HttpStatusCode code, string body)
        : base($"API call failed ({(int)code} {code}): {body}")
    {
        StatusCode = code;
        Body = body;
    }
}
