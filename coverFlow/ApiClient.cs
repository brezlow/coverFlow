using System.Text.Json;
using System.Web;

namespace coverFlow;

public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    // 预定义的 JsonSerializerOptions，忽略 null 值并且属性名不敏感
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient(string baseUrl = "http://localhost:3000") // 默认指向本地API服务器
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl.TrimEnd('/');
    }

    private string AppendTimestamp(string endpoint)
    {
        var uriBuilder = new UriBuilder(endpoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public async Task<T?> GetAsync<T>(string endpoint, string? cookies = null)
    {
        string fullUrl = _baseUrl + AppendTimestamp(endpoint);
        var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);

        if (!string.IsNullOrEmpty(cookies))
        {
            request.Headers.Add("Cookie", cookies);
        }

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // 如果HTTP状态码不是2xx，则抛出异常
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse, _jsonSerializerOptions);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message} for URL: {fullUrl}");
            return default(T);
        }
        catch (JsonException e)
        {
            Console.WriteLine($"JSON parsing error: {e.Message}");
            return default(T);
        }
    }

    // 如果有POST请求，可以添加 PostAsync 方法
    // public async Task<T?> PostAsync<T>(string endpoint, HttpContent content, string? cookies = null)
    // {
    //     // ... 实现 POST 请求 ...
    // }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}