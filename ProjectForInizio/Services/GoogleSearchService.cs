using System.Text.Json;
using ProjectForInizio.Dtos;

namespace ProjectForInizio.Services;

public class GoogleSearchService : IGoogleSearchService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _cx;

    public GoogleSearchService(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _apiKey = cfg["Google:ApiKey"] ?? throw new InvalidOperationException("Google:ApiKey missing");
        _cx = cfg["Google:Cx"] ?? throw new InvalidOperationException("Google:Cx missing");
    }

    public async Task<SearchResultDto> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query is empty.");

        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_cx))
            throw new InvalidOperationException("Missing Google:ApiKey or Google:Cx. Check user-secrets / app settings.");

        var url = $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_cx}&q={Uri.EscapeDataString(query)}";

        using var res = await _http.GetAsync(url, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google API error {(int)res.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var items = new List<SearchItemDto>();
        if (doc.RootElement.TryGetProperty("items", out var arr))
        {
            foreach (var it in arr.EnumerateArray())
            {
                var title = it.TryGetProperty("title", out var t) ? t.GetString() : null;
                var link = it.TryGetProperty("link", out var l) ? l.GetString() : null;
                var snippet = it.TryGetProperty("snippet", out var s) ? s.GetString() : null;
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(link))
                    items.Add(new SearchItemDto(title!, link!, snippet));
            }
        }

        return new SearchResultDto(query, DateTime.UtcNow, items);
    }


}
