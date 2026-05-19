using Microsoft.Extensions.Caching.Memory;

namespace OddsPushClient.Services;

public interface IBetTypeService
{
    Task<string> GetBetTypeNameAsync(int betType);
}

public class BetTypeService : IBetTypeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BetTypeService> _logger;

    public BetTypeService(
        HttpClient httpClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<BetTypeService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetBetTypeNameAsync(int betType)
    {
        string cacheKey = $"BetTypeName_{betType}";
        if (_cache.TryGetValue(cacheKey, out string? cachedName))
        {
            return cachedName ?? betType.ToString();
        }

        try
        {
            var host = _configuration["OddsFeedApi:Host"];
            var vendorId = _configuration["OddsFeedApi:VendorId"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{host}/api/GetBetTypeName");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(vendorId ?? ""), "vendor_id");
            content.Add(new StringContent(betType.ToString()), "bet_type");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResult = System.Text.Json.JsonSerializer.Deserialize<BetTypeApiResponse>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResult?.Data?.Names != null)
            {
                // Prefer 'en' or 'ch' or fallback to the first one available
                var nameObj = apiResult.Data.Names.FirstOrDefault(n => n.Lang == "ch")
                           ?? apiResult.Data.Names.FirstOrDefault(n => n.Lang == "en")
                           ?? apiResult.Data.Names.FirstOrDefault();

                if (nameObj != null)
                {
                    _cache.Set(cacheKey, nameObj.Name, TimeSpan.FromHours(24));
                    return nameObj.Name;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bet type name for {BetType}", betType);
        }

        return betType.ToString();
    }
}

public class BetTypeApiResponse
{
    public int Error_Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public BetTypeData? Data { get; set; }
}

public class BetTypeData
{
    public List<BetTypeName>? Names { get; set; }
}

public class BetTypeName
{
    public string Lang { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
